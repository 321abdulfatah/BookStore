using BookStore.Books;
using BookStore.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace BookStore.Authors
{
    public class AuthorAppService :
    CrudAppService<
        Author, //The Author entity
        AuthorDto, //Used to show authors
        Guid, //Primary key of the author entity
        GetAuthorListDto, //Used for paging/sorting
        CreateAuthorDto,UpdateAuthorDto>, //Used to create/update a author
    IAuthorAppService
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly AuthorManager _authorManager;
        private readonly IRepository<Book, Guid> _bookRepository;


        public AuthorAppService(
            IAuthorRepository authorRepository,
            AuthorManager authorManager,
            IRepository<Book, Guid> bookRepository)
            : base(authorRepository)
        {
            _authorRepository = authorRepository;
            _authorManager = authorManager;
            _bookRepository = bookRepository;
            GetPolicyName = BookStorePermissions.Authors.Default;
            GetListPolicyName = BookStorePermissions.Authors.Default;
            CreatePolicyName = BookStorePermissions.Authors.Create;
            UpdatePolicyName = BookStorePermissions.Authors.Edit;
            DeletePolicyName = BookStorePermissions.Authors.Delete;
        }

        public override async Task<AuthorDto> CreateAsync(CreateAuthorDto input)
        {
            foreach (var bookDto in input.Books)
            {
                var book = ObjectMapper.Map<BookLookupDto, Book>(bookDto);

                var existingBook = await _bookRepository.FirstOrDefaultAsync(b => b.Name == book.Name);
                if (existingBook != null)
                {
                    throw new UserFriendlyException($"The book with the name {book.Name} is under the name of another author, " +
                        "so it cannot be added. Please check the book name and then try again to add the author");
                }
            }

                var author = await _authorManager.CreateAsync(
                input.Name,
                input.BirthDate,
                input.ShortBio
            );

            // Get the ID of the newly created author
            var authorId = author.Id;

            foreach (var bookDto in input.Books)
            {
                var book = ObjectMapper.Map<BookLookupDto,Book>(bookDto);

                book.AuthorId = authorId;

                await _bookRepository.InsertAsync(book);
            }

            await _authorRepository.InsertAsync(author);

            return ObjectMapper.Map<Author, AuthorDto>(author);
        }

        public override async Task<AuthorDto> UpdateAsync(Guid id, UpdateAuthorDto input)
        {
            var author = await _authorRepository.GetAsync(id);

            if (author.Name != input.Name)
            {
                await _authorManager.ChangeNameAsync(author, input.Name);
            }

            author.BirthDate = input.BirthDate;
            author.ShortBio = input.ShortBio;

            foreach (var bookDto in input.Books)
            {
                var book = ObjectMapper.Map<BookLookupDto, Book>(bookDto);
                book.AuthorId = id;

                var existingBook = await _bookRepository.FirstOrDefaultAsync(b => b.Name == book.Name);
                if (existingBook != null && existingBook.AuthorId != book.AuthorId)
                {
                    throw new UserFriendlyException($"The book with the name {book.Name} is under the name of another author, " +
                        "so it cannot be added. Please check the book name and then try again to add the author");
                }
                await _bookRepository.InsertAsync(book);
            }

            await _authorRepository.UpdateAsync(author);

            return ObjectMapper.Map<Author, AuthorDto>(author);
        }

        public override async Task<PagedResultDto<AuthorDto>> GetListAsync(GetAuthorListDto input)
        {
            //Get the IQueryable<Book> from the repository
            var queryable = await _authorRepository.GetQueryableAsync();

            Expression<Func<Author,bool>> filter = author => true;

            if (!string.IsNullOrEmpty(input.Filter))
                filter = author => author.Name.Contains(input.Filter);
            //Paging
            queryable = queryable
                .Where(filter)
                .OrderBy(input.Sorting ?? nameof(Author.Name))
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            //Execute the query and get a list
            var authors = await AsyncExecuter.ToListAsync(queryable);

            //Get the total count with another query
            var totalCount = await _authorRepository.CountAsync(filter);

            return new PagedResultDto<AuthorDto>(
                totalCount,
                ObjectMapper.Map<List<Author>, List<AuthorDto>>(authors)
            );
        }
    }
}
