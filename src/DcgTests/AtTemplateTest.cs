/*
 *  Dynamic Code Generator
 *  Copyright (C) 2006 Wei Yuan
 *
 *  This library is free software; you can redistribute it and/or modify it
 *  under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation; either version 2.1 of the License, or (at
 *  your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful, but
 *  WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 *  or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public
 *  License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with this library; if not, write to the Free Software Foundation,
 *  Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
 *
 */

using System;
using System.Collections.Generic;
using System.IO;
using Cavingdeep.Dcg;
using Cavingdeep.Dcg.At;
using NUnit.Framework;

namespace Cavingdeep.Tests.Dcg
{
    [TestFixture]
    public class AtTemplateTest
    {
        [Test]
        public void NothingToParse()
        {
            using (IAtTemplate template = new AtTemplate(""))
            {
                template.Parse();
                Assert.IsEmpty(template.Render());
            }
        }

        [Test]
        public void SingleInstance()
        {
            IAtTemplate template = new AtTemplate(
                @"@global
int number = 1;
@end_global
@(number++)");
            try
            {
                template.Parse();
            }
            catch (TemplateCompilationException ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

            Assert.AreEqual("1\r\n", template.Render());
            Assert.AreEqual("2\r\n", template.Render());
        }

        [Test]
        public void NewInstance()
        {
            IAtTemplate template = new AtTemplate(
                @"@global
int number = 1;
@end_global
@(number++)");
            template.Parse();

            Assert.AreEqual("1\r\n", template.Render());
            IAtTemplateInstance instance1 =
                template.NewInstance();
            Assert.AreEqual("1\r\n", instance1.Render());
        }

        [Test]
        public void ProxyNewInstance()
        {
            using (IAtTemplate template = new AtTemplateProxy(
                @"@global
int number = 1;
@end_global
@(number++)"))
            {
                template.Parse();

                Assert.AreEqual("1\r\n", template.Render());
                IAtTemplateInstance instance1 =
                    template.NewInstance();
                Assert.AreEqual("1\r\n", instance1.Render());
            }
        }

        [Test]
        public void TextLine()
        {
            AtTemplate template = new AtTemplate("line 1\r\nline 2");
            template.Parse();

            Assert.AreEqual("line 1\r\nline 2\r\n", template.Render());
        }

        [Test]
        public void MultiOutput()
        {
            AtTemplate template = new AtTemplate(
                @"main output
@output key1
key1 output
@end_output");
            template.Parse();

            IDictionary<string, TextWriter> writers =
                new Dictionary<string, TextWriter>();
            writers.Add(AtTemplate.MainOutputKey, new StringWriter());
            writers.Add("key1", new StringWriter());

            template.Render(writers);

            Assert.AreEqual("main output\r\n",
                            writers[AtTemplate.MainOutputKey].ToString());
            Assert.AreEqual("key1 output\r\n", writers["key1"].ToString());
        }

        [Test]
        public void MultiOutputWithEvaluationDirective()
        {
            AtTemplate template = new AtTemplate(
                @"main output
@output key1
@(""key1"") output
@end_output");
            template.Parse();

            IDictionary<string, TextWriter> writers =
                new Dictionary<string, TextWriter>();
            writers.Add(AtTemplate.MainOutputKey, new StringWriter());
            writers.Add("key1", new StringWriter());

            template.Render(writers);

            Assert.AreEqual("main output\r\n",
                            writers[AtTemplate.MainOutputKey].ToString());
            Assert.AreEqual("key1 output\r\n", writers["key1"].ToString());
        }

        [Test]
        [ExpectedException(typeof(TemplateParsingException))]
        public void EmptyOutputKey()
        {
            AtTemplate template = new AtTemplate(
                @"main output
@output
something
@end_output");
            template.Parse();
        }

        [Test]
        public void WhiteSpace()
        {
            AtTemplate template = new AtTemplate(
                @"line1@
,line1 too
");
            template.Parse();

            Assert.AreEqual("line1,line1 too\r\n", template.Render());
        }

        [Test]
        public void FakeWhiteSpace()
        {
            AtTemplate template = new AtTemplate(
                @"line1@@@@
,line1 too
");
            template.Parse();

            Assert.AreEqual("line1@@\r\n,line1 too\r\n", template.Render());
        }

        [Test]
        public void StaticEscaping()
        {
            AtTemplate template = new AtTemplate(
                "@@reference System.dll\r\n");

            template.Parse();

            Assert.AreEqual("@reference System.dll\r\n", template.Render());
        }

        [Test]
        public void Comment()
        {
            AtTemplate template = new AtTemplate(
                @"@# sample line 1
@# sample line 2
");

            template.Parse();

            Assert.AreEqual(0, template.Render().Length);
        }

        [Test]
        public void EvaluationEscape()
        {
            AtTemplate template = new AtTemplate("@@@@(1+1)");
            template.Parse();

            Assert.AreEqual("@@(1+1)\r\n", template.Render(),
                            "Par not working.");

            template = new AtTemplate("@@@(1+1)");
            template.Parse();

            Assert.AreEqual("@2\r\n", template.Render(),
                            "Single not working.");
        }

        [Test]
        public void CodeDirective()
        {
            AtTemplate template = new AtTemplate(
                @"line 1
@code
Dcg.Write(""line 2\r\n"");
@end_code
line 3");
            template.Parse();

            Assert.AreEqual("line 1\r\nline 2\r\nline 3\r\n",
                            template.Render());
        }

        [Test]
        public void CodeAndTextDirective()
        {
            AtTemplate template = new AtTemplate(
@"line 1
    @code
        int two = 2;
        @text
        line @(two)
        @end_text
    @end_code
");
            template.Parse();

            Assert.AreEqual("line 1\r\n    line 2\r\n", template.Render());
        }

        [Test]
        [ExpectedException(typeof(TemplateParsingException))]
        public void WrongTextEndDirective()
        {
            AtTemplate template = new AtTemplate("@end_text");
            template.Parse();
        }

        [Test]
        [ExpectedException(typeof(TemplateParsingException))]
        public void WrongParamDirective()
        {
            AtTemplate template = new AtTemplate("@param s: ");
            template.Parse();
        }

        [TestCase("@global\r\nint a = 1;", ExpectedException=typeof(TemplateParsingException))]
        [TestCase("@end_global", ExpectedException=typeof(TemplateParsingException))]
        public void WrongGlobalDirective(string text)
        {
            AtTemplate template = new AtTemplate(text);
            template.Parse();
        }

        [Test]
        [ExpectedException(typeof(TemplateParsingException))]
        public void TextDirective()
        {
            AtTemplate template = new AtTemplate(
                @"@code
    @text
    @code
        @text
        line 1

        @end_text
    @end_code
    @end_text
@end_code");
            template.Parse();
        }

        [Test]
        public void MultilineEvaluation()
        {
            AtTemplate template = new AtTemplate(
                "    @= \"line1\\r\\nline2\"");
            template.Parse();

            Assert.AreEqual("    line1\r\n    line2\r\n", template.Render());
        }

        [Test]
        public void MultilineEvaluationWithParentIndentation()
        {
            AtTemplate template = new AtTemplate(
@"line
    @code
        @text
        @= ""line1\r\nline2""
        @end_text
    @end_code");
            template.Parse();

            Assert.AreEqual("line\r\n    line1\r\n    line2\r\n", template.Render());
        }

        [Test]
        public void EmptyLine()
        {
            AtTemplate template = new AtTemplate("\r\n");
            template.Parse();

            Assert.AreEqual("\r\n", template.Render());
        }

        [Test]
        public void BetweenBlock()
        {
            AtTemplate template = new AtTemplate(
                @"{@{
if (true)
{
    @text
    1@
    @end_text
}
@}}");
            template.Parse();

            Assert.AreEqual("{1}\r\n", template.Render());
        }

        [Test]
        public void SpacesAfterDirective()
        {
            AtTemplate template = new AtTemplate(
                @"1@ 
@code 
if (true) {
    @text 
    1
    @end_text 
}
@end_code ");
            template.Parse();

            Assert.AreEqual("11\r\n", template.Render());
        }

        [Test]
        public void GlobalLineLocation()
        {
            AtTemplate template = new AtTemplate(
                @"@# line 1
@global
string b;
int a = 1;
@end_global
@
@global
c = 1;
@end_global");
            template.Debug = true;

            try
            {
                template.Parse();
                Assert.Fail();
            }
            catch (TemplateCompilationException ex)
            {
                Assert.AreEqual(8, ex.Errors[0].Line);
            }
        }

        [Test]
        public void ParameterLineLocation()
        {
            AtTemplate template = new AtTemplate(
                @"@# line 1
@param a:A
");
            template.Debug = true;

            try
            {
                template.Parse();
                Assert.Fail();
            }
            catch (TemplateCompilationException ex)
            {
                Assert.AreEqual(2, ex.Errors[1].Line);
            }
        }

        [Test]
        public void AtTemplateProxy()
        {
            string generatedFile = null;

            using (IAtTemplate template = new AtTemplateProxy("1+1"))
            {
                template.Debug = true;
                template.Parse();
                template.Render();

                generatedFile = template.GeneratedAssemblyPath;
            }

            Assert.IsFalse(File.Exists(generatedFile));
        }

        [Test]
        public void InnerMultiOutput()
        {
            AtTemplate template = new AtTemplate(
                @"@output page2
<html>
    <br/>
    @! for (int i = 1; i <= 3; i++) {
      @output page3
      Just kidding.
        @code
          for (int j = 1; j < 3; j++) {
            @text
            page3
              @output page4
              OK
              @end_output
            @end_text
          }
        @end_code
      @end_output
    @! }
</html>
@end_output");
            template.Parse();

            Dictionary<string, TextWriter> outputs =
                new Dictionary<string, TextWriter>();
            outputs.Add(AtTemplate.MainOutputKey, new StringWriter());
            outputs.Add("page2", new StringWriter());
            outputs.Add("page3", new StringWriter());
            outputs.Add("page4", new StringWriter());

            template.Render(outputs);

            Assert.AreEqual(@"<html>
    <br/>
</html>
", outputs["page2"].ToString());

            Assert.AreEqual(@"Just kidding.
  page3
  page3
Just kidding.
  page3
  page3
Just kidding.
  page3
  page3
", outputs["page3"].ToString());

            Assert.AreEqual(@"OK
OK
OK
OK
OK
OK
", outputs["page4"].ToString());
        }

        [Test]
        public void ComplexWriterKey()
        {
            AtTemplate template = new AtTemplate(
@"@output my\""output""
something
@end_output");
            template.Parse();

            Dictionary<string, TextWriter> writers = new Dictionary<string,TextWriter>()
            {
                { AtTemplate.MainOutputKey, new StringWriter() },
                { "my\\\"output\"", new StringWriter() }
            };

            template.Render(writers);

            Assert.AreEqual("something\r\n", writers["my\\\"output\""].ToString());
        }

        [Test]
        public void Compiler35()
        {
            AtTemplate template = new AtTemplate(
@"@reference System.Core.dll
", "v3.5");
            template.Parse();
        }

        [Test]
        public void Compiler20()
        {
            AtTemplate template = new AtTemplate(
@"@reference System.Data.dll
", "v2.0");
            template.Parse();
        }

        [Test, ExpectedException(typeof(TemplateCompilationException))]
        public void Compiler20_35Reference()
        {
            AtTemplate template = new AtTemplate(
@"@reference System.Core.dll
", "v2.0");
            template.Parse();
        }

        [Test]
        [TestCase(
@"@+ Fields
@section Fields
Hello DCG
@end_section")]
        [TestCase(
@"@+ Fields()
@section Fields
Hello DCG
@end_section")]
        public void SimpleSectionReference(string code)
        {
            AtTemplate template = new AtTemplate(code);
            template.Parse();

            Assert.That(template.Render(), Is.EqualTo("Hello DCG\r\n"));
        }

        [Test]
        public void SectionReferenceWithParameters()
        {
            AtTemplate template = new AtTemplate(
@"@! string name = ""Seth"";
@+ SayHello(name)
@section SayHello(name : string)
Hello @(name)
@end_section");
            template.Parse();

            Assert.That(template.Render(), Is.EqualTo("Hello Seth\r\n"));
        }

        [Test]
        public void SectionReferenceWithIndentation()
        {
            AtTemplate template = new AtTemplate(
@"    @+ SayHello
@section SayHello  
line1
line2
@end_section");
            template.Parse();

            Assert.That(template.Render(), Is.EqualTo("    line1\r\n    line2\r\n"));
        }

        [Test]
        public void SectionReferenceWithinSectionReference()
        {
            AtTemplate template = new AtTemplate(
@"    @+ Outer
@section Outer ()
line1
    @+ inner
@end_section
@section inner
inner line
@end_section");
            template.Parse();

            Assert.That(template.Render(), Is.EqualTo("    line1\r\n        inner line\r\n"));
        }

        [Test]
        public void SectionReferenceWithinAnOutput()
        {
            AtTemplate template = new AtTemplate(
@"@output o\1
    @+ Outer
@end_output
@section Outer ()
line1
    @+ inner
@end_section
@section inner
inner line
@end_section");
            template.Parse();

            IDictionary<string, TextWriter> writers = new Dictionary<string, TextWriter>() {
                { AtTemplate.MainOutputKey, new StringWriter() },
                { "o\\1", new StringWriter() }
            };

            template.Render(writers);

            Assert.That(
                writers["o\\1"].ToString(),
                Is.EqualTo("    line1\r\n        inner line\r\n"));
        }

        [Test]
        public void LineEndHandlingWithIndentation()
        {
            AtTemplate template = new AtTemplate(
@"    @code
    @text
    line1@
    continuation
    @end_text
@end_code");
            template.Parse();

            Assert.That(template.Render(), Is.EqualTo("    line1continuation\r\n"));
        }

        [Test]
        public void LineEndHandlingWithIndentation_WithinSection()
        {
            AtTemplate template = new AtTemplate(
@"    @+ MySection
@section MySection
@code
    @text
    line1@
    continuation
    @end_text
@end_code
@end_section");
            template.Parse();

            Assert.That(template.Render(), Is.EqualTo("    line1continuation\r\n"));
        }

        [Test]
        [TestCase("\r\n", TestName="Windows")]
        [TestCase("\r", TestName="Mac")]
        [TestCase("\n", TestName="Linux")]
        public void LineEnding(string lineEnding)
        {
            AtTemplate template = new AtTemplate(
                string.Format("line1{0}line2", lineEnding));
            template.Parse();

            Assert.That(template.Render(), Is.EqualTo(string.Format("line1{0}line2{0}", lineEnding)));
        }
    }
}
