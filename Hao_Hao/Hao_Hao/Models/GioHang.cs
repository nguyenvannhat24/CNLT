using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hao_Hao.Models
{
    [Table("GioHang")]
    public class GioHang
    {
        [Key]
        public int Id { get; set; } // ID của giỏ hàng

        [Required]
        public int UserId { get; set; } // Người dùng sở hữu giỏ hàng

        [ForeignKey("UserId")]
        public virtual User User { get; set; } // Liên kết với bảng Users

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo giỏ hàng

        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } // Danh sách sản phẩm trong giỏ hàng
    }
}
