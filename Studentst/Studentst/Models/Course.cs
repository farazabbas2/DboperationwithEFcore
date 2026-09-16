namespace Studentst.Models
{
    public class Course
    {

        public int id {  get; set; }
        public string Coursename { get; set; }

        public string duration { get; set; }

        public int fees { get; set; }

        public ICollection<Student> students { get; set; }
    }
}
