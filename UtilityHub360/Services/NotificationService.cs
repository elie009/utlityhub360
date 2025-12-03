using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Entities;
using UtilityHub360.Models;

namespace UtilityHub360.Services
{
    // Helper class for loading enhanced notification data via raw SQL
    public class NotificationEnhancedData
    {
        public string Id { get; set; } = string.Empty;
        public string? Channel { get; set; }
        public string? Priority { get; set; }
        public DateTime? ScheduledFor { get; set; }
        public string? TemplateId { get; set; }
        public string? TemplateVariables { get; set; }
        public string? Status { get; set; }
    }

    public class NotificationService : INotificationService, IEnhancedNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<NotificationService>? _logger;

        public NotificationService(
            ApplicationDbContext context,
            IEmailService emailService,
            ISmsService smsService,
            ILogger<NotificationService>? logger = null)
        {
            _context = context;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task<ApiResponse<PaginatedResponse<NotificationDto>>> GetUserNotificationsAsync(string userId, string? status, string? type, int page, int limit)
        {
            try
            {
                var query = _context.Notifications.Where(n => n.UserId == userId);

                if (!string.IsNullOrEmpty(status))
                {
                    if (status.ToLower() == "unread")
                    {
                        query = query.Where(n => !n.IsRead);
                    }
                    else if (status.ToLower() == "read")
                    {
                        query = query.Where(n => n.IsRead);
                    }
                }

                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(n => n.Type == type);
                }

                var totalCount = await query.CountAsync();
                var notifications = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                // Load enhanced columns using raw SQL since EF model snapshot doesn't include them yet
                var notificationIds = notifications.Select(n => n.Id).ToList();
                var enhancedData = new Dictionary<string, NotificationEnhancedData>();
                
                if (notificationIds.Any())
                {
                    try
                    {
                        // Use direct SQL execution to load enhanced columns
                        var connection = _context.Database.GetDbConnection();
                        var wasOpen = connection.State == System.Data.ConnectionState.Open;
                        if (!wasOpen) await connection.OpenAsync();
                        
                        using var command = connection.CreateCommand();
                        // Build safe parameterized query
                        var parameters = new List<object>();
                        var placeholders = new List<string>();
                        for (int i = 0; i < notificationIds.Count; i++)
                        {
                            placeholders.Add($"@p{i}");
                            var param = command.CreateParameter();
                            param.ParameterName = $"@p{i}";
                            param.Value = notificationIds[i];
                            command.Parameters.Add(param);
                        }
                        
                        command.CommandText = $"SELECT Id, Channel, Priority, ScheduledFor, TemplateId, TemplateVariables, Status FROM Notifications WHERE Id IN ({string.Join(",", placeholders)})";
                        
                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            var id = reader.GetString(0);
                            enhancedData[id] = new NotificationEnhancedData
                            {
                                Id = id,
                                Channel = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Priority = reader.IsDBNull(2) ? null : reader.GetString(2),
                                ScheduledFor = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                                TemplateId = reader.IsDBNull(4) ? null : reader.GetString(4),
                                TemplateVariables = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Status = reader.IsDBNull(6) ? null : reader.GetString(6)
                            };
                        }
                        
                        if (!wasOpen) await connection.CloseAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to load enhanced notification data, using defaults");
                    }
                }

                var notificationDtos = notifications.Select(notification =>
                {
                    var enhanced = enhancedData.ContainsKey(notification.Id) ? enhancedData[notification.Id] : null;
                    return new NotificationDto
                    {
                        Id = notification.Id,
                        UserId = notification.UserId,
                        Type = notification.Type,
                        Title = notification.Title,
                        Message = notification.Message,
                        IsRead = notification.IsRead,
                        CreatedAt = notification.CreatedAt,
                        ReadAt = notification.ReadAt,
                        Channel = enhanced?.Channel,
                        Priority = enhanced?.Priority ?? "NORMAL",
                        ScheduledFor = enhanced?.ScheduledFor,
                        TemplateId = enhanced?.TemplateId,
                        TemplateVariables = !string.IsNullOrEmpty(enhanced?.TemplateVariables)
                            ? JsonSerializer.Deserialize<Dictionary<string, string>>(enhanced.TemplateVariables)
                            : null,
                        Status = enhanced?.Status ?? "PENDING"
                    };
                }).ToList();

                var paginatedResponse = new PaginatedResponse<NotificationDto>
                {
                    Data = notificationDtos,
                    Page = page,
                    Limit = limit,
                    TotalCount = totalCount
                };

                return ApiResponse<PaginatedResponse<NotificationDto>>.SuccessResult(paginatedResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResponse<NotificationDto>>.ErrorResult($"Failed to get notifications: {ex.Message}");
            }
        }

