using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Demo.Core.Models;
using Microsoft.AspNetCore.Identity;
using Demo.Core.Services;
using Demo.Core.Repositories;
using Demo.Application.Repositories;

namespace Demo.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IMongoClient _client;
        private readonly UserManager<User> _userManager;
        private readonly IUserGroupManager _userGroupManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        public CartController(IMongoClient client,
            UserManager<User> userManager,
            IUserGroupManager userGroupManager,
            SignInManager<User> signInManager,
            IUserRepository userRepository,
            ICourseRepository courseRepository
            )
        {
            _client = client;
            _userManager = userManager;
            _userGroupManager = userGroupManager;
            _signInManager = signInManager;
            _userRepository = userRepository;
            _courseRepository = courseRepository;
        }

        [Route("/cart.html", Name = "Cart")]
        public IActionResult Index()
        {
            var user = _userRepository.GetByUsername(User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (user.CartItem == null || !user.CartItem.Any())
            {
                return View(new List<Course>()); // Trả về view với danh sách trống
            }
            ViewBag.CartCount = user.CartItem.Count;
            return View(user.CartItem);
        }

        [HttpPost]
        [Route("api/cart/add")]
        public IActionResult AddToCart(Guid courseID)
        {
            var database = _client.GetDatabase("Demo");
            try
            {
                var user = _userRepository.GetByUsername(User.Identity.Name);
                if (user == null)
                {
                    return Json(new { success = false });
                }
                if (user.CartItem.Any(c => c.Id == courseID))
                {
                    return Json(new { success = true });
                }
                var course = _courseRepository.Find(c => c.Id == courseID).SingleOrDefault();
                if (course == null)
                {
                    return Json(new { success = false });
                }
                user.CartItem.Add(course);
                var result = _userRepository.UpdateAsync(user);

                if (result == null)
                {
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }

        // [HttpPost]
        // [Route("api/cart/update")]
        // public IActionResult UpdateCart(Guid courseID, int? quantity)
        // {
        //     // lấy giỏ hàng ra để xử lý
        //     var user = _userRepository.GetByUsername(User.Identity.Name);
        //     try
        //     {
        //         if (user.CartItem != null)
        //         {
        //             CartItem item = cart.SingleOrDefault(p => p.courses.Id == courseID);
        //             if (item != null && quantity.HasValue)//đã có --) cập nhật số lượng
        //             {
        //                 item.quantity = quantity.Value;
        //             }
        //             //lưu lại session
        //             HttpContext.Session.Set<List<CartItem>>("CartItems", cart);
        //         }
        //         return Json(new { success = true });
        //     }
        //     catch
        //     {
        //         return Json(new { success = false });
        //     }
        // }

        [HttpPost]
        [Route("api/cart/remove")]
        public async Task<ActionResult> Remove(Guid courseID)
        {
            try
            {
                var user = _userRepository.GetByUsername(User.Identity.Name);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Remove course by its ID
                var courseToRemove = user.CartItem.FirstOrDefault(c => c.Id == courseID);
                if (courseToRemove != null)
                {
                    user.CartItem.RemoveAll(c => c.Id == courseID); // Remove by Id, not by object reference
                    var result = await _userRepository.UpdateAsync(user); // Ensure this is awaited

                    if (result != null)
                    {
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Course not found in cart" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}