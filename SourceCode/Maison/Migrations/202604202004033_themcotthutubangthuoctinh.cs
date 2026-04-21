namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class themcotthutubangthuoctinh : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ThuocTinh", "ThuTuHienThi", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ThuocTinh", "ThuTuHienThi");
        }
    }
}
