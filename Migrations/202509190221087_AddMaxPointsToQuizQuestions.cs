namespace Brain_Mint.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMaxPointsToQuizQuestions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizAttempts", "Quiz_Id", c => c.Int());
            AddColumn("dbo.QuizQuestions", "MaxPoints", c => c.Int(nullable: false));
            CreateIndex("dbo.QuizAttempts", "Quiz_Id");
            AddForeignKey("dbo.QuizAttempts", "Quiz_Id", "dbo.Quizzes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuizAttempts", "Quiz_Id", "dbo.Quizzes");
            DropIndex("dbo.QuizAttempts", new[] { "Quiz_Id" });
            DropColumn("dbo.QuizQuestions", "MaxPoints");
            DropColumn("dbo.QuizAttempts", "Quiz_Id");
        }
    }
}
