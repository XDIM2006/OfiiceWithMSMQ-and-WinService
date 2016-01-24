namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Images", "PreviewPath", c => c.String());
            AddColumn("dbo.ResizedImages", "PreviewPath", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ResizedImages", "PreviewPath");
            DropColumn("dbo.Images", "PreviewPath");
        }
    }
}
