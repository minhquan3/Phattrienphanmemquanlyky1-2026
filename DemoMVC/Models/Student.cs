using System.ComponentModel.DataAnnotations;

namespace DemoMVC.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "Họ tên phải từ 3 đến 100 ký tự"
        )]
        public string Fullname { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Range(
            18,
            60,
            ErrorMessage = "Tuổi phải từ 18 đến 60"
        )]
        public int Age { get; set; }

        [Range(
            0,
            10,
            ErrorMessage = "GPA phải từ 0 đến 10"
        )]
        public double GPA { get; set; }

        [Required(ErrorMessage = "Trường đại học không được để trống")]
        public string University { get; set; }
    }
}