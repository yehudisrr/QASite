using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace QASite.Data
{
    public class QuestionsRepository
    {
        private readonly string _connectionString;

        public QuestionsRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public void AddUser(User user, string password)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(password);
            using var ctx = new QAContext(_connectionString);
            user.PasswordHash = hash;
            ctx.Users.Add(user);
            ctx.SaveChanges();
        }

    
        public User Login(string email, string password)
        {
            var user = GetByEmail(email);
            if (user == null)
            {
                return null;
            }

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (isValidPassword)
            {
                return user; 
            }

            return null;
        }

        public User GetByEmail(string email)
        {
            using var ctx = new QAContext(_connectionString);
            return ctx.Users.Include(u => u.Likes).FirstOrDefault(u => u.Email == email);
        }

        public bool IsEmailAvailable(string email)
        {
            using var ctx = new QAContext(_connectionString);
            return !ctx.Users.Any(u => u.Email == email);
        }
        private Tag GetTag(string name)
        {
            using var ctx = new QAContext(_connectionString);
            return ctx.Tags.FirstOrDefault(t => t.Name == name);
        }

        private int AddTag(string name)
        {
            using var ctx = new QAContext(_connectionString);
            var tag = new Tag { Name = name };
            ctx.Tags.Add(tag);
            ctx.SaveChanges();
            return tag.Id;
        }

        public List<Question> GetAllQuestions()
        {
            using var ctx = new QAContext(_connectionString);
            return ctx.Questions.OrderByDescending(q => q.DatePosted).Include(q => q.Likes)
                .Include(q => q.Answers)
                .Include(q => q.QuestionsTags)
                .ThenInclude(qt => qt.Tag)
                .ToList();
        }

        public List<Question> GetQuestionsForTag(string name)
        {
            using var ctx = new QAContext(_connectionString);
            return ctx.Questions.Include(q => q.QuestionsTags)
                .ThenInclude(qt => qt.Tag)
                .Where(c => c.QuestionsTags.Any(t => t.Tag.Name == name))
                .ToList();
        }

        public Question GetQuestionById(int id)
        {
            using var ctx = new QAContext(_connectionString);
            return ctx.Questions.Include(q => q.Answers).Include(q => q.Likes)
                .ThenInclude(a => a.User)
                .Include(q => q.QuestionsTags)
                .ThenInclude(qt => qt.Tag)
                .Include(q => q.User)
                .FirstOrDefault(q => q.Id == id);
        }

        public void AddQuestion(Question question, IEnumerable<string> tags)
        {
            using var ctx = new QAContext(_connectionString);
            ctx.Questions.Add(question);
            ctx.SaveChanges();
            foreach (string tag in tags)
            {
                Tag t = GetTag(tag);
                int tagId;
                if (t == null)
                {
                    tagId = AddTag(tag);
                }
                else
                {
                    tagId = t.Id;
                }
                ctx.QuestionsTags.Add(new QuestionsTags
                {
                    QuestionId = question.Id,
                    TagId = tagId
                });
            }

            ctx.SaveChanges();
        }
        public void AddAnswer(Answer answer)
        {
            using var ctx = new QAContext(_connectionString);
            ctx.Answers.Add(answer);
            ctx.SaveChanges();
        }
        public void AddLike(int questionId, int userId)
        {
            using var context = new QAContext(_connectionString);
            var like = new Like
            {
                QuestionId = questionId,
                UserId = userId
            };
            context.Likes.Add(like);
            context.SaveChanges();
        }

        public int GetLikes(int id)
        {
            using var context = new QAContext(_connectionString);
            return context.Likes.Where(l => l.QuestionId == id).Count();
        }
    }
}