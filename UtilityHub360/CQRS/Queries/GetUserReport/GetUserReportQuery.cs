using MediatR;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries.GetUserReport
{
    public class GetUserReportQuery : IRequest<UserReportDto>
    {
        public int UserId { get; set; }
        public string? Period { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

