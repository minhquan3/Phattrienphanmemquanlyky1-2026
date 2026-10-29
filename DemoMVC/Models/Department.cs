
using System.ComponentModel.DataAnnotations;

namespace DemoMVC.Models
{
    public class Department
    {
        // Khóa chính của khoa
        public int DepartmentId { get; set; }

        // Tên khoa
        [Required(ErrorMessage = "Tên khoa không được để trống")]
        [StringLength(
            100,
            ErrorMessage = "Tên khoa không được vượt quá 100 ký tự"
        )]
        [Display(Name = "Tên khoa")]
        public string DepartmentName { get; set; } = string.Empty;

        // Mô tả khoa
        [StringLength(
            500,
            ErrorMessage = "Mô tả không được vượt quá 500 ký tự"
        )]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        // Một khoa có nhiều sinh viên
        public ICollection<Student> Students { get; set; }
            = new List<Student>();
    }
}
