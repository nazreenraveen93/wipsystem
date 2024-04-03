using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WIPSystem.Web.Data;
using WIPSystem.Web.ViewModels;

namespace WIPSystem.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly WIPDbContext _wIPDbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<AccountController> _logger; // Add this line

        public AccountController(
            WIPDbContext wIPDbContext,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
             ILogger<AccountController> logger)
        {
            _wIPDbContext = wIPDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger; // Initialize the logger
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Set the returnUrl to the Current Product Status page by default
            returnUrl = returnUrl ?? Url.Action("Index", "CurrentStatus");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.Username);
                    if (user != null)
                    {
                        // Log successful login
                        _logger.LogInformation($"User {model.Username} logged in.");

                        // Get the user's associated process
                        var userEntity = await _wIPDbContext.UserEntities
                            .Include(u => u.Process)
                            .FirstOrDefaultAsync(u => u.username == model.Username);

                        if (userEntity?.Process != null)
                        {
                            // Construct the returnUrl based on the user's process
                            string processRedirectUrl = Url.Action("Index", userEntity.Process.ProcessName);
                            return LocalRedirect(processRedirectUrl);
                        }
                        else
                        {
                            // Redirect to a default page if no process is associated
                            return RedirectToAction("Index", "CurrentStatus");
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"Failed login attempt for user {model.Username}.");
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out."); // If you want to log the logout event
            return RedirectToAction("Login"); // Redirect to the login page or home page as per your application flow
        }

    }

}
