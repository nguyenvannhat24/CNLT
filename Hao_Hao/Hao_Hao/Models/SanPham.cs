using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Hao_Hao.Models
{
    [Table("SanPham")]
    public class SanPham
    {
        [Key]
        public int Id { get; set; } // ID sản phẩm

        [Required]
        [StringLength(255)]
        public string TenSanPham { get; set; } // Tên sản phẩm

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Gia { get; set; } // Giá sản phẩm

      
        public string MoTa { get; set; } // Mô tả sản phẩm

        [Required]
        [Range(0, int.MaxValue)]
        public int SoLuongTon { get; set; } // Số lượng tồn kho

        public string HinhAnh { get; set; } // Đường dẫn hình ảnh sản phẩm

        // Khởi tạo các ICollection để tránh lỗi khi thêm sản phẩm mới
        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();
        public virtual ICollection<ChiTietMuaHang> ChiTietMuaHangs { get; set; } = new List<ChiTietMuaHang>();

    }
}
