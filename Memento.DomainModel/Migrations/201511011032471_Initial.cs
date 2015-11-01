namespace Memento.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Answers",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Time = c.DateTime(nullable: false),
                        Owner = c.String(),
                        IsCorrect = c.Boolean(nullable: false),
                        ClozeID = c.Int(nullable: false),
                        CardID = c.Int(nullable: false),
                        DeckID = c.Int(nullable: false),
                        CardsInRepetition = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Cards",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        DeckID = c.Int(nullable: false),
                        Text = c.String(nullable: false),
                        Answer = c.String(nullable: false),
                        IsValid = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Decks", t => t.DeckID, cascadeDelete: true)
                .Index(t => t.DeckID);
            
            CreateTable(
                "dbo.Clozes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CardID = c.Int(nullable: false),
                        Label = c.String(),
                        Position = c.Int(nullable: false),
                        IsNew = c.Boolean(nullable: false),
                        LastDelay = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Cards", t => t.CardID, cascadeDelete: true)
                .Index(t => t.CardID);
            
            CreateTable(
                "dbo.Decks",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        Owner = c.String(),
                        ControlMode = c.Int(nullable: false),
                        DelayMode = c.Int(nullable: false),
                        AllowSmallDelays = c.Boolean(nullable: false),
                        StartDelay = c.Int(nullable: false),
                        Coeff = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Cards", "DeckID", "dbo.Decks");
            DropForeignKey("dbo.Clozes", "CardID", "dbo.Cards");
            DropIndex("dbo.Clozes", new[] { "CardID" });
            DropIndex("dbo.Cards", new[] { "DeckID" });
            DropTable("dbo.Decks");
            DropTable("dbo.Clozes");
            DropTable("dbo.Cards");
            DropTable("dbo.Answers");
        }
    }
}
