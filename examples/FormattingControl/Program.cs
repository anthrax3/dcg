using System;
using System.Text;
using Cavingdeep.Dcg.At;

namespace FormattingControl
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            AtTemplate template = new AtTemplate("class.al", (Encoding) null);
            template.Parse();

            template.Context = new object[]
            {
                "SampleClass", 5, new string[] { "id", "name" }
            };

            Console.WriteLine(template.Render());
            Console.ReadLine();
        }
    }
}
