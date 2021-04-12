using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QASite.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using QASite.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace QASite.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connectionString;
        public HomeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ConStr");
        }
        public IActionResult Index()
        {
            var repo = new QuestionsRepository(_connectionString);
            var questions = repo.GetAllQuestions();

            QuestionsViewModel vm = new QuestionsViewModel
            {
                Questions = questions
            };

            return View(vm);
         }
        public IActionResult ViewQuestion(int id)
        {
            var repo = new QuestionsRepository(_connectionString);
            var question = repo.GetQuestionById(id);
            
            var email = User.Identity.Name;
            var user = repo.GetByEmail(email);
         
            QuestionViewModel vm = new QuestionViewModel();
            vm.Question = question;
            if (user != null && user.Likes != null)
            {
                vm.Liked = user.Likes.Any(l => l.QuestionId == id);
            }
      
            return View(vm);
        }
        [Authorize]
        public IActionResult AskAQuestion()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(Question question, IEnumerable<string> tags)
        {
            var repo = new QuestionsRepository(_connectionString);
            question.DatePosted = DateTime.Now;
            var email = User.Identity.Name;
            question.UserId = repo.GetByEmail(email).Id;
            repo.AddQuestion(question, tags);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult AddAnswer(Answer answer)
        {
            var repo = new QuestionsRepository(_connectionString);
            answer.DateAnswered = DateTime.Now;
            var email = User.Identity.Name;
            answer.UserId = repo.GetByEmail(email).Id;
            repo.AddAnswer(answer);
            return Redirect($"/home/ViewQuestion?id={answer.QuestionId}");
        }

        [HttpPost]
        public void Like(int id)
        {
            var repo = new QuestionsRepository(_connectionString);
            var email = User.Identity.Name;
            var userId = repo.GetByEmail(email).Id;
            repo.AddLike(id, userId);
        }

        public IActionResult GetLikes(int id)
        {
            var repo = new QuestionsRepository(_connectionString);
            return Json(repo.GetLikes(id));
        }
    }
 
}
