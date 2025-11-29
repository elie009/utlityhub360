# 🚀 Onboarding Wizard API Documentation

## 📋 Overview

The Onboarding Wizard API provides a guided step-by-step process for new users to set up their financial profile in UtilityHub360. This API implements the UX improvements identified in the disposable amount flow analysis, specifically addressing the "Onboarding Complexity" issue with a guided wizard with progress indicators.

## 🎯 Features

- **6-Step Guided Wizard**: Welcome → Income → Bills → Loans → Expenses → Dashboard Tour
- **Progress Tracking**: Real-time progress indicators and completion percentages
- **Flexible Flow**: Users can skip optional steps (bills, loans, expenses)
- **Batch Operations**: Complete all steps at once or individually
- **Smart Defaults**: Pre-filled values and suggestions for better UX
- **Progress Persistence**: Save progress and resume later

---

## 🔗 Base URL

```
http://localhost:5000/api/Onboarding
```

---

## 🔐 Authentication

All endpoints require JWT authentication. Include the Bearer token in the Authorization header:

```
Authorization: Bearer {your-jwt-token}
```

---

## 📊 API Endpoints

### 1. Get Onboarding Progress

**GET** `/api/Onboarding/progress`

Get the current user's onboarding progress and step details. This endpoint provides all the information needed for the frontend, including completion status, current step, and detailed step information.

#### Response
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "userId": "user123",
    "currentStep": 3,
    "totalSteps": 6,
    "completionPercentage": 50.0,
    "isCompleted": false,
    "startedAt": "2025-01-11T10:00:00Z",
    "completedAt": null,
    "steps": [
      {
        "stepNumber": 1,
        "title": "Welcome & Setup",
        "description": "Get to know UtilityHub360 and set your preferences",
        "isCompleted": true,
        "isCurrent": false,
        "icon": "👋",
        "color": "#22c55e"
      },
      {
        "stepNumber": 2,
        "title": "Add Income Sources",
        "description": "Tell us about your monthly income sources",
        "isCompleted": true,
        "isCurrent": false,
        "icon": "💰",
        "color": "#22c55e"
      },
      {
        "stepNumber": 3,
        "title": "Add Fixed Bills",
        "description": "Add your recurring monthly bills and expenses",
        "isCompleted": false,
        "isCurrent": true,
        "icon": "📋",
        "color": "#f59e0b"
      }
    ]
  }
}
```

### 2. Start Onboarding

**POST** `/api/Onboarding/start`

Initialize the onboarding process for a new user.

#### Response
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "userId": "user123",
    "currentStep": 1,
    "totalSteps": 6,
    "completionPercentage": 0.0,
    "isCompleted": false,
    "startedAt": "2025-01-11T10:00:00Z",
    "completedAt": null,
    "steps": [...]
  }
}
```

### 3. Update Current Step

**PUT** `/api/Onboarding/current-step/{stepNumber}`

Update the current step in the onboarding process.

#### Parameters
- `stepNumber` (int): Step number (1-6)

