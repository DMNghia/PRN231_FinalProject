using FinalProject.Dto;
using FinalProject.Dto.Response;
using FinalProject.Models;
using FinalProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text.Json;

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

            //get lesson infor 
            requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5000/api/Lesson/Lesson-detail?href={href}");
            requestMessage.Headers.Add("Authorization", $"Bearer {HttpContext.Request.Cookies["jwt_token"]}");
            HttpResponseMessage response1 = await client.SendAsync(requestMessage);
            var lesson = response1.Content.ReadFromJsonAsync<LessonDTO>().Result;
            ViewData["lesson"] = lesson;

            requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:5000/api/Mooc/GetMoocs");
            requestMessage.Headers.Add("Authorization", $"Bearer {HttpContext.Request.Cookies["jwt_token"]}");
            HttpResponseMessage response2 = client.SendAsync(requestMessage).Result;
            var moocs = response2.Content.ReadFromJsonAsync<List<MoocDTO>>().Result;
            ViewData["moocs"] = moocs;

            return Page();
        }

        public async Task<IActionResult> OnPost(string href, MoocDTO moocDTO)
        {
            try
            {
                HttpClient _httpClient = new HttpClient();

                if (Request.Form.TryGetValue("SubmitAddMooc", out var submitMoocValue) && submitMoocValue == "SubmitAddMooc")
                {
					moocDTO.Lessons = new List<LessonDTO>();
					var contentBody = new StringContent(JsonSerializer.Serialize(moocDTO), null, "application/json");
                    var request = new HttpRequestMessage(HttpMethod.Post, $"https://localhost:5000/api/Mooc/AddMooc?courseHref={href}");
                    request.Content = contentBody;
                    string? jwtToken = HttpContext.Request.Cookies["jwt_token"];
                    request.Headers.Add("Authorization", $"Bearer {jwtToken}");
                    var response = await _httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Add mooc successfully.";
                        return RedirectToPage(new { hrefKhoaHoc =  href });
                    }
                    else
                    {
						TempData["SuccessMessage"] = "Add mooc successfully.";
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

                return RedirectToPage();



            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return RedirectToPage();
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

            return RedirectToPage();
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
