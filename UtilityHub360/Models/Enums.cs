namespace UtilityHub360.Models
{
    public enum UserRole
    {
        USER,
        ADMIN
    }

    public enum LoanStatus
    {
        PENDING,
        APPROVED,
        REJECTED,
        ACTIVE,
        COMPLETED,
        DEFAULTED
    }

    public enum PaymentMethod
    {
        BANK_TRANSFER,
        CARD,
        WALLET,
        CASH
    }

    public enum PaymentStatus
    {
        PENDING,
        COMPLETED,
        FAILED
    }

    public enum RepaymentStatus
    {
        PENDING,
        PAID,
        OVERDUE
    }

    public enum TransactionType
    {
        DISBURSEMENT,
        PAYMENT,
        INTEREST,
        PENALTY
    }

    public enum NotificationType
    {
        PAYMENT_DUE,
        PAYMENT_RECEIVED,
        LOAN_APPROVED,
        LOAN_REJECTED,
        GENERAL
    }

    public enum EmploymentStatus
    {
        employed,
        self_employed,
        unemployed,
        retired,
        student
    }

    public enum ApplicationStatus
    {
        PENDING,
        APPROVED,
        REJECTED
    }

    public enum ExpenseCategory
    {
        // Food & Dining
        FOOD,
        GROCERIES,
        RESTAURANTS,
        COFFEE,
        FAST_FOOD,
        
        // Transportation
        TRANSPORTATION,
        GAS,
        PUBLIC_TRANSPORT,
        TAXI,
        RIDESHARE,
        PARKING,
        CAR_MAINTENANCE,
        
        // Entertainment & Recreation
        ENTERTAINMENT,
        MOVIES,
        GAMES,
        SPORTS,
        HOBBIES,
        
        // Shopping
        SHOPPING,
        CLOTHING,
        ELECTRONICS,
        BOOKS,
        PERSONAL_CARE,
        
        // Health & Medical
        HEALTHCARE,
        MEDICINE,
        FITNESS,
        DOCTOR,
        
        // Education
        EDUCATION,
        COURSES,
        BOOKS_EDUCATION,
        
        // Travel
        TRAVEL,
        HOTEL,
        FLIGHTS,
        VACATION,
        
        // Utilities & Bills
        UTILITIES,
        INTERNET,
        PHONE,
        ELECTRICITY,
        WATER,
        GAS_UTILITY,
        
        // Housing
        HOUSING,
        RENT,
        MORTGAGE,
        HOME_IMPROVEMENT,
        
        // Insurance
        INSURANCE,
        CAR_INSURANCE,
        HEALTH_INSURANCE,
        LIFE_INSURANCE,
        
        // Miscellaneous
        OTHER,
        GIFTS,
        DONATIONS,
        FEES,
        SUBSCRIPTIONS
    }

    public enum BankTransactionType
    {
        CREDIT,
        DEBIT
    }

    public enum SavingsType
    {
        EMERGENCY,
        VACATION,
        INVESTMENT,
        RETIREMENT,
        EDUCATION,
        HOME_DOWN_PAYMENT,
        CAR_PURCHASE,
        WEDDING,
        TRAVEL,
        BUSINESS,
        HEALTH,
        TAX_SAVINGS,
        GENERAL
    }

    public enum SavingsTransactionType
    {
        DEPOSIT,
        WITHDRAWAL,
        TRANSFER,
        INTEREST,
        BONUS
    }

    public enum SavingsCategory
    {
        MONTHLY_SAVINGS,
        BONUS,
        TAX_REFUND,
        GIFT,
        SIDE_INCOME,
        INVESTMENT_RETURN,
        EMERGENCY_WITHDRAWAL,
        PLANNED_EXPENSE,
        TRANSFER,
        OTHER
    }
}

