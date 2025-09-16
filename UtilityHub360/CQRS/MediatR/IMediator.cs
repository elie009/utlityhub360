using System.Threading.Tasks;
using UtilityHub360.CQRS.Common;

namespace UtilityHub360.CQRS.MediatR
{
    /// <summary>
    /// Mediator interface for sending requests
    /// </summary>
    public interface IMediator
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request);
        Task<Unit> Send(IRequest request);
    }
}
