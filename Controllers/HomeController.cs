using Dapper;
using gelirGiderTakip.Codes.Models;
using gelirGiderTakip.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Xml.Linq;

namespace gelirGiderTakip.Controllers
{
    public class HomeController : Controller
    {
        string connectionString = "X";
        public bool CheckLogin()
        {
            if (HttpContext.Session.GetString("Username") != null)
            {
                return true;
            }
            return false;
        }
        public void GetSessions()
        {
            ViewData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["UserId"] = HttpContext.Session.GetString("UserId");
            ViewData["RoleId"] = HttpContext.Session.GetString("RoleId");
            ViewData["Email"] = HttpContext.Session.GetString("Email");
        }
        public IActionResult Index()
        {
            GetSessions();
            if (!CheckLogin())
            {
                TempData["Alert"] = "Gelir Gider Takip uygulamasını kullanabilmek için bir hesaba giriş yapmalısın.";
                TempData["AlertCss"] = "alert-danger";
                return RedirectToAction("Login", "Home");
            }

            using var connection = new SqlConnection(connectionString);
            
                var sql = "SELECT * FROM gelirCategories";
                var categories = connection.Query<Category>(sql).ToList();
            

            return View(categories);
        }

        [HttpPost]
        [Route("addIncome")]
        public IActionResult AddIncome(Income model)
        {
            GetSessions();

            if (!ModelState.IsValid)
            {
                TempData["AlertCss"] = "alert-danger";
                TempData["Alert"] = "Eksik form bilgisi.";
                return RedirectToAction("Index");
            }

            if (!CheckLogin())
            {
                TempData["Alert"] = "Gelir ve gider tablosunu gönderebilmek için bir hesaba giriş yapmalısın.";
                TempData["AlertCss"] = "alert-danger";
                return RedirectToAction("Login", "Home");
            }

            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO gelirTables (name, price, type, categoryId, userId) VALUES (@Name, @Price, @Type, @CategoryId, @UserId)";

            var data = new
            {
                Name = model.Name,
                Price = model.Price,
                Type = model.Type,
                CategoryId = model.CategoryId,
                UserId = model.UserId
            };

            var rowsAffected = connection.Execute(sql, data);

            TempData["Alert"] = "Gider başarı ile eklendi!";
            TempData["AlertCss"] = "alert-success";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("addCategory")]
        public IActionResult AddCategory(Category model)
        {
            if (!ModelState.IsValid)
            {
                TempData["AlertCss"] = "alert-danger";
                TempData["Alert"] = "Eksik form bilgisi.";
                return RedirectToAction("Index");
            }

            if (!CheckLogin())
            {
                TempData["Alert"] = "Gelir ve gider tablosunu gönderebilmek için bir hesaba giriş yapmalısın.";
                TempData["AlertCss"] = "alert-danger";
                return RedirectToAction("Login", "Home");
            }

            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO gelirCategories (name) VALUES (@Name)";

            var data = new
            {
                Name = model.Name
            };

            var rowsAffected = connection.Execute(sql, data);

            TempData["Alert"] = "Kategori başarı ile eklendi!";
            TempData["AlertCss"] = "alert-success";
            return RedirectToAction("Index");
        }

        [Route("login")]
        public IActionResult Login()
        {
            if (CheckLogin())
            {
                TempData["Alert"] = "Zaten bir hesaba giriş yapılmış.";
                TempData["AlertCss"] = "alert-success";
                return RedirectToAction("Index", "Home");
            }

            ViewData["Username"] = HttpContext.Session.GetString("Username");
            return View(new Register());
        }

        [Route("login")]
        [HttpPost]
        public IActionResult Login(User model)
        {
            var inputPasswordHash = Helper.Hash(model.Password);

            using var connection = new SqlConnection(connectionString);

            var sql = "SELECT * FROM gelirUsers WHERE Username = @Username AND Password = @Password";
            var user = connection.QuerySingleOrDefault<User>(sql, new { model.Username, Password = inputPasswordHash });

            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserId", user.Id.ToString());

                TempData["Alert"] = $"Giriş başarılı. Hoşgeldin {user.Username}";
                TempData["AlertCss"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Alert"] = "Kullanıcı adı veya şifre hatalı.";
                TempData["AlertCss"] = "alert-danger";
                return RedirectToAction("Login");
            }
        }

        [Route("register")]
        [HttpPost]
        public IActionResult Register(Register model)
        {

            if (!ModelState.IsValid)
            {
                TempData["AlertCss"] = "alert-danger";
                TempData["Alert"] = "Eksik form bilgisi.";
                return RedirectToAction("Login", model);
            }

            if (model.Password != model.PasswordCheck)
            {
                TempData["AlertCss"] = "alert-danger";
                TempData["Alert"] = "Sifre dogrulamasi hatali.";
                return View("Login", model);
            }

            model.Password = Helper.Hash(model.Password);

            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO gelirUsers (username, email, password, createdDate) VALUES (@Username, @Email, @Password, @CreatedDate)";

            var data = new
            {
                model.Username,
                model.Email,
                model.Password,
                CreatedDate = DateTime.Now,
            };

            var rowsAffected = connection.Execute(sql, data);

            TempData["AlertCss"] = "alert-success";
            TempData["Alert"] = "Kullanıcı kayıdı başarı ile oluşturuldu!";
            return RedirectToAction("Login");
        }

        [Route("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Username");
            HttpContext.Session.Remove("RoleId");
            HttpContext.Session.Remove("Email");

            TempData["AlertCss"] = "alert-warning";
            TempData["Alert"] = "Hesaptan çıkış yapıldı.";

            return RedirectToAction("Login");
        }

        [Route("dashboard")]
        public IActionResult Dashboard()
        {
            if (!CheckLogin())
            {
                TempData["Alert"] = "Gelir ve gider tablosunu gönderebilmek için bir hesaba giriş yapmalısın.";
                TempData["AlertCss"] = "alert-danger";
                return RedirectToAction("Login", "Home");
            }

            GetSessions();

            var total = new Total();

            using (var connection = new SqlConnection(connectionString))
            {
                var sql = "SELECT * FROM gelirTables WHERE type = 0;";
                var income = connection.Query<Income>(sql).ToList();
                total.Income = income;
            }

            using (var connection = new SqlConnection(connectionString))
            {
                var sql = "SELECT * FROM gelirTables WHERE type = 1;";
                var outcome = connection.Query<Income>(sql).ToList();
                total.Outcome = outcome;
            }
            return View(total);
        }
    }
}
