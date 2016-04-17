namespace Memento.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Maketextfieldrequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Cards", "Text", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Cards", "Text", c => c.String());
        }
    }
}
