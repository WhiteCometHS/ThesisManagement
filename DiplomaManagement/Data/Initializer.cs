using DiplomaManagement.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.IO;

namespace DiplomaManagement.Data
{
    public static class Initializer
    {
        public static async Task CreateAdminUser(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string adminEmail = "admin@example.com";
            string adminPassword = "*Admin123";

            var roles = await roleManager.Roles.AnyAsync();

            if (!roles)
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
                await roleManager.CreateAsync(new IdentityRole("Director"));
                await roleManager.CreateAsync(new IdentityRole("Promoter"));
                await roleManager.CreateAsync(new IdentityRole("Student"));
                await roleManager.CreateAsync(new IdentityRole("SeminarLeader"));
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "Admin",
                    Email = adminEmail,
                    FirstName = "Jan",
                    LastName = "Kowalski",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        public static async Task PopulateInstituteTable(IServiceProvider serviceProvider)
        {
            var _context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            List<Institute> institutes = await _context.Institutes.ToListAsync();
            if (institutes.Count == 0)
            {
                var instituteList = new List<Institute>
                {
                    new Institute { 
                        Name = "Instytut Informatyki",
                        SiteAddress = "www.ii.uws.edu.pl",
                        Street = "ul. 3 Maja 54",
                        City = "Siedlce",
                        PostalCode = "08-110",
                        Email = "sekretariat_ii@uws.edu.pl"
                    },
                    new Institute {
                        Name = "Instytut Matematyki",
                        SiteAddress = "www.im.uws.edu.pl",
                        Street = "ul. 3 Maja 54",
                        City = "Siedlce",
                        PostalCode = "08-110",
                        Email = "imif@uws.edu.pl"
                    },
                    new Institute {
                        Name = "Instytut Nauk Biologicznych",
                        SiteAddress = "www.inb.uws.edu.pl",
                        Street = "ul. Bolesława Prusa 14",
                        City = "Siedlce",
                        PostalCode = "08-110",
                        Email = "biologia@uws.edu.pl"
                    },
                    new Institute {
                        Name = "Instytut Nauk Chemicznych",
                        SiteAddress = "www.inc.uws.edu.pl",
                        Street = "ul. 3 Maja 54",
                        City = "Siedlce",
                        PostalCode = "08-110",
                        Email = "chemsekr@uws.edu.pl"
                    },
                    new Institute {
                        Name = "Instytut Rolnictwa i Ogrodnictwa",
                        SiteAddress = "www.irio.uws.edu.pl",
                        Street = "ul. Bolesława Prusa 14",
                        City = "Siedlce",
                        PostalCode = "08-110",
                        Email = "iro@uws.edu.pl"
                    },
                    new Institute {
                        Name = "Instytut Zootechniki i Rybactwa",
                        SiteAddress = "www.izir.uws.edu.pl",
                        Street = "ul. Bolesława Prusa 14",
                        City = "Siedlce",
                        PostalCode = "08-110",
                        Email = "izir@uws.edu.pl"
                    },
                    new Institute {
                        Name = "Instytut Nauk o Zdrowiu",
                        SiteAddress = "www.inoz.uws.edu.pl",
                        Street = "ul. Bolesława Prusa 14",
                        City = "Siedlce",
                        PostalCode = "08-110",
                        Email = "inoz@uws.edu.pl"
                    },
                    new Institute {
                        Name = "Instytut Historii",
                        SiteAddress = "www.ih.uws.edu.pl",
                        Street = "ul. Żytnia 39",
                        City = "Siedlce",
                        PostalCode = "08-110",
                        Email = "historia@uws.edu.pl"
                    },
                    new Institute {
                        Name = "Instytut Językoznawstwa i Literaturoznawstwa",
                        SiteAddress = "www.ijil.uws.edu.pl",
                        Street = "ul. Żytnia 39",
                        City = "Siedlce",
                        PostalCode = "08-110",
                        Email = "ijil@uws.edu.pl"
                    },
                    new Institute {
                        Name = "Instytut Pedagogiki",
                        SiteAddress = "www.ijil.uws.edu.pl",
                        Street = "ul. Żytnia 39",
                        City = "Siedlce",
                        PostalCode = "08-110",
                        Email = "edagogika@uws.edu.pl"
                    },
                    new Institute {
                        Name = "Instytut Nauk o Bezpieczeństwie",
                        SiteAddress = "www.inob.uws.edu.pl",
                        Street = "ul. Żytnia 39",
                        City = "Siedlce",
                        PostalCode = "08-110",
                        Email = "bezpieczenstwo@uws.edu.pl"
                    },
                    new Institute {
                        Name = "Instytut Nauk o Polityce i Administracji",
                        SiteAddress = "www.inpa.uws.edu.pl",
                        Street = "ul. Żytnia 39",
                        City = "Siedlce",
                        PostalCode = "08-110",
                        Email = "inpa@uws.edu.pl"
                    },
                };

                _context.Institutes.AddRange(instituteList);
                await _context.SaveChangesAsync();
            }
        }
    }
}
