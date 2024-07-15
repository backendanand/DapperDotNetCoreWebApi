# Authentication in ASP.NET Core Web Api

## Setup
Install the following packages
- ``` dotnet install-package Dapper```
- ``` dotnet install-package Microsoft.Data.SqlClient ```
- ``` dotnet install-package Microsoft.AspNetCore.Authentication.JwtBearer ```
- ``` dotnet install-package System.IdentityModel.Tokens.Jwt ```
- ``` dotnet install-package Newtonsoft.Json ```

Make sure you have already configured Dapper in your project

## App Settings
Add the following lines in `appsettings.json`
```
"ConnectionStrings": {
  "DefaultConnection": "Data Source=\\SQLEXPRESS;Initial Catalog=DbName;TrustServerCertificate=True;Trusted_Connection=True;"
},
"JWT": {
  "SecretKey": "NTQzNDRkNGYtZjI4Yy00MGRhLWJmMjItNjQ5MjI2MDhkNTk0",
  "ValidAudience": "https://localhost:7299;http://localhost:7299",
  "ValidIssuer": "https://localhost:7299;http://localhost:7299",
  "TokenValidityInMinutes": 240,
  "RefreshTokenValidityInMinutes": 10080
}
```

Add the following lines in `Program.cs`
```
// Configure authentication with JWT Bearer
var key = Encoding.ASCII.GetBytes(builder.Configuration["JWT:SecretKey"]);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
builder.Services.AddSwaggerGen(c =>
{
    // Define the JWT Bearer scheme that's in use
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
            {
                new OpenApiSecurityScheme {
                    Reference = new OpenApiReference {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
});
```

## Models
Create a class `User.cs` in `Models` folder
```
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string? Role { get; set; }
    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }
}
```

## DTOs
Create a DTO class `RegisterDto` for Register
```
public class RegisterDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}
```

Create a DTO class `LoginDto` for Login
```
public class LoginDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}
```

## Interfaces
Create an Interface Repositories\Interfaces\ `IUserRepository.cs`
```
public interface IUserRepository
{
    Task<User> GetUserByUsername(string username);
    Task<int> CreateUser(User user);
    (string passwordHash, string passwordSalt) CreatePasswordHash(string password);
}
```

## Repositories
Create a class Repositories\ `UserRepository.cs`
```
public class UserRepository : IUserRepository
{
    private readonly DapperContext _context;
    public UserRepository(DapperContext context)
    {
        _context = context;
    }
    public (string passwordHash, string passwordSalt) CreatePasswordHash(string password)
    {
        using (var hmac = new HMACSHA512())
        {
            var passwordSalt = Convert.ToBase64String(hmac.Key);
            var passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return (passwordHash, passwordSalt);
        }
    }

    public async Task<int> CreateUser(User user)
    {
        var query = "INSERT INTO Users (Username, Role, PasswordHash, PasswordSalt) VALUES (@Username, @Role, @PasswordHash, @PasswordSalt)";
        using (var connection = _context.CreateConnection())
        {
            return await connection.ExecuteAsync(query, user);
        }
    }

    public async Task<User> GetUserByUsername(string username)
    {
        var query = "SELECT * FROM Users WHERE Username = @Username";
        using (var connection = _context.CreateConnection())
        {
            return await connection.QuerySingleOrDefaultAsync<User>(query, new {Username = username});
        }
    }
}
```

## Dependecy Injection
Add the following lines in `Program.cs`
```
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
```

## Controllers
Create a controller Controllers\ `AuthController`
```
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthController(IUserRepository userRepository, IConfiguration configuration)
    {
        _configuration = configuration;
        _userRepository = userRepository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto userDto)
    {
        var user = await _userRepository.GetUserByUsername(userDto.Username);
        if (user != null) return BadRequest("User already exists");

        var (passwordHash, passwordSalt) = _userRepository.CreatePasswordHash(userDto.Password);
        var newUser = new User
        {
            Username = userDto.Username.Trim().ToLower(),
            Role = "user",
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        await _userRepository.CreateUser(newUser);
        return Ok("User created successfully");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto userDto)
    {
        var user = await _userRepository.GetUserByUsername(userDto.Username);
        if (user == null) return BadRequest("Invalid credentials.");

        using (var hmac = new HMACSHA512(Convert.FromBase64String(user.PasswordSalt)))
        {
            var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(userDto.Password)));
            if(computedHash != user.PasswordHash)
            {
                return Unauthorized("Invalid credentials.");
            }
        }

        var token = GenerateJwtToken(user);
        return Ok(new {token });
    }

    [NonAction]
    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.Role),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```


