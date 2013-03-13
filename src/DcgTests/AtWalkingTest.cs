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
using Cavingdeep.Dcg;
using Cavingdeep.Dcg.At.Parsing;
using Cavingdeep.Dcg.At.Walkers;
using NUnit.Framework;
using Cavingdeep.Dcg.At;
using System.IO;
using Cavingdeep.ObjectIntruder;

namespace Cavingdeep.Tests.Dcg
{
    [TestFixture]
    public class AtWalkingTest
    {
        [Test]
        public void SingleStaticText()
        {
            AtTemplateAst ast = new AtTemplateAst();
            ast.Head.Parameters.Add(new Parameter("myName", "string"));
            ast.Body.Directives.Add(new StaticText("Hello World!\r\n", 2));

            SourceGenerator generator = new SourceGenerator();
            generator.Ast = ast;
            generator.Debugging = true;
            generator.Walk();

            Console.WriteLine(generator.SourceCode);
        }

        [Test]
        public void Indentation()
        {
            AtParser parser = new AtParser();
            parser.Reader = new StringReader(
@"{
    @code
        int i = 0;
        @text
        123
        @end_text
    @end_code
}");
            parser.Debug = true;

            Intruder parserIntruder = new Intruder(parser);
            parserIntruder.CallMethod<object>("InitParsing");
            parserIntruder.CallMethod<object>("BuildAst");

            SourceGenerator generator = new SourceGenerator();
            generator.Ast = parserIntruder.ReadField<AtTemplateAst>("ast");
            generator.Debugging = true;
            generator.Walk();

            Console.WriteLine(generator.SourceCode);
        }
    }
}
