using Demo.Web.Startup;
using Demo.Web.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.AddMongoDatabase();
builder.Services.RegisterServices(builder.Configuration);
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// app.MapAreaControllerRoute(
//     name: "Admin",
//     areaName: "Admin",
//     pattern: "admin/{controller=Home}/{action=Index}"
// );

app.MapControllerRoute(
    name: "areaRoute",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
name: "danhsachkhoahoc",
pattern: FriendlyUrl.CoursesListFrUrl,
defaults: new { controller = "Course", action = "List" });

app.MapControllerRoute(
    name: "thongtinkhoahoc",
    pattern: FriendlyUrl.CourseDetailFrm,
    defaults: new { controller = "Course", action = "Detail" });

app.MapControllerRoute(
name: "cuahangsach",
pattern: FriendlyUrl.BookShopFrUrl,
defaults: new { controller = "BookShop", action = "List" });

app.MapControllerRoute(
name: "sukien",
pattern: FriendlyUrl.EventsListFrUrl,
defaults: new { controller = "Event", action = "List" });

app.MapControllerRoute(
name: "khoahoccuatoi",
pattern: FriendlyUrl.MyCoursesFrUrl,
defaults: new { controller = "Orders", action = "MyCourses" });

app.MapControllerRoute(
name: "donhangcuatoi",
pattern: FriendlyUrl.MyOrdersFrUrl,
defaults: new { controller = "Orders", action = "MyCourses" });

app.MapControllerRoute(
    name: "thongtincuatoi",
    pattern: FriendlyUrl.MyProfileFrUrl,
    defaults: new { controller = "Account", action = "MyProfile" });

app.MapControllerRoute(
    name: "muakhoahoc",
    pattern: FriendlyUrl.CourseShopFrUrl,
    defaults: new { controller = "Orders", action = "Checkout" });

app.MapControllerRoute(
    name: "danhsachtin",
    pattern: FriendlyUrl.NewsListFrUrl,
    defaults: new { controller = "News", action = "List" });

app.MapControllerRoute(
    name: "tintuc",
    pattern: FriendlyUrl.NewsFrUrl,
    defaults: new { controller = "News", action = "Detail" });

app.MapControllerRoute(
    name: "vechungtoi",
    pattern: FriendlyUrl.AboutUsFrUrl,
    defaults: new { controller = "Home", action = "AboutUs" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
