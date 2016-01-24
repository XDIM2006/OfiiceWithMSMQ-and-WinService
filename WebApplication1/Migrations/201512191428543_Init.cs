namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Images",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FilePath = c.String(),
                        FileName = c.String(),
                        Width = c.Int(nullable: false),
                        Height = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ResizedImages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FilePath = c.String(),
                        FileName = c.String(),
                        Width = c.Int(nullable: false),
                        Height = c.Int(nullable: false),
                        ParentId = c.Int(nullable: false),
                        StartTime = c.DateTime(),
                        FinishTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id).ForeignKey("dbo.Images", t => t.ParentId).Index(t => t.ParentId); ;
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ResizedImages");
            DropTable("dbo.Images");
        }
    }
}
