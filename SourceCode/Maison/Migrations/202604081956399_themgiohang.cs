namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class themgiohang : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GioHang",
                c => new
                    {
                        MaGH = c.Int(nullable: false, identity: true),
                        MaTK = c.Int(nullable: false),
                        MaBT = c.Int(nullable: false),
                        SoLuong = c.Int(nullable: false),
                        NgayThem = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.MaGH)
                .ForeignKey("dbo.BienThe", t => t.MaBT, cascadeDelete: true)
                .ForeignKey("dbo.TaiKhoanNguoiDung", t => t.MaTK, cascadeDelete: true)
                .Index(t => t.MaTK)
                .Index(t => t.MaBT);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GioHang", "MaTK", "dbo.TaiKhoanNguoiDung");
            DropForeignKey("dbo.GioHang", "MaBT", "dbo.BienThe");
            DropIndex("dbo.GioHang", new[] { "MaBT" });
            DropIndex("dbo.GioHang", new[] { "MaTK" });
            DropTable("dbo.GioHang");
        }
    }
}
