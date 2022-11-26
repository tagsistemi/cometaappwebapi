using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using UsersClassLibrary.Models;
using ResponseUtils;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Swashbuckle.AspNetCore.Annotations;
using Azure;
using CometaAppWebApi.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CometaAppWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IDbConnection _conn;
        private readonly UsersRepository _usersRepository;
        private readonly IJWTGen _jwt;

        public UsersController(IConfiguration configuration, IJWTGen jwtGen)
        {
            _configuration = configuration;
            _conn = new SqlConnection(_configuration.GetConnectionString("CN01"));
            _usersRepository = new UsersRepository(_conn);
            _jwt = jwtGen;
        }

        // GET api/<UsersController>
        [SwaggerResponse(400, Type = typeof(RESTResponse<string>), Description = "BAD_TOKEN")]
        [SwaggerResponse(401, Type = typeof(RESTResponse<string>), Description = "UNAUTHORIZED")]
        [SwaggerResponse(404, Type = typeof(RESTResponse<string>), Description = "USER_NOT_FOUND")]
        [HttpGet("@me", Name = "Get")]
        [Authorize]
        public async Task<ActionResult<RESTResponse<UserOutModule>>> Get([FromHeader] string authorization)
        {
            var response = new RESTResponse<UserOutModule>();
            var splittedToken = authorization.Split(" ");
            if (splittedToken.Count() <= 1)
            {
                response.Status = 400;
                response.Message = "BAD_TOKEN";
                return BadRequest(response);
            }
            var token = splittedToken[1];
            var IdUser = _jwt.getUserIdFromToken(token);
            if (IdUser == null)
            {
                response.Status = 401;
                response.Message = "UNAUTHORIZED";
                return Unauthorized(response);
            }

            User? user = await _usersRepository.getUserById(IdUser);
            if(user == null)
            {
                response.Status = 404;
                response.Message = "USER_NOT_FOUND";
                return NotFound(response);
            }

            response.Status = 200;
            response.Message = "OK";
            response.Data = user.getSecuredModule();
            return Ok(response);
        }

        // GET: api/<UsersController>/{ idUser }
        [SwaggerResponse(404, Type = typeof(RESTResponse<string>), Description = "USER_NOT_FOUND")]
        [HttpGet("{idUser}", Name = "GetUser")]
        public async Task<ActionResult<RESTResponse<UserOutModule>>> GetUser(String idUser)
        {
            RESTResponse<UserOutModule> resp = new RESTResponse<UserOutModule>();
            User? user = await _usersRepository.getUserById(idUser);
            if(user == null)
            {
                resp.Message = "USER_NOT_FOUND";
                resp.Status = 404;
                return NotFound(resp);
            }
            resp.Message = "OK";
            resp.Status = 200;
            resp.Data = user.getSecuredModule();
            return Ok(resp);
        }

        // POST api/<UsersController>
        [HttpPost]
        public async Task<ActionResult<RESTResponse<String>>> Post([FromBody] User receivedUser)
        {
            var response = new RESTResponse<String>();
            await _usersRepository.insertUser(receivedUser);
            response.Status = 200;
            response.Message = "OK";
            return Ok(response);
        }

        // PUT api/<UsersController>/5
        [SwaggerResponse(404, Type = typeof(RESTResponse<string>), Description = "USER_NOT_FOUND")]
        [HttpPut("@me")]
        [Authorize]
        public async Task<ActionResult<RESTResponse<string>>> Put([FromHeader] String authorization, [FromBody] User newUser)
        {
            String idUser = _jwt.getUserIdFromToken(authorization);
            var response = new RESTResponse<string>();
            User? user = await _usersRepository.getUserById(idUser);
            if (user == null)
            {
                response.Status = 404;
                response.Message = "USER_NOT_FOUND";
                return NotFound(response);
            } else
            {
                await _usersRepository.updateUser(idUser, newUser);
                response.Status = 200;
                response.Message = "OK";
                return Ok(response);
            }
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("@me")]
        [Authorize]
        public async Task<ActionResult<RESTResponse<string>>> Delete([FromHeader] String authorization)
        {
            String idUser = _jwt.getUserIdFromToken(authorization);
            var response = new RESTResponse<string>();
            User? user = await _usersRepository.getUserById(idUser);
            if(user == null)
            {
                response.Status = 404;
                response.Message = "USER_NOT_FOUND";
                return NotFound(response);
            }
            await _usersRepository.deleteUser(user);
            response.Status = 200;
            response.Message = "OK";
            return Ok(response);
        }
    }
}
