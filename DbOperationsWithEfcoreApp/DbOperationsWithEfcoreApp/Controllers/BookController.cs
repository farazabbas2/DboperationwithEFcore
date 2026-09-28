using Azure;
using DbOperationsWithEfcoreApp.Data;
using DbOperationsWithEfcoreApp.Dtos;
using DbOperationsWithEfcoreApp.Models; 
using DbOperationsWithEfcoreApp.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
namespace DbOperationsWithEfcoreApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IValidator<CreateBookDto> _createBookValidator;
        private readonly IMapper _mapper;

        public BookController(AppDbContext appDbContext, IValidator<CreateBookDto> createBookValidator ,IMapper mapper)
        {
            _appDbContext = appDbContext;
            _createBookValidator = createBookValidator;
            _mapper = mapper;
        }
        [Authorize]
        [HttpGet("")]
        public async Task<IActionResult> GetAllBooks()
        {
            // 1. Related data ko Include karein (Eager Loading)
            var books = await _appDbContext.Books
                .Include(b => b.BookLanguages)
                    .ThenInclude(bl => bl.Language)
                .Include(b => b.BookColors)
                    .ThenInclude(bc => bc.color)
                .Include(b => b.BookPrices)
                    .ThenInclude(bp => bp.Currency)
                    .IgnoreQueryFilters()
                .ToListAsync();

            // 2. Data ko clean format mein project karein (Frontend ke liye best)
            /*  var response = books.Select(b => new
              {
                  id = b.Id,
                  title = b.Title,
                  description = b.Description,
                  noOfPages = b.NoOfPages,
                  createdAt = b.CreatedAt,
                  updatedAt = b.UpdatedAt,
                  isActive = b.IsActive,

                  // Languages Array
                  languages = b.BookLanguages.Select(bl => new
                  {
                      languageId = bl.LanguageId,
                      languageName = bl.Language?.Name ?? "Unknown"
                  }).ToList(),

                  // Colors Array
                  colors = b.BookColors.Select(bc => new
                  {
                      colorId = bc.ColorId,
                      colorName = bc.color?.Name ?? "Unknown"
                  }).ToList(),

                  // Prices Array (Optional - agar BookPrices use kar rahe hain)
                  prices = b.BookPrices.Select(bp => new
                  {
                      priceId = bp.Id,
                      amount = bp.amount,
                      currencyName = bp.Currency?.Title ?? "Unknown"
                  }).ToList()
              })*/
             var response = _mapper.Map<List<BookResponseDto>>(books);

            return Ok(new
            {
                success = true,
                count = response.Count(),
                data = response
            });
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBookByIdAsync([FromRoute] int id)
        {
            var book = await _appDbContext.Books
                .Include(b => b.BookLanguages)
                    .ThenInclude(bl => bl.Language)
                .Include(b => b.BookColors)
                    .ThenInclude(bc => bc.color)
                .Include(b => b.BookPrices)
                    .ThenInclude(bp => bp.Currency)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound(new { success = false, message = "Book not found." });
            }

            var result = _mapper.Map<BookResponseDto>(book);

            return Ok(new { success = true, data = result });
        }

        [Authorize(Roles ="Admin")]

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id,[FromBody] UpdateBookDto bookDto)

        {
            // 1. Find existing book
            var book = await _appDbContext.Books.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (book == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Book not found"
                });
            }

            // 2. Validate Language IDs
            var languages = await _appDbContext.Languages
                .Where(x => bookDto.LanguageIds.Contains(x.Id))
                .ToListAsync();

            if (languages.Count != bookDto.LanguageIds.Count)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "One or more Language IDs are invalid."
                });
            }

            // 3. Validate Color IDs
            var colors = await _appDbContext.Colors
                .Where(x => bookDto.ColorIds.Contains(x.Id))
                .ToListAsync();

            if (colors.Count != bookDto.ColorIds.Count)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "One or more Color IDs are invalid."
                });
            }

            // 4. Update Book basic information
            book.Title = bookDto.Title;
            book.Description = bookDto.Description;
            book.NoOfPages = bookDto.NoOfPages;


            // 5. Remove old BookLanguages
            var oldLanguages = await _appDbContext.BookLanguages
                .Where(x => x.BookId == id)
                .ToListAsync();

            _appDbContext.BookLanguages.RemoveRange(oldLanguages);


            // 6. Add new BookLanguages
            var newBookLanguages = bookDto.LanguageIds
                .Select(languageId => new BookLanguage
                {
                    BookId = id,
                    LanguageId = languageId
                })
                .ToList();

            await _appDbContext.BookLanguages.AddRangeAsync(newBookLanguages);


            // 7. Remove old BookColors
            var oldColors = await _appDbContext.BookColors
                .Where(x => x.BookId == id)
                .ToListAsync();

            _appDbContext.BookColors.RemoveRange(oldColors);


            // 8. Add new BookColors
            var newBookColors = bookDto.ColorIds
                .Select(colorId => new BookColor
                {
                    BookId = id,
                    ColorId = colorId
                })
                .ToList();

            await _appDbContext.BookColors.AddRangeAsync(newBookColors);


            // 9. Save everything
            await _appDbContext.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Book updated successfully"
            });
        }

        [HttpPost("{id}/upload-pdf")]
        [Authorize(Roles = "Admin")] // Sirf Admin upload kar sake
        public async Task<IActionResult> UploadBookPdf(int id, IFormFile pdfFile)
        {
            // 1. Validation
            if (pdfFile == null || pdfFile.Length == 0)
                return BadRequest("Please select a valid PDF file.");

            if (!pdfFile.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only PDF files are allowed.");

            // 2. Book ko database se dhundho
            var book = await _appDbContext.Books.FindAsync(id);
            if (book == null) return NotFound("Book not found.");

            // 3. Folder banayein (agar exist nahi karta)
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "books");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 4. Unique naam do file ko (taaki purani file overwrite na ho)
            var uniqueFileName = $"{Guid.NewGuid()}_{pdfFile.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 5. File ko server par save karo
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(fileStream);
            }

            // 6. Database me file ka path save karo
            book.PdfFilePath = $"/uploads/books/{uniqueFileName}";
            await _appDbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "PDF uploaded successfully!", path = book.PdfFilePath });
        }


        [HttpGet("{id}/read")]
        [Authorize(Roles = "Admin,User,Viewer")] // Sab padh sakte hain
        public async Task<IActionResult> ReadBook(int id)
        {
            var book = await _appDbContext.Books.FindAsync(id);

            // Check karo book hai ya nahi, aur kya uska PDF path set hai
            if (book == null || string.IsNullOrEmpty(book.PdfFilePath))
            {
                return NotFound(new { message = "Book or PDF not found." });
            }

            // Full path banao
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", book.PdfFilePath.TrimStart('/'));

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound(new { message = "PDF file missing on server." });
            }

            // File ko read karke browser ko bhejo
            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, "application/pdf", book.Title + ".pdf");
        }

        [Authorize(Roles ="Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id )
        {
            var exisitingRecord = await _appDbContext.Books.FirstOrDefaultAsync(x => x.Id == id); 
                if(exisitingRecord==null)
            {
                return NotFound(new
                {
                    success = false,
                    message="Book id Not found"
                });
               
            }
            if (exisitingRecord.IsActive == false)
            {

                return Conflict(new
                {
                    success = false,
                    message = "Book is already deleted"
                });

               
            }
           
            exisitingRecord.IsActive = false;
            exisitingRecord.UpdatedAt = DateTime.UtcNow;
            await _appDbContext.SaveChangesAsync();
            return Ok(new
            {
                message = "Book Deleted Successfully"
            });
        }

       [Authorize(Roles = "Admin")]
        [HttpPost("bulk")]
        public async Task<IActionResult> AddBooksBulk([FromBody] List<CreateBookDto> booksDto)
        {
            // 1. Validation: Check if list is null or empty
            if (booksDto == null || !booksDto.Any())
            {
                return BadRequest(new { message = "Book list cannot be null or empty." });
            }

            // 2. Collect all unique IDs first (Null check added for safety)
            var allLanguageIds = booksDto.SelectMany(b => b.LanguageIds ?? new List<int>()).Distinct().ToList();
            var allColorIds = booksDto.SelectMany(b => b.ColorIds ?? new List<int>()).Distinct().ToList();

            // 3. Fetch actual entities from database (Only if lists are not empty to avoid SQL 'IN ()' errors)
            var languages = new List<Language>();
            var colors = new List<Color>();

            if (allLanguageIds.Any())
            {
                languages = await _appDbContext.Languages
                    .Where(l => allLanguageIds.Contains(l.Id))
                    .ToListAsync();
            }

            if (allColorIds.Any())
            {
                colors = await _appDbContext.Colors
                    .Where(c => allColorIds.Contains(c.Id))
                    .ToListAsync();
            }

            // 4. Validation: Check if all requested IDs actually exist in the database
            if (allLanguageIds.Any() && languages.Count != allLanguageIds.Count)
            {
                return BadRequest(new { message = "Some LanguageIds do not exist in the database." });
            }

            if (allColorIds.Any() && colors.Count != allColorIds.Count)
            {
                return BadRequest(new { message = "Some ColorIds do not exist in the database." });
            }

            // 5. Create lookup dictionaries for fast O(1) access
            var languageDict = languages.ToDictionary(l => l.Id);
            var colorDict = colors.ToDictionary(c => c.Id);

            // 6. Mapping with proper entity references
            var booksToInsert = booksDto.Select(dto => new Book
            {
                Title = dto.Title,
                Description = dto.Description,
                NoOfPages = dto.NoOfPages,

                // Explicit join tables mapping (Fixed syntax errors here)
                BookLanguages = dto.LanguageIds.Select(id => new BookLanguage
                {
                    LanguageId = id,
                    Language = languageDict[id]  // Principal entity set karna zaroori hai
                }).ToList(),

                BookColors = dto.ColorIds.Select(id => new BookColor
                {
                    ColorId = id,
                    color = colorDict[id]        
                }).ToList()                      
            }).ToList();                         

            // 7. Save to Database
            try
            {
                _appDbContext.Books.AddRange(booksToInsert);
                await _appDbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = $"{booksToInsert.Count} books added successfully!",
                    insertedCount = booksToInsert.Count
                });
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    message = "Database error while saving books.",
                    details = ex.InnerException?.Message
                });
            }
        } // FIX 4: Method ka closing brace '}' add kiya


        [Authorize(Roles = "Admin,User")]
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookDto createDto)
        {
            // 1. Validation Check
            var validationResult = await _createBookValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = validationResult.Errors.Select(e => new
                    {
                        propertyName = e.PropertyName,
                        errorMessage = e.ErrorMessage
                    })
                });
            }

            // 2. Duplicate Check (Title aur Description match karke)
            string cleanTitle = createDto.Title.Trim().ToLower();
            string cleanDesc = createDto.Description.Trim().ToLower();

            var existingResult = await _appDbContext.Books
                .FirstOrDefaultAsync(x =>
                    x.Title.Trim().ToLower() == cleanTitle &&
                    x.Description.Trim().ToLower() == cleanDesc);

            if (existingResult != null)
            {
                return Conflict(new
                {
                    success = false,
                    message = "Book with this Title and description already exists"
                });
            }

            // 3. Book Entity Create Karein
            var book = new Book
            {
                Title = createDto.Title.Trim(),
                Description = createDto.Description.Trim(),
                NoOfPages = createDto.NoOfPages,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,

                // Junction tables ke liye empty lists initialize karein
                BookColors = new List<BookColor>(),
                BookLanguages = new List<BookLanguage>()
            };

            // 4. Multiple Colors ko Junction Table mein Add Karein
            if (createDto.ColorIds != null && createDto.ColorIds.Any())
            {
                foreach (var colorId in createDto.ColorIds)
                {
                    book.BookColors.Add(new BookColor { ColorId = colorId });
                }
            }

            // 5. Multiple Languages ko Junction Table mein Add Karein
            if (createDto.LanguageIds != null && createDto.LanguageIds.Any())
            {
                foreach (var languageId in createDto.LanguageIds)
                {
                    book.BookLanguages.Add(new BookLanguage { LanguageId = languageId });
                }
            }

            // 6. Database mein Save Karein
            _appDbContext.Books.Add(book);
            await _appDbContext.SaveChangesAsync();

            // 7. Saved Book ko Details (Colors & Languages) ke saath Fetch Karein
            var createdBookWithDetails = await _appDbContext.Books
                .Include(b => b.BookColors).ThenInclude(bc => bc.color)      // ✅ Capital 'C'
                .Include(b => b.BookLanguages).ThenInclude(bl => bl.Language)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == book.Id);                   // ✅ Capital 'I'

            // 8. Null Check (Safety)
            if (createdBookWithDetails == null)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Book created but failed to fetch details"
                });
            }

            // 9. Response Return Karein (201 Created)
            return StatusCode(201, new
            {
                success = true,
                message = "Book created successfully.",
                data = new
                {
                    id = createdBookWithDetails.Id,                           // ✅ Capital 'I'
                    title = createdBookWithDetails.Title,
                    description = createdBookWithDetails.Description,
                    noOfPages = createdBookWithDetails.NoOfPages,

                    // ✅ Multiple Colors return karein
                    colors = createdBookWithDetails.BookColors.Select(bc => new
                    {
                        colorId = bc.ColorId,
                        colorName = bc.color?.Name ?? "Unknown"              // ✅ Capital 'C' + null safety
                    }).ToList(),

                    // ✅ Multiple Languages return karein
                    languages = createdBookWithDetails.BookLanguages.Select(bl => new
                    {
                        languageId = bl.LanguageId,
                        languageName = bl.Language?.Name ?? "Unknown"        // ✅ Name, Title nahi!
                    }).ToList(),

                    createdAt = createdBookWithDetails.CreatedAt,
                    isActive = createdBookWithDetails.IsActive             // ✅ IsActive (1 = Active)
                }
            });

            
        }

           
 

    }
   
}
