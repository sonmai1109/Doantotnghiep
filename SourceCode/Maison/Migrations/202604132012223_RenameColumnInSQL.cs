namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameColumnInSQL : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.TaiKhoanNguoiDung", "MaTk", "MaTK");
        }
        
        public override void Down()
        {
        }
    }
}
