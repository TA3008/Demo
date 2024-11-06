using System.ComponentModel.DataAnnotations;

namespace Demo.Core.Models
{
    public class Lesson : BaseEntity
    {
        [Display(Name = "Tiêu đề")]
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; }

        [Display(Name = "Mô tả")]
        [Required(ErrorMessage = "Mô tả không được để trống")]
        public string Description { get; set; }

        [Display(Name = "Khoá học")]
        public Course course { get; set; }

        [Display(Name = "Video")]
        public string YouTubeUrl { get; set; }

        [Display(Name = "Nội dung")]
        public string Thumb { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool Active { get; set; }
    }
}