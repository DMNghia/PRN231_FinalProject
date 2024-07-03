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
                    // Process Lesson addition
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
                        //phan nay van chua ok phai hoi NghiaDM vi ko tra ve page
                        return RedirectToPage(new { hrefKhoaHoc = courseHref });
                    }
                    else
                    {
                        var errorMessageLesson = await responseLesson.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, $"Failed to add Lesson. Error message: {errorMessageLesson}");
                    }
                }

                // Handle any other cases or errors here
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
 