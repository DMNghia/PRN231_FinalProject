using FinalProject.Dto;
using FinalProject.Dto.Request;
using FinalProject.Dto.Response;
using FinalProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace FinalProject.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> logger;

        public LoginModel(ILogger<LoginModel> logger)
        {
            this.logger = logger;
        }

        [BindProperty]
        public SignInRequest SignInRequest { get; set; }
        public void OnGet()
        {
            ViewData["errorMessage"] = HttpContext.Session.GetString("errorMessage");
            ViewData["successMessage"] = HttpContext.Session.GetString("successMessage");
            if (ViewData["errorMessage"] != null)
            {
                HttpContext.Session.Remove("errorMessage");
            }
            if (ViewData["successMessage"] != null)
            {
                HttpContext.Session.Remove("successMessage");
            }
        }

        public async Task<IActionResult> OnPost()
        {
            string? error = ModelState.Values.SelectMany(m => m.Errors).Select(e => e.ErrorMessage).FirstOrDefault();
            if (error == null)
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:5000/api/auth/sign-in");
                var content = new StringContent(JsonSerializer.Serialize(SignInRequest), null, "application/json");
                request.Content = content;
                var responseMessage = await client.SendAsync(request);
                var response = await responseMessage.Content.ReadFromJsonAsync<BaseResponse<SignInResponse>>();
                if (response?.code == 0)
                {
                    AuthService.SetPrinciple(HttpContext, JwtService.GetPrincipleFromToken(response.data.Token));
                    return RedirectToPage("/Index");
                } else
                {
                    error = response.message;
                }
            }
            ViewData["errorMessage"] = error;
            return Page();
        }
    }
}
