using Microsoft.AspNetCore.Mvc;
using WIPSystem.Web.Models;
using WIPSystem.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
// other necessary usings

namespace WIPSystem.Web.Controllers
{
    [Authorize(Roles = "Admin, Super Admin")]
    public class DepartmentController : Controller
    {
        private readonly WIPDbContext _wIPDbContext;

        public DepartmentController(WIPDbContext wIPDbContext)
        {
            _wIPDbContext = wIPDbContext;
        }

        // Add Department
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                _wIPDbContext.Departments.Add(department);
                await _wIPDbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        //List and View Departments
        public async Task<IActionResult> Index()
        {
            var departments = await _wIPDbContext.Departments.ToListAsync();
            return View(departments);
        }

        //Edit a Department
        [HttpGet]
        public async Task<IActionResult> Edit(int? DepartmentId)
        {
            if (DepartmentId == null)
            {
                return NotFound();
            }

            var department = await _wIPDbContext.Departments.FindAsync(DepartmentId);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int DepartmentId, Department department)
        {
            if (DepartmentId != department.DepartmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _wIPDbContext.Update(department);
                    await _wIPDbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.DepartmentId))
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
            return View(department);
        }

        private bool DepartmentExists(int DepartmentId)
        {
            return _wIPDbContext.Departments.Any(e => e.DepartmentId == DepartmentId);
        }

        //Remove a Department
        [HttpGet]
        public async Task<IActionResult> Delete(int? DepartmentId)
        {
            if (DepartmentId == null)
            {
                return NotFound();
            }

            var department = await _wIPDbContext.Departments
                .FirstOrDefaultAsync(m => m.DepartmentId == DepartmentId);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int DepartmentId)
        {
            var department = await _wIPDbContext.Departments.FindAsync(DepartmentId);
            _wIPDbContext.Departments.Remove(department);
            await _wIPDbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
