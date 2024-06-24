using FinalProject.Constants;
using FinalProject.Dto;
using FinalProject.Dto.Response;
using FinalProject.Models;

namespace FinalProject.Services
{
    public class AuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly Prn231_FinalProjectContext context;
        private readonly JwtService jwtService;

        public AuthService(ILogger<AuthService> logger, Prn231_FinalProjectContext context)
        {
            _logger = logger;
            this.context = context;
        }

        public async Task<BaseResponse<object>> LoginWithGoogle(string email, string name, HttpContext HttpContext)
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
                HttpContext.Session.SetString("token", tokenValue);
                return new BaseResponse<object>
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
                Email = existUser.Email,
                FullName = existUser.FullName,
                IsActive = existUser.IsActive ?? false,
                Role = existUser.Role,
                TypeAuthentication = existUser.TypeAuthentication
            });
            return new BaseResponse<object>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = new SignInResponse
                {
                    Token = token
                }
            };
        }
    }
}
