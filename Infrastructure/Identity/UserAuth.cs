using JWT_Train.Infrastructure.Helpers;
using JWT_Train.Domain.Entities;
using JWT_Train.Domain.Models;
using JWT_Train.Application.DTOs;
using JWT_Train.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace JWT_Train.Infrastructure.Identity
{
    public class UserAuth : IUserAuth
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly JWT _jwt;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserAuth> _logger;

        public UserAuth(UserManager<AppUser> userManager,
            IOptions<JWT> jwt, ILogger<UserAuth> logger, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _logger = logger;
            _roleManager = roleManager;
        }

        public async Task<string> AddToRole(AddRoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if(user==null)
                return "User not found";

            if(!await _roleManager.RoleExistsAsync(model.Role))
                return "Role not found";

            if(await _userManager.IsInRoleAsync(user,model.Role))
                return "User already assigned to this role";

            var result = await _userManager.AddToRoleAsync(user, model.Role);

            if (!result.Succeeded)
                return "Cannot Add User To role";

            return string.Empty;
        }

        public async Task<AuthModel> GetTokenAsync(LoginUser model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if(user==null || !await _userManager.CheckPasswordAsync(user,model.Password))
            {
                return new AuthModel { Message = "Email or Password is incorrect!", IsAuthenticated = false };
            }

            var token = await CreateToke(user);
            return new AuthModel
            {
                Message = "Login Successfull!",
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Username = user.UserName,
                Email = user.Email,
                ExpireOn = token.ValidTo,
                Roles = await _userManager.GetRolesAsync(user) as List<string>
            };
        }

        public async Task<AuthModel> RegisterAsync(RegisterUser model)
        {
           if(await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "Email is already registered!" ,IsAuthenticated=false};

            if (await _userManager.FindByEmailAsync(model.UserName) is not null)
                return new AuthModel { Message = "UserName is already registered!", IsAuthenticated = false };

           var user = new AppUser
           {
               UserName = model.UserName,
               Email = model.Email,
               FirstName = model.FirstName,
               LastName = model.LastName,
           };

            var result = await _userManager.CreateAsync(user, model.Password);

            if(!result.Succeeded)
            {
                StringBuilder errors = new StringBuilder();
                foreach (var error in result.Errors)
                {
                    errors.Append($"{error.Description},");
                }
                return new AuthModel { Message = errors.ToString(), IsAuthenticated = false };
            }

            var isRoleAdded = _userManager.AddToRoleAsync(user, "User");

            var token  = await CreateToke(user);

            return new AuthModel
            {
                Message = "User Registered Successfully!",
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Username = user.UserName,
                Email = user.Email,
                ExpireOn = token.ValidTo,
                Roles = new List<string> { "User" }
            };
        }

        private async Task<JwtSecurityToken> CreateToke(AppUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<System.Security.Claims.Claim>();

            foreach (var role in roles)
            {
                roleClaims.Add(new System.Security.Claims.Claim("roles", role));
            }

            var Claims = new[]
            {
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub,user.UserName),
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Email,user.Email),
                new System.Security.Claims.Claim("uid",user.Id),
            };

            var symmetricSecurityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(symmetricSecurityKey,Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: Claims.Union(userClaims).Union(roleClaims),
                expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: signingCredentials
                );
            return token;
        }
    }
}
