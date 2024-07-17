using FinalProject.Constants;
using FinalProject.Dto.Request;
using FinalProject.Dto.Response;
using FinalProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly Prn231_FinalProjectContext _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(Prn231_FinalProjectContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            _logger.LogInformation("GET ALL CATEGORIES >>> SUCCESS");
            return Ok(new BaseResponse<List<GetCategoryResponse>>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = _context.Categories.Select(c => new GetCategoryResponse
                {
                    Name = c.Name,
                    Description = c.Description,
                    Href = c.Href,
                }).ToList()
            });
        }

        [HttpGet("{href}")]
        public IActionResult GetByHref(string href)
        {
            Category? category = _context.Categories.FirstOrDefault(c => c.Href == href);
            if (category == null)
            {
                _logger.LogInformation($"GET CATEGORY BY HREF:{href} >>> ERROR NOT FOUND");
                return NotFound(new BaseResponse<GetCategoryResponse>
                {
                    code = ResponseCode.ERROR.GetHashCode(),
                    message = "Không thể tìm thấy"
                });
            }
            _logger.LogInformation($"GET CATEGORY BY HREF:{href} >>> SUCCESS");
            return Ok(new BaseResponse<GetCategoryResponse>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Thành công",
                data = new GetCategoryResponse
                {
                    Name = category.Name,
                    Description = category.Description,
                    Href = category.Href,
                }
            });
        }

        [HttpPost("Add")]
        public IActionResult Add(AddCategoryRequest request)
        {
            Category? category = _context.Categories.FirstOrDefault(c => c.Href == request.Href);
            if (category != null)
            {
                _logger.LogInformation($"ADD CATEGORY REQUEST: {JsonSerializer.Serialize(request)} >>> ERROR HREF EXISTS");
                return BadRequest(new BaseResponse<object>
                {
                    code = ResponseCode.ERROR.GetHashCode(),
                    message = "Thất bại href đã tồn tại vui lòng nhập href khác"
                });
            }
            Category newCategory = new Category
            {
                Name = request.Name,
                Description = request.Description,
                Href = request.Href,
            };
            _context.Categories.Add(newCategory);
            _context.SaveChanges();
            _logger.LogInformation($"ADD CATEGORY REQUEST: {JsonSerializer.Serialize(request)} >>> SUCCESS");
            return Ok(new BaseResponse<object>
            {
                code = ResponseCode.SUCCESS.GetHashCode(),
                message = "Tạo thành công"
            });
        }
    }
}
