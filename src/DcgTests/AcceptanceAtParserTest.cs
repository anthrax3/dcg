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
using Cavingdeep.ObjectIntruder;
using NUnit.Framework;
using Cavingdeep.Dcg.At.Parsing;

namespace Cavingdeep.Tests.Dcg
{
    [TestFixture]
    public class AcceptanceAtParserTest
    {
        [Test]
        public void First()
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(
                @"@# comment
@param name: string
@param age: int
@
Hello @(name), @{
    if (age >= 18)
    {
        @text
        you are adult.
        @end_text
    }
@}");
            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");

            AtTemplateAst ast = parserIntruder.ReadField<AtTemplateAst>("ast");

            Assert.That(ast.Head.Parameters.Count, Is.EqualTo(2));
            Assert.That(ast.Head.Parameters[0].Name, Is.EqualTo("name"));
            Assert.That(ast.Head.Parameters[0].Type, Is.EqualTo("string"));
            Assert.That(ast.Head.Parameters[1].Name, Is.EqualTo("age"));
            Assert.That(ast.Head.Parameters[1].Type, Is.EqualTo("int"));

            Assert.That(((StaticText) ast.Body.Directives[0]).Value, Is.EqualTo("Hello "));
            Assert.That(((Evaluation) ast.Body.Directives[1]).Expression, Is.EqualTo("name"));
            Assert.That(((StaticText) ast.Body.Directives[2]).Value, Is.EqualTo(", "));
            Assert.That(((DynamicText) ((Between) ast.Body.Directives[3]).Directives[0]).Value, Is.EqualTo("    if (age >= 18)"));
            Assert.That(((DynamicText) ((Between) ast.Body.Directives[3]).Directives[1]).Value, Is.EqualTo("    {"));
            Assert.That(((StaticText) ((Cavingdeep.Dcg.At.Parsing.Text) ((Between) ast.Body.Directives[3]).Directives[2]).Directives[0]).Value, Is.EqualTo("you are adult.\r\n"));
            Assert.That(((DynamicText) ((Between) ast.Body.Directives[3]).Directives[3]).Value, Is.EqualTo("    }"));
            Assert.That(((StaticText) ast.Body.Directives[4]).Value, Is.EqualTo("\r\n"));
        }
    }
}
