using System;
using System.Text;
using Cavingdeep.Dcg.At;

namespace ParametersAndBlocks
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            AtTemplate template = new AtTemplate("numbers.al", (Encoding) null);
            template.Parse();

            template.Context = new object[] { 5 };

            Console.WriteLine(template.Render());
            Console.ReadLine();
        }
    }
}
