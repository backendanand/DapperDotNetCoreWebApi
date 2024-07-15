# Authentication in ASP.NET Core Web Api

## Setup
Install the following packages
- ``` dotnet install-package Dapper```
- ``` dotnet install-package Microsoft.Data.SqlClient ```
- ``` dotnet install-package Microsoft.AspNetCore.Authentication.JwtBearer ```
- ``` dotnet install-package System.IdentityModel.Tokens.Jwt ```

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
