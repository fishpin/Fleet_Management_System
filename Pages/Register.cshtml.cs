using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[AllowAnonymous]
[IgnoreAntiforgeryToken]
public class RegisterModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;

    public RegisterModel(UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [BindProperty]
    public string Email { get; set; }

    [BindProperty]
    public string Password { get; set; }

    [BindProperty]
    public string ConfirmPassword { get; set; }

    [BindProperty]
    public string StaffCode { get; set; }

    public string Message { get; set; } = "";
    public string MessageType { get; set; } = "info";

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword) || string.IsNullOrEmpty(StaffCode))
        {
            Message = "Please fill in all fields.";
            MessageType = "danger";
            return Page();
        }

        // Verify staff code
        string correctStaffCode = _configuration["StaffCode"] ?? "staff";
        if (StaffCode != correctStaffCode)
        {
            Message = "Invalid staff code. Please try again.";
            MessageType = "danger";
            return Page();
        }

        // Validate email format
        if (!Email.Contains("@") || !Email.Contains("."))
        {
            Message = "Please enter a valid email address.";
            MessageType = "danger";
            return Page();
        }

        // Check password length
        if (Password.Length < 6)
        {
            Message = "Password must be at least 6 characters long.";
            MessageType = "danger";
            return Page();
        }

        // Check passwords match
        if (Password != ConfirmPassword)
        {
            Message = "Passwords do not match.";
            MessageType = "danger";
            return Page();
        }

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(Email);
        if (existingUser != null)
        {
            Message = "An account with this email already exists.";
            MessageType = "warning";
            return Page();
        }

        // Create new user
        var user = new IdentityUser
        {
            UserName = Email,
            Email = Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, Password);
        if (result.Succeeded)
        {
            Message = "Account created successfully! Redirecting to login...";
            MessageType = "success";
            return RedirectToPage("Index");
        }

        // Show errors
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        Message = $"Registration failed: {errors}";
        MessageType = "danger";
        return Page();
    }
}