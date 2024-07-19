using System.Net.Http;
using System.Text.Json;
using FinalProject.Constants;
using FinalProject.Dto;
using FinalProject.Dto.Response;
using FinalProject.Models;
using FinalProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;

namespace FinalProject.Pages
{
    public class LessonModel : PageModel
    {
        public async Task<IActionResult> OnGet(string hrefKhoaHoc, string? hrefBaiHoc)
        {
            UserLoginPrinciple? principle = AuthService.GetPrinciple(HttpContext);
            if (principle == null)
            {
                return RedirectToPage("./Login");
            }
            HttpClient _httpClient = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5000/api/Lesson/Lesson-detail?href={hrefBaiHoc}");
            requestMessage.Headers.Add("Authorization", $"Bearer {HttpContext.Request.Cookies["jwt_token"]}");
            HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);
            var lesson = response.Content.ReadFromJsonAsync<LessonDTO>().Result;

            requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5000/api/Lesson/ListLesson");
            requestMessage.Headers.Add("Authorization", $"Bearer {HttpContext.Request.Cookies["jwt_token"]}");
            response = _httpClient.SendAsync(requestMessage).Result;
            var lessons = response.Content.ReadFromJsonAsync<List<LessonDTO>>().Result;

            requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5000/api/Mooc/GetMoocs");
            requestMessage.Headers.Add("Authorization", $"Bearer {HttpContext.Request.Cookies["jwt_token"]}");
            HttpResponseMessage response2 = _httpClient.SendAsync(requestMessage).Result;
            var moocs = response2.Content.ReadFromJsonAsync<List<MoocDTO>>().Result;

			requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost:5000/api/Category");
			HttpResponseMessage response3 = await _httpClient.SendAsync(requestMessage);
			BaseResponse<List<GetCategoryResponse>> responseGetAllCategories = await response3.Content.ReadFromJsonAsync<BaseResponse<List<GetCategoryResponse>>>();

			// Get course detail for isTeacherCreated
			requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5000/api/Course/detail/{hrefKhoaHoc}");
			requestMessage.Headers.Add("Authorization", $"Barear {HttpContext.Request.Cookies["jwt_token"]}");
			HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage);
			BaseResponse<GetCourseDetailResponse> response4 = await responseMessage.Content.ReadFromJsonAsync<BaseResponse<GetCourseDetailResponse>>();
			
			ViewData["isTeacherCreated"] = response4.data.Teacher.Id == principle.Id;

			// Update course enrolled
			requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://localhost:5000/api/Course/add-course-enroll?hrefCourse={hrefKhoaHoc}&hrefLesson={hrefBaiHoc}");
            requestMessage.Headers.Add("Authorization", $"Barear {HttpContext.Request.Cookies["jwt_token"]}");
            await _httpClient.SendAsync(requestMessage);

            ViewData["categories"] = responseGetAllCategories.data;

			ViewData["lesson"] = lesson;
            ViewData["lessons"] = lessons;
            ViewData["hrefKhoaHoc"] = hrefKhoaHoc;
            ViewData["moocs"] = moocs;

