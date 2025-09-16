using System;

namespace UtilityHub360.CQRS.Common
{
    /// <summary>
    /// Base interface for all requests (Commands and Queries)
    /// </summary>
    public interface IRequest
    {
    }

    /// <summary>
    /// Base interface for requests that return a result
    /// </summary>
    /// <typeparam name="TResponse">The type of response</typeparam>
    public interface IRequest<out TResponse> : IRequest
    {
    }
}
