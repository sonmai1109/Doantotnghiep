namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Themnhieubang : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.sanphamchitiet", "MaSP", "dbo.SanPham");
            DropIndex("dbo.sanphamchitiet", new[] { "MaSP" });
            CreateTable(
                "dbo.Baohanhs",
                c => new
                    {
                        MaPhieu = c.Int(nullable: false, identity: true),
                        MaBT = c.Int(nullable: false),
                        MaHD = c.Int(nullable: false),
                        MaTK = c.Int(nullable: false),
                        NgayTiepNhan = c.DateTime(),
                        NgayHenTra = c.DateTime(),
                        TinhTrangLoi = c.String(),
                        NoiDungSuaChua = c.String(),
                        ChiPhiSuaChua = c.Decimal(precision: 18, scale: 2),
                        TrangThai = c.Int(),
                        BienThe_MaBT = c.Int(),
                        HoaDon_MaHD = c.Int(),
                    })
                .PrimaryKey(t => t.MaPhieu)
                .ForeignKey("dbo.BienThe", t => t.BienThe_MaBT)
                .ForeignKey("dbo.HoaDon", t => t.HoaDon_MaHD)
                .ForeignKey("dbo.BienThe", t => t.MaBT)
                .ForeignKey("dbo.HoaDon", t => t.MaHD)
                .ForeignKey("dbo.TaiKhoanNguoiDung", t => t.MaTK)
                .Index(t => t.MaBT)
                .Index(t => t.MaHD)
                .Index(t => t.MaTK)
                .Index(t => t.BienThe_MaBT)
                .Index(t => t.HoaDon_MaHD);
            
            CreateTable(
                "dbo.BienThe",
                c => new
                    {
                        MaBT = c.Int(nullable: false, identity: true),
                        MaSP = c.Int(nullable: false),
                        GiaBan = c.Decimal(nullable: false, storeType: "money"),
                        SoLuongTon = c.Int(nullable: false),
                        HinhAnh = c.String(maxLength: 150),
                        TrangThai = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.MaBT)
                .ForeignKey("dbo.SanPham", t => t.MaSP, cascadeDelete: true)
                .Index(t => t.MaSP);
            
            CreateTable(
                "dbo.ChiTietBT",
                c => new
                    {
                        MaBT = c.Int(nullable: false),
                        MaGT = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.MaBT, t.MaGT })
                .ForeignKey("dbo.BienThe", t => t.MaBT, cascadeDelete: true)
                .ForeignKey("dbo.GiaTriTT", t => t.MaGT, cascadeDelete: true)
                .Index(t => t.MaBT)
                .Index(t => t.MaGT);
            
            CreateTable(
                "dbo.GiaTriTT",
                c => new
                    {
                        MaGT = c.Int(nullable: false, identity: true),
                        MaTT = c.Int(nullable: false),
                        GiaTri = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.MaGT)
                .ForeignKey("dbo.ThuocTinh", t => t.MaTT, cascadeDelete: true)
                .Index(t => t.MaTT);
            
            CreateTable(
                "dbo.ThuocTinh",
                c => new
                    {
                        MaTT = c.Int(nullable: false, identity: true),
                        MaDM = c.Int(),
                        TenTT = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.MaTT)
                .ForeignKey("dbo.DanhMuc", t => t.MaDM)
                .Index(t => t.MaDM);
            
            CreateTable(
                "dbo.ChiTietHoaDon",
                c => new
                    {
                        MaHD = c.Int(nullable: false),
                        MaBT = c.Int(nullable: false),
                        SoLuongMua = c.Int(nullable: false),
                        GiaMua = c.Decimal(nullable: false, storeType: "money"),
                        NgayHetHanBH = c.DateTime(),
                    })
                .PrimaryKey(t => new { t.MaHD, t.MaBT })
                .ForeignKey("dbo.BienThe", t => t.MaBT, cascadeDelete: true)
                .ForeignKey("dbo.HoaDon", t => t.MaHD, cascadeDelete: true)
                .Index(t => t.MaHD)
                .Index(t => t.MaBT);
            
            CreateTable(
                "dbo.HoaDon",
                c => new
                    {
                        MaHD = c.Int(nullable: false, identity: true),
                        MaTK = c.Int(nullable: false),
                        NgayDat = c.DateTime(nullable: false),
                        GhiChu = c.String(maxLength: 500),
                        TrangThai = c.Int(nullable: false),
                        HoTenNguoiNhan = c.String(nullable: false, maxLength: 100),
                        DiaChiNhan = c.String(nullable: false, maxLength: 250),
                        SoDienThoaiNhan = c.String(nullable: false, maxLength: 11),
                        NgaySua = c.DateTime(),
                        NguoiSua = c.String(),
                    })
                .PrimaryKey(t => t.MaHD)
                .ForeignKey("dbo.TaiKhoanNguoiDung", t => t.MaTK)
                .Index(t => t.MaTK);
            
            DropTable("dbo.sanphamchitiet");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.sanphamchitiet",
                c => new
                    {
                        IDCTSP = c.Int(nullable: false, identity: true),
                        MaSP = c.Int(nullable: false),
                        MaKichCo = c.Int(nullable: false),
                        SoLuong = c.Int(nullable: false),
                        MaMau = c.Int(),
                        Gia = c.Decimal(precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.IDCTSP);
            
            DropForeignKey("dbo.Baohanhs", "MaTK", "dbo.TaiKhoanNguoiDung");
            DropForeignKey("dbo.Baohanhs", "MaHD", "dbo.HoaDon");
            DropForeignKey("dbo.Baohanhs", "MaBT", "dbo.BienThe");
            DropForeignKey("dbo.HoaDon", "MaTK", "dbo.TaiKhoanNguoiDung");
            DropForeignKey("dbo.ChiTietHoaDon", "MaHD", "dbo.HoaDon");
            DropForeignKey("dbo.Baohanhs", "HoaDon_MaHD", "dbo.HoaDon");
            DropForeignKey("dbo.ChiTietHoaDon", "MaBT", "dbo.BienThe");
            DropForeignKey("dbo.GiaTriTT", "MaTT", "dbo.ThuocTinh");
            DropForeignKey("dbo.ThuocTinh", "MaDM", "dbo.DanhMuc");
            DropForeignKey("dbo.BienThe", "MaSP", "dbo.SanPham");
            DropForeignKey("dbo.ChiTietBT", "MaGT", "dbo.GiaTriTT");
            DropForeignKey("dbo.ChiTietBT", "MaBT", "dbo.BienThe");
            DropForeignKey("dbo.Baohanhs", "BienThe_MaBT", "dbo.BienThe");
            DropIndex("dbo.HoaDon", new[] { "MaTK" });
            DropIndex("dbo.ChiTietHoaDon", new[] { "MaBT" });
            DropIndex("dbo.ChiTietHoaDon", new[] { "MaHD" });
            DropIndex("dbo.ThuocTinh", new[] { "MaDM" });
            DropIndex("dbo.GiaTriTT", new[] { "MaTT" });
            DropIndex("dbo.ChiTietBT", new[] { "MaGT" });
            DropIndex("dbo.ChiTietBT", new[] { "MaBT" });
            DropIndex("dbo.BienThe", new[] { "MaSP" });
            DropIndex("dbo.Baohanhs", new[] { "HoaDon_MaHD" });
            DropIndex("dbo.Baohanhs", new[] { "BienThe_MaBT" });
            DropIndex("dbo.Baohanhs", new[] { "MaTK" });
            DropIndex("dbo.Baohanhs", new[] { "MaHD" });
            DropIndex("dbo.Baohanhs", new[] { "MaBT" });
            DropTable("dbo.HoaDon");
            DropTable("dbo.ChiTietHoaDon");
            DropTable("dbo.ThuocTinh");
            DropTable("dbo.GiaTriTT");
            DropTable("dbo.ChiTietBT");
            DropTable("dbo.BienThe");
            DropTable("dbo.Baohanhs");
            CreateIndex("dbo.sanphamchitiet", "MaSP");
            AddForeignKey("dbo.sanphamchitiet", "MaSP", "dbo.SanPham", "MaSP", cascadeDelete: true);
        }
    }
}
