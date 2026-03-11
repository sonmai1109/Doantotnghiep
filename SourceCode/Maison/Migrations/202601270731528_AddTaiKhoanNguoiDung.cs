namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTaiKhoanNguoiDung : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TaiKhoanNguoiDung",
                c => new
                    {
                        MaTk = c.Int(nullable: false, identity: true),
                        TenDangNhap = c.String(nullable: false, maxLength: 100),
                        MatKhau = c.String(nullable: false, maxLength: 50),
                        HoTen = c.String(nullable: false, maxLength: 100),
                        SoDienThoai = c.String(nullable: false, maxLength: 11),
                        DiaChi = c.String(nullable: false, maxLength: 100),
                        NgaySinh = c.DateTime(nullable: false),
                        Email = c.String(nullable: false, maxLength: 100),
                        GioiTinh = c.Boolean(nullable: false),
                        TrangThai = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.MaTk);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TaiKhoanNguoiDung");
        }
    }
}
