using System;
using Microsoft.AspNetCore.Identity;

namespace BoardingBase.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        string[] roles = {"Landlord", "Tenant", "Staff"};

        foreach (var role in roles)
        {
            if(!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }
}
