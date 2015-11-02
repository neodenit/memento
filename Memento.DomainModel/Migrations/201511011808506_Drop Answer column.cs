namespace Memento.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DropAnswercolumn : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Cards", "Answer");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Cards", "Answer", c => c.String(nullable: false));
        }
    }
}
