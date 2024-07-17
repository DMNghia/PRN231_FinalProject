using FinalProject.Constants;
using FinalProject.Dto.Response;
using FinalProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly Prn231_FinalProjectContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(Prn231_FinalProjectContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("totalStudent")]
        public IActionResult GetTotalStudent()
        {
            return Ok(new BaseResponse<int>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = _context.Users.Where(u => u.Role.Equals(RoleName.STUDENT.ToString()) && u.IsActive == true).Count()
            });
        }

        [HttpGet("totalTeacher")]
        public IActionResult GetTotalTeacher()
        {
            return Ok(new BaseResponse<int>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = _context.Users.Where(u => u.Role.Equals(RoleName.TEACHER.ToString()) && u.IsActive == true).Count()
            });
        }
    }
}
