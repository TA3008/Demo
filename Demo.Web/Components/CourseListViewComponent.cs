using Microsoft.AspNetCore.Mvc;
using Demo.Application.Repositories;

namespace Demo.Web.Components
{
    public class CourseListViewComponent : ViewComponent
    {
        private readonly ICourseRepository _courseRepository;

        public CourseListViewComponent(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public IViewComponentResult Invoke(Guid? ignoreCourseId)
        {
            var courses = _courseRepository.Find(x => x.Deleted == false).Take(4).ToList();

            if (ignoreCourseId != null)
            {
                courses = courses.Where(x => x.Id != ignoreCourseId).ToList();
            }

            return View(courses);
        }
    }
}
