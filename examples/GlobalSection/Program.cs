using System;
using System.Text;
using Cavingdeep.Dcg.At;

namespace GlobalSection
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            AtTemplate template = new AtTemplate("capitalize.al", (Encoding) null);
            template.Parse();

            Console.WriteLine(template.Render());
            Console.ReadLine();
        }
    }
}
