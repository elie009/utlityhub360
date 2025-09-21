using AutoMapper;
using UtilityHub360.DTOs;
using UtilityHub360.Models;

namespace UtilityHub360.Mapping
{
    /// <summary>
    /// AutoMapper profile for mapping between entities and DTOs
    /// </summary>
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<User, UserDto>();
            CreateMap<RegisterDataDto, User>();
            CreateMap<UpdateUserDto, User>();

            // Loan mappings
            CreateMap<Loan, LoanDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Name : ""));
            CreateMap<LoanApplicationDto, Loan>();

            // Payment mappings
            CreateMap<Payment, PaymentDto>();
            CreateMap<PaymentDto, Payment>();

            // Loan Application mappings
            CreateMap<LoanApplication, LoanApplicationDto>();
            CreateMap<LoanApplicationDto, LoanApplication>();

            // Notification mappings
            CreateMap<Notification, NotificationDto>();
            CreateMap<NotificationDto, Notification>();

            // Transaction mappings
            CreateMap<Transaction, TransactionDto>();
            CreateMap<TransactionDto, Transaction>();

            // Repayment Schedule mappings
            CreateMap<RepaymentSchedule, RepaymentScheduleDto>();
            CreateMap<RepaymentScheduleDto, RepaymentSchedule>();
        }
    }
}

