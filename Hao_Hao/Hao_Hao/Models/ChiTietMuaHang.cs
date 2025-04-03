using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hao_Hao.Models
{
    [Table("ChiTietMuaHang")]
    public class ChiTietMuaHang
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IdUser { get; set; } // Khóa ngoại đến bảng User

        [Required]
        public int IdProduct { get; set; } // Khóa ngoại đến bảng SanPham

        [Required]
        public DateTime TimeBuy { get; set; } // Ngày mua hàng

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTien { get; set; } // Số tiền thanh toán

        // Định nghĩa mối quan hệ với bảng User
        [ForeignKey("IdUser")]
        public virtual User User { get; set; }

        // Định nghĩa mối quan hệ với bảng SanPham
        [ForeignKey("IdProduct")]
        public virtual SanPham SanPham { get; set; }
    }
}
