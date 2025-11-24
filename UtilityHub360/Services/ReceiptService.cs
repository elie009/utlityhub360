using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;
using System.Text.Json;

namespace UtilityHub360.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorage;
        private readonly IOcrService _ocrService;
        private readonly ILogger<ReceiptService> _logger;

        public ReceiptService(
            ApplicationDbContext context,
            IFileStorageService fileStorage,
            IOcrService ocrService,
            ILogger<ReceiptService> logger)
        {
            _context = context;
            _fileStorage = fileStorage;
            _ocrService = ocrService;
            _logger = logger;
        }

        public async Task<ApiResponse<ReceiptDto>> UploadReceiptAsync(string userId, Stream fileStream, string fileName, string contentType)
        {
            try
            {
                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp", "application/pdf" };
                if (!allowedTypes.Contains(contentType.ToLower()))
                {
                    return ApiResponse<ReceiptDto>.ErrorResult("Invalid file type. Only JPG, PNG, WEBP, and PDF files are allowed.");
                }

                // Validate file size (max 10MB)
                if (fileStream.Length > 10 * 1024 * 1024)
                {
                    return ApiResponse<ReceiptDto>.ErrorResult("File size exceeds maximum limit of 10MB.");
                }

                // Save file
                var filePath = await _fileStorage.SaveFileAsync(fileStream, fileName, userId, contentType);

                // Create receipt entity
                var receipt = new ExpenseReceipt
                {
                    UserId = userId,
                    FileName = Path.GetFileName(filePath),
                    FilePath = filePath,
                    FileType = contentType,
                    FileSize = fileStream.Length,
                    OriginalFileName = fileName,
                    IsOcrProcessed = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ExpenseReceipts.Add(receipt);
                await _context.SaveChangesAsync();

                // Process OCR in background (fire and forget)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ProcessReceiptOcrAsync(receipt.Id, userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing OCR for receipt {ReceiptId}", receipt.Id);
                    }
                });

                var dto = MapToDto(receipt);
                return ApiResponse<ReceiptDto>.SuccessResult(dto, "Receipt uploaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading receipt for user {UserId}", userId);
                return ApiResponse<ReceiptDto>.ErrorResult($"Failed to upload receipt: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ReceiptDto>> ProcessReceiptOcrAsync(string receiptId, string userId)
        {
            try
            {
                var receipt = await _context.ExpenseReceipts
                    .FirstOrDefaultAsync(r => r.Id == receiptId && r.UserId == userId && !r.IsDeleted);

                if (receipt == null)
                {
                    return ApiResponse<ReceiptDto>.ErrorResult("Receipt not found");
                }

                // Get file stream
                var fileStream = await _fileStorage.GetFileAsync(receipt.FilePath);

                // Process OCR based on file type
                OcrResult ocrResult;
                if (receipt.FileType.ToLower().Contains("pdf"))
                {
                    ocrResult = await _ocrService.ProcessPdfAsync(fileStream);
                }
                else
                {
                    ocrResult = await _ocrService.ProcessImageAsync(fileStream, receipt.FileType);
                }

                // Update receipt with OCR results
                receipt.OcrText = ocrResult.FullText;
                receipt.ExtractedAmount = ocrResult.Amount;
                receipt.ExtractedDate = ocrResult.Date;
                receipt.ExtractedMerchant = ocrResult.Merchant;
                receipt.IsOcrProcessed = true;
                receipt.OcrProcessedAt = DateTime.UtcNow;
                receipt.UpdatedAt = DateTime.UtcNow;

                // Store items as JSON
                if (ocrResult.Items.Any())
                {
                    var itemsJson = JsonSerializer.Serialize(ocrResult.Items.Select(i => new
                    {
                        i.Description,
                        i.Price,
                        i.Quantity
                    }));
                    receipt.ExtractedItems = itemsJson;
                }

                await _context.SaveChangesAsync();

                var dto = MapToDto(receipt);
                return ApiResponse<ReceiptDto>.SuccessResult(dto, "OCR processing completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OCR for receipt {ReceiptId}", receiptId);
                return ApiResponse<ReceiptDto>.ErrorResult($"Failed to process OCR: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ReceiptDto>> GetReceiptAsync(string receiptId, string userId)
        {
            try
            {
                var receipt = await _context.ExpenseReceipts
                    .FirstOrDefaultAsync(r => r.Id == receiptId && r.UserId == userId && !r.IsDeleted);

                if (receipt == null)
                {
                    return ApiResponse<ReceiptDto>.ErrorResult("Receipt not found");
                }

                var dto = MapToDto(receipt);
                return ApiResponse<ReceiptDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting receipt {ReceiptId}", receiptId);
                return ApiResponse<ReceiptDto>.ErrorResult($"Failed to get receipt: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ReceiptDto>>> GetReceiptsAsync(string userId, ReceiptSearchDto? searchDto = null)
        {
            try
            {
                var query = _context.ExpenseReceipts
                    .Where(r => r.UserId == userId && !r.IsDeleted)
                    .AsQueryable();

                if (searchDto != null)
                {
                    if (searchDto.StartDate.HasValue)
                    {
                        query = query.Where(r => r.CreatedAt >= searchDto.StartDate.Value || 
                                                 (r.ExtractedDate.HasValue && r.ExtractedDate >= searchDto.StartDate.Value));
                    }

                    if (searchDto.EndDate.HasValue)
                    {
                        query = query.Where(r => r.CreatedAt <= searchDto.EndDate.Value || 
                                                 (r.ExtractedDate.HasValue && r.ExtractedDate <= searchDto.EndDate.Value));
                    }

                    if (!string.IsNullOrEmpty(searchDto.Merchant))
                    {
                        query = query.Where(r => r.ExtractedMerchant != null && 
                                                 r.ExtractedMerchant.Contains(searchDto.Merchant));
                    }

                    if (searchDto.MinAmount.HasValue)
                    {
                        query = query.Where(r => r.ExtractedAmount.HasValue && 
                                                 r.ExtractedAmount >= searchDto.MinAmount.Value);
                    }

                    if (searchDto.MaxAmount.HasValue)
                    {
                        query = query.Where(r => r.ExtractedAmount.HasValue && 
                                                 r.ExtractedAmount <= searchDto.MaxAmount.Value);
                    }

                    if (searchDto.IsOcrProcessed.HasValue)
                    {
                        query = query.Where(r => r.IsOcrProcessed == searchDto.IsOcrProcessed.Value);
                    }

                    if (!string.IsNullOrEmpty(searchDto.SearchText))
                    {
                        var searchLower = searchDto.SearchText.ToLower();
                        query = query.Where(r => 
                            (r.ExtractedMerchant != null && r.ExtractedMerchant.ToLower().Contains(searchLower)) ||
                            (r.OcrText != null && r.OcrText.ToLower().Contains(searchLower)) ||
                            (r.OriginalFileName != null && r.OriginalFileName.ToLower().Contains(searchLower)));
                    }
                }

                var totalCount = await query.CountAsync();

                var page = searchDto?.Page ?? 1;
                var limit = searchDto?.Limit ?? 20;
                var receipts = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var dtos = receipts.Select(MapToDto).ToList();
                return ApiResponse<List<ReceiptDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting receipts for user {UserId}", userId);
                return ApiResponse<List<ReceiptDto>>.ErrorResult($"Failed to get receipts: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteReceiptAsync(string receiptId, string userId)
        {
            try
            {
                var receipt = await _context.ExpenseReceipts
                    .FirstOrDefaultAsync(r => r.Id == receiptId && r.UserId == userId && !r.IsDeleted);

                if (receipt == null)
                {
                    return ApiResponse<bool>.ErrorResult("Receipt not found");
                }

                // Soft delete
                receipt.IsDeleted = true;
                receipt.DeletedAt = DateTime.UtcNow;
                receipt.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Optionally delete file from storage
                _ = Task.Run(async () =>
                {
                    await _fileStorage.DeleteFileAsync(receipt.FilePath);
                    if (!string.IsNullOrEmpty(receipt.ThumbnailPath))
                    {
                        await _fileStorage.DeleteFileAsync(receipt.ThumbnailPath);
                    }
                });

                return ApiResponse<bool>.SuccessResult(true, "Receipt deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting receipt {ReceiptId}", receiptId);
                return ApiResponse<bool>.ErrorResult($"Failed to delete receipt: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ReceiptDto>> LinkReceiptToExpenseAsync(string receiptId, string expenseId, string userId)
        {
            try
            {
                var receipt = await _context.ExpenseReceipts
                    .FirstOrDefaultAsync(r => r.Id == receiptId && r.UserId == userId && !r.IsDeleted);

                if (receipt == null)
                {
                    return ApiResponse<ReceiptDto>.ErrorResult("Receipt not found");
                }

                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.Id == expenseId && e.UserId == userId && !e.IsDeleted);

                if (expense == null)
                {
                    return ApiResponse<ReceiptDto>.ErrorResult("Expense not found");
                }

                receipt.ExpenseId = expenseId;
                receipt.UpdatedAt = DateTime.UtcNow;

                expense.ReceiptId = receiptId;
                expense.HasReceipt = true;
                expense.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var dto = MapToDto(receipt);
                return ApiResponse<ReceiptDto>.SuccessResult(dto, "Receipt linked to expense successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking receipt {ReceiptId} to expense {ExpenseId}", receiptId, expenseId);
                return ApiResponse<ReceiptDto>.ErrorResult($"Failed to link receipt: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ExpenseMatchDto>>> FindMatchingExpensesAsync(string receiptId, string userId)
        {
            try
            {
                var receipt = await _context.ExpenseReceipts
                    .FirstOrDefaultAsync(r => r.Id == receiptId && r.UserId == userId && !r.IsDeleted);

                if (receipt == null)
                {
                    return ApiResponse<List<ExpenseMatchDto>>.ErrorResult("Receipt not found");
                }

                if (!receipt.IsOcrProcessed || !receipt.ExtractedAmount.HasValue)
                {
                    return ApiResponse<List<ExpenseMatchDto>>.ErrorResult("Receipt OCR not processed yet");
                }

                var matches = new List<ExpenseMatchDto>();

                // Find expenses by amount (within 5% tolerance)
                var amountTolerance = receipt.ExtractedAmount.Value * 0.05m;
                var dateRange = receipt.ExtractedDate.HasValue 
                    ? TimeSpan.FromDays(7) 
                    : TimeSpan.FromDays(30);

                var potentialExpenses = await _context.Expenses
                    .Include(e => e.Category)
                    .Where(e => e.UserId == userId && 
                               !e.IsDeleted &&
                               Math.Abs(e.Amount - receipt.ExtractedAmount.Value) <= amountTolerance)
                    .ToListAsync();

                foreach (var expense in potentialExpenses)
                {
                    var matchScore = 0.0;
                    var matchReasons = new List<string>();

                    // Amount match
                    if (Math.Abs(expense.Amount - receipt.ExtractedAmount.Value) <= amountTolerance)
                    {
                        matchScore += 40;
                        matchReasons.Add("Amount matches");
                    }

                    // Date match
                    if (receipt.ExtractedDate.HasValue)
                    {
                        var dateDiff = Math.Abs((expense.ExpenseDate - receipt.ExtractedDate.Value).TotalDays);
                        if (dateDiff <= 7)
                        {
                            matchScore += 30;
                            matchReasons.Add("Date within 7 days");
                        }
                        else if (dateDiff <= 30)
                        {
                            matchScore += 15;
                            matchReasons.Add("Date within 30 days");
                        }
                    }

                    // Merchant match
                    if (!string.IsNullOrEmpty(receipt.ExtractedMerchant) && 
                        !string.IsNullOrEmpty(expense.Merchant))
                    {
                        if (expense.Merchant.Contains(receipt.ExtractedMerchant, StringComparison.OrdinalIgnoreCase) ||
                            receipt.ExtractedMerchant.Contains(expense.Merchant, StringComparison.OrdinalIgnoreCase))
                        {
                            matchScore += 30;
                            matchReasons.Add("Merchant name matches");
                        }
                    }

                    if (matchScore >= 50) // Minimum threshold
                    {
                        matches.Add(new ExpenseMatchDto
                        {
                            ExpenseId = expense.Id,
                            Description = expense.Description,
                            Amount = expense.Amount,
                            ExpenseDate = expense.ExpenseDate,
                            Merchant = expense.Merchant,
                            Category = expense.Category?.Name ?? "Unknown",
                            MatchScore = matchScore,
                            MatchReason = string.Join(", ", matchReasons)
                        });
                    }
                }

                matches = matches.OrderByDescending(m => m.MatchScore).ToList();

                return ApiResponse<List<ExpenseMatchDto>>.SuccessResult(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding matching expenses for receipt {ReceiptId}", receiptId);
                return ApiResponse<List<ExpenseMatchDto>>.ErrorResult($"Failed to find matches: {ex.Message}");
            }
        }

        public async Task<Stream> GetReceiptFileAsync(string receiptId, string userId)
        {
            var receipt = await _context.ExpenseReceipts
                .FirstOrDefaultAsync(r => r.Id == receiptId && r.UserId == userId && !r.IsDeleted);

            if (receipt == null)
            {
                throw new FileNotFoundException("Receipt not found");
            }

            return await _fileStorage.GetFileAsync(receipt.FilePath);
        }

        public async Task<Stream> GetReceiptThumbnailAsync(string receiptId, string userId)
        {
            var receipt = await _context.ExpenseReceipts
                .FirstOrDefaultAsync(r => r.Id == receiptId && r.UserId == userId && !r.IsDeleted);

            if (receipt == null || string.IsNullOrEmpty(receipt.ThumbnailPath))
            {
                throw new FileNotFoundException("Thumbnail not found");
            }

            return await _fileStorage.GetFileAsync(receipt.ThumbnailPath);
        }

        private ReceiptDto MapToDto(ExpenseReceipt receipt)
        {
            var dto = new ReceiptDto
            {
                Id = receipt.Id,
                UserId = receipt.UserId,
                ExpenseId = receipt.ExpenseId,
                FileName = receipt.FileName,
                FilePath = receipt.FilePath,
                FileType = receipt.FileType,
                FileSize = receipt.FileSize,
                OriginalFileName = receipt.OriginalFileName,
                ExtractedAmount = receipt.ExtractedAmount,
                ExtractedDate = receipt.ExtractedDate,
                ExtractedMerchant = receipt.ExtractedMerchant,
                OcrText = receipt.OcrText,
                IsOcrProcessed = receipt.IsOcrProcessed,
                OcrProcessedAt = receipt.OcrProcessedAt,
                ThumbnailPath = receipt.ThumbnailPath,
                Notes = receipt.Notes,
                FileUrl = _fileStorage.GetFileUrl(receipt.FilePath),
                ThumbnailUrl = !string.IsNullOrEmpty(receipt.ThumbnailPath) 
                    ? _fileStorage.GetFileUrl(receipt.ThumbnailPath) 
                    : null,
                CreatedAt = receipt.CreatedAt,
                UpdatedAt = receipt.UpdatedAt
            };

            // Parse extracted items
            if (!string.IsNullOrEmpty(receipt.ExtractedItems))
            {
                try
                {
                    var items = JsonSerializer.Deserialize<List<ReceiptItemDto>>(receipt.ExtractedItems);
                    dto.ExtractedItems = items;
                }
                catch
                {
                    // Ignore parsing errors
                }
            }

            return dto;
        }
    }
}

