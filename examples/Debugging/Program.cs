using System;
using System.Text;
using Cavingdeep.Dcg;
using Cavingdeep.Dcg.At;

namespace Debugging
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            AtTemplate template = new AtTemplate("numbers.al", (Encoding) null);

            template.Debug = true;

            try
            {
                template.Parse();

                template.Context = new object[] { 5 };
                
                Console.WriteLine(template.Render());
            }
            catch (TemplateParsingException ex)
            {
                Console.Write("A parsing error at line ");
                Console.WriteLine(ex.SourceLine);

                Console.WriteLine(ex.Message);
            }
            catch (TemplateCompilationException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            catch (TemplateRuntimeException ex)
            {
                Console.WriteLine(ex);
            }

            Console.ReadLine();
        }
    }
}
