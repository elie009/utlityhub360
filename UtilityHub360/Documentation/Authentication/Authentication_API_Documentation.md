# 🔐 Authentication API Documentation

## Overview

This document provides comprehensive documentation for user authentication endpoints including registration, signin (login), and password recovery (forgot/reset password).

**Base URL:** `http://localhost:5000/api/auth`

**Authentication:** Most endpoints do NOT require authentication (except `/me` and `/logout`)

**Content-Type:** `application/json`

---

## 📋 Table of Contents

1. [User Registration](#1-user-registration)
2. [User Signin (Login)](#2-user-signin-login)
3. [Forgot Password](#3-forgot-password)
4. [Reset Password](#4-reset-password)
5. [Get Current User](#5-get-current-user)
6. [Refresh Token](#6-refresh-token)
7. [Logout](#7-logout)
8. [Error Handling](#error-handling)
9. [Frontend Implementation Examples](#frontend-implementation-examples)

---

## 1. User Registration

### Endpoint
```
POST /api/auth/register
```

### Description
Register a new user account. Upon successful registration, the user will receive a JWT token and refresh token for immediate authentication.

### Authentication
Not required

### Request Body

```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "+1234567890",
  "password": "password123",
  "confirmPassword": "password123"
}
```

### Field Validation Rules

| Field | Type | Required | Validation Rules |
|-------|------|----------|------------------|
| `name` | string | ✅ Yes | 2-100 characters |
| `email` | string | ✅ Yes | Valid email format |
| `phone` | string | ❌ No | Valid phone format (optional) |
| `password` | string | ✅ Yes | Minimum 6 characters |
| `confirmPassword` | string | ✅ Yes | Must match `password` |

### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyLWlkIiwibmFtZSI6IkpvaG4gRG9lIiwiZW1haWwiOiJqb2huQGV4YW1wbGUuY29tIiwicm9sZSI6IlVTRVIiLCJpYXQiOjE3MDEyMzQ1NjcsImV4cCI6MTcwMTIzODE2N30.abc123...",
    "refreshToken": "guid-refresh-token-here",
    "expiresAt": "2024-01-01T12:00:00Z",
    "user": {
      "id": "user-id-here",
      "name": "John Doe",
      "email": "john@example.com",
      "phone": "+1234567890",
      "role": "USER",
      "isActive": true,
      "createdAt": "2024-01-01T10:00:00Z",
      "updatedAt": "2024-01-01T10:00:00Z"
    }
  }
}
```

### Error Responses

#### 400 Bad Request - Validation Failed
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "The Password field is required.",
    "The ConfirmPassword field does not match the Password field."
  ]
}
```

#### 400 Bad Request - User Already Exists
```json
{
  "success": false,
  "message": "Registration failed: User with this email already exists"
}
```

#### 400 Bad Request - Phone Number Already Exists
```json
{
  "success": false,
  "message": "Registration failed: User with this phone number already exists"
}
```

### Example Request (cURL)

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john@example.com",
    "phone": "+1234567890",
    "password": "password123",
    "confirmPassword": "password123"
  }'
```

### Example Request (JavaScript/Fetch)

```javascript
const registerUser = async (userData) => {
  try {
    const response = await fetch('http://localhost:5000/api/auth/register', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        name: userData.name,
        email: userData.email,
        phone: userData.phone,
        password: userData.password,
        confirmPassword: userData.confirmPassword
      })
    });

    const result = await response.json();
    
    if (result.success) {
      // Store tokens
      localStorage.setItem('token', result.data.token);
      localStorage.setItem('refreshToken', result.data.refreshToken);
      return result.data;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Registration failed:', error);
    throw error;
  }
};
```

---

## 2. User Signin (Login)

### Endpoint
```
POST /api/auth/login
```

### Description
Authenticate an existing user and receive JWT token and refresh token for accessing protected endpoints.

### Authentication
Not required

### Request Body

```json
{
  "email": "john@example.com",
  "password": "password123"
}
```

### Field Validation Rules

| Field | Type | Required | Validation Rules |
|-------|------|----------|------------------|
| `email` | string | ✅ Yes | Valid email format |
| `password` | string | ✅ Yes | Minimum 6 characters |

### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyLWlkIiwibmFtZSI6IkpvaG4gRG9lIiwiZW1haWwiOiJqb2huQGV4YW1wbGUuY29tIiwicm9sZSI6IlVTRVIiLCJpYXQiOjE3MDEyMzQ1NjcsImV4cCI6MTcwMTIzODE2N30.abc123...",
    "refreshToken": "guid-refresh-token-here",
    "expiresAt": "2024-01-01T12:00:00Z",
    "user": {
      "id": "user-id-here",
      "name": "John Doe",
      "email": "john@example.com",
      "phone": "+1234567890",
      "role": "USER",
      "isActive": true,
      "createdAt": "2024-01-01T10:00:00Z",
      "updatedAt": "2024-01-01T10:00:00Z"
    }
  }
}
```

### Error Responses

#### 400 Bad Request - Validation Failed
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "The Email field is required.",
    "The Password field is required."
  ]
}
```

#### 401 Unauthorized - Invalid Credentials
```json
{
  "success": false,
  "message": "Login failed: Invalid email or password"
}
```

#### 401 Unauthorized - User Not Active
```json
{
  "success": false,
  "message": "Login failed: Invalid email or password"
}
```

### Example Request (cURL)

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "password123"
  }'
```

### Example Request (JavaScript/Fetch)

```javascript
const loginUser = async (email, password) => {
  try {
    const response = await fetch('http://localhost:5000/api/auth/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        email: email,
        password: password
      })
    });

    const result = await response.json();
    
    if (result.success) {
      // Store tokens
      localStorage.setItem('token', result.data.token);
      localStorage.setItem('refreshToken', result.data.refreshToken);
      return result.data;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Login failed:', error);
    throw error;
  }
};
```

---

## 3. Forgot Password

### Endpoint
```
POST /api/auth/forgot-password
```

### Description
Request a password reset link to be sent to the user's email address. This endpoint will send an email with a password reset token if the email exists in the system.

**Security Note:** The API returns the same success message regardless of whether the email exists to prevent email enumeration attacks.

### Authentication
Not required

### Request Body

```json
{
  "email": "john@example.com"
}
```

### Field Validation Rules

| Field | Type | Required | Validation Rules |
|-------|------|----------|------------------|
| `email` | string | ✅ Yes | Valid email format |

### Success Response (200 OK)

```json
{
  "success": true,
  "message": "If the email exists, a password reset link has been sent.",
  "data": {}
}
```

**Note:** The message is intentionally generic for security purposes. Always display this message to the user, regardless of whether their email exists in the system.

### Error Responses

#### 400 Bad Request - Validation Failed
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "The Email field is required.",
    "The Email field is not a valid e-mail address."
  ]
}
```

### Example Request (cURL)

```bash
curl -X POST http://localhost:5000/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com"
  }'
