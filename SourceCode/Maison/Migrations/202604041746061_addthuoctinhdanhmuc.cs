namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addthuoctinhdanhmuc : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DanhMuc", "NguoiSua", c => c.String(maxLength: 50));
            AddColumn("dbo.DanhMuc", "NguoiTao", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DanhMuc", "NguoiTao");
            DropColumn("dbo.DanhMuc", "NguoiSua");
        }
    }
}
