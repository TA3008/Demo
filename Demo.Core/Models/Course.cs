using System.ComponentModel.DataAnnotations;

namespace Demo.Core.Models
{
    public class Course : BaseEntity
    {
        [Display(Name = "Tiêu đề")]
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string Thumb { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool Active { get; set; }

        [Display(Name = "Giá")]
        public long Price { get; set; }
        public int FakePrice { get; set; }
    }
}