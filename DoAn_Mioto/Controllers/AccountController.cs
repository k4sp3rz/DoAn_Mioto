using DoAn_Mioto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
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
                    if (db.KhachHang.Any(x => x.Email == kh.Email))
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại. Vui lòng sử dụng email khác.");
                        return View(kh);
                    }

                    var otp = new Random().Next(100000, 999999).ToString();
                    Session["OTP"] = otp;
                    Session["RegisterInfo"] = kh;

                    SendOtpEmail(kh.Email, otp);

                    return RedirectToAction("VerifyOtp");
                }
                return View(kh);
            }
            catch
            {
                ViewBag.ErrorRegister = "Đăng ký không thành công. Vui lòng thử lại.";
                return View(kh);
            }
        }

        private void SendOtpEmail(string email, string otp)
        {
            var fromAddress = new MailAddress("sydang2296@gmail.com", "Mioto");
            var toAddress = new MailAddress(email);
            const string fromPassword = "oopz vicm xvmn dinl​";
            const string subject = "Mã xác thực đăng ký tài khoản";
            string body = $"Mã xác thực của bạn là: {otp}";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }

        public ActionResult VerifyOtp()
        {


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyOtp(string otp)
        {
            var sessionOtp = Session["OTP"]?.ToString();
            var registerInfo = Session["RegisterInfo"] as MD_Register;

            if (otp == sessionOtp && registerInfo != null)
            {
                var newKhachHang = new KhachHang
                {
                    Ten = registerInfo.Ten,
                    Email = registerInfo.Email,
                    GioiTinh = registerInfo.GioiTinh,
                    DiaChi = registerInfo.DiaChi,
                    SDT = registerInfo.SDT,
                    GPLX = "0",
                    NgaySinh = registerInfo.NgaySinh,
                    MatKhau = registerInfo.MatKhau,
                    CCCD = "0"
                };
                db.KhachHang.Add(newKhachHang);
                db.SaveChanges();

                TempData["Message"] = "Đăng ký thành công!";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("OTP", "Mã xác thực không đúng.");
            return View();
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