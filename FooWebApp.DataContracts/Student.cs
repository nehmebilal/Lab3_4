using System.Collections.Generic;

namespace FooWebApp.DataContracts
{
    public class Student
    {
        public string Id { get; set; }
        public string Name { get; set;}
        public int GradePercentage { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Student student &&
                   Id == student.Id &&
                   Name == student.Name &&
                   GradePercentage == student.GradePercentage;
        }

        public override int GetHashCode()
        {
            var hashCode = -256925990;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + GradePercentage.GetHashCode();
            return hashCode;
        }
    }
}
