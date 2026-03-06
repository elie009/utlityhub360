namespace UtilityHub360.DTOs
{
    public class BankStatementUploadLimitDto
    {
        public bool CanUpload { get; set; }
        public int CurrentUploads { get; set; }
        public int? UploadLimit { get; set; }
        public int? RemainingUploads { get; set; }
        public bool IsFreeTier { get; set; }
    }
}

