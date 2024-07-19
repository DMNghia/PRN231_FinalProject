using FinalProject.Constants;
using FinalProject.Dto;
using FinalProject.Dto.Response;
using FinalProject.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;

namespace FinalProject.Services
{
    public class AuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly Prn231_FinalProjectContext context;
        private readonly JwtService jwtService;

        public AuthService(ILogger<AuthService> logger, Prn231_FinalProjectContext context, JwtService jwtService)
        {
            _logger = logger;
            this.context = context;
            this.jwtService = jwtService;
        }

        public async Task<BaseResponse<SignInResponse>> LoginWithGoogle(string email, string name, HttpContext HttpContext)
        {
            _logger.LogInformation($"LOGIN WITH GOOGLE EMAIL: {email}; NAME: {name}");
            User? existUser = context.Users.FirstOrDefault(u => u.Email == email);
            if (existUser == null)
            {
                User user = new User
                {
                    Email = email,
                    FullName = name,
                    CreateAt = DateTime.Now,
                    IsActive = true,
                    Role = RoleName.STUDENT.ToString(),
                    TypeAuthentication = TypeAuthentication.GOOGLE.ToString(),
                };
                context.Users.Add(user);
                context.SaveChangesAsync();
                string tokenValue = jwtService.GenerateJSONWebToken(new UserLoginPrinciple
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    IsActive = user.IsActive ?? false,
                    Role = user.Role,
                    TypeAuthentication = user.TypeAuthentication
                });
                HttpContext.Session.SetString("principle", JsonSerializer.Serialize(new UserLoginPrinciple
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    IsActive = user.IsActive ?? false,
                    Role = user.Role,
                    TypeAuthentication = user.TypeAuthentication
                }));
                return new BaseResponse<SignInResponse>
                {
                    code = ResponseCode.SUCCESS.GetHashCode(),
                    message = "Thành công",
                    data = new SignInResponse
                    {
                        Token = tokenValue
                    }
                };
            }
            string token = jwtService.GenerateJSONWebToken(new UserLoginPrinciple
            {
                Id = existUser.Id,
                Email = existUser.Email,
                FullName = existUser.FullName,
                IsActive = existUser.IsActive ?? false,
                Role = existUser.Role,
                TypeAuthentication = existUser.TypeAuthentication
            });
            return new BaseResponse<SignInResponse>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = new SignInResponse
                {
                    Token = token
                }
            };
        }

        public static void SetPrinciple(HttpContext context, UserLoginPrinciple principle)
        {
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(TypeAuthentication.LOCAL.ToString(), "PRINCIPLE", principle.Role);
            claimsIdentity.AddClaims(new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, principle.FullName),
                        new Claim(ClaimTypes.Email, principle.Email),
                        new Claim(ClaimTypes.AuthenticationMethod, principle.TypeAuthentication),
                        new Claim("id", principle.Id.ToString()),
                        new Claim(ClaimTypes.Role,  principle.Role),
                        new Claim("isActive", principle.IsActive.ToString())
                    });
            context.User = new ClaimsPrincipal(claimsIdentity);
        }

        public static UserLoginPrinciple? GetPrinciple(HttpContext HttpContext)
        {
            try
            {
                if (HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals("id")) == null)
                {
                    string? token = HttpContext.Request.Cookies["jwt_token"];
                    
                    if (token.IsNullOrEmpty())
                    {
                        return null;
                    }
                    return JwtService.GetPrincipleFromToken(token);
                }
                return new UserLoginPrinciple
                {
                    Id = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals("id")).Value),
                    FullName = HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Name)).Value,
                    Email = HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email)).Value,
                    IsActive = bool.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals("isActive")).Value),
                    Role = HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Role)).Value,
                    TypeAuthentication = HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.AuthenticationMethod)).Value
                };
            } catch (Exception ex)
            {
                return null;
            }
            
        }
    }
}
