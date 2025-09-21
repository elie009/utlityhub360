using Microsoft.AspNetCore.Mvc;
using MediatR;
using UtilityHub360.DTOs;
using UtilityHub360.CQRS.Commands.RegisterUser;
using UtilityHub360.CQRS.Commands.LoginUser;
using UtilityHub360.CQRS.Queries.GetUserById;

namespace UtilityHub360.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] RegisterDataDto registerData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var command = new RegisterUserCommand
                {
                    Name = registerData.Name,
                    Email = registerData.Email,
                    Phone = registerData.Phone,
                    Password = registerData.Password,
                    Role = "USER" // Default role for new registrations
                };

                var user = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetMe), new { }, user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Authenticate user
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginCredentialsDto credentials)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var command = new LoginUserCommand
                {
                    Email = credentials.Email,
                    Password = credentials.Password
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Get current authenticated user
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetMe()
        {
            try
            {
                // TODO: Get user ID from JWT token
                var userId = 1; // Placeholder - should come from authenticated user
                
                var query = new GetUserByIdQuery { Id = userId };
                var user = await _mediator.Send(query);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    public class LoginResponseDto
    {
        public UserDto User { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}

