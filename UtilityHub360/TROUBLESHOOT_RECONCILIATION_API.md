# 🔍 Troubleshooting: Reconciliation API Not Reaching

## Endpoint Details
- **URL:** `GET /api/reconciliation/account/{bankAccountId}`
- **Full URL:** `http://localhost:5000/api/reconciliation/account/cac74e59-47a4-49bd-b0f2-78e3d8771bf7`
- **Requires:** JWT Authentication (Bearer token)

---

## ✅ Check These Issues

### 1. **Authentication Required** ⚠️

The endpoint requires a JWT token. Make sure you're sending:

```
Authorization: Bearer {your_jwt_token}
```

**Test in Swagger:**
1. Go to `http://localhost:5000/swagger`
2. Click "Authorize" button (lock icon)
3. Enter: `Bearer {your_token}`
4. Click "Authorize"
5. Try the endpoint: `GET /api/reconciliation/account/{bankAccountId}`

---

### 2. **Database Tables Don't Exist** ❌

**Error you'll see:**
```json
{
    "success": false,
    "message": "Failed to get reconciliations: Invalid object name 'Reconciliations'."
}
```

**Solution:** Run the migration script:
- File: `run_migration_direct_FIXED.sql`
- Execute it on database: `174.138.185.18` → `DBUTILS`

---

### 3. **Application Not Running** ⚠️

**Check if app is running:**
```
GET http://localhost:5000/health
```

**Expected response:**
```json
{
    "status": "Healthy",
    "environment": "Production",
    "databaseServer": "LIVE Production Database (174.138.185.18)"
}
```

**If you get connection refused:**
- Start the application: `dotnet run`

---

### 4. **Route Not Found (404)** ❌

**Check Swagger:**
1. Go to `http://localhost:5000/swagger`
2. Look for "Reconciliation" section
3. Find: `GET /api/reconciliation/account/{bankAccountId}`

**If not visible:**
- Restart the application
- Check if `ReconciliationController` is in the project
- Verify service is registered in `Program.cs` (line 155)

---

### 5. **CORS Issues** ⚠️

If calling from frontend, check browser console for CORS errors.

**Solution:** The CORS is configured in `Program.cs` to allow `localhost:3000` and `localhost:5000`

---

## 🧪 Step-by-Step Testing

### Step 1: Verify Application is Running
```bash
curl http://localhost:5000/health
```

### Step 2: Get Authentication Token
```bash
POST http://localhost:5000/api/auth/login
{
    "email": "your@email.com",
    "password": "yourpassword"
}
```

### Step 3: Test Endpoint with Token
```bash
GET http://localhost:5000/api/reconciliation/account/cac74e59-47a4-49bd-b0f2-78e3d8771bf7
Headers:
  Authorization: Bearer {token_from_step_2}
```

---

## 📋 Common Error Codes

| Status Code | Meaning | Solution |
|------------|---------|----------|
| **401 Unauthorized** | Missing or invalid JWT token | Add `Authorization: Bearer {token}` header |
| **400 Bad Request** | Database table doesn't exist | Run migration script |
| **404 Not Found** | Route not found | Check if app is running, restart if needed |
| **500 Internal Server Error** | Server error | Check application logs |

---

## ✅ Quick Fix Checklist

- [ ] Application is running (`/health` returns OK)
- [ ] JWT token is included in request
- [ ] Database tables exist (run migration script)
- [ ] Endpoint visible in Swagger UI
- [ ] No CORS errors in browser console

---

## 🎯 Expected Success Response

```json
{
    "success": true,
    "message": null,
    "data": [],
    "errors": []
}
```

(Empty array if no reconciliations exist yet)

---

**Need more help?** Check the application logs for detailed error messages.

