using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UtilityHub360.Data;
using UtilityHub360.DTOs;
using UtilityHub360.Models;
using BCrypt.Net;

namespace UtilityHub360.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSettings;
        private readonly IEmailService _emailService;

        public AuthService(ApplicationDbContext context, IConfiguration configuration, IEmailService emailService, JwtSettings jwtSettings)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _jwtSettings = jwtSettings;
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDataDto registerData)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == registerData.Email))
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            // Only check for phone number uniqueness if phone number is provided
            if (!string.IsNullOrEmpty(registerData.Phone) && 
                await _context.Users.AnyAsync(u => u.Phone == registerData.Phone))
            {
                throw new InvalidOperationException("User with this phone number already exists");
            }

            // Create new user
            var user = new Entities.User
            {
                Name = registerData.Name,
                Email = registerData.Email,
                Phone = registerData.Phone,
                Role = "USER",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerData.Password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate tokens
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            return ApiResponse<AuthResponseDto>.SuccessResult(new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                }
            });
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginCredentialsDto loginCredentials)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginCredentials.Email && u.IsActive);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Verify the password
            if (!BCrypt.Net.BCrypt.Verify(loginCredentials.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            return ApiResponse<AuthResponseDto>.SuccessResult(new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                }
            });
        }

        public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            // In a real application, you would validate the refresh token
            // For now, we'll generate a new token
            throw new NotImplementedException("Refresh token functionality not implemented yet");
        }

        public async Task<UserDto> GetCurrentUserAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync(string userId)
        {
            // In a real application, you would invalidate the refresh token
            // For now, this is a placeholder
            await Task.CompletedTask;
        }

        private string GenerateJwtToken(Entities.User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey); 

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }

        public async Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                // Check if user exists
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email && u.IsActive);

                if (user == null)
                {
                    // For security, don't reveal if email exists or not
                    return ApiResponse<object>.SuccessResult(new { }, "If the email exists, a password reset link has been sent.");
                }

                // Generate reset token
                var resetToken = Guid.NewGuid().ToString();
                var expiresAt = DateTime.UtcNow.AddHours(1); // Token expires in 1 hour

                // Invalidate any existing reset tokens for this user
                var existingResets = await _context.PasswordResets
                    .Where(pr => pr.UserId == user.Id && !pr.IsUsed)
                    .ToListAsync();

                foreach (var existingReset in existingResets)
                {
                    existingReset.IsUsed = true;
                    existingReset.UsedAt = DateTime.UtcNow;
                }

                // Create new password reset record
                var passwordReset = new Entities.PasswordReset
                {
                    UserId = user.Id,
                    Token = resetToken,
                    Email = user.Email,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    IsUsed = false
                };

                _context.PasswordResets.Add(passwordReset);
                await _context.SaveChangesAsync();

                // Send email
                await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken, user.Name);

                return ApiResponse<object>.SuccessResult(new { }, "If the email exists, a password reset link has been sent.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to process password reset request: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                // Find the password reset record
                var passwordReset = await _context.PasswordResets
                    .FirstOrDefaultAsync(pr => pr.Token == resetPasswordDto.Token && 
                                             pr.Email == resetPasswordDto.Email && 
                                             !pr.IsUsed && 
                                             pr.ExpiresAt > DateTime.UtcNow);

                if (passwordReset == null)
                {
                    throw new InvalidOperationException("Invalid or expired reset token.");
                }

                // Find the user
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == passwordReset.UserId && u.IsActive);

                if (user == null)
                {
                    throw new InvalidOperationException("User not found.");
                }

                // Update user password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                // Mark reset token as used
                passwordReset.IsUsed = true;
                passwordReset.UsedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<object>.SuccessResult(new { }, "Password has been reset successfully.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to reset password: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> ChangePasswordAsync(ChangePasswordDto changePasswordDto, string userId)
        {
            try
            {
                // Get the user
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return ApiResponse<object>.ErrorResult("User not found");
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
                {
                    return ApiResponse<object>.ErrorResult("Current password is incorrect");
                }

                // Check if new password is different from current password
                if (BCrypt.Net.BCrypt.Verify(changePasswordDto.NewPassword, user.PasswordHash))
                {
                    return ApiResponse<object>.ErrorResult("New password must be different from current password");
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<object>.SuccessResult(new { }, "Password has been changed successfully.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to change password: {ex.Message}");
            }
        }

        public async Task<ApiResponse<object>> ClearAllUserDataAsync(ClearAllDataDto clearAllDataDto, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get the user
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return ApiResponse<object>.ErrorResult("User not found");
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(clearAllDataDto.Password, user.PasswordHash))
                {
                    return ApiResponse<object>.ErrorResult("Password is incorrect");
                }

                // Verify agreement
                if (!clearAllDataDto.AgreementConfirmed)
                {
                    return ApiResponse<object>.ErrorResult("You must confirm the agreement to delete all your data");
                }

                // Track deletion counts
                var deletionSummary = new Dictionary<string, int>();

                var category = clearAllDataDto.Category;

                // Delete based on selected category
                switch (category)
                {
                    case DeleteCategory.PaymentsAndTransactions:
                        await DeletePaymentsAndTransactionsAsync(userId, deletionSummary);
                        break;

                    case DeleteCategory.BillsAndUtility:
                        await DeleteBillsAndUtilityAsync(userId, deletionSummary);
                        break;

                    case DeleteCategory.Loan:
                        await DeleteLoansAsync(userId, deletionSummary);
                        break;

                    case DeleteCategory.Savings:
                        await DeleteSavingsAsync(userId, deletionSummary);
                        break;

                    case DeleteCategory.BankAccount:
                        await DeleteBankAccountsAsync(userId, deletionSummary);
                        break;

                    case DeleteCategory.All:
                    default:
                        // Delete everything (existing logic)
                        await DeleteAllDataAsync(userId, deletionSummary);
                        break;
                }

                // Save all changes
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Calculate total records deleted
                var totalRecords = deletionSummary.Values.Sum();

                var categoryName = category == DeleteCategory.All ? "all data" : $"selected category ({category})";
                return ApiResponse<object>.SuccessResult(
                    new
                    {
                        message = $"User {categoryName} has been cleared successfully",
                        deletedRecords = deletionSummary,
                        totalRecordsDeleted = totalRecords,
                        category = category.ToString()
                    },
                    $"Successfully deleted {totalRecords} records across {deletionSummary.Count} categories."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"Failed to clear user data: {ex.Message}");
            }
        }

        // Helper methods for category-specific deletions
        private async Task DeletePaymentsAndTransactionsAsync(string userId, Dictionary<string, int> deletionSummary)
        {
            // 1. Payments
            var payments = await _context.Payments
                .Where(p => p.UserId == userId)
                .ToListAsync();
            if (payments.Any())
            {
                _context.Payments.RemoveRange(payments);
                deletionSummary["payments"] = payments.Count;
            }

            // 2. BankTransactions
            var bankTransactions = await _context.BankTransactions
                .Where(bt => bt.UserId == userId)
                .ToListAsync();
            if (bankTransactions.Any())
            {
                _context.BankTransactions.RemoveRange(bankTransactions);
                deletionSummary["bankTransactions"] = bankTransactions.Count;
            }
        }

        private async Task DeleteBillsAndUtilityAsync(string userId, Dictionary<string, int> deletionSummary)
        {
            // 1. Payments related to bills
            var billIds = await _context.Bills
                .Where(b => b.UserId == userId)
                .Select(b => b.Id)
                .ToListAsync();
            
            var billPayments = await _context.Payments
                .Where(p => p.BillId != null && billIds.Contains(p.BillId))
                .ToListAsync();
            if (billPayments.Any())
            {
                _context.Payments.RemoveRange(billPayments);
                deletionSummary["billPayments"] = billPayments.Count;
            }

            // 2. Bills
            var bills = await _context.Bills
                .Where(b => b.UserId == userId)
                .ToListAsync();
            if (bills.Any())
            {
                _context.Bills.RemoveRange(bills);
                deletionSummary["bills"] = bills.Count;
            }

            // 3. IncomeSource (utilities)
            var incomeSources = await _context.IncomeSources
                .Where(i => i.UserId == userId)
                .ToListAsync();
            if (incomeSources.Any())
            {
                _context.IncomeSources.RemoveRange(incomeSources);
                deletionSummary["incomeSources"] = incomeSources.Count;
            }

            // 4. VariableExpense (utilities)
            var variableExpenses = await _context.VariableExpenses
                .Where(v => v.UserId == userId)
                .ToListAsync();
            if (variableExpenses.Any())
            {
                _context.VariableExpenses.RemoveRange(variableExpenses);
                deletionSummary["variableExpenses"] = variableExpenses.Count;
            }
        }

        private async Task DeleteLoansAsync(string userId, Dictionary<string, int> deletionSummary)
        {
            // 1. Get loan IDs first
            var loanIds = await _context.Loans
                .Where(l => l.UserId == userId)
                .Select(l => l.Id)
                .ToListAsync();

            // 2. Payments related to loans
            var loanPayments = await _context.Payments
                .Where(p => p.LoanId != null && loanIds.Contains(p.LoanId))
                .ToListAsync();
            if (loanPayments.Any())
            {
                _context.Payments.RemoveRange(loanPayments);
                deletionSummary["loanPayments"] = loanPayments.Count;
            }

            // 3. RepaymentSchedules
            var repaymentSchedules = await _context.RepaymentSchedules
                .Where(rs => loanIds.Contains(rs.LoanId))
                .ToListAsync();
            if (repaymentSchedules.Any())
            {
                _context.RepaymentSchedules.RemoveRange(repaymentSchedules);
                deletionSummary["repaymentSchedules"] = repaymentSchedules.Count;
            }

            // 4. Loans
            var loans = await _context.Loans
                .Where(l => l.UserId == userId)
                .ToListAsync();
            if (loans.Any())
            {
                _context.Loans.RemoveRange(loans);
                deletionSummary["loans"] = loans.Count;
            }

            // 5. LoanApplications
            var loanApplications = await _context.LoanApplications
                .Where(la => la.UserId == userId)
                .ToListAsync();
            if (loanApplications.Any())
            {
                _context.LoanApplications.RemoveRange(loanApplications);
                deletionSummary["loanApplications"] = loanApplications.Count;
            }
        }

        private async Task DeleteSavingsAsync(string userId, Dictionary<string, int> deletionSummary)
        {
            // 1. Get savings account IDs first
            var savingsAccountIds = await _context.SavingsAccounts
                .Where(sa => sa.UserId == userId)
                .Select(sa => sa.Id)
                .ToListAsync();
            
            // 2. Payments related to savings
            var savingsPayments = await _context.Payments
                .Where(p => p.SavingsAccountId != null && savingsAccountIds.Contains(p.SavingsAccountId))
                .ToListAsync();
            if (savingsPayments.Any())
            {
                _context.Payments.RemoveRange(savingsPayments);
                deletionSummary["savingsPayments"] = savingsPayments.Count;
            }

            // 3. SavingsTransactions
            var savingsTransactions = await _context.SavingsTransactions
                .Where(st => savingsAccountIds.Contains(st.SavingsAccountId))
                .ToListAsync();
            if (savingsTransactions.Any())
            {
                _context.SavingsTransactions.RemoveRange(savingsTransactions);
                deletionSummary["savingsTransactions"] = savingsTransactions.Count;
            }

            // 4. SavingsAccounts
            var savingsAccounts = await _context.SavingsAccounts
                .Where(sa => sa.UserId == userId)
                .ToListAsync();
            if (savingsAccounts.Any())
            {
                _context.SavingsAccounts.RemoveRange(savingsAccounts);
                deletionSummary["savingsAccounts"] = savingsAccounts.Count;
            }
        }

        private async Task DeleteBankAccountsAsync(string userId, Dictionary<string, int> deletionSummary)
        {
            // 1. Get bank account IDs first
            var bankAccountIds = await _context.BankAccounts
                .Where(ba => ba.UserId == userId)
                .Select(ba => ba.Id)
                .ToListAsync();
            
            // 2. Payments related to bank accounts
            var bankAccountPayments = await _context.Payments
                .Where(p => p.BankAccountId != null && bankAccountIds.Contains(p.BankAccountId))
                .ToListAsync();
            if (bankAccountPayments.Any())
            {
                _context.Payments.RemoveRange(bankAccountPayments);
                deletionSummary["bankAccountPayments"] = bankAccountPayments.Count;
            }

            // 3. BankTransactions
            var bankTransactions = await _context.BankTransactions
                .Where(bt => bt.UserId == userId)
                .ToListAsync();
            if (bankTransactions.Any())
            {
                _context.BankTransactions.RemoveRange(bankTransactions);
                deletionSummary["bankTransactions"] = bankTransactions.Count;
            }

            // 4. BankAccounts
            var bankAccounts = await _context.BankAccounts
                .Where(ba => ba.UserId == userId)
                .ToListAsync();
            if (bankAccounts.Any())
            {
                _context.BankAccounts.RemoveRange(bankAccounts);
                deletionSummary["bankAccounts"] = bankAccounts.Count;
            }
        }

        private async Task DeleteAllDataAsync(string userId, Dictionary<string, int> deletionSummary)
        {
            // Delete in order to respect foreign key constraints
                // 1. Payments (may reference loans, bills, savings accounts, bank accounts)
                var payments = await _context.Payments
                    .Where(p => p.UserId == userId)
                    .ToListAsync();
                if (payments.Any())
                {
                    _context.Payments.RemoveRange(payments);
                    deletionSummary["payments"] = payments.Count;
                }

                // 2. RepaymentSchedules (references loans)
                var loanIds = await _context.Loans
                    .Where(l => l.UserId == userId)
                    .Select(l => l.Id)
                    .ToListAsync();
                var repaymentSchedules = await _context.RepaymentSchedules
                    .Where(rs => loanIds.Contains(rs.LoanId))
                    .ToListAsync();
                if (repaymentSchedules.Any())
                {
                    _context.RepaymentSchedules.RemoveRange(repaymentSchedules);
                    deletionSummary["repaymentSchedules"] = repaymentSchedules.Count;
                }

                // 3. Loans (cascade will handle repayment schedules, but we already deleted them)
                var loans = await _context.Loans
                    .Where(l => l.UserId == userId)
                    .ToListAsync();
                if (loans.Any())
                {
                    _context.Loans.RemoveRange(loans);
                    deletionSummary["loans"] = loans.Count;
                }

                // 4. LoanApplications
                var loanApplications = await _context.LoanApplications
                    .Where(la => la.UserId == userId)
                    .ToListAsync();
                if (loanApplications.Any())
                {
                    _context.LoanApplications.RemoveRange(loanApplications);
                    deletionSummary["loanApplications"] = loanApplications.Count;
                }

                // 5. BankTransactions (references bank accounts)
                var bankTransactions = await _context.BankTransactions
                    .Where(bt => bt.UserId == userId)
                    .ToListAsync();
                if (bankTransactions.Any())
                {
                    _context.BankTransactions.RemoveRange(bankTransactions);
                    deletionSummary["bankTransactions"] = bankTransactions.Count;
                }

                // 6. BankAccounts
                var bankAccounts = await _context.BankAccounts
                    .Where(ba => ba.UserId == userId)
                    .ToListAsync();
                if (bankAccounts.Any())
                {
                    _context.BankAccounts.RemoveRange(bankAccounts);
                    deletionSummary["bankAccounts"] = bankAccounts.Count;
                }

                // 7. SavingsTransactions (references savings accounts)
                var savingsAccountIds = await _context.SavingsAccounts
                    .Where(sa => sa.UserId == userId)
                    .Select(sa => sa.Id)
                    .ToListAsync();
                var savingsTransactions = await _context.SavingsTransactions
                    .Where(st => savingsAccountIds.Contains(st.SavingsAccountId))
                    .ToListAsync();
                if (savingsTransactions.Any())
                {
                    _context.SavingsTransactions.RemoveRange(savingsTransactions);
                    deletionSummary["savingsTransactions"] = savingsTransactions.Count;
                }

                // 8. SavingsAccounts
                var savingsAccounts = await _context.SavingsAccounts
                    .Where(sa => sa.UserId == userId)
                    .ToListAsync();
                if (savingsAccounts.Any())
                {
                    _context.SavingsAccounts.RemoveRange(savingsAccounts);
                    deletionSummary["savingsAccounts"] = savingsAccounts.Count;
                }

                // 9. Bills
                var bills = await _context.Bills
                    .Where(b => b.UserId == userId)
                    .ToListAsync();
                if (bills.Any())
                {
                    _context.Bills.RemoveRange(bills);
                    deletionSummary["bills"] = bills.Count;
                }

                // 10. IncomeSource
                var incomeSources = await _context.IncomeSources
                    .Where(i => i.UserId == userId)
                    .ToListAsync();
                if (incomeSources.Any())
                {
                    _context.IncomeSources.RemoveRange(incomeSources);
                    deletionSummary["incomeSources"] = incomeSources.Count;
                }

                // 11. VariableExpense
                var variableExpenses = await _context.VariableExpenses
                    .Where(v => v.UserId == userId)
                    .ToListAsync();
                if (variableExpenses.Any())
                {
                    _context.VariableExpenses.RemoveRange(variableExpenses);
                    deletionSummary["variableExpenses"] = variableExpenses.Count;
                }

                // 12. Notifications
                var notifications = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .ToListAsync();
                if (notifications.Any())
                {
                    _context.Notifications.RemoveRange(notifications);
                    deletionSummary["notifications"] = notifications.Count;
                }

                // 13. UserProfile
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(up => up.UserId == userId);
                if (userProfile != null)
                {
                    _context.UserProfiles.Remove(userProfile);
                    deletionSummary["userProfile"] = 1;
                }

                // 14. UserOnboarding (skip if table doesn't exist)
                try
                {
                    // Use raw SQL to check and delete to avoid EF Core model validation issues
                    var result = await _context.Database.ExecuteSqlRawAsync(@"
                        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserOnboardings')
                        BEGIN
                            DELETE FROM UserOnboardings WHERE UserId = {0}
                        END", userId);
                    
                    if (result > 0)
                    {
                        deletionSummary["userOnboarding"] = 1;
                    }
                }
                catch (Microsoft.Data.SqlClient.SqlException)
                {
                    // Table doesn't exist or SQL error, skip
                }
                catch (Exception)
                {
                    // Any other error, skip
                }

                // 15. PasswordReset
                var passwordResets = await _context.PasswordResets
                    .Where(pr => pr.UserId == userId)
                    .ToListAsync();
                if (passwordResets.Any())
                {
                    _context.PasswordResets.RemoveRange(passwordResets);
                    deletionSummary["passwordResets"] = passwordResets.Count;
                }

                // 16. BudgetSetting (skip if table doesn't exist)
                try
                {
                    var budgetSettings = await _context.BudgetSettings
                        .Where(bs => bs.UserId == userId)
                        .ToListAsync();
                    if (budgetSettings.Any())
                    {
                        _context.BudgetSettings.RemoveRange(budgetSettings);
                        deletionSummary["budgetSettings"] = budgetSettings.Count;
                    }
                }
                catch (Exception)
                {
                    // Table doesn't exist, skip
                }

                // 17. BillAnalyticsCache (skip if table doesn't exist)
                try
                {
                    var billAnalyticsCaches = await _context.BillAnalyticsCaches
                        .Where(bac => bac.UserId == userId)
                        .ToListAsync();
                    if (billAnalyticsCaches.Any())
                    {
                        _context.BillAnalyticsCaches.RemoveRange(billAnalyticsCaches);
                        deletionSummary["billAnalyticsCaches"] = billAnalyticsCaches.Count;
                    }
                }
                catch (Exception)
                {
                    // Table doesn't exist, skip
                }

                // 18. BillAlert (skip if table doesn't exist)
                try
                {
                    var billAlerts = await _context.BillAlerts
                        .Where(ba => ba.UserId == userId)
                        .ToListAsync();
                    if (billAlerts.Any())
                    {
                        _context.BillAlerts.RemoveRange(billAlerts);
                        deletionSummary["billAlerts"] = billAlerts.Count;
                    }
                }
                catch (Exception)
                {
                    // Table doesn't exist, skip
                }

                // 19. ChatMessages (references chat conversations) (skip if table doesn't exist)
                try
                {
                    var conversationIds = await _context.ChatConversations
                        .Where(cc => cc.UserId == userId)
                        .Select(cc => cc.Id)
                        .ToListAsync();
                    var chatMessages = await _context.ChatMessages
                        .Where(cm => cm.UserId == userId || conversationIds.Contains(cm.ConversationId))
                        .ToListAsync();
                    if (chatMessages.Any())
                    {
                        _context.ChatMessages.RemoveRange(chatMessages);
                        deletionSummary["chatMessages"] = chatMessages.Count;
                    }
                }
                catch (Exception)
                {
                    // Table doesn't exist, skip
                }

                // 20. ChatConversation (skip if table doesn't exist)
                try
                {
                    var chatConversations = await _context.ChatConversations
                        .Where(cc => cc.UserId == userId)
                        .ToListAsync();
                    if (chatConversations.Any())
                    {
                        _context.ChatConversations.RemoveRange(chatConversations);
                        deletionSummary["chatConversations"] = chatConversations.Count;
                    }
                }
                catch (Exception)
                {
                    // Table doesn't exist, skip
                }

                // 21. JournalEntryLines (references journal entries) (skip if table doesn't exist)
                try
                {
                    var journalEntryIds = await _context.JournalEntries
                        .Where(je => je.UserId == userId)
                        .Select(je => je.Id)
                        .ToListAsync();
                    var journalEntryLines = await _context.JournalEntryLines
                        .Where(jel => journalEntryIds.Contains(jel.JournalEntryId))
                        .ToListAsync();
                    if (journalEntryLines.Any())
                    {
                        _context.JournalEntryLines.RemoveRange(journalEntryLines);
                        deletionSummary["journalEntryLines"] = journalEntryLines.Count;
                    }
                }
                catch (Exception)
                {
                    // Table doesn't exist, skip
                }

                // 22. JournalEntry (skip if table doesn't exist)
                try
                {
                    var journalEntries = await _context.JournalEntries
                        .Where(je => je.UserId == userId)
                        .ToListAsync();
                    if (journalEntries.Any())
                    {
                        _context.JournalEntries.RemoveRange(journalEntries);
                        deletionSummary["journalEntries"] = journalEntries.Count;
                    }
                }
                catch (Exception)
                {
                    // Table doesn't exist, skip
                }
        }
    }
}
