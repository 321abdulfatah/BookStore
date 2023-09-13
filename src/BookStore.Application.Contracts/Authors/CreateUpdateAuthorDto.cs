using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Authors
{
    public class CreateUpdateAuthorDto
    {

        [Required]
        [StringLength(AuthorConsts.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        public string ShortBio { get; set; }

        public ICollection<BookLookupDto> Books { get; set; }
    }
}
