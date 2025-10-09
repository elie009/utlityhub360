# Authentication Guide

## 🔐 JWT Authentication System

UtilityHub360 uses JSON Web Tokens (JWT) for secure authentication and authorization.

## 🏗️ Authentication Flow

### 1. User Registration
```
POST /api/auth/register
```
- Creates new user account
- Returns JWT token and refresh token
- User role defaults to "USER"

### 2. User Login
```
POST /api/auth/login
```
- Validates credentials
- Returns JWT token and refresh token
- Token expires in 60 minutes (configurable)

### 3. API Requests
```
Authorization: Bearer <jwt_token>
```
- Include token in Authorization header
- Token contains user ID and role information

### 4. Token Refresh
```
POST /api/auth/refresh
```
- Use refresh token to get new JWT token
- Refresh tokens have longer expiration

## 🔑 JWT Token Structure

### Header
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

### Payload
```json
{
  "sub": "user-id-here",
  "name": "John Doe",
  "email": "john@example.com",
  "role": "USER",
  "iat": 1695123456,
  "exp": 1695127056
}
```

### Claims
- `sub` (Subject): User ID
- `name`: User's full name
- `email`: User's email address
- `role`: User role (USER, ADMIN)
- `iat`: Issued at timestamp
- `exp`: Expiration timestamp

## 👥 User Roles

### USER
- Default role for registered users
- Can manage own loans and payments
- Can update own loan information
- Cannot access admin endpoints

### ADMIN
- Can access all endpoints
- Can manage all users' loans
- Can update financial details (interest rate, payments, balance)
- Can approve/reject loans
- Can disburse and close loans

## 🔒 Authorization Examples

### Regular User Access
```csharp
[Authorize] // Any authenticated user
public async Task<ActionResult> GetMyLoans()
```

### Admin Only Access
```csharp
[Authorize(Roles = "ADMIN")] // Admin users only
public async Task<ActionResult> GetAllLoans()
```

### Role-based Logic in Controllers
```csharp
var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

if (userRole == "ADMIN")
{
    // Admin can access any loan
}
else
{
    // Regular users can only access their own loans
}
```

## ⚙️ JWT Configuration

### appsettings.json
```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-here-must-be-long-enough",
    "Issuer": "UtilityHub360",
    "Audience": "UtilityHub360-Users",
    "ExpirationMinutes": 60
  }
}
```

### Security Requirements
- Secret key must be at least 256 bits (32 characters)
- Store secret key securely in production
- Use different keys for different environments

## 🛡️ Security Best Practices

### Token Security
- Tokens are sent via HTTPS only
- Tokens expire automatically
- Refresh tokens for seamless experience
- Secure token storage on client side

### Password Security
- Passwords are hashed using BCrypt
- Minimum password requirements enforced
- Password reset functionality available

### API Security
- All endpoints require authentication except register/login
- Role-based authorization implemented
- Input validation on all endpoints
- SQL injection protection via Entity Framework

## 🔄 Token Lifecycle

### Access Token
- **Lifetime**: 60 minutes
- **Usage**: API requests
- **Storage**: Client-side (localStorage, sessionStorage, or secure storage)

### Refresh Token
- **Lifetime**: 7 days (configurable)
- **Usage**: Getting new access tokens
- **Storage**: Secure client-side storage

### Token Refresh Flow
1. Client makes API request with access token
2. If token expired, client uses refresh token
3. Server validates refresh token
4. Server issues new access token
5. Client retries original request

## 🧪 Testing Authentication

### Using Swagger UI
1. Navigate to `/swagger`
2. Click "Authorize" button
3. Enter: `Bearer <your-jwt-token>`
4. Test authenticated endpoints

### Using Postman
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Using curl
```bash
curl -H "Authorization: Bearer <token>" \
     -H "Content-Type: application/json" \
     https://localhost:5001/api/loans
```

## 🚨 Common Issues

### Token Expired
- **Error**: 401 Unauthorized
- **Solution**: Use refresh token to get new access token

### Invalid Token
- **Error**: 401 Unauthorized
- **Solution**: Re-login to get new token

### Insufficient Permissions
- **Error**: 403 Forbidden
- **Solution**: Check user role and endpoint requirements

### Missing Authorization Header
- **Error**: 401 Unauthorized
- **Solution**: Include "Authorization: Bearer <token>" header

## 📝 Implementation Notes

- JWT tokens are stateless (no server-side storage)
- User role is checked from both JWT claims and database
- Fallback to database role if JWT role is missing
- All sensitive operations require fresh authentication