        public async Task<ApiResponse<NotificationDto>> MarkNotificationAsReadAsync(string notificationId, string userId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

                if (notification == null)
                {
                    return ApiResponse<NotificationDto>.ErrorResult("Notification not found");
                }

                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var notificationDto = new NotificationDto
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    Type = notification.Type,
                    Title = notification.Title,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt,
                    ReadAt = notification.ReadAt,
                    Channel = notification.Channel,
                    Priority = notification.Priority,
                    ScheduledFor = notification.ScheduledFor,
                    TemplateId = notification.TemplateId,
                    TemplateVariables = !string.IsNullOrEmpty(notification.TemplateVariables)
                        ? JsonSerializer.Deserialize<Dictionary<string, string>>(notification.TemplateVariables)
                        : null,
                    Status = notification.Status
                };

                return ApiResponse<NotificationDto>.SuccessResult(notificationDto, "Notification marked as read");
            }
            catch (Exception ex)
            {
                return ApiResponse<NotificationDto>.ErrorResult($"Failed to mark notification as read: {ex.Message}");
            }
        }

        public async Task<ApiResponse<NotificationDto>> SendNotificationAsync(CreateNotificationDto notification)
        {
            try
            {
                // Check for duplicate notifications within the last 24 hours
                // This prevents spam from background services running multiple times
                var twentyFourHoursAgo = DateTime.UtcNow.AddHours(-24);
                var existingNotifications = await _context.Notifications
                    .Where(n => 
                        n.UserId == notification.UserId &&
                        n.Type == notification.Type &&
                        n.CreatedAt >= twentyFourHoursAgo)
                    .ToListAsync();
                
                // Check if any existing notification matches this one
                // For bill/loan notifications, also check template variables to match specific bill/loan
                Entities.Notification? existingNotification = null;
                if (existingNotifications.Any())
                {
                    if (notification.TemplateVariables != null && notification.TemplateVariables.Count > 0)
                    {
                        // Check for matching billId or loanId in template variables
                        var billId = notification.TemplateVariables.ContainsKey("billId") 
                            ? notification.TemplateVariables["billId"] 
                            : null;
                        var loanId = notification.TemplateVariables.ContainsKey("loanId") 
                            ? notification.TemplateVariables["loanId"] 
                            : null;
                        
                        foreach (var existing in existingNotifications)
                        {
                            if (string.IsNullOrEmpty(existing.TemplateVariables)) continue;
                            
                            try
                            {
                                var existingVars = JsonSerializer.Deserialize<Dictionary<string, string>>(existing.TemplateVariables);
                                if (existingVars != null)
                                {
                                    var matches = true;
                                    if (billId != null && (!existingVars.ContainsKey("billId") || existingVars["billId"] != billId))
                                        matches = false;
                                    if (loanId != null && (!existingVars.ContainsKey("loanId") || existingVars["loanId"] != loanId))
                                        matches = false;
                                    
                                    if (matches)
                                    {
                                        existingNotification = existing;
                                        break;
                                    }
                                }
                            }
                            catch
                            {
                                // If JSON parsing fails, skip this notification
                                continue;
                            }
                        }
                    }
                    else
                    {
                        // No template variables, just check type and user
                        existingNotification = existingNotifications.FirstOrDefault();
                    }
                }
                
                // If duplicate found and it's not scheduled, return the existing notification
                if (existingNotification != null && !notification.ScheduledFor.HasValue)
                {
                    _logger?.LogInformation($"Duplicate notification prevented for user {notification.UserId}, type {notification.Type}");
                    var existingDto = MapToDto(existingNotification);
                    return ApiResponse<NotificationDto>.SuccessResult(existingDto, "Notification already exists (duplicate prevented)");
                }

                var newNotification = new Entities.Notification
                {
                    UserId = notification.UserId,
                    Type = notification.Type,
                    Title = notification.Title,
                    Message = notification.Message,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    Channel = notification.Channel ?? "IN_APP",
                    Priority = notification.Priority ?? "NORMAL",
                    ScheduledFor = notification.ScheduledFor,
                    TemplateId = notification.TemplateId,
                    TemplateVariables = notification.TemplateVariables != null
                        ? JsonSerializer.Serialize(notification.TemplateVariables)
                        : null,
                    Status = notification.ScheduledFor.HasValue ? "SCHEDULED" : "PENDING"
                };

                _context.Notifications.Add(newNotification);
                await _context.SaveChangesAsync();

                // Send immediately if not scheduled
                if (!notification.ScheduledFor.HasValue)
                {
                    await ProcessNotificationAsync(newNotification);
                }

                var notificationDto = MapToDto(newNotification);
                return ApiResponse<NotificationDto>.SuccessResult(notificationDto, "Notification sent successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<NotificationDto>.ErrorResult($"Failed to send notification: {ex.Message}");
            }
        }

        public async Task<ApiResponse<int>> GetUnreadNotificationCountAsync(string userId)
        {
            try
            {
                var count = await _context.Notifications
                    .CountAsync(n => n.UserId == userId && !n.IsRead);

                return ApiResponse<int>.SuccessResult(count);
            }
            catch (Exception ex)
            {
                return ApiResponse<int>.ErrorResult($"Failed to get unread notification count: {ex.Message}");
            }
        }

        public async Task<ApiResponse<int>> DeleteAllNotificationsAsync(string userId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .ToListAsync();

                var count = notifications.Count;

                if (count > 0)
                {
                    _context.Notifications.RemoveRange(notifications);
                    await _context.SaveChangesAsync();
                    _logger?.LogInformation($"Deleted {count} notifications for user {userId}");
                }

                return ApiResponse<int>.SuccessResult(count, $"Successfully deleted {count} notification(s)");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Failed to delete all notifications for user {userId}");
                return ApiResponse<int>.ErrorResult($"Failed to delete notifications: {ex.Message}");
            }
        }

        // ==========================================
        // ENHANCED NOTIFICATION METHODS
        // ==========================================

        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                Channel = notification.Channel,
                Priority = notification.Priority,
                ScheduledFor = notification.ScheduledFor,
                TemplateId = notification.TemplateId,
                TemplateVariables = !string.IsNullOrEmpty(notification.TemplateVariables)
                    ? JsonSerializer.Deserialize<Dictionary<string, string>>(notification.TemplateVariables)
                    : null,
                Status = notification.Status
            };
        }

        private async Task ProcessNotificationAsync(Notification notification)
        {
            try
            {
                // Get user preferences
                var preference = await _context.NotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == notification.UserId && p.NotificationType == notification.Type);

                var user = await _context.Users.FindAsync(notification.UserId);
                if (user == null) return;

                // Check quiet hours
                if (preference?.QuietHoursEnabled == true && IsInQuietHours(preference))
                {
                    _logger?.LogInformation($"Notification {notification.Id} skipped due to quiet hours");
                    return;
                }

                // Send via enabled channels
                var channels = GetEnabledChannels(preference, notification.Channel ?? "IN_APP");
                
                foreach (var channel in channels)
                {
                    await SendViaChannelAsync(notification, channel, user, preference);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error processing notification {notification.Id}");
            }
        }

        private List<string> GetEnabledChannels(NotificationPreference? preference, string defaultChannel)
        {
            var channels = new List<string>();

            if (preference == null)
            {
                // Default: only in-app
                channels.Add("IN_APP");
                return channels;
            }

            if (preference.InAppEnabled) channels.Add("IN_APP");
            if (preference.EmailEnabled) channels.Add("EMAIL");
            if (preference.SmsEnabled) channels.Add("SMS");
            if (preference.PushEnabled) channels.Add("PUSH");

            // If no preferences set, use default
            if (channels.Count == 0)
            {
                channels.Add(defaultChannel);
            }

            return channels;
        }

        private bool IsInQuietHours(NotificationPreference preference)
        {
            if (!preference.QuietHoursEnabled || string.IsNullOrEmpty(preference.QuietHoursStart) || string.IsNullOrEmpty(preference.QuietHoursEnd))
                return false;

            var now = DateTime.UtcNow.TimeOfDay;
            var start = TimeSpan.Parse(preference.QuietHoursStart);
            var end = TimeSpan.Parse(preference.QuietHoursEnd);

            if (start <= end)
            {
                return now >= start && now <= end;
            }
            else
            {
                // Quiet hours span midnight
                return now >= start || now <= end;
            }
        }

        private async Task SendViaChannelAsync(Notification notification, string channel, User user, NotificationPreference? preference)
        {
            var history = new NotificationHistory
            {
                UserId = notification.UserId,
                NotificationId = notification.Id,
                NotificationType = notification.Type,
                Channel = channel,
                Subject = notification.Title,
                Message = notification.Message,
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                bool success = false;
                string? externalId = null;
                string? errorMessage = null;

                switch (channel.ToUpper())
                {
                    case "EMAIL":
                        if (!string.IsNullOrEmpty(user.Email))
                        {
                            history.Recipient = user.Email;
                            success = await _emailService.SendEmailAsync(user.Email, notification.Title, notification.Message);
                            if (!success) errorMessage = "Email service failed";
                        }
                        break;

                    case "SMS":
                        if (!string.IsNullOrEmpty(user.Phone))
                        {
                            history.Recipient = user.Phone;
                            success = await _smsService.SendSmsAsync(user.Phone, notification.Message);
                            if (!success) errorMessage = "SMS service failed";
                        }
                        break;

                    case "PUSH":
                        // TODO: Implement push notification service
                        history.Recipient = "Device Token";
                        _logger?.LogInformation($"Push notification would be sent for notification {notification.Id}");
                        success = true; // Placeholder
                        break;

                    case "IN_APP":
                        // In-app notifications are already created
                        history.Recipient = user.Id;
                        success = true;
                        break;
                }

                history.Status = success ? "SENT" : "FAILED";
                history.SentAt = success ? DateTime.UtcNow : null;
                history.ErrorMessage = errorMessage;

                if (success)
                {
                    notification.Status = "SENT";
                }
            }
            catch (Exception ex)
            {
                history.Status = "FAILED";
                history.ErrorMessage = ex.Message;
                _logger?.LogError(ex, $"Failed to send {channel} notification {notification.Id}");
            }
            finally
            {
                _context.NotificationHistories.Add(history);
                await _context.SaveChangesAsync();
            }
        }

        // Notification Preferences Methods
        public async Task<ApiResponse<List<NotificationPreferenceDto>>> GetUserPreferencesAsync(string userId)
        {
            try
            {
                var preferences = await _context.NotificationPreferences
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                var dtos = preferences.Select(p => MapPreferenceToDto(p)).ToList();
                return ApiResponse<List<NotificationPreferenceDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<NotificationPreferenceDto>>.ErrorResult($"Failed to get preferences: {ex.Message}");
            }
        }

        public async Task<ApiResponse<NotificationPreferenceDto>> GetPreferenceAsync(string userId, string notificationType)
        {
            try
            {
                var preference = await _context.NotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.NotificationType == notificationType);

                if (preference == null)
                {
                    return ApiResponse<NotificationPreferenceDto>.ErrorResult("Preference not found");
                }

                return ApiResponse<NotificationPreferenceDto>.SuccessResult(MapPreferenceToDto(preference));
            }
            catch (Exception ex)
            {
                return ApiResponse<NotificationPreferenceDto>.ErrorResult($"Failed to get preference: {ex.Message}");
            }
        }

        public async Task<ApiResponse<NotificationPreferenceDto>> CreatePreferenceAsync(string userId, CreateNotificationPreferenceDto preferenceDto)
        {
            try
            {
                var existing = await _context.NotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.NotificationType == preferenceDto.NotificationType);

                if (existing != null)
                {
                    return ApiResponse<NotificationPreferenceDto>.ErrorResult("Preference already exists. Use update instead.");
                }

                var preference = new NotificationPreference
                {
                    UserId = userId,
                    NotificationType = preferenceDto.NotificationType,
                    InAppEnabled = preferenceDto.InAppEnabled,
                    EmailEnabled = preferenceDto.EmailEnabled,
                    SmsEnabled = preferenceDto.SmsEnabled,
                    PushEnabled = preferenceDto.PushEnabled,
                    ScheduledEnabled = preferenceDto.ScheduledEnabled,
                    ScheduleTime = preferenceDto.ScheduleTime,
                    ScheduleDays = preferenceDto.ScheduleDays != null ? JsonSerializer.Serialize(preferenceDto.ScheduleDays) : null,
                    QuietHoursEnabled = preferenceDto.QuietHoursEnabled,
                    QuietHoursStart = preferenceDto.QuietHoursStart,
                    QuietHoursEnd = preferenceDto.QuietHoursEnd,
                    MaxNotificationsPerDay = preferenceDto.MaxNotificationsPerDay,
                    MinMinutesBetweenNotifications = preferenceDto.MinMinutesBetweenNotifications,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.NotificationPreferences.Add(preference);
                await _context.SaveChangesAsync();

                return ApiResponse<NotificationPreferenceDto>.SuccessResult(MapPreferenceToDto(preference));
            }
            catch (Exception ex)
            {
                return ApiResponse<NotificationPreferenceDto>.ErrorResult($"Failed to create preference: {ex.Message}");
            }
        }

        public async Task<ApiResponse<NotificationPreferenceDto>> UpdatePreferenceAsync(string userId, string notificationType, UpdateNotificationPreferenceDto preferenceDto)
        {
            try
            {
                var preference = await _context.NotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.NotificationType == notificationType);

                if (preference == null)
                {
                    return ApiResponse<NotificationPreferenceDto>.ErrorResult("Preference not found");
                }

                if (preferenceDto.InAppEnabled.HasValue) preference.InAppEnabled = preferenceDto.InAppEnabled.Value;
                if (preferenceDto.EmailEnabled.HasValue) preference.EmailEnabled = preferenceDto.EmailEnabled.Value;
                if (preferenceDto.SmsEnabled.HasValue) preference.SmsEnabled = preferenceDto.SmsEnabled.Value;
                if (preferenceDto.PushEnabled.HasValue) preference.PushEnabled = preferenceDto.PushEnabled.Value;
                if (preferenceDto.ScheduledEnabled.HasValue) preference.ScheduledEnabled = preferenceDto.ScheduledEnabled.Value;
                if (preferenceDto.ScheduleTime != null) preference.ScheduleTime = preferenceDto.ScheduleTime;
                if (preferenceDto.ScheduleDays != null) preference.ScheduleDays = JsonSerializer.Serialize(preferenceDto.ScheduleDays);
                if (preferenceDto.QuietHoursEnabled.HasValue) preference.QuietHoursEnabled = preferenceDto.QuietHoursEnabled.Value;
                if (preferenceDto.QuietHoursStart != null) preference.QuietHoursStart = preferenceDto.QuietHoursStart;
                if (preferenceDto.QuietHoursEnd != null) preference.QuietHoursEnd = preferenceDto.QuietHoursEnd;
                if (preferenceDto.MaxNotificationsPerDay.HasValue) preference.MaxNotificationsPerDay = preferenceDto.MaxNotificationsPerDay;
                if (preferenceDto.MinMinutesBetweenNotifications.HasValue) preference.MinMinutesBetweenNotifications = preferenceDto.MinMinutesBetweenNotifications;

                preference.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<NotificationPreferenceDto>.SuccessResult(MapPreferenceToDto(preference));
            }
            catch (Exception ex)
            {
                return ApiResponse<NotificationPreferenceDto>.ErrorResult($"Failed to update preference: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeletePreferenceAsync(string userId, string notificationType)
        {
            try
            {
                var preference = await _context.NotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.NotificationType == notificationType);

                if (preference == null)
                {
                    return ApiResponse<bool>.ErrorResult("Preference not found");
                }

                _context.NotificationPreferences.Remove(preference);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete preference: {ex.Message}");
            }
        }

        private NotificationPreferenceDto MapPreferenceToDto(NotificationPreference preference)
        {
            return new NotificationPreferenceDto
            {
                Id = preference.Id,
                UserId = preference.UserId,
                NotificationType = preference.NotificationType,
                InAppEnabled = preference.InAppEnabled,
                EmailEnabled = preference.EmailEnabled,
                SmsEnabled = preference.SmsEnabled,
                PushEnabled = preference.PushEnabled,
                ScheduledEnabled = preference.ScheduledEnabled,
                ScheduleTime = preference.ScheduleTime,
                ScheduleDays = !string.IsNullOrEmpty(preference.ScheduleDays)
                    ? JsonSerializer.Deserialize<List<string>>(preference.ScheduleDays)
                    : null,
                QuietHoursEnabled = preference.QuietHoursEnabled,
                QuietHoursStart = preference.QuietHoursStart,
                QuietHoursEnd = preference.QuietHoursEnd,
                MaxNotificationsPerDay = preference.MaxNotificationsPerDay,
                MinMinutesBetweenNotifications = preference.MinMinutesBetweenNotifications,
                CreatedAt = preference.CreatedAt,
                UpdatedAt = preference.UpdatedAt
            };
        }

        // Notification Templates Methods (continued in next part due to length)
        public async Task<ApiResponse<List<NotificationTemplateDto>>> GetTemplatesAsync(string? notificationType = null, string? channel = null)
        {
            try
            {
                var query = _context.NotificationTemplates.Where(t => t.IsActive);

                if (!string.IsNullOrEmpty(notificationType))
                {
                    query = query.Where(t => t.NotificationType == notificationType);
                }

                if (!string.IsNullOrEmpty(channel))
                {
                    query = query.Where(t => t.Channel == channel);
                }

                var templates = await query.ToListAsync();
                var dtos = templates.Select(t => MapTemplateToDto(t)).ToList();

                return ApiResponse<List<NotificationTemplateDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<NotificationTemplateDto>>.ErrorResult($"Failed to get templates: {ex.Message}");
            }
        }

        public async Task<ApiResponse<NotificationTemplateDto>> GetTemplateAsync(string templateId)
        {
            try
            {
                var template = await _context.NotificationTemplates.FindAsync(templateId);
                if (template == null)
                {
                    return ApiResponse<NotificationTemplateDto>.ErrorResult("Template not found");
                }

                return ApiResponse<NotificationTemplateDto>.SuccessResult(MapTemplateToDto(template));
            }
            catch (Exception ex)
            {
                return ApiResponse<NotificationTemplateDto>.ErrorResult($"Failed to get template: {ex.Message}");
            }
        }

        public async Task<ApiResponse<NotificationTemplateDto>> CreateTemplateAsync(CreateNotificationTemplateDto templateDto)
        {
            try
            {
                var template = new NotificationTemplate
                {
                    Name = templateDto.Name,
                    NotificationType = templateDto.NotificationType,
                    Channel = templateDto.Channel,
                    Subject = templateDto.Subject,
                    Body = templateDto.Body,
                    Description = templateDto.Description,
                    IsActive = true,
                    IsSystemTemplate = false,
                    Variables = templateDto.Variables != null ? JsonSerializer.Serialize(templateDto.Variables) : null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.NotificationTemplates.Add(template);
                await _context.SaveChangesAsync();

                return ApiResponse<NotificationTemplateDto>.SuccessResult(MapTemplateToDto(template));
            }
            catch (Exception ex)
            {
                return ApiResponse<NotificationTemplateDto>.ErrorResult($"Failed to create template: {ex.Message}");
            }
        }

        public async Task<ApiResponse<NotificationTemplateDto>> UpdateTemplateAsync(string templateId, UpdateNotificationTemplateDto templateDto)
        {
            try
            {
                var template = await _context.NotificationTemplates.FindAsync(templateId);
                if (template == null)
                {
                    return ApiResponse<NotificationTemplateDto>.ErrorResult("Template not found");
                }

                if (template.IsSystemTemplate)
                {
                    return ApiResponse<NotificationTemplateDto>.ErrorResult("Cannot update system template");
                }

                if (templateDto.Name != null) template.Name = templateDto.Name;
                if (templateDto.Subject != null) template.Subject = templateDto.Subject;
                if (templateDto.Body != null) template.Body = templateDto.Body;
                if (templateDto.Description != null) template.Description = templateDto.Description;
                if (templateDto.IsActive.HasValue) template.IsActive = templateDto.IsActive.Value;
                if (templateDto.Variables != null) template.Variables = JsonSerializer.Serialize(templateDto.Variables);

                template.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<NotificationTemplateDto>.SuccessResult(MapTemplateToDto(template));
            }
            catch (Exception ex)
            {
                return ApiResponse<NotificationTemplateDto>.ErrorResult($"Failed to update template: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteTemplateAsync(string templateId)
        {
            try
            {
                var template = await _context.NotificationTemplates.FindAsync(templateId);
                if (template == null)
                {
                    return ApiResponse<bool>.ErrorResult("Template not found");
                }

                if (template.IsSystemTemplate)
                {
                    return ApiResponse<bool>.ErrorResult("Cannot delete system template");
                }

                _context.NotificationTemplates.Remove(template);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to delete template: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> RenderTemplateAsync(string templateId, Dictionary<string, string> variables)
        {
            try
            {
                var template = await _context.NotificationTemplates.FindAsync(templateId);
                if (template == null)
                {
                    return ApiResponse<string>.ErrorResult("Template not found");
                }

                var rendered = template.Body;
                foreach (var variable in variables)
                {
                    rendered = rendered.Replace($"{{{{{variable.Key}}}}}", variable.Value);
                }

                return ApiResponse<string>.SuccessResult(rendered);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResult($"Failed to render template: {ex.Message}");
            }
        }

        private NotificationTemplateDto MapTemplateToDto(NotificationTemplate template)
        {
            return new NotificationTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                NotificationType = template.NotificationType,
                Channel = template.Channel,
                Subject = template.Subject,
                Body = template.Body,
                Description = template.Description,
                IsActive = template.IsActive,
                IsSystemTemplate = template.IsSystemTemplate,
                CreatedBy = template.CreatedBy,
                Variables = !string.IsNullOrEmpty(template.Variables)
                    ? JsonSerializer.Deserialize<List<string>>(template.Variables)
                    : null,
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            };
        }

        // Enhanced Send Notification
        public async Task<ApiResponse<NotificationDto>> SendNotificationAsync(SendNotificationRequestDto request)
        {
            try
            {
                // Get or create template
                string? title = request.Title;
                string? message = request.Message;

                if (!string.IsNullOrEmpty(request.TemplateId))
                {
                    var template = await _context.NotificationTemplates.FindAsync(request.TemplateId);
                    if (template != null && request.TemplateVariables != null)
                    {
                        title = template.Subject;
                        message = template.Body;
                        foreach (var variable in request.TemplateVariables)
                        {
                            title = title?.Replace($"{{{{{variable.Key}}}}}", variable.Value);
                            message = message?.Replace($"{{{{{variable.Key}}}}}", variable.Value);
                        }
                    }
                }

                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
                {
                    return ApiResponse<NotificationDto>.ErrorResult("Title and message are required");
                }

                // Create notification for each channel
                Notification? mainNotification = null;
                var results = new List<NotificationDto>();

                foreach (var channel in request.Channels)
                {
                    var notification = new Notification
                    {
                        UserId = request.UserId,
                        Type = request.NotificationType,
                        Title = title,
                        Message = message,
                        Channel = channel,
                        Priority = request.Priority ?? "NORMAL",
                        ScheduledFor = request.ScheduledFor,
                        TemplateId = request.TemplateId,
                        TemplateVariables = request.TemplateVariables != null
                            ? JsonSerializer.Serialize(request.TemplateVariables)
                            : null,
                        Status = request.ScheduledFor.HasValue ? "SCHEDULED" : "PENDING",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);
                    if (mainNotification == null) mainNotification = notification;
                }

                await _context.SaveChangesAsync();

                // Process notifications if not scheduled
                if (!request.ScheduledFor.HasValue && mainNotification != null)
                {
                    await ProcessNotificationAsync(mainNotification);
                }

                return ApiResponse<NotificationDto>.SuccessResult(MapToDto(mainNotification!), "Notification sent successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<NotificationDto>.ErrorResult($"Failed to send notification: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<NotificationDto>>> SendBulkNotificationsAsync(List<SendNotificationRequestDto> requests)
        {
            try
            {
                var results = new List<NotificationDto>();

                foreach (var request in requests)
                {
                    var result = await SendNotificationAsync(request);
                    if (result.Success && result.Data != null)
                    {
                        results.Add(result.Data);
                    }
                }

                return ApiResponse<List<NotificationDto>>.SuccessResult(results);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<NotificationDto>>.ErrorResult($"Failed to send bulk notifications: {ex.Message}");
            }
        }

        // Notification History
        public async Task<ApiResponse<PaginatedResponse<NotificationHistoryDto>>> GetNotificationHistoryAsync(string userId, NotificationHistoryQueryDto query)
        {
            try
            {
                var queryable = _context.NotificationHistories.Where(h => h.UserId == userId);

                if (!string.IsNullOrEmpty(query.NotificationType))
                {
                    queryable = queryable.Where(h => h.NotificationType == query.NotificationType);
                }

                if (!string.IsNullOrEmpty(query.Channel))
                {
                    queryable = queryable.Where(h => h.Channel == query.Channel);
                }

                if (!string.IsNullOrEmpty(query.Status))
                {
                    queryable = queryable.Where(h => h.Status == query.Status);
                }

                if (query.StartDate.HasValue)
                {
                    queryable = queryable.Where(h => h.CreatedAt >= query.StartDate.Value);
                }

                if (query.EndDate.HasValue)
                {
                    queryable = queryable.Where(h => h.CreatedAt <= query.EndDate.Value);
                }

                var totalCount = await queryable.CountAsync();
                var histories = await queryable
                    .OrderByDescending(h => h.CreatedAt)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                var dtos = histories.Select(h => MapHistoryToDto(h)).ToList();

                var paginatedResponse = new PaginatedResponse<NotificationHistoryDto>
                {
                    Data = dtos,
                    Page = query.Page,
                    Limit = query.PageSize,
                    TotalCount = totalCount
                };

                return ApiResponse<PaginatedResponse<NotificationHistoryDto>>.SuccessResult(paginatedResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse<PaginatedResponse<NotificationHistoryDto>>.ErrorResult($"Failed to get history: {ex.Message}");
            }
        }

        public async Task<ApiResponse<NotificationHistoryDto>> GetHistoryItemAsync(string historyId)
        {
            try
            {
                var history = await _context.NotificationHistories.FindAsync(historyId);
                if (history == null)
                {
                    return ApiResponse<NotificationHistoryDto>.ErrorResult("History item not found");
                }

                return ApiResponse<NotificationHistoryDto>.SuccessResult(MapHistoryToDto(history));
            }
            catch (Exception ex)
            {
                return ApiResponse<NotificationHistoryDto>.ErrorResult($"Failed to get history item: {ex.Message}");
            }
        }

        private NotificationHistoryDto MapHistoryToDto(NotificationHistory history)
        {
            return new NotificationHistoryDto
            {
                Id = history.Id,
                UserId = history.UserId,
                NotificationId = history.NotificationId,
                NotificationType = history.NotificationType,
                Channel = history.Channel,
                Subject = history.Subject,
                Message = history.Message,
                Recipient = history.Recipient,
                Status = history.Status,
                ErrorMessage = history.ErrorMessage,
                Provider = history.Provider,
                ExternalId = history.ExternalId,
                SentAt = history.SentAt,
                DeliveredAt = history.DeliveredAt,
                Metadata = !string.IsNullOrEmpty(history.Metadata)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(history.Metadata)
                    : null,
                CreatedAt = history.CreatedAt
            };
        }

        // Scheduled Notifications
        public async Task<ApiResponse<List<NotificationDto>>> GetScheduledNotificationsAsync(string userId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && n.ScheduledFor.HasValue && n.Status == "SCHEDULED")
                    .OrderBy(n => n.ScheduledFor)
                    .ToListAsync();

                var dtos = notifications.Select(n => MapToDto(n)).ToList();
                return ApiResponse<List<NotificationDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<NotificationDto>>.ErrorResult($"Failed to get scheduled notifications: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> CancelScheduledNotificationAsync(string notificationId)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification == null)
                {
                    return ApiResponse<bool>.ErrorResult("Notification not found");
                }

                if (!notification.ScheduledFor.HasValue)
                {
                    return ApiResponse<bool>.ErrorResult("Notification is not scheduled");
                }

                notification.Status = "CANCELLED";
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResult($"Failed to cancel scheduled notification: {ex.Message}");
            }
        }
    }
}

