namespace Openbound_Network_Object_Library.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitalCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Guild",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Tag = c.String(nullable: false, maxLength: 6),
                        Name = c.String(nullable: false, maxLength: 20),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Tag, unique: true);
            
            CreateTable(
                "dbo.Player",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nickname = c.String(nullable: false, maxLength: 30),
                        Password = c.String(nullable: false, maxLength: 172),
                        Email = c.String(nullable: false, maxLength: 60),
                        CharacterGender = c.Int(nullable: false),
                        PrimaryMobile = c.Int(nullable: false),
                        SecondaryMobile = c.Int(nullable: false),
                        LeavePercentage = c.Single(nullable: false),
                        Guild_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Guild", t => t.Guild_Id)
                .Index(t => t.Nickname, unique: true)
                .Index(t => t.Email, unique: true)
                .Index(t => t.Guild_Id);
            
            CreateTable(
                "dbo.FriendList",
                c => new
                    {
                        Player_Id = c.Int(nullable: false),
                        Player_Id1 = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Player_Id, t.Player_Id1 })
                .ForeignKey("dbo.Player", t => t.Player_Id)
                .ForeignKey("dbo.Player", t => t.Player_Id1)
                .Index(t => t.Player_Id)
                .Index(t => t.Player_Id1);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Player", "Guild_Id", "dbo.Guild");
            DropForeignKey("dbo.FriendList", "Player_Id1", "dbo.Player");
            DropForeignKey("dbo.FriendList", "Player_Id", "dbo.Player");
            DropIndex("dbo.FriendList", new[] { "Player_Id1" });
            DropIndex("dbo.FriendList", new[] { "Player_Id" });
            DropIndex("dbo.Player", new[] { "Guild_Id" });
            DropIndex("dbo.Player", new[] { "Email" });
            DropIndex("dbo.Player", new[] { "Nickname" });
            DropIndex("dbo.Guild", new[] { "Tag" });
            DropTable("dbo.FriendList");
            DropTable("dbo.Player");
            DropTable("dbo.Guild");
        }
    }
}
