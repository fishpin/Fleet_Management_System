using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

[AllowAnonymous]
[ValidateAntiForgeryToken]
public class IndexModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    public class LoginInputModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public async Task OnGetAsync()
    {
        if (_signInManager.IsSignedIn(User))
        {
            Response.Redirect("/Vehicle/Index");
            return;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName, 
            Input.Password, 
            isPersistent: false,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return Redirect("/Vehicle/Index");
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return Page();
    }
}