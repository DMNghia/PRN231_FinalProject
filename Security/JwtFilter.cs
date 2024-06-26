﻿using FinalProject.Constants;
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
        private readonly ILogger<JwtFilter> logger;

        private readonly string[] WHITE_LIST_URL = new string[] { "/api/auth/*" };
        private readonly string[] FRONT_END_URL = new string[] { "/dang-nhap", "/", "/dang-ky", "/dang-nhap-voi-google", "/google-response", "/kich-hoat-tai-khoan" };
        private readonly string[] TEACHER_ROLE_URL = new string[] { };

        public JwtFilter(JwtService jwtService, ILogger<JwtFilter> logger)
        {
            this.jwtService = jwtService;
            this.logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                string path = context.HttpContext.Request.Path;
                if (!isIgnoreUrl(path))
                {
                    string jwtToken = GetTokenFromRequest(context.HttpContext.Request);
                    if (jwtToken.IsNullOrEmpty())
                    {
                        context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Result = new JsonResult(new BaseResponse<object>
                        {
                            code = ResponseCode.ERROR.GetHashCode(),
                            message = "Vui lòng đăng nhập"
                        });
                        return;
                    }
                    if (!jwtService.CheckTokenExpired(jwtToken))
                    {
                        context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Result = new JsonResult(new BaseResponse<object>
                        {
                            code = ResponseCode.ERROR.GetHashCode(),
                            message = "Vui lòng đăng nhập lại"
                        });
                        return;
                    }
                    UserLoginPrinciple principle = jwtService.GetPrincipleFromToken(jwtToken);
                    if (isTeacherRoleUrl(path) && principle.Role != RoleName.TEACHER.ToString())
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
                logger.LogError($"ERROR WHEN FILTER >>> {ex}");
                context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Result = new JsonResult(new BaseResponse<object>
                {
                    code = ResponseCode.ERROR.GetHashCode(),
                    message = "Có lỗi xảy ra"
                });
            }
        }

        private bool isTeacherRoleUrl(string path)
        {
            path = path.ToLower();
            foreach (string url in TEACHER_ROLE_URL)
            {
                string lowerUrl = url.ToLower();
                if (lowerUrl.EndsWith("/*"))
                {
                    string startWithUrl = lowerUrl.Substring(0, lowerUrl.Length - 2);
                    if (path.StartsWith(startWithUrl))
                    {
                        return true;
                    }
                }
                else
                {
                    if (lowerUrl == path)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool isIgnoreUrl(string path)
        {
            path = path.ToLower();
            foreach (string url in WHITE_LIST_URL)
            {
                string lowerUrl = url.ToLower();
                if (lowerUrl.EndsWith("/*"))
                {
                    string startWithUrl = lowerUrl.Substring(0, lowerUrl.Length - 2);
                    if (path.StartsWith(startWithUrl))
                    {
                        return true;
                    }
                } else
                {
                    if (lowerUrl == path)
                    {
                        return true;
                    }
                }
            }

            foreach (string url in FRONT_END_URL)
            {
                string lowerUrl = url.ToLower();
                if (lowerUrl.EndsWith("/*"))
                {
                    string startWithUrl = lowerUrl.Substring(0, lowerUrl.Length - 2);
                    if (path.StartsWith(startWithUrl))
                    {
                        return true;
                    }
                }
                else
                {
                    if (lowerUrl == path)
                    {
                        return true;
                    }
                }
            }
            return false;
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
