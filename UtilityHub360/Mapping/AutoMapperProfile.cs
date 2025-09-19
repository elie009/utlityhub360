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
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();

            // Borrower mappings
            CreateMap<Borrower, BorrowerDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FirstName + " " + src.LastName));

            // Loan mappings
            CreateMap<Loan, LoanDto>()
                .ForMember(dest => dest.BorrowerName, opt => opt.MapFrom(src => src.Borrower != null ? src.Borrower.FirstName + " " + src.Borrower.LastName : ""));

            // Payment mappings
            CreateMap<LoanPayment, PaymentDto>();

            // Loan Portfolio mapping
            CreateMap<Loan, LoanDto>()
                .ForMember(dest => dest.BorrowerName, opt => opt.MapFrom(src => src.Borrower != null ? src.Borrower.FirstName + " " + src.Borrower.LastName : ""));
        }
    }
}