            return Page();
        }

        public async Task<IActionResult> OnPost(string courseHref, MoocDTO moocDTO)
        {
            try
            {
                HttpClient _httpClient = new HttpClient();

                if (Request.Form.TryGetValue("SubmitAddMooc", out var submitMoocValue) && submitMoocValue == "SubmitAddMooc")
                {
                    moocDTO.Lessons = new List<LessonDTO>();
                    var contentBody = new StringContent(JsonSerializer.Serialize(moocDTO), null, "application/json");
                    var request = new HttpRequestMessage(HttpMethod.Post, $"https://localhost:5000/api/Mooc/AddMooc?courseHref={courseHref}");
                    request.Content = contentBody;
                    string? jwtToken = HttpContext.Request.Cookies["jwt_token"];
                    request.Headers.Add("Authorization", $"Bearer {jwtToken}");
                    var response = await _httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Add mooc successfully.";
                        return RedirectToPage();
                    }
                    else
                    {
						TempData["ErrorMessage"] = "An error occur.";
						return RedirectToPage();
					}
                }
                else if (Request.Form.TryGetValue("SubmitAddLesson", out var submitLessonValue) && submitLessonValue == "SubmitAddLesson")
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5000/api/Lesson/CheckHrefExists?href={Request.Form["Href"]}");
                    string? jwtToken = HttpContext.Request.Cookies["jwt_token"];
                    request.Headers.Add("Authorization", $"Bearer {jwtToken}");
                    var existingHrefResponse = await _httpClient.SendAsync(request);
                    if (existingHrefResponse.IsSuccessStatusCode)
                    {
                        var content = await existingHrefResponse.Content.ReadAsStringAsync();
                        var isHrefExists = bool.Parse(content);

                        if (isHrefExists)
                        {
                            TempData["ErrorMessage"] = "Href already exists. Please choose a different one.";
                            return RedirectToPage();

                        }
                        else
                        {
                            var lessonDTO = new LessonDTO
                            {
                                Name = Request.Form["LessonName"],
                                Description = Request.Form["LessonDescription"],
                                Href = Request.Form["Href"],
                                VideoUrl = Request.Form["VideoUrl"],
                                VideoTranscript = Request.Form["VideoTranscript"],
                                MoocId = int.Parse(Request.Form["MoocId"]),
                            };

                            var contentBody = new StringContent(JsonSerializer.Serialize(lessonDTO), null, "application/json");
                            request = new HttpRequestMessage(HttpMethod.Post, $"https://localhost:5000/api/Lesson/AddLesson");
                            request.Content = contentBody;
                            jwtToken = HttpContext.Request.Cookies["jwt_token"];
                            request.Headers.Add("Authorization", $"Bearer {jwtToken}");
                            var responseLesson = await _httpClient.SendAsync(request);
                            if (responseLesson.IsSuccessStatusCode)
                            {
                                TempData["SuccessMessage"] = "Add lesson succesfully.";
                                return RedirectToPage();
                            }
                            else
                            {
                                var errorMessageLesson = await responseLesson.Content.ReadAsStringAsync();
                                ViewData["ErrorMessage"] = $"Failed to add Lesson. Error message: {errorMessageLesson}";
                            }
                        }
                    }
                    else
                    {
                        ViewData["ErrorMessage"] = "Fail to check href.";
                    }
                }
                else if (Request.Form.TryGetValue("SubmitEditLesson", out var submitEditLessonValue) && submitEditLessonValue == "SubmitEditLesson")
                {

                    var request = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5000/api/Lesson/CheckHrefExists?href={Request.Form["Href"]}");
                    string? jwtToken = HttpContext.Request.Cookies["jwt_token"];
                    request.Headers.Add("Authorization", $"Bearer {jwtToken}");
                    var existingHrefResponse = await _httpClient.SendAsync(request);
                    if (existingHrefResponse.IsSuccessStatusCode)
                    {
                        var content = await existingHrefResponse.Content.ReadAsStringAsync();
                        var isHrefExists = bool.Parse(content);

                        if (isHrefExists && Request.Form["Href"] != Request.Form["OldHref"])
                        {
                            TempData["ErrorMessage"] = "Href already exists. Please choose a different one.";
                            return RedirectToPage();
                        }
                        else
                        {

                            var lessonDTO = new LessonDTO
                            {
                                Id = int.Parse(Request.Form["LessonId"]),
                                Name = Request.Form["LessonName"],
                                Description = Request.Form["LessonDescription"],
                                Href = Request.Form["Href"],
                                VideoUrl = Request.Form["VideoUrl"],
                                VideoTranscript = Request.Form["VideoTranscript"],
                                MoocId = int.Parse(Request.Form["MoocId"]),
                            };

                            var contentBody = new StringContent(JsonSerializer.Serialize(lessonDTO), null, "application/json");
                            request = new HttpRequestMessage(HttpMethod.Put, $"https://localhost:5000/api/Lesson/EditLesson/{lessonDTO.Id}");
                            request.Content = contentBody;
                            jwtToken = HttpContext.Request.Cookies["jwt_token"];
                            request.Headers.Add("Authorization", $"Bearer {jwtToken}");
                            var responseLesson = await _httpClient.SendAsync(request);
                            if (responseLesson.IsSuccessStatusCode)
                            {
                                return RedirectToPage();
                            }
                            else
                            {
                                BaseResponse<object> errorMessageLesson = await responseLesson.Content.ReadFromJsonAsync<BaseResponse<object>>();
                                TempData["ErrorMessage"] = $"Failed to update Lesson. Error message: {errorMessageLesson.message}";
                                return RedirectToPage();
                            }
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Fail to check if href exist.";
                    }
                }
                else if (Request.Form.TryGetValue("SubmitDeleteLesson", out var submitDeleteLessonValue) && submitDeleteLessonValue == "SubmitDeleteLesson")
                {
                    int lessonId = int.Parse(Request.Form["LessonId"]);
                    var request = new HttpRequestMessage(HttpMethod.Delete, $"https://localhost:5000/api/Lesson/DeleteLesson/{lessonId}");
                    string? jwtToken = HttpContext.Request.Cookies["jwt_token"];
                    request.Headers.Add("Authorization", $"Bearer {jwtToken}");
                    var response = await _httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToPage("/Course-Detail");
                    }
                    else
                    {
                        var errorMessageLesson = await response.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = $"Failed to delete Lesson. Error message: {errorMessageLesson}";
                    }
                }
                //else if (Request.Form.TryGetValue("SubmitDeleteMooc", out var submitDeleteMoocValue) && submitDeleteMoocValue == "SubmitDeleteMooc")
                //{
                //    int moocId = int.Parse(Request.Form["MoocId"]);

                //    var response = await _httpClient.DeleteAsync($"https://localhost:5000/api/Mooc/DeleteMooc/{moocId}");
                //    if (response.IsSuccessStatusCode)
                //    {
                //        return RedirectToPage("/Course-Detail");
                //    }
                //    else
                //    {
                //        var errorMessage = await response.Content.ReadAsStringAsync();
                //        ModelState.AddModelError(string.Empty, $"Failed to delete Mooc. Error message: {errorMessage}");
                //    }
                //}

                return Page();



            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteMoocAsync(int moocId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    HttpClient _httpClient = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Delete, $"https://localhost:5000/api/Mooc/DeleteMooc/{moocId}");
                    string? jwtToken = HttpContext.Request.Cookies["jwt_token"];
                    request.Headers.Add("Authorization", $"Bearer {jwtToken}");
                    var response = await _httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Delete Mooc Successfuly";
                        return RedirectToPage("/Course-Detail");
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = $"Failed to delete Mooc. Error message: {errorMessage}";
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }

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
					if (response.IsSuccessStatusCode)
					{
						TempData["SuccessMessage"] = "Add Category successfully.";
						return RedirectToPage();
					}
					else
					{
						ModelState.AddModelError(string.Empty, "An error occurred while adding the category.");
					}
				}
				return Page();
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
				return Page();
			}

		}




	}

}
