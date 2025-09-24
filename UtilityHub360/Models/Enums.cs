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
}

