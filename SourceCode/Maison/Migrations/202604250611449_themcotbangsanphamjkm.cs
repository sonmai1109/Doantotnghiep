namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class themcotbangsanphamjkm : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SanPhamKhuyenMai", "MaKM", "dbo.KhuyenMai");
            DropIndex("dbo.SanPhamKhuyenMai", new[] { "MaKM" });
            AddColumn("dbo.SanPhamKhuyenMai", "SoLuongKhuyenMai", c => c.Int());
            AddColumn("dbo.SanPhamKhuyenMai", "SoLuongDaBan", c => c.Int(nullable: false));
            AlterColumn("dbo.SanPhamKhuyenMai", "MaKM", c => c.Int());
            CreateIndex("dbo.SanPhamKhuyenMai", "MaKM");
            AddForeignKey("dbo.SanPhamKhuyenMai", "MaKM", "dbo.KhuyenMai", "MaKM");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SanPhamKhuyenMai", "MaKM", "dbo.KhuyenMai");
            DropIndex("dbo.SanPhamKhuyenMai", new[] { "MaKM" });
            AlterColumn("dbo.SanPhamKhuyenMai", "MaKM", c => c.Int(nullable: false));
            DropColumn("dbo.SanPhamKhuyenMai", "SoLuongDaBan");
            DropColumn("dbo.SanPhamKhuyenMai", "SoLuongKhuyenMai");
            CreateIndex("dbo.SanPhamKhuyenMai", "MaKM");
            AddForeignKey("dbo.SanPhamKhuyenMai", "MaKM", "dbo.KhuyenMai", "MaKM", cascadeDelete: true);
        }
    }
}
