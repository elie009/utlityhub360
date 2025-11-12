# Income Source Management Documentation

## Overview

The Income Source Management system in UtilityHub360 provides comprehensive functionality for managing multiple income sources, tracking income by category and frequency, and calculating monthly income summaries. This system supports various income types including salary, freelance work, passive income, investments, and side hustles.

## Table of Contents

1. [Quick Start Guide](#quick-start-guide)
2. [Authentication](#authentication)
3. [API Endpoints](#api-endpoints)
4. [Data Transfer Objects (DTOs)](#data-transfer-objects-dtos)
5. [Request/Response Examples](#requestresponse-examples)
6. [Frontend Integration Examples](#frontend-integration-examples)
7. [Error Handling](#error-handling)
8. [Best Practices](#best-practices)

## Quick Start Guide

### Base URL
```
/api/incomesource
```

### Authentication
All endpoints require JWT authentication via the `Authorization` header:
```
Authorization: Bearer <jwt_token>
```

### Common Use Cases

1. **Create Income Source**: Track salary, freelance income, or other income streams
2. **View All Income Sources**: Display user's income sources with monthly totals
3. **Calculate Monthly Income**: Get total monthly income across all sources
4. **Filter by Category**: Group income by PRIMARY, PASSIVE, BUSINESS, etc.
5. **Filter by Frequency**: View WEEKLY, MONTHLY, ANNUALLY income separately

## Authentication

All Income Source endpoints require authentication. The user ID is automatically extracted from the JWT token.

**Header Required:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## API Endpoints

### 1. Create Income Source

**Endpoint:** `POST /api/incomesource`

**Description:** Create a new income source for the authenticated user.

**Request Body:**
```json
{
  "name": "Software Developer Salary",
  "amount": 5000.00,
  "frequency": "MONTHLY",
  "category": "PRIMARY",
  "currency": "USD",
  "description": "Full-time employment at Tech Corp",
  "company": "Tech Corp"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Income source created successfully",
  "data": {
    "id": "abc123-def456-ghi789",
    "userId": "user-id-123",
    "name": "Software Developer Salary",
    "amount": 5000.00,
    "frequency": "MONTHLY",
    "category": "PRIMARY",
    "currency": "USD",
    "isActive": true,
    "description": "Full-time employment at Tech Corp",
    "company": "Tech Corp",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z",
    "monthlyAmount": 5000.00
  },
  "errors": []
}
```

**Fields:**
- `name` (required): Income source name (max 100 chars)
- `amount` (required): Income amount (must be > 0)
- `frequency` (required): `WEEKLY`, `BI_WEEKLY`, `MONTHLY`, `QUARTERLY`, `ANNUALLY` (default: `MONTHLY`)
- `category` (required): `PRIMARY`, `PASSIVE`, `BUSINESS`, `SIDE_HUSTLE`, `INVESTMENT`, `RENTAL`, `OTHER` (default: `PRIMARY`)
- `currency` (optional): Currency code (default: `USD`, max 10 chars)
- `description` (optional): Description (max 500 chars)
- `company` (optional): Company name (max 200 chars)

**Error Responses:**
- `400 Bad Request`: Validation error or duplicate name
- `401 Unauthorized`: Invalid or missing token

---

### 2. Create Multiple Income Sources (Bulk)

**Endpoint:** `POST /api/incomesource/bulk`

**Description:** Create multiple income sources in a single request.

**Request Body:**
```json
{
  "incomeSources": [
    {
      "name": "Software Developer Salary",
      "amount": 5000.00,
      "frequency": "MONTHLY",
      "category": "PRIMARY",
      "company": "Tech Corp"
    },
    {
      "name": "Freelance Web Development",
      "amount": 2000.00,
      "frequency": "MONTHLY",
      "category": "SIDE_HUSTLE",
      "description": "Part-time freelance work"
    }
  ]
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Income sources created successfully",
  "data": [
    {
      "id": "source-id-1",
      "name": "Software Developer Salary",
      "amount": 5000.00,
      "monthlyAmount": 5000.00,
      ...
    },
    {
      "id": "source-id-2",
      "name": "Freelance Web Development",
      "amount": 2000.00,
      "monthlyAmount": 2000.00,
      ...
    }
  ],
  "errors": []
}
```

---

### 3. Get Income Source by ID

**Endpoint:** `GET /api/incomesource/{incomeSourceId}`

**Description:** Retrieve a specific income source by its ID.

**Path Parameters:**
- `incomeSourceId`: The unique identifier of the income source

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "id": "abc123-def456-ghi789",
    "userId": "user-id-123",
    "name": "Software Developer Salary",
    "amount": 5000.00,
    "frequency": "MONTHLY",
    "category": "PRIMARY",
    "currency": "USD",
    "isActive": true,
    "description": "Full-time employment at Tech Corp",
    "company": "Tech Corp",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z",
    "monthlyAmount": 5000.00
  },
  "errors": []
}
```

**Error Responses:**
- `404 Not Found`: Income source not found or doesn't belong to user
- `401 Unauthorized`: Invalid or missing token

---

### 4. Get All Income Sources

**Endpoint:** `GET /api/incomesource`

**Description:** Get all income sources for the authenticated user.

**Query Parameters:**
- `activeOnly` (optional, default: `true`): Filter to only active income sources
- `page` (optional, default: `1`): Page number for pagination
- `limit` (optional, default: `50`): Items per page

**Example Request:**
```
GET /api/incomesource?activeOnly=true&page=1&limit=50
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "id": "source-id-1",
      "name": "Software Developer Salary",
      "amount": 5000.00,
      "frequency": "MONTHLY",
      "category": "PRIMARY",
      "monthlyAmount": 5000.00,
      "isActive": true,
      ...
    },
    {
      "id": "source-id-2",
      "name": "Freelance Web Development",
      "amount": 2000.00,
      "frequency": "MONTHLY",
      "category": "SIDE_HUSTLE",
      "monthlyAmount": 2000.00,
      "isActive": true,
      ...
    }
  ],
  "errors": []
}
```

---

### 5. Get All Income Sources with Summary

**Endpoint:** `GET /api/incomesource/with-summary`

**Description:** Get all income sources along with summary statistics.

**Query Parameters:**
- `activeOnly` (optional, default: `true`): Filter to only active income sources

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "incomeSources": [
      {
        "id": "source-id-1",
        "name": "Software Developer Salary",
        "amount": 5000.00,
        "monthlyAmount": 5000.00,
        ...
      }
    ],
    "totalActiveSources": 3,
    "totalPrimarySources": 1,
    "totalSources": 3,
    "totalMonthlyIncome": 7000.00
  },
  "errors": []
}
```

---

### 6. Update Income Source

**Endpoint:** `PUT /api/incomesource/{incomeSourceId}`

**Description:** Update an existing income source. All fields are optional - only provided fields will be updated.

**Path Parameters:**
- `incomeSourceId`: The unique identifier of the income source

**Request Body (all fields optional):**
```json
{
  "name": "Senior Software Developer Salary",
  "amount": 6000.00,
  "frequency": "MONTHLY",
  "category": "PRIMARY",
  "description": "Promoted to senior role",
  "isActive": true
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Income source updated successfully",
  "data": {
    "id": "abc123-def456-ghi789",
    "name": "Senior Software Developer Salary",
    "amount": 6000.00,
    "monthlyAmount": 6000.00,
    ...
    "updatedAt": "2024-01-20T14:30:00Z"
  },
  "errors": []
}
```

**Error Responses:**
- `400 Bad Request`: Validation error
- `404 Not Found`: Income source not found
- `401 Unauthorized`: Invalid or missing token

---

### 7. Delete Income Source

**Endpoint:** `DELETE /api/incomesource/{incomeSourceId}`

**Description:** Delete an income source (soft delete).

**Path Parameters:**
- `incomeSourceId`: The unique identifier of the income source

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Income source deleted successfully",
  "data": true,
  "errors": []
}
```

**Error Responses:**
- `404 Not Found`: Income source not found
- `401 Unauthorized`: Invalid or missing token

---

### 8. Get Total Monthly Income

**Endpoint:** `GET /api/incomesource/total-monthly-income`

**Description:** Get the total monthly income from all active income sources (automatically converts all frequencies to monthly equivalent).

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": 7000.00,
  "errors": []
}
```

**Note:** This endpoint automatically converts all income frequencies to monthly amounts:
- `WEEKLY` × 4.33 = monthly
- `BI_WEEKLY` × 2.17 = monthly
- `MONTHLY` = as-is
- `QUARTERLY` ÷ 3 = monthly
- `ANNUALLY` ÷ 12 = monthly

---

### 9. Get Income Breakdown by Category

**Endpoint:** `GET /api/incomesource/income-by-category`

**Description:** Get total income grouped by category.

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "PRIMARY": 5000.00,
    "SIDE_HUSTLE": 2000.00,
    "PASSIVE": 500.00,
    "INVESTMENT": 1000.00
  },
  "errors": []
}
```

---

### 10. Get Income Breakdown by Frequency

**Endpoint:** `GET /api/incomesource/income-by-frequency`

**Description:** Get total income grouped by frequency (in monthly equivalent amounts).

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "MONTHLY": 6000.00,
    "WEEKLY": 866.00,
    "ANNUALLY": 133.33
  },
  "errors": []
}
```

