using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WIPSystem.Web.Data;
using WIPSystem.Web.Models;

namespace WIPSystem.Web.Controllers
{
    [Authorize(Roles = "Admin, Super Admin")]
    public class UsersController : Controller
    {
        private readonly WIPDbContext _wIPDbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(WIPDbContext wIPDbContext, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _wIPDbContext = wIPDbContext;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var loggedInUser = await _userManager.GetUserAsync(User);
            var loggedInUserEntity = await _wIPDbContext.UserEntities
                                         .Include(u => u.Department)
                                         .Include(u => u.Process)
                                         .FirstOrDefaultAsync(u => u.username == loggedInUser.UserName);

            if (loggedInUserEntity == null)
            {
                // Handle unauthorized access
                return RedirectToAction("AccessDenied", "Account");
            }

            // Check if the logged-in user is a Super Admin
            bool isSuperAdmin = loggedInUserEntity.Role == "Super Admin";
            ViewData["IsSuperAdmin"] = isSuperAdmin;

            // Assume that the admin role check is based on a role named "Admin" 
            // and that admins only manage users within the same process.
            bool isAdminOfProcess = loggedInUserEntity.Role == "Admin" &&
                                    // This is a placeholder, you'll need to implement the actual check 
                                    // depending on how you determine the admin of a process.
                                    _wIPDbContext.UserEntities.Any(pa => pa.UserId == loggedInUserEntity.UserId && pa.ProcessId == loggedInUserEntity.ProcessId);

            ViewData["IsAdminOfProcess"] = isAdminOfProcess;
            // Set CanEdit to true if the user is a Super Admin or an Admin of the current process
            ViewData["CanEdit"] = isSuperAdmin || isAdminOfProcess;

            IQueryable<UserEntity> users;

            if (isSuperAdmin)
            {
                // Super Admin can see all users
                users = _wIPDbContext.UserEntities
                         .Include(u => u.Department)
                         .Include(u => u.Process);
            }
            else if (isAdminOfProcess)
            {
                // Admins can only see users from the same process
                var currentUserProcessId = loggedInUserEntity.ProcessId;
                users = _wIPDbContext.UserEntities
                         .Include(u => u.Department)
                         .Include(u => u.Process)
                         .Where(u => u.ProcessId == currentUserProcessId);
            }
            else
            {
                // Non-admin users might have different visibility or no visibility at all.
                // Handle this case as required by your application's business logic.
                // For example, you might redirect to an error page, show only the logged-in user, etc.
                return RedirectToAction("AccessDenied", "Account");
            }

            return View(await users.ToListAsync());
        }


        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _wIPDbContext.UserEntities == null)
            {
                return NotFound();
            }

            var userEntity = await _wIPDbContext.UserEntities
                .Include(u => u.Department)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (userEntity == null)
            {
                return NotFound();
            }

