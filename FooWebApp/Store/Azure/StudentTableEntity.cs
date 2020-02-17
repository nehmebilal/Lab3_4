using Microsoft.WindowsAzure.Storage.Table;

namespace FooWebApp.Store
{
    public class StudentTableEntity : TableEntity
    {
        public StudentTableEntity() // default constructor is mandatory
        {
        }

        public string Name { get; set; }
        public int GradePercentage { get; set; }
    }
}