```

### Example Request (JavaScript/Fetch)

```javascript
const forgotPassword = async (email) => {
  try {
    const response = await fetch('http://localhost:5000/api/auth/forgot-password', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        email: email
      })
    });

    const result = await response.json();
    
    if (result.success) {
      // Always show success message (security best practice)
      alert(result.message);
      return true;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Forgot password failed:', error);
    throw error;
  }
};
```

### Password Reset Email

When a user requests a password reset, they will receive an email containing:
- A password reset link with a unique token
- Instructions on how to reset their password
- The token expires after a set period (typically 24 hours)

**Example Email Content:**
```
Subject: Reset Your Password - UtilityHub360

Hello,

You requested to reset your password. Please click the link below to reset your password:

https://your-app.com/reset-password?token={reset-token}&email={user-email}

This link will expire in 24 hours.

If you did not request this, please ignore this email.
```

---

## 4. Reset Password

### Endpoint
```
POST /api/auth/reset-password
```

### Description
Reset the user's password using the token received via email from the forgot password process.

### Authentication
Not required

### Request Body

```json
{
  "token": "reset-token-from-email",
  "email": "john@example.com",
  "newPassword": "NewPassword123!",
  "confirmPassword": "NewPassword123!"
}
```

### Field Validation Rules

| Field | Type | Required | Validation Rules |
|-------|------|----------|------------------|
| `token` | string | ✅ Yes | Valid reset token from email |
| `email` | string | ✅ Yes | Valid email format (must match token) |
| `newPassword` | string | ✅ Yes | Minimum 6 characters |
| `confirmPassword` | string | ✅ Yes | Must match `newPassword` |

### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Password has been reset successfully.",
  "data": {}
}
```

