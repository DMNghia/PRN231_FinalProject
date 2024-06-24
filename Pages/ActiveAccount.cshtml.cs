using FinalProject.Dto.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinalProject.Pages
{
    public class ActiveAccountModel : PageModel
    {
        public async Task<IActionResult> OnGet(string? tokenValue)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.PostAsync($"https://localhost:5000/api/auth/active-account/{tokenValue}", null).Result;
            BaseResponse<object> baseResponse = await response.Content.ReadFromJsonAsync<BaseResponse<object>>();
            if (baseResponse.code != 0)
            {
                HttpContext.Session.SetString("errorMessage", baseResponse.message);
            } else
            {
                HttpContext.Session.SetString("successMessage", baseResponse.message);
            }
            return RedirectToPage("./Login");
        }
    }
}
