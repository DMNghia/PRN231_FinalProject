using FinalProject.Constants;
using FinalProject.Dto;
using FinalProject.Dto.Response;
using FinalProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace FinalProject.Security
{
    public class JwtFilter : IAuthorizationFilter
    {
        private readonly JwtService jwtService;
        private readonly string[] WHITE_LIST_URL = new string[] { "/api/auth/signin", "/api/auth/signup" };
        private readonly string[] FRONT_END_URL = new string[] { "/dang-nhap", "/trang-chu", "/dang-ky" };
        private readonly string[] TEACHER_ROLE_URL = new string[] { };

        public JwtFilter(JwtService jwtService)
        {
            this.jwtService = jwtService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                string path = context.HttpContext.Request.Path;
                path = path.ToLower();
                if (!WHITE_LIST_URL.Contains(path) && !FRONT_END_URL.Contains(path))
                {
                    string jwtToken = GetTokenFromRequest(context.HttpContext.Request);
                    if (jwtToken.IsNullOrEmpty())
                    {
                        context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Result = new JsonResult(new BaseResponse<object>
                        {
                            code = ResponseCode.ERROR.GetHashCode(),
                            message = "Thất bại"
                        });
                        return;
                    }
                    if (!jwtService.CheckTokenExpired(jwtToken))
                    {
                        context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Result = new JsonResult(new BaseResponse<object>
                        {
                            code = ResponseCode.ERROR.GetHashCode(),
                            message = "Vui lòng đăng nhập"
                        });
                        return;
                    }
                    UserLoginPrinciple principle = jwtService.GetPrincipleFromToken(jwtToken);
                    if (TEACHER_ROLE_URL.Contains(path) && principle.Role != RoleName.TEACHER.ToString())
                    {
                        context.Result = new JsonResult(new BaseResponse<object>
                        {
                            code = ResponseCode.ERROR.GetHashCode(),
                            message = "Thất bại"
                        });
                        return;
                    }
                    context.HttpContext.Items["principle"] = principle;
                }
            }
            catch (Exception ex)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Result = new JsonResult(new BaseResponse<object>
                {
                    code = ResponseCode.ERROR.GetHashCode(),
                    message = "Thất bại"
                });
            }
        }

        private string GetTokenFromRequest(HttpRequest request)
        {
            string TOKEN_HEAD = "Bearer ";
            string HEADER_AUTHENTICATION = "Authorization";
            string token = request.Headers[HEADER_AUTHENTICATION].ToString();
            return token.Substring(TOKEN_HEAD.Length);
        }
    }
}
