using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Maison.Models
{
    public class shopdb: DbContext

    {
        public shopdb():base("name=shop") 
        
        {
            this.Configuration.ProxyCreationEnabled = false;
        }
        public virtual DbSet<Sanpham> Sanphams { get; set; }
        public virtual DbSet<BienThe> BienThes { get; set; }
        public virtual DbSet<ThuocTinh> ThuocTinhs { get; set; }
        public virtual DbSet<GiaTriTT> GiaTriTTs { get; set; }
        public virtual DbSet<HoaDon> HoaDons { get; set; }
        public virtual DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }
        public virtual DbSet<ChiTietBT> ChiTietBTs { get; set; }
        public virtual DbSet<Danhmuc> Danhmucs { get; set; }
        public virtual DbSet<TaiKhoanNguoiDung> TaiKhoanNguoiDungs { get; set; }
        public virtual DbSet<TaiKhoanQuanTri> TaiKhoanQuanTris { get; set; }
        public virtual DbSet<Baohanh> Baohanhs { get; set; }
        public virtual DbSet<Brand> Brands { get; set; }
        public virtual DbSet<TinTuc> TinTucs { get; set; }
        public virtual DbSet<KhuyenMai> KhuyenMais { get; set; }
        public virtual DbSet<DanhGia> DanhGias { get; set; }
        public virtual DbSet<SanPhamKhuyenMai> SanPhamKhuyenMais { get; set; }
        public virtual DbSet<ThuVienAnh> ThuVienAnhs { get; set; }
        public virtual DbSet<ChatbotKnowledge> ChatbotKnowledges { get; set; }
        public virtual DbSet<ChatbotLog> ChatbotLogs { get; set; }
        public virtual DbSet<GioHang> GioHangs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // 1. Tắt cascade: TaiKhoan -> HoaDon (Đã sửa lỗi tự đẻ cột)
            modelBuilder.Entity<HoaDon>()
                .HasRequired(h => h.TaiKhoanNguoiDung)
                .WithMany(t => t.HoaDons) // Khớp với ICollection<HoaDon> HoaDons trong TaiKhoanNguoiDung
                .HasForeignKey(h => h.MaTK)
                .WillCascadeOnDelete(false);

            // 2. Tắt cascade: TaiKhoan -> BaoHanh (Đã sửa lỗi)
            modelBuilder.Entity<Baohanh>()
                .HasRequired(b => b.TaiKhoanNguoiDung)
                .WithMany(t => t.Baohanhs) // Khớp với ICollection<Baohanh> Baohanhs
                .HasForeignKey(b => b.MaTK)
                .WillCascadeOnDelete(false);

            // 3. Tắt cascade: HoaDon -> BaoHanh (Đã sửa lỗi)
            modelBuilder.Entity<Baohanh>()
                .HasRequired(b => b.HoaDon)
                .WithMany(h => h.Baohanhs) // Khớp với ICollection<Baohanh> Baohanhs trong HoaDon
                .HasForeignKey(b => b.MaHD)
                .WillCascadeOnDelete(false);

            // 4. Tắt cascade: BienThe -> BaoHanh
            modelBuilder.Entity<Baohanh>()
                .HasRequired(b => b.BienThe)
                .WithMany() // Cứ để trống nếu trong bảng BienThe bạn KHÔNG tạo ICollection<Baohanh>
                .HasForeignKey(b => b.MaBT)
                .WillCascadeOnDelete(false);

            // 5. Ngăn lỗi cho Giỏ Hàng (Bổ sung thêm cho an toàn tuyệt đối)
            modelBuilder.Entity<GioHang>()
                .HasRequired(g => g.TaiKhoanNguoiDung)
                .WithMany(t => t.GioHangs) // Khớp với ICollection<GioHang> GioHangs
                .HasForeignKey(g => g.MaTK)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}