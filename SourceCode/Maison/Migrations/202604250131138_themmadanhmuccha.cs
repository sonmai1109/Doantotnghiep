namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class themmadanhmuccha : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DanhMuc", "MaDMCha", c => c.Int());
            CreateIndex("dbo.DanhMuc", "MaDMCha");
            AddForeignKey("dbo.DanhMuc", "MaDMCha", "dbo.DanhMuc", "MaDM");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DanhMuc", "MaDMCha", "dbo.DanhMuc");
            DropIndex("dbo.DanhMuc", new[] { "MaDMCha" });
            DropColumn("dbo.DanhMuc", "MaDMCha");
        }
    }
}
