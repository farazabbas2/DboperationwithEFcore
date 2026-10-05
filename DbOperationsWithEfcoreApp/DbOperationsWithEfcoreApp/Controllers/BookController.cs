using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DbOperationsWithEfcoreApp.Dtos;
using DbOperationsWithEfcoreApp.Interfaces;
using DbOperationsWithEfcoreApp.Models;
using DbOperationsWithEfcoreApp.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.EntityFrameworkCore; // DbUpdateException ke liye

namespace DbOperationsWithEfcoreApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IValidator<CreateBookDto> _createBookValidator;
        private readonly IMapper _mapper;


        public BookController(IBookRepository bookRepository, IValidator<CreateBookDto> createBookValidator, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _createBookValidator = createBookValidator;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet("")]
        public async Task<IActionResult> GetAllBooks()
        {
            var books = await _bookRepository.GetAllBooksAsync();
            var response = _mapper.Map<List<BookResponseDto>>(books);

            return Ok(new { success = true, count = response.Count, data = response });
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBookByIdAsync([FromRoute] int id)
        {
            var book = await _bookRepository.GetBookByIdAsync(id);

            if (book == null)
                return NotFound(new { success = false, message = "Book not found." });

            var result = _mapper.Map<BookResponseDto>(book);
            return Ok(new { success = true, data = result });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookDto bookDto)
        {
            var book = await _bookRepository.GetBookByIdIgnoreFiltersAsync(id);
            if (book == null)
                return NotFound(new { success = false, message = "Book not found" });

            // Validation via Repository
            var languages = await _bookRepository.GetLanguagesByIdsAsync(bookDto.LanguageIds);
            if (languages.Count != bookDto.LanguageIds.Count)
                return BadRequest(new { success = false, message = "One or more Language IDs are invalid." });

            var colors = await _bookRepository.GetColorsByIdsAsync(bookDto.ColorIds);
            if (colors.Count != bookDto.ColorIds.Count)
                return BadRequest(new { success = false, message = "One or more Color IDs are invalid." });

            // Basic info update
            book.Title = bookDto.Title;
            book.Description = bookDto.Description;
            book.NoOfPages = bookDto.NoOfPages;
            if (bookDto.Prices != null)
            {
                await _bookRepository.UpdateBookPricesAsync(id, bookDto.Prices);
            }

            // Complex relations update (Languages & Colors)
            await _bookRepository.ReplaceBookRelationsAsync(id, bookDto.LanguageIds, bookDto.ColorIds);




            // Complex relations update ab Repository sambhal raha hai
            await _bookRepository.ReplaceBookRelationsAsync(id, bookDto.LanguageIds, bookDto.ColorIds);

            return Ok(new { success = true, message = "Book updated successfully" });
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchBook(int id, [FromBody] PatchBookDto patchDto)
        {
            var book = await _bookRepository.GetBookByIdIgnoreFiltersAsync(id);
            if (book == null)
                return NotFound(new { success = false, message = "Book not found" });
            // Only update the fields that are provided
            if (!string.IsNullOrEmpty(patchDto.Description))
                book.Description = patchDto.Description;
            await _bookRepository.UpdateBookAsync(book);
            return Ok(new { success = true, message = "Book patched successfully" });
        }



        [HttpPost("{id}/upload-pdf")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadBookPdf(int id, IFormFile pdfFile)
        {
            if (pdfFile == null || pdfFile.Length == 0)
                return BadRequest("Please select a valid PDF file.");

            if (!pdfFile.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only PDF files are allowed.");

            var book = await _bookRepository.GetBookByIdAsync(id);
            if (book == null) return NotFound("Book not found.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "books");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{pdfFile.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(fileStream);
            }

            var dbFilePath = $"/uploads/books/{uniqueFileName}";
            await _bookRepository.UpdateBookPdfPathAsync(id, dbFilePath);

            return Ok(new { success = true, message = "PDF uploaded successfully!", path = dbFilePath });
        }

        [HttpGet("{id}/read")]
        [Authorize(Roles = "Admin,User,Viewer")]
        public async Task<IActionResult> ReadBook(int id)
        {
            var book = await _bookRepository.GetBookByIdAsync(id);

            if (book == null || string.IsNullOrEmpty(book.PdfFilePath))
                return NotFound(new { message = "Book or PDF not found." });

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", book.PdfFilePath.TrimStart('/'));

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { message = "PDF file missing on server." });

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, "application/pdf", book.Title + ".pdf");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var existingRecord = await _bookRepository.GetBookByIdAsync(id);
            if (existingRecord == null)
                return NotFound(new { success = false, message = "Book id Not found" });

            if (existingRecord.IsActive == false)
                return Conflict(new { success = false, message = "Book is already deleted" });

            await _bookRepository.SoftDeleteBookAsync(id);

            return Ok(new { message = "Book Deleted Successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("bulk")]
        public async Task<IActionResult> AddBooksBulk([FromBody] List<CreateBookDto> booksDto)
        {
            if (booksDto == null || !booksDto.Any())
                return BadRequest(new { message = "Book list cannot be null or empty." });

            var allLanguageIds = booksDto.SelectMany(b => b.LanguageIds ?? new List<int>()).Distinct().ToList();
            var allColorIds = booksDto.SelectMany(b => b.ColorIds ?? new List<int>()).Distinct().ToList();

            var languages = new List<Language>();
            var colors = new List<Color>();

            if (allLanguageIds.Any())
                languages = await _bookRepository.GetLanguagesByIdsAsync(allLanguageIds);

            if (allColorIds.Any())
                colors = await _bookRepository.GetColorsByIdsAsync(allColorIds);

            if (allLanguageIds.Any() && languages.Count != allLanguageIds.Count)
                return BadRequest(new { message = "Some LanguageIds do not exist in the database." });

            if (allColorIds.Any() && colors.Count != allColorIds.Count)
                return BadRequest(new { message = "Some ColorIds do not exist in the database." });

            var languageDict = languages.ToDictionary(l => l.Id);
            var colorDict = colors.ToDictionary(c => c.Id);

            var booksToInsert = booksDto.Select(dto => new Book
            {
                Title = dto.Title,
                Description = dto.Description,
                NoOfPages = dto.NoOfPages,
                BookLanguages = dto.LanguageIds.Select(id => new BookLanguage { LanguageId = id, Language = languageDict[id] }).ToList(),
                BookColors = dto.ColorIds.Select(id => new BookColor { ColorId = id, color = colorDict[id] }).ToList()
            }).ToList();

            try
            {
                await _bookRepository.AddBooksRangeAsync(booksToInsert);
                return Ok(new { message = $"{booksToInsert.Count} books added successfully!", insertedCount = booksToInsert.Count });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new { message = "Database error while saving books.", details = ex.InnerException?.Message });
            }
        }

        [Authorize(Roles = "Admin,User")]
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookDto createDto)
        {
            var validationResult = await _createBookValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = validationResult.Errors.Select(e => new { propertyName = e.PropertyName, errorMessage = e.ErrorMessage })
                });
            }

            // Duplicate check ab Repository kar raha hai
            var isDuplicate = await _bookRepository.IsBookDuplicateAsync(createDto.Title, createDto.Description);
            if (isDuplicate)
            {
                return Conflict(new { success = false, message = "Book with this Title and description already exists" });
            }

            var book = new Book
            {
                Title = createDto.Title.Trim(),
                Description = createDto.Description.Trim(),
                NoOfPages = createDto.NoOfPages,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                BookColors = new List<BookColor>(),
                BookLanguages = new List<BookLanguage>(),
                BookPrices = new List<BookPrice>()
            };

            if (createDto.ColorIds != null && createDto.ColorIds.Any())
            {
                foreach (var colorId in createDto.ColorIds)
                    book.BookColors.Add(new BookColor { ColorId = colorId });
            }

            if (createDto.LanguageIds != null && createDto.LanguageIds.Any())
            {
                foreach (var languageId in createDto.LanguageIds)
                    book.BookLanguages.Add(new BookLanguage { LanguageId = languageId });
            }

            // Save via Repository
            await _bookRepository.AddBookAsync(book);

            if (createDto.Prices != null && createDto.Prices.Any())
            {
                await _bookRepository.AddBookPricesAsync(book.Id, createDto.Prices);
            }

            // Created book ko details ke saath fetch karna bhi ab Repository ka kaam hai
            var createdBookWithDetails = await _bookRepository.GetBookByIdAsync(book.Id);
            var pricesCount = createdBookWithDetails.BookPrices.Count();
            Console.WriteLine($"Total prices fetched: {pricesCount}");

            if (createdBookWithDetails == null)
                return StatusCode(500, new { success = false, message = "Book created but failed to fetch details" });
        
                return StatusCode(201, new
                {
                    success = true,
                    message = "Book created successfully.",
                    data = new
                    {
                        id = createdBookWithDetails.Id,
                        title = createdBookWithDetails.Title,
                        description = createdBookWithDetails.Description,
                        noOfPages = createdBookWithDetails.NoOfPages,
                        colors = createdBookWithDetails.BookColors.Select(bc => new { colorId = bc.ColorId, colorName = bc.color?.Name ?? "Unknown" }).ToList(),
                        languages = createdBookWithDetails.BookLanguages.Select(bl => new { languageId = bl.LanguageId, languageName = bl.Language?.Name ?? "Unknown" }).ToList(),
                        createdAt = createdBookWithDetails.CreatedAt,
                        isActive = createdBookWithDetails.IsActive,
                        price = createdBookWithDetails.BookPrices.Select(bp => new
                        {
                            priceId = bp.Id,  // Ya bp.PriceId (jo bhi aapka primary key hai)
                            price = bp.amount, // Price ka value
                            currencyId = bp.CurrencyId
                        }).ToList()
                    }

                });
            }
        }
    }
