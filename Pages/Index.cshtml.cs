using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[AllowAnonymous]
[IgnoreAntiforgeryToken]
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
    public string Email { get; set; }

    [BindProperty]
    public string Password { get; set; }

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
        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
        {
            ModelState.Clear();
            ModelState.AddModelError(string.Empty, "Email and password are required.");
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Email);
        if (user == null)
        {
            ModelState.Clear();
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName, 
            Password, 
            isPersistent: false,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return Redirect("/Vehicle/Index");
        }

        ModelState.Clear();
        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return Page();
    }
}