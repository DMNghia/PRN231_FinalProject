using FinalProject.Dto;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FinalProject.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public UserLoginPrinciple GetPrincipleFromToken(string token)
        {
            var claims = new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;
            return new UserLoginPrinciple
            {
                FullName = claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value,
                Email = claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value,
                Role = claims.First(c => c.Type == "role").Value,
                IsActive = bool.Parse(claims.First(c => c.Type == "isActive").Value),
                TypeAuthentication = claims.First(c => c.Type == "typeAuthentication").Value,
            };
        }

        public string GenerateJSONWebToken(UserLoginPrinciple principle)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, principle.FullName),
                new Claim(JwtRegisteredClaimNames.Email, principle.Email),
                new Claim("role", principle.Role),
                new Claim("isActive", principle.IsActive.ToString()),
                new Claim("typeAuthentication", principle.TypeAuthentication)
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