### Error Responses

#### 400 Bad Request - Validation Failed
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "The Token field is required.",
    "The NewPassword field is required.",
    "The ConfirmPassword field does not match the NewPassword field."
  ]
}
```

#### 400 Bad Request - Invalid Token
```json
{
  "success": false,
  "message": "Failed to reset password: Invalid or expired reset token"
}
```

#### 400 Bad Request - Token Expired
```json
{
  "success": false,
  "message": "Failed to reset password: Reset token has expired"
}
```

### Example Request (cURL)

```bash
curl -X POST http://localhost:5000/api/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{
    "token": "reset-token-from-email",
    "email": "john@example.com",
    "newPassword": "NewPassword123!",
    "confirmPassword": "NewPassword123!"
  }'
```

### Example Request (JavaScript/Fetch)

```javascript
const resetPassword = async (token, email, newPassword, confirmPassword) => {
  try {
    const response = await fetch('http://localhost:5000/api/auth/reset-password', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        token: token,
        email: email,
        newPassword: newPassword,
        confirmPassword: confirmPassword
      })
    });

    const result = await response.json();
    
    if (result.success) {
      alert(result.message);
      // Redirect to login page
      window.location.href = '/login';
      return true;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Reset password failed:', error);
    throw error;
  }
};
```

---

## 5. Get Current User

### Endpoint
```
GET /api/auth/me
```

### Description
Get information about the currently authenticated user.

### Authentication
**Required** - Must include JWT token in Authorization header

### Headers

```
Authorization: Bearer <jwt-token>
```

### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "id": "user-id-here",
    "name": "John Doe",
    "email": "john@example.com",
    "phone": "+1234567890",
    "role": "USER",
    "isActive": true,
    "createdAt": "2024-01-01T10:00:00Z",
    "updatedAt": "2024-01-01T10:00:00Z"
  }
}
```

### Error Responses

#### 401 Unauthorized - Missing Token
```json
{
  "success": false,
  "message": "User not authenticated"
}
```

### Example Request (cURL)

```bash
curl -X GET http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## 6. Refresh Token

### Endpoint
```
POST /api/auth/refresh
```

### Description
Get a new JWT access token using a valid refresh token. This allows users to maintain their session without re-logging in.

### Authentication
Not required (uses refresh token in request body)

### Request Body

```json
{
  "refreshToken": "guid-refresh-token-here"
}
```

### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "new-refresh-token",
    "expiresAt": "2024-01-01T12:00:00Z",
    "user": {
      "id": "user-id-here",
      "name": "John Doe",
      "email": "john@example.com",
      "phone": "+1234567890",
      "role": "USER",
      "isActive": true,
      "createdAt": "2024-01-01T10:00:00Z",
      "updatedAt": "2024-01-01T10:00:00Z"
    }
  }
}
```

---

## 7. Logout

### Endpoint
```
POST /api/auth/logout
```

### Description
Logout the current user. Invalidates the refresh token on the server side.

### Authentication
**Required** - Must include JWT token in Authorization header

### Headers

```
Authorization: Bearer <jwt-token>
```

### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Logged out successfully",
  "data": {}
}
```

---

## Error Handling

### Common Error Codes

| Status Code | Description | Common Causes |
|-------------|-------------|---------------|
| 200 | Success | Request completed successfully |
| 400 | Bad Request | Validation errors, invalid input |
| 401 | Unauthorized | Missing or invalid token, invalid credentials |
| 403 | Forbidden | Valid token but insufficient permissions |
| 500 | Internal Server Error | Server-side error |

### Error Response Format

All error responses follow this format:

```json
{
  "success": false,
  "message": "Error message here",
  "errors": ["Optional", "Array", "Of", "Validation", "Errors"]
}
```

---

## Frontend Implementation Examples

### Complete React Authentication Hook

```javascript
import { useState } from 'react';

