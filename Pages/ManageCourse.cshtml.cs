using FinalProject.Constants;
using FinalProject.Dto;
using FinalProject.Dto.Response;
using FinalProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Web;

namespace FinalProject.Pages
{
    public class CreateCourseRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Href { get; set; }
        public string Image { get; set; }
        public List<string> categories { get; set; }
    }

    public class ManageCourseModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync(int? pageNum, string? category, string? searchKey)
        {
            UserLoginPrinciple? principle = AuthService.GetPrinciple(HttpContext);
            if (principle == null)
            {
                return RedirectToPage("./Login");
            }

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
                requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5000/api/Course/teacher-created?page={pageNo}{searchQuery}");
            }
            else
            {
                requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5000/api/Course/teacher-created?page={pageNo}&hrefCategory={category}{searchQuery}");
            }
            requestMessage.Headers.Add("Authorization", $"Bearer {HttpContext.Request.Cookies["jwt_token"]}");
            HttpResponseMessage response = await client.SendAsync(requestMessage);
            BaseResponse<List<GetAllCourseResponse>> responseData = await response.Content.ReadFromJsonAsync<BaseResponse<List<GetAllCourseResponse>>>();
            ViewData["courses"] = responseData.data;

            // Get totalPage
            string categoryQuery = category != null ? $"?hrefCategory={category}" : "";
            searchQuery = category == null ? $"?searchKey={searchKey}" : searchQuery;
            string query = (category != null ? $"?category={category}" : "") + searchQuery;
            string totalPageUrl = $"https://localhost:5000/api/Course/totalPage/teacher-created{categoryQuery}{searchQuery}";
            requestMessage = new HttpRequestMessage(HttpMethod.Get, totalPageUrl);
            requestMessage.Headers.Add("Authorization", $"Bearer {HttpContext.Request.Cookies["jwt_token"]}");
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

        public async Task<IActionResult> OnPostAddCategory()
        {
            HttpClient _httpClient = new HttpClient();

            try
            {
                if (Request.Form.TryGetValue("SubmitAddCategory", out var submitMoocValue) && submitMoocValue == "SubmitAddCategory")
                {
                    var categoryDTO = new CategoryDto
                    {
                        Name = Request.Form["Name"],
                        Description = Request.Form["Description"],
                        Href = Request.Form["Href"],

                    };
                    var contentBody = new StringContent(JsonSerializer.Serialize(categoryDTO), null, "application/json");
                    var request = new HttpRequestMessage(HttpMethod.Post, $"https://localhost:5000/api/Category/Add");
                    request.Content = contentBody;
                    string? jwtToken = HttpContext.Request.Cookies["jwt_token"];
                    request.Headers.Add("Authorization", $"Bearer {jwtToken}");
                    var response = await _httpClient.SendAsync(request);
                    BaseResponse<object> responseData = await response.Content.ReadFromJsonAsync<BaseResponse<object>>();
                    if (responseData.code != ResponseCode.ERROR.GetHashCode())
                    {
                        TempData["SuccessMessage"] = responseData.message;
                        return RedirectToPage();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = responseData.message;
                        return RedirectToPage();
                    }
                }
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra";
                return RedirectToPage();
            }

        }

        public async Task<IActionResult> OnPostAsync(CreateCourseRequest request)
        {
            if (request.categories.IsNullOrEmpty())
            {
                TempData["ErrorMessage"] = "Vui lòng thêm ít nhất một loại";
                return RedirectToPage();
            }

            HttpClient client = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost:5000/api/Course/add");
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(request), null, "application/json");
            requestMessage.Headers.Add("Authorization", $"Bearer {HttpContext.Request.Cookies["jwt_token"]}");
            HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
            BaseResponse<object> response = await responseMessage.Content.ReadFromJsonAsync<BaseResponse<object>>();
            if (response.code != ResponseCode.SUCCESS.GetHashCode())
            {
                TempData["ErrorMessage"] = response.message;
            } else
            {
                TempData["SuccessMessage"] = response.message;
            }

            return RedirectToPage();
        }
    }
}
