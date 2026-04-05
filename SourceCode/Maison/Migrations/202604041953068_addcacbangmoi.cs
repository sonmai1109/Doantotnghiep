namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addcacbangmoi : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SanPhamKhuyenMai",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        MaSP = c.Int(nullable: false),
                        MaKM = c.Int(nullable: false),
                        MaBT = c.Int(),
                        PhanTramGiam = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.BienThe", t => t.MaBT)
                .ForeignKey("dbo.KhuyenMai", t => t.MaKM, cascadeDelete: true)
                .ForeignKey("dbo.SanPham", t => t.MaSP, cascadeDelete: true)
                .Index(t => t.MaSP)
                .Index(t => t.MaKM)
                .Index(t => t.MaBT);
            
            CreateTable(
                "dbo.KhuyenMai",
                c => new
                    {
                        MaKM = c.Int(nullable: false, identity: true),
                        TenKM = c.String(nullable: false, maxLength: 200),
                        MoTa = c.String(),
                        NgayBatDau = c.DateTime(),
                        NgayKetThuc = c.DateTime(),
                        TrangThai = c.Int(nullable: false),
                        NgayTao = c.DateTime(),
                        NguoiTao = c.String(maxLength: 50),
                        NgaySua = c.DateTime(),
                        NguoiSua = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.MaKM);
            
            CreateTable(
                "dbo.DanhGia",
                c => new
                    {
                        MaDanhGia = c.Int(nullable: false, identity: true),
                        MaTK = c.Int(nullable: false),
                        XepHang = c.Int(nullable: false),
                        BinhLuan = c.String(),
                        NgayTao = c.DateTime(),
                        TrangThai = c.Int(nullable: false),
                        MaBT = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaDanhGia)
                .ForeignKey("dbo.BienThe", t => t.MaBT, cascadeDelete: true)
                .ForeignKey("dbo.TaiKhoanNguoiDung", t => t.MaTK, cascadeDelete: true)
                .Index(t => t.MaTK)
                .Index(t => t.MaBT);
            
            CreateTable(
                "dbo.TinTuc",
                c => new
                    {
                        MaTin = c.Int(nullable: false, identity: true),
                        TieuDe = c.String(nullable: false, maxLength: 250),
                        AnhDaiDien = c.String(maxLength: 250),
                        TomTat = c.String(),
                        NoiDung = c.String(),
                        NgayDang = c.DateTime(),
                        MaTK = c.Int(),
                        LuotXem = c.Int(),
                        TrangThai = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaTin)
                .ForeignKey("dbo.TaiKhoanNguoiDung", t => t.MaTK)
                .Index(t => t.MaTK);
            
            AddColumn("dbo.Baohanh", "TaiKhoanNguoiDung_MaTk", c => c.Int());
            AddColumn("dbo.HoaDon", "TaiKhoanNguoiDung_MaTk", c => c.Int());
            CreateIndex("dbo.Baohanh", "TaiKhoanNguoiDung_MaTk");
            CreateIndex("dbo.HoaDon", "TaiKhoanNguoiDung_MaTk");
            AddForeignKey("dbo.Baohanh", "TaiKhoanNguoiDung_MaTk", "dbo.TaiKhoanNguoiDung", "MaTk");
            AddForeignKey("dbo.HoaDon", "TaiKhoanNguoiDung_MaTk", "dbo.TaiKhoanNguoiDung", "MaTk");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TinTuc", "MaTK", "dbo.TaiKhoanNguoiDung");
            DropForeignKey("dbo.HoaDon", "TaiKhoanNguoiDung_MaTk", "dbo.TaiKhoanNguoiDung");
            DropForeignKey("dbo.DanhGia", "MaTK", "dbo.TaiKhoanNguoiDung");
            DropForeignKey("dbo.DanhGia", "MaBT", "dbo.BienThe");
            DropForeignKey("dbo.Baohanh", "TaiKhoanNguoiDung_MaTk", "dbo.TaiKhoanNguoiDung");
            DropForeignKey("dbo.SanPhamKhuyenMai", "MaSP", "dbo.SanPham");
            DropForeignKey("dbo.SanPhamKhuyenMai", "MaKM", "dbo.KhuyenMai");
            DropForeignKey("dbo.SanPhamKhuyenMai", "MaBT", "dbo.BienThe");
            DropIndex("dbo.TinTuc", new[] { "MaTK" });
            DropIndex("dbo.DanhGia", new[] { "MaBT" });
            DropIndex("dbo.DanhGia", new[] { "MaTK" });
            DropIndex("dbo.HoaDon", new[] { "TaiKhoanNguoiDung_MaTk" });
            DropIndex("dbo.SanPhamKhuyenMai", new[] { "MaBT" });
            DropIndex("dbo.SanPhamKhuyenMai", new[] { "MaKM" });
            DropIndex("dbo.SanPhamKhuyenMai", new[] { "MaSP" });
            DropIndex("dbo.Baohanh", new[] { "TaiKhoanNguoiDung_MaTk" });
            DropColumn("dbo.HoaDon", "TaiKhoanNguoiDung_MaTk");
            DropColumn("dbo.Baohanh", "TaiKhoanNguoiDung_MaTk");
            DropTable("dbo.TinTuc");
            DropTable("dbo.DanhGia");
            DropTable("dbo.KhuyenMai");
            DropTable("dbo.SanPhamKhuyenMai");
        }
    }
}
