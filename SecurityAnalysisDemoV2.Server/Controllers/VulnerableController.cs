using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace SecurityAnalysisDemoV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly string _connectionString;

        public UserController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost("create-user")]
        public IActionResult CreateUser([FromBody] UserModel user)
        {
            if (string.IsNullOrWhiteSpace(user.Username) ||
                string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest("Username and password are required");
            }

            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(user.Password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                string hashedPassword = Convert.ToHexString(hashBytes);

                var query = $@"INSERT INTO Users (Username, Password, Role) 
                             VALUES ('{user.Username}', '{hashedPassword}', 'User')";

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(query, connection);
                    command.ExecuteNonQuery();
                }
            }

            return Ok("User created successfully");
        }
    }

    public class UserModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}