using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using UsersClassLibrary.Models;
using ResponseUtils;
using RequestUtils;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using CometaAppWebApi.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CometaAppWebApi.Controllers
{
    [Route("api/token")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IDbConnection _conn;
        private readonly UsersRepository _usersRepository;
        private readonly IJWTGen _jwt;

        public AuthController(IConfiguration configuration, IJWTGen jwtGen)
        {
            _configuration = configuration;
            _conn = new SqlConnection(_configuration.GetConnectionString("CN01"));
            _usersRepository = new UsersRepository(_conn);
            _jwt = jwtGen;
        }

        // POST api/token
        [SwaggerResponse(404, Type = typeof(RESTResponse<string>), Description = "USER_NOT_FOUND")]
        [HttpPost]
        public async Task<ActionResult<RESTResponse<string>>> Post([FromBody] LoginBody loginBody)
        {
            User? user = await _usersRepository.getUserByNameAndPassword(loginBody.username, loginBody.password);
            var response = new RESTResponse<String>();
            if(user == null)
            {
                response.Status = 404;
                response.Message = "USER_NOT_FOUND";
                return NotFound(response);
            }
            String token = _jwt.generateUserToken(user);
            response.Status = 200;
            response.Message = "OK";
            response.Data = token;
            return Ok(response);
        }
    }
}
