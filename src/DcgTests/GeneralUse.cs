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
using System.Text;
using Cavingdeep.Dcg;
using Cavingdeep.Dcg.At;
using NUnit.Framework;

namespace Cavingdeep.Tests.Dcg
{
    [TestFixture]
    public class GeneralUse
    {
        private const string TemplateFile = "array.txt";

        [TestFixtureSetUp]
        public void CreateTestTemplateFile()
        {
            string content = @"@param p1: string
@param p2: string
1 + 1 = @(1+1)";

            using (StreamWriter writer =
                new StreamWriter(TemplateFile, false, Encoding.Default))
            {
                writer.Write(content);
            }
        }

        [TestFixtureTearDown]
        public void DeleteTestTemplateFile()
        {
            File.Delete(TemplateFile);
        }

        #region Normal Usage

        [Test]
        [Category("Normal Usage")]
        public void SimpleRender()
        {
            ITemplate template =
                new AtTemplate(TemplateFile, Encoding.Default);

            template.Parse();

            template.Context = new object[] { "arg1", "arg2" };

            Assert.AreEqual("1 + 1 = 2\r\n", template.Render());
        }

        [Test]
        [Category("Normal Usage")]
        public void WriterRender()
        {
            ITemplate template =
                new AtTemplate(TemplateFile, Encoding.Default);

            template.Parse();

            template.Context = new object[] { "arg1", "arg2" };

            StringWriter writer = new StringWriter();
            template.Render(writer);
            Assert.AreEqual("1 + 1 = 2\r\n", writer.ToString());
        }

        [Test]
        [Category("Normal Usage")]
        public void GenerateAssembly()
        {
            ITemplate template =
                new AtTemplate(TemplateFile, Encoding.Default);

            template.Parse();

            Assert.IsNotNull(template.GeneratedAssemblyPath);
        }

        #endregion

        #region Usage with Debug

        [Test]
        [Category("Debug Usage")]
        public void SimpleRenderWithDebug()
        {
            ITemplate template =
                new AtTemplate(TemplateFile, Encoding.Default);
            template.Debug = true;

            template.Parse();

            template.Context = new object[] { "arg1", "arg2" };

            Assert.AreEqual("1 + 1 = 2\r\n", template.Render());
        }

        [Test]
        [Category("Debug Usage")]
        public void WriterRenderWithDebug()
        {
            ITemplate template =
                new AtTemplate(TemplateFile, Encoding.Default);
            template.Debug = true;

            template.Parse();

            template.Context = new object[] { "arg1", "arg2" };

            StringWriter writer = new StringWriter();
            template.Render(writer);
            Assert.AreEqual("1 + 1 = 2\r\n", writer.ToString());
        }

        [Test]
        [Category("Debug Usage")]
        [Explicit]
        public void GenerateSourceCodeWithDebug()
        {
            ITemplate template =
                new AtTemplate(TemplateFile, Encoding.Default);
            template.Debug = true;

            template.Parse();

            Console.WriteLine(template.SourceCode);
        }

        [Test]
        [Category("Debug Usage")]
        public void GenerateAssemblyWithDebug()
        {
            ITemplate template =
                new AtTemplate(TemplateFile, Encoding.Default);
            template.Debug = true;

            template.Parse();

            Assert.IsNotNull(template.GeneratedAssemblyPath);
            Assert.IsTrue(
                File.Exists(
                    template.GeneratedAssemblyPath.Substring(
                        0, template.GeneratedAssemblyPath.Length-4) + ".pdb"));
        }

        #endregion
    }
}
