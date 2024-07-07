using System.Net.Http;
using FinalProject.Constants;
using FinalProject.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinalProject.Pages
{
    public class LessonModel : PageModel
    {
        public void OnGet(string hrefKhoaHoc, string? hrefBaiHoc)
        {
            HttpClient _httpClient = new HttpClient();
            
            
            HttpResponseMessage response = _httpClient.GetAsync($"https://localhost:5000/api/Lesson/Lesson-detail?href={hrefBaiHoc}").Result;
            var lesson = response.Content.ReadFromJsonAsync<LessonDTO>().Result;
            HttpResponseMessage response1 = _httpClient.GetAsync($"https://localhost:5000/api/Lesson/ListLesson").Result;
            var lessons = response1.Content.ReadFromJsonAsync<List<LessonDTO>>().Result;
            HttpResponseMessage response2 = _httpClient.GetAsync($"https://localhost:5000/api/Mooc/GetMoocs").Result;
            var moocs = response2.Content.ReadFromJsonAsync<List<MoocDTO>>().Result;
            ViewData["lesson"] = lesson;
            ViewData["lessons"] = lessons;
            ViewData["hrefKhoaHoc"] = hrefKhoaHoc;
            ViewData["moocs"] = moocs;

            //UserLoginPrinciple? principle = HttpContext.Items["principle"] as UserLoginPrinciple;
        }

        public async Task<IActionResult> OnPost(string courseHref, MoocDTO moocDTO)
        {
            try
            {
                HttpClient _httpClient = new HttpClient();

                if (Request.Form.TryGetValue("SubmitAddMooc", out var submitMoocValue) && submitMoocValue == "SubmitAddMooc")
                {
                    var response = await _httpClient.PostAsJsonAsync($"https://localhost:5000/api/Mooc/AddMooc?courseHref={courseHref}", moocDTO);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToPage(new { hrefKhoaHoc = courseHref });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "An error occurred while adding the mooc.");
                    }
                }
                else if (Request.Form.TryGetValue("SubmitAddLesson", out var submitLessonValue) && submitLessonValue == "SubmitAddLesson")
                {
                    var existingHrefResponse = await _httpClient.GetAsync($"https://localhost:5000/api/Lesson/CheckHrefExists?href={Request.Form["Href"]}");
                    if (existingHrefResponse.IsSuccessStatusCode)
                    {
                        var content = await existingHrefResponse.Content.ReadAsStringAsync();
                        var isHrefExists = bool.Parse(content); 

                        if (isHrefExists)
                        {
                            ModelState.AddModelError(string.Empty, "Href already exists. Please choose a different one.");
                            return Page();

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
                            var responseLesson = await _httpClient.PostAsJsonAsync("https://localhost:5000/api/Lesson/AddLesson", lessonDTO);
                            if (responseLesson.IsSuccessStatusCode)
                            {
                                return RedirectToPage();
                            }
                            else
                            {
                                var errorMessageLesson = await responseLesson.Content.ReadAsStringAsync();
                                ModelState.AddModelError(string.Empty, $"Failed to add Lesson. Error message: {errorMessageLesson}");
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to check if href exists.");
                    }
                } else if(Request.Form.TryGetValue("SubmitEditLesson", out var submitEditLessonValue) && submitEditLessonValue == "SubmitEditLesson")
                {
                    
                    var existingHrefResponse = await _httpClient.GetAsync($"https://localhost:5000/api/Lesson/CheckHrefExists?href={Request.Form["Href"]}");
                    if (existingHrefResponse.IsSuccessStatusCode)
                    {
                        var content = await existingHrefResponse.Content.ReadAsStringAsync();
                        var isHrefExists = bool.Parse(content);

                        if (isHrefExists && Request.Form["Href"] != Request.Form["OldHref"])
                        {
                            ModelState.AddModelError(string.Empty, "Href already exists. Please choose a different one.");
                            return Page();
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

							
							var responseLesson = await _httpClient.PutAsJsonAsync($"https://localhost:5000/api/Lesson/EditLesson/{lessonDTO.Id}", lessonDTO);
							if (responseLesson.IsSuccessStatusCode)
                            {
                                return RedirectToPage();
                            }
                            else
                            {
                                var errorMessageLesson = await responseLesson.Content.ReadAsStringAsync();
                                ModelState.AddModelError(string.Empty, $"Failed to update Lesson. Error message: {errorMessageLesson}");
                            }
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to check if href exists.");
                    }
                }
                else if (Request.Form.TryGetValue("SubmitDeleteLesson", out var submitDeleteLessonValue) && submitDeleteLessonValue == "SubmitDeleteLesson")
                {
                    int lessonId = int.Parse(Request.Form["LessonId"]);

                    var response = await _httpClient.DeleteAsync($"https://localhost:5000/api/Lesson/DeleteLesson/{lessonId}");
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToPage("/Course-Detail");
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, $"Failed to delete Lesson. Error message: {errorMessage}");
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

					var response = await _httpClient.DeleteAsync($"https://localhost:5000/api/Mooc/DeleteMooc/{moocId}");
					if (response.IsSuccessStatusCode)
					{
						return RedirectToPage("/Course-Detail");
					}
					else
					{
						var errorMessage = await response.Content.ReadAsStringAsync();
						ModelState.AddModelError(string.Empty, $"Failed to delete Mooc. Error message: {errorMessage}");
					}
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
				}
			}

			// Trả về trang hiện tại với thông tin lỗi nếu có
			return Page();
		}


	}

}
 