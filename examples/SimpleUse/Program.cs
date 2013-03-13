using System;
using System.Text;
using Cavingdeep.Dcg.At;

namespace SimpleUse
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            AtTemplate template = new AtTemplate("numbers.al", (Encoding) null);
            template.Parse();

            Console.WriteLine(template.Render());
            Console.ReadLine();
        }
    }
}
