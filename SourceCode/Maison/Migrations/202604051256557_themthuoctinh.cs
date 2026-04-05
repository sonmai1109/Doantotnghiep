namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class themthuoctinh : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ThuocTinh", "LaThuocTinhChinh", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ThuocTinh", "LaThuocTinhChinh");
        }
    }
}
