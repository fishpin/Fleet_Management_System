using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;

public class DebugModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    public List<IdentityUser> Users { get; set; } = new();

    public DebugModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task OnGetAsync()
    {
        Users = _userManager.Users.ToList();
    }
}