using FinalProject.Dto.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinalProject.Pages
{
    public class CourseModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync(int? pageNum, string? category)
        {
            int pageNo = 1;
            if (pageNum != null)
            {
                pageNo = pageNum.Value;
            }
            if (category != null)
            {
                ViewData["category"] = category;
            }
            ViewData["pageNo"] = pageNo;

            // Call api
            HttpClient client = new HttpClient();
            HttpRequestMessage requestMessage;
            if (category == null)
            {
                requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5000/api/Course?page={pageNo}");
            } else
            {
                requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5000/api/Course?page={pageNo}&hrefCategory={category}");
            }
            HttpResponseMessage response = await client.SendAsync(requestMessage);
            BaseResponse<List<GetAllCourseResponse>> responseData = await response.Content.ReadFromJsonAsync<BaseResponse<List<GetAllCourseResponse>>>();
            ViewData["courses"] = responseData.data;

            // Get totalPage
            string totalPageUrl = "https://localhost:5000/api/Course/totalPage";
            if (category != null)
            {
                totalPageUrl += $"?hrefCategory={category}";
            }
            requestMessage = new HttpRequestMessage(HttpMethod.Get, totalPageUrl);
            response = await client.SendAsync(requestMessage);
            BaseResponse<double> responseGetTotalPage = await response.Content.ReadFromJsonAsync<BaseResponse<double>>();
            ViewData["totalPage"] = (int)responseGetTotalPage.data;

            // Get all categories
            requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost:5000/api/Category");
            response = await client.SendAsync(requestMessage);
            BaseResponse<List<GetCategoryResponse>> responseGetAllCategories = await response.Content.ReadFromJsonAsync<BaseResponse<List<GetCategoryResponse>>>();
            ViewData["categories"] = responseGetAllCategories.data;

            return Page();
        }
    }
}
