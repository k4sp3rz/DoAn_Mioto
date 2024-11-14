using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DoAn_Mioto.Models
{
    public class TripReview
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Nội dung đánh giá không được vượt quá 1000 ký tự.")]
        public string NoiDung { get; set; }
        public DateTime Ngay { get; set; }
    }
}