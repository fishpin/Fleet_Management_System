using Microsoft.AspNetCore.Identity;

// Creates default login accounts on first run if they don't already exist.
public static class SeedUsers
{
    public static async Task InitializeUsers(UserManager<IdentityUser> userManager)
    {
        try
        {
            // Default credentials for demo/testing — change before production use
            var defaultUsers = new[]
            {
                new { Email = "user@example.com", Password = "Test@123" },
                new { Email = "admin@example.com", Password = "Admin@123" },
                new { Email = "manager@example.com", Password = "Manager@123" }
            };

            foreach (var userInfo in defaultUsers)
            {
                var user = await userManager.FindByEmailAsync(userInfo.Email);
                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = userInfo.Email,
                        Email = userInfo.Email,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, userInfo.Password);
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create user {userInfo.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"User seeding failed: {ex.Message}", ex);
        }
    }
}