using BookStore.DataAccess;
using BookStore.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Models.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        readonly RoleManager<IdentityRole> _roleManager;
        readonly DbContextApp _context;
        readonly IConfiguration _config;
        ApplicationUser _adminUser;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            DbContextApp context,
            IConfiguration config,
            IOptionsMonitor<ApplicationUser> options)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _config = config;
            _adminUser = options.CurrentValue;
        }

        public void Initialize()
        {
            //migrations if they aren't applied
            try
            {
                if(_context.Database.GetPendingMigrations().Count() > 0)
                {
                    _context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {

            }
            //create roles if they are not created
            if (!(_roleManager.RoleExistsAsync(SD.RoleAdmin).GetAwaiter().GetResult()))
            {
                _roleManager.CreateAsync(new IdentityRole(SD.RoleAdmin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.RoleUserIndividual)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.RoleUserCompany)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.RoleEmployee)).GetAwaiter().GetResult();

                //if roles are not created create admin user

                _userManager.CreateAsync(_adminUser, _config.GetSection("AdminPassword").Get<string>()).GetAwaiter().GetResult();
                _adminUser = _context.ApplicationUsers.FirstOrDefault(u => u.Email == _adminUser.Email);
                _userManager.AddToRoleAsync(_adminUser, SD.RoleAdmin).GetAwaiter().GetResult();
            }


            return;
        }
    }
}
