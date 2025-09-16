using System;
using System.Collections.Generic;
using AutoMapper;
using UtilityHub360.CQRS.Commands;
using UtilityHub360.CQRS.Common;
using UtilityHub360.CQRS.MediatR;
using UtilityHub360.CQRS.Queries;
using UtilityHub360.DTOs;
using UtilityHub360.Mapping;
using UtilityHub360.Models;

namespace UtilityHub360.DependencyInjection
{
    /// <summary>
    /// Simple service container for dependency injection
    /// </summary>
    public class ServiceContainer : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();

        public void RegisterSingleton<TInterface, TImplementation>() where TImplementation : class, TInterface
        {
            var instance = Activator.CreateInstance<TImplementation>();
            _services[typeof(TInterface)] = instance;
        }

        public void RegisterTransient<TInterface, TImplementation>() where TImplementation : class, TInterface
        {
            _factories[typeof(TInterface)] = () => Activator.CreateInstance<TImplementation>();
        }

        public void RegisterInstance<TInterface>(TInterface instance)
        {
            _services[typeof(TInterface)] = instance;
        }

        public T GetService<T>()
        {
            var type = typeof(T);
            
            if (_services.ContainsKey(type))
                return (T)_services[type];

            if (_factories.ContainsKey(type))
                return (T)_factories[type]();

            throw new InvalidOperationException("Service of type " + type + " not registered");
        }

        public object GetService(Type type)
        {
            if (_services.ContainsKey(type))
                return _services[type];

            if (_factories.ContainsKey(type))
                return _factories[type]();

            return null; // IServiceProvider expects null when service not found
        }

        public static ServiceContainer CreateDefault()
        {
            var container = new ServiceContainer();

            // Register AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });
            var mapper = config.CreateMapper();
            container.RegisterInstance<IMapper>(mapper);

            // Register DbContext
            container.RegisterTransient<UtilityHubDbContext, UtilityHubDbContext>();

            // Register MediatR
            container.RegisterInstance<IMediator>(new Mediator((IServiceProvider)container));

            // Register Query Handlers
            container.RegisterTransient<IRequestHandler<GetAllUsersQuery, System.Collections.Generic.List<DTOs.UserDto>>, GetAllUsersQueryHandler>();
            container.RegisterTransient<IRequestHandler<GetUserByIdQuery, DTOs.UserDto>, GetUserByIdQueryHandler>();

            // Register Command Handlers
            container.RegisterTransient<IRequestHandler<CreateUserCommand, DTOs.UserDto>, CreateUserCommandHandler>();
            container.RegisterTransient<IRequestHandler<UpdateUserCommand, DTOs.UserDto>, UpdateUserCommandHandler>();
            container.RegisterTransient<IRequestHandler<DeleteUserCommand>, DeleteUserCommandHandler>();

            return container;
        }
    }
}
