namespace UtilityHub360.DTOs
{
    public class PaymentMethodDto
    {
        public string Type { get; set; } = string.Empty; // BANK_TRANSFER, CARD, WALLET, CASH

        public PaymentMethodDetailsDto? Details { get; set; }
    }

    public class PaymentMethodDetailsDto
    {
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? CardLast4 { get; set; }
        public string? WalletProvider { get; set; }
    }
}
