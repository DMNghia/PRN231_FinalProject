using FinalProject.Constants;
using FinalProject.Dto;
using FinalProject.Dto.Request;
using FinalProject.Dto.Response;
using FinalProject.Models;
using FinalProject.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using System.Linq;
using System.Text.Json;

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

        [HttpPost("add")]
        public IActionResult AddCourse([FromBody] AddCourseRequest request)
        {
            UserLoginPrinciple principle = AuthService.GetPrinciple(HttpContext);
            var categories = _context.Categories.Where(cate => request.Categories.Contains(cate.Href));
            Course? existHrefCourse = _context.Courses.FirstOrDefault(c => c.Href.Equals(request.Href));
            if (existHrefCourse != null)
            {
                _logger.LogInformation($"ADD NEW COURSE REQUEST: {JsonSerializer.Serialize(request)} >>> ERROR HREF EXISTS");
                return BadRequest(new BaseResponse<object>
                {
                    code = ResponseCode.ERROR.GetHashCode(),
                    message = "Href đã tồn tại"
                });
            }
            Course newCourse = new Course
            {
                Name = request.Name,
                Description = request.Description,
                Image = request.Image,
                Href = request.Href,
                CreateAt = DateTime.Now
            };
            _context.Courses.Add(newCourse);
            _context.SaveChanges();
            _context.CourseCategories.AddRange(categories.Select(cate => new CourseCategory
            {
                CategoryId = cate.Id,
                CourseId = newCourse.Id
            }));
            _context.UserCourses.Add(new UserCourse
            {
                CourseId = newCourse.Id,
                UserId = principle.Id
            });
            _context.SaveChanges();
            return Ok(new BaseResponse<object>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Tạo khóa học thành công"
            });
        }

        [HttpPost("add-course-enroll")]
        public IActionResult AddCourseEnroll(string hrefCourse, string? hrefLesson)
        {
            UserLoginPrinciple principle = AuthService.GetPrinciple(HttpContext);
            Course? course = _context.Courses.FirstOrDefault(c => c.Href.Equals(hrefCourse));
            if (course == null)
            {
                return NotFound(new BaseResponse<object>
                {
                    code = ResponseCode.ERROR.GetHashCode(),
                    message = "Thất bại"
                });
            }
            string href = $"/khoa-hoc/{hrefCourse}";
            if (hrefCourse != null)
            {
                Lesson? lesson = _context.Lessons.FirstOrDefault(l => l.Href.Equals(hrefLesson));
                if (lesson  == null)
                {
                    _logger.LogInformation($"ADD COURSE ENROLLED HREF COURSE: {hrefCourse}; HREF LESSON: {hrefLesson} >>> FAIL LESSON NOT EXIST");
                    return NotFound(new BaseResponse<object>
                    {
                        code = ResponseCode.ERROR.GetHashCode(),
                        message = "Thất bại"
                    });
                }
                href += $"/{hrefLesson}";
            }
            UserCourse? userCourse = _context.UserCourses.FirstOrDefault(uc => uc.UserId == principle.Id && uc.CourseId == course.Id);
            if (userCourse != null)
            {
                _logger.LogInformation($"ADD COURSE ENROLLED USER: {principle.Id}; HREF: {hrefCourse} >>> FAIL USER IS TEACHER");
                return BadRequest(new BaseResponse<object>
                {
                    code = ResponseCode.ERROR.GetHashCode(),
                    message = "Không thể thêm vì user là giáo viên"
                });
            }
            CourseEnrolled? courseEnrolled = _context.CourseEnrolleds.FirstOrDefault(ce => ce.UserId == principle.Id && ce.CourseId == course.Id);
            if (courseEnrolled == null)
            {
                _context.CourseEnrolleds.Add(new CourseEnrolled
                {
                    CourseId = course.Id,
                    Href = href,
                    UserId = principle.Id,
                    UpdateAt = DateTime.Now,
                });
            } else
            {
                courseEnrolled.Href = href;
                courseEnrolled.UpdateAt = DateTime.Now;
            }
            
            _context.SaveChanges();
            return Ok(new BaseResponse<object>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công"
            });
        }

        [HttpGet("detail/{href}")]
        public IActionResult GetDetail(string href)
        {
            UserLoginPrinciple principle = AuthService.GetPrinciple(HttpContext);
            Course? course = _context.Courses.Include(c => c.UserCourses).ThenInclude(uc => uc.User).Include(c => c.Moocs).ThenInclude(m => m.Lessons)
                .FirstOrDefault(c => c.Href.Equals(href));
            if (course == null )
            {
                _logger.LogInformation($"GET COURSE DETAIL HREF: {href} >>> ERROR NOT FOUND");
                return NotFound(new BaseResponse<GetAllCourseResponse>
                {
                    code = ResponseCode.ERROR.GetHashCode(),
                    message = "Không tìm thấy"
                });
            }
            _logger.LogInformation($"GET COURSE DETAIL HREF: {href} >>> SUCCESS");
            return Ok(new BaseResponse<GetCourseDetailResponse>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = new GetCourseDetailResponse
                {
                    Id = course.Id,
                    Name = course.Name,
                    Description = course.Description,
                    Href = course.Href,
                    Image = course.Image,
                    Teacher = course.UserCourses.Select(uc => new UserDto
                    {
                        Id = uc.User.Id,
                        FullName = uc.User.FullName
                    }).FirstOrDefault(),
                    Moocs = course.Moocs.Select(m => new MoocDTO
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        CourseId = course.Id,
                        Lessons = m.Lessons.Select(l => new LessonDTO
                        {
                            Id = l.Id,
                            Name = l.Name,
                            Description = l.Description,
                            Href = l.Href,
                            MoocId = l.MoocId,
                            VideoUrl = l.VideoUrl,
                            VideoTranscript = l.VideoTranscript
                        }).ToList(),
                    }).ToList()
                }
            });
        }

        [HttpGet("totalPage")]
        public IActionResult GetTotalPage(string? hrefCategory, string? searchKey)
        {
            var courses = _context.Courses.Include(c => c.Moocs).ThenInclude(m => m.Lessons).Include(c => c.CourseCategories).ThenInclude(cc => cc.Category)
                .Where(c => (searchKey != null ? c.Name.ToLower().Contains(searchKey) : true));
            courses = courses.Where(c => hrefCategory != null ? c.CourseCategories.Select(cc => cc.Category.Href).Contains(hrefCategory) : true);
            double totalPage = (double) courses.Count() / PAGE_LIMIT;
            return Ok(new BaseResponse<double>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = Math.Ceiling(totalPage)
            });
        }

        [HttpGet]
        public IActionResult Get(int? page, string? hrefCategory, string? searchKey)
        {
            int pageNo = 1;
            if (page != null)
            {
                pageNo = page.Value;
            }
            if (searchKey != null)
            {
                searchKey = searchKey.ToLower();
            }
            var courses = _context.Courses.Include(c => c.Moocs).ThenInclude(m => m.Lessons).Include(c => c.CourseCategories).ThenInclude(cc => cc.Category)
                .Where(c => (searchKey != null ? c.Name.ToLower().Contains(searchKey) : true));
            courses = courses.Where(c => hrefCategory != null ? c.CourseCategories.Select(cc => cc.Category.Href).Contains(hrefCategory) : true);
            return Ok(new BaseResponse<List<GetAllCourseResponse>>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = courses
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
                Href = ce.Href,
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
            return Ok(new BaseResponse<List<GetAllCourseResponse>>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = CourseEnrolled
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

        [HttpGet("totalPage/teacher-created")]
        public IActionResult GetTotalPageTeacherCreated(string? hrefCategory, string? searchKey)
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
            var courses = _context.Courses.Include(c => c.UserCourses).Include(c => c.CourseCategories).ThenInclude(cc => cc.Category)
                .Where(c => searchKey != null ? c.Name.ToLower().Contains(searchKey.ToLower()) : true);
            courses = courses.Where(c => c.UserCourses.Select(uc => uc.UserId).First() == principle.Id);
            courses = courses.Where(c => hrefCategory != null ? c.CourseCategories.Select(cc => cc.Category.Href).FirstOrDefault(href => href.Equals(hrefCategory)) != null : true);

            double totalPage = (double)courses.Count() / PAGE_LIMIT;
            return Ok(new BaseResponse<double>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = Math.Ceiling(totalPage)
            });
        }

        [HttpGet("teacher-created")]
        public IActionResult GetAllCourseCreated(int? page, string? hrefCategory, string? searchKey)
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
            int pageNo = 1;
            if (page != null)
            {
                pageNo = page.Value;
            }
            if (searchKey != null)
            {
                searchKey = searchKey.ToLower();
            }
            var courses = _context.Courses.Include(c => c.UserCourses).Include(c => c.CourseCategories).ThenInclude(cc => cc.Category)
               .Where(c => searchKey != null ? c.Name.ToLower().Contains(searchKey.ToLower()) : true);
            courses = courses.Where(c => c.UserCourses.Select(uc => uc.UserId).First() == principle.Id);
            courses = courses.Where(c => hrefCategory != null ? c.CourseCategories.Select(cc => cc.Category.Href).FirstOrDefault(href => href.Equals(hrefCategory)) != null : true);

            return Ok(new BaseResponse<List<GetAllCourseResponse>>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = courses
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
    }
}
