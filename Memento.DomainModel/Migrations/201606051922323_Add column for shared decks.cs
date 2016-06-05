namespace Memento.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addcolumnforshareddecks : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Decks", "IsShared", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Decks", "IsShared");
        }
    }
}
