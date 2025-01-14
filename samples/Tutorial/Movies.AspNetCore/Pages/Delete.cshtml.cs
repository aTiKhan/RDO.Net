﻿using DevZest.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Movies.AspNetCore.Pages
{
    public class DeleteModel : PageModel
    {
        private readonly Db _db;

        public DeleteModel(Db db)
        {
            _db = db;
        }

        public DataSet<Movie> Movie { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Movie = await _db.GetMovieAsync(id);

            if (Movie.Count == 0)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            await _db.Movie.DeleteAsync(m => m.ID == id);
            return RedirectToPage("./Index");
        }
    }
}