using AutoMapper;
using DbOperationsWithEfcoreApp.Models;
using DbOperationsWithEfcoreApp.Dtos;

namespace DbOperationsWithEfcoreApp.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // GET API ke liye (Entity -> Response DTO)
            CreateMap<Book, BookResponseDto>()
                .ForMember(dest => dest.Languages, opt => opt.MapFrom(src => src.BookLanguages))
                .ForMember(dest => dest.Colors, opt => opt.MapFrom(src => src.BookColors))
                .ForMember(dest => dest.Prices, opt => opt.MapFrom(src => src.BookPrices));

            // Nested mappings
            CreateMap<BookLanguage, BookLanguageDto>()
                .ForMember(dest => dest.LanguageName, opt => opt.MapFrom(src => src.Language.Name));

            CreateMap<BookColor, BookColorDto>()
                .ForMember(dest => dest.ColorName, opt => opt.MapFrom(src => src.color.Name));
            // Note: Agar aapke model mein 'Color' capital C se hai, toh src.Color.Name karein.

            CreateMap<BookPrice, BookPriceDto>()
                .ForMember(dest => dest.CurrencyName, opt => opt.MapFrom(src => src.Currency.Title));

            // POST/PUT API ke liye (DTO -> Entity)
            CreateMap<CreateBookDto, Book>();
            CreateMap<UpdateBookDto, Book>();
            // Apne MappingProfile.cs me jayein aur ye line add karein:
            CreateMap<BookPrice, BookPriceDto>()
                .ForMember(dest => dest.PriceId, opt => opt.MapFrom(src => src.Id)); // ✅ Yahan batayein ki PriceId, Id se aayega
        }
    }
}