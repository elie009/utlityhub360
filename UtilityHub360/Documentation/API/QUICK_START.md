# API Quick Start Guide

## Main Endpoints

### 1. Dashboard Summary
```
GET /api/Dashboard/summary
```
Returns financial summary with remaining amount calculation.

**Formula:**
```
Remaining Amount = Total Income - Total Expenses - Total Savings
```

### 2. Disposable Amount
```
GET /api/Dashboard/disposable-amount
```
Returns detailed disposable amount with breakdown of fixed and variable expenses.

**Formula:**
```
Disposable Amount = Total Income - (Fixed Expenses + Variable Expenses)
```

## Authentication

All endpoints require JWT Bearer token:
```
Authorization: Bearer {your_jwt_token}
```

## Base URL

- **Development:** `http://localhost:5000/api`
- **Production:** Configure in appsettings.json

## Complete Documentation

For detailed API documentation, see:
- [API_ENDPOINTS_REFERENCE.md](./API_ENDPOINTS_REFERENCE.md) - Complete endpoint reference
- [COMPLETE_API_DOCUMENTATION.md](./COMPLETE_API_DOCUMENTATION.md) - Full API documentation

