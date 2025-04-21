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
                        //error message for when the migration fails
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
                        //list of all the roles for app
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

                //Create Users. all the default roles for the app
                using (var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>())
                {
                    try
                    {
                        //this is the default password which can be changed for the admin
                        string defaultPassword = "Pa55w@rd";

                        if (userManager.FindByEmailAsync("admin@haverniagara.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "admin@haverniagara.com",
                                Email = "admin@haverniagara.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

                            if (result.Succeeded)
                            {
                                userManager.AddToRoleAsync(user, "Admin").Wait();
                            }
                        }
                        if (userManager.FindByEmailAsync("sales@haverniagara.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "sales@haverniagara.com",
                                Email = "sales@haverniagara.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

                            if (result.Succeeded)
                            {
                                userManager.AddToRoleAsync(user, "Sales").Wait();
                            }
                        }
                        if (userManager.FindByEmailAsync("engineering@haverniagara.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "engineering@haverniagara.com",
                                Email = "engineering@haverniagara.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

                            if (result.Succeeded)
                            {
                                userManager.AddToRoleAsync(user, "Engineering").Wait();
                            }
                        }
                        if (userManager.FindByEmailAsync("procurement@haverniagara.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "pment@haverniagara.com",
                                Email = "pment@haverniagara.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

                            if (result.Succeeded)
                            {
                                userManager.AddToRoleAsync(user, "Procurement").Wait();
                            }
                        }
                        if (userManager.FindByEmailAsync("production@haverniagara.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "production@haverniagara.com",
                                Email = "production@haverniagara.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

                            if (result.Succeeded)
                            {
                                userManager.AddToRoleAsync(user, "Production").Wait();
                            }
                        }
                        if (userManager.FindByEmailAsync("pic@haverniagara.com").Result == null)
                        {
                            IdentityUser user = new IdentityUser
                            {
                                UserName = "pic@haverniagara.com",
                                Email = "pic@haverniagara.com",
                                EmailConfirmed = true
                            };

                            IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;

                            if (result.Succeeded)
                            {
                                userManager.AddToRoleAsync(user, "PIC").Wait();
                            }
                        }
                        //test role for the user but removed since other users can be added in through the app itself
                        //if (userManager.FindByEmailAsync("user@haverniagara.com").Result == null)
                        //{
                        //    IdentityUser user = new IdentityUser
                        //    {
                        //        UserName = "user@haverniagara.com",
                        //        Email = "user@haverniagara.com",
                        //        EmailConfirmed = true
                        //    };

                        //    IdentityResult result = userManager.CreateAsync(user, defaultPassword).Result;
                        //    //Not in any role
                        //}
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
