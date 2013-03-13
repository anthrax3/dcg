using System;

namespace ComplexTypeReferencing
{
    public enum Gender
    {
        Male,
        Female
    }

    public class Person
    {
        public Person(string firstName, string lastName, int age, Gender gender)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Age = age;
            this.Gender = gender;
        }

        public string FirstName
        {
            get;
            set;
        }

        public string LastName
        {
            get;
            set;
        }

        public int Age
        {
            get;
            set;
        }

        public Gender Gender
        {
            get;
            set;
        }
    }
}