const useAuth = () => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(false);

  const API_BASE_URL = 'http://localhost:5000/api/auth';

  // Registration
  const register = async (userData) => {
    setLoading(true);
    try {
      const response = await fetch(`${API_BASE_URL}/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(userData)
      });
      
      const result = await response.json();
      
      if (result.success) {
        localStorage.setItem('token', result.data.token);
        localStorage.setItem('refreshToken', result.data.refreshToken);
        setUser(result.data.user);
        return { success: true, data: result.data };
      } else {
        return { success: false, message: result.message, errors: result.errors };
      }
    } catch (error) {
      return { success: false, message: error.message };
    } finally {
      setLoading(false);
    }
  };

  // Login
  const login = async (email, password) => {
    setLoading(true);
    try {
      const response = await fetch(`${API_BASE_URL}/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
      });
      
      const result = await response.json();
      
      if (result.success) {
        localStorage.setItem('token', result.data.token);
        localStorage.setItem('refreshToken', result.data.refreshToken);
        setUser(result.data.user);
        return { success: true, data: result.data };
      } else {
        return { success: false, message: result.message };
      }
    } catch (error) {
      return { success: false, message: error.message };
    } finally {
      setLoading(false);
    }
  };

  // Forgot Password
  const forgotPassword = async (email) => {
    setLoading(true);
    try {
      const response = await fetch(`${API_BASE_URL}/forgot-password`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email })
      });
      
      const result = await response.json();
      return { success: result.success, message: result.message };
    } catch (error) {
      return { success: false, message: error.message };
    } finally {
      setLoading(false);
    }
  };

  // Reset Password
  const resetPassword = async (token, email, newPassword, confirmPassword) => {
    setLoading(true);
    try {
      const response = await fetch(`${API_BASE_URL}/reset-password`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ token, email, newPassword, confirmPassword })
      });
      
      const result = await response.json();
      return { success: result.success, message: result.message };
    } catch (error) {
      return { success: false, message: error.message };
    } finally {
      setLoading(false);
    }
  };

  // Logout
  const logout = async () => {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        await fetch(`${API_BASE_URL}/logout`, {
          method: 'POST',
          headers: { 'Authorization': `Bearer ${token}` }
        });
      } catch (error) {
        console.error('Logout error:', error);
      }
    }
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    setUser(null);
  };

  return {
    user,
    loading,
    register,
    login,
    forgotPassword,
    resetPassword,
    logout
  };
};

export default useAuth;
```

### Complete Registration Form Example

```javascript
import React, { useState } from 'react';
import useAuth from './useAuth';

const RegisterForm = () => {
  const { register, loading } = useAuth();
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    phone: '',
    password: '',
    confirmPassword: ''
  });
  const [errors, setErrors] = useState({});

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErrors({});

    const result = await register(formData);
    
    if (result.success) {
      // Redirect to dashboard or show success message
      alert('Registration successful!');
      window.location.href = '/dashboard';
    } else {
      // Display errors
      if (result.errors) {
        setErrors(result.errors);
      } else {
        alert(result.message);
      }
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label>Name *</label>
        <input
          type="text"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          required
          minLength={2}
          maxLength={100}
        />
        {errors.name && <span className="error">{errors.name}</span>}
      </div>

      <div>
        <label>Email *</label>
        <input
          type="email"
          value={formData.email}
          onChange={(e) => setFormData({ ...formData, email: e.target.value })}
          required
        />
        {errors.email && <span className="error">{errors.email}</span>}
      </div>

      <div>
        <label>Phone (Optional)</label>
        <input
          type="tel"
          value={formData.phone}
          onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
        />
      </div>

      <div>
        <label>Password *</label>
        <input
          type="password"
          value={formData.password}
          onChange={(e) => setFormData({ ...formData, password: e.target.value })}
          required
          minLength={6}
        />
        {errors.password && <span className="error">{errors.password}</span>}
      </div>

      <div>
        <label>Confirm Password *</label>
        <input
          type="password"
          value={formData.confirmPassword}
          onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
          required
          minLength={6}
        />
        {errors.confirmPassword && <span className="error">{errors.confirmPassword}</span>}
      </div>

      <button type="submit" disabled={loading}>
        {loading ? 'Registering...' : 'Register'}
      </button>
    </form>
  );
};

