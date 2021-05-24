﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookAuthors.Data;
using BookAuthors.Models;
using BookAuthors.ViewModels;
using DynamicVML;
using DynamicVML.Extensions;

namespace BookAuthors.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthorsController(ApplicationDbContext context)
        {
            _context = context;

            // First, let's setup some initial 
            // data to be used in the examples
            if (_context.Author.Count() == 0)
            {
                _context.Author.Add(new Author
                {
                    FirstName = "Sun",
                    LastName = "Tzu",
                    Books = { new Book { Title = "The Art Of War", PublicationYear = "500 b.c." } }
                });
                _context.Author.Add(new Author
                {
                    FirstName = "Carl",
                    LastName = "Jung",
                    Books = {
                        new Book { Title = "The Red Book", PublicationYear = "2009" },
                        new Book { Title = "Man and His Symbols", PublicationYear = "1964" } }
                });
                _context.Author.Add(new Author
                {
                    FirstName = "César",
                    LastName = "De Souza",
                    Books = {
                        new Book { Title = "Action Recognition in Videos: Data-efficient approaches for supervised learning of human action classification models for video", PublicationYear = "2018" },
                        new Book { Title = "Reconhecimento de Gestos da Língua Brasileira de Sinais através de Máquinas de Vetores de Suporte e Campos Condicionais Aleatórios Ocultos", PublicationYear = "2013" }
                    }
                });

                _context.SaveChanges();
            }
        }

        // How to setup dynamic view lists for your view models
        // ----------------------------------------------------
        //
        // Step 1) Add a reference to ~/lib/dynamic-viewmodel-lists/dvml.js in your view 
        //         (see the contents of the Create.cshtml, Edit.cshtml, Details.cshtml, 
        //         and Delete.cshtml files in this sample project).
        //
        // Step 2) Update your view models to use DynamicList<T> instead of List<T> for your
        //         view model collections (you can also use subclasses to keep the names short).
        // 
        // Step 3) Add an action to your controller to retrieve a dynamic view for the 
        //         list item that the user is wants to include in the list. It can be 
        //         either a GET or POST action method.
        // 
        // Step 4) Done!


        public IActionResult AddBook(AddNewDynamicItem parameters)
        {
            // This is the GET action that will be called whenever the user clicks 
            // the "Add new item" link in a DynamicList view. It accepts a an object
            // of the class ListItemParameters that contains information about where
            // the item needs to be inserted (i.e. the id of the div to where contents 
            // as well as the path to your viewmodels in the main form). All of those
            // are computed automatically from the view by the library.

            // Now here you can create another view model for your model
            var newBookViewModel = new BookViewModel()
            {
                Title = "New book"
            };

            // Now you have to call the extension PartialView method passing the view model.
            // This is an extension method that creates a partial view with the needed HTML 
            // prefix for the fields in your form so the form will post correctly when it 
            // gets submitted.
            return this.PartialView(newBookViewModel, parameters);
        }



        // Note: you can do these mappings with automapper or
        // any other way you prefer, this is just an example:
        private static AuthorViewModel modelToViewModel(Author model)
        {
            var authorViewModel = new AuthorViewModel()
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Books = model.Books.ToDynamicList(b => new BookViewModel()
                {
                    Id = b.Id,
                    Title = b.Title,
                    PublicationYear = b.PublicationYear
                })
            };

            return authorViewModel;
        }

        private static Author viewModelToModel(AuthorViewModel viewModel)
        {
            return new Author()
            {
                Id = viewModel.Id,
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                Books = viewModel.Books.ToModel(b => new Book
                {
                    Id = b.Id,
                    Title = b.Title,
                    PublicationYear = b.PublicationYear
                }).ToList()
            };
        }

        private static void updateModel(Author model, AuthorViewModel viewModel)
        {
            model.FirstName = viewModel.FirstName;
            model.LastName = viewModel.LastName;

            // Note: There are better ways to update collections from viewmodels, such as AutoMapper.Collections. This is just an example
            List<BookViewModel> booksToAdd = viewModel.Books.ViewModels.Where(x => !model.Books.Select(y => y.Id).Contains(x.Id)).ToList();
            List<Book> booksToRemove = model.Books.Where(x => !viewModel.Books.Select(y => y.ViewModel.Id).Contains(x.Id)).ToList();
            List<Book> booksToUpdate = model.Books.Where(x => viewModel.Books.Select(y => y.ViewModel.Id).Contains(x.Id)).ToList();

            foreach (Book book in booksToRemove)
            {
                model.Books.Remove(book);
            }

            foreach (BookViewModel bookViewModel in booksToAdd)
            {
                model.Books.Add(new Book
                {
                    Id = bookViewModel.Id,
                    Title = bookViewModel.Title,
                    PublicationYear = bookViewModel.PublicationYear
                });
            }

            foreach (Book book in booksToUpdate)
            {
                BookViewModel bookViewModel = viewModel.Books.ViewModels.Where(x => x.Id == book.Id).Single();
                book.Title = bookViewModel.Title;
                book.PublicationYear = bookViewModel.PublicationYear;
            }
        }


        // Note: the code below is just the default code generated by
        // VS assistant wizard, with the calls to our mapping functions
        // above in order to convert models to viewmodels and vice-versa


        // GET: Authors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Author.ToListAsync());
        }

        // GET: Authors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var author = await _context.Author
                .Include(x => x.Books)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (author == null)
                return NotFound();

            return View(modelToViewModel(author));
        }



        // GET: Authors/Create
        public IActionResult Create()
        {
            return View(new AuthorViewModel());
        }

        // POST: Authors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuthorViewModel author)
        {
            if (ModelState.IsValid)
            {
                _context.Add(viewModelToModel(author));
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(author);
        }

        // GET: Authors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var author = await _context.Author
                .Include(x => x.Books)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (author == null)
                return NotFound();

            return View(modelToViewModel(author));
        }

        // POST: Authors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AuthorViewModel viewModel)
        {
            if (id != viewModel.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                Author author = await _context.Author
                    .Include(x => x.Books)
                    .FirstOrDefaultAsync(m => m.Id == id);

                updateModel(author, viewModel);
                _context.Update(author);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: Authors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var author = await _context.Author
                .Include(x => x.Books)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (author == null)
                return NotFound();

            return View(modelToViewModel(author));
        }

        // POST: Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var author = await _context.Author.FindAsync(id);
            _context.Author.Remove(author);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuthorExists(int id)
        {
            return _context.Author.Any(e => e.Id == id);
        }
    }
}
