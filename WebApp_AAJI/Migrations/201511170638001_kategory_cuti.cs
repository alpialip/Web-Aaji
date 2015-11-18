namespace WebApp_AAJI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class kategory_cuti : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.levels", "saldoCuti2", c => c.Int(nullable: false));
            AddColumn("dbo.levels", "saldoCuti3", c => c.Int(nullable: false));
            AddColumn("dbo.levels", "saldoCuti4", c => c.Int(nullable: false));
            AddColumn("dbo.sisaSaldoCutis", "countedApproved2", c => c.Int(nullable: false));
            AddColumn("dbo.sisaSaldoCutis", "amount2", c => c.Int(nullable: false));
            AddColumn("dbo.sisaSaldoCutis", "countedApproved3", c => c.Int(nullable: false));
            AddColumn("dbo.sisaSaldoCutis", "amount3", c => c.Int(nullable: false));
            AddColumn("dbo.sisaSaldoCutis", "countedApproved4", c => c.Int(nullable: false));
            AddColumn("dbo.sisaSaldoCutis", "amount4", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.sisaSaldoCutis", "amount4");
            DropColumn("dbo.sisaSaldoCutis", "countedApproved4");
            DropColumn("dbo.sisaSaldoCutis", "amount3");
            DropColumn("dbo.sisaSaldoCutis", "countedApproved3");
            DropColumn("dbo.sisaSaldoCutis", "amount2");
            DropColumn("dbo.sisaSaldoCutis", "countedApproved2");
            DropColumn("dbo.levels", "saldoCuti4");
            DropColumn("dbo.levels", "saldoCuti3");
            DropColumn("dbo.levels", "saldoCuti2");
        }
    }
}
