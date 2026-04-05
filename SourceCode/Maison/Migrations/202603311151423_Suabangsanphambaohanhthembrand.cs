namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Suabangsanphambaohanhthembrand : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Baohanhs", newName: "Baohanh");
            CreateTable(
                "dbo.Brand",
                c => new
                    {
                        MaBrand = c.Int(nullable: false, identity: true),
                        TenBrand = c.String(nullable: false, maxLength: 100),
                        Logo = c.String(maxLength: 255),
                        MoTa = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.MaBrand);
            
            AddColumn("dbo.SanPham", "MaBrand", c => c.Int(nullable: false));
            AddColumn("dbo.SanPham", "ThoiHanBaoHanh", c => c.Int(nullable: false));
            AlterColumn("dbo.SanPham", "MoTa", c => c.String(storeType: "ntext"));
            AlterColumn("dbo.SanPham", "NguoiTao", c => c.String());
            AlterColumn("dbo.SanPham", "NguoiSua", c => c.String());
            AlterColumn("dbo.SanPham", "HinhAnh", c => c.String(maxLength: 255));
            CreateIndex("dbo.SanPham", "MaBrand");
            AddForeignKey("dbo.SanPham", "MaBrand", "dbo.Brand", "MaBrand", cascadeDelete: true);
            DropColumn("dbo.SanPham", "Gia");
            DropColumn("dbo.SanPham", "ChatLieu");
            DropColumn("dbo.SanPham", "HuongDan");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SanPham", "HuongDan", c => c.String(nullable: false, storeType: "ntext"));
            AddColumn("dbo.SanPham", "ChatLieu", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.SanPham", "Gia", c => c.Decimal(nullable: false, storeType: "money"));
            DropForeignKey("dbo.SanPham", "MaBrand", "dbo.Brand");
            DropIndex("dbo.SanPham", new[] { "MaBrand" });
            AlterColumn("dbo.SanPham", "HinhAnh", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.SanPham", "NguoiSua", c => c.String(maxLength: 100));
            AlterColumn("dbo.SanPham", "NguoiTao", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.SanPham", "MoTa", c => c.String(nullable: false, storeType: "ntext"));
            DropColumn("dbo.SanPham", "ThoiHanBaoHanh");
            DropColumn("dbo.SanPham", "MaBrand");
            DropTable("dbo.Brand");
            RenameTable(name: "dbo.Baohanh", newName: "Baohanhs");
        }
    }
}
