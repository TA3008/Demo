using System.ComponentModel.DataAnnotations;

namespace Demo.Core.Models
{
    public class Class : BaseEntity
    {
        [Display(Name = "Tiêu đề")]
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string ClassName { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Ảnh")]
        public string Thumb { get; set; }

        [Display(Name = "Khoá học")]
        public string Course { get; set; }

        [Display(Name = "Môn học")]
        public string Lesson { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool Active { get; set; }
    }
}