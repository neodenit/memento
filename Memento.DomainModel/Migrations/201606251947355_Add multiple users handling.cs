namespace Memento.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addmultipleusershandling : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserRepetitions",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                        ClozeID = c.Int(nullable: false),
                        Position = c.Int(nullable: false),
                        IsNew = c.Boolean(nullable: false),
                        LastDelay = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Clozes", t => t.ClozeID, cascadeDelete: true)
                .Index(t => t.ClozeID);
            
            DropColumn("dbo.Clozes", "Position");
            DropColumn("dbo.Clozes", "IsNew");
            DropColumn("dbo.Clozes", "LastDelay");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Clozes", "LastDelay", c => c.Int(nullable: false));
            AddColumn("dbo.Clozes", "IsNew", c => c.Boolean(nullable: false));
            AddColumn("dbo.Clozes", "Position", c => c.Int(nullable: false));
            DropForeignKey("dbo.UserRepetitions", "ClozeID", "dbo.Clozes");
            DropIndex("dbo.UserRepetitions", new[] { "ClozeID" });
            DropTable("dbo.UserRepetitions");
        }
    }
}