---

### 11. Get Available Categories

**Endpoint:** `GET /api/incomesource/available-categories`

**Description:** Get list of available income source categories.

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": [
    "PRIMARY",
    "PASSIVE",
    "BUSINESS",
    "SIDE_HUSTLE",
    "INVESTMENT",
    "RENTAL",
    "OTHER"
  ],
  "errors": []
}
```

---

### 12. Get Available Frequencies

**Endpoint:** `GET /api/incomesource/available-frequencies`

**Description:** Get list of available income frequencies.

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": [
    "WEEKLY",
    "BI_WEEKLY",
    "MONTHLY",
    "QUARTERLY",
    "ANNUALLY"
  ],
  "errors": []
}
```

---

### 13. Get Income Sources by Category

**Endpoint:** `GET /api/incomesource/by-category/{category}`

**Description:** Get all income sources filtered by a specific category.

**Path Parameters:**
- `category`: Category name (e.g., `PRIMARY`, `SIDE_HUSTLE`, `PASSIVE`)

**Query Parameters:**
- `page` (optional, default: `1`): Page number
- `limit` (optional, default: `50`): Items per page

**Example Request:**
```
GET /api/incomesource/by-category/PRIMARY?page=1&limit=50
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "id": "source-id-1",
      "name": "Software Developer Salary",
      "category": "PRIMARY",
      "amount": 5000.00,
      ...
    }
  ],
  "errors": []
}
```

