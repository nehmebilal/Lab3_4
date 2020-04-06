using System.Collections.Generic;

namespace FooWebApp.DataContracts
{
    public class GetStudentsResponse
    {
        public List<Student> Students { get; set; } 
        public string NextUri { get; set; }
    }
}