#### Response
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "userId": "user123",
    "currentStep": 3,
    "totalSteps": 6,
    "completionPercentage": 50.0,
    "isCompleted": false,
    "startedAt": "2025-01-11T10:00:00Z",
    "completedAt": null,
    "steps": [...]
  }
}
```

---

## 🎯 Step Completion Endpoints

### 4. Complete Welcome Step

**POST** `/api/Onboarding/complete-welcome`

Complete the welcome and preferences setup step.

#### Request Body
```json
{
  "preferredCurrency": "PHP",
  "financialGoal": "SAVINGS",
  "monthlyIncomeTarget": 50000.00,
  "monthlyExpenseTarget": 35000.00,
  "savingsGoalAmount": 100000.00,
  "savingsGoalDate": "2025-12-31T00:00:00Z"
}
```

#### Response
```json
{
  "success": true,
  "message": "Welcome setup completed successfully!",
  "data": {
    "success": true,
    "message": "Welcome setup completed successfully!",
    "itemsCreated": 0,
    "createdItems": ["User preferences saved"],
    "progress": {
      "userId": "user123",
      "currentStep": 2,
      "totalSteps": 6,
      "completionPercentage": 16.67,
      "isCompleted": false,
      "steps": [...]
    }
  }
}
```

### 5. Complete Income Step

**POST** `/api/Onboarding/complete-income`

Complete the income sources setup step.

#### Request Body
```json
{
  "incomeSources": [
    {
      "name": "Monthly Salary",
      "amount": 45000.00,
      "frequency": "MONTHLY",
      "category": "PRIMARY",
      "description": "Primary employment income",
      "isActive": true
    },
    {
      "name": "Freelance Work",
      "amount": 5000.00,
      "frequency": "MONTHLY",
      "category": "SIDE_HUSTLE",
      "description": "Part-time freelance projects",
      "isActive": true
    }
  ]
}
```

#### Response
```json
{
  "success": true,
  "message": "Income setup completed! Created 2 income source(s).",
  "data": {
    "success": true,
    "message": "Income setup completed! Created 2 income source(s).",
    "itemsCreated": 2,
    "createdItems": [
      "Income source: Monthly Salary",
      "Income source: Freelance Work"
    ],
    "progress": {
      "userId": "user123",
      "currentStep": 3,
      "totalSteps": 6,
      "completionPercentage": 33.33,
      "isCompleted": false,
      "steps": [...]
    }
  }
}
```

### 6. Complete Bills Step

**POST** `/api/Onboarding/complete-bills`

Complete the bills setup step.

#### Request Body
```json
{
  "bills": [
    {
      "billName": "Meralco",
      "billType": "utility",
      "amount": 2500.00,
      "dueDate": "2025-01-15T00:00:00Z",
      "frequency": "monthly",
      "description": "Electricity bill",
      "isActive": true
    },
    {
      "billName": "PLDT",
      "billType": "utility",
      "amount": 1500.00,
      "dueDate": "2025-01-20T00:00:00Z",
      "frequency": "monthly",
      "description": "Internet bill",
      "isActive": true
    }
  ],
  "skipBills": false
}
```

#### Response
```json
{
  "success": true,
  "message": "Bills setup completed! Created 2 bill(s).",
  "data": {
    "success": true,
    "message": "Bills setup completed! Created 2 bill(s).",
    "itemsCreated": 2,
    "createdItems": [
      "Bill: Meralco",
      "Bill: PLDT"
    ],
    "progress": {
      "userId": "user123",
      "currentStep": 4,
      "totalSteps": 6,
      "completionPercentage": 50.0,
      "isCompleted": false,
      "steps": [...]
    }
  }
}
```

### 7. Complete Loans Step

**POST** `/api/Onboarding/complete-loans`

Complete the loans setup step (optional).

#### Request Body
```json
{
  "loans": [
    {
      "loanName": "Car Loan",
      "loanType": "vehicle",
      "principalAmount": 500000.00,
      "monthlyPayment": 15000.00,
      "interestRate": 8.5,
      "startDate": "2024-06-01T00:00:00Z",
      "endDate": "2027-06-01T00:00:00Z",
      "description": "Vehicle financing",
      "isActive": true
    }
  ],
  "skipLoans": false
}
```

#### Response
```json
{
  "success": true,
  "message": "Loans setup completed! Created 1 loan(s).",
  "data": {
    "success": true,
    "message": "Loans setup completed! Created 1 loan(s).",
    "itemsCreated": 1,
    "createdItems": [
      "Loan: Car Loan"
    ],
    "progress": {
      "userId": "user123",
      "currentStep": 5,
      "totalSteps": 6,
      "completionPercentage": 66.67,
      "isCompleted": false,
      "steps": [...]
    }
  }
}
```

### 8. Complete Variable Expenses Step

**POST** `/api/Onboarding/complete-expenses`

Complete the variable expenses setup step.

#### Request Body
```json
{
  "expenses": [
    {
      "description": "Weekly grocery shopping",
      "amount": 2500.00,
      "category": "GROCERIES",
      "expenseDate": "2025-01-10T00:00:00Z",
      "merchant": "SM Supermarket",
      "paymentMethod": "CARD",
      "notes": "Weekly groceries"
    },
    {
      "description": "Gas for car",
      "amount": 1500.00,
      "category": "TRANSPORTATION",
      "expenseDate": "2025-01-09T00:00:00Z",
      "merchant": "Shell",
      "paymentMethod": "CARD",
      "notes": "Weekly fuel"
    }
  ],
  "skipExpenses": false
}
```

#### Response
```json
{
  "success": true,
  "message": "Variable expenses setup completed! Created 2 expense(s).",
  "data": {
    "success": true,
    "message": "Variable expenses setup completed! Created 2 expense(s).",
    "itemsCreated": 2,
    "createdItems": [
      "Expense: Weekly grocery shopping",
      "Expense: Gas for car"
    ],
    "progress": {
      "userId": "user123",
      "currentStep": 6,
      "totalSteps": 6,
      "completionPercentage": 83.33,
      "isCompleted": false,
      "steps": [...]
    }
  }
}
```

### 9. Complete Dashboard Tour

**POST** `/api/Onboarding/complete-tour`

Complete the dashboard tour step.

#### Request Body
```json
{
  "tourCompleted": true,
  "viewedSections": ["hero-card", "income-chart", "expenses-breakdown"],
  "timeSpentSeconds": 180
}
```

#### Response
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "userId": "user123",
    "currentStep": 6,
    "totalSteps": 6,
    "completionPercentage": 100.0,
    "isCompleted": true,
    "startedAt": "2025-01-11T10:00:00Z",
    "completedAt": "2025-01-11T10:15:00Z",
    "steps": [...]
  }
}
```

