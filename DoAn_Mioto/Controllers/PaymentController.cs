using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Mioto.Models;
using System.Net;
using System.Web.Mvc;
using System.Linq;
using System.Data.Entity;
using DoAn_Mioto.Models;
using System.Data.Entity.Infrastructure;

namespace Mioto.Controllers
{
    public class PaymentController : Controller
    {
        private readonly DB_MiotoAEntities db = new DB_MiotoAEntities();
        public bool IsLoggedIn => Session["KhachHang"] != null || Session["ChuXe"] != null;

        public ActionResult InfoCar(string BienSoXe)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(BienSoXe))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var xe = db.Xe.FirstOrDefault(x => x.BienSo == BienSoXe);
            if (xe == null)
            {
                return HttpNotFound();
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            return View(xe);
        }

        public ActionResult Alert()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            return View();
        }

        // GET: BookingCar
        public ActionResult BookingCar(string BienSoXe)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var khachHang = Session["KhachHang"] as KhachHang;
            var xe = db.Xe.FirstOrDefault(x => x.BienSo == BienSoXe);
            var chuXe = db.ChuXe.FirstOrDefault(x => x.IDCX == xe.IDCX);

            if (xe == null || chuXe == null)
                return HttpNotFound();

            if (khachHang != null)
            {
                if (khachHang.CCCD == "No" || khachHang.GPLX == "No")
                {
                    return RedirectToAction("Alert", "Payment");
                }
            }

            var bookingCarModel = new MD_BookingCar
            {
                Xe = xe,
                ChuXe = chuXe,
            };
            return View(bookingCarModel);
        }

        // POST: BookingCar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BookingCar(MD_BookingCar bookingCar)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var khachHang = Session["KhachHang"] as KhachHang;
            if (ModelState.IsValid)
            {
                // Kiểm tra xe
                var isAvailableXe = !db.DonThueXe.Any(d => d.BienSo == bookingCar.Xe.BienSo &&
                                              ((d.NgayThue < bookingCar.NgayTra && d.NgayTra > bookingCar.NgayThue) ||
                                               (d.NgayThue < bookingCar.NgayThue && d.NgayTra > bookingCar.NgayThue) ||
                                               (d.NgayThue < bookingCar.NgayTra && d.NgayTra > bookingCar.NgayTra)) && d.TrangThaiThanhToan == 1);

                if (!isAvailableXe)
                {
                    ModelState.AddModelError("", "Xe không còn khả dụng trong khoảng thời gian này.");
                    return View(bookingCar);
                }

                // Tạo đơn thuê xe
                var donThueXe = new DonThueXe
                {
                    NgayThue = bookingCar.NgayThue,
                    NgayTra = bookingCar.NgayTra,
                    TrangThaiThanhToan = 0,
                    TongTien = bookingCar.Xe.GiaThue * (bookingCar.NgayTra - bookingCar.NgayThue).Days,
                    PhanTramHoaHong = 10,
                    IDKH = khachHang.IDKH,
                    BienSo = bookingCar.Xe.BienSo,
                    IDMGG = null
                };

                // Thêm đơn thuê xe vào cơ sở dữ liệu
                db.DonThueXe.Add(donThueXe);
                db.SaveChanges();

                // Có thể chuyển hướng hoặc thông báo thành công
                return RedirectToAction("Payment", new { iddt = donThueXe.IDTX });
            }

            // Nếu model không hợp lệ, trở lại view với thông báo lỗi
            return View(bookingCar);
        }



        public ActionResult Payment(int iddt)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var khachHang = Session["KhachHang"] as KhachHang;
            if (khachHang == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var donThueXe = db.DonThueXe.FirstOrDefault(t => t.IDTX == iddt);
            if (donThueXe == null)
            {
                return HttpNotFound();
            }

            var xe = db.Xe.FirstOrDefault(t => t.BienSo == donThueXe.BienSo);
            if (xe == null)
            {
                return HttpNotFound();
            }

            var thanhToan = new MD_Payment
            {
                IDDT = donThueXe.IDTX,
                PhuongThuc = "Chưa xác định", // Có thể để người dùng chọn phương thức thanh toán
                NgayTT = DateTime.Now,
                SoTien = donThueXe.TongTien,
                TrangThai = "Chưa thanh toán",
            };

            // Lưu trữ xe trong Session nếu cần thiết
            Session["Xe"] = xe;

            // Truyền thông tin thanh toán tới View để hiển thị form thanh toán
            return View(thanhToan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(MD_Payment thanhToan)
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                // Tìm mã giảm giá (nếu có)
                MaGiamGia maGiamGia = null;
                if (!string.IsNullOrEmpty(thanhToan.MaGiamGia))
                {
                    maGiamGia = db.MaGiamGia.FirstOrDefault(m => m.MaGG == thanhToan.MaGiamGia);
                }

                // Tìm đơn thuê xe
                var donThueXe = db.DonThueXe.FirstOrDefault(t => t.IDTX == thanhToan.IDDT);
                if (donThueXe == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy đơn thuê xe.");
                    return View(thanhToan);
                }

                // Tính toán số tiền thanh toán
                var soTien = donThueXe.TongTien;
                if (maGiamGia != null)
                {
                    // Áp dụng giảm giá
                    soTien -= (soTien * maGiamGia.PhanTramGiam / 100);
                    if (soTien < 0) soTien = 0;
                    donThueXe.TongTien = soTien;

                    //db.Entry(donThueXe).State = EntityState.Modified;
                    db.DonThueXe.Add(donThueXe);
                    //db.SaveChanges();
                }

                // Cập nhật thông tin thanh toán vào đơn thuê xe
                donThueXe.TrangThaiThanhToan = 1; // 1 có thể chỉ trạng thái "Đã thanh toán"
                                                  //donThueXe.NgayTT = DateTime.Now;
                db.Entry(donThueXe).State = EntityState.Modified;
                db.SaveChanges();
                Session["DonThueXe"] = donThueXe;
                return RedirectToAction("QRPayment", "Payment");
            }
            return RedirectToAction("Home", "Home");
        }

        public ActionResult QRPayment()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");

            var donthuexe = Session["DonThueXe"] as DonThueXe;
            var info = new Dictionary<string, string>
            {
                {"BANK_ID", "MB" },
                {"ACCOUNT_NO", "0932175716" },
                {"OWNER", "Nguyen Huu Phuoc" }
            };
            var tongtien = Convert.ToInt32(donthuexe.TongTien);
            var random = new Random();
            var randomLetters = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            var paidContent = $"MIOTO{donthuexe.IDTX}{randomLetters}";
            string QR = $"https://img.vietqr.io/image/{info["BANK_ID"]}-{info["ACCOUNT_NO"]}-compact2.png?amount={tongtien}&addInfo={paidContent}&accountName={Uri.EscapeDataString(info["OWNER"])}";
            ViewBag.QRCodeUrl = QR;
            Session["DonThueXe"] = donthuexe;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ApplyDiscount(string discountCode, decimal SoTien)
        {
            var khachhang = Session["KhachHang"] as KhachHang;
            var donThueXe = db.DonThueXe.FirstOrDefault(t => t.IDKH == khachhang.IDKH);

            var discount = db.MaGiamGia.FirstOrDefault(m => m.MaGG == discountCode);

            if (discount == null)
            {
                return Json(new { success = false, message = "Mã giảm giá không đúng." });
            }
            else if (discount.SoLuong <= 0)
            {
                return Json(new { success = false, message = "Mã giảm giá đã hết lần sử dụng." });
            }
            else if (discount.NgayKeyThuc < DateTime.Now)
            {
                return Json(new { success = false, message = "Mã giảm giá đã hết hạn sử dụng." });
            }
            else
            {
                var hasUsedCode = db.DonThueXe.Any(t => t.IDMGG == discount.IDMGG && t.IDTX == donThueXe.IDTX);
                if (hasUsedCode)
                {
                    return Json(new { success = false, message = "Bạn đã sử dụng mã giảm giá này." });
                }

                discount.SoLuong--;
                db.Entry(discount).State = EntityState.Modified;
                db.SaveChanges();

                var discountedAmount = SoTien - (SoTien * discount.PhanTramGiam / 100);
                if (discountedAmount < 0) discountedAmount = 0;

                return Json(new { success = true, discountedAmount = discountedAmount.ToString("N0") });
            }
        }




        public ActionResult CongratulationPaymentDone()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Account");
            return View();
        }

        // Trang đánh giá chuyến đi
        public ActionResult TripReview()
        {
            // Lấy thông tin về chuyến đi gần đây của khách hàng để hiển thị (ví dụ: thông tin từ bảng DonThueXe)
            var customerId = Session["IDKH"] as int?; 
            var lastTrip = db.DonThueXe
                             .Where(d => d.IDKH == customerId)
                             .OrderByDescending(d => d.NgayThue)
                             .FirstOrDefault();

            if (lastTrip == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy chuyến đi gần đây để đánh giá.";
                return RedirectToAction("TripReview", "Payment");
            }

            // Tạo đối tượng Đánh giá mới
            var review = new DanhGia
            {
                IDDT = lastTrip.IDTX,
                Ngay = DateTime.Now
            };

            return View(review);
        }

        // Xử lý gửi đánh giá
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitReview(DanhGia model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin chuyến đi (DonThueXe) tương ứng với IDDT trong đánh giá
                    var trip = db.DonThueXe.Find(model.IDDT);

                    if (trip != null)
                    {
                        // Thêm đánh giá vào cơ sở dữ liệu
                        db.DanhGia.Add(model);
                        db.SaveChanges();

                        TempData["SuccessMessage"] = "Đánh giá của bạn đã được gửi thành công!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy chuyến đi để đánh giá.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình gửi đánh giá. Vui lòng thử lại!";
                }

                return RedirectToAction("TripReview");
            }

            // Nếu có lỗi trong quá trình xác thực mô hình
            return View("TripReview", model);
        }
    }
}

