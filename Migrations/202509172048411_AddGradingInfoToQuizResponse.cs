namespace Brain_Mint.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGradingInfoToQuizResponse : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Assignments", "CreatedByUserId", "dbo.Users");
            AddColumn("dbo.QuizResponses", "GradedDate", c => c.DateTime());
            AddColumn("dbo.QuizResponses", "GradedByUserId", c => c.Int());
            AlterColumn("dbo.QuizResponses", "StudentAnswer", c => c.String());
            AlterColumn("dbo.QuizResponses", "TeacherFeedback", c => c.String());
            CreateIndex("dbo.QuizResponses", "GradedByUserId");
            AddForeignKey("dbo.QuizResponses", "GradedByUserId", "dbo.Users", "Id");
            AddForeignKey("dbo.Assignments", "CreatedByUserId", "dbo.Users", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Assignments", "CreatedByUserId", "dbo.Users");
            DropForeignKey("dbo.QuizResponses", "GradedByUserId", "dbo.Users");
            DropIndex("dbo.QuizResponses", new[] { "GradedByUserId" });
            AlterColumn("dbo.QuizResponses", "TeacherFeedback", c => c.String(maxLength: 500));
            AlterColumn("dbo.QuizResponses", "StudentAnswer", c => c.String(maxLength: 1000));
            DropColumn("dbo.QuizResponses", "GradedByUserId");
            DropColumn("dbo.QuizResponses", "GradedDate");
            AddForeignKey("dbo.Assignments", "CreatedByUserId", "dbo.Users", "Id");
        }
    }
}
