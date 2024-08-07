using FinalProject.Dto.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;

namespace FinalProject.Pages
{
    public class CourseModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync(int? pageNum, string? category, string? searchKey)
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
            if (searchKey != null)
            {
                searchKey = HttpUtility.UrlEncodeUnicode(searchKey);
            }

            // Call api
            HttpClient client = new HttpClient();
            string searchQuery = searchKey != null ? $"&searchKey={searchKey}" : "";
            HttpRequestMessage requestMessage;
            if (category == null)
            {
                requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5000/api/Course?page={pageNo}{searchQuery}");
            } else
            {
                requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5000/api/Course?page={pageNo}&hrefCategory={category}{searchQuery}");
            }
            HttpResponseMessage response = await client.SendAsync(requestMessage);
            BaseResponse<List<GetAllCourseResponse>> responseData = await response.Content.ReadFromJsonAsync<BaseResponse<List<GetAllCourseResponse>>>();
            ViewData["courses"] = responseData.data;

            // Get totalPage
            string categoryQuery = category != null ? $"?hrefCategory={category}" : "";
            searchQuery = category == null ? $"?searchKey={searchKey}" : searchQuery;
            string query = (category != null ? $"?category={category}" : "") + searchQuery;
            string totalPageUrl = $"https://localhost:5000/api/Course/totalPage{categoryQuery}{searchQuery}";
            requestMessage = new HttpRequestMessage(HttpMethod.Get, totalPageUrl);
            response = await client.SendAsync(requestMessage);
            BaseResponse<double> responseGetTotalPage = await response.Content.ReadFromJsonAsync<BaseResponse<double>>();
            ViewData["totalPage"] = (int)responseGetTotalPage.data;

            // Get all categories
            requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost:5000/api/Category");
            response = await client.SendAsync(requestMessage);
            BaseResponse<List<GetCategoryResponse>> responseGetAllCategories = await response.Content.ReadFromJsonAsync<BaseResponse<List<GetCategoryResponse>>>();
            ViewData["categories"] = responseGetAllCategories.data;

            ViewData["query"] = query;
            return Page();
        }
    }
}
