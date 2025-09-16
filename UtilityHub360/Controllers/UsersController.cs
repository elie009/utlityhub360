using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using UtilityHub360.CQRS.Commands;
using UtilityHub360.CQRS.MediatR;
using UtilityHub360.CQRS.Queries;
using UtilityHub360.DTOs;
using UtilityHub360.DependencyInjection;

namespace UtilityHub360.Controllers
{
    /// <summary>
    /// API Controller for managing users using CQRS pattern
    /// </summary>
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private readonly IMediator _mediator;

        public UsersController()
        {
            _mediator = ServiceContainer.CreateDefault().GetService<IMediator>();
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet]
        [Route("")]
        [ResponseType(typeof(IEnumerable<UserDto>))]
        public async Task<IHttpActionResult> GetUsers()
        {
            try
            {
                var query = new GetAllUsersQuery();
                var users = await _mediator.Send(query);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        [HttpGet]
        [Route("{id}")]
        [ResponseType(typeof(UserDto))]
        public async Task<IHttpActionResult> GetUser(int id)
        {
            try
            {
                var query = new GetUserByIdQuery(id);
                var user = await _mediator.Send(query);
                
                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="createUserDto">User details</param>
        /// <returns>Created user</returns>
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(UserDto))]
        public async Task<IHttpActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var command = new CreateUserCommand(createUserDto);
                var user = await _mediator.Send(command);

                return CreatedAtRoute("DefaultApi", new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="updateUserDto">Updated user details</param>
        /// <returns>Updated user</returns>
        [HttpPut]
        [Route("{id}")]
        [ResponseType(typeof(UserDto))]
        public async Task<IHttpActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != updateUserDto.Id)
                {
                    return BadRequest();
                }

                var command = new UpdateUserCommand(updateUserDto);
                var user = await _mediator.Send(command);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Delete a user (soft delete)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>No content</returns>
        [HttpDelete]
        [Route("{id}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> DeleteUser(int id)
        {
            try
            {
                var command = new DeleteUserCommand(id);
                await _mediator.Send(command);

                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
