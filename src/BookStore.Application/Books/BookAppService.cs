using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BookStore.Authors;
using BookStore.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace BookStore.Books
{
    public class BookAppService :
    CrudAppService<
        Book, //The Book entity
        BookDto, //Used to show books
        Guid, //Primary key of the book entity
        GetBookListDto, //Used for paging/sorting
        CreateUpdateBookDto>, //Used to create/update a book
    IBookAppService //implement the IBookAppService
    {
        private readonly IAuthorRepository _authorRepository;

        public BookAppService(
            IRepository<Book, Guid> repository,
            IAuthorRepository authorRepository)
            : base(repository)
        {
            _authorRepository = authorRepository;
            GetPolicyName = BookStorePermissions.Books.Default;
            GetListPolicyName = BookStorePermissions.Books.Default;
            CreatePolicyName = BookStorePermissions.Books.Create;
            UpdatePolicyName = BookStorePermissions.Books.Edit;
            DeletePolicyName = BookStorePermissions.Books.Delete;
        }

        public override async Task<BookDto> GetAsync(Guid id)
        {
            //Get the IQueryable<Book> from the repository
            var query = await Repository.WithDetailsAsync(x => x.Author);

            //Execute the query and get the book with author
            var book = await AsyncExecuter.FirstOrDefaultAsync(query)
                ?? throw new EntityNotFoundException(typeof(Book), id);
            
            var bookDto = ObjectMapper.Map<Book, BookDto>(book);

            return bookDto;
        }

        public override async Task<PagedResultDto<BookDto>> GetListAsync(GetBookListDto input)
        {
            //Get the IQueryable<Book> from the repository
            var queryable = await Repository.WithDetailsAsync(x => x.Author);
            
            Expression<Func<Book, bool>> filter = book => true;

            if (!string.IsNullOrEmpty(input.Filter))
                filter = book => book.Name.Contains(input.Filter);
            //Paging
            queryable = queryable
                .Where(filter)
                .OrderBy(input.Sorting ?? nameof(Book.Name))
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);
            //Execute the query and get a list
            var books = await AsyncExecuter.ToListAsync(queryable);

            //Get the total count with another query
            var totalCount = await Repository.CountAsync(filter);

            return new PagedResultDto<BookDto>(
                totalCount,
                ObjectMapper.Map<List<Book>, List<BookDto>>(books)
            );
        }

        public async Task<ListResultDto<AuthorLookupDto>> GetAuthorLookupAsync()
        {
            var authors = await _authorRepository.GetListAsync();

            return new ListResultDto<AuthorLookupDto>(
                ObjectMapper.Map<List<Author>, List<AuthorLookupDto>>(authors)
            );
        }
    }
}