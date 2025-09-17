namespace Brain_Mint.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddAssignmentTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Assignments",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Title = c.String(nullable: false, maxLength: 200),
                    Description = c.String(nullable: false),
                    Subject = c.String(nullable: false, maxLength: 100),
                    DueDate = c.DateTime(nullable: false),
                    CreatedDate = c.DateTime(nullable: false),
                    CreatedByUserId = c.Int(nullable: false),
                    Status = c.String(nullable: false, maxLength: 20),
                    MaxPoints = c.Int(nullable: false),
                    SubmissionType = c.String(maxLength: 20),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.CreatedByUserId)
                .Index(t => t.CreatedByUserId);

            CreateTable(
                "dbo.AssignmentSubmissions",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    AssignmentId = c.Int(nullable: false),
                    StudentId = c.Int(nullable: false),
                    TextSubmission = c.String(),
                    ImagePath = c.String(maxLength: 500),
                    OriginalFileName = c.String(maxLength: 200),
                    SubmissionDate = c.DateTime(nullable: false),
                    LastModified = c.DateTime(),
                    Status = c.String(maxLength: 20),
                    Points = c.Int(),
                    TeacherFeedback = c.String(),
                    GradedDate = c.DateTime(),
                    GradedByUserId = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Assignments", t => t.AssignmentId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.GradedByUserId)
                .ForeignKey("dbo.Users", t => t.StudentId)
                .Index(t => t.AssignmentId)
                .Index(t => t.StudentId)
                .Index(t => t.GradedByUserId);

        }

        public override void Down()
        {
            DropForeignKey("dbo.AssignmentSubmissions", "StudentId", "dbo.Users");
            DropForeignKey("dbo.AssignmentSubmissions", "GradedByUserId", "dbo.Users");
            DropForeignKey("dbo.AssignmentSubmissions", "AssignmentId", "dbo.Assignments");
            DropForeignKey("dbo.Assignments", "CreatedByUserId", "dbo.Users");
            DropIndex("dbo.AssignmentSubmissions", new[] { "GradedByUserId" });
            DropIndex("dbo.AssignmentSubmissions", new[] { "StudentId" });
            DropIndex("dbo.AssignmentSubmissions", new[] { "AssignmentId" });
            DropIndex("dbo.Assignments", new[] { "CreatedByUserId" });
            DropTable("dbo.AssignmentSubmissions");
            DropTable("dbo.Assignments");
        }
    }
}