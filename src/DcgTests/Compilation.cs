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
using Cavingdeep.Dcg;
using Cavingdeep.ObjectIntruder;
using NUnit.Framework;

namespace Cavingdeep.Tests.Dcg
{
    [TestFixture]
    public class Compilation
    {
        private const string OutFile = "output.dll";

        private Intruder compiler;

        [SetUp]
        public void CreateCompiler()
        {
            Intruder compilerType = new Intruder(Type.GetType("Cavingdeep.Dcg.TemplateCompiler, Cavingdeep.Dcg"));
            this.compiler = new Intruder(compilerType.Create());
            this.compiler.WriteProperty("OutputFile", OutFile);
        }

        [TearDown]
        public void Cleaning()
        {
            File.Delete(OutFile);
            File.Delete(OutFile.Substring(0, OutFile.Length - 4) + ".pdb");
        }

        [Test]
        public void SuccessfulCompilation()
        {
            string sourceCode = @"namespace A {
   public class AA {
      public AA() {}
   }
}";
            this.compiler.WriteProperty("SourceCode", sourceCode);
            this.compiler.WriteProperty("Debug", false);
            this.compiler.CallMethod<object>("Compile");

            Assert.IsTrue(File.Exists(OutFile));
        }

        [Test]
        public void SuccessfulDebugCompilation()
        {
            string sourceCode = @"namespace A {
   public class AA {
      public AA() {}
   }
}";
            this.compiler.WriteProperty("SourceCode", sourceCode);
            this.compiler.WriteProperty("Debug", true);
            this.compiler.CallMethod<object>("Compile");

            Assert.IsTrue(File.Exists(OutFile));
            Assert.IsTrue(File.Exists(
                  OutFile.Substring(0, OutFile.Length - 4) + ".pdb"));
        }

        [Test]
        public void CompilationWithErrors()
        {
            string sourceCode = "1 + 1";
            try
            {
                this.compiler.WriteProperty("SourceCode", sourceCode);
                this.compiler.CallMethod<object>("Compile");
                Assert.Fail("Compilation must fail.");
            }
            catch (TemplateCompilationException ex)
            {
                Assert.AreEqual(1, ex.Errors.Count);
            }
        }

        [Test]
        public void CompilationWithReferences()
        {
            string sourceCode = @"namespace A {
   public class AA {
      public AA() {}
   }
}";
            this.compiler.WriteProperty("SourceCode", sourceCode);
            this.compiler.WriteProperty("References", new string[] { "System.Windows.Forms.dll" });
            this.compiler.CallMethod<object>("Compile");

            Assert.IsTrue(File.Exists(OutFile));
        }
    }
}