namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addtkqt : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TaiKhoanQuanTri",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TenDangNhap = c.String(nullable: false, maxLength: 100),
                        MatKhau = c.String(nullable: false, maxLength: 50),
                        LoaiTaiKhoan = c.Boolean(nullable: false),
                        HoTen = c.String(nullable: false, maxLength: 100),
                        TrangThai = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TaiKhoanQuanTri");
        }
    }
}
