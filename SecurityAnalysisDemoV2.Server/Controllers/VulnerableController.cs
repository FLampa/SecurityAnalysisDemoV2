using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
using BCrypt.Net;

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
            // Validate input
            if (string.IsNullOrWhiteSpace(user.Username) ||
                string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest("Username and password are required");
            }

            // Secure password hashing (using BCrypt or similar)
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

            // Use parameterized query to prevent SQL injection
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    INSERT INTO Users (Username, Password, Role) 
                    VALUES (@Username, @Password, @Role)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Password", hashedPassword);
                    command.Parameters.AddWithValue("@Role", "User");

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