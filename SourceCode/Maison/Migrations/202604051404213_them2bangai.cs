namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class them2bangai : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChatbotKnowledge",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CauHoi = c.String(nullable: false, maxLength: 500),
                        CauTraLoi = c.String(nullable: false),
                        TuKhoa = c.String(maxLength: 200),
                        MaDM = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DanhMuc", t => t.MaDM)
                .Index(t => t.MaDM);
            
            CreateTable(
                "dbo.ChatbotLog",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MaTK = c.Int(),
                        CauHoiKhach = c.String(nullable: false),
                        BotTraLoi = c.String(),
                        ThoiGian = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TaiKhoanNguoiDung", t => t.MaTK)
                .Index(t => t.MaTK);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatbotLog", "MaTK", "dbo.TaiKhoanNguoiDung");
            DropForeignKey("dbo.ChatbotKnowledge", "MaDM", "dbo.DanhMuc");
            DropIndex("dbo.ChatbotLog", new[] { "MaTK" });
            DropIndex("dbo.ChatbotKnowledge", new[] { "MaDM" });
            DropTable("dbo.ChatbotLog");
            DropTable("dbo.ChatbotKnowledge");
        }
    }
}
