using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly IFileStorageService _fileStorageService;

        public TicketsController(ITicketService ticketService, IFileStorageService fileStorageService)
        {
            _ticketService = ticketService;
            _fileStorageService = fileStorageService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<TicketDto>>>> GetTickets(
            [FromQuery] string? status = null,
            [FromQuery] string? priority = null,
            [FromQuery] string? category = null,
            [FromQuery] string? assignedTo = null,
            [FromQuery] string? search = null,
            [FromQuery] DateTime? createdFrom = null,
            [FromQuery] DateTime? createdTo = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var isAdmin = userRole == "ADMIN";

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<PaginatedResponse<TicketDto>>.ErrorResult("User not authenticated"));
                }

                var filters = new TicketFilters
                {
                    Status = status,
                    Priority = priority,
                    Category = category,
                    AssignedTo = assignedTo,
                    Search = search,
                    CreatedFrom = createdFrom,
                    CreatedTo = createdTo
                };

                var result = await _ticketService.GetTicketsAsync(userId, filters, page, limit, isAdmin);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<PaginatedResponse<TicketDto>>.ErrorResult($"Failed to get tickets: {ex.Message}"));
            }
        }

        [HttpGet("{ticketId}")]
        public async Task<ActionResult<ApiResponse<TicketDto>>> GetTicketById(string ticketId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var isAdmin = userRole == "ADMIN";

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<TicketDto>.ErrorResult("User not authenticated"));
                }

                var result = await _ticketService.GetTicketByIdAsync(ticketId, userId, isAdmin);

                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TicketDto>.ErrorResult($"Failed to get ticket: {ex.Message}"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<TicketDto>>> CreateTicket([FromBody] CreateTicketDto createDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<TicketDto>.ErrorResult("User not authenticated"));
                }

                if (string.IsNullOrEmpty(createDto.Title) || string.IsNullOrEmpty(createDto.Description))
                {
                    return BadRequest(ApiResponse<TicketDto>.ErrorResult("Title and description are required"));
                }

                var result = await _ticketService.CreateTicketAsync(createDto, userId);

                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetTicketById), new { ticketId = result.Data?.Id }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TicketDto>.ErrorResult($"Failed to create ticket: {ex.Message}"));
            }
        }

        [HttpPut("{ticketId}")]
        public async Task<ActionResult<ApiResponse<TicketDto>>> UpdateTicket(string ticketId, [FromBody] UpdateTicketDto updateDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var isAdmin = userRole == "ADMIN";

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<TicketDto>.ErrorResult("User not authenticated"));
                }

                var result = await _ticketService.UpdateTicketAsync(ticketId, updateDto, userId, isAdmin);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TicketDto>.ErrorResult($"Failed to update ticket: {ex.Message}"));
            }
        }

        [HttpDelete("{ticketId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteTicket(string ticketId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var isAdmin = userRole == "ADMIN";

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResult("User not authenticated"));
                }

                var result = await _ticketService.DeleteTicketAsync(ticketId, userId, isAdmin);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete ticket: {ex.Message}"));
            }
        }

        [HttpPost("{ticketId}/comments")]
        public async Task<ActionResult<ApiResponse<TicketCommentDto>>> AddComment(string ticketId, [FromBody] CreateTicketCommentDto commentDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var isAdmin = userRole == "ADMIN";

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<TicketCommentDto>.ErrorResult("User not authenticated"));
                }

                if (string.IsNullOrEmpty(commentDto.Comment))
                {
                    return BadRequest(ApiResponse<TicketCommentDto>.ErrorResult("Comment is required"));
                }

                var result = await _ticketService.AddCommentAsync(ticketId, commentDto, userId, isAdmin);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TicketCommentDto>.ErrorResult($"Failed to add comment: {ex.Message}"));
            }
        }

        [HttpGet("{ticketId}/comments")]
        public async Task<ActionResult<ApiResponse<List<TicketCommentDto>>>> GetTicketComments(string ticketId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var isAdmin = userRole == "ADMIN";

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<TicketCommentDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _ticketService.GetTicketCommentsAsync(ticketId, userId, isAdmin);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<TicketCommentDto>>.ErrorResult($"Failed to get comments: {ex.Message}"));
            }
        }

        [HttpPost("{ticketId}/attachments")]
        public async Task<ActionResult<ApiResponse<TicketAttachmentDto>>> AddAttachment(
            string ticketId,
            [FromForm] IFormFile file)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<TicketAttachmentDto>.ErrorResult("User not authenticated"));
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest(ApiResponse<TicketAttachmentDto>.ErrorResult("File is required"));
                }

                // Upload file
                string fileUrl;
                using (var fileStream = file.OpenReadStream())
                {
                    fileUrl = await _fileStorageService.SaveFileAsync(
                        fileStream,
                        file.FileName,
                        userId,
                        file.ContentType ?? "application/octet-stream"
                    );
                }
                
                if (string.IsNullOrEmpty(fileUrl))
                {
                    return BadRequest(ApiResponse<TicketAttachmentDto>.ErrorResult("Failed to upload file"));
                }
                
                // Get full URL if needed
                fileUrl = _fileStorageService.GetFileUrl(fileUrl);

                var result = await _ticketService.AddAttachmentAsync(
                    ticketId,
                    file.FileName,
                    fileUrl,
                    file.ContentType,
                    file.Length,
                    userId
                );

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TicketAttachmentDto>.ErrorResult($"Failed to add attachment: {ex.Message}"));
            }
        }

        [HttpGet("{ticketId}/attachments")]
        public async Task<ActionResult<ApiResponse<List<TicketAttachmentDto>>>> GetTicketAttachments(string ticketId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var isAdmin = userRole == "ADMIN";

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<TicketAttachmentDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _ticketService.GetTicketAttachmentsAsync(ticketId, userId, isAdmin);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<TicketAttachmentDto>>.ErrorResult($"Failed to get attachments: {ex.Message}"));
            }
        }

        [HttpPut("{ticketId}/status")]
        public async Task<ActionResult<ApiResponse<TicketStatusHistoryDto>>> UpdateTicketStatus(
            string ticketId,
            [FromBody] UpdateTicketStatusRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var isAdmin = userRole == "ADMIN";

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<TicketStatusHistoryDto>.ErrorResult("User not authenticated"));
                }

                var result = await _ticketService.UpdateTicketStatusAsync(ticketId, request.Status, request.Notes, userId, isAdmin);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TicketStatusHistoryDto>.ErrorResult($"Failed to update ticket status: {ex.Message}"));
            }
        }

        [HttpGet("{ticketId}/status-history")]
        public async Task<ActionResult<ApiResponse<List<TicketStatusHistoryDto>>>> GetTicketStatusHistory(string ticketId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var isAdmin = userRole == "ADMIN";

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<TicketStatusHistoryDto>>.ErrorResult("User not authenticated"));
                }

                var result = await _ticketService.GetTicketStatusHistoryAsync(ticketId, userId, isAdmin);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<TicketStatusHistoryDto>>.ErrorResult($"Failed to get status history: {ex.Message}"));
            }
        }

        [HttpPut("{ticketId}/assign")]
        public async Task<ActionResult<ApiResponse<TicketDto>>> AssignTicket(
            string ticketId,
            [FromBody] AssignTicketRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var isAdmin = userRole == "ADMIN";

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<TicketDto>.ErrorResult("User not authenticated"));
                }

                var result = await _ticketService.AssignTicketAsync(ticketId, request.AssignedTo, userId, isAdmin);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TicketDto>.ErrorResult($"Failed to assign ticket: {ex.Message}"));
            }
        }
    }

    public class UpdateTicketStatusRequest
    {
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class AssignTicketRequest
    {
        public string? AssignedTo { get; set; }
    }
}

