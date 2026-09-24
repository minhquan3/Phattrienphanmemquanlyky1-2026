using DemoMVC.Data;
using DemoMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoMVC.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Dependency Injection
        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // READ - DANH SÁCH SINH VIÊN
        // GET: /Student
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var students = await _context.Students.ToListAsync();

            return View(students);
        }

        // =========================================================
        // READ - CHI TIẾT
        // GET: /Student/Details/5
        // =========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // =========================================================
        // CREATE - HIỂN THỊ FORM
        // GET: /Student/Create
        // =========================================================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // =========================================================
        // CREATE - XỬ LÝ FORM
        // POST: /Student/Create
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            // Kiểm tra Validation
            if (ModelState.IsValid)
            {
                _context.Students.Add(student);

                await _context.SaveChangesAsync();

                TempData["Message"] = "Thêm sinh viên thành công!";

                return RedirectToAction(nameof(Index));
            }

            // Nếu validation lỗi
            // trả lại dữ liệu đã nhập cho View
            return View(student);
        }

        // =========================================================
        // UPDATE - HIỂN THỊ FORM SỬA
        // GET: /Student/Edit/5
        // =========================================================
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

            return View(student);
        }

        // =========================================================
        // UPDATE - XỬ LÝ FORM SỬA
        // POST: /Student/Edit/5
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Student student)
        {
            // Kiểm tra ID trên URL
            // có trùng với ID của Student hay không
            if (id != student.Id)
            {
                return NotFound();
            }

            // Kiểm tra Validation
            if (!ModelState.IsValid)
            {
                return View(student);
            }

            try
            {
                _context.Update(student);

                await _context.SaveChangesAsync();

                TempData["Message"] = "Cập nhật sinh viên thành công!";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(student.Id))
                {
                    return NotFound();
                }

                throw;
            }
        }

        // =========================================================
        // DELETE - HIỂN THỊ TRANG XÁC NHẬN XÓA
        // GET: /Student/Delete/5
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // =========================================================
        // DELETE - THỰC HIỆN XÓA
        // POST: /Student/Delete/5
        // =========================================================
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

        // =========================================================
        // KIỂM TRA STUDENT CÓ TỒN TẠI HAY KHÔNG
        // =========================================================
        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}