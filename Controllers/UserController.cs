using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDTO userRegisterDTO)
    {
        if (userRegisterDTO == null || string.IsNullOrEmpty(userRegisterDTO.Email) || string.IsNullOrEmpty(userRegisterDTO.Password))
        {
            return BadRequest("Invalid user data.");
        }

        var user = new User
        {
            Name = userRegisterDTO.Name,
            Email = userRegisterDTO.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(userRegisterDTO.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "User registered successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDTO userLoginDTO)
    {
        if (userLoginDTO == null || string.IsNullOrEmpty(userLoginDTO.Email) || string.IsNullOrEmpty(userLoginDTO.Password))
        {
            return BadRequest("Invalid login data.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userLoginDTO.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(userLoginDTO.Password, user.Password))
        {
            return Unauthorized("Invalid email or password.");
        }

        return Ok(new { Message = "Login successful." });
    }

    [HttpGet("getAll")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(users);
    }
}
