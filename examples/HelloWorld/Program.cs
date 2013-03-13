using System;
using Cavingdeep.Dcg.At;

namespace HelloWorld
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            string templateContent = "@! string name = \"Cavingdeep\";\r\n"
                                   + "Hello @(name).";
            AtTemplate template = new AtTemplate(templateContent);
            template.Parse();

            Console.WriteLine(template.Render());
            Console.ReadLine();
        }
    }
}
