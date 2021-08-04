﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WizLib_DataAccess.Data;
using WizLib_Model.Models;
using WizLib_Model.ViewModels;

namespace WizLib.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _db;

        public BookController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            List<Book> objList = _db.Books.Include(u => u.Publisher)
                                    .Include(u => u.BookAuthors).ThenInclude(u => u.Author).ToList();

            //List<Book> objList = _db.Books.ToList();
            //foreach (var obj in objList)
            //{
            //    Least Efficient
            //    obj.Publisher = _db.Publishers.FirstOrDefault(u => u.Publisher_Id == obj.Publisher_Id);

            //    Explicit loading more efficient
            //    _db.Entry(obj).Reference(u => u.Publisher).Load();
            //    _db.Entry(obj).Collection(u => u.BookAuthors).Load();
            //    foreach (var bookAuth in obj.BookAuthors)
            //    {
            //        _db.Entry(bookAuth).Reference(u => u.Author).Load();
            //    }
            //}
            return View(objList);
        }

        public IActionResult Upsert(int? id)
        {
            BookVM obj = new BookVM();
            obj.PublisherList = _db.Publishers.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Publisher_Id.ToString()
            });

            if (id == null)
            {
                return View(obj);
            }

            //this for edit
            obj.Book = _db.Books.FirstOrDefault(u => u.Book_Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(BookVM obj)
        {
            if (obj.Book.Book_Id == 0)
            {
                // this is create
                _db.Books.Add(obj.Book);
            }
            else
            {
                //this is an update
                _db.Books.Update(obj.Book);
            }
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Details(int? id)
        {
            BookVM obj = new BookVM();

            if (id == null)
            {
                return View(obj);
            }

            //this for edit
            obj.Book = _db.Books.Include(u => u.BookDetail).FirstOrDefault(u => u.Book_Id == id);
            obj.Book.BookDetail = _db.BookDetails.FirstOrDefault(u => u.BookDetail_Id == obj.Book.BookDetail_Id);
            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details(BookVM obj)
        {
            if (obj.Book.BookDetail.BookDetail_Id == 0)
            {
                // this is create
                _db.BookDetails.Add(obj.Book.BookDetail);
                _db.SaveChanges();

                var BookFromDb = _db.Books.FirstOrDefault(u => u.Book_Id == obj.Book.Book_Id);
                BookFromDb.BookDetail_Id = obj.Book.BookDetail.BookDetail_Id;
                _db.SaveChanges();
            }
            else
            {
                //this is an update
                _db.BookDetails.Update(obj.Book.BookDetail);
                _db.SaveChanges();
            }
            
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Delete(int id)
        {
            var objFromDb = _db.Books.FirstOrDefault(u => u.Book_Id == id);
            _db.Books.Remove(objFromDb);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult ManageAuthors(int id)
        {
            BookAuthorVM obj = new BookAuthorVM
            {
                BookAuthorList = _db.BookAuthors.Include(u => u.Author).Include(u => u.Book)
                                    .Where(u => u.Book_Id == id).ToList(),
                BookAuthor = new BookAuthor()
                {
                    Book_Id = id
                },
                Book = _db.Books.FirstOrDefault(u => u.Book_Id == id)
            };
            List<int> tempListOfAssignedAuthors = obj.BookAuthorList.Select(u => u.Author_Id).ToList();
            //NOT IN Clause in LING
            //Get all the authros whos id is not in tempListOfAssignedAuthors
            var tempList = _db.Authors.Where(u => !tempListOfAssignedAuthors.Contains(u.Author_Id)).ToList();

            obj.AuthorList = tempList.Select(i => new SelectListItem
            {
                Text = i.FullName,
                Value = i.Author_Id.ToString()
            }); ;
     
            return View(obj);
        }


        [HttpPost]
        public IActionResult ManageAuthors(BookAuthorVM bookAuthorVM)
        {
            if (bookAuthorVM.BookAuthor.Book_Id != 0 && bookAuthorVM.BookAuthor.Author_Id != 0)
            {
                _db.BookAuthors.Add(bookAuthorVM.BookAuthor);
                _db.SaveChanges();
            }
            return RedirectToAction(nameof(ManageAuthors), new { @id = bookAuthorVM.BookAuthor.Book_Id});
        }


        [HttpPost]
        public IActionResult RemoveAuthors(int authorId, BookAuthorVM bookAuthorVM)
        {
            int bookId = bookAuthorVM.Book.Book_Id;
            BookAuthor bookAuthor = _db.BookAuthors.FirstOrDefault(
                u => u.Author_Id == authorId && u.Book_Id == bookId);
            _db.BookAuthors.Remove(bookAuthor);
            _db.SaveChanges();
            return RedirectToAction(nameof(ManageAuthors), new { @id = bookId });
        }

        //public IActionResult PlayGround()
        //{
        //    var bookTemp = _db.Books.FirstOrDefault();
        //    bookTemp.Price = 100;

        //    var bookCollection = _db.Books;
        //    double totalPrice = 0;

        //    foreach (var book in bookCollection)
        //    {
        //        totalPrice += book.Price;
        //    }

        //    var bookList = _db.Books.ToList();
        //    foreach (var book in bookList)
        //    {
        //        totalPrice += book.Price;
        //    }

        //    var bookCollection2 = _db.Books;
        //    var bookCount1 = bookCollection2.Count();

        //    var bookCount2 = _db.Books.Count();

        //    IEnumerable<Book> BookList1 = _db.Books;
        //    var FilteredBook1 = BookList1.Where(b => b.Price > 500).ToList();

        //    IQueryable<Book> BookList2 = _db.Books;
        //    var fileredBook2 = BookList2.Where(b => b.Price > 500).ToList();


        //    //updating Related Data
        //    var bookTemp1 = _db.Books.Include(b => b.BookDetail).FirstOrDefault(b => b.Book_Id == 2);
        //    bookTemp1.BookDetail.NumberOfChapters = 222;
        //    _db.Books.Update(bookTemp1);
        //    _db.SaveChanges();


        //    var bookTemp2 = _db.Books.Include(b => b.BookDetail).FirstOrDefault(b => b.Book_Id == 4);
        //    bookTemp2.BookDetail.Weight = 333;
        //    _db.Books.Attach(bookTemp2);
        //    _db.SaveChanges();


        //    return RedirectToAction(nameof(Index));
        //}

    }

}
