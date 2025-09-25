# Billing Form Validation Guide

## 📋 Overview

This document provides detailed validation rules and examples for all billing forms in the frontend application.

---

## 🔧 Create Bill Form

### Form Fields & Validation

#### 1. Bill Name
```javascript
// Validation Rules
const billNameValidation = {
  required: true,
  minLength: 1,
  maxLength: 255,
  pattern: /^[a-zA-Z0-9\s\-_.,()]+$/, // Alphanumeric, spaces, hyphens, underscores, periods, commas, parentheses
  errorMessages: {
    required: "Bill name is required",
    minLength: "Bill name must be at least 1 character",
    maxLength: "Bill name cannot exceed 255 characters",
    pattern: "Bill name contains invalid characters"
  }
};

// Example Implementation
const validateBillName = (value) => {
  if (!value || value.trim().length === 0) {
    return "Bill name is required";
  }
  if (value.length > 255) {
    return "Bill name cannot exceed 255 characters";
  }
  if (!/^[a-zA-Z0-9\s\-_.,()]+$/.test(value)) {
    return "Bill name contains invalid characters";
  }
  return null;
};
```

#### 2. Bill Type
```javascript
// Validation Rules
const billTypeOptions = [
  { value: "utility", label: "Utility" },
  { value: "subscription", label: "Subscription" },
  { value: "loan", label: "Loan" },
  { value: "others", label: "Others" }
];

const billTypeValidation = {
  required: true,
  enum: ["utility", "subscription", "loan", "others"],
  errorMessages: {
    required: "Please select a bill type",
    invalid: "Invalid bill type selected"
  }
};

// Example Implementation
const validateBillType = (value) => {
  if (!value) {
    return "Please select a bill type";
  }
  if (!["utility", "subscription", "loan", "others"].includes(value)) {
    return "Invalid bill type selected";
  }
  return null;
};
```

#### 3. Amount
```javascript
// Validation Rules
const amountValidation = {
  required: true,
  min: 0.01,
  max: 999999.99,
  decimalPlaces: 2,
  pattern: /^\d+(\.\d{1,2})?$/,
  errorMessages: {
    required: "Amount is required",
    min: "Amount must be greater than 0",
    max: "Amount cannot exceed $999,999.99",
    decimalPlaces: "Amount can have maximum 2 decimal places",
    pattern: "Please enter a valid amount"
  }
};

// Example Implementation
const validateAmount = (value) => {
  if (!value || value === "") {
    return "Amount is required";
  }
  
  const numValue = parseFloat(value);
  if (isNaN(numValue)) {
    return "Please enter a valid amount";
  }
  
  if (numValue <= 0) {
    return "Amount must be greater than 0";
  }
  
  if (numValue > 999999.99) {
    return "Amount cannot exceed $999,999.99";
  }
  
  if (!/^\d+(\.\d{1,2})?$/.test(value)) {
    return "Amount can have maximum 2 decimal places";
  }
  
  return null;
};
```

#### 4. Due Date
```javascript
// Validation Rules
const dueDateValidation = {
  required: true,
  minDate: new Date(), // Today's date
  maxDate: new Date(Date.now() + 365 * 24 * 60 * 60 * 1000), // 1 year from now
  errorMessages: {
    required: "Due date is required",
    minDate: "Due date must be today or in the future",
    maxDate: "Due date cannot be more than 1 year from now"
  }
};

// Example Implementation
const validateDueDate = (value) => {
  if (!value) {
    return "Due date is required";
  }
  
  const selectedDate = new Date(value);
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  
  if (selectedDate < today) {
    return "Due date must be today or in the future";
  }
  
  const maxDate = new Date(Date.now() + 365 * 24 * 60 * 60 * 1000);
  if (selectedDate > maxDate) {
    return "Due date cannot be more than 1 year from now";
  }
  
  return null;
};
```

#### 5. Frequency
```javascript
// Validation Rules
const frequencyOptions = [
  { value: "monthly", label: "Monthly" },
  { value: "quarterly", label: "Quarterly" },
  { value: "yearly", label: "Yearly" }
];

const frequencyValidation = {
  required: true,
  enum: ["monthly", "quarterly", "yearly"],
  errorMessages: {
    required: "Please select a frequency",
    invalid: "Invalid frequency selected"
  }
};

// Example Implementation
const validateFrequency = (value) => {
  if (!value) {
    return "Please select a frequency";
  }
  if (!["monthly", "quarterly", "yearly"].includes(value)) {
    return "Invalid frequency selected";
  }
  return null;
};
```

#### 6. Notes (Optional)
```javascript
// Validation Rules
const notesValidation = {
  required: false,
  maxLength: 500,
  pattern: /^[a-zA-Z0-9\s\-_.,()!?@#$%&*+=:;'"<>\/\\[\]{}|`~]*$/,
  errorMessages: {
    maxLength: "Notes cannot exceed 500 characters",
    pattern: "Notes contain invalid characters"
  }
};

// Example Implementation
const validateNotes = (value) => {
  if (!value) return null; // Optional field
  
  if (value.length > 500) {
    return "Notes cannot exceed 500 characters";
  }
  
  return null;
};
```

#### 7. Provider (Optional)
```javascript
// Validation Rules
const providerValidation = {
  required: false,
  maxLength: 100,
  pattern: /^[a-zA-Z0-9\s\-_.,()&]+$/,
  errorMessages: {
    maxLength: "Provider name cannot exceed 100 characters",
    pattern: "Provider name contains invalid characters"
  }
};

// Example Implementation
const validateProvider = (value) => {
  if (!value) return null; // Optional field
  
  if (value.length > 100) {
    return "Provider name cannot exceed 100 characters";
  }
  
  return null;
};
```

#### 8. Reference Number (Optional)
```javascript
// Validation Rules
const referenceNumberValidation = {
  required: false,
  maxLength: 100,
  pattern: /^[a-zA-Z0-9\-_.#]+$/,
  errorMessages: {
    maxLength: "Reference number cannot exceed 100 characters",
    pattern: "Reference number contains invalid characters"
  }
};

// Example Implementation
const validateReferenceNumber = (value) => {
  if (!value) return null; // Optional field
  
  if (value.length > 100) {
    return "Reference number cannot exceed 100 characters";
  }
  
  return null;
};
```

---

## 🔄 Update Bill Form

### Form Fields & Validation

The update form uses the same validation rules as the create form, but all fields are optional except for the bill ID in the URL.

```javascript
// Example Implementation
const validateUpdateBill = (formData) => {
  const errors = {};
  
  if (formData.billName !== undefined) {
    errors.billName = validateBillName(formData.billName);
  }
  
  if (formData.billType !== undefined) {
    errors.billType = validateBillType(formData.billType);
  }
  
  if (formData.amount !== undefined) {
    errors.amount = validateAmount(formData.amount);
  }
  
  if (formData.dueDate !== undefined) {
    errors.dueDate = validateDueDate(formData.dueDate);
  }
  
  if (formData.frequency !== undefined) {
    errors.frequency = validateFrequency(formData.frequency);
  }
  
  if (formData.notes !== undefined) {
    errors.notes = validateNotes(formData.notes);
  }
  
  if (formData.provider !== undefined) {
    errors.provider = validateProvider(formData.provider);
  }
  
  if (formData.referenceNumber !== undefined) {
    errors.referenceNumber = validateReferenceNumber(formData.referenceNumber);
  }
  
  return errors;
};
```

---

## 📊 Filter Forms

### Bill Status Filter
```javascript
const statusOptions = [
  { value: "", label: "All Statuses" },
  { value: "PENDING", label: "Pending" },
  { value: "PAID", label: "Paid" },
  { value: "OVERDUE", label: "Overdue" }
];
```

### Bill Type Filter
```javascript
const billTypeFilterOptions = [
  { value: "", label: "All Types" },
  { value: "utility", label: "Utility" },
  { value: "subscription", label: "Subscription" },
  { value: "loan", label: "Loan" },
  { value: "others", label: "Others" }
];
```

### Pagination Controls
```javascript
const paginationValidation = {
  page: {
    min: 1,
    max: 10000,
    errorMessages: {
      min: "Page must be at least 1",
      max: "Page cannot exceed 10,000"
    }
  },
  limit: {
    min: 1,
    max: 100,
    errorMessages: {
      min: "Limit must be at least 1",
      max: "Limit cannot exceed 100"
    }
  }
};
```

---

## 🎯 Complete Form Validation Example

### React Hook Form Example
```javascript
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';

const createBillSchema = yup.object({
  billName: yup
    .string()
    .required('Bill name is required')
    .min(1, 'Bill name must be at least 1 character')
    .max(255, 'Bill name cannot exceed 255 characters')
    .matches(/^[a-zA-Z0-9\s\-_.,()]+$/, 'Bill name contains invalid characters'),
  
  billType: yup
    .string()
    .required('Please select a bill type')
    .oneOf(['utility', 'subscription', 'loan', 'others'], 'Invalid bill type'),
  
  amount: yup
    .number()
    .required('Amount is required')
    .min(0.01, 'Amount must be greater than 0')
    .max(999999.99, 'Amount cannot exceed $999,999.99')
    .test('decimal', 'Amount can have maximum 2 decimal places', value => {
      if (!value) return true;
      return /^\d+(\.\d{1,2})?$/.test(value.toString());
    }),
  
  dueDate: yup
    .date()
    .required('Due date is required')
    .min(new Date(), 'Due date must be today or in the future'),
  
  frequency: yup
    .string()
    .required('Please select a frequency')
    .oneOf(['monthly', 'quarterly', 'yearly'], 'Invalid frequency'),
  
  notes: yup
    .string()
    .max(500, 'Notes cannot exceed 500 characters')
    .nullable(),
  
  provider: yup
    .string()
    .max(100, 'Provider name cannot exceed 100 characters')
    .nullable(),
  
  referenceNumber: yup
    .string()
    .max(100, 'Reference number cannot exceed 100 characters')
    .nullable()
});

const CreateBillForm = () => {
  const { register, handleSubmit, formState: { errors } } = useForm({
    resolver: yupResolver(createBillSchema)
  });

  const onSubmit = async (data) => {
    // Format data for API
    const apiData = {
      ...data,
      dueDate: data.dueDate.toISOString(),
      amount: parseFloat(data.amount)
    };
    
    // Send to API
    try {
      const response = await fetch('/api/bills', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(apiData)
      });
      
      const result = await response.json();
      if (result.success) {
        // Handle success
        console.log('Bill created:', result.data);
      } else {
        // Handle API errors
        console.error('API Error:', result.errors);
      }
    } catch (error) {
      console.error('Network Error:', error);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <div>
        <label>Bill Name</label>
        <input {...register('billName')} />
        {errors.billName && <span>{errors.billName.message}</span>}
      </div>
      
      <div>
        <label>Bill Type</label>
        <select {...register('billType')}>
          <option value="">Select Bill Type</option>
          <option value="utility">Utility</option>
          <option value="subscription">Subscription</option>
          <option value="loan">Loan</option>
          <option value="others">Others</option>
        </select>
        {errors.billType && <span>{errors.billType.message}</span>}
      </div>
      
      <div>
        <label>Amount</label>
        <input type="number" step="0.01" {...register('amount')} />
        {errors.amount && <span>{errors.amount.message}</span>}
      </div>
      
      <div>
        <label>Due Date</label>
        <input type="date" {...register('dueDate')} />
        {errors.dueDate && <span>{errors.dueDate.message}</span>}
      </div>
      
      <div>
        <label>Frequency</label>
        <select {...register('frequency')}>
          <option value="">Select Frequency</option>
          <option value="monthly">Monthly</option>
          <option value="quarterly">Quarterly</option>
          <option value="yearly">Yearly</option>
        </select>
        {errors.frequency && <span>{errors.frequency.message}</span>}
      </div>
      
      <div>
        <label>Notes (Optional)</label>
        <textarea {...register('notes')} />
        {errors.notes && <span>{errors.notes.message}</span>}
      </div>
      
      <div>
        <label>Provider (Optional)</label>
        <input {...register('provider')} />
        {errors.provider && <span>{errors.provider.message}</span>}
      </div>
      
      <div>
        <label>Reference Number (Optional)</label>
        <input {...register('referenceNumber')} />
        {errors.referenceNumber && <span>{errors.referenceNumber.message}</span>}
      </div>
      
      <button type="submit">Create Bill</button>
    </form>
  );
};
```

---

## 🎨 UI/UX Recommendations

### Form Layout
1. **Group related fields** (e.g., bill details, payment info)
2. **Use clear labels** and placeholder text
3. **Show validation errors** immediately below fields
4. **Use appropriate input types** (date, number, email, etc.)
5. **Provide helpful hints** for complex fields

### Error Handling
1. **Display errors clearly** with red text/icons
2. **Show field-level errors** below each input
3. **Display form-level errors** at the top
4. **Provide actionable error messages**
5. **Clear errors** when user starts typing

### Success Feedback
1. **Show success messages** after form submission
2. **Redirect to relevant page** (bills list, bill details)
3. **Use loading states** during API calls
4. **Provide confirmation** for destructive actions

---

## 🔍 Testing Checklist

### Form Validation Testing
- [ ] Test all required field validations
- [ ] Test field length limits
- [ ] Test pattern matching (regex)
- [ ] Test numeric range validations
- [ ] Test date validations
- [ ] Test optional field behavior

### API Integration Testing
- [ ] Test successful form submissions
- [ ] Test API error handling
- [ ] Test network error handling
- [ ] Test authentication errors
- [ ] Test validation error responses

### User Experience Testing
- [ ] Test form usability on mobile
- [ ] Test keyboard navigation
- [ ] Test screen reader compatibility
- [ ] Test form reset functionality
- [ ] Test auto-save (if implemented)
