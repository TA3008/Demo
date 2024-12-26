using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Demo.Common.Extensions;
using Demo.Application.Repositories;
using Demo.Web.Filters;
using Demo.Core.Permission;
using Demo.Core.Models;
using Demo.Web.Helpers;
using Microsoft.Extensions.Logging;
using Demo.Database.Repositories;

namespace Demo.Web.Areas.Admin.Controllers
{
    [WebAuthorize(RoleList.Content, RoleList.Admin)]
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly ICategoryRepository _categoriesRepository;


        public CategoryController(ILogger<CategoryController> logger,
            ICategoryRepository categoriesRepository)
        {
            _logger = logger;
            _categoriesRepository = categoriesRepository;


        }

        public IActionResult Index()
        {
            var categories = _categoriesRepository.Find(x => x.Deleted == false).ToList();
            return View(categories);
        }

        public IActionResult Edit(Guid? id)
        {
            Category? model = null;
            var Categories = _categoriesRepository.Find(x => x.Deleted == false).ToList();
            // var Lesson =
            if (id.HasValue)
            {
                model = _categoriesRepository.Get(id.Value);
            }
            if (model == null)
            {
                model = new Category();
            }
            ViewBag.Categories = Categories;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category model, string returnUrl, Guid? CategoryId)
        {
            // if(ModelState.IsValid){
            if (CategoryId.HasValue)
            {
                var category = _categoriesRepository.Find(c => c.Id == CategoryId.Value && c.Deleted == false).FirstOrDefault();
                if (category != null)
                {
                    model.ParentCategory = new Category
                    {
                        Id = category.Id,
                        CatName = category.CatName
                        // Cần bổ sung thêm các thông tin của Danh mục cha 
                    };
                }
            }
            // model.ModifiedBy = User?.Identity?.Name;
            model.Modified = DateTimeExtensions.UTCNowVN;

            await (model.Id == Guid.Empty ? _categoriesRepository.AddAsync(model) : _categoriesRepository.UpdateAsync(model));

            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
            else return Redirect(returnUrl);
            // }

            // return View(model);
        }

        public async Task<IActionResult> Delete(Guid id, string returnUrl)
        {
            await _categoriesRepository.SetAsync(id, nameof(Category.Deleted), true);
            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
            else return Redirect(returnUrl);
        }
    }
}
