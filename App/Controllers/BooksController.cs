using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace App.Controllers
{
    public class Book
    {
        public int Id { get; set; }    
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime PublishedDate { get; set; }
    }

    public static class BookRepo
    {
        public static List<Book> _books = new List<Book>();

        static BookRepo()
        {
            _books.Add(new Book
            {
                Id = 1,
                Author = "Mario",
                PublishedDate = DateTime.Now,
                Title = ".NET"
            });

            _books.Add(new Book
            {
                Id = 2,
                Author = "Geovanni",
                PublishedDate = DateTime.Now,
                Title = "Angular.JS"
            });
        }

        public static IQueryable<Book> GetAll()
        {
            return _books.AsQueryable();
        }
    }
    
    public class BooksController : ApiController
    {
        [HttpGet]
        public IHttpActionResult All()
        {
            return Ok(BookRepo.GetAll());
        }

        [HttpGet]
        public IHttpActionResult Single(int id)
        {
            var current = BookRepo.GetAll()
                .FirstOrDefault(x => x.Id == id);

            if (current == null)
                return NotFound();

            return Ok(current);
        }
    }
}