using System.Collections.Generic;

namespace IncognitoReads.Models
{
    public class BookDetailsViewModel
    {
        public AddBookViewModel Book { get; set; } = new AddBookViewModel();

        
        public List<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();

        
        public ReviewViewModel NewReview { get; set; } = new ReviewViewModel();
    }
}
