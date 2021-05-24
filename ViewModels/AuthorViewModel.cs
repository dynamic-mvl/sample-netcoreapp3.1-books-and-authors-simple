using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using DynamicVML;

namespace BookAuthors.ViewModels
{
    public class AuthorViewModel
    {
        public int Id { get; set; }

        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Authored books")]
        public virtual DynamicList<BookViewModel> Books { get; set; } = new DynamicList<BookViewModel>();
    }
}
