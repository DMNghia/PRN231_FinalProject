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
            ViewData["lesson"] = lesson;
            ViewData["lessons"] = lessons;
            ViewData["hrefKhoaHoc"] = hrefKhoaHoc;

            //UserLoginPrinciple? principle = HttpContext.Items["principle"] as UserLoginPrinciple;
        }

       
    }
}
 