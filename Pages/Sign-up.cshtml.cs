using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Pages
{

    public class SignUpDto
    {
        public string FullName { get; set; }
        [EmailAddress]
        [Required(ErrorMessage = "Email not empty")]
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
    public class Sign_upModel : PageModel
    {
        [BindProperty]
        public SignUpDto SignUp { get; set; }
        public void OnGet()
        {
        }

        public PartialViewResult OnGetPartial()
        {
            if (ModelState.IsValid)
            {
                return new PartialViewResult();
            }
            return new PartialViewResult();
        }

        public async Task<IActionResult> OnPost()
        {
            return Page();
        }
    }
}
