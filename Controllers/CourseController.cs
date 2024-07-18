using FinalProject.Constants;
using FinalProject.Dto;
using FinalProject.Dto.Response;
using FinalProject.Models;
using FinalProject.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using System.Linq;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly Prn231_FinalProjectContext _context;
        private readonly ILogger<CourseController> _logger;
        private readonly int PAGE_LIMIT = 6;

        public CourseController(Prn231_FinalProjectContext context, ILogger<CourseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("detail/{href}")]
        public IActionResult GetDetail(string href)
        {
            UserLoginPrinciple principle = AuthService.GetPrinciple(HttpContext);

            return Ok();
        }

        [HttpGet("totalPage")]
        public IActionResult GetTotalPage(string? hrefCategory)
        {
            int totalCourse = _context.Courses.Include(c => c.CourseCategories).ThenInclude(cc => cc.Category)
                .Where(c => hrefCategory != null ? c.CourseCategories.Select(cc => cc.Category.Href).FirstOrDefault(href => href.Equals(hrefCategory)) != null : true).Count();
            double totalPage = (double)totalCourse / PAGE_LIMIT;
            return Ok(new BaseResponse<double>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = Math.Ceiling(totalPage)
            });
        }

        [HttpGet]
        public IActionResult Get(int? page, string? hrefCategory)
        {
            int pageNo = 1;
            if (page != null)
            {
                pageNo = page.Value;
            }
            return Ok(new BaseResponse<List<GetAllCourseResponse>>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = _context.Courses.Include(c => c.Moocs).ThenInclude(m => m.Lessons).Include(c => c.CourseCategories).ThenInclude(cc => cc.Category)
                .Where(c => hrefCategory != null ? c.CourseCategories.Select(cc => cc.Category.Href).FirstOrDefault(href => href.Equals(hrefCategory)) != null : true)
                .Skip((pageNo - 1) * PAGE_LIMIT)
                .Take(PAGE_LIMIT)
                .Select(c => new GetAllCourseResponse
                {
                    Name = c.Name,
                    Description = c.Description,
                    Href = c.Href,
                    Image = c.Image,
                    Categories = c.CourseCategories.Select(cc => new CategoryDto
                    {
                        Name = cc.Category.Name,
                        Description = cc.Category.Description,
                        Href = cc.Category.Href
                    }).ToList(),
                    Teacher = new UserDto
                    {
                        Id = c.UserCourses.First().UserId,
                        FullName = c.UserCourses.First().User.FullName
                    }
                }).ToList()
            });
        }

        [HttpGet("totalCourse")]
        public IActionResult GetTotalCourse()
        {
            return Ok(new BaseResponse<int>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = _context.Courses.Count()
            });
        }

        [HttpGet("enrolleds")]
        public IActionResult GetEnrolledCourses()
        {
            UserLoginPrinciple? principle = AuthService.GetPrinciple(HttpContext);
            if (principle == null)
            {
                return Unauthorized(new BaseResponse<object>
                {
                    code = ResponseCode.ERROR.GetHashCode(),
                    message = "Vui lòng đăng nhập"
                });
            }
            var CourseEnrolled = _context.CourseEnrolleds.Include(ce => ce.Course).ThenInclude(c => c.CourseCategories).Include(ce => ce.Course).ThenInclude(c => c.UserCourses).OrderByDescending(ce => ce.UpdateAt).Where(ce => ce.UserId == principle.Id).Select(ce => new GetAllCourseResponse
            {
                Name = ce.Course.Name,
                Description = ce.Course.Description,
                Href = ce.Course.Href,
                Image = ce.Course.Image,
                Categories = ce.Course.CourseCategories.Select(cc => new CategoryDto
                {
                    Name = cc.Category.Name,
                    Description = cc.Category.Description,
                    Href = cc.Category.Href
                }).ToList(),
                Teacher = new UserDto
                {
                    Id = ce.Course.UserCourses.First().UserId,
                    FullName = ce.Course.UserCourses.First().User.FullName
                }
            }).ToList();
            return Ok();
        }

        [HttpGet("popular")]
        public IActionResult GetPopularCourse()
        {
            var popularCourseEnrolled = _context.CourseEnrolleds.GroupBy(cc => cc.CourseId).Select(group => new
            {
                CourseId = group.Key,
                CourseCount = group.Count()
            })
            .OrderByDescending(x => x.CourseCount)
            .Skip(0)
            .Take(3)
            .ToList();
            return Ok(new BaseResponse<List<GetAllCourseResponse>>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = _context.Courses.Include(c => c.CourseCategories).Include(c => c.UserCourses)
                .Where(c => popularCourseEnrolled.Select(p => p.CourseId).Contains(c.Id))
                .Select(c => new GetAllCourseResponse
                {
                    Name = c.Name,
                    Description = c.Description,
                    Href = c.Href,
                    Image = c.Image,
                    Categories = c.CourseCategories.Select(cc => new CategoryDto
                    {
                        Name = cc.Category.Name,
                        Description = cc.Category.Description,
                        Href = cc.Category.Href
                    }).ToList(),
                    Teacher = new UserDto
                    {
                        Id = c.UserCourses.First().UserId,
                        FullName = c.UserCourses.First().User.FullName
                    }
                }).ToList()
            });
        }

        [HttpGet("teacher-created")]
        public IActionResult GetAllCourseCreated()
        {
            UserLoginPrinciple? principle = AuthService.GetPrinciple(HttpContext);
            if (principle == null)
            {
                return Unauthorized(new BaseResponse<object>
                {
                    code = ResponseCode.ERROR.GetHashCode(),
                    message = "Vui lòng đăng nhập"
                });
            }
            if (principle.Role != RoleName.TEACHER.ToString())
            {
                return BadRequest(new BaseResponse<object>
                {
                    code = ResponseCode.ERROR.GetHashCode(),
                    message = "Có lỗi xảy ra"
                });
            }
            return Ok(_context.Courses.Where(c => c.UserCourses.Select(uc => uc.UserId).Contains(principle.Id)));
        }
    }
}
