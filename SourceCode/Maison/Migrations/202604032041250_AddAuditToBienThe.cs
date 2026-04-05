namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAuditToBienThe : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BienThe", "NgayTao", c => c.DateTime());
            AddColumn("dbo.BienThe", "NguoiTao", c => c.String(maxLength: 50));
            AddColumn("dbo.BienThe", "NgaySua", c => c.DateTime());
            AddColumn("dbo.BienThe", "NguoiSua", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BienThe", "NguoiSua");
            DropColumn("dbo.BienThe", "NgaySua");
            DropColumn("dbo.BienThe", "NguoiTao");
            DropColumn("dbo.BienThe", "NgayTao");
        }
    }
}
