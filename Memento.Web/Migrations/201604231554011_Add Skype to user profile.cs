namespace Memento.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSkypetouserprofile : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Skype", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Skype");
        }
    }
}