            //return View(userEntity);
            // Return the PartialView instead of a full view
            return PartialView("_DetailsModal", userEntity);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["DepartmentName"] = new SelectList(_wIPDbContext.Departments, "DepartmentId", "DepartmentName");
            ViewData["Roles"] = new SelectList(_roleManager.Roles, "Name", "Name");
            ViewData["Processes"] = new SelectList(_wIPDbContext.Process, "ProcessId", "ProcessName");
            return View();
        }
        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("username,password,employeeNo,DepartmentId,Role,ProcessId,Email")] UserEntity userEntity)
        {
            if (ModelState.IsValid)
            {
                // Create a new IdentityUser instance for the new user
                var identityUser = new IdentityUser { UserName = userEntity.username }; // Use a valid email
                var createResult = await _userManager.CreateAsync(identityUser, userEntity.password);

                if (createResult.Succeeded)
                {
                    // Check if the role exists and assign the role to the newly created user
                    if (!string.IsNullOrEmpty(userEntity.Role))
                    {
                        var roleExist = await _roleManager.RoleExistsAsync(userEntity.Role);
                        if (roleExist)
                        {
                            await _userManager.AddToRoleAsync(identityUser, userEntity.Role);
                        }
                    }

                    // If the user role is either Admin or Super Admin, prompt the user to enter their email
                    if (userEntity.Role == "Admin" || userEntity.Role == "Super Admin")
                    {
                        // Check if email is provided
                        if (string.IsNullOrEmpty(userEntity.Email))
                        {
                            ModelState.AddModelError(string.Empty, "Please enter your email address.");
                            ViewData["DepartmentId"] = new SelectList(_wIPDbContext.Departments, "DepartmentId", "DepartmentName", userEntity.DepartmentId);
                            ViewData["Roles"] = new SelectList(_roleManager.Roles, "Name", "Name", userEntity.Role);
                            ViewData["Processes"] = new SelectList(_wIPDbContext.Process, "ProcessId", "ProcessName", userEntity.ProcessId);
                            return View(userEntity);
                        }
                    }

                    // Create and save the UserEntity with additional details
                    _wIPDbContext.UserEntities.Add(userEntity);
                    await _wIPDbContext.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Handle errors, e.g., invalid password, duplicate username
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // If we got this far, something failed, so re-display form
            ViewData["DepartmentId"] = new SelectList(_wIPDbContext.Departments, "DepartmentId", "DepartmentName", userEntity.DepartmentId);
            ViewData["Roles"] = new SelectList(_roleManager.Roles, "Name", "Name", userEntity.Role);
            ViewData["Processes"] = new SelectList(_wIPDbContext.Process, "ProcessId", "ProcessName", userEntity.ProcessId);
            return View(userEntity);
        }


        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _wIPDbContext.UserEntities == null)
            {
                return NotFound();
            }

            var userEntity = await _wIPDbContext.UserEntities.FindAsync(id);
            if (userEntity == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(_wIPDbContext.Departments, "DepartmentId", "DepartmentId", userEntity.DepartmentId);
            ViewData["Email"] = userEntity.Email; // Add email to ViewData
            return View(userEntity);
        }

      // POST: Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("UserId,username,password,employeeNo,DepartmentId,Role,ProcessId,Email")] UserEntity userEntity)
    {
    if (id != userEntity.UserId)
    {
        return NotFound();
    }

    if (ModelState.IsValid)
    {
        try
        {
            // Retrieve the existing user
            var existingUser = await _wIPDbContext.UserEntities.FindAsync(id);

            if (existingUser != null)
            {
                // Update the properties of the existing user
                existingUser.username = userEntity.username;
                // Do not update the password directly like this, you should use UserManager to change passwords.
                existingUser.employeeNo = userEntity.employeeNo;
                existingUser.DepartmentId = userEntity.DepartmentId;
                existingUser.Role = userEntity.Role;
                existingUser.ProcessId = userEntity.ProcessId;
                existingUser.Email = userEntity.Email;

                        // Update the user in the database
                 _wIPDbContext.Update(existingUser);
                await _wIPDbContext.SaveChangesAsync();
            }
            else
            {
                return NotFound();
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserEntityExists(userEntity.UserId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return RedirectToAction(nameof(Index));
    }
    // If we got this far, something failed, so re-display the form
    ViewData["DepartmentId"] = new SelectList(_wIPDbContext.Departments, "DepartmentId", "DepartmentName", userEntity.DepartmentId);
    ViewData["ProcessId"] = new SelectList(_wIPDbContext.Process, "ProcessId", "ProcessName", userEntity.ProcessId);
    return View(userEntity);
}


        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _wIPDbContext.UserEntities == null)
            {
                return NotFound();
            }

            var userEntity = await _wIPDbContext.UserEntities
                .Include(u => u.Department)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (userEntity == null)
            {
                return NotFound();
            }

            return View(userEntity);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_wIPDbContext.UserEntities == null)
            {
                return Problem("Entity set 'WIPDbContext.UserEntities'  is null.");
            }
            var userEntity = await _wIPDbContext.UserEntities.FindAsync(id);
            if (userEntity != null)
            {
                _wIPDbContext.UserEntities.Remove(userEntity);
            }
            
            await _wIPDbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserEntityExists(int id)
        {
          return _wIPDbContext.UserEntities.Any(e => e.UserId == id);
        }
    }
}
