using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Demo.Core.Models;
using Demo.Application.Repositories;
using Demo.Web.Helpers;
using Demo.Web.Filters;
using Demo.Core.Permission;
using Demo.Common.Extensions;
using Demo.Application.Services;

namespace Demo.Web.Areas.Admin.Controllers
{
    [WebAuthorize(RoleList.Admin, RoleList.Customer, RoleList.Sale)]
    [Area("Admin")]
    public class ClassController : Controller
    {
        private readonly ILogger<ClassController> _logger;
        private readonly IClassRepository _classRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IFileService _fileService;

        public ClassController(ILogger<ClassController> logger,
            IClassRepository classRepository, ICourseRepository courseRepository, ILessonRepository lessonRepository, IFileService fileService)
        {
            _logger = logger;
            _classRepository = classRepository;
            _courseRepository = courseRepository;
            _lessonRepository = lessonRepository;
            _fileService = fileService;

        }

        public IActionResult Index()
        {
            var classes = _classRepository.Find(x => x.Deleted == false).ToList();
            return View(classes);
        }

        public IActionResult Edit(Guid? id)
        {
            Class? model = null;
            var course = _courseRepository.Find(x => x.Deleted == false).ToList();
            var lesson = _lessonRepository.Find(x => x.Deleted == false).ToList();
            if (id.HasValue)
            {
                model = _classRepository.Get(id.Value);
            }
            if (model == null)
            {
                model = new Class();
                model.Id = Guid.NewGuid();
            }
            ViewBag.Courses = course;
            ViewBag.Lessons = lesson;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Class model, IFormFile fileInput, string returnUrl, Guid? CourseId, Guid? LessonId)
        {
            try
            {
                if (!ModelState.IsValid && (!ModelState.ContainsKey("returnUrl") && !ModelState.ContainsKey("fileInput")))
                {
                    return View(model);
                }
                model.ModifiedBy = User?.Identity?.Name;
                model.Modified = DateTimeExtensions.UTCNowVN;
                if (model.Id != Guid.Empty && model.Id != null)
                {
                    model.CreatedBy = model.ModifiedBy;
                    model.Created = DateTimeExtensions.UTCNowVN;
                }

                if (fileInput != null)
                {
                    model.Thumb = _fileService.ResizeImageJpeg(fileInput.OpenReadStream(), 360, 230, "classes", $"{model.Id}.thumb.png");
                }

                if (CourseId.HasValue)
                {
                    var course = _courseRepository.Find(c => c.Id == CourseId.Value && c.Deleted == false).FirstOrDefault();
                    if (course != null)
                    {
                        model.course = new Course
                        {
                            Id = course.Id,
                            Title = course.Title
                        };
                    }
                }
                if (LessonId.HasValue)
                {
                    var lesson = _lessonRepository.Find(c => c.Id == LessonId.Value && c.Deleted == false).FirstOrDefault();
                    if (lesson != null)
                    {
                        model.lesson = new Lesson
                        {
                            Id = lesson.Id,
                            Title = lesson.Title
                        };
                    }
                }

                if (String.IsNullOrEmpty(model.FriendlyUrl))
                {
                    var url = StringHelpers.ToFriendlyUrl(model.ClassName);
                    if (_classRepository.Find(x => x.FriendlyUrl == url && x.Deleted != true).FirstOrDefault() != null)
                    {
                        do
                        {
                            model.FriendlyUrl = url + "-" + new Random().Next(1, 100);
                        }
                        while (_classRepository.Find(x => x.FriendlyUrl == model.FriendlyUrl && x.Deleted != true).FirstOrDefault() != null);
                    }
                    else
                    {
                        model.FriendlyUrl = url;
                    }
                }
                await _classRepository.UpsertAsync(model);
                if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
                else return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving course");
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(Guid id, string returnUrl)
        {
            await _classRepository.SetAsync(id, nameof(Class.Deleted), true);
            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
            else return Redirect(returnUrl);
        }
    }
}