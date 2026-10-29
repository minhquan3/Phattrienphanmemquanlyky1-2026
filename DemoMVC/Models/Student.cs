
using System.ComponentModel.DataAnnotations;

namespace DemoMVC.Models
{
    public class Student
    {
        public int Id { get; set; }

        // Mã sinh viên tự động sinh
        [Display(Name = "Mã sinh viên")]
        [StringLength(10, ErrorMessage = "Mã sinh viên tối đa 10 ký tự")]
public string StudentCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "Họ tên phải từ 3 đến 100 ký tự"
        )]
        public string Fullname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Range(18, 60, ErrorMessage = "Tuổi phải từ 18 đến 60")]
        public int Age { get; set; }

        [Range(0, 10, ErrorMessage = "GPA phải từ 0 đến 10")]
        public double GPA { get; set; }

        [Required(ErrorMessage = "Trường đại học không được để trống")]
        public string University { get; set; } = string.Empty;

        // Khóa ngoại liên kết với khoa
        [Display(Name = "Khoa")]
        public int? DepartmentId { get; set; }

        // Navigation property: khoa của sinh viên
        public Department? Department { get; set; }
    }
}