---

### 14. Get Income Sources by Frequency

**Endpoint:** `GET /api/incomesource/by-frequency/{frequency}`

**Description:** Get all income sources filtered by a specific frequency.

**Path Parameters:**
- `frequency`: Frequency name (e.g., `MONTHLY`, `WEEKLY`, `ANNUALLY`)

**Query Parameters:**
- `page` (optional, default: `1`): Page number
- `limit` (optional, default: `50`): Items per page

**Example Request:**
```
GET /api/incomesource/by-frequency/MONTHLY?page=1&limit=50
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "id": "source-id-1",
      "name": "Software Developer Salary",
      "frequency": "MONTHLY",
      "amount": 5000.00,
      ...
    }
  ],
  "errors": []
}
```

---

### 15. Toggle Income Source Status

**Endpoint:** `GET /api/incomesource/toggle-status`

**Description:** Get summary of income source status counts (active, primary, total).

**Note:** This endpoint appears to be misnamed - it returns summary statistics rather than toggling status. Use the `UpdateIncomeSource` endpoint with `isActive: false` to deactivate an income source.

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "totalActiveSources": 3,
    "totalPrimarySources": 1,
    "totalSources": 3
  },
  "errors": []
}
```

---

## Data Transfer Objects (DTOs)

### CreateIncomeSourceDto

```typescript
interface CreateIncomeSourceDto {
  name: string;              // Required, max 100 chars
  amount: number;            // Required, must be > 0
  frequency: string;         // Required: WEEKLY, BI_WEEKLY, MONTHLY, QUARTERLY, ANNUALLY
  category: string;          // Required: PRIMARY, PASSIVE, BUSINESS, SIDE_HUSTLE, INVESTMENT, RENTAL, OTHER
  currency?: string;         // Optional, default: "USD", max 10 chars
  description?: string;       // Optional, max 500 chars
  company?: string;          // Optional, max 200 chars
}
```

### UpdateIncomeSourceDto

```typescript
interface UpdateIncomeSourceDto {
  name?: string;             // Optional, max 100 chars
  amount?: number;            // Optional, must be > 0
  frequency?: string;         // Optional: WEEKLY, BI_WEEKLY, MONTHLY, QUARTERLY, ANNUALLY
  category?: string;          // Optional: PRIMARY, PASSIVE, BUSINESS, SIDE_HUSTLE, INVESTMENT, RENTAL, OTHER
  currency?: string;         // Optional, max 10 chars
  description?: string;       // Optional, max 500 chars
  company?: string;          // Optional, max 200 chars
  isActive?: boolean;        // Optional
}
```

### IncomeSourceDto

```typescript
interface IncomeSourceDto {
  id: string;
  userId: string;
  name: string;
  amount: number;
  frequency: string;
  category: string;
  currency: string;
  isActive: boolean;
  description?: string;
  company?: string;
  createdAt: string;         // ISO 8601 date string
  updatedAt: string;         // ISO 8601 date string
  monthlyAmount: number;     // Automatically calculated monthly equivalent
}
```

### IncomeSourceListResponseDto

```typescript
interface IncomeSourceListResponseDto {
  incomeSources: IncomeSourceDto[];
  totalActiveSources: number;
  totalPrimarySources: number;
  totalSources: number;
  totalMonthlyIncome: number;
}
```

### BulkCreateIncomeSourceDto

```typescript
interface BulkCreateIncomeSourceDto {
  incomeSources: CreateIncomeSourceDto[];
}
```

---

## Request/Response Examples

### Example 1: Create Monthly Salary

**Request:**
```http
POST /api/incomesource
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Software Engineer Salary",
  "amount": 7500.00,
  "frequency": "MONTHLY",
  "category": "PRIMARY",
  "currency": "USD",
  "company": "Tech Solutions Inc.",
  "description": "Full-time software engineer position"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Income source created successfully",
  "data": {
    "id": "abc-123-def",
    "name": "Software Engineer Salary",
    "amount": 7500.00,
    "monthlyAmount": 7500.00,
    "frequency": "MONTHLY",
    "category": "PRIMARY",
    "isActive": true,
    ...
  }
}
```

---

### Example 2: Create Weekly Freelance Income

**Request:**
```http
POST /api/incomesource
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Freelance Design Work",
  "amount": 500.00,
  "frequency": "WEEKLY",
  "category": "SIDE_HUSTLE",
  "description": "Weekly design projects"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "xyz-789-abc",
    "name": "Freelance Design Work",
    "amount": 500.00,
    "monthlyAmount": 2165.00,  // 500 × 4.33 = 2165
    "frequency": "WEEKLY",
    "category": "SIDE_HUSTLE",
    ...
  }
}
```

---

### Example 3: Create Annual Bonus

**Request:**
```http
POST /api/incomesource
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "Annual Performance Bonus",
  "amount": 15000.00,
  "frequency": "ANNUALLY",
  "category": "PRIMARY",
  "description": "Year-end performance bonus"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "bonus-123-456",
    "name": "Annual Performance Bonus",
    "amount": 15000.00,
    "monthlyAmount": 1250.00,  // 15000 ÷ 12 = 1250
    "frequency": "ANNUALLY",
    "category": "PRIMARY",
    ...
  }
}
```

---

## Frontend Integration Examples

### React/TypeScript Example

```typescript
import axios from 'axios';

