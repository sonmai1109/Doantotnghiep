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
            // ❌ Tắt cascade: TaiKhoan → HoaDon
            modelBuilder.Entity<HoaDon>()
                .HasRequired(h => h.TaiKhoanNguoiDung)
                .WithMany()
                .HasForeignKey(h => h.MaTK)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Baohanh>()
            .HasRequired(b => b.TaiKhoanNguoiDung)
            .WithMany()
            .HasForeignKey(b => b.MaTK)
            .WillCascadeOnDelete(false);

            // ❌ Tắt cascade: BaoHanh → TaiKhoan
            modelBuilder.Entity<Baohanh>()
                .HasRequired(b => b.TaiKhoanNguoiDung)
                .WithMany()
                .HasForeignKey(b => b.MaTK)
                .WillCascadeOnDelete(false);

            // ❌ Tắt cascade: BaoHanh → HoaDon
            modelBuilder.Entity<Baohanh>()
                .HasRequired(b => b.HoaDon)
                .WithMany()
                .HasForeignKey(b => b.MaHD)
                .WillCascadeOnDelete(false);

            // ❌ Tắt cascade: BaoHanh → BienThe
            modelBuilder.Entity<Baohanh>()
                .HasRequired(b => b.BienThe)
                .WithMany()
                .HasForeignKey(b => b.MaBT)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}