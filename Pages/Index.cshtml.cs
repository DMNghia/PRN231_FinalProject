using FinalProject.Constants;
using FinalProject.Dto;
using FinalProject.Dto.Response;
using FinalProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Claims;
using System.Text.Json;

namespace FinalProject.Pages
{
    public class IndexModel : PageModel
    {
        public async Task OnGet()
        {
            // Get all categories
            HttpClient client = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost:5000/api/Category");
            HttpResponseMessage response = await client.SendAsync(requestMessage);
            BaseResponse<List<GetCategoryResponse>> responseGetAllCategories = await response.Content.ReadFromJsonAsync<BaseResponse<List<GetCategoryResponse>>>();
            ViewData["categories"] = responseGetAllCategories.data;

            // total course
            response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://localhost:5000/api/Course/totalCourse"));
            ViewData["totalCourse"] = (await response.Content.ReadFromJsonAsync<BaseResponse<int>>()).data;

            // total student
            response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://localhost:5000/api/User/totalStudent"));
            ViewData["totalStudent"] = (await response.Content.ReadFromJsonAsync<BaseResponse<int>>()).data;

            // total teacher
            response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://localhost:5000/api/User/totalTeacher"));
            ViewData["totalTeacher"] = (await response.Content.ReadFromJsonAsync<BaseResponse<int>>()).data;

            // get popular courses
            response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://localhost:5000/api/Course/popular"));
            ViewData["popularCourses"] = (await response.Content.ReadFromJsonAsync<BaseResponse<List<GetAllCourseResponse>>>()).data;

            // get enrolled course if already login
            UserLoginPrinciple? principle = AuthService.GetPrinciple(HttpContext);
            if (principle != null)
            {
                requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost:5000/api/Course/enrolleds");
                requestMessage.Headers.Add("Authorization", $"Barear {HttpContext.Request.Cookies["jwt_token"]}");
                response = await client.SendAsync(requestMessage);
                ViewData["enrolledCourses"] = (await response.Content.ReadFromJsonAsync<BaseResponse<List<GetAllCourseResponse>>>()).data;
            }
        }

        public async Task<IActionResult> OnPost()
        {
            HttpClient _httpClient = new HttpClient();

            try
            {
                if (Request.Form.TryGetValue("SubmitAddCategory", out var submitMoocValue) && submitMoocValue == "SubmitAddCategory")
                {
                    var categoryDTO = new CategoryDto
                    {
                        Name  = Request.Form["Name"],
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
            } catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra";
                return RedirectToPage();
            }
           
        }
    }
}

