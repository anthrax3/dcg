using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cavingdeep.Dcg.At;

namespace MultiOutput
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            AtTemplate template = new AtTemplate("multi.al", (Encoding) null);
            template.Parse();

            string output2 = "output2";

            Dictionary<string, TextWriter> writers =
                new Dictionary<string, TextWriter>();
            writers.Add(AtTemplate.MainOutputKey, new StringWriter());
            writers.Add(output2, new StringWriter());

            template.Render(writers);

            Console.WriteLine("Main Output:");
            Console.WriteLine(writers[AtTemplate.MainOutputKey].ToString());

            Console.WriteLine("Output 2:");
            Console.WriteLine(writers[output2].ToString());

            Console.ReadLine();
        }
    }
}
