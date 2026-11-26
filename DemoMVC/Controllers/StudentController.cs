
using DemoMVC.Data;
using DemoMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DemoMVC.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task LoadDepartmentsAsync(int? selectedDepartmentId = null)
        {
            ViewBag.DepartmentId = new SelectList(
                await _context.Departments
                    .OrderBy(d => d.DepartmentName)
                    .ToListAsync(),
                "DepartmentId",
                "DepartmentName",
                selectedDepartmentId
            );
        }

        private async Task<string> GenerateStudentCodeAsync()
        {
            const string prefix = "282105";

            var lastCode = await _context.Students
                .Where(s => s.StudentCode.StartsWith(prefix))
                .OrderByDescending(s => s.StudentCode)
                .Select(s => s.StudentCode)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastCode))
            {
                string sequence = lastCode.Substring(prefix.Length);

                if (int.TryParse(sequence, out int currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
            }

            if (nextNumber > 9999)
            {
                throw new InvalidOperationException(
                    "Đã hết số thứ tự cho nhóm mã sinh viên 282105."
                );
            }

            return $"{prefix}{nextNumber:D4}";
        }

        private static string NormalizeSearch(string? value)
        {
            return value?.Trim() ?? string.Empty;
        }

        public async Task<IActionResult> Index(
            string? keyword,
            int? departmentId,
            int page = 1,
            int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var normalizedKeyword = NormalizeSearch(keyword);

            var query = _context.Students
                .Include(s => s.Department)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(s =>
                    s.Id.ToString().Contains(normalizedKeyword)
                    || s.Fullname.Contains(normalizedKeyword)
                    || s.Email.Contains(normalizedKeyword)
                    || s.StudentCode.Contains(normalizedKeyword)
                    || s.University.Contains(normalizedKeyword)
                    || (s.Department != null &&
                        s.Department.DepartmentName.Contains(normalizedKeyword))
                );
            }

            if (departmentId.HasValue)
            {
                query = query.Where(s => s.DepartmentId == departmentId.Value);
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var students = await query
                .OrderBy(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var departments = await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName", departmentId);
            ViewBag.Keyword = normalizedKeyword;
            ViewBag.DepartmentId = departmentId;

            var result = new PagedResult<Student>
            {
                Items = students,
                PageNumber = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                SearchTerm = normalizedKeyword
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

            var student = await _context.Students
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DetailsModal", student);
            }

            return View(student);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDepartmentsAsync();
            return PartialView("_CreateModal", new Student());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            ModelState.Remove(nameof(student.StudentCode));

            if (student.DepartmentId.HasValue)
            {
                bool departmentExists = await _context.Departments
                    .AnyAsync(d => d.DepartmentId == student.DepartmentId.Value);

                if (!departmentExists)
                {
                    ModelState.AddModelError(
                        nameof(student.DepartmentId),
                        "Khoa được chọn không tồn tại."
                    );
                }
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

            student.StudentCode = await GenerateStudentCodeAsync();
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var createdStudent = await _context.Students
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.Id == student.Id);

            return Json(new AjaxResponse
            {
                Success = true,
                Message = "Thêm sinh viên thành công!",
                Data = new
                {
                    student.Id,
                    student.StudentCode,
                    student.Fullname,
                    student.Email,
                    student.Age,
                    student.GPA,
                    student.University,
                    DepartmentName = createdStudent?.Department?.DepartmentName ?? "Chưa phân khoa"
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            await LoadDepartmentsAsync(student.DepartmentId);
            return PartialView("_EditModal", student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.Id)
            {
                return Json(new AjaxResponse
                {
                    Success = false,
                    Message = "Mã sinh viên không hợp lệ!"
                });
            }

            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingStudent == null)
            {
                return Json(new AjaxResponse
                {
                    Success = false,
                    Message = "Không tìm thấy sinh viên cần sửa!"
                });
            }

            ModelState.Remove(nameof(student.StudentCode));

            if (student.DepartmentId.HasValue)
            {
                bool departmentExists = await _context.Departments
                    .AnyAsync(d => d.DepartmentId == student.DepartmentId.Value);

                if (!departmentExists)
                {
                    ModelState.AddModelError(
                        nameof(student.DepartmentId),
                        "Khoa được chọn không tồn tại."
                    );
                }
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

            existingStudent.Fullname = student.Fullname.Trim();
            existingStudent.Address = student.Address.Trim();
            existingStudent.Email = student.Email.Trim();
            existingStudent.Age = student.Age;
            existingStudent.GPA = student.GPA;
            existingStudent.University = student.University.Trim();
            existingStudent.DepartmentId = student.DepartmentId;

            await _context.SaveChangesAsync();

            return Json(new AjaxResponse
            {
                Success = true,
                Message = "Cập nhật sinh viên thành công!",
                Data = new
                {
                    existingStudent.Id,
                    existingStudent.StudentCode,
                    existingStudent.Fullname,
                    existingStudent.Email,
                    DepartmentName = (await _context.Departments
                        .Where(d => d.DepartmentId == existingStudent.DepartmentId)
                        .Select(d => d.DepartmentName)
                        .FirstOrDefaultAsync()) ?? "Chưa phân khoa"
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return PartialView("_DeleteModal", student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return Json(new AjaxResponse
                {
                    Success = false,
                    Message = "Không tìm thấy sinh viên cần xóa!"
                });
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return Json(new AjaxResponse
            {
                Success = true,
                Message = "Xóa sinh viên thành công!"
            });
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(s => s.Id == id);
        }
    }
}
