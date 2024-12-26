using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Demo.Core.Models
{
    public class Category : BaseEntity
    {
        [Display(Name = "Tên danh mục")]
        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        public string CatName { get; set; }

        [Display(Name = "Miêu tả danh mục")]
        public string? Description { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool Active { get; set; }

        [Display(Name = "Tên thay thế")]
        public string? Alias { get; set; }

        [Display(Name = "Thuộc danh mục cha")]
        public Category? ParentCategory { get; set; } = null;

        [Display(Name = "Đường dẫn")]
        public string? LinkAddress { get; set; }

        [Display(Name = "Thứ tự")]
        public int? Order { get; set; }
    }
}