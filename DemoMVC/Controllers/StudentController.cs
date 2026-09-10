using Microsoft.AspNetCore.Mvc;
using DemoMVC.Models;
using System.Text.Json;

namespace DemoMVC.Controllers
{
    public class StudentController : Controller
    {
        // GET: /Student
        public IActionResult Index()
        {
            ViewBag.Title = "Nhập thông tin sinh viên";

            ViewData["University"] = "Đại học Công nghệ";

            return View();
        }

        // GET: /Student/Information
        public IActionResult Information()
        {
            ViewBag.Title = "Thông tin sinh viên";

            ViewBag.Description = "Thông tin được gửi từ Controller bằng ViewBag";

            ViewData["University"] = "Đại học Công nghệ";
            ViewData["Note"] = "Đây là dữ liệu được truyền bằng ViewData";

            return View();
        }

        // POST: /Student/Submit
        [HttpPost]
        public IActionResult Submit(Student student)
        {
            // Kiểm tra dữ liệu
            if (string.IsNullOrWhiteSpace(student.Fullname) ||
                string.IsNullOrWhiteSpace(student.Address) ||
                string.IsNullOrWhiteSpace(student.University))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin sinh viên.";

                return View("Index", student);
            }

            // Lưu thông báo vào TempData
            TempData["Message"] = "Thêm thông tin sinh viên thành công!";

            // Lưu tạm Model để trang Success nhận lại
            TempData["Student"] = JsonSerializer.Serialize(student);

            // Chuyển sang request mới
            return RedirectToAction("Success");
        }

        // GET: /Student/Success
        public IActionResult Success()
        {
            ViewBag.Title = "Kết quả";

            Student student = new Student();

            if (TempData["Student"] is string studentJson)
            {
                student = JsonSerializer.Deserialize<Student>(studentJson)
                          ?? new Student();
            }

            return View(student);
        }
    }
}