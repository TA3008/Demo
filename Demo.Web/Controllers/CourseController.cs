﻿using Microsoft.AspNetCore.Mvc;
using Demo.Application.Repositories;
using Demo.Core.Permission;
using Demo.Web.Filters;

namespace Demo.Web.Controllers
{
    public class CourseController : Controller
    {
        private readonly ILogger<CourseController> _logger;
        private readonly ICourseRepository _courseRepository;
        // private readonly IFileService _fileService;

        public CourseController(ILogger<CourseController> logger,
            ICourseRepository courseRepository)
        // IFileService fileService)
        {
            _logger = logger;
            _courseRepository = courseRepository;
            // _fileService = fileService;
        }

        public IActionResult List()
        {
            var model = _courseRepository.Find(x => x.Active && x.Deleted == false).OrderByDescending(x => x.Created).ToList();
            return View(model);
        }

        public IActionResult Detail(string url)
        {
            var course = _courseRepository.Find(x => x.FriendlyUrl == url && x.Deleted == false && x.Active).ToList().FirstOrDefault();
            return View(course);
        }

        [WebAuthorize(RoleList.Content, RoleList.Admin)]
        public IActionResult Preview(string url)
        {
            var article = _courseRepository.Find(x => x.FriendlyUrl == url && x.Deleted == false).ToList().FirstOrDefault();
            return View("Detail", article);
        }

        // [HttpPost]
        // public IActionResult Upload(IFormFile file)
        // {
        //     if (file != null)
        //     {
        //         var url = _fileService.UpsertImage("imgs", $"{DateTimeExtensions.UTCNowVN.Year}/{DateTimeExtensions.UTCNowVN.Month}/{Guid.NewGuid()}.png", file.OpenReadStream());
        //         return Json(new { location = url });
        //     }
        //     return Json("Không upload được ảnh!");
        // }
    }
}
