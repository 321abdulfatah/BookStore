using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace BookStore.Authors
{
    public interface IAuthorAppService :
        ICrudAppService< //Defines CRUD methods
        AuthorDto, //Used to show books
        Guid, //Primary key of the book entity
        GetAuthorListDto, //Used for paging/sorting
        CreateAuthorDto,UpdateAuthorDto> //Used to create/update a book
    {
    }
}
