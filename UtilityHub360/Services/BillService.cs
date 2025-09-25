using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class BillService : IBillService
    {
        private readonly ApplicationDbContext _context;

        public BillService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<BillDto>> CreateBillAsync(CreateBillDto createBillDto, string userId)
        {
            try
            {
                var bill = new Bill
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    BillName = createBillDto.BillName,
                    BillType = createBillDto.BillType.ToLower(),
                    Amount = createBillDto.Amount,
                    DueDate = createBillDto.DueDate,
                    Frequency = createBillDto.Frequency.ToLower(),
                    Status = "PENDING",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Notes = createBillDto.Notes,
                    Provider = createBillDto.Provider,
                    ReferenceNumber = createBillDto.ReferenceNumber
                };

                _context.Bills.Add(bill);
                await _context.SaveChangesAsync();

                var billDto = MapToBillDto(bill);
                return ApiResponse<BillDto>.SuccessResult(billDto, "Bill created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BillDto>.ErrorResult($"Failed to create bill: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BillDto>> GetBillAsync(string billId, string userId)
        {
            try
            {
                var bill = await _context.Bills
                    .FirstOrDefaultAsync(b => b.Id == billId && b.UserId == userId);

                if (bill == null)
                {
                    return ApiResponse<BillDto>.ErrorResult("Bill not found");
                }

                var billDto = MapToBillDto(bill);
                return ApiResponse<BillDto>.SuccessResult(billDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<BillDto>.ErrorResult($"Failed to get bill: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BillDto>> UpdateBillAsync(string billId, UpdateBillDto updateBillDto, string userId)
        {
            try
            {
                var bill = await _context.Bills
                    .FirstOrDefaultAsync(b => b.Id == billId && b.UserId == userId);

                if (bill == null)
                {
                    return ApiResponse<BillDto>.ErrorResult("Bill not found");
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(updateBillDto.BillName))
                    bill.BillName = updateBillDto.BillName;

                if (!string.IsNullOrEmpty(updateBillDto.BillType))
                    bill.BillType = updateBillDto.BillType.ToLower();

                if (updateBillDto.Amount.HasValue)
                    bill.Amount = updateBillDto.Amount.Value;

                if (updateBillDto.DueDate.HasValue)
                    bill.DueDate = updateBillDto.DueDate.Value;

                if (!string.IsNullOrEmpty(updateBillDto.Frequency))
                    bill.Frequency = updateBillDto.Frequency.ToLower();

                if (!string.IsNullOrEmpty(updateBillDto.Status))
                    bill.Status = updateBillDto.Status.ToUpper();

                if (updateBillDto.Notes != null)
                    bill.Notes = updateBillDto.Notes;

                if (updateBillDto.Provider != null)
                    bill.Provider = updateBillDto.Provider;

                if (updateBillDto.ReferenceNumber != null)
                    bill.ReferenceNumber = updateBillDto.ReferenceNumber;

                bill.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var billDto = MapToBillDto(bill);
                return ApiResponse<BillDto>.SuccessResult(billDto, "Bill updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<BillDto>.ErrorResult($"Failed to update bill: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteBillAsync(string billId, string userId)
        {
            try
            {
                var bill = await _context.Bills
                    .FirstOrDefaultAsync(b => b.Id == billId && b.UserId == userId);

                if (bill == null)
                {
                    return ApiResponse<bool>.ErrorResult("Bill not found");
                }

                _context.Bills.Remove(bill);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Bill deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete bill: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PaginatedResponse<BillDto>>> GetUserBillsAsync(string userId, string? status, string? billType, int page, int limit)
        {
            try
            {
                var query = _context.Bills.Where(b => b.UserId == userId);

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(b => b.Status == status.ToUpper());
                }

                if (!string.IsNullOrEmpty(billType))
                {
                    query = query.Where(b => b.BillType == billType.ToLower());
                }

                var totalCount = await query.CountAsync();
                var bills = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var billDtos = bills.Select(MapToBillDto).ToList();

                var paginatedResponse = new PaginatedResponse<BillDto>
                {
                    Data = billDtos,
                    Page = page,
                    Limit = limit,
                    TotalCount = totalCount
                };

                return ApiResponse<PaginatedResponse<BillDto>>.SuccessResult(paginatedResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResponse<BillDto>>.ErrorResult($"Failed to get bills: {ex.Message}");
            }
        }

        public async Task<ApiResponse<decimal>> GetTotalPendingAmountAsync(string userId)
        {
            try
            {
                var totalAmount = await _context.Bills
                    .Where(b => b.UserId == userId && b.Status == "PENDING")
                    .SumAsync(b => b.Amount);

                return ApiResponse<decimal>.SuccessResult(totalAmount);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult($"Failed to get total pending amount: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BillSummaryDto>> GetTotalPaidAmountAsync(string userId, string period)
        {
            try
            {
                var (startDate, endDate) = GetPeriodDates(period);
                
                var totalAmount = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.Status == "PAID" && 
                               b.PaidAt >= startDate && 
                               b.PaidAt <= endDate)
                    .SumAsync(b => b.Amount);

                var count = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.Status == "PAID" && 
                               b.PaidAt >= startDate && 
                               b.PaidAt <= endDate)
                    .CountAsync();

                var summary = new BillSummaryDto
                {
                    Amount = totalAmount,
                    Count = count,
                    Period = period.ToUpper(),
                    StartDate = startDate,
                    EndDate = endDate
                };

                return ApiResponse<BillSummaryDto>.SuccessResult(summary);
            }
            catch (Exception ex)
            {
                return ApiResponse<BillSummaryDto>.ErrorResult($"Failed to get total paid amount: {ex.Message}");
            }
        }

        public async Task<ApiResponse<decimal>> GetTotalOverdueAmountAsync(string userId)
        {
            try
            {
                var currentDate = DateTime.UtcNow.Date;
                
                var totalAmount = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.Status == "PENDING" && 
                               b.DueDate < currentDate)
                    .SumAsync(b => b.Amount);

                return ApiResponse<decimal>.SuccessResult(totalAmount);
            }
            catch (Exception ex)
            {
                return ApiResponse<decimal>.ErrorResult($"Failed to get total overdue amount: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BillAnalyticsDto>> GetBillAnalyticsAsync(string userId)
        {
            try
            {
                var currentDate = DateTime.UtcNow.Date;

                var pendingAmount = await _context.Bills
                    .Where(b => b.UserId == userId && b.Status == "PENDING")
                    .SumAsync(b => b.Amount);

                var overdueAmount = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.Status == "PENDING" && 
                               b.DueDate < currentDate)
                    .SumAsync(b => b.Amount);

                var paidAmount = await _context.Bills
                    .Where(b => b.UserId == userId && b.Status == "PAID")
                    .SumAsync(b => b.Amount);

                var pendingCount = await _context.Bills
                    .Where(b => b.UserId == userId && b.Status == "PENDING")
                    .CountAsync();

                var overdueCount = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.Status == "PENDING" && 
                               b.DueDate < currentDate)
                    .CountAsync();

                var paidCount = await _context.Bills
                    .Where(b => b.UserId == userId && b.Status == "PAID")
                    .CountAsync();

                var analytics = new BillAnalyticsDto
                {
                    TotalPendingAmount = pendingAmount,
                    TotalPaidAmount = paidAmount,
                    TotalOverdueAmount = overdueAmount,
                    TotalPendingBills = pendingCount,
                    TotalPaidBills = paidCount,
                    TotalOverdueBills = overdueCount,
                    GeneratedAt = DateTime.UtcNow
                };

                return ApiResponse<BillAnalyticsDto>.SuccessResult(analytics);
            }
            catch (Exception ex)
            {
                return ApiResponse<BillAnalyticsDto>.ErrorResult($"Failed to get bill analytics: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BillDto>> MarkBillAsPaidAsync(string billId, string userId, string? notes)
        {
            try
            {
                var bill = await _context.Bills
                    .FirstOrDefaultAsync(b => b.Id == billId && b.UserId == userId);

                if (bill == null)
                {
                    return ApiResponse<BillDto>.ErrorResult("Bill not found");
                }

                bill.Status = "PAID";
                bill.PaidAt = DateTime.UtcNow;
                bill.UpdatedAt = DateTime.UtcNow;
                
                if (!string.IsNullOrEmpty(notes))
                {
                    bill.Notes = notes;
                }

                await _context.SaveChangesAsync();

                var billDto = MapToBillDto(bill);
                return ApiResponse<BillDto>.SuccessResult(billDto, "Bill marked as paid");
            }
            catch (Exception ex)
            {
                return ApiResponse<BillDto>.ErrorResult($"Failed to mark bill as paid: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> UpdateBillStatusAsync(string billId, string status, string userId)
        {
            try
            {
                var bill = await _context.Bills
                    .FirstOrDefaultAsync(b => b.Id == billId && b.UserId == userId);

                if (bill == null)
                {
                    return ApiResponse<bool>.ErrorResult("Bill not found");
                }

                bill.Status = status.ToUpper();
                bill.UpdatedAt = DateTime.UtcNow;

                if (status.ToUpper() == "PAID" && bill.PaidAt == null)
                {
                    bill.PaidAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Bill status updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to update bill status: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BillDto>>> GetOverdueBillsAsync(string userId)
        {
            try
            {
                var currentDate = DateTime.UtcNow.Date;
                
                var overdueBills = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.Status == "PENDING" && 
                               b.DueDate < currentDate)
                    .OrderBy(b => b.DueDate)
                    .ToListAsync();

                var billDtos = overdueBills.Select(MapToBillDto).ToList();
                return ApiResponse<List<BillDto>>.SuccessResult(billDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BillDto>>.ErrorResult($"Failed to get overdue bills: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BillDto>>> GetUpcomingBillsAsync(string userId, int days = 7)
        {
            try
            {
                var currentDate = DateTime.UtcNow.Date;
                var futureDate = currentDate.AddDays(days);
                
                var upcomingBills = await _context.Bills
                    .Where(b => b.UserId == userId && 
                               b.Status == "PENDING" && 
                               b.DueDate >= currentDate && 
                               b.DueDate <= futureDate)
                    .OrderBy(b => b.DueDate)
                    .ToListAsync();

                var billDtos = upcomingBills.Select(MapToBillDto).ToList();
                return ApiResponse<List<BillDto>>.SuccessResult(billDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<BillDto>>.ErrorResult($"Failed to get upcoming bills: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PaginatedResponse<BillDto>>> GetAllBillsAsync(string? status, string? billType, int page, int limit)
        {
            try
            {
                var query = _context.Bills.AsQueryable();

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(b => b.Status == status.ToUpper());
                }

                if (!string.IsNullOrEmpty(billType))
                {
                    query = query.Where(b => b.BillType == billType.ToLower());
                }

                var totalCount = await query.CountAsync();
                var bills = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var billDtos = bills.Select(MapToBillDto).ToList();

                var paginatedResponse = new PaginatedResponse<BillDto>
                {
                    Data = billDtos,
                    Page = page,
                    Limit = limit,
                    TotalCount = totalCount
                };

                return ApiResponse<PaginatedResponse<BillDto>>.SuccessResult(paginatedResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResponse<BillDto>>.ErrorResult($"Failed to get all bills: {ex.Message}");
            }
        }

        // Helper Methods
        private static BillDto MapToBillDto(Bill bill)
        {
            return new BillDto
            {
                Id = bill.Id,
                UserId = bill.UserId,
                BillName = bill.BillName,
                BillType = bill.BillType,
                Amount = bill.Amount,
                DueDate = bill.DueDate,
                Frequency = bill.Frequency,
                Status = bill.Status,
                CreatedAt = bill.CreatedAt,
                UpdatedAt = bill.UpdatedAt,
                PaidAt = bill.PaidAt,
                Notes = bill.Notes,
                Provider = bill.Provider,
                ReferenceNumber = bill.ReferenceNumber
            };
        }

        private static (DateTime startDate, DateTime endDate) GetPeriodDates(string period)
        {
            var now = DateTime.UtcNow;
            
            return period.ToLower() switch
            {
                "week" => (now.AddDays(-7).Date, now.Date),
                "month" => (now.AddDays(-30).Date, now.Date),
                "quarter" => (now.AddDays(-90).Date, now.Date),
                "year" => (now.AddDays(-365).Date, now.Date),
                _ => (now.AddDays(-30).Date, now.Date) // Default to month
            };
        }
    }
}
