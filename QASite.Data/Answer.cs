using System;
using System.Collections.Generic;
using System.Text;

namespace QASite.Data
{
    public class Answer
    {
        public int Id { get; set; }
        public string AnswerText { get; set; }
        public Question Question { get; set; }
        public int QuestionId { get; set; }
        public DateTime DateAnswered { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
    }
}
