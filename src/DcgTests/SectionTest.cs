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
using System.IO;
using Cavingdeep.Dcg.At;
using Cavingdeep.Dcg.At.Parsing;
using Cavingdeep.Dcg.At.Walkers;
using Cavingdeep.ObjectIntruder;
using NUnit.Framework;
using Cavingdeep.Dcg;

namespace Cavingdeep.Tests.Dcg
{
    [TestFixture]
    public class SectionTest
    {
        [Test]
        public void BasicSectionDefinition()
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(
@"line1
@section sec
line2
@end_section
");

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");

            AtTemplateAst ast = parserIntruder.ReadField<AtTemplateAst>("ast");
            SectionDefinition section = (SectionDefinition) ast.Body.Directives[1];

            Assert.That(section.Name, Is.EqualTo("sec"));
            Assert.That(section.Directives.Count, Is.EqualTo(1));
        }

        [Test]
        public void TwoSectionDefinition()
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(
@"line1
@section sec
line2
@end_section
@section sec2
line3
line4
@end_section
");

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");

            AtTemplateAst ast = parserIntruder.ReadField<AtTemplateAst>("ast");
            SectionDefinition section1 = (SectionDefinition) ast.Body.Directives[1];
            SectionDefinition section2 = (SectionDefinition) ast.Body.Directives[2];

            Assert.That(section1.Name, Is.EqualTo("sec"));
            Assert.That(section1.Directives.Count, Is.EqualTo(1));

            Assert.That(section2.Name, Is.EqualTo("sec2"));
            Assert.That(section2.Directives.Count, Is.EqualTo(2));
        }

        [Test, ExpectedException(typeof(TemplateParsingException))]
        public void TwoSectionDefinition_WithSameName()
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(
@"line1
@section sec
line2
@end_section
@section sec
line3
@end_section
");

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");
        }

        [Test, ExpectedException(typeof(TemplateParsingException))]
        public void InnerSectionDefinition()
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(
@"line1
@section sec
line2
@section sec2
line3
@end_section
@end_section
");

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");
        }

        [Test, ExpectedException(typeof(TemplateParsingException))]
        public void NonTopLevelSectionDefinition()
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(
@"@code
    @text
    @section sec
    line2
    @end_section
    @end_text
@end_code");

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");
        }

        [Test]
        [TestCase(
@"@section Foo()
line2
@end_section", 0)]
        [TestCase(
@"@section Foo(param1: int)
line2
@end_section", 1)]
        [TestCase(
@"@section Foo(param1: int, param2: string)
line2
@end_section", 2)]
        public void WithParameters(string code, int paramCount)
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(code);

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");

            AtTemplateAst ast = parserIntruder.ReadField<AtTemplateAst>("ast");
            SectionDefinition section = (SectionDefinition) ast.Body.Directives[0];

            Assert.That(section.Name, Is.EqualTo("Foo"));
            Assert.That(section.Parameters.Count, Is.EqualTo(paramCount));
        }

        [Test, ExpectedException(typeof(TemplateParsingException))]
        [TestCase(
@"line1
@sectionsec
line2
@end_section")]
        [TestCase(
@"line1
@section sec(
line2
@end_section")]
        [TestCase(
@"line1
@section 12sec()
line2
@end_section")]
        public void BadSectionDefinition(string code)
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(code);

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");
        }

        [Test]
        public void SectionRefWithNoParameters()
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(
@"class A {
    @+ Fields  
}
@section Fields
line1
@end_section");

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");

            AtTemplateAst ast = parserIntruder.ReadField<AtTemplateAst>("ast");
            SectionReference sectionRef = (SectionReference) ast.Body.Directives[1];

            Assert.That(sectionRef.Name, Is.EqualTo("Fields"));
        }

        [Test]
        public void SectionRefWithParameters()
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(
@"class A {
    @+ Fields(fields)
}
@section Fields(f: string[])
@code
    foreach (var field in f)
    {
        @text
        private string @(field);
        @end_text
    }
@end_code

@end_section");

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");

            AtTemplateAst ast = parserIntruder.ReadField<AtTemplateAst>("ast");
            SectionReference sectionRef = (SectionReference) ast.Body.Directives[1];

            Assert.That(sectionRef.Name, Is.EqualTo("Fields"));
            Assert.That(sectionRef.Rest, Is.EqualTo("(fields)"));
        }

        [Test, ExpectedException(typeof(TemplateParsingException))]
        public void BadSectionRef()
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(
@"class A {
    @+ 12Fields
}
@section Fields
line1
@end_section");

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");
        }
    }
}
