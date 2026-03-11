namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveNguoiTaoNguoiSuaFromDanhMuc : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.DanhMuc", "NguoiTao");
            DropColumn("dbo.DanhMuc", "NguoiSua");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DanhMuc", "NguoiSua", c => c.String(maxLength: 100));
            AddColumn("dbo.DanhMuc", "NguoiTao", c => c.String(nullable: false, maxLength: 100));
        }
    }
}