export default RegisterForm;
```

### Complete Login Form Example

```javascript
import React, { useState } from 'react';
import useAuth from './useAuth';

const LoginForm = () => {
  const { login, loading } = useAuth();
  const [formData, setFormData] = useState({
    email: '',
    password: ''
  });
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    const result = await login(formData.email, formData.password);
    
    if (result.success) {
      // Redirect to dashboard
      window.location.href = '/dashboard';
    } else {
      setError(result.message);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label>Email *</label>
        <input
          type="email"
          value={formData.email}
          onChange={(e) => setFormData({ ...formData, email: e.target.value })}
          required
        />
      </div>

      <div>
        <label>Password *</label>
        <input
          type="password"
          value={formData.password}
          onChange={(e) => setFormData({ ...formData, password: e.target.value })}
          required
          minLength={6}
        />
      </div>

      {error && <div className="error">{error}</div>}

      <button type="submit" disabled={loading}>
        {loading ? 'Logging in...' : 'Login'}
      </button>

      <a href="/forgot-password">Forgot Password?</a>
    </form>
  );
};

export default LoginForm;
```

### Forgot Password Form Example

```javascript
import React, { useState } from 'react';
import useAuth from './useAuth';

const ForgotPasswordForm = () => {
  const { forgotPassword, loading } = useAuth();
  const [email, setEmail] = useState('');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setMessage('');
    setError('');

    const result = await forgotPassword(email);
    
    if (result.success) {
      // Always show success message (security best practice)
      setMessage(result.message);
      setEmail(''); // Clear form
    } else {
      setError(result.message);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label>Email *</label>
        <input
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
          placeholder="Enter your email address"
        />
      </div>

      {message && <div className="success">{message}</div>}
      {error && <div className="error">{error}</div>}

      <button type="submit" disabled={loading || !email}>
        {loading ? 'Sending...' : 'Send Reset Link'}
      </button>

      <a href="/login">Back to Login</a>
    </form>
  );
};

export default ForgotPasswordForm;
```

### Reset Password Form Example

```javascript
import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import useAuth from './useAuth';

const ResetPasswordForm = () => {
  const { resetPassword, loading } = useAuth();
  const [searchParams] = useSearchParams();
  
  const [formData, setFormData] = useState({
    token: '',
    email: '',
    newPassword: '',
    confirmPassword: ''
  });
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    // Get token and email from URL query parameters
    const token = searchParams.get('token');
    const email = searchParams.get('email');
    
    if (token && email) {
      setFormData(prev => ({ ...prev, token, email }));
    }
  }, [searchParams]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setMessage('');
    setError('');

    if (formData.newPassword !== formData.confirmPassword) {
      setError('Passwords do not match');
      return;
    }

    const result = await resetPassword(
      formData.token,
      formData.email,
      formData.newPassword,
      formData.confirmPassword
    );
    
    if (result.success) {
      setMessage(result.message);
      // Redirect to login after 2 seconds
      setTimeout(() => {
        window.location.href = '/login';
      }, 2000);
    } else {
      setError(result.message);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label>Email *</label>
        <input
          type="email"
          value={formData.email}
          onChange={(e) => setFormData({ ...formData, email: e.target.value })}
          required
          readOnly={!!searchParams.get('email')}
        />
      </div>

      <div>
        <label>New Password *</label>
        <input
          type="password"
          value={formData.newPassword}
          onChange={(e) => setFormData({ ...formData, newPassword: e.target.value })}
          required
          minLength={6}
          placeholder="Enter new password (minimum 6 characters)"
        />
      </div>

      <div>
        <label>Confirm Password *</label>
        <input
          type="password"
          value={formData.confirmPassword}
          onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
          required
          minLength={6}
          placeholder="Confirm new password"
        />
      </div>

      {message && <div className="success">{message}</div>}
      {error && <div className="error">{error}</div>}

      <button type="submit" disabled={loading || !formData.token}>
        {loading ? 'Resetting...' : 'Reset Password'}
      </button>
    </form>
  );
};

