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
using Cavingdeep.Dcg.At.Parsing;
using Cavingdeep.ObjectIntruder;
using NUnit.Framework;

namespace Cavingdeep.Tests.Dcg
{
    [TestFixture]
    public class SimpleAtParserTest
    {
        [Test]
        public void Head()
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(
                @"@# comment
@reference System.Windows.Forms.dll
@import System.Windows.Forms
@param name: string
@
@global
string firstName;
string lastName;
@end_global");
            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");

            AtTemplateAst ast = parserIntruder.ReadField<AtTemplateAst>("ast");

            Assert.That(ast.Head.References.Count, Is.EqualTo(3));
            Assert.That(ast.Head.Imports.Count, Is.EqualTo(1));
            Assert.That(ast.Head.Parameters.Count, Is.EqualTo(1));
            Assert.That(ast.Head.Global.Length, Is.GreaterThan(0));
        }

        [TestCase("hello world", "hello world\r\n")]
        [TestCase("line@", "line")]
        [TestCase("a @ c", "a @ c\r\n")]
        [TestCase("a @@ c", "a @ c\r\n")]
        [TestCase("a @@code c", "a @code c\r\n")]
        [TestCase("a \"\" b", "a \"\" b\r\n")]
        public void StaticTexts(string text, string expected)
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(text);

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");

            AtTemplateAst ast = parserIntruder.ReadField<AtTemplateAst>("ast");
            Assert.That(((StaticText) ast.Body.Directives[0]).Value, Is.EqualTo(expected));
        }

        [TestCase("@(1 + 1)", "1 + 1")]
        [TestCase("@(1+1)@", "1+1")]
        [TestCase("a @(1+1)", "1+1")]
        [TestCase("a @(1+1) c", "1+1")]
        [TestCase("@(1+1) c", "1+1")]
        public void Evaluations(string text, string expected)
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(text);

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");

            AtTemplateAst ast = parserIntruder.ReadField<AtTemplateAst>("ast");

            Assert.That(((Evaluation) ast.Body.Directives[1]).Expression, Is.EqualTo(expected));
        }

        [TestCase("@{\r\n@}")]
        [TestCase("12@{\r\n@}")]
        [TestCase("@{\r\n@}123")]
        public void Between(string text)
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(text);

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");

            AtTemplateAst ast = parserIntruder.ReadField<AtTemplateAst>("ast");

            Assert.That(ast.Body.Directives[1] is Between);
        }

        [TestCase(@"@code
int a = 1;
@end_code", "int a = 1;")]
        [TestCase(@"@code
//
@end_code", "//")]
        public void Code(string text, string expected)
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(text);

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");

            AtTemplateAst ast = parserIntruder.ReadField<AtTemplateAst>("ast");

            Assert.That(
                ((DynamicText) ((Code) ast.Body.Directives[0]).Directives[0]).Value,
                Is.EqualTo(expected));
        }

        [TestCase("@code\r\n@text\r\n@end_text\r\n@end_code")]
        public void Text(string text)
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(text);

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");

            AtTemplateAst ast = parserIntruder.ReadField<AtTemplateAst>("ast");

            Assert.That(
                ((Code) ast.Body.Directives[0]).Directives[0] is Cavingdeep.Dcg.At.Parsing.Text);
        }

        [Test]
        public void InnerDirectives()
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(@"@# comment
@code
    @text
        @code
        @end_code
    @end_text
@end_code");

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");

            AtTemplateAst ast = parserIntruder.ReadField<AtTemplateAst>("ast");

            Assert.That(
                ((Cavingdeep.Dcg.At.Parsing.Text) ((Code) ast.Body.Directives[0]).Directives[0]).Directives[0] is Code);
        }

        [Test]
        public void Execution()
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader("@! int a = 1;");

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");

            AtTemplateAst ast = parserIntruder.ReadField<AtTemplateAst>("ast");

            Assert.That(((Execution) ast.Body.Directives[0]).Statement, Is.EqualTo(" int a = 1;"));
        }

        [Test]
        public void MultiEvaluation()
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(@"@global
    string Foo() { return ""1" + "\r\n" + @"2""; }
@end_global
    @= Foo()");

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");

            AtTemplateAst ast = parserIntruder.ReadField<AtTemplateAst>("ast");

            Assert.That(((MultiLineEvaluation) ast.Body.Directives[0]).Expression, Is.EqualTo(" Foo()"));
            Assert.That(((MultiLineEvaluation) ast.Body.Directives[0]).Spaces, Is.EqualTo("    "));
        }

        [Test]
        public void Output()
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(@"@output key
123
@end_output");

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");

            AtTemplateAst ast = parserIntruder.ReadField<AtTemplateAst>("ast");

            Assert.That(((Output) ast.Body.Directives[0]).WriterKey, Is.EqualTo("key"));
        }
    }
}
