
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

        // Nạp danh sách khoa cho form thêm và sửa
        private async Task LoadDepartmentsAsync(
            int? selectedDepartmentId = null)
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

        // Tự động sinh mã sinh viên theo tiền tố 282105
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

        // READ: Danh sách sinh viên
        // GET: /Student
        public async Task<IActionResult> Index()
        {
            var students = await _context.Students
                .Include(s => s.Department)
                .OrderBy(s => s.Id)
                .ToListAsync();

            return View(students);
        }

        // READ: Chi tiết sinh viên
        // GET: /Student/Details/5
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

            return View(student);
        }

        // CREATE: Hiển thị form thêm sinh viên
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDepartmentsAsync();
            return View();
        }

        // CREATE: Xử lý thêm sinh viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            // Không nhận mã sinh viên do người dùng gửi lên
            ModelState.Remove(nameof(student.StudentCode));

            if (student.DepartmentId.HasValue)
            {
                bool departmentExists = await _context.Departments
                    .AnyAsync(d =>
                        d.DepartmentId == student.DepartmentId.Value);

                if (!departmentExists)
                {
                    ModelState.AddModelError(
                        nameof(student.DepartmentId),
                        "Khoa được chọn không tồn tại."
                    );
                }
            }

            if (ModelState.IsValid)
            {
                student.StudentCode = await GenerateStudentCodeAsync();

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Thêm sinh viên thành công!";
                return RedirectToAction(nameof(Index));
            }

            await LoadDepartmentsAsync(student.DepartmentId);
            return View(student);
        }

        // UPDATE: Hiển thị form sửa sinh viên
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
            return View(student);
        }

        // UPDATE: Xử lý sửa sinh viên, giữ nguyên mã sinh viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            // Lấy bản ghi đang có trong database
            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingStudent == null)
            {
                return NotFound();
            }

            // Không kiểm tra mã sinh viên do form gửi lên
            ModelState.Remove(nameof(student.StudentCode));

            if (student.DepartmentId.HasValue)
            {
                bool departmentExists = await _context.Departments
                    .AnyAsync(d =>
                        d.DepartmentId == student.DepartmentId.Value);

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
                student.StudentCode = existingStudent.StudentCode;
                await LoadDepartmentsAsync(student.DepartmentId);
                return View(student);
            }

            try
            {
                // Chỉ cập nhật các trường được phép sửa
                existingStudent.Fullname = student.Fullname;
                existingStudent.Address = student.Address;
                existingStudent.Email = student.Email;
                existingStudent.Age = student.Age;
                existingStudent.GPA = student.GPA;
                existingStudent.University = student.University;
                existingStudent.DepartmentId = student.DepartmentId;

                // Giữ nguyên existingStudent.StudentCode
                await _context.SaveChangesAsync();

                TempData["Message"] = "Cập nhật sinh viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }

                throw;
            }
        }

        // DELETE: Hiển thị trang xác nhận xóa
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

            return View(student);
        }

        // DELETE: Thực hiện xóa
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Xóa sinh viên thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Kiểm tra sinh viên có tồn tại không
        private bool StudentExists(int id)
        {
            return _context.Students.Any(s => s.Id == id);
        }
    }
}
