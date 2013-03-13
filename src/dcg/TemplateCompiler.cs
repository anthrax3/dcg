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
using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using Microsoft.CSharp;
using Assertion = System.Diagnostics.Debug;
using System.Collections.Generic;

namespace Cavingdeep.Dcg
{
    /// <summary>
    /// Compiles source code for a template.
    /// </summary>
    [Serializable]
    internal class TemplateCompiler
    {
        private bool debug;
        private string sourceCode;
        private string outputFile;
        private string[] references;
        private string compilerVersion;

        public TemplateCompiler() : this(null)
        {
        }

        public TemplateCompiler(string compilerVersion)
        {
            this.compilerVersion = compilerVersion;
        }

        public bool Debug
        {
            get
            {
                return this.debug;
            }

            set
            {
                this.debug = value;
            }
        }

        public string SourceCode
        {
            get
            {
                return this.sourceCode;
            }

            set
            {
                this.sourceCode = value;
            }
        }

        public string OutputFile
        {
            get
            {
                return this.outputFile;
            }

            set
            {
                this.outputFile = value;
            }
        }

        public string[] References
        {
            get
            {
                return this.references;
            }

            set
            {
                this.references = value;
            }
        }

        public void Compile()
        {
            Assertion.Assert(
                !string.IsNullOrEmpty(this.sourceCode),
                "sourceCode cannot be null or empty.");
            Assertion.Assert(
                !string.IsNullOrEmpty(this.outputFile),
                "outputFile cannot be null or empty.");

            CompilerParameters options = new CompilerParameters();
            options.GenerateExecutable = false;
            options.GenerateInMemory = false;
            options.IncludeDebugInformation = this.debug;
            options.OutputAssembly = this.outputFile;

            if ((this.references != null) && (this.references.Length > 0))
            {
                options.ReferencedAssemblies.AddRange(this.references);
            }

            CompilerResults result;
            CodeDomProvider provider;

            if (string.IsNullOrEmpty(this.compilerVersion))
            {
                provider = new CSharpCodeProvider();
            }
            else
            {
                provider = new CSharpCodeProvider(
                    new Dictionary<string, string>() {{"CompilerVersion", this.compilerVersion}});
            }

            if (this.debug)
            {
                using (StreamWriter writer = new StreamWriter(
                    this.outputFile + ".cs", false, Encoding.UTF8))
                {
                    writer.Write(this.sourceCode);
                }

                result = provider.CompileAssemblyFromFile(options, this.outputFile + ".cs");
            }
            else
            {
                result = provider.CompileAssemblyFromSource(options, this.sourceCode);
            }

            TempFileManager.RegisterFile(this.outputFile);

            if (this.debug)
            {
                TempFileManager.RegisterFile(
                    this.outputFile.Substring(0, this.outputFile.Length - 4)
                    + ".pdb");
                TempFileManager.RegisterFile(this.outputFile + ".cs");
            }

            if (result.Errors.Count > 0)
            {
                throw new TemplateCompilationException(result.Errors);
            }
        }
    }
}