using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotnetStockAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace DotnetStockAPI.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AuthenticateController : ControllerBase
{
    //Make object of ApplicationDbContext
    private readonly ApplicationDbContext _context;
    //Make object for manage user
    private readonly UserManager<IdentityUser> _userManager;
    //Make object for manage role
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    //Make Construcotr for initial value of objects(ApplicationDbContext,...)
    public AuthenticateController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    //Endpoint for Test connect to DB
    [HttpGet("[action]")]
    public void TestConnectionDB()
    {
        if (_context.Database.CanConnect())
        {
            //If can connect to DB then show Text "Connected"
            Response.WriteAsync("Connected");
        }
        else
        {
            //If can not connect to DB then show Text "Not Connected"
            Response.WriteAsync("Not Connected");
        }
    }

    //Endpoint for User//
    //Enpoint for Register
    [HttpPost]
    [Route("register-user")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterModel model)
    {
        //Check user same other user
        var userExists = await _userManager.FindByNameAsync(model.Username);
        if (userExists != null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = "User already exists!" });
        }
        //Check email same other email
        var emailExists = await _userManager.FindByEmailAsync(model.Email);
        if (emailExists != null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = "Email already exists!" });
        }
        //Create new user
        IdentityUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };
        //Create new user in System
        var result = await _userManager.CreateAsync(user, model.Password);
        //Check user created in not done
        if (!result.Succeeded)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "error", Message = "User creation failed! Please check user details and try again." });
        }
        //Define role for Admin, Manager, User
        if (!await _roleManager.RoleExistsAsync(UserRoleModel.Admin))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoleModel.Admin));
        }
        if (!await _roleManager.RoleExistsAsync(UserRoleModel.Manager))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoleModel.Manager));
        }
        if (!await _roleManager.RoleExistsAsync(UserRoleModel.User))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoleModel.User));
            await _userManager.AddToRoleAsync(user, UserRoleModel.User);
        }

        return Ok(new ResponseModel { Status = "Success", Message = "User created successfully!" });
    }

    //Enpoint for Login
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.Username!);

        //If login success
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password!))
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GetToken(authClaims);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                userData = new
                {
                    userName = user.UserName,
                    email = user.Email,
                    roles = userRoles
                }
            });
        }
        //If login not success
        return Unauthorized();
    }

    //Function for Making Token
    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddHours(24),//Expire in 24 hours
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
        return token;
    }
}

