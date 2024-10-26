using Microsoft.AspNetCore.Mvc;
using Demo.Common.Extensions;
using Demo.Application.Repositories;
using Demo.Application.Services;
using Demo.Core.Models;
using Demo.Core.Repositories;
using Demo.Core.ValueObjects;
using Demo.Web.Models;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace Demo.Web.Controllers
{
    public class OrderController : Demo
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICourseRepository _courseRepository;
        private readonly IOrderService _orderService;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        //private readonly IVoucherRepository _voucherRepository;
        private readonly ISystemParameters _systemParameters;
        private readonly IPaymentService _paymentService;
        //private readonly IMailService _mailService;
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITempDataProvider _tempDataProvider;

        public OrderController(ILogger<HomeController> logger,
            ICourseRepository courseRepository,
            IOrderService orderService,
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            //IVoucherRepository voucherRepository,
            ISystemParameters systemParameters,
            IPaymentService paymentService,
            //IMailService mailService,
            IRazorViewEngine razorViewEngine,
            IServiceProvider serviceProvider,
            ITempDataProvider tempDataProvider)
        {
            _logger = logger;
            _courseRepository = courseRepository;
            _orderService = orderService;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _systemParameters = systemParameters;
            _paymentService = paymentService;
            //_voucherRepository = voucherRepository;
            //_mailService = mailService;
            _razorViewEngine = razorViewEngine;
            _serviceProvider = serviceProvider;
            _tempDataProvider = tempDataProvider;
        }

        [HttpGet]
        public IActionResult Checkout(string productIds)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Checkout" });
            }

            var courses = new List<Course>();
            if (!String.IsNullOrEmpty(productIds))
            {
                try
                {
                    var courseIds = productIds.Split(',').Select(x => Guid.Parse(x)).ToList();
                    courses = _courseRepository.Find(x => courseIds.Contains(x.Id)).ToList();
                }
                catch (FormatException)
                {
                    return RedirectToAction("Cart"); // Điều hướng trở lại giỏ hàng nếu productIds không hợp lệ
                }
            }

            if (courses.Count == 0)
            {
                return RedirectToAction("Cart"); // Điều hướng trở lại giỏ hàng nếu không có khóa học
            }

            var model = new OrderViewModel();
            var currentUser = _userRepository.GetByUsername(User.Identity.Name);
            if (currentUser != null)
            {
                model.CustomerName = currentUser.FullName;
                model.CustomerPhone = currentUser.PhoneNumber;
                model.CustomerEmail = currentUser.Email;
                model.CustomerAddress = currentUser.Address;
                model.CartItem = courses;
                model.ProductIds = productIds;
            }

            return View(model); // Hiển thị trang checkout với dữ liệu đã load
        }

        [HttpPost]
        public IActionResult Checkout(OrderViewModel model, string productIds)
        {
            productIds = model.ProductIds;
            if (!ModelState.IsValid)
            {
                string messages = base.GetModalStateErrorMsg();
                return Json(new JsonReturn(false, messages));
            }

            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new JsonReturn(false, "Vui lòng đăng nhập để tạo đơn hàng."));
            }

            // Ensure the product IDs are not empty
            if (string.IsNullOrEmpty(productIds))
            {
                return Json(new JsonReturn(false, "Vui lòng chọn ít nhất một sản phẩm để thanh toán."));
            }

            // Split the productIds string to a list of GUIDs
            var courseIds = productIds.Split(',').Select(Guid.Parse).ToList();

            // Fetch the selected courses from the repository
            var courses = _courseRepository.Find(x => courseIds.Contains(x.Id)).ToList();
            if (!courses.Any())
            {
                return Json(new JsonReturn(false, "Có lỗi xảy ra với khoá học đã chọn, vui lòng thử lại."));
            }

            // long totalPrice = 0;

            // foreach (var course in courses)
            // {
            //     if (model.PaymentOption == PaymentOption.OneMonth.GetHashCode())
            //     {
            //         totalPrice += course.Price * (100 - 10) / 100;  // 10% discount for 1 month payment
            //     }
            //     else if (model.PaymentOption == PaymentOption.ThreeMonths.GetHashCode())
            //     {
            //         totalPrice += course.Price * (100 - 15) / 100;  // 15% discount for 3 months payment
            //     }
            // }

            var order = new Order
            {
                // PaymentOption = (PaymentOption)model.PaymentOption,
                Created = DateTimeExtensions.UTCNowVN,
                CreatedBy = User?.Identity?.Name,
                ModifiedBy = User?.Identity?.Name,
                Modified = DateTimeExtensions.UTCNowVN,
                Price = model.CartItem.Sum(x => x.Price),
                Status = OrderStatus.Pending,
                Username = User.Identity.Name,
                CustomerAddress = model.CustomerAddress,
                CustomerName = model.CustomerName,
                CustomerPhone = model.CustomerPhone,
                CustomerNote = model.CustomerNote,
                // VerifyImageUrl = model.VerifyImageUrl,
                StatusHistories = new List<OrderStatusDetails>
        {
            new OrderStatusDetails
            {
                ActionTime = DateTimeExtensions.UTCNowVN,
                Status = OrderStatus.Pending,
                Author = User?.Identity?.Name
            }
        },
                Code = User.Identity.Name.Length > 4
                    ? User.Identity.Name.Substring(0, 4) + DateTimeExtensions.UTCNowVN.ToString("yyMMddHHmmss")
                    : User.Identity.Name.Length + DateTimeExtensions.UTCNowVN.ToString("yyMMddHHmmss")
            };

            // Handle voucher logic
            //if (!string.IsNullOrEmpty(model.VoucherCode))
            //{
            //    var voucher = _voucherRepository.Find(x => x.Code == model.VoucherCode).FirstOrDefault();
            //    if (voucher != null && voucher.StartDate <= DateTimeExtensions.UTCNowVN && voucher.Expired >= DateTimeExtensions.UTCNowVN && voucher.Quantity > 0)
            //    {
            //        if (voucher.DiscountRate > 0)
            //        {
            //            totalPrice = totalPrice * (100 - voucher.DiscountRate) / 100;
            //        }
            //        else if (voucher.DiscountAmount > 0)
            //        {
            //            totalPrice -= voucher.DiscountAmount;
            //        }
            //        order.Voucher = voucher;

            //        // Update voucher quantity
            //        _voucherRepository.UpdateQuantity(voucher.Id, Math.Max(voucher.Quantity - 1, 0));
            //    }
            //    else
            //    {
            //        return Json(new JsonReturn(false, "Mã giảm giá không hợp lệ hoặc đã hết hạn."));
            //    }
            //}

            order.Price = model.CartItem.Sum(x => x.Price);

            // Save the order
            _orderRepository.UpsertAsync(order);

            // Send email notification
            //_mailService.OrderStatusChanged(order);

            return Json(new JsonReturn(true, "Đặt hàng thành công!"));
        }


        //[HttpPost]
        //public IActionResult Upload(IFormFile file)
        //{
        //    if (file != null)
        //    {
        //        var date = DateTimeExtensions.UTCNowVN;
        //        string ext = Path.GetExtension(file.FileName);
        //        var url = _fileService.UpsertImage("orders", $"{date.Year}/{date.Month}/{Guid.NewGuid()}.{date.ToString("yyyyMMdd")}.{ext ?? "png"}", file.OpenReadStream());
        //        return Json(new JsonReturn(true, url));
        //    }
        //    return Json(new JsonReturn(false));
        //}

        public IActionResult MyOrder()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account");
            }
            var orders = _orderService.GetOrdersByUsername(User.Identity.Name).OrderByDescending(x => x.Created).ToList();
            return View(orders);
        }

        public IActionResult MyCourse()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account");
            }
            var orders = _orderService.GetActivePendingPaidOrdersByUsername(User.Identity.Name);
            return View(orders);
        }

        // public IActionResult MyCourse(string code)
        // {
        //     if (User?.Identity?.IsAuthenticated != true)
        //     {
        //         return RedirectToAction("Login", "Account");
        //     }
        //     ViewData["PaymentTime"] = _systemParameters.PaymentPendingDays;
        //     var order = _orderRepository.Find(x => x.Code == code && x.Deleted != true).FirstOrDefault();

        //     List<BillingInformationViewModel> lsBillinfo = new List<BillingInformationViewModel>();
        //     var pay = _paymentService.GetPaid(order.Id, PaymentState.Paid);
        //     if (pay.Any())
        //     {

        //         order.Combo.Vegetables.ForEach(x =>
        //         {
        //             var harvestedGr = 0;
        //             deliveries.ForEach(y =>
        //             {
        //                 var h = y.Vegetables.FirstOrDefault(z => z.Id == x.Id);
        //                 if (h != null)
        //                 {
        //                     harvestedGr += h.Weight;
        //                 }
        //             });
        //             lsBillinfo.Add(new BillingInformationViewModel()
        //             {
        //                 Id = x.Id,
        //                 MoneyGot = harvestedGr
        //             });
        //         });
        //     }
        //     ViewData["Harvested"] = lsHarvested;
        //     return View(order);
        // }

        //public IActionResult Voucher(string code)
        //{
        //    if (String.IsNullOrEmpty(code))
        //    {
        //        return Json(new JsonReturn(false, $"Mã giảm giá trống!"));
        //    }
        //    code = code.Trim();
        //    var voucher = _voucherRepository.Find(x => x.Code == code).FirstOrDefault();
        //    if (voucher == null
        //        || new DateTime(voucher.StartDate.Year, voucher.StartDate.Month, voucher.StartDate.Day) > new DateTime(DateTimeExtensions.UTCNowVN.Year, DateTimeExtensions.UTCNowVN.Month, DateTimeExtensions.UTCNowVN.Day))
        //    {
        //        return Json(new JsonReturn(false, $"Mã giảm giá {code} không tồn tại!"));
        //    }
        //    if (voucher.Expired.Date < DateTimeExtensions.UTCNowVN.Date)
        //    {
        //        return Json(new JsonReturn(false, $"Mã giảm giá {code} đã hết hạn!"));
        //    }
        //    if (voucher.Quantity <= 0)
        //    {
        //        return Json(new JsonReturn(false, $"Mã giảm giá {code} đã hết!"));
        //    }
        //    return Json(new JsonReturn(true, voucher.DiscountRate > 0 ? $"{voucher.DiscountRate}%" : $"{voucher.DiscountAmount}vnd"));
        //}

        private async Task<string> RenderRazorViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var actionContext = new ActionContext(HttpContext, RouteData, ControllerContext.ActionDescriptor, ModelState);

                var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"View {viewName} was not found.");
                }

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
    }
}