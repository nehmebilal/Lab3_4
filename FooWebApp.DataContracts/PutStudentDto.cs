using System.Collections.Generic;

namespace FooWebApp.DataContracts
{
    public class PutStudentDto
    {
        public string Name { get; set;}
        public int GradePercentage { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Student student &&
                   Name == student.Name &&
                   GradePercentage == student.GradePercentage;
        }

        public override int GetHashCode()
        {
            var hashCode = -256925990;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + GradePercentage.GetHashCode();
            return hashCode;
        }
    }
}