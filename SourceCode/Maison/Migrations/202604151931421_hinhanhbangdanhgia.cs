namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hinhanhbangdanhgia : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DanhGia", "HinhAnh", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DanhGia", "HinhAnh");
        }
    }
}
