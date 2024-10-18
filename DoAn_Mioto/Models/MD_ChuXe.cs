using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DoAn_Mioto.Models
{
    public class MD_ChuXe
    {
        public int IDCX { get; set; }
        public string HinhAnh { get; set; }

        // Các thuộc tính của Xe
        [Required(ErrorMessage = "Vui lòng nhập biển số xe.")]
        [StringLength(20, ErrorMessage = "Độ dài tối đa của {0} là {1} ký tự.")]
        public string BienSo { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập hãng xe.")]
        [StringLength(50, ErrorMessage = "Độ dài tối đa của {0} là {1} ký tự.")]
        public string HangXe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập màu xe.")]
        [StringLength(50, ErrorMessage = "Độ dài tối đa của {0} là {1} ký tự.")]
        public string Mau { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập năm sản xuất.")]
        [Range(1886, int.MaxValue, ErrorMessage = "Năm sản xuất phải lớn hơn hoặc bằng 1886.")]
        public int NamSX { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số ghế.")]
        [Range(1, 100, ErrorMessage = "Số ghế phải nằm trong khoảng từ 1 đến 100.")]
        public int SoGhe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tính năng.")]
        [StringLength(255, ErrorMessage = "Độ dài tối đa của {0} là {1} ký tự.")]
        public string TinhNang { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá thuê.")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Giá thuê phải lớn hơn 0.")]
        public decimal GiaThue { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khu vực.")]
        [StringLength(100, ErrorMessage = "Khu vực không hợp lệ.")]
        public string KhuVuc { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đơn giá vận chuyển.")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá vận chuyển phải lớn hơn 0.")]
        public decimal DonGiaVanChuyen { get; set; }

    }
}