using AutoMapper;
using Studentst.DTOs;
using Studentst.Models;

namespace Studentst.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Rule 1: Jab StudentDto aaye, toh usse Student model mein convert karo
            CreateMap<StudentDto, Student>();

            // Rule 2: Jab UpdateStudentDto aaye, toh usse Student model mein convert karo
            CreateMap<UpdateStudentDto, Student>();


        }
    }
}