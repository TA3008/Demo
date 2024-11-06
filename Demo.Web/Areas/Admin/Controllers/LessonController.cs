using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Demo.Common.Extensions;
using Demo.Application.Repositories;
using Demo.Web.Filters;
using Demo.Core.Permission;
using Demo.Core.Models;
using Demo.Web.Helpers;

namespace Demo.Web.Areas.Admin.Controllers
{
    [WebAuthorize(RoleList.Content, RoleList.Product, RoleList.Admin)]
    [Area("Admin")]
    public class LessonController : Controller
    {
        private readonly ILogger<LessonController> _logger;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;

        public LessonController(ILogger<LessonController> logger,
            ILessonRepository lessonRepository, ICourseRepository courseRepository)
        {
            _logger = logger;
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
        }

        public IActionResult Index()
        {
            var lessons = _lessonRepository.Find(x => x.Deleted == false).ToList();
            return View(lessons);
        }

        public IActionResult Edit(Guid? id)
        {
            Lesson? model = null;
            var lscourse = _courseRepository.Find(x => x.Deleted == false).ToList();

            if (id.HasValue)
            {
                model = _lessonRepository.Get(id.Value);
            }

            if (model == null)
            {
                model = new Lesson
                {
                    Id = Guid.NewGuid(),
                    course = new Course()
                };
            }

            ViewBag.Courses = lscourse;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Lesson model, string returnUrl, Microsoft.AspNetCore.Http.IFormFile fThumb, Guid? CourseId)
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

                if (!string.IsNullOrEmpty(model.YouTubeUrl))
                {
                    var videoId = ExtractYouTubeVideoId(model.YouTubeUrl);
                    model.Thumb = $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg";
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

                if (string.IsNullOrEmpty(model.FriendlyUrl))
                {
                    var url = StringHelpers.ToFriendlyUrl(model.Title);
                    if (_lessonRepository.Find(x => x.FriendlyUrl == url && x.Deleted != true).FirstOrDefault() != null)
                    {
                        do
                        {
                            model.FriendlyUrl = url + "-" + new Random().Next(1, 100);
                        }
                        while (_lessonRepository.Find(x => x.FriendlyUrl == model.FriendlyUrl && x.Deleted != true).FirstOrDefault() != null);
                    }
                    else
                    {
                        model.FriendlyUrl = url;
                    }
                }

                await _lessonRepository.UpsertAsync(model);
                if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
                else return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving lesson");
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(Guid id, string returnUrl)
        {
            await _lessonRepository.SetAsync(id, nameof(Lesson.Deleted), true);
            if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(nameof(Index));
            else return Redirect(returnUrl);
        }

        private string ExtractYouTubeVideoId(string url)
        {
            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query["v"];
        }
    }
}
