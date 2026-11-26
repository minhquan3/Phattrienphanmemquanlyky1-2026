
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

        private static string NormalizeSearch(string? value)
        {
            return value?.Trim() ?? string.Empty;
        }

        // GET: /Departments
        public async Task<IActionResult> Index(
            string? searchTerm,
            int page = 1,
            int pageSize = 10)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 10;
            }

            var normalizedSearch = NormalizeSearch(searchTerm);

            var query = _context.Departments
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(normalizedSearch))
            {
                query = query.Where(d =>
                    d.DepartmentId.ToString().Contains(normalizedSearch)
                    || d.DepartmentName.Contains(normalizedSearch)
                    || (d.Description != null &&
                        d.Description.Contains(normalizedSearch))
                );
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var items = await query
                .OrderBy(d => d.DepartmentId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<Department>
            {
                Items = items,
                PageNumber = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                SearchTerm = normalizedSearch
            };

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(d => d.Students)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null)
            {
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DetailsModal", department);
            }

            return View(department);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_CreateModal", new Department());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToList();

                return Json(new AjaxResponse
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ!",
                    Errors = errors
                });
            }

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return Json(new AjaxResponse
            {
                Success = true,
                Message = "Thêm khoa thành công!",
                Data = new { department.DepartmentId, department.DepartmentName }
            });
        }

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

            return PartialView("_EditModal", department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            if (id != department.DepartmentId)
            {
                return Json(new AjaxResponse
                {
                    Success = false,
                    Message = "Mã khoa không hợp lệ!"
                });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToList();

                return Json(new AjaxResponse
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ!",
                    Errors = errors
                });
            }

            var existingDepartment = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (existingDepartment == null)
            {
                return Json(new AjaxResponse
                {
                    Success = false,
                    Message = "Không tìm thấy khoa cần sửa!"
                });
            }

            existingDepartment.DepartmentName = department.DepartmentName.Trim();
            existingDepartment.Description = department.Description?.Trim();

            await _context.SaveChangesAsync();

            return Json(new AjaxResponse
            {
                Success = true,
                Message = "Cập nhật khoa thành công!",
                Data = new { existingDepartment.DepartmentId, existingDepartment.DepartmentName }
            });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(d => d.Students)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null)
            {
                return NotFound();
            }

            return PartialView("_DeleteModal", department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Students)
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (department == null)
            {
                return Json(new AjaxResponse
                {
                    Success = false,
                    Message = "Không tìm thấy khoa cần xóa!"
                });
            }

            if (department.Students.Any())
            {
                return Json(new AjaxResponse
                {
                    Success = false,
                    Message = "Không thể xóa khoa vì vẫn còn sinh viên thuộc khoa này."
                });
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return Json(new AjaxResponse
            {
                Success = true,
                Message = "Xóa khoa thành công!"
            });
        }
    }
}
