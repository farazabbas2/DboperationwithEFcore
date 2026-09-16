namespace Studentst.Models
{
    public class Department
    {
        public int id {  get; set; }
        public string name { get; set; }
        public string description { get; set; }


        public ICollection<Student> Student {  get; set; }
    }
}
