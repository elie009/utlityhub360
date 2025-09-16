using System.Collections.Generic;
using UtilityHub360.CQRS.Common;
using UtilityHub360.DTOs;

namespace UtilityHub360.CQRS.Queries
{
    /// <summary>
    /// Query to get all borrowers
    /// </summary>
    public class GetAllBorrowersQuery : IRequest<List<BorrowerDto>>
    {
    }
}