const API_BASE_URL = 'https://api.utilityhub360.com/api';

// Create Income Source
async function createIncomeSource(token: string, incomeData: CreateIncomeSourceDto) {
  try {
    const response = await axios.post(
      `${API_BASE_URL}/incomesource`,
      incomeData,
      {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      }
    );
    
    if (response.data.success) {
      return response.data.data;
    } else {
      throw new Error(response.data.message);
    }
  } catch (error) {
    console.error('Error creating income source:', error);
    throw error;
  }
}

// Get All Income Sources
async function getAllIncomeSources(token: string, activeOnly: boolean = true) {
  try {
    const response = await axios.get(
      `${API_BASE_URL}/incomesource?activeOnly=${activeOnly}`,
      {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      }
    );
    
    return response.data.data;
  } catch (error) {
    console.error('Error fetching income sources:', error);
    throw error;
  }
}

// Get Total Monthly Income
async function getTotalMonthlyIncome(token: string) {
  try {
    const response = await axios.get(
      `${API_BASE_URL}/incomesource/total-monthly-income`,
      {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      }
    );
    
    return response.data.data;
  } catch (error) {
    console.error('Error fetching total monthly income:', error);
    throw error;
  }
}

// Update Income Source
async function updateIncomeSource(
  token: string, 
  incomeSourceId: string, 
  updateData: UpdateIncomeSourceDto
) {
  try {
    const response = await axios.put(
      `${API_BASE_URL}/incomesource/${incomeSourceId}`,
      updateData,
      {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      }
    );
    
    return response.data.data;
  } catch (error) {
    console.error('Error updating income source:', error);
    throw error;
  }
}

