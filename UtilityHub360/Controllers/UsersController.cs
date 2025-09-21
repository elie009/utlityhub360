using Microsoft.AspNetCore.Mvc;
using MediatR;
using UtilityHub360.CQRS.Commands.RegisterUser;
using UtilityHub360.CQRS.Queries;
using UtilityHub360.CQRS.Queries.GetUserById;
using UtilityHub360.DTOs;

namespace UtilityHub360.Controllers
{
    /// <summary>
    /// API Controller for managing users using CQRS pattern
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), 200)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            try
            {
                var query = new GetAllUsersQuery();
                var users = await _mediator.Send(query);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            try
            {
                var query = new GetUserByIdQuery { Id = id };
                var user = await _mediator.Send(query);
                
                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="createUserDto">User details</param>
        /// <returns>Created user</returns>
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] RegisterDataDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var command = new RegisterUserCommand
                {
                    Name = createUserDto.Name,
                    Email = createUserDto.Email,
                    Phone = createUserDto.Phone,
                    Password = createUserDto.Password,
                    Role = "USER"
                };
                var user = await _mediator.Send(command);

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
