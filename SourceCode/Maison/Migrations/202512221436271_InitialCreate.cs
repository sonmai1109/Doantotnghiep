namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DanhMuc",
                c => new
                    {
                        MaDM = c.Int(nullable: false, identity: true),
                        TenDM = c.String(nullable: false, maxLength: 100),
                        NgayTao = c.DateTime(nullable: false),
                        NguoiTao = c.String(nullable: false, maxLength: 100),
                        NgaySua = c.DateTime(),
                        NguoiSua = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.MaDM);
            
            CreateTable(
                "dbo.SanPham",
                c => new
                    {
                        MaSP = c.Int(nullable: false, identity: true),
                        MaDM = c.Int(nullable: false),
                        TenSP = c.String(nullable: false, maxLength: 150),
                        Gia = c.Decimal(nullable: false, storeType: "money"),
                        MoTa = c.String(nullable: false, storeType: "ntext"),
                        ChatLieu = c.String(nullable: false, maxLength: 50),
                        HuongDan = c.String(nullable: false, storeType: "ntext"),
                        NgayTao = c.DateTime(nullable: false),
                        NguoiTao = c.String(nullable: false, maxLength: 100),
                        NgaySua = c.DateTime(),
                        NguoiSua = c.String(maxLength: 100),
                        HinhAnh = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.MaSP)
                .ForeignKey("dbo.DanhMuc", t => t.MaDM, cascadeDelete: true)
                .Index(t => t.MaDM);
            
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
                .PrimaryKey(t => t.IDCTSP)
                .ForeignKey("dbo.SanPham", t => t.MaSP, cascadeDelete: true)
                .Index(t => t.MaSP);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SanPham", "MaDM", "dbo.DanhMuc");
            DropForeignKey("dbo.sanphamchitiet", "MaSP", "dbo.SanPham");
            DropIndex("dbo.sanphamchitiet", new[] { "MaSP" });
            DropIndex("dbo.SanPham", new[] { "MaDM" });
            DropTable("dbo.sanphamchitiet");
            DropTable("dbo.SanPham");
            DropTable("dbo.DanhMuc");
        }
    }
}
