namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addthutubangthuvienanh : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ThuVienAnh", "ThuTu", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ThuVienAnh", "ThuTu");
        }
    }
}
