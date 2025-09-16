@echo off
echo Compiling UtilityHub360 application...

"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe" /target:library /out:bin\UtilityHub360.dll /reference:bin\EntityFramework.dll /reference:bin\EntityFramework.SqlServer.dll /reference:bin\AutoMapper.dll /reference:bin\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Web.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Web.Extensions.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Net.Http.dll" /reference:"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.ComponentModel.DataAnnotations.dll" ^
Global.asax.cs ^
CQRS\Common\IRequest.cs ^
CQRS\Common\IRequestHandler.cs ^
CQRS\MediatR\IMediator.cs ^
CQRS\MediatR\Mediator.cs ^
CQRS\Commands\CreateUserCommand.cs ^
CQRS\Commands\CreateUserCommandHandler.cs ^
CQRS\Commands\DeleteUserCommand.cs ^
CQRS\Commands\DeleteUserCommandHandler.cs ^
CQRS\Commands\UpdateUserCommand.cs ^
CQRS\Commands\UpdateUserCommandHandler.cs ^
CQRS\Queries\GetAllUsersQuery.cs ^
CQRS\Queries\GetAllUsersQueryHandler.cs ^
CQRS\Queries\GetUserByIdQuery.cs ^
CQRS\Queries\GetUserByIdQueryHandler.cs ^
DTOs\CreateUserDto.cs ^
DTOs\UpdateUserDto.cs ^
DTOs\UserDto.cs ^
DependencyInjection\ServiceContainer.cs ^
Mapping\AutoMapperProfile.cs ^
Models\User.cs ^
Models\UtilityHubDbContext.cs ^
Controllers\HomeController.cs ^
Controllers\UsersController.cs ^
Controllers\ValuesController.cs ^
App_Start\BundleConfig.cs ^
App_Start\FilterConfig.cs ^
App_Start\RouteConfig.cs ^
App_Start\WebApiConfig.cs ^
TestCQRS.aspx.cs ^
TestEF.aspx.cs

if %ERRORLEVEL% EQU 0 (
    echo Compilation successful!
) else (
    echo Compilation failed!
)

pause
