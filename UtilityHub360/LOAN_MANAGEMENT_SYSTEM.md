# 🏦 Loan Management System SaaS

A comprehensive loan management system built with CQRS architecture, Entity Framework, and Web API.

## 🚀 Features Implemented

### 1. Borrower Management
- ✅ Borrower registration with personal details
- ✅ Credit score and loan history tracking
- ✅ Multiple active/closed loans per borrower
- ✅ Borrower status management (Active, Blacklisted, Inactive)
- ✅ Advanced filtering and search capabilities

### 2. Loan Management
- ✅ Loan creation with comprehensive details
- ✅ Support for multiple loan types (Personal, Business, Mortgage, etc.)
- ✅ Flexible repayment frequencies (Daily, Weekly, Monthly)
- ✅ Multiple amortization types (Flat, Reducing Balance, Compound)
- ✅ Automatic repayment schedule generation
- ✅ Loan status tracking (Active, Closed, Defaulted)

### 3. Payment Tracking
- ✅ Manual payment recording
- ✅ Automatic loan balance updates
- ✅ Penalty calculation for overdue payments
- ✅ Prepayment support
- ✅ Payment method tracking

### 4. Interest Calculation
- ✅ Flat Interest: `(Principal × Rate × Term)`
- ✅ Reducing Balance: `(Outstanding balance × Rate × Time)`
- ✅ Compound Interest: `(Principal × (1 + Rate)^Time) − Principal`
- ✅ Automatic penalty calculation

### 5. Reports & Analytics
- ✅ Loan Portfolio Summary
- ✅ Outstanding Balances Report
- ✅ Delinquent Accounts Report
- ✅ Interest Revenue Analysis
- ✅ Overdue Loans Tracking

## 📊 Database Schema

### Core Entities
- **Borrowers**: Personal information, credit scores, status
- **Loans**: Loan details, terms, status
- **RepaymentSchedules**: Payment schedules with principal/interest breakdown
- **Payments**: Payment records and methods
- **Penalties**: Overdue payment penalties
- **Notifications**: SMS/Email notifications (ready for integration)

## 🔧 API Endpoints

### Borrower Management
```
GET    /api/loan-management/borrowers
POST   /api/loan-management/borrowers
```

### Loan Management
```
GET    /api/loan-management/loans
POST   /api/loan-management/loans
```

### Payment Management
```
POST   /api/loan-management/payments
```

### Reports & Analytics
```
GET    /api/loan-management/portfolio
```

## 💡 Key Features

### Interest Calculation Service
- **Flat Interest**: Simple calculation for short-term loans
- **Reducing Balance**: Progressive interest reduction
- **Compound Interest**: Interest on interest calculation
- **Penalty Calculation**: Automatic overdue penalty application

### CQRS Architecture
- **Commands**: CreateBorrower, CreateLoan, RecordPayment
- **Queries**: GetAllBorrowers, GetAllLoans, GetLoanPortfolio
- **Handlers**: Dedicated handlers for each operation
- **MediatR**: Decoupled command/query handling

### Advanced Features
- **Automatic Schedule Generation**: Based on loan terms
- **Penalty Management**: Automatic overdue penalty calculation
- **Portfolio Analytics**: Comprehensive loan portfolio insights
- **Flexible Filtering**: Multiple filter options for all queries

## 🛠 Technical Stack

- **.NET Framework 4.8**
- **Entity Framework 6.4.4**
- **Web API 2**
- **CQRS Pattern**
- **MediatR**
- **AutoMapper**
- **Swagger Documentation**

## 📈 Business Logic

### Loan Creation Process
1. Validate borrower exists
2. Create loan record
3. Generate repayment schedule
4. Calculate interest based on amortization type
5. Set up payment tracking

### Payment Processing
1. Record payment details
2. Update loan balance
3. Mark schedule as paid (if applicable)
4. Apply penalties for overdue payments
5. Update loan status

### Portfolio Analytics
- Total loans count by status
- Outstanding balance calculations
- Interest earned vs projected
- Overdue loans identification
- Revenue analysis

## 🔮 Future Enhancements

### Ready for Integration
- **SMS/Email Notifications**: Notification entity ready
- **Payment Gateways**: Payment method tracking implemented
- **Accounting Integration**: Transaction export capability
- **Multi-branch Support**: Branch filtering in portfolio queries

### Additional Features
- **Document Management**: Loan documents and contracts
- **Credit Scoring**: Advanced credit assessment
- **Risk Management**: Loan risk analysis
- **Compliance**: Regulatory reporting
- **Dashboard**: Real-time analytics dashboard

## 🚀 Getting Started

1. **Database Setup**: Run Entity Framework migrations
2. **API Testing**: Use Swagger UI at `/swagger`
3. **Sample Data**: Create borrowers and loans via API
4. **Portfolio View**: Check loan portfolio analytics

## 📋 Sample API Usage

### Create a Borrower
```json
POST /api/loan-management/borrowers
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phone": "+1234567890",
  "address": "123 Main St, City, State",
  "governmentId": "SSN123456789",
  "creditScore": 750,
  "status": "Active"
}
```

### Create a Loan
```json
POST /api/loan-management/loans
{
  "borrowerId": 1,
  "loanType": "Personal",
  "principalAmount": 10000.00,
  "interestRate": 12.5,
  "termMonths": 24,
  "repaymentFrequency": "Monthly",
  "amortizationType": "Reducing",
  "startDate": "2024-01-01"
}
```

### Record a Payment
```json
POST /api/loan-management/payments
{
  "loanId": 1,
  "scheduleId": 1,
  "amountPaid": 500.00,
  "paymentMethod": "Bank Transfer",
  "notes": "Monthly payment"
}
```

## 🎯 Business Value

- **Automated Processing**: Reduces manual loan management tasks
- **Accurate Calculations**: Precise interest and penalty calculations
- **Real-time Analytics**: Instant portfolio insights
- **Scalable Architecture**: CQRS pattern for future growth
- **Integration Ready**: Built for external system integration
- **Compliance Ready**: Audit trail and reporting capabilities

This loan management system provides a solid foundation for a comprehensive SaaS lending platform with room for extensive customization and feature expansion.
