using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace BookStore.Books
{
    public class GetBookListDto : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
    }
}
