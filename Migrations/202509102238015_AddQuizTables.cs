namespace Brain_Mint.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddQuizTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuizQuestions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        QuestionText = c.String(nullable: false, maxLength: 500),
                        OptionA = c.String(nullable: false, maxLength: 200),
                        OptionB = c.String(nullable: false, maxLength: 200),
                        OptionC = c.String(nullable: false, maxLength: 200),
                        OptionD = c.String(nullable: false, maxLength: 200),
                        CorrectAnswer = c.String(nullable: false, maxLength: 1),
                        QuestionOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quizzes", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
            CreateTable(
                "dbo.Quizzes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 100),
                        Category = c.String(nullable: false, maxLength: 50),
                        Description = c.String(maxLength: 500),
                        Difficulty = c.String(nullable: false, maxLength: 20),
                        TimeLimit = c.Int(nullable: false),
                        CreatedByUserId = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        Status = c.String(nullable: false, maxLength: 20),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.CreatedByUserId)
                .Index(t => t.CreatedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuizQuestions", "QuizId", "dbo.Quizzes");
            DropForeignKey("dbo.Quizzes", "CreatedByUserId", "dbo.Users");
            DropIndex("dbo.Quizzes", new[] { "CreatedByUserId" });
            DropIndex("dbo.QuizQuestions", new[] { "QuizId" });
            DropTable("dbo.Quizzes");
            DropTable("dbo.QuizQuestions");
        }
    }
}
