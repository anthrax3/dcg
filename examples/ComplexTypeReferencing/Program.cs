using System;
using System.Text;
using Cavingdeep.Dcg.At;

namespace ComplexTypeReferencing
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            AtTemplate template = new AtTemplate("person.al", (Encoding) null);
            template.Parse();

            template.Context = new object[]
            {
                new Person("Seth", "Yuan", 24, Gender.Male)
            };

            Console.WriteLine(template.Render());
            Console.ReadLine();
        }
    }
}
