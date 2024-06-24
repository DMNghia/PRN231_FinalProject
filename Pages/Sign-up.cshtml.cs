using FinalProject.Dto.Request;
using FinalProject.Dto.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace FinalProject.Pages
{

    public class Sign_upModel : PageModel
    {
        [BindProperty]
        public SignUpRequest SignUpRequest { get; set; }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(SignUpRequest SignUpRequest)
        {
            string? error = ModelState.Values.SelectMany(m => m.Errors).Select(e => e.ErrorMessage).FirstOrDefault();
            if (error == null)
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:5000/api/auth/sign-up");
                var content = new StringContent(JsonSerializer.Serialize(SignUpRequest), null, "application/json");
                request.Content = content;
                var responseMessage = await client.SendAsync(request);
                var response = await responseMessage.Content.ReadFromJsonAsync<BaseResponse<object>>();
                if (response?.code == 0)
                {
                    HttpContext.Session.SetString("successMessage", response?.message);
                    return RedirectToPage("/Login");
                }
                else
                {
                    error = response?.message;
                }
            }
            ViewData["errorMessage"] = error;
            return Page();
        }
    }
}
