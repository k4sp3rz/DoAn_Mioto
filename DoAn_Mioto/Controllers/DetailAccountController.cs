using DoAn_Mioto.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace DoAn_Mioto.Controllers
{
    public class DetailAccountController : Controller
    {
        // GET: DetailAccount
        DB_MiotoAEntities db = new DB_MiotoAEntities();
        public bool IsLoggedIn { get => Session["KhachHang"] != null || Session["ChuXe"] != null; }
        private static readonly HttpClient client = new HttpClient();

        List<SelectListItem> tinhThanhPho = new List<SelectListItem>
        {
            new SelectListItem { Text = "TP Hồ Chí Minh", Value = "TP Hồ Chí Minh" },
            new SelectListItem { Text = "Hà Nội", Value = "Hà Nội" },
            new SelectListItem { Text = "Đà Nẵng", Value = "Đà Nẵng" },
            new SelectListItem { Text = "Bình Dương", Value = "Bình Dương" },
            new SelectListItem { Text = "Cần Thơ", Value = "Cần Thơ" },
            new SelectListItem { Text = "Đà Lạt", Value = "Đà Lạt" },
            new SelectListItem { Text = "Nha Trang", Value = "Nha Trang" },
            new SelectListItem { Text = "Quy Nhơn", Value = "Quy Nhơn" },
            new SelectListItem { Text = "Phú Quốc", Value = "Phú Quốc" },
            new SelectListItem { Text = "Hải Phòng", Value = "Hải Phòng" },
            new SelectListItem { Text = "Vũng Tàu", Value = "Vũng Tàu" },
            new SelectListItem { Text = "Thành phố khác", Value = "Thành phố khác" },
        };

        List<SelectListItem> gioitinh = new List<SelectListItem>
        {
         new SelectListItem { Text = "Nam", Value = "Nam" },
         new SelectListItem { Text = "Nữ", Value = "Nữ" }
        };


        List<SelectListItem> TrangThaiXe = new List<SelectListItem>
        {
         new SelectListItem { Text = "Sẵn sàng", Value = "Sẵn sàng" },
         new SelectListItem { Text = "Bảo trì", Value = "Bảo trì" },
         new SelectListItem { Text = "Ngưng cho thuê", Value = "Ngưng cho thuê" }
        };

        // GET: DetailAccount
        public ActionResult InfoAccount()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            var guest = Session["KhachHang"] as KhachHang;
            if (guest == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Khách hàng không tồn tại");
            var kh = db.KhachHang.Where(x => x.IDKH == guest.IDKH);
            return View(kh);
        }

        // GET: EditCCCD/InfoAccount
        public ActionResult EditCCCD(int IDKH)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            // Lấy dữ liệu KhachHang theo IDKH
            var khachHang = db.KhachHang.FirstOrDefault(x => x.IDKH == IDKH);
            if (khachHang == null)
                return HttpNotFound();

            // Truyền dữ liệu KhachHang vào view
            return View(khachHang);
        }

        // POST: EditGPLX/InfoAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCCCD(KhachHang cccd)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Home");

            // Lấy bản ghi KhachHang dựa trên IDKH
            var guest = db.KhachHang.FirstOrDefault(x => x.IDKH == cccd.IDKH);

            try
            {
                if (ModelState.IsValid)
                {
                    // Cập nhật thông tin CCCD trong KhachHang
                    if (guest != null)
                    {
                        guest.CCCD = cccd.CCCD;
                        db.Entry(guest).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    // Kiểm tra nếu khách hàng cũng là chủ xe, thì cập nhật CCCD trong ChuXe
                    var chuxe = db.ChuXe.FirstOrDefault(x => x.IDKH == guest.IDKH);
                    if (chuxe != null)
                    {
                        chuxe.HinhAnh = guest.CCCD; // Giả định HinhAnh lưu CCCD; điều chỉnh nếu cần
                        db.Entry(chuxe).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    // Cập nhật dữ liệu trong Session
                    Session["KhachHang"] = guest;
                    Session["ChuXe"] = chuxe;

                    return RedirectToAction("InfoAccount");
                }
                return View(cccd);
            }
            catch
            {
                return View(cccd);
            }
        }

        // GET: EditGPLX/InfoAccount
        // GET: EditGPLX/InfoAccount
        public ActionResult EditGPLX(int IDKH)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            // Lấy thông tin khách hàng dựa trên IDKH
            var khachHang = db.KhachHang.FirstOrDefault(x => x.IDKH == IDKH);
            if (khachHang == null)
                return HttpNotFound();

            // Truyền thông tin khách hàng vào view để chỉnh sửa GPLX
            return View(khachHang);
        }

        // POST: EditGPLX/InfoAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditGPLX(KhachHang gplx)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Home");

            // Lấy khách hàng từ cơ sở dữ liệu dựa trên IDKH
            var guest = db.KhachHang.FirstOrDefault(x => x.IDKH == gplx.IDKH);

            try
            {
                if (ModelState.IsValid)
                {
                    // Cập nhật thông tin GPLX trong KhachHang
                    if (guest != null)
                    {
                        guest.GPLX = gplx.GPLX; // Giả định trường GPLX trong KhachHang
                        db.Entry(guest).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    // Kiểm tra nếu khách hàng cũng là chủ xe, thì cập nhật GPLX trong ChuXe
                    var chuxe = db.ChuXe.FirstOrDefault(x => x.IDKH == guest.IDKH);
                    if (chuxe != null)
                    {
                        chuxe.HinhAnh = guest.GPLX; // Giả định HinhAnh lưu GPLX; điều chỉnh nếu cần
                        db.Entry(chuxe).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    // Cập nhật thông tin trong session
                    Session["KhachHang"] = guest;
                    Session["ChuXe"] = chuxe;

                    return RedirectToAction("InfoAccount");
                }
                return View(gplx);
            }
            catch
            {
                return View(gplx);
            }
        }


        // GET: EditInfoUser/InfoAccount
        public ActionResult EditInfoUser(int IDKH)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            // Lấy thông tin khách hàng dựa trên IDKH
            var khachHang = db.KhachHang.FirstOrDefault(x => x.IDKH == IDKH);
            if (khachHang == null)
                return HttpNotFound();

            // Cài đặt ViewBag.GioiTinh để chọn giới tính
            ViewBag.GioiTinh = new SelectList(new[] { "Nam", "Nữ", "Khác" }, khachHang.GioiTinh);
            return View(khachHang);
        }

        // POST: EditInfoUser/InfoAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditInfoUser(KhachHang kh)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Home");

            try
            {
                if (ModelState.IsValid)
                {
                    // Lấy thông tin khách hàng và chủ xe từ session
                    var guest = Session["KhachHang"] as KhachHang;
                    var chuxe = Session["ChuXe"] as ChuXe;

                    // Cập nhật thông tin cần thiết từ session vào đối tượng khách hàng
                    kh.GPLX = guest.GPLX;
                    kh.MatKhau = guest.MatKhau;
                    kh.CCCD = guest.CCCD;
                    kh.HinhAnh = guest.HinhAnh;

                    // Cập nhật thông tin khách hàng trong cơ sở dữ liệu
                    db.Entry(kh).State = EntityState.Modified;
                    db.SaveChanges();

                    // Cập nhật thông tin GPLX nếu tồn tại
                    if (!string.IsNullOrEmpty(guest.GPLX))
                    {
                        var existingKhachHang = db.KhachHang.FirstOrDefault(x => x.IDKH == kh.IDKH);
                        if (existingKhachHang != null)
                        {
                            existingKhachHang.Ten = kh.Ten;
                            existingKhachHang.NgaySinh = kh.NgaySinh;
                            existingKhachHang.DiaChi = kh.DiaChi;
                            existingKhachHang.SDT = kh.SDT;
                            existingKhachHang.GioiTinh = kh.GioiTinh;
                            existingKhachHang.Email = kh.Email;
                            db.Entry(existingKhachHang).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }

                    // Cập nhật lại thông tin trong session
                    guest = db.KhachHang.FirstOrDefault(x => x.IDKH == guest.IDKH);
                    chuxe = db.ChuXe.FirstOrDefault(x => x.IDCX == guest.IDKH);
                    Session["KhachHang"] = guest;
                    Session["ChuXe"] = chuxe;

                    // Điều hướng về trang thông tin tài khoản
                    return RedirectToAction("InfoAccount");
                }

                return View(kh);
            }
            catch
            {
                return View(kh);
            }
        }


        // GET: Detailt/MyCar
        public ActionResult MyCar()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            var guest = Session["KhachHang"] as KhachHang;
            var chuxe = Session["ChuXe"] as ChuXe;
            if (guest == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Khách hàng không tồn tại");
            var cars = db.Xe.Where(x => x.IDCX == guest.IDKH).ToList();
            if (chuxe != null)
            {
                cars = db.Xe.Where(x => x.IDCX == chuxe.IDCX).ToList();
                return View(cars);
            }
            return View(cars);
        }

        // GET: EditCar/MyCar
        public ActionResult EditCar(string BienSoXe)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            ViewBag.TinhThanhPho = tinhThanhPho;
            ViewBag.TrangThaiXe = TrangThaiXe;
            if (String.IsNullOrEmpty(BienSoXe))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var xe = db.Xe.FirstOrDefault(x => x.BienSo == BienSoXe);
            if (xe == null)
            {
                return HttpNotFound();
            }
            return View(xe);
        }
        // POST: EditCar/MyCar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCar(Xe xe)
        {
            if (!IsLoggedIn)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.TinhThanhPho = tinhThanhPho;
            try
            {
                if (ModelState.IsValid)
                {
                    var guest = Session["KhachHang"] as KhachHang;
                    xe.IDCX = guest.IDKH;
                    xe.KhuVuc = xe.KhuVuc;
                    db.Entry(xe).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("MyCar");
                }
                return View(xe);
            }
            catch
            {
                return View(xe);
            }
        }


        public ActionResult MyTrip()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var guest = Session["KhachHang"] as KhachHang;
            var chuxe = Session["ChuXe"] as ChuXe;

            if (guest == null && chuxe == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Khách hàng hoặc chủ xe không tồn tại");

            List<MD_MyTrip> myTrips = new List<MD_MyTrip>();

            // Nếu là Khách hàng
            if (guest != null)
            {
                myTrips = db.DonThueXe
                    .Where(x => x.IDKH == guest.IDKH)
                    .Select(dtx => new MD_MyTrip
                    {
                        IDDT = dtx.IDTX,
                        BienSoXe = dtx.BienSo,
                        NgayThue = dtx.NgayThue,
                        NgayTra = dtx.NgayTra,
                        TongTien = dtx.TongTien,
                        //TrangThai = dtx.TrangThaiThanhToan == 1 ? "Đã thanh toán" : "Chưa thanh toán",
                        //ThanhToan = db.ThanhToan.FirstOrDefault(x => x.IDTX == dtx.IDTX),
                        ChuXe = db.ChuXe.FirstOrDefault(cx => cx.IDCX == db.Xe.FirstOrDefault(e => e.BienSo == dtx.BienSo).IDCX),
                    }).ToList();
            }

            // Nếu là Chủ xe
            if (chuxe != null)
            {
                myTrips = db.DonThueXe
                    .Where(x => db.Xe.Any(e => e.BienSo == x.BienSo && e.IDCX == chuxe.IDCX))
                    .Select(dtx => new MD_MyTrip
                    {
                        IDDT = dtx.IDTX,
                        BienSoXe = dtx.BienSo,
                        NgayThue = dtx.NgayThue,
                        NgayTra = dtx.NgayTra,
                        TongTien = dtx.TongTien,
                        //TrangThai = dtx.TrangThaiThanhToan == 1 ? "Đã thanh toán" : "Chưa thanh toán",
                        //ThanhToan = db.ThanhToan.FirstOrDefault(x => x.IDTX == dtx.IDTX),
                        ChuXe = db.ChuXe.FirstOrDefault(cx => cx.IDCX == chuxe.IDCX)
                    }).ToList();
            }

            return View(myTrips);
        }



        public ActionResult LongTermOrder()
        {
            return View();
        }
        public ActionResult RequestDeleteAccount()
        {
            return View();
        }
        public ActionResult MyAddress()
        {
            return View();
        }

        public ActionResult ChangePassword()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(MD_ChangePassword model)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            if (!ModelState.IsValid)
                return View(model);

            var guest = Session["KhachHang"] as KhachHang;
            var existingKH = db.KhachHang.Find(guest.IDKH);
            var existingCX = db.ChuXe.Find(guest.IDKH);

            if (guest == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra mật khẩu cũ
            if (VerifyPassword(guest, model.OldPassword))
            {
                if (existingCX == null)
                {
                    // Cập nhật mật khẩu mới cho Khách hàng
                    existingKH.MatKhau = model.NewPassword;
                    db.Entry(existingKH).State = EntityState.Modified;
                    db.SaveChanges();
                    Session["KhachHang"] = existingKH;
                }
                else
                {
                    // Cập nhật mật khẩu mới cho Khách hàng
                    existingKH.MatKhau = model.NewPassword;
                    db.Entry(existingKH).State = EntityState.Modified;
                    db.SaveChanges();
                    Session["KhachHang"] = existingKH;
                    //// Cập nhật mật khẩu mới cho Chủ xe
                    //existingCX.MatKhau = model.NewPassword;
                    //db.Entry(existingCX).State = EntityState.Modified;
                    //db.SaveChanges();
                    //Session["ChuXe"] = existingCX;
                }
                return RedirectToAction("InfoAccount");
            }
            else
            {
                // Mật khẩu cũ không đúng
                ViewBag.ErrorMessage = "Mật khẩu hiện tại không đúng. Vui lòng thử lại.";
                return View(model);
            }
        }

        // Thay đổi avatar user
        [HttpPost]
        public ActionResult ChangeAvatarUser(HttpPostedFileBase avatar)
        {
            // Kiểm tra xem file có tồn tại không
            if (avatar != null && avatar.ContentLength > 0)
            {
                // Kiểm tra định dạng file (chỉ cho phép các định dạng ảnh phổ biến)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(avatar.FileName).ToLower();

                if (allowedExtensions.Contains(extension))
                {
                    // Tạo tên file duy nhất để tránh trùng lặp
                    var fileName = Guid.NewGuid().ToString() + extension;

                    // Đường dẫn lưu file
                    var path = Path.Combine(Server.MapPath("~/AvatarUser/"), fileName);

                    // Lưu file lên server
                    avatar.SaveAs(path);

                    // Cập nhật thông tin hình ảnh của khách hàng trong cơ sở dữ liệu
                    var guest = Session["KhachHang"] as KhachHang;
                    var existingKH = db.KhachHang.Find(guest.IDKH);
                    var existingCX = db.ChuXe.FirstOrDefault(x => x.IDKH == guest.IDKH);

                    if (existingKH != null)
                    {
                        existingKH.HinhAnh = fileName;

                        // Kiểm tra chủ xe
                        if (existingCX != null)
                        {
                            existingCX.HinhAnh = fileName;
                            db.Entry(existingCX).State = EntityState.Modified;
                            Session["ChuXe"] = existingCX;
                        }
                        // Cập nhật cơ sở dữ liệu
                        Session["KhachHang"] = existingKH;
                        db.Entry(existingKH).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    return RedirectToAction("InfoAccount");
                }
                else
                {
                    ModelState.AddModelError("", "Định dạng file không hợp lệ. Vui lòng chọn một file ảnh.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Vui lòng chọn một file ảnh.");
            }

            // Trả về view với các lỗi nếu có
            return View("InfoAccount");
        }

        public ActionResult Logout()
        {
            // Xóa tất cả các session của người dùng
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }
        private bool VerifyPassword(KhachHang user, string password)
        {
            return user.MatKhau == password;
        }

        public ActionResult DeleteTrip(int id)
        {
            // Kiểm tra người dùng đã đăng nhập
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            // Lấy thông tin chuyến thuê xe
            var donThueXe = db.DonThueXe.FirstOrDefault(x => x.IDTX == id);
            if (donThueXe == null)
                return HttpNotFound("Chuyến thuê không tồn tại");

            var currentDateTime = DateTime.Now;
            var bookingDateTime = donThueXe.NgayThue;
            var timeDifference = (currentDateTime - donThueXe.NgayThue).TotalDays;

            // Xác định người dùng hủy chuyến
            var guest = Session["KhachHang"] as KhachHang;

            decimal hoanTien = 0;
            if (guest != null)
            {
                // Khách hàng hủy chuyến
                if (currentDateTime <= bookingDateTime.AddHours(1))
                {
                    // <= 1 giờ sau giữ chỗ
                    hoanTien = donThueXe.TongTien; // Hoàn tiền 100%
                }
                else if (timeDifference > 7)
                {
                    // Hủy trước chuyến đi > 7 ngày
                    hoanTien = donThueXe.TongTien * 70 / 100; // Hoàn tiền 70%
                }
                else
                {
                    // Hủy <= 7 ngày trước chuyến đi
                    hoanTien = 0; // Không hoàn tiền
                }

                // Lưu thông tin phí hủy chuyến cho khách hàng
                //var phiHuyChuyen = new PhiHuyChuyen
                //{
                //    IDDT = id,
                //    LoaiHuyChuyen = 1, // Khách hàng
                //    ThoiGianHuy = (currentDateTime <= bookingDateTime.AddHours(1)) ? 1 : (timeDifference > 7 ? 2 : 3),
                //    HoanTien = hoanTien,
                //    DenTien = 0,
                //    MoTa = "Hủy chuyến do khách hàng yêu cầu."
                //};

                //db.PhiHuyChuyen.Add(phiHuyChuyen);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Người dùng không hợp lệ");
            }

            db.SaveChanges();

            // Xóa đơn thanh toán
            //var thanhtoan = db.ThanhToan.FirstOrDefault(x => x.IDDT == donThueXe.IDDT);
            //if (thanhtoan != null)
            //{
            //    db.ThanhToan.Remove(thanhtoan);
            //    db.SaveChanges();
            //}

            //// Xóa đơn thuê xe
            //donThueXe.TrangThai = 2;
            //db.Entry(donThueXe).State = EntityState.Modified;
            //db.SaveChanges();

            return RedirectToAction("MyTrip");
        }
        // GET: RentedCar
        public ActionResult RentedCar()
        {
            // Kiểm tra người dùng đã đăng nhập
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var chuxe = Session["ChuXe"] as ChuXe;
            var khachhang = Session["KhachHang"] as KhachHang;

            if (chuxe != null)
            {
                // Lấy danh sách các xe đang được thuê của chủ xe
                var rentedCars = db.DonThueXe
                    .Where(d => d.IDKH == chuxe.IDCX)
                    .Select(d => new MD_RentedCar
                    {
                        IDDT = d.IDTX, // Sử dụng IDTX từ DonThueXe
                        BienSoXe = d.BienSo, // Giả sử có thuộc tính BienSo trong DonThueXe
                        HangXe = db.Xe.FirstOrDefault(x => x.BienSo == d.BienSo).HangXe, // Lấy HangXe từ bảng Xe
                        MauXe = db.Xe.FirstOrDefault(x => x.BienSo == d.BienSo).Mau, // Lấy MauXe từ bảng Xe
                        NgayThue = d.NgayThue,
                        NgayTra = d.NgayTra,
                        TongTien = d.TongTien,
                        //TrangThai = d.TrangThaiThanhToan // Sử dụng TrangThaiThanhToan từ DonThueXe
                    }).ToList();

                return View(rentedCars);
            }

            return View();
        }


        // GET: RentedCar/CustomerDetails/5
        public ActionResult CustomerDetails(int id)
        {
            // Kiểm tra người dùng đã đăng nhập
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var rentedCar = db.DonThueXe
                .Where(d => d.IDTX == id)
                .Select(d => new MD_CustomerDetails
                {
                    BienSoXe = d.Xe.BienSo,
                    HangXe = d.Xe.HangXe,
                    MauXe = d.Xe.Mau,
                    NgayThue = d.NgayThue,
                    NgayTra = d.NgayTra,
                    TongTien = d.TongTien,
                    //TrangThai = d.TrangThaiThanhToan,
                    KhachHang = d.KhachHang
                })
                .FirstOrDefault();

            if (rentedCar == null)
                return HttpNotFound();

            return View(rentedCar);
        }

        // GET: RentedCar/OwnerDetailt/5
        public ActionResult OwnerDetailt(int id)
        {
            // Kiểm tra người dùng đã đăng nhập
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            // Lấy thông tin thuê xe từ cơ sở dữ liệu
            var donthuexe = db.DonThueXe.FirstOrDefault(x => x.IDTX == id);
            if (donthuexe == null)
                return HttpNotFound();

            // Lấy thông tin chủ xe từ cơ sở dữ liệu
            var chuxe = db.ChuXe.FirstOrDefault(x => x.IDCX == donthuexe.Xe.IDCX);

            // Tạo đối tượng MD_OwnerDetailt từ dữ liệu đã lấy
            var ownerDetailt = new MD_OwnerDetailt
            {
                BienSoXe = donthuexe.Xe.BienSo,
                NgayThue = donthuexe.NgayThue,
                NgayTra = donthuexe.NgayTra,
                TongTien = donthuexe.TongTien,
               // TrangThai = donthuexe.TrangThaiThanhToan,
                ChuXe = chuxe
            };

            // Trả về View với đối tượng MD_OwnerDetailt
            return View(ownerDetailt);
        }



        public ActionResult DeleteRentedCar(int id)
        {
            // Kiểm tra người dùng đã đăng nhập
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            // Lấy thông tin chuyến thuê xe
            var donThueXe = db.DonThueXe.FirstOrDefault(x => x.IDTX == id);
            if (donThueXe == null)
                return HttpNotFound("Chuyến thuê không tồn tại");

            var currentDateTime = DateTime.Now;
            var bookingDateTime = donThueXe.NgayThue;
            var timeDifference = (currentDateTime - bookingDateTime).TotalDays;

            // Xác định người dùng hủy chuyến
            var guest = Session["KhachHang"] as KhachHang;
            var chuxe = Session["ChuXe"] as ChuXe;

            decimal denTien = 0;

            if (chuxe != null)
            {
                // Chủ xe hủy chuyến
                if (currentDateTime <= bookingDateTime.AddHours(1))
                {
                    // <= 1 giờ sau giữ chỗ
                    denTien = 0; // Không mất phí
                }
                else if (timeDifference > 7)
                {
                    // Hủy trước chuyến đi > 7 ngày
                    denTien = donThueXe.TongTien * 30 / 100; // Đền tiền 30%
                }
                else
                {
                    // Hủy <= 7 ngày trước chuyến đi
                    denTien = donThueXe.TongTien; // Đền tiền 100%
                }

                //// Lưu thông tin phí hủy chuyến cho chủ xe
                //var phiHuyChuyen = new PhiHuyChuyen
                //{
                //    IDDT = id,
                //    LoaiHuyChuyen = 2, // Chủ xe
                //    ThoiGianHuy = (currentDateTime <= bookingDateTime.AddHours(1)) ? 1 : (timeDifference > 7 ? 2 : 3),
                //    HoanTien = 0,
                //    DenTien = denTien,
                //    MoTa = "Hủy chuyến do chủ xe yêu cầu."
                //};

                //db.PhiHuyChuyen.Add(phiHuyChuyen);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Người dùng không hợp lệ");
            }

            db.SaveChanges();

            //// Xóa đơn thanh toán
            //var thanhtoan = db.ThanhToan.FirstOrDefault(x => x.IDDT == donThueXe.IDDT);
            //if (thanhtoan != null)
            //{
            //    db.ThanhToan.Remove(thanhtoan);
            //    db.SaveChanges();
            //}

            //// Xóa đơn thuê xe
            //donThueXe.TrangThaiThanhToan = 2; // Hủy
            //db.Entry(donThueXe).State = EntityState.Modified;
            //db.SaveChanges();

            return RedirectToAction("RentedCar");
        }

        public ActionResult RevenueChart()
        {
            // Kiểm tra người dùng đã đăng nhập
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var chuxe = Session["ChuXe"] as ChuXe;
            var khachhang = Session["KhachHang"] as KhachHang;
            if (chuxe == null)
            {
                var view_kh = new DoanhThu
                {
                    DoanhThuNgay = 0,
                    DoanhThuTuan = 0,
                    DoanhThuThang = 0,
                    DoanhThuNam = 0
                };
                return View(view_kh);
            }

            var doanhThu = db.DoanhThu.FirstOrDefault(x => x.IDCX == chuxe.IDCX);
            if (doanhThu == null)
            {
                doanhThu = new DoanhThu
                {
                    DoanhThuNgay = 0,
                    DoanhThuTuan = 0,
                    DoanhThuThang = 0,
                    DoanhThuNam = 0
                };
            }

            var viewModel = new DoanhThu
            {
                DoanhThuNgay = doanhThu.DoanhThuNgay,
                DoanhThuTuan = doanhThu.DoanhThuTuan,
                DoanhThuThang = doanhThu.DoanhThuThang,
                DoanhThuNam = doanhThu.DoanhThuNam
            };

            return View(viewModel);
        }
    }
}