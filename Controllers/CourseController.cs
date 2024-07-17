using FinalProject.Constants;
using FinalProject.Dto;
using FinalProject.Dto.Response;
using FinalProject.Models;
using FinalProject.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly Prn231_FinalProjectContext _context;
        private readonly ILogger<CourseController> _logger;

        public CourseController(Prn231_FinalProjectContext context, ILogger<CourseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new BaseResponse<List<GetAllCourseResponse>>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = _context.Courses.Include(c => c.CourseCategories).Include(c => c.UserCourses)
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

        [HttpGet("popular")]
        public IActionResult GetPopularCourse()
        {
            var popularCourseEnrolled = _context.CourseEnrolleds.GroupBy(cc => cc.CourseId).Select(group => new
            {
                CourseId = group.Key,
                CourseCount = group.Count()
            })
            .OrderByDescending(x => x.CourseCount)
            .Skip(0) // Skip 0 elements
            .Take(3) // Take the top 3
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

        [HttpGet("GetAllByCategory")]
        public IActionResult GetByCategory(string hrefCategory)
        {
            return Ok(new BaseResponse<List<GetAllCourseResponse>>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = _context.Courses.Include(c => c.Moocs).ThenInclude(m => m.Lessons).Include(c => c.CourseCategories).ThenInclude(cc => cc.Category)
                .Where(c => c.CourseCategories.Select(cc => cc.Category.Href).FirstOrDefault(href => href.Equals(hrefCategory)) != null)
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
    }
}
