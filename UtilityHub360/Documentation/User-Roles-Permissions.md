# User Roles & Permissions

## 👥 User Roles Overview

UtilityHub360 implements a role-based access control (RBAC) system with two primary roles:

### 🔹 USER (Default Role)
Standard users who can manage their own loan applications and payments.

### 🔹 ADMIN (Administrative Role)
Administrative users with full system access and management capabilities.

## 🔐 Permission Matrix

| Feature | USER | ADMIN |
|---------|------|-------|
| **Authentication** |
| Register Account | ✅ | ✅ |
| Login | ✅ | ✅ |
| Logout | ✅ | ✅ |
| Refresh Token | ✅ | ✅ |
| **Loan Management** |
| Apply for Loan | ✅ | ✅ |
| View Own Loans | ✅ | ✅ |
| Update Own Loan Info | ✅ | ✅ |
| Update Loan Status | ✅ | ✅ |
| View All Loans | ❌ | ✅ |
| Approve/Reject Loans | ❌ | ✅ |
| Disburse Loans | ❌ | ✅ |
| Close Loans | ❌ | ✅ |
| **User Management** |
| View Own Profile | ✅ | ✅ |
| Update Own Profile | ✅ | ✅ |
| View All Users | ❌ | ✅ |
| **Payments** |
| Make Payments | ✅ | ✅ |
| View Own Payment History | ✅ | ✅ |
| View All Payments | ❌ | ✅ |
| **Notifications** |
| View Own Notifications | ✅ | ✅ |
| Mark Notifications as Read | ✅ | ✅ |
| Send Notifications | ❌ | ✅ |

## 🔒 Detailed Permissions

### USER Role Permissions

#### ✅ Allowed Actions
- **Account Management**
  - Register new account
  - Login/logout
  - Update personal information
  - Change password

- **Loan Operations**
  - Apply for new loans
  - View personal loan applications
  - Update loan purpose and additional info
  - Update loan status (PENDING, APPROVED, etc.)
  - View repayment schedules
  - View transaction history

- **Payment Operations**
  - Make loan payments
  - View payment history
  - Update payment methods

- **Notifications**
  - View personal notifications
  - Mark notifications as read

#### ❌ Restricted Actions
- Cannot access other users' data
- Cannot approve/reject loans
- Cannot disburse funds
- Cannot access admin functions
- Cannot view system-wide reports

### ADMIN Role Permissions

#### ✅ Full System Access
- **User Management**
  - View all user accounts
  - Manage user roles
  - Activate/deactivate accounts
  - Reset user passwords

- **Loan Administration**
  - View all loan applications
  - Approve or reject loan applications
  - Set interest rates and terms
  - Disburse approved loans
  - Close completed loans
  - Update all loan fields including financial data

- **Financial Management**
  - View all transactions
  - Process payments
  - Generate financial reports
  - Manage repayment schedules

- **System Administration**
  - Send system notifications
  - Access admin dashboard
  - Generate reports
  - System configuration

## 🛡️ Implementation Details

### Role Assignment
```csharp
// Default role assignment during registration
public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDataDto registerData)
{
    var user = new User
    {
        Id = Guid.NewGuid().ToString(),
        Name = registerData.Name,
        Email = registerData.Email,
        Phone = registerData.Phone,
        Role = "USER", // Default role
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
```

### Role-based Authorization
```csharp
// Controller-level authorization
[Authorize(Roles = "ADMIN")]
public class AdminController : ControllerBase
{
    // Admin-only endpoints
}

// Action-level authorization
[HttpGet]
[Authorize(Roles = "ADMIN")]
public async Task<ActionResult> GetAllLoans()
{
    // Admin can see all loans
}
```

### Dynamic Role Checking
```csharp
// Runtime role checking
var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

if (userRole == "ADMIN")
{
    // Admin logic - can access any loan
    return await _context.Loans.FindAsync(loanId);
}
else
{
    // User logic - can only access own loans
    return await _context.Loans
        .FirstOrDefaultAsync(l => l.Id == loanId && l.UserId == userId);
}
```

## 🔄 Permission Inheritance

### USER Permissions
- Inherits all basic authentication permissions
- Can perform CRUD operations on own data
- Cannot access other users' data
- Cannot perform administrative actions

### ADMIN Permissions
- Inherits all USER permissions
- Additional access to all system data
- Administrative and management capabilities
- System configuration access

## 🚨 Security Considerations

### Data Isolation
- Users can only access their own data
- Database queries filtered by user ID
- Cross-user data access prevented

### Role Validation
- JWT tokens contain role information
- Server-side role validation
- Database role fallback for security

### Audit Trail
- All administrative actions logged
- User action tracking
- Security event monitoring

## 📝 Role Management

### Creating Admin Users
```sql
-- Update user role to admin
UPDATE Users 
SET Role = 'ADMIN' 
WHERE Email = 'admin@example.com';
```

### Role Verification
```csharp
// Check if user is admin
public bool IsAdmin(string userId)
{
    var user = _context.Users.Find(userId);
    return user?.Role == "ADMIN";
}
```

### Role-based UI
```javascript
// Frontend role checking
const userRole = getCurrentUserRole();

if (userRole === 'ADMIN') {
    showAdminPanel();
    showAllUsersData();
} else {
    showUserPanel();
    showOwnDataOnly();
}
```

## 🔧 Customization

### Adding New Roles
1. Update enum in `Models/Enums.cs`
2. Add role checks in controllers
3. Update database seed data
4. Modify frontend role handling

### Custom Permissions
```csharp
// Custom permission attribute
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
    {
        Policy = permission;
    }
}

// Usage
[RequirePermission("LOAN_APPROVAL")]
public async Task<ActionResult> ApproveLoan()
{
    // Custom permission logic
}
```

## 🧪 Testing Roles

### Unit Testing
```csharp
[Test]
public void User_CanOnlyAccessOwnLoans()
{
    // Arrange
    var user = CreateTestUser("USER");
    var otherUserLoan = CreateTestLoan("OTHER_USER");
    
    // Act & Assert
    Assert.Throws<UnauthorizedAccessException>(() => 
        _loanService.GetLoan(otherUserLoan.Id, user.Id));
}
```

### Integration Testing
```csharp
[Test]
public async Task Admin_CanAccessAllLoans()
{
    // Arrange
    var admin = CreateTestUser("ADMIN");
    var token = GenerateJwtToken(admin);
    
    // Act
    var response = await _client.GetAsync("/api/loans")
        .WithBearerToken(token);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## 📊 Role Analytics

### User Role Distribution
```sql
SELECT Role, COUNT(*) as UserCount
FROM Users
WHERE IsActive = 1
GROUP BY Role;
```

### Admin Activity Tracking
```sql
SELECT u.Name, u.Email, COUNT(*) as AdminActions
FROM AdminActions a
JOIN Users u ON a.AdminId = u.Id
WHERE a.CreatedAt >= DATEADD(day, -30, GETDATE())
GROUP BY u.Name, u.Email;
```

This role-based system ensures secure access control while providing flexibility for different user types and their specific needs.
