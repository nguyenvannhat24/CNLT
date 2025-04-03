using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hao_Hao.Models
{
    [Table("ChiTietGioHang")]
    public class ChiTietGioHang
    {
        internal object id;

        [Key]
        public int Id { get; set; }

        [Required]
        public int GioHangId { get; set; }

        [ForeignKey("GioHangId")]
        public virtual GioHang GioHang { get; set; }

        [Required]
        public int ProductId { get; set; } // ID sản phẩm

        [ForeignKey("ProductId")]
        public virtual SanPham? SanPham { get; set; } // Phải là kiểu tham chiếu (class)

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
    }
}
