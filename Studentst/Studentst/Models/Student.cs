using Microsoft.Identity.Client;

namespace Studentst.Models
{
    public class Student
    {
        public int id {  get; set; }
        public string Name{  get; set; }
        public int age {  get; set; }
        public string PhoneNo {  get; set; }

        public bool isDeleted { get; set; }=false;

        public int DepartmentId { get; set; }

        public Department department { get; set; }

        public int teamId { get; set; }
        public Team team { get; set; }

        public int CourseId { get; set; }
        public Course course { get; set; }
    }
}
