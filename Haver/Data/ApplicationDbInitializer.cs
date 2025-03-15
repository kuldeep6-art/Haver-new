using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace haver.Data
{
    public static class ApplicationDbInitializer
    {
        public static async void Initialize(IServiceProvider serviceProvider,
            bool UseMigrations = true, bool SeedSampleData = true)
        {
            #region Prepare the Database
            if (UseMigrations)
            {
                using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
                {
                    try
                    {
                        //Create the database if it does not exist and apply the Migration
                        context.Database.Migrate();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.GetBaseException().Message);
                    }
                }
            }
            #endregion

            #region Seed Sample Data 
            if (SeedSampleData)
            {
                //Create Roles
                using (var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>())
                {
                    try
                    {
                        string[] roleNames = { "Admin", "Sales", "Engineering", "Procurement", "Production", "PIC" };

                        IdentityResult roleResult;
                        foreach (var roleName in roleNames)
                        {
                            var roleExist = await roleManager.RoleExistsAsync(roleName);
                            if (!roleExist)
                            {
                                roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.GetBaseException().Message);
                    }
                }

                //Create Users
                using (var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>())
                {
                    try
                    {
                        string defaultPassword = "Pa55w@rd";

                        if (userManager.FindByEmailAsync("admin@outlook.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "admin@outlook.com",
                                Email = "admin@outlook.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

                            if (result.Succeeded)
                            {
                                userManager.AddToRoleAsync(user, "Admin").Wait();
                            }
                        }
                        if (userManager.FindByEmailAsync("sales@outlook.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "sales@outlook.com",
                                Email = "sales@outlook.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

                            if (result.Succeeded)
                            {
                                userManager.AddToRoleAsync(user, "Sales").Wait();
                            }
                        }
                        if (userManager.FindByEmailAsync("engineering@outlook.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "engineering@outlook.com",
                                Email = "engineering@outlook.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

                            if (result.Succeeded)
                            {
                                userManager.AddToRoleAsync(user, "Engineering").Wait();
                            }
                        }
                        if (userManager.FindByEmailAsync("procurement@outlook.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "prourement@outlook.com",
                                Email = "procurement@outlook.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

                            if (result.Succeeded)
                            {
                                userManager.AddToRoleAsync(user, "Procurement").Wait();
                            }
                        }
                        if (userManager.FindByEmailAsync("production@outlook.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "production@outlook.com",
                                Email = "production@outlook.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

                            if (result.Succeeded)
                            {
                                userManager.AddToRoleAsync(user, "Production").Wait();
                            }
                        }
                        if (userManager.FindByEmailAsync("pic@outlook.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "pic@outlook.com",
                                Email = "pic@outlook.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

                            if (result.Succeeded)
                            {
                                userManager.AddToRoleAsync(user, "PIC").Wait();
                            }
                        }
                        if (userManager.FindByEmailAsync("user@outlook.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "user@outlook.com",
                                Email = "user@outlook.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;
                            //Not in any role
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.GetBaseException().Message);
                    }
                }
            }
            #endregion
        }
    }
}
