namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class themcotthanhtoanvaohoadon : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.HoaDon", "PhuongThucThanhToan", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.HoaDon", "TrangThaiThanhToan", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.HoaDon", "TrangThaiThanhToan");
            DropColumn("dbo.HoaDon", "PhuongThucThanhToan");
        }
    }
}
