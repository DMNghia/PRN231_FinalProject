using FinalProject.Dto;
using FinalProject.Dto.Response;
using FinalProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinalProject.Pages
{
    public class Course_DetailModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync(string href)
        {
            UserLoginPrinciple? principle = AuthService.GetPrinciple(HttpContext);
            if (principle == null)
            {
                return RedirectToPage("./Login");
            }

            if (HttpContext.Request.Cookies["jwt_token"] == null)
            {
                return RedirectToPage("./Login");
            }
            ViewData["href"] = href;

            HttpClient client = new HttpClient();
            // Get categories
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost:5000/api/Category");
            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
            BaseResponse<List<GetCategoryResponse>> responseGetAllCategories = await responseMessage.Content.ReadFromJsonAsync<BaseResponse<List<GetCategoryResponse>>>();
            ViewData["categories"] = responseGetAllCategories.data;

            // Get course detail
            requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5000/api/Course/detail/{href}");
            requestMessage.Headers.Add("Authorization", $"Barear {HttpContext.Request.Cookies["jwt_token"]}");
            responseMessage = await client.SendAsync(requestMessage);
            BaseResponse<GetCourseDetailResponse> response = await responseMessage.Content.ReadFromJsonAsync<BaseResponse<GetCourseDetailResponse>>();
            ViewData["course"] = response.data;
            ViewData["isTeacherCreated"] = response.data.Teacher.Id == principle.Id;

            // Update course enrolled
            requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://localhost:5000/api/Course/add-course-enroll?hrefCourse={href}");
            requestMessage.Headers.Add("Authorization", $"Barear {HttpContext.Request.Cookies["jwt_token"]}");
            await client.SendAsync(requestMessage);

            return Page();
        }
    }
}
