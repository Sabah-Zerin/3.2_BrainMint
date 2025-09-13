namespace Brain_Mint.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizQuestions", "QuestionType", c => c.String(maxLength: 20));
            AlterColumn("dbo.QuizQuestions", "OptionA", c => c.String(maxLength: 200));
            AlterColumn("dbo.QuizQuestions", "OptionB", c => c.String(maxLength: 200));
            AlterColumn("dbo.QuizQuestions", "OptionC", c => c.String(maxLength: 200));
            AlterColumn("dbo.QuizQuestions", "OptionD", c => c.String(maxLength: 200));
            AlterColumn("dbo.QuizQuestions", "CorrectAnswer", c => c.String(maxLength: 1000));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.QuizQuestions", "CorrectAnswer", c => c.String(nullable: false, maxLength: 1));
            AlterColumn("dbo.QuizQuestions", "OptionD", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.QuizQuestions", "OptionC", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.QuizQuestions", "OptionB", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.QuizQuestions", "OptionA", c => c.String(nullable: false, maxLength: 200));
            DropColumn("dbo.QuizQuestions", "QuestionType");
        }
    }
}
