namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class bangthuvienanh : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ThuVienAnh",
                c => new
                    {
                        MaAnh = c.Int(nullable: false, identity: true),
                        MaBT = c.Int(nullable: false),
                        DuongDanAnh = c.String(nullable: false, maxLength: 500),
                    })
                .PrimaryKey(t => t.MaAnh)
                .ForeignKey("dbo.BienThe", t => t.MaBT, cascadeDelete: true)
                .Index(t => t.MaBT);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ThuVienAnh", "MaBT", "dbo.BienThe");
            DropIndex("dbo.ThuVienAnh", new[] { "MaBT" });
            DropTable("dbo.ThuVienAnh");
        }
    }
}
