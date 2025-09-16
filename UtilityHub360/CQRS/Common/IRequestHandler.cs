using System.Threading.Tasks;

namespace UtilityHub360.CQRS.Common
{
    /// <summary>
    /// Base interface for request handlers
    /// </summary>
    /// <typeparam name="TRequest">The type of request</typeparam>
    /// <typeparam name="TResponse">The type of response</typeparam>
    public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(TRequest request);
    }

    /// <summary>
    /// Base interface for request handlers that don't return a value
    /// </summary>
    /// <typeparam name="TRequest">The type of request</typeparam>
    public interface IRequestHandler<in TRequest> where TRequest : IRequest
    {
        Task<Unit> Handle(TRequest request);
    }

    /// <summary>
    /// Represents a unit value (void)
    /// </summary>
    public class Unit
    {
        private static readonly Unit _value = new Unit();
        public static Unit Value { get { return _value; } }
    }
}
