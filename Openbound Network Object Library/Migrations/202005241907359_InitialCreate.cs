namespace Openbound_Network_Object_Library.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Guild",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Tag = c.String(nullable: false, maxLength: 6),
                        Name = c.String(nullable: false, maxLength: 20),
                    })
                .PrimaryKey(t => t.ID)
                .Index(t => t.Tag, unique: true);
            
            CreateTable(
                "dbo.Player",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Nickname = c.String(nullable: false, maxLength: 30),
                        Password = c.String(nullable: false, maxLength: 172),
                        Email = c.String(nullable: false, maxLength: 60),
                        CharacterGender = c.Int(nullable: false),
                        PrimaryMobile = c.Int(nullable: false),
                        SecondaryMobile = c.Int(nullable: false),
                        LeavePercentage = c.Single(nullable: false),
                        Player_ID = c.Int(),
                        Guild_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Player", t => t.Player_ID)
                .ForeignKey("dbo.Guild", t => t.Guild_ID)
                .Index(t => t.Nickname, unique: true)
                .Index(t => t.Email, unique: true)
                .Index(t => t.Player_ID)
                .Index(t => t.Guild_ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Player", "Guild_ID", "dbo.Guild");
            DropForeignKey("dbo.Player", "Player_ID", "dbo.Player");
            DropIndex("dbo.Player", new[] { "Guild_ID" });
            DropIndex("dbo.Player", new[] { "Player_ID" });
            DropIndex("dbo.Player", new[] { "Email" });
            DropIndex("dbo.Player", new[] { "Nickname" });
            DropIndex("dbo.Guild", new[] { "Tag" });
            DropTable("dbo.Player");
            DropTable("dbo.Guild");
        }
    }
}
