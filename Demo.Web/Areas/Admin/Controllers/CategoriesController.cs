using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Demo.Common.Extensions;
using Demo.Application.Repositories;
using Demo.Web.Filters;
using Demo.Core.Permission;
using Demo.Core.Models;
using Demo.Web.Helpers;
using Microsoft.Extensions.Logging;

namespace Demo.Web.Areas.Admin.Controllers
{
    [WebAuthorize(RoleList.Content, RoleList.Admin)]
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ILogger<CategoriesController> _logger;
        private readonly ICategoryRepository _CategoriesRepository;


        public CategoriesController(ILogger<CategoriesController> logger,
            ICategoryRepository CategoriesRepository)
        {
            _logger = logger;
            _CategoriesRepository = CategoriesRepository;


        }

        public IActionResult Index()
        {
            var categories = _CategoriesRepository.Find(x => x.Deleted == false).ToList();
            return View(categories);
        }

        public IActionResult Edit(Guid? id)
        {
            Categories? model = null;
            var Categories = _CategoriesRepository.Find(x => x.Deleted == false).ToList();
            // var Lesson =
            if (id.HasValue)
            {
                model = _CategoriesRepository.Get(id.Value);
            }
            if (model == null)
            {
                model = new Categories();
            }
            ViewBag.Categories = Categories;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Categories model, string returnUrl)
        {
            // if(ModelState.IsValid){


            // model.ModifiedBy = User?.Identity?.Name;
            model.Modified = DateTimeExtensions.UTCNowVN;



            await (model.Id == Guid.Empty ? _CategoriesRepository.AddAsync(model) : _CategoriesRepository.UpdateAsync(model));

            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
            else return Redirect(returnUrl);
            // }

            // return View(model);
        }

        public async Task<IActionResult> Delete(Guid id, string returnUrl)
        {
            await _CategoriesRepository.SetAsync(id, nameof(Categories.Deleted), true);
            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
            else return Redirect(returnUrl);
        }
    }
}
