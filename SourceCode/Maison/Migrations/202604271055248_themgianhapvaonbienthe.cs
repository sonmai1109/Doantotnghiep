namespace Maison.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class themgianhapvaonbienthe : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BienThe", "GiaNhap", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BienThe", "GiaNhap");
        }
    }
}
