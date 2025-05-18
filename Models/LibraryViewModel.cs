using System.Collections.Generic;

namespace IncognitoReads.Models
{
    public class LibraryViewModel
    {
        public List<AddBookViewModel> Books { get; set; } = new List<AddBookViewModel>();
    }
}
