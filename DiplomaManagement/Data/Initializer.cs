using DiplomaManagement.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DiplomaManagement.Data
{
    public static class Initializer
    {
        private static string ADMIN_PASSWORD = "*Admin123";
        private static string DIRECTOR_PASSWORD = "*Dir123";
        public static async Task CreateAdminUser(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string adminEmail = "admin@example.com";

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

                var result = await userManager.CreateAsync(adminUser, ADMIN_PASSWORD);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        public static async Task PopulateInstituteTable(IServiceProvider serviceProvider)
        {
            var _context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            bool hasInstitutes = await _context.Institutes.AnyAsync();
            bool hasDirectors= await _context.Directors.AnyAsync();
            if (!hasInstitutes && !hasDirectors)
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

                var savedInstitutes = await _context.Institutes.ToListAsync();

                var directorList = new List<ApplicationUser>
                {
                    new ApplicationUser
                    {
                        UserName = "a.niewiadomski@gmail.com",
                        Email = "a.niewiadomski@gmail.com",
                        FirstName = "Artur",
                        LastName = "Niewiadomski",
                        EmailConfirmed = true,
                        InstituteId = savedInstitutes.FirstOrDefault(i => i.Name == "Instytut Informatyki")?.Id
                    },
                    new ApplicationUser
                    {
                        UserName = "a.swiderska@gmail.com",
                        Email = "a.swiderska@gmail.com",
                        FirstName = "Agnieszka",
                        LastName = "Gil - Świderska",
                        EmailConfirmed = true,
                        InstituteId = savedInstitutes.FirstOrDefault(i => i.Name == "Instytut Matematyki")?.Id
                    },
                    new ApplicationUser
                    {
                        UserName = "m.stanska@gmail.com",
                        Email = "m.stanska@gmail.com",
                        FirstName = "Marzena",
                        LastName = "Stańska",
                        EmailConfirmed = true,
                        InstituteId = savedInstitutes.FirstOrDefault(i => i.Name == "Instytut Nauk Biologicznych")?.Id
                    },
                    new ApplicationUser
                    {
                        UserName = "j.kopyra@gmail.com",
                        Email = "j.kopyra@gmail.com",
                        FirstName = "Janina",
                        LastName = "Kopyra",
                        EmailConfirmed = true,
                        InstituteId = savedInstitutes.FirstOrDefault(i => i.Name == "Instytut Nauk Chemicznych")?.Id
                    },
                    new ApplicationUser
                    {
                        UserName = "k.pakula@gmail.com",
                        Email = "k.pakula@gmail.com",
                        FirstName = "Krzysztof",
                        LastName = "Pakuła",
                        EmailConfirmed = true,
                        InstituteId = savedInstitutes.FirstOrDefault(i => i.Name == "Instytut Rolnictwa i Ogrodnictwa")?.Id
                    },
                    new ApplicationUser
                    {
                        UserName = "d.danaszewska@gmail.com",
                        Email = "d.danaszewska@gmail.com",
                        FirstName = "Dorota",
                        LastName = "Banaszewska",
                        EmailConfirmed = true,
                        InstituteId = savedInstitutes.FirstOrDefault(i => i.Name == "Instytut Zootechniki i Rybactwa")?.Id
                    },
                    new ApplicationUser
                    {
                        UserName = "e.nieczyporuk@gmail.com",
                        Email = "e.nieczyporuk@gmail.com",
                        FirstName = "Elżbieta",
                        LastName = "Krzęcio-Nieczyporuk",
                        EmailConfirmed = true,
                        InstituteId = savedInstitutes.FirstOrDefault(i => i.Name == "Instytut Nauk o Zdrowiu")?.Id
                    },
                    new ApplicationUser
                    {
                        UserName = "d.magier@gmail.com",
                        Email = "d.magier@gmail.com",
                        FirstName = "Dariusz",
                        LastName = "Magier",
                        EmailConfirmed = true,
                        InstituteId = savedInstitutes.FirstOrDefault(i => i.Name == "Instytut Historii")?.Id
                    },
                    new ApplicationUser
                    {
                        UserName = "s.sobieraj@gmail.com",
                        Email = "s.sobieraj@gmail.com",
                        FirstName = "Sławomir",
                        LastName = "Sobieraj",
                        EmailConfirmed = true,
                        InstituteId = savedInstitutes.FirstOrDefault(i => i.Name == "Instytut Językoznawstwa i Literaturoznawstwa")?.Id
                    },
                    new ApplicationUser
                    {
                        UserName = "a.klimaszewska@gmail.com",
                        Email = "a.klimaszewska@gmail.com",
                        FirstName = "Anna",
                        LastName = "Klim-Klimaszewska",
                        EmailConfirmed = true,
                        InstituteId = savedInstitutes.FirstOrDefault(i => i.Name == "Instytut Pedagogiki")?.Id
                    },
                    new ApplicationUser
                    {
                        UserName = "s.topolewski@gmail.com",
                        Email = "s.topolewski@gmail.com",
                        FirstName = "Stanisław",
                        LastName = "Topolewski",
                        EmailConfirmed = true,
                        InstituteId = savedInstitutes.FirstOrDefault(i => i.Name == "Instytut Nauk o Bezpieczeństwie")?.Id
                    },
                    new ApplicationUser
                    {
                        UserName = "a.indraszczyk@gmail.com",
                        Email = "a.indraszczyk@gmail.com",
                        FirstName = "Arkadiusz",
                        LastName = "Indraszczyk",
                        EmailConfirmed = true,
                        InstituteId = savedInstitutes.FirstOrDefault(i => i.Name == "Instytut Nauk o Polityce i Administracji")?.Id
                    },
                };

                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var directors = new List<Director>();

                foreach (var user in directorList)
                {
                    var result = await userManager.CreateAsync(user, DIRECTOR_PASSWORD);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Director");

                        Director director = new Director
                        {
                            DirectorUserId = user.Id
                        };

                        directors.Add(director);
                    }
                }

                _context.AddRange(directors);
                await _context.SaveChangesAsync();
            }
        }
    }
}
