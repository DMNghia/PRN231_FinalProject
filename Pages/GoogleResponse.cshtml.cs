using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using FinalProject.Services;
using FinalProject.Dto.Response;

namespace FinalProject.Pages
{
    public class GoogleResponseModel : PageModel
    {
        private readonly AuthService authService;

        public GoogleResponseModel(AuthService authService)
        {
            this.authService = authService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var Claims = result.Principal.Identities.FirstOrDefault().Claims
                .Select(claim => new
                {
                    claim.Value,
                    claim.Issuer,
                    claim.OriginalIssuer,
                    claim.Type,
                });
            string email = Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email).Value;
            string name = Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name).Value;
            BaseResponse<SignInResponse> response = await authService.LoginWithGoogle(email, name, HttpContext);
            HttpContext.Response.Cookies.Append("jwt_token", response.data.Token, new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7)
            });
            AuthService.SetPrinciple(HttpContext, JwtService.GetPrincipleFromToken(response.data.Token));
            return RedirectToPage("/Index");
        }
    }
}
