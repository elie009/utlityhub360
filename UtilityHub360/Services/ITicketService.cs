using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    public interface ITicketService
    {
        Task<ApiResponse<PaginatedResponse<TicketDto>>> GetTicketsAsync(string userId, TicketFilters? filters, int page, int limit, bool isAdmin = false);
        Task<ApiResponse<TicketDto>> GetTicketByIdAsync(string ticketId, string userId, bool isAdmin = false);
        Task<ApiResponse<TicketDto>> CreateTicketAsync(CreateTicketDto createDto, string userId);
        Task<ApiResponse<TicketDto>> UpdateTicketAsync(string ticketId, UpdateTicketDto updateDto, string userId, bool isAdmin = false);
        Task<ApiResponse<bool>> DeleteTicketAsync(string ticketId, string userId, bool isAdmin = false);
        Task<ApiResponse<TicketCommentDto>> AddCommentAsync(string ticketId, CreateTicketCommentDto commentDto, string userId, bool isAdmin = false);
        Task<ApiResponse<List<TicketCommentDto>>> GetTicketCommentsAsync(string ticketId, string userId, bool isAdmin = false);
        Task<ApiResponse<TicketAttachmentDto>> AddAttachmentAsync(string ticketId, string fileName, string fileUrl, string? fileType, long? fileSize, string userId);
        Task<ApiResponse<List<TicketAttachmentDto>>> GetTicketAttachmentsAsync(string ticketId, string userId, bool isAdmin = false);
        Task<ApiResponse<TicketStatusHistoryDto>> UpdateTicketStatusAsync(string ticketId, string newStatus, string? notes, string userId, bool isAdmin = false);
        Task<ApiResponse<List<TicketStatusHistoryDto>>> GetTicketStatusHistoryAsync(string ticketId, string userId, bool isAdmin = false);
        Task<ApiResponse<TicketDto>> AssignTicketAsync(string ticketId, string? assignedToUserId, string userId, bool isAdmin = false);
    }
}

