namespace Brain_Mint.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLoginLog : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LoginLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 50),
                        UserRole = c.String(nullable: false, maxLength: 20),
                        LoginTime = c.DateTime(nullable: false),
                        IpAddress = c.String(maxLength: 45),
                        UserAgent = c.String(maxLength: 500),
                        LogoutTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LoginLogs", "UserId", "dbo.Users");
            DropIndex("dbo.LoginLogs", new[] { "UserId" });
            DropTable("dbo.LoginLogs");
        }
    }
}
