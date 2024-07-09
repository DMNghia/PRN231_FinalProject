﻿using System.Net.Http;
using System.Text.Json;
using FinalProject.Constants;
using FinalProject.Dto;
using FinalProject.Dto.Response;
using FinalProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;

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
                    var contentBody = new StringContent(JsonSerializer.Serialize(moocDTO), null, "application/json");
                    var request = new HttpRequestMessage(HttpMethod.Post, $"https://localhost:5000/api/Mooc/AddMooc?courseHref={courseHref}");
                    request.Content = contentBody;
                    string? jwtToken = HttpContext.Request.Cookies["jwt_token"];
                    request.Headers.Add("Authorization", $"Bearer {jwtToken}");
                    var response = await _httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Add mooc successfully.";
                        return RedirectToPage(new { hrefKhoaHoc = courseHref });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "An error occurred while adding the mooc.");
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




    }

}
