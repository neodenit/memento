namespace Memento.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Removeclozescount : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Answers", "CardsInRepetition");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Answers", "CardsInRepetition", c => c.Int(nullable: false));
        }
    }
}
