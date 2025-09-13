namespace Brain_Mint.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddQuizAttemptTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuizAttempts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        StudentId = c.Int(nullable: false),
                        StartTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(),
                        Status = c.String(nullable: false, maxLength: 20),
                        TotalScore = c.Int(),
                        MaximumScore = c.Int(),
                        PercentageScore = c.Decimal(precision: 18, scale: 2),
                        GradingStatus = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quizzes", t => t.QuizId)
                .ForeignKey("dbo.Users", t => t.StudentId)
                .Index(t => t.QuizId)
                .Index(t => t.StudentId);
            
            CreateTable(
                "dbo.QuizResponses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizAttemptId = c.Int(nullable: false),
                        QuestionId = c.Int(nullable: false),
                        StudentAnswer = c.String(maxLength: 1000),
                        IsCorrect = c.Boolean(),
                        PointsAwarded = c.Int(),
                        TeacherFeedback = c.String(maxLength: 500),
                        ResponseTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.QuizQuestions", t => t.QuestionId)
                .ForeignKey("dbo.QuizAttempts", t => t.QuizAttemptId, cascadeDelete: true)
                .Index(t => t.QuizAttemptId)
                .Index(t => t.QuestionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuizAttempts", "StudentId", "dbo.Users");
            DropForeignKey("dbo.QuizResponses", "QuizAttemptId", "dbo.QuizAttempts");
            DropForeignKey("dbo.QuizResponses", "QuestionId", "dbo.QuizQuestions");
            DropForeignKey("dbo.QuizAttempts", "QuizId", "dbo.Quizzes");
            DropIndex("dbo.QuizResponses", new[] { "QuestionId" });
            DropIndex("dbo.QuizResponses", new[] { "QuizAttemptId" });
            DropIndex("dbo.QuizAttempts", new[] { "StudentId" });
            DropIndex("dbo.QuizAttempts", new[] { "QuizId" });
            DropTable("dbo.QuizResponses");
            DropTable("dbo.QuizAttempts");
        }
    }
}
