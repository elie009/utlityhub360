using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UtilityHub360.CQRS.Common;

namespace UtilityHub360.CQRS.MediatR
{
    /// <summary>
    /// Simple Mediator implementation
    /// </summary>
    public class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = _serviceProvider.GetService(handlerType);

            if (handler == null)
                throw new InvalidOperationException("Handler not found for request type " + request.GetType());

            var method = handlerType.GetMethod("Handle");
            var result = method.Invoke(handler, new object[] { request });

            if (result is Task<TResponse> task)
                return await task;

            return (TResponse)result;
        }

        public async Task<Unit> Send(IRequest request)
        {
            var handlerType = typeof(IRequestHandler<>).MakeGenericType(request.GetType());
            var handler = _serviceProvider.GetService(handlerType);

            if (handler == null)
                throw new InvalidOperationException("Handler not found for request type " + request.GetType());

            var method = handlerType.GetMethod("Handle");
            var result = method.Invoke(handler, new object[] { request });

            if (result is Task<Unit> task)
                return await task;

            return Unit.Value;
        }
    }
}
