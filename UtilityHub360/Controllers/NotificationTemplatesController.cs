using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationTemplatesController : ControllerBase
    {
        private readonly IEnhancedNotificationService _notificationService;

        public NotificationTemplatesController(IEnhancedNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<NotificationTemplateDto>>>> GetTemplates(
            [FromQuery] string? notificationType = null,
            [FromQuery] string? channel = null)
        {
            try
            {
                var result = await _notificationService.GetTemplatesAsync(notificationType, channel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<NotificationTemplateDto>>.ErrorResult($"Failed to get templates: {ex.Message}"));
            }
        }

        [HttpGet("{templateId}")]
        public async Task<ActionResult<ApiResponse<NotificationTemplateDto>>> GetTemplate(string templateId)
        {
            try
            {
                var result = await _notificationService.GetTemplateAsync(templateId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<NotificationTemplateDto>.ErrorResult($"Failed to get template: {ex.Message}"));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<NotificationTemplateDto>>> CreateTemplate([FromBody] CreateNotificationTemplateDto template)
        {
            try
            {
                var result = await _notificationService.CreateTemplateAsync(template);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<NotificationTemplateDto>.ErrorResult($"Failed to create template: {ex.Message}"));
            }
        }

        [HttpPut("{templateId}")]
        public async Task<ActionResult<ApiResponse<NotificationTemplateDto>>> UpdateTemplate(string templateId, [FromBody] UpdateNotificationTemplateDto template)
        {
            try
            {
                var result = await _notificationService.UpdateTemplateAsync(templateId, template);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<NotificationTemplateDto>.ErrorResult($"Failed to update template: {ex.Message}"));
            }
        }

        [HttpDelete("{templateId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteTemplate(string templateId)
        {
            try
            {
                var result = await _notificationService.DeleteTemplateAsync(templateId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult($"Failed to delete template: {ex.Message}"));
            }
        }

        [HttpPost("{templateId}/render")]
        public async Task<ActionResult<ApiResponse<string>>> RenderTemplate(string templateId, [FromBody] Dictionary<string, string> variables)
        {
            try
            {
                var result = await _notificationService.RenderTemplateAsync(templateId, variables);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResult($"Failed to render template: {ex.Message}"));
            }
        }
    }
}

