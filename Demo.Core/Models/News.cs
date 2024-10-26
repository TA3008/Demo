using System.ComponentModel.DataAnnotations;

namespace Demo.Core.Models
{
    public class News : BaseEntity
    {
        // public int ID{ get; set; }
        [Display(Name = "Tiêu đề")]
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string? Title { get; set; }

        [Display(Name = "Mô tả ngắn")]
        [Required(ErrorMessage = "Mô tả ngắn không được để trống")]
        public string? SContents { get; set; }

        [Display(Name = "Nội dung")]
        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string? Contents { get; set; }

        [Display(Name = "Hình ảnh")]
        public string? Image { get; set; }

        [Display(Name = "Công khai")]
        public bool Published { get; set; }

        [Display(Name = "Tên thay thế")]
        public string? Alias { get; set; }

        [Display(Name = "Tác giả")]
        [Required(ErrorMessage = "Tác giả không được để trống")]
        public string? Author { get; set; }

        [Display(Name = "Nhãn")]
        public string? Tags { get; set; }

        [Display(Name = "Lượt xem")]
        public int? Views { get; set; }
        public string? Thumbnail { get; set; }
    }
}