---

## 🔄 Complete Onboarding

### 10. Complete All Steps

**POST** `/api/Onboarding/complete-all`

Complete the entire onboarding process in one request.

#### Request Body
```json
{
  "welcomeSetup": {
    "preferredCurrency": "PHP",
    "financialGoal": "SAVINGS",
    "monthlyIncomeTarget": 50000.00,
    "monthlyExpenseTarget": 35000.00,
    "savingsGoalAmount": 100000.00,
    "savingsGoalDate": "2025-12-31T00:00:00Z"
  },
  "incomeSetup": {
    "incomeSources": [
      {
        "name": "Monthly Salary",
        "amount": 45000.00,
        "frequency": "MONTHLY",
        "category": "PRIMARY",
        "description": "Primary employment income",
        "isActive": true
      }
    ]
  },
  "billsSetup": {
    "bills": [
      {
        "billName": "Meralco",
        "billType": "utility",
        "amount": 2500.00,
        "dueDate": "2025-01-15T00:00:00Z",
        "frequency": "monthly",
        "description": "Electricity bill",
        "isActive": true
      }
    ],
    "skipBills": false
  },
  "loansSetup": {
    "loans": [],
    "skipLoans": true
  },
  "variableExpensesSetup": {
    "expenses": [],
    "skipExpenses": true
  },
  "dashboardTour": {
    "tourCompleted": true,
    "viewedSections": ["hero-card", "income-chart"],
    "timeSpentSeconds": 120
  }
}
```

#### Response
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "userId": "user123",
    "currentStep": 6,
    "totalSteps": 6,
    "completionPercentage": 100.0,
    "isCompleted": true,
    "startedAt": "2025-01-11T10:00:00Z",
    "completedAt": "2025-01-11T10:15:00Z",
    "steps": [...]
  }
}
```

---

## ⚡ Utility Endpoints

### 11. Skip Onboarding

**POST** `/api/Onboarding/skip`

Skip the onboarding process entirely.

#### Response
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "userId": "user123",
    "currentStep": 6,
    "totalSteps": 6,
    "completionPercentage": 100.0,
    "isCompleted": true,
    "startedAt": "2025-01-11T10:00:00Z",
    "completedAt": "2025-01-11T10:15:00Z",
    "steps": [...]
  }
}
```

### 12. Reset Onboarding

**POST** `/api/Onboarding/reset`

Reset the onboarding process to start over.

#### Response
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "userId": "user123",
    "currentStep": 1,
    "totalSteps": 6,
    "completionPercentage": 0.0,
    "isCompleted": false,
    "startedAt": "2025-01-11T10:15:00Z",
    "completedAt": null,
    "steps": [...]
  }
}
```


---

## 📝 Data Models

### OnboardingProgressDto
```typescript
interface OnboardingProgressDto {
  userId: string;
  currentStep: number;
  totalSteps: number;
  completionPercentage: number;
  isCompleted: boolean;
  startedAt: string;
  completedAt?: string;
  steps: OnboardingStepDto[];
}
```

### OnboardingStepDto
```typescript
interface OnboardingStepDto {
  stepNumber: number;
  title: string;
  description: string;
  isCompleted: boolean;
  isCurrent: boolean;
  icon?: string;
  color?: string;
}
```

### WelcomeSetupDto
```typescript
interface WelcomeSetupDto {
  preferredCurrency: string; // "PHP", "USD", etc.
  financialGoal: string; // "SAVINGS", "DEBT_FREEDOM", "EMERGENCY_FUND", "INVESTMENT"
  monthlyIncomeTarget?: number;
  monthlyExpenseTarget?: number;
  savingsGoalAmount?: number;
  savingsGoalDate?: string;
}
```

### IncomeSourceSetupDto
```typescript
interface IncomeSourceSetupDto {
  name: string;
  amount: number;
  frequency: string; // "WEEKLY", "BI_WEEKLY", "MONTHLY", "QUARTERLY", "ANNUALLY"
  category: string; // "PRIMARY", "SECONDARY", "SIDE_HUSTLE", "BONUS", "INVESTMENT"
  description?: string;
  isActive: boolean;
}
```

### BillSetupDto
```typescript
interface BillSetupDto {
  billName: string;
  billType: string; // "utility", "rent", "insurance", etc.
  amount: number;
  dueDate: string;
  frequency: string; // "weekly", "monthly", "quarterly", "annually"
  description?: string;
  isActive: boolean;
}
```

### LoanSetupDto
```typescript
interface LoanSetupDto {
  loanName: string;
  loanType: string; // "vehicle", "home", "personal", etc.
  principalAmount: number;
  monthlyPayment: number;
  interestRate: number;
  startDate: string;
  endDate: string;
  description?: string;
  isActive: boolean;
}
```

### VariableExpenseSetupDto
```typescript
interface VariableExpenseSetupDto {
  description: string;
  amount: number;
  category: string; // "GROCERIES", "TRANSPORTATION", "FOOD", "ENTERTAINMENT", "SHOPPING"
  expenseDate: string;
  merchant?: string;
  paymentMethod?: string;
  notes?: string;
}
```

---

## 🎨 Frontend Integration Examples

### React/TypeScript Example

```typescript
// Onboarding hook
const useOnboarding = () => {
  const [progress, setProgress] = useState<OnboardingProgressDto | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const getProgress = async () => {
    setIsLoading(true);
    try {
      const response = await fetch('/api/Onboarding/progress', {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });
      const result = await response.json();
      setProgress(result.data);
    } catch (error) {
      console.error('Failed to get onboarding progress:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const completeStep = async (stepNumber: number, stepData: any) => {
    setIsLoading(true);
    try {
      const endpoint = `/api/Onboarding/complete-${getStepEndpoint(stepNumber)}`;
      const response = await fetch(endpoint, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(stepData)
      });
      const result = await response.json();
      setProgress(result.data.progress);
      return result;
    } catch (error) {
      console.error('Failed to complete step:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  return { progress, isLoading, getProgress, completeStep };
};

// Onboarding component
const OnboardingWizard = () => {
  const { progress, isLoading, completeStep } = useOnboarding();
  const [currentStep, setCurrentStep] = useState(1);

  useEffect(() => {
    getProgress();
  }, []);

  const handleStepComplete = async (stepData: any) => {
    try {
      await completeStep(currentStep, stepData);
      setCurrentStep(currentStep + 1);
    } catch (error) {
      // Handle error
    }
  };

  if (isLoading) return <LoadingSpinner />;

  return (
    <div className="onboarding-wizard">
      <ProgressBar 
        currentStep={progress?.currentStep || 1}
        totalSteps={progress?.totalSteps || 6}
        percentage={progress?.completionPercentage || 0}
      />
      
      <StepContent 
        stepNumber={currentStep}
        onComplete={handleStepComplete}
      />
    </div>
  );
};
```

### Vue.js Example

```vue
<template>
  <div class="onboarding-wizard">
    <div class="progress-bar">
      <div class="progress-fill" :style="{ width: `${progress?.completionPercentage || 0}%` }"></div>
    </div>
    
    <div class="step-indicators">
      <div 
        v-for="step in progress?.steps" 
        :key="step.stepNumber"
        :class="['step-indicator', { 
          'completed': step.isCompleted, 
          'current': step.isCurrent 
        }]"
      >
        <span class="step-icon">{{ step.icon }}</span>
        <span class="step-title">{{ step.title }}</span>
      </div>
    </div>

    <component 
      :is="currentStepComponent" 
      @step-complete="handleStepComplete"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useOnboardingStore } from '@/stores/onboarding';

const onboardingStore = useOnboardingStore();
const currentStep = ref(1);

const progress = computed(() => onboardingStore.progress);

const currentStepComponent = computed(() => {
  const components = {
    1: 'WelcomeStep',
    2: 'IncomeStep',
    3: 'BillsStep',
    4: 'LoansStep',
    5: 'ExpensesStep',
    6: 'TourStep'
  };
  return components[currentStep.value];
});

const handleStepComplete = async (stepData: any) => {
  try {
    await onboardingStore.completeStep(currentStep.value, stepData);
    currentStep.value++;
  } catch (error) {
    console.error('Failed to complete step:', error);
  }
};

onMounted(() => {
  onboardingStore.fetchProgress();
});
</script>
```

---

## 🚀 Getting Started

### 1. Start Onboarding Flow

```bash
# Start the onboarding process
curl -X POST "http://localhost:5000/api/Onboarding/start" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "Content-Type: application/json"
```

### 2. Complete Steps Sequentially

```bash
# Step 1: Welcome & Preferences
curl -X POST "http://localhost:5000/api/Onboarding/complete-welcome" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "preferredCurrency": "PHP",
    "financialGoal": "SAVINGS",
    "monthlyIncomeTarget": 50000.00,
    "savingsGoalAmount": 100000.00
  }'

