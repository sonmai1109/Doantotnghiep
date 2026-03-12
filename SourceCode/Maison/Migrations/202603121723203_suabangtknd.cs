namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class suabangtknd : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TaiKhoanNguoiDung", "GioiTinh", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TaiKhoanNguoiDung", "GioiTinh", c => c.Boolean(nullable: false));
        }
    }
}
