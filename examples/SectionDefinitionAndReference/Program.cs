using System;
using Cavingdeep.Dcg.At;

namespace SectionDefinitionAndReference
{
    class Program
    {
        static void Main(string[] args)
        {
            using (IAtTemplate template = new AtTemplateProxy("recursive_list.txt", null, null))
            {
                template.Parse();
                template.Context = new object[] { 10 };

                Console.WriteLine(template.Render());
            }

            Console.ReadKey();
        }
    }
}