// Delete Income Source
async function deleteIncomeSource(token: string, incomeSourceId: string) {
  try {
    const response = await axios.delete(
      `${API_BASE_URL}/incomesource/${incomeSourceId}`,
      {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      }
    );
    
    return response.data.success;
  } catch (error) {
    console.error('Error deleting income source:', error);
    throw error;
  }
}
```

### React Hook Example

```typescript
import { useState, useEffect } from 'react';

function useIncomeSources(token: string) {
  const [incomeSources, setIncomeSources] = useState<IncomeSourceDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [totalMonthlyIncome, setTotalMonthlyIncome] = useState<number>(0);

  useEffect(() => {
    async function fetchData() {
      try {
        setLoading(true);
        
        // Fetch income sources
        const sources = await getAllIncomeSources(token);
        setIncomeSources(sources);
        
        // Fetch total monthly income
        const total = await getTotalMonthlyIncome(token);
        setTotalMonthlyIncome(total);
        
        setError(null);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load income sources');
      } finally {
        setLoading(false);
      }
    }

    if (token) {
      fetchData();
    }
  }, [token]);

  return {
    incomeSources,
    totalMonthlyIncome,
    loading,
    error,
    refetch: () => {
      // Trigger re-fetch
      setLoading(true);
      getAllIncomeSources(token).then(setIncomeSources);
      getTotalMonthlyIncome(token).then(setTotalMonthlyIncome).finally(() => setLoading(false));
    }
  };
}
```

### React Component Example

```tsx
import React, { useState } from 'react';