export default ResetPasswordForm;
```

---

## Security Best Practices

### Client-Side

1. **Token Storage**
   - Store tokens securely (consider using `httpOnly` cookies in production)
   - Never store tokens in localStorage if XSS is a concern
   - Implement token refresh logic automatically

2. **Password Handling**
   - Never log passwords or send them in error messages
   - Always use HTTPS in production
   - Implement client-side password strength validation

3. **Error Messages**
   - Display generic error messages for forgot password (prevent email enumeration)
   - Don't reveal whether email exists in the system
   - Provide helpful validation messages for user experience

### Server-Side

1. **Password Security**
   - Passwords are hashed using BCrypt
   - Passwords must be at least 6 characters
   - Passwords are never returned in responses

2. **Token Security**
   - JWT tokens expire after 60 minutes (configurable)
   - Refresh tokens have longer expiration periods
   - Tokens contain user ID and role information

3. **Email Verification**
   - Password reset tokens expire after a set period
   - Tokens are single-use only
   - Email addresses must match token

---

## Token Management

### JWT Token Structure

The JWT token contains:
- **sub**: User ID
- **name**: User's full name
- **email**: User's email address
- **role**: User role (USER, ADMIN)
- **iat**: Issued at timestamp
- **exp**: Expiration timestamp

### Token Expiration

- **Access Token**: 60 minutes (configurable in `appsettings.json`)
- **Refresh Token**: 7 days (configurable)

### Automatic Token Refresh Flow

```javascript
// Interceptor for automatic token refresh
const refreshTokenIfNeeded = async () => {
  const token = localStorage.getItem('token');
  const refreshToken = localStorage.getItem('refreshToken');
  
  if (!token || !refreshToken) {
    return null;
  }

  // Decode token to check expiration
  const decoded = JSON.parse(atob(token.split('.')[1]));
  const expirationTime = decoded.exp * 1000; // Convert to milliseconds
  const currentTime = Date.now();
  
  // Refresh if token expires in less than 5 minutes
  if (expirationTime - currentTime < 5 * 60 * 1000) {
    try {
      const response = await fetch('http://localhost:5000/api/auth/refresh', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken })
      });
      
      const result = await response.json();
      if (result.success) {
        localStorage.setItem('token', result.data.token);
        localStorage.setItem('refreshToken', result.data.refreshToken);
        return result.data.token;
      }
    } catch (error) {
      console.error('Token refresh failed:', error);
      // Redirect to login
      window.location.href = '/login';
    }
  }
  
  return token;
};
```

---

## Testing

### Using Swagger UI

1. Navigate to `http://localhost:5000/swagger`
2. Find the `/api/auth` endpoints
3. Test endpoints directly in the browser
4. Use the "Authorize" button to set tokens for protected endpoints

### Using Postman

1. Create a new request collection
2. Set base URL: `http://localhost:5000/api/auth`
3. Set Content-Type: `application/json`
4. Test each endpoint with appropriate request bodies
5. Save tokens from register/login responses for authenticated requests

### Example Test Cases

#### Registration Test
```json
POST /api/auth/register
{
  "name": "Test User",
  "email": "test@example.com",
  "phone": "+1234567890",
  "password": "test123",
  "confirmPassword": "test123"
}
```

#### Login Test
```json
POST /api/auth/login
{
  "email": "test@example.com",
  "password": "test123"
}
```

#### Forgot Password Test
```json
POST /api/auth/forgot-password
{
  "email": "test@example.com"
}
```

---

## Support

For additional help or questions:
- Check the main API documentation: `Documentation/API/API_ENDPOINTS_REFERENCE.md`
- Review the authentication guide: `Documentation/General/authenticationGuide.md`
- Contact the development team

---

**Last Updated:** 2024
**Version:** 1.0

