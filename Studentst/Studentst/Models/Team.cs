namespace Studentst.Models
{
    public class Team
    {

        public int id {  get; set; }

        public string teamName { get; set; }

        public ICollection<Student> students { get; set; }
    }
}
