
using DemoMVC.Data;
using DemoMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoMVC.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Departments
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments
                .AsNoTracking()
                .ToListAsync();

            return View(departments);
        }

        // GET: /Departments/Details/1
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: /Departments/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (!ModelState.IsValid)
            {
                return View(department);
            }

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Thêm khoa thành công!";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Departments/Edit/1
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: /Departments/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Department department)
        {
            if (id != department.DepartmentId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(department);
            }

            _context.Departments.Update(department);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Cập nhật khoa thành công!";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Departments/Delete/1
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: /Departments/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Students)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null)
            {
                return NotFound();
            }

            if (department.Students.Any())
            {
                TempData["Message"] =
                    "Không thể xóa khoa vì vẫn còn sinh viên thuộc khoa này.";

                return RedirectToAction(nameof(Index));
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Xóa khoa thành công!";

            return RedirectToAction(nameof(Index));
        }
    }
}