# Step 2: Income Sources
curl -X POST "http://localhost:5000/api/Onboarding/complete-income" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "incomeSources": [
      {
        "name": "Monthly Salary",
        "amount": 45000.00,
        "frequency": "MONTHLY",
        "category": "PRIMARY",
        "isActive": true
      }
    ]
  }'
```

### 3. Check Progress

```bash
# Get current progress
curl -X GET "http://localhost:5000/api/Onboarding/progress" \
  -H "Authorization: Bearer {your-jwt-token}"
```

---

## 🎯 UX Benefits

This onboarding implementation addresses the key UX issues identified in the disposable amount flow analysis:

### ✅ **Solved: Onboarding Complexity**
- **Before**: Users had to navigate multiple sections manually
- **After**: Guided 6-step wizard with clear progression

### ✅ **Progress Indicators**
- Real-time completion percentage
- Visual step indicators with icons and colors
- Clear next steps guidance

### ✅ **Flexible Flow**
- Users can skip optional steps (bills, loans, expenses)
- Resume onboarding at any time
- Complete all steps at once or individually

### ✅ **Smart Defaults**
- Pre-filled currency and common values
- Suggested categories and frequencies
- Contextual help and descriptions

### ✅ **Mobile-Friendly**
- Responsive design considerations
- Touch-friendly interface
- Optimized for mobile-first experience

---

## 🔧 Error Handling

All endpoints return consistent error responses:

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "Amount must be greater than 0",
    "Due date is required"
  ]
}
```

### Common Error Scenarios

1. **Validation Errors**: Invalid input data
2. **Authentication Errors**: Missing or invalid JWT token
3. **Business Logic Errors**: Duplicate entries, invalid references
4. **Server Errors**: Database connection issues, internal errors

---

## 📈 Performance Considerations

- **Lazy Loading**: Steps are loaded only when needed
- **Batch Operations**: Multiple items can be created in single request
- **Progress Caching**: Progress is cached for quick access
- **Optimistic Updates**: UI updates immediately, syncs in background

---

## 🔄 Integration with Disposable Amount Flow

The onboarding wizard seamlessly integrates with the existing disposable amount calculation:

1. **Income Sources** → Used in disposable amount calculation
2. **Bills** → Counted as fixed expenses
3. **Loans** → Included in monthly payment calculations
4. **Variable Expenses** → Tracked for spending patterns
5. **Dashboard Tour** → Introduces users to disposable amount insights

Once onboarding is complete, users immediately see their disposable amount dashboard with all the data they've entered during the wizard.

---

**🎉 The onboarding wizard transforms the complex setup process into an intuitive, guided experience that gets users to their financial dashboard faster and with better understanding of the system!**

---

## ✅ Clean API Summary

The cleaned onboarding API now provides **12 focused endpoints** instead of 15:

### **Core Progress (1 endpoint)**
- `GET /api/Onboarding/progress` - Single source of truth for all progress information

### **Step Management (2 endpoints)**  
- `POST /api/Onboarding/start` - Initialize onboarding
- `PUT /api/Onboarding/current-step/{stepNumber}` - Update current step

### **Step Completion (6 endpoints)**
- `POST /api/Onboarding/complete-welcome` - Complete welcome step
- `POST /api/Onboarding/complete-income` - Complete income setup
- `POST /api/Onboarding/complete-bills` - Complete bills setup  
- `POST /api/Onboarding/complete-loans` - Complete loans setup
- `POST /api/Onboarding/complete-expenses` - Complete expenses setup
- `POST /api/Onboarding/complete-tour` - Complete dashboard tour

### **Batch & Utility (3 endpoints)**
- `POST /api/Onboarding/complete-all` - Complete all steps at once
- `POST /api/Onboarding/skip` - Skip onboarding entirely
- `POST /api/Onboarding/reset` - Reset onboarding progress

### **Benefits of Cleanup**
- ✅ **Simpler API**: 12 endpoints instead of 15
- ✅ **Less Code**: Removed ~150 lines of redundant code  
- ✅ **Single Source of Truth**: All status info comes from `/progress` endpoint
- ✅ **Easier Maintenance**: Fewer endpoints to test and maintain
- ✅ **Better Performance**: Frontend makes fewer API calls
