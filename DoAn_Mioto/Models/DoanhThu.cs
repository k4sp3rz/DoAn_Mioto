//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DoAn_Mioto.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class DoanhThu
    {
        public int IDDT { get; set; }
        public decimal DoanhThuNgay { get; set; }
        public decimal DoanhThuTuan { get; set; }
        public decimal DoanhThuThang { get; set; }
        public decimal DoanhThuNam { get; set; }
        public System.DateTime NgayCapNhat { get; set; }
        public int IDCX { get; set; }
        public int IDNV { get; set; }
    
        public virtual ChuXe ChuXe { get; set; }
        public virtual NhanVien NhanVien { get; set; }
    }
}
