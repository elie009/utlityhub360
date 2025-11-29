using Microsoft.EntityFrameworkCore;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<TicketService>? _logger;

        public TicketService(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<TicketService>? logger = null)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApiResponse<PaginatedResponse<TicketDto>>> GetTicketsAsync(string userId, TicketFilters? filters, int page, int limit, bool isAdmin = false)
        {
            try
            {
                var query = _context.Tickets.AsQueryable();

                // Non-admin users can only see their own tickets
                if (!isAdmin)
                {
                    query = query.Where(t => t.UserId == userId);
                }

                // Apply filters
                if (filters != null)
                {
                    if (!string.IsNullOrEmpty(filters.Status))
                    {
                        query = query.Where(t => t.Status == filters.Status);
                    }

                    if (!string.IsNullOrEmpty(filters.Priority))
                    {
                        query = query.Where(t => t.Priority == filters.Priority);
                    }

                    if (!string.IsNullOrEmpty(filters.Category))
                    {
                        query = query.Where(t => t.Category == filters.Category);
                    }

                    if (!string.IsNullOrEmpty(filters.AssignedTo))
                    {
                        query = query.Where(t => t.AssignedTo == filters.AssignedTo);
                    }

                    if (!string.IsNullOrEmpty(filters.Search))
                    {
                        var search = filters.Search.ToLower();
                        query = query.Where(t => 
                            t.Title.ToLower().Contains(search) || 
                            t.Description.ToLower().Contains(search));
                    }

                    if (filters.CreatedFrom.HasValue)
                    {
                        query = query.Where(t => t.CreatedAt >= filters.CreatedFrom.Value);
                    }

                    if (filters.CreatedTo.HasValue)
                    {
                        query = query.Where(t => t.CreatedAt <= filters.CreatedTo.Value);
                    }
                }

                var totalCount = await query.CountAsync();
                var tickets = await query
                    .Include(t => t.User)
                    .Include(t => t.AssignedUser)
                    .Include(t => t.ResolvedByUser)
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                var ticketDtos = tickets.Select(t => MapToDto(t)).ToList();

                var paginatedResponse = new PaginatedResponse<TicketDto>
                {
                    Data = ticketDtos,
                    TotalCount = totalCount,
                    Page = page,
                    Limit = limit
                };

                return ApiResponse<PaginatedResponse<TicketDto>>.SuccessResult(paginatedResponse, "Tickets retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving tickets");
                return ApiResponse<PaginatedResponse<TicketDto>>.ErrorResult($"Failed to retrieve tickets: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TicketDto>> GetTicketByIdAsync(string ticketId, string userId, bool isAdmin = false)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.User)
                    .Include(t => t.AssignedUser)
                    .Include(t => t.ResolvedByUser)
                    .Include(t => t.Comments)
                        .ThenInclude(c => c.User)
                    .Include(t => t.Attachments)
                    .Include(t => t.StatusHistory)
                        .ThenInclude(h => h.ChangedByUser)
                    .FirstOrDefaultAsync(t => t.Id == ticketId);

                if (ticket == null)
                {
                    return ApiResponse<TicketDto>.ErrorResult("Ticket not found");
                }

                // Non-admin users can only view their own tickets
                if (!isAdmin && ticket.UserId != userId)
                {
                    return ApiResponse<TicketDto>.ErrorResult("Access denied");
                }

                var ticketDto = MapToDto(ticket, includeDetails: true);
                return ApiResponse<TicketDto>.SuccessResult(ticketDto, "Ticket retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving ticket {TicketId}", ticketId);
                return ApiResponse<TicketDto>.ErrorResult($"Failed to retrieve ticket: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TicketDto>> CreateTicketAsync(CreateTicketDto createDto, string userId)
        {
            try
            {
                var ticket = new Ticket
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Title = createDto.Title,
                    Description = createDto.Description,
                    Status = "OPEN",
                    Priority = createDto.Priority ?? "NORMAL",
                    Category = createDto.Category ?? "GENERAL",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Tickets.Add(ticket);

                // Create initial status history entry
                var statusHistory = new TicketStatusHistory
                {
                    Id = Guid.NewGuid().ToString(),
                    TicketId = ticket.Id,
                    OldStatus = "",
                    NewStatus = "OPEN",
                    ChangedBy = userId,
                    ChangedAt = DateTime.UtcNow,
                    Notes = "Ticket created"
                };
                _context.TicketStatusHistories.Add(statusHistory);

                await _context.SaveChangesAsync();

                // Send email notification to admins (optional)
                try
                {
                    await NotifyAdminsNewTicket(ticket);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to send email notification for new ticket");
                }

                var ticketDto = await GetTicketByIdAsync(ticket.Id, userId);
                return ticketDto;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating ticket");
                return ApiResponse<TicketDto>.ErrorResult($"Failed to create ticket: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TicketDto>> UpdateTicketAsync(string ticketId, UpdateTicketDto updateDto, string userId, bool isAdmin = false)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == ticketId);

                if (ticket == null)
                {
                    return ApiResponse<TicketDto>.ErrorResult("Ticket not found");
                }

                // Non-admin users can only update their own tickets, and only certain fields
                if (!isAdmin)
                {
                    if (ticket.UserId != userId)
                    {
                        return ApiResponse<TicketDto>.ErrorResult("Access denied");
                    }
                    // Regular users can only update title and description
                    if (!string.IsNullOrEmpty(updateDto.Title))
                        ticket.Title = updateDto.Title;
                    if (!string.IsNullOrEmpty(updateDto.Description))
                        ticket.Description = updateDto.Description;
                }
                else
                {
                    // Admins can update all fields
                    if (!string.IsNullOrEmpty(updateDto.Title))
                        ticket.Title = updateDto.Title;
                    if (!string.IsNullOrEmpty(updateDto.Description))
                        ticket.Description = updateDto.Description;
                    if (!string.IsNullOrEmpty(updateDto.Status))
                    {
                        var oldStatus = ticket.Status;
                        ticket.Status = updateDto.Status;
                        ticket.UpdatedAt = DateTime.UtcNow;

                        // Create status history entry
                        var statusHistory = new TicketStatusHistory
                        {
                            Id = Guid.NewGuid().ToString(),
                            TicketId = ticket.Id,
                            OldStatus = oldStatus,
                            NewStatus = updateDto.Status,
                            ChangedBy = userId,
                            ChangedAt = DateTime.UtcNow,
                            Notes = updateDto.ResolutionNotes
                        };
                        _context.TicketStatusHistories.Add(statusHistory);

                        // Update resolved fields if status is RESOLVED or CLOSED
                        if (updateDto.Status == "RESOLVED" || updateDto.Status == "CLOSED")
                        {
                            ticket.ResolvedAt = DateTime.UtcNow;
                            ticket.ResolvedBy = userId;
                            ticket.ResolutionNotes = updateDto.ResolutionNotes;
                        }
                    }
                    if (!string.IsNullOrEmpty(updateDto.Priority))
                        ticket.Priority = updateDto.Priority;
                    if (!string.IsNullOrEmpty(updateDto.Category))
                        ticket.Category = updateDto.Category;
                    if (updateDto.AssignedTo != null)
                        ticket.AssignedTo = updateDto.AssignedTo;
                    if (!string.IsNullOrEmpty(updateDto.ResolutionNotes))
                        ticket.ResolutionNotes = updateDto.ResolutionNotes;
                }

                ticket.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Send email notification on status change
                if (!string.IsNullOrEmpty(updateDto.Status))
                {
                    try
                    {
                        await NotifyUserStatusChange(ticket, updateDto.Status);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to send email notification for status change");
                    }
                }

                var ticketDto = await GetTicketByIdAsync(ticket.Id, userId, isAdmin);
                return ticketDto;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating ticket {TicketId}", ticketId);
                return ApiResponse<TicketDto>.ErrorResult($"Failed to update ticket: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteTicketAsync(string ticketId, string userId, bool isAdmin = false)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                {
                    return ApiResponse<bool>.ErrorResult("Ticket not found");
                }

                // Only admins can delete tickets, or users can delete their own open tickets
                if (!isAdmin && (ticket.UserId != userId || ticket.Status != "OPEN"))
                {
                    return ApiResponse<bool>.ErrorResult("Access denied");
                }

                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "Ticket deleted successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting ticket {TicketId}", ticketId);
                return ApiResponse<bool>.ErrorResult($"Failed to delete ticket: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TicketCommentDto>> AddCommentAsync(string ticketId, CreateTicketCommentDto commentDto, string userId, bool isAdmin = false)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                {
                    return ApiResponse<TicketCommentDto>.ErrorResult("Ticket not found");
                }

                // Check access
                if (!isAdmin && ticket.UserId != userId)
                {
                    return ApiResponse<TicketCommentDto>.ErrorResult("Access denied");
                }

                var comment = new TicketComment
                {
                    Id = Guid.NewGuid().ToString(),
                    TicketId = ticketId,
                    UserId = userId,
                    Comment = commentDto.Comment,
                    IsInternal = commentDto.IsInternal && isAdmin, // Only admins can create internal comments
                    CreatedAt = DateTime.UtcNow
                };

                _context.TicketComments.Add(comment);
                ticket.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Load user info for response
                var commentWithUser = await _context.TicketComments
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == comment.Id);

                var commentDtoResponse = MapCommentToDto(commentWithUser!);

                // Send email notification
                try
                {
                    await NotifyUserNewComment(ticket, commentDtoResponse);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to send email notification for new comment");
                }

                return ApiResponse<TicketCommentDto>.SuccessResult(commentDtoResponse, "Comment added successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding comment to ticket {TicketId}", ticketId);
                return ApiResponse<TicketCommentDto>.ErrorResult($"Failed to add comment: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<TicketCommentDto>>> GetTicketCommentsAsync(string ticketId, string userId, bool isAdmin = false)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                {
                    return ApiResponse<List<TicketCommentDto>>.ErrorResult("Ticket not found");
                }

                // Check access
                if (!isAdmin && ticket.UserId != userId)
                {
                    return ApiResponse<List<TicketCommentDto>>.ErrorResult("Access denied");
                }

                var comments = await _context.TicketComments
                    .Include(c => c.User)
                    .Where(c => c.TicketId == ticketId)
                    .Where(c => isAdmin || !c.IsInternal) // Non-admins can't see internal comments
                    .OrderBy(c => c.CreatedAt)
                    .ToListAsync();

                var commentDtos = comments.Select(c => MapCommentToDto(c)).ToList();
                return ApiResponse<List<TicketCommentDto>>.SuccessResult(commentDtos, "Comments retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving comments for ticket {TicketId}", ticketId);
                return ApiResponse<List<TicketCommentDto>>.ErrorResult($"Failed to retrieve comments: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TicketAttachmentDto>> AddAttachmentAsync(string ticketId, string fileName, string fileUrl, string? fileType, long? fileSize, string userId)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                {
                    return ApiResponse<TicketAttachmentDto>.ErrorResult("Ticket not found");
                }

                // Check access - users can only add attachments to their own tickets
                if (ticket.UserId != userId)
                {
                    return ApiResponse<TicketAttachmentDto>.ErrorResult("Access denied");
                }

                var attachment = new TicketAttachment
                {
                    Id = Guid.NewGuid().ToString(),
                    TicketId = ticketId,
                    FileName = fileName,
                    FileUrl = fileUrl,
                    FileType = fileType,
                    FileSize = fileSize,
                    UploadedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.TicketAttachments.Add(attachment);
                ticket.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var attachmentDto = MapAttachmentToDto(attachment);
                return ApiResponse<TicketAttachmentDto>.SuccessResult(attachmentDto, "Attachment added successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding attachment to ticket {TicketId}", ticketId);
                return ApiResponse<TicketAttachmentDto>.ErrorResult($"Failed to add attachment: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<TicketAttachmentDto>>> GetTicketAttachmentsAsync(string ticketId, string userId, bool isAdmin = false)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                {
                    return ApiResponse<List<TicketAttachmentDto>>.ErrorResult("Ticket not found");
                }

                // Check access
                if (!isAdmin && ticket.UserId != userId)
                {
                    return ApiResponse<List<TicketAttachmentDto>>.ErrorResult("Access denied");
                }

                var attachments = await _context.TicketAttachments
                    .Include(a => a.UploadedByUser)
                    .Where(a => a.TicketId == ticketId)
                    .OrderBy(a => a.CreatedAt)
                    .ToListAsync();

                var attachmentDtos = attachments.Select(a => MapAttachmentToDto(a)).ToList();
                return ApiResponse<List<TicketAttachmentDto>>.SuccessResult(attachmentDtos, "Attachments retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving attachments for ticket {TicketId}", ticketId);
                return ApiResponse<List<TicketAttachmentDto>>.ErrorResult($"Failed to retrieve attachments: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TicketStatusHistoryDto>> UpdateTicketStatusAsync(string ticketId, string newStatus, string? notes, string userId, bool isAdmin = false)
        {
            try
            {
                if (!isAdmin)
                {
                    return ApiResponse<TicketStatusHistoryDto>.ErrorResult("Only admins can update ticket status");
                }

                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                {
                    return ApiResponse<TicketStatusHistoryDto>.ErrorResult("Ticket not found");
                }

                var oldStatus = ticket.Status;
                ticket.Status = newStatus;
                ticket.UpdatedAt = DateTime.UtcNow;

                // Update resolved fields if status is RESOLVED or CLOSED
                if (newStatus == "RESOLVED" || newStatus == "CLOSED")
                {
                    ticket.ResolvedAt = DateTime.UtcNow;
                    ticket.ResolvedBy = userId;
                    if (!string.IsNullOrEmpty(notes))
                        ticket.ResolutionNotes = notes;
                }

                // Create status history entry
                var statusHistory = new TicketStatusHistory
                {
                    Id = Guid.NewGuid().ToString(),
                    TicketId = ticketId,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    ChangedBy = userId,
                    ChangedAt = DateTime.UtcNow,
                    Notes = notes
                };
                _context.TicketStatusHistories.Add(statusHistory);

                await _context.SaveChangesAsync();

                // Send email notification
                try
                {
                    await NotifyUserStatusChange(ticket, newStatus);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to send email notification for status change");
                }

                var statusHistoryDto = MapStatusHistoryToDto(statusHistory);
                return ApiResponse<TicketStatusHistoryDto>.SuccessResult(statusHistoryDto, "Ticket status updated successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating ticket status {TicketId}", ticketId);
                return ApiResponse<TicketStatusHistoryDto>.ErrorResult($"Failed to update ticket status: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<TicketStatusHistoryDto>>> GetTicketStatusHistoryAsync(string ticketId, string userId, bool isAdmin = false)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                {
                    return ApiResponse<List<TicketStatusHistoryDto>>.ErrorResult("Ticket not found");
                }

                // Check access
                if (!isAdmin && ticket.UserId != userId)
                {
                    return ApiResponse<List<TicketStatusHistoryDto>>.ErrorResult("Access denied");
                }

                var history = await _context.TicketStatusHistories
                    .Include(h => h.ChangedByUser)
                    .Where(h => h.TicketId == ticketId)
                    .OrderBy(h => h.ChangedAt)
                    .ToListAsync();

                var historyDtos = history.Select(h => MapStatusHistoryToDto(h)).ToList();
                return ApiResponse<List<TicketStatusHistoryDto>>.SuccessResult(historyDtos, "Status history retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving status history for ticket {TicketId}", ticketId);
                return ApiResponse<List<TicketStatusHistoryDto>>.ErrorResult($"Failed to retrieve status history: {ex.Message}");
            }
        }

        public async Task<ApiResponse<TicketDto>> AssignTicketAsync(string ticketId, string? assignedToUserId, string userId, bool isAdmin = false)
        {
            try
            {
                if (!isAdmin)
                {
                    return ApiResponse<TicketDto>.ErrorResult("Only admins can assign tickets");
                }

                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null)
                {
                    return ApiResponse<TicketDto>.ErrorResult("Ticket not found");
                }

                // Verify assigned user exists if provided
                if (!string.IsNullOrEmpty(assignedToUserId))
                {
                    var assignedUser = await _context.Users.FindAsync(assignedToUserId);
                    if (assignedUser == null)
                    {
                        return ApiResponse<TicketDto>.ErrorResult("Assigned user not found");
                    }
                }

                ticket.AssignedTo = assignedToUserId;
                ticket.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var ticketDto = await GetTicketByIdAsync(ticketId, userId, isAdmin);
                return ticketDto;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error assigning ticket {TicketId}", ticketId);
                return ApiResponse<TicketDto>.ErrorResult($"Failed to assign ticket: {ex.Message}");
            }
        }

        // Helper methods
        private TicketDto MapToDto(Ticket ticket, bool includeDetails = false)
        {
            var dto = new TicketDto
            {
                Id = ticket.Id,
                UserId = ticket.UserId,
                UserName = ticket.User?.Name,
                UserEmail = ticket.User?.Email,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                Category = ticket.Category,
                AssignedTo = ticket.AssignedTo,
                AssignedToName = ticket.AssignedUser?.Name,
                ResolutionNotes = ticket.ResolutionNotes,
                ResolvedAt = ticket.ResolvedAt,
                ResolvedBy = ticket.ResolvedBy,
                ResolvedByName = ticket.ResolvedByUser?.Name,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                CommentCount = includeDetails ? ticket.Comments?.Count ?? 0 : 0,
                AttachmentCount = includeDetails ? ticket.Attachments?.Count ?? 0 : 0
            };

            if (includeDetails)
            {
                dto.Comments = ticket.Comments?.Select(c => MapCommentToDto(c)).ToList();
                dto.Attachments = ticket.Attachments?.Select(a => MapAttachmentToDto(a)).ToList();
                dto.StatusHistory = ticket.StatusHistory?.Select(h => MapStatusHistoryToDto(h)).ToList();
            }

            return dto;
        }

        private TicketCommentDto MapCommentToDto(TicketComment comment)
        {
            return new TicketCommentDto
            {
                Id = comment.Id,
                TicketId = comment.TicketId,
                UserId = comment.UserId,
                UserName = comment.User?.Name,
                Comment = comment.Comment,
                IsInternal = comment.IsInternal,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }

        private TicketAttachmentDto MapAttachmentToDto(TicketAttachment attachment)
        {
            return new TicketAttachmentDto
            {
                Id = attachment.Id,
                TicketId = attachment.TicketId,
                FileName = attachment.FileName,
                FileUrl = attachment.FileUrl,
                FileType = attachment.FileType,
                FileSize = attachment.FileSize,
                UploadedBy = attachment.UploadedBy,
                UploadedByName = attachment.UploadedByUser?.Name,
                CreatedAt = attachment.CreatedAt
            };
        }

        private TicketStatusHistoryDto MapStatusHistoryToDto(TicketStatusHistory history)
        {
            return new TicketStatusHistoryDto
            {
                Id = history.Id,
                TicketId = history.TicketId,
                OldStatus = history.OldStatus,
                NewStatus = history.NewStatus,
                ChangedBy = history.ChangedBy,
                ChangedByName = history.ChangedByUser?.Name,
                Notes = history.Notes,
                ChangedAt = history.ChangedAt
            };
        }

        private async Task NotifyAdminsNewTicket(Ticket ticket)
        {
            try
            {
                var admins = await _context.Users
                    .Where(u => u.Role == "ADMIN")
                    .ToListAsync();

                foreach (var admin in admins)
                {
                    await _emailService.SendEmailAsync(
                        admin.Email,
                        "New Support Ticket Created",
                        $"A new support ticket has been created:\n\n" +
                        $"Title: {ticket.Title}\n" +
                        $"Category: {ticket.Category}\n" +
                        $"Priority: {ticket.Priority}\n" +
                        $"Description: {ticket.Description}\n\n" +
                        $"Please review and assign the ticket."
                    );
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error notifying admins of new ticket");
            }
        }

        private async Task NotifyUserStatusChange(Ticket ticket, string newStatus)
        {
            try
            {
                var user = await _context.Users.FindAsync(ticket.UserId);
                if (user == null) return;

                await _emailService.SendEmailAsync(
                    user.Email,
                    $"Ticket Status Updated: {ticket.Title}",
                    $"Your support ticket status has been updated to: {newStatus}\n\n" +
                    $"Ticket: {ticket.Title}\n" +
                    $"Previous Status: {ticket.Status}\n" +
                    $"New Status: {newStatus}\n\n" +
                    (!string.IsNullOrEmpty(ticket.ResolutionNotes) 
                        ? $"Resolution Notes: {ticket.ResolutionNotes}\n" 
                        : "")
                );
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error notifying user of status change");
            }
        }

        private async Task NotifyUserNewComment(Ticket ticket, TicketCommentDto comment)
        {
            try
            {
                var user = await _context.Users.FindAsync(ticket.UserId);
                if (user == null) return;

                // Don't notify if the comment is from the ticket owner
                if (comment.UserId == ticket.UserId) return;

                await _emailService.SendEmailAsync(
                    user.Email,
                    $"New Comment on Ticket: {ticket.Title}",
                    $"A new comment has been added to your support ticket:\n\n" +
                    $"Ticket: {ticket.Title}\n" +
                    $"Comment by: {comment.UserName}\n" +
                    $"Comment: {comment.Comment}\n\n" +
                    $"Please review the ticket for more details."
                );
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error notifying user of new comment");
            }
        }
    }
}

