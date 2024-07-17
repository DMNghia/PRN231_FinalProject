using FinalProject.Dto.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinalProject.Pages
{
    public class CourseModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync(string? category)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage requestMessage;
            if (category == null)
            {
                requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost:5000/api/Course");
            } else
            {
                requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5000/api/Course/GetAllByCategory?hrefCategory={category}");
            }
            HttpResponseMessage response = await client.SendAsync(requestMessage);
            BaseResponse<List<GetAllCourseResponse>> responseData = await response.Content.ReadFromJsonAsync<BaseResponse<List<GetAllCourseResponse>>>();
            ViewData["courses"] = responseData.data;

            // Get all categories
            requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost:5000/api/Category");
            response = await client.SendAsync(requestMessage);
            BaseResponse<List<GetCategoryResponse>> responseGetAllCategories = await response.Content.ReadFromJsonAsync<BaseResponse<List<GetCategoryResponse>>>();
            ViewData["categories"] = responseGetAllCategories.data;

            return Page();
        }
    }
}
