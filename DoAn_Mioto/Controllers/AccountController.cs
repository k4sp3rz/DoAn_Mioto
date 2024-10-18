using DoAn_Mioto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace DoAn_Mioto.Controllers
{
    public class AccountController : Controller
    {
        DB_MiotoAEntities db = new DB_MiotoAEntities();
        List<SelectListItem> gioitinh = new List<SelectListItem>
        {
            new SelectListItem { Text = "Nam", Value = "Nam" },
            new SelectListItem { Text = "Nữ", Value = "Nữ" }
        };

        // GET: Account/Login
        public ActionResult Login()
        {
            if (Session["KhachHang"] != null || Session["NhanVien"] != null || Session["ChuXe"] != null)
                return Logout();
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(MD_Login _user)
        {
            // Check if user is a KhachHang
            var IsGuest = db.KhachHang.SingleOrDefault(s => s.Email == _user.Email && s.MatKhau == _user.MatKhau);

            // Check if user is a ChuXe (check via KhachHang relation)
            var IsChuXe = (IsGuest != null) ? db.ChuXe.SingleOrDefault(s => s.IDKH == IsGuest.IDKH) : null;

            // Check if user is a NhanVien
            var IsNhanVien = db.NhanVien.SingleOrDefault(s => s.Email == _user.Email && s.MatKhau == _user.MatKhau);

            // If KhachHang exists, proceed to login
            if (IsGuest != null)
            {
                Session["KhachHang"] = IsGuest;

                if (IsChuXe != null)
                {
                    Session["ChuXe"] = IsChuXe;
                }

                return RedirectToAction("Home", "Home");
            }

            // If NhanVien exists, proceed to login
            if (IsNhanVien != null)
            {
                Session["NhanVien"] = IsNhanVien;
                return RedirectToAction("Home", "Home");
            }

            // If no valid user found, return error
            ViewBag.ErrorLogin = "Email hoặc mật khẩu không chính xác";
            return View();
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            ViewBag.GioiTinh = gioitinh;
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(MD_Register kh)
        {
            ViewBag.GioiTinh = gioitinh;
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if the email is already registered
                    if (db.KhachHang.Any(x => x.Email == kh.Email))
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại. Vui lòng sử dụng email khác.");
                        return View(kh);
                    }

                    // Create new KhachHang object and set default values for CCCD and GPLX
                    var newKhachHang = new KhachHang
                    {
                        Ten = kh.Ten,
                        Email = kh.Email,
                        GioiTinh = kh.GioiTinh,
                        DiaChi = kh.DiaChi,
                        SDT = kh.SDT,
                        GPLX = kh.GPLX ?? "No",
                        NgaySinh = kh.NgaySinh,
                        MatKhau = kh.MatKhau,
                        CCCD = kh.CCCD ?? "No",
                        HinhAnh = kh.HinhAnh
                    };

                    // Add and save the new KhachHang record
                    db.KhachHang.Add(newKhachHang);
                    db.SaveChanges();

                    TempData["Message"] = "Đăng ký thành công!";
                    return RedirectToAction("Login");
                }
                return View(kh);
            }
            catch
            {
                ViewBag.ErrorRegister = "Đăng ký không thành công. Vui lòng thử lại.";
                return View(kh);
            }
        }

        //GET : Home/Logout
        public ActionResult Logout()
        {
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Home");
        }

    }
}