namespace Memento.DomainModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addcommentstocards : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Cards", "Comment", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Cards", "Comment");
        }
    }
}
