using FinalProject.Constants;
using FinalProject.Dto;
using FinalProject.Models;
using FinalProject.Security;
using FinalProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {

        private readonly Prn231_FinalProjectContext _context;
        //private readonly JwtService _jwtService;

        public CourseController(Prn231_FinalProjectContext context)
        {
            _context = context;
            //_jwtService = jwtService;
        }
        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
        // GET: api/Courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDTO>>> GetCourses()
        {
            var userPrincipal = AuthService.GetPrinciple(HttpContext); // Lấy thông tin người dùng từ JWT token

            IQueryable<Course> query = _context.Courses
                .Include(c => c.CourseCategories)
                .ThenInclude(cc => cc.Category);

            if (userPrincipal != null)
            {
                if (userPrincipal.Role == RoleName.TEACHER.ToString())
                {
                    // Lấy danh sách khóa học mà người dùng đã tạo (vai trò là giáo viên)
                    query = query.Where(c => c.Id == userPrincipal.Id);
                }
                // Nếu là học sinh, không cần thêm điều kiện, sẽ lấy toàn bộ danh sách khóa học
            }

            var courses = await query
                .Select(c => new CourseDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Image = c.Image,
                    Href = c.Href,
                    Category = c.CourseCategories.Any() ? new CategoryDTO
                    {
                        Id = c.CourseCategories.First().Category.Id,
                        Name = c.CourseCategories.First().Category.Name
                    } : null
                })
                .ToListAsync();

            return courses;
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult<CourseDTO>> GetCourseDetail(int id)
        {
            //var userPrincipal = AuthService.GetPrinciple(HttpContext); // Lấy thông tin người dùng từ JWT token

            var course = await _context.Courses
                .Include(c => c.CourseCategories)
                .ThenInclude(cc => cc.Category)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền truy cập chi tiết khóa học
            //if (userPrincipal.Role == RoleName.TEACHER.ToString())
            //{
            //    return Unauthorized();
            //}

            var courseDTO = new CourseDTO
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Image = course.Image,
                Href = course.Href,
                Category = course.CourseCategories.Any() ? new CategoryDTO
                {
                    Id = course.CourseCategories.First().Category.Id,
                    Name = course.CourseCategories.First().Category.Name
                } : null
            };

            return Ok(courseDTO);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Course>> GetCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            return course;
        }

        [HttpPost("addCourse")]
        public async Task<ActionResult<CourseDTO>> AddCourse(CourseCreateDTO courseCreateDTO)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == courseCreateDTO.CategoryName);
            if (category == null)
            {
                return BadRequest("Invalid category name.");
            }

            var course = new Course
            {
                Name = courseCreateDTO.Name,
                Description = courseCreateDTO.Description,
                Image = courseCreateDTO.Image,
                Href = courseCreateDTO.Href,
                CourseCategories = new List<CourseCategory>
        {
            new CourseCategory
            {
                Category = category
            }
        }
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var courseDTO = new CourseDTO
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Image = course.Image,
                Href = course.Href,
                Category = new CategoryDTO
                {
                    Id = category.Id,
                    Name = category.Name
                }
            };

            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, courseDTO);
        }

        [HttpPut("updateCourse/{id}")]
        public async Task<IActionResult> UpdateCourse(int id, CourseUpdateDTO courseUpdateDTO)
        {
            try
            {
                // Kiểm tra courseUpdateDTO có null không
                if (courseUpdateDTO == null)
                {
                    return BadRequest("Course update data is null.");
                }

                // Tìm khóa học cần cập nhật
                var course = await _context.Courses
                    .Include(c => c.CourseCategories)
                    .ThenInclude(cc => cc.Category)
                    .FirstOrDefaultAsync(c => c.Id == id);

                // Kiểm tra nếu không tìm thấy khóa học
                if (course == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin khóa học từ courseUpdateDTO
                course.Name = courseUpdateDTO.Name;
                course.Description = courseUpdateDTO.Description;
                course.Image = courseUpdateDTO.Image;
                course.Href = courseUpdateDTO.Href;

                // Tìm danh mục theo tên từ courseUpdateDTO
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == courseUpdateDTO.CategoryName);

                // Kiểm tra nếu danh mục không hợp lệ
                if (category == null)
                {
                    return BadRequest("Invalid category name.");
                }

                // Cập nhật hoặc thêm mới quan hệ giữa khóa học và danh mục
                var existingCourseCategory = course.CourseCategories.FirstOrDefault();
                if (existingCourseCategory != null)
                {
                    existingCourseCategory.Category = category;
                }
                else
                {
                    course.CourseCategories.Add(new CourseCategory
                    {
                        Category = category,
                        CourseId = course.Id
                    });
                }

                // Lưu các thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();

                // Trả về kết quả thành công
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Xử lý khi có xung đột dữ liệu
                if (!CourseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw; // Ném ngoại lệ khi có xung đột khác
                }
            }
        }
    }
}
