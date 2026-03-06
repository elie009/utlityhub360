# PIN Login & Biometric (Face/Fingerprint) – Backend Documentation

## Overview

This document describes how **PIN login** and **biometric (face/fingerprint) login** work from the backend perspective. These methods are **mobile-only** and intended for the UtilityHub360 Flutter app.

- **Biometric (face/fingerprint)**: Handled entirely on the device by the OS. The backend does **not** receive biometric data. The app uses biometrics to unlock an existing session or to authorize a PIN login. No dedicated biometric API is required.
- **PIN login**: Can be used in two ways:
  1. **Local-only**: PIN is stored and verified on the device. The backend is not involved in PIN verification. The app uses PIN to “unlock” an already stored session (e.g. token from a previous email/password login).
  2. **Server-side PIN**: PIN hash is stored per user. The app sends email + PIN to the backend; the backend verifies and returns JWT/refresh token. This allows PIN login to work after reinstall or on a new device.

**Base URL:** `http://localhost:5000/api/Auth` (or your deployed API base).

---

## 1. Biometric (Face / Fingerprint)

- Biometric authentication is performed by the mobile OS (e.g. Android Keystore, iOS Secure Enclave).
- The backend **never** receives or verifies biometric data.
- Flow:
  1. User taps “Use fingerprint/face to access” in the app.
  2. App calls platform APIs (e.g. `local_auth` in Flutter).
  3. On success, the app either:
     - Restores an existing session (token/user from local storage), or
     - Performs a PIN login (local or via `POST /Auth/login-pin` if server-side PIN is used).
- **Backend requirement:** None for biometrics. Optional: support PIN login endpoint so that biometric success can trigger a PIN-based token issuance.

---

## 2. PIN Login – Local-Only (Optional Flutter Behavior)

- PIN can be stored on device (e.g. `StorageService`, key `user_pin`).
- Login flow: User enters PIN → app compares to stored value → if match, app restores session (existing token/user) and navigates to dashboard.
- **Backend:** No change. Existing `POST /Auth/login` (email/password) and `GET /UserProfile` remain the source of tokens and user data. PIN only gates access to an already-stored session on the device.

---

## 3. PIN Login – Server-Side API

The backend supports optional server-side PIN for mobile clients.

### 3.1 Data model

- **User entity**: Optional field `PinHash` (string, nullable) – BCrypt hash of the user’s 6-digit PIN.
- **Migration:** `20260131000000_AddPinHashToUser.cs` adds the `PinHash` column to the `Users` table.

### 3.2 Setup PIN (authenticated)

**Endpoint:** `POST /api/Auth/setup-pin`

**Auth:** Required (Bearer JWT).

**Request body:**

```json
{
  "pin": "123456"
}
```

| Field | Type   | Required | Validation        |
|-------|--------|----------|-------------------|
| `pin` | string | Yes      | Exactly 6 digits  |

**Success response (200 OK):**

```json
{
  "success": true,
  "message": "PIN set successfully.",
  "data": {}
}
```

**Error responses:**

- **401 Unauthorized:** User not authenticated.
- **400 Bad Request:** Validation failed (e.g. PIN not 6 digits).

### 3.3 Login with PIN

**Endpoint:** `POST /api/Auth/login-pin`

**Auth:** Not required (same as email/password login).

**Request body:**

```json
{
  "email": "user@example.com",
  "pin": "123456"
}
```

| Field   | Type   | Required | Validation        |
|---------|--------|----------|-------------------|
| `email` | string | Yes      | Valid email       |
| `pin`   | string | Yes      | Exactly 6 digits  |

**Success response (200 OK):**

Same shape as `POST /Auth/login`:

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "...",
    "expiresAt": "2024-01-01T12:00:00Z",
    "user": {
      "id": "...",
      "name": "...",
      "email": "...",
      "phone": "...",
      "country": "...",
      "role": "USER",
      "isActive": true,
      "emailVerified": true,
      "createdAt": "...",
      "updatedAt": "..."
    }
  }
}
```

**Error responses:**

- **401 Unauthorized:** Invalid email or PIN, or PIN not set, or account inactive.
- **400 Bad Request:** Validation failed.

### 3.4 Security notes

- Only a hash of the PIN is stored (BCrypt); never store plain PIN.
- Use HTTPS only.
- Optional: rate limit `login-pin` by IP or device to prevent brute force.
- PIN is **mobile-only**: these endpoints are intended for use by the mobile app; web can remain email/password only.

---

## 4. Summary

| Feature             | Where it runs | Backend API needed                          |
|---------------------|---------------|---------------------------------------------|
| Face / fingerprint  | Device only   | None (optional: use PIN login API after)    |
| PIN (local)         | Device only   | None                                        |
| PIN (server-side)    | App + backend | `POST /Auth/setup-pin`, `POST /Auth/login-pin` |

---

## 5. Related docs

- [Authentication_API_Documentation.md](./Authentication_API_Documentation.md) – Email/password login, refresh, logout.
- [authentication-flow.md](../UserFlows/authentication-flow.md) – High-level auth flows.
