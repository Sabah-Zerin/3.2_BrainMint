using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Brain_Mint.Models
{
    public class BrainMintDbContext : DbContext
    {
        public BrainMintDbContext() : base("name=BrainMintDB")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<LoginLog> LoginLogs { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<QuizResponse> QuizResponses { get; set; }

        // Assignment Tables
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Quiz relationships
            modelBuilder.Entity<Quiz>()
                .HasRequired(q => q.CreatedBy)
                .WithMany()
                .HasForeignKey(q => q.CreatedByUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<QuizQuestion>()
                .HasRequired(qq => qq.Quiz)
                .WithMany(q => q.Questions)
                .HasForeignKey(qq => qq.QuizId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<QuizAttempt>()
                .HasRequired(qa => qa.Quiz)
                .WithMany()
                .HasForeignKey(qa => qa.QuizId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<QuizAttempt>()
                .HasRequired(qa => qa.Student)
                .WithMany()
                .HasForeignKey(qa => qa.StudentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<QuizResponse>()
                .HasRequired(qr => qr.QuizAttempt)
                .WithMany(qa => qa.Responses)
                .HasForeignKey(qr => qr.QuizAttemptId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<QuizResponse>()
                .HasRequired(qr => qr.Question)
                .WithMany()
                .HasForeignKey(qr => qr.QuestionId)
                .WillCascadeOnDelete(false);

            // Assignment relationships
            modelBuilder.Entity<Assignment>()
                .HasRequired(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedByUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AssignmentSubmission>()
                .HasRequired(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<AssignmentSubmission>()
                .HasRequired(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AssignmentSubmission>()
                .HasOptional(s => s.GradedBy)
                .WithMany()
                .HasForeignKey(s => s.GradedByUserId)
                .WillCascadeOnDelete(false);
        }
    }
}