function IncomeSourceForm({ token, onSuccess }: { token: string, onSuccess: () => void }) {
  const [formData, setFormData] = useState<CreateIncomeSourceDto>({
    name: '',
    amount: 0,
    frequency: 'MONTHLY',
    category: 'PRIMARY',
    currency: 'USD'
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      await createIncomeSource(token, formData);
      onSuccess();
      // Reset form
      setFormData({
        name: '',
        amount: 0,
        frequency: 'MONTHLY',
        category: 'PRIMARY',
        currency: 'USD'
      });
    } catch (error) {
      alert('Failed to create income source');
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <input
        type="text"
        placeholder="Income Source Name"
        value={formData.name}
        onChange={(e) => setFormData({ ...formData, name: e.target.value })}
        required
      />
      
      <input
        type="number"
        placeholder="Amount"
        value={formData.amount}
        onChange={(e) => setFormData({ ...formData, amount: parseFloat(e.target.value) })}
        required
        min="0.01"
        step="0.01"
      />
      
      <select
        value={formData.frequency}
        onChange={(e) => setFormData({ ...formData, frequency: e.target.value })}
      >
        <option value="WEEKLY">Weekly</option>
        <option value="BI_WEEKLY">Bi-Weekly</option>
        <option value="MONTHLY">Monthly</option>
        <option value="QUARTERLY">Quarterly</option>
        <option value="ANNUALLY">Annually</option>
      </select>
      
      <select
        value={formData.category}
        onChange={(e) => setFormData({ ...formData, category: e.target.value })}
      >
        <option value="PRIMARY">Primary</option>
        <option value="PASSIVE">Passive</option>
        <option value="BUSINESS">Business</option>
        <option value="SIDE_HUSTLE">Side Hustle</option>
        <option value="INVESTMENT">Investment</option>
        <option value="RENTAL">Rental</option>
        <option value="OTHER">Other</option>
      </select>
      
      <button type="submit">Create Income Source</button>
    </form>
  );
}
```

---

## Error Handling

### Common Error Responses

#### 400 Bad Request - Validation Error
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "Amount must be greater than 0",
    "Name cannot exceed 100 characters"
  ]
}
```

#### 400 Bad Request - Duplicate Name
```json
{
  "success": false,
  "message": "Income source with this name already exists",
  "data": null,
  "errors": []
}
```

#### 401 Unauthorized
```json
{
  "success": false,
  "message": "User not authenticated",
  "data": null,
  "errors": []
}
```

#### 404 Not Found
```json
{
  "success": false,
  "message": "Income source not found",
  "data": null,
  "errors": []
}
```

### Error Handling Best Practices

```typescript
async function handleApiCall<T>(
  apiCall: () => Promise<T>
): Promise<{ data?: T; error?: string }> {
  try {
    const data = await apiCall();
    return { data };
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const errorMessage = error.response?.data?.message || error.message;
      return { error: errorMessage };
    }
    return { error: 'An unexpected error occurred' };
  }
}

// Usage
const { data, error } = await handleApiCall(() => 
  createIncomeSource(token, incomeData)
);

if (error) {
  // Show error message to user
  console.error(error);
} else {
  // Handle success
  console.log('Income source created:', data);
}
```

---

## Best Practices

### 1. Always Include Authorization Header
```typescript
const headers = {
  'Authorization': `Bearer ${token}`,
  'Content-Type': 'application/json'
};
```

### 2. Validate Input Before Sending
```typescript
function validateIncomeSource(data: CreateIncomeSourceDto): string[] {
  const errors: string[] = [];
  
  if (!data.name || data.name.length === 0) {
    errors.push('Name is required');
  }
  
  if (data.name && data.name.length > 100) {
    errors.push('Name cannot exceed 100 characters');
  }
  
  if (data.amount <= 0) {
    errors.push('Amount must be greater than 0');
  }
  
  const validFrequencies = ['WEEKLY', 'BI_WEEKLY', 'MONTHLY', 'QUARTERLY', 'ANNUALLY'];
  if (!validFrequencies.includes(data.frequency)) {
    errors.push('Invalid frequency');
  }
  
  const validCategories = ['PRIMARY', 'PASSIVE', 'BUSINESS', 'SIDE_HUSTLE', 'INVESTMENT', 'RENTAL', 'OTHER'];
  if (!validCategories.includes(data.category)) {
    errors.push('Invalid category');
  }
  
  return errors;
}
```

### 3. Handle Monthly Amount Conversion
The API automatically converts all frequencies to monthly amounts, but you can also display the original amount and frequency:

```typescript
function formatIncomeAmount(income: IncomeSourceDto): string {
  const formattedAmount = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: income.currency || 'USD'
  }).format(income.amount);
  
  return `${formattedAmount} ${income.frequency}`;
}

// Example output: "$5,000.00 MONTHLY" or "$500.00 WEEKLY"
```

### 4. Cache Available Categories and Frequencies
Fetch available categories and frequencies once and cache them:

```typescript
let cachedCategories: string[] | null = null;
let cachedFrequencies: string[] | null = null;

async function getCategories(token: string): Promise<string[]> {
  if (cachedCategories) {
    return cachedCategories;
  }
  
  const response = await axios.get(
    `${API_BASE_URL}/incomesource/available-categories`,
    { headers: { 'Authorization': `Bearer ${token}` } }
  );
  
  cachedCategories = response.data.data;
  return cachedCategories;
}
```

### 5. Use Loading States
Always show loading indicators during API calls:

```typescript
const [loading, setLoading] = useState(false);

async function createIncomeSource(data: CreateIncomeSourceDto) {
  setLoading(true);
  try {
    await createIncomeSource(token, data);
    // Success
  } finally {
    setLoading(false);
  }
}

// In component
{loading ? <Spinner /> : <SubmitButton />}
```

### 6. Handle Empty States
Show appropriate messages when no income sources exist:

```tsx
{incomeSources.length === 0 ? (
  <div>No income sources found. Create your first income source!</div>
) : (
  <IncomeSourceList sources={incomeSources} />
)}
```

### 7. Format Currency Properly
Always format currency amounts for display:

```typescript
function formatCurrency(amount: number, currency: string = 'USD'): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: currency
  }).format(amount);
}

// Usage
formatCurrency(5000.00, 'USD'); // "$5,000.00"
formatCurrency(5000.00, 'EUR'); // "€5,000.00"
```

### 8. Group Income by Category
Use the category breakdown endpoint for visualization:

```typescript
async function getIncomeBreakdown(token: string) {
  const response = await axios.get(
    `${API_BASE_URL}/incomesource/income-by-category`,
    { headers: { 'Authorization': `Bearer ${token}` } }
  );
  
  return response.data.data;
  // Returns: { "PRIMARY": 5000, "SIDE_HUSTLE": 2000, ... }
}
```

---

## Frequency to Monthly Conversion

The system automatically converts all income frequencies to monthly equivalents:

| Frequency | Monthly Conversion | Example |
|-----------|-------------------|---------|
| `WEEKLY` | Amount × 4.33 | $500/week = $2,165/month |
| `BI_WEEKLY` | Amount × 2.17 | $1,000/bi-weekly = $2,170/month |
| `MONTHLY` | Amount × 1 | $5,000/month = $5,000/month |
| `QUARTERLY` | Amount ÷ 3 | $15,000/quarter = $5,000/month |
| `ANNUALLY` | Amount ÷ 12 | $60,000/year = $5,000/month |

**Note:** The `monthlyAmount` field in the response contains the automatically calculated monthly equivalent.

---

## Summary

This documentation covers all Income Source API endpoints with:
- ✅ Complete endpoint reference
- ✅ Request/response examples
- ✅ TypeScript interfaces
- ✅ Frontend integration examples
- ✅ Error handling patterns
- ✅ Best practices

For additional help or questions, refer to the main API documentation or contact the development team.


