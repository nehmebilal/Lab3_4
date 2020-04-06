using System.Collections.Generic;
using FooWebApp.DataContracts;

namespace FooWebApp.Store
{
    public class GetStudentsResult
    {
        public string ContinuationToken { get; set; }
        public List<Student> Students { get; set; }
    }
}
