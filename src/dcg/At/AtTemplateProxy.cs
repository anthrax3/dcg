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
using System.Reflection;
using System.Text;

namespace Cavingdeep.Dcg.At
{
    /// <summary>
    /// An <see cref="AtTemplate"/> that runs on another application
    /// domain.
    /// </summary>
    [Serializable]
    public class AtTemplateProxy : IAtTemplate, IDisposable
    {
        private AppDomain domain;
        private AtTemplate template;
        private bool isDisposed = false;

        /// <summary>
        /// Creates a template with specified text that
        /// runs on another application domain.
        /// </summary>
        /// <param name="text">Template text.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="text"/> is null or empty.
        /// </exception>
        public AtTemplateProxy(string text) : this(text, (string) null)
        {
        }

        /// <summary>
        /// Creates a template with specified text that
        /// runs on another application domain.
        /// </summary>
        /// <param name="text">Template text.</param>
        /// <param name="compilerVersion">Indicates the compiler version to use.
        /// Such as "v3.5" or "v2.0".</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="text"/> is null or empty.
        /// </exception>
        public AtTemplateProxy(string text, string compilerVersion)
        {
            if (text == null)
            {
                throw new ArgumentNullException("templateText");
            }

            if (compilerVersion == null)
            {
                compilerVersion = String.Empty;
            }

            CreateDomain();

            this.template =
                (AtTemplate) this.domain.CreateInstanceFromAndUnwrap(
                    Assembly.GetExecutingAssembly().CodeBase,
                    typeof(AtTemplate).FullName,
                    false,
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new object[] { text, compilerVersion },
                    null,
                    null,
                    null);
        }

        /// <summary>
        /// Creates a template with reading from a file that
        /// runs on another application domain.
        /// </summary>
        /// <param name="templateFile">Template file.</param>
        /// <param name="encoding">Encoding used to read
        /// file content, if NULL the system default encoding
        /// is used.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="templateFile"/> is null or empty.
        /// </exception>
        public AtTemplateProxy(string templateFile, Encoding encoding)
            : this(templateFile, encoding, (string) null)
        {
        }

        /// <summary>
        /// Creates a template with reading from a file that
        /// runs on another application domain.
        /// </summary>
        /// <param name="templateFile">Template file.</param>
        /// <param name="encoding">Encoding used to read
        /// file content, if NULL the system default encoding
        /// is used.</param>
        /// <param name="compilerVersion">Indicates the compiler version to use.
        /// Such as "v3.5" or "v2.0".</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="templateFile"/> is null or empty.
        /// </exception>
        public AtTemplateProxy(string templateFile, Encoding encoding, string compilerVersion)
        {
            if (string.IsNullOrEmpty(templateFile))
            {
                throw new ArgumentNullException("templateFile");
            }

            if (compilerVersion == null)
            {
                compilerVersion = String.Empty;
            }

            CreateDomain();

            this.template =
                (AtTemplate) this.domain.CreateInstanceFromAndUnwrap(
                    Assembly.GetExecutingAssembly().CodeBase,
                    typeof(AtTemplate).FullName,
                    false,
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new object[] { templateFile, encoding, compilerVersion },
                    null,
                    null,
                    null);
        }

        /// <summary>
        /// Context information (parameter values) template
        /// requires.
        /// </summary>
        public object[] Context
        {
            get
            {
                return this.template.Context;
            }

            set
            {
                this.template.Context = value;
            }
        }

        /// <summary>
        /// Indicates if it is necessary to generate debugging
        /// information for template.
        /// </summary>
        public bool Debug
        {
            get
            {
                return this.template.Debug;
            }

            set
            {
                this.template.Debug = value;
            }
        }

        /// <summary>
        /// Generated source code for template.
        /// </summary>
        public string SourceCode
        {
            get
            {
                return this.template.SourceCode;
            }
        }

        /// <summary>
        /// Generated assembly path.
        /// </summary>
        public string GeneratedAssemblyPath
        {
            get
            {
                return this.template.GeneratedAssemblyPath;
            }
        }

        /// <summary>
        /// Parse template before rendering.
        /// </summary>
        /// <remarks>
        /// Once parsed, you can render as much times as you want.
        /// </remarks>
        /// <exception cref="TemplateParsingException">
        /// If an error ocurrs during parsing such as wrong
        /// syntax used.
        /// </exception>
        /// <exception cref="TemplateCompilationException">
        /// If a bad dynamic language syntax is found such as
        /// a bad C# code syntax.
        /// </exception>
        public void Parse()
        {
            this.template.Parse();
        }

        /// <summary>
        /// Renders template as single output template.
        /// </summary>
        /// <returns>Output of template.</returns>
        /// <exception cref="TemplateRuntimeException">
        /// If an error ocurrs during rendering such as
        /// a C# runtime exception.
        /// </exception>
        public string Render()
        {
            return this.template.Render();
        }

        /// <summary>
        /// Renders template by passsing explicitly a writer.
        /// </summary>
        /// <param name="writer">Writer used to write template
        /// output.</param>
        /// <exception cref="TemplateRuntimeException">
        /// If an error ocurrs during rendering such as
        /// a C# runtime exception.
        /// </exception>
        public void Render(TextWriter writer)
        {
            this.template.Render(writer);
        }

        /// <summary>
        /// Renders template as multi-output template.
        /// </summary>
        /// <param name="writers">Dictionary of writers
        /// that template uses for output.</param>
        /// <exception cref="TemplateRuntimeException">
        /// If an error ocurrs during rendering such as
        /// a C# runtime exception.
        /// </exception>
        public void Render(IDictionary<string, System.IO.TextWriter> writers)
        {
            this.template.Render(writers);
        }

        /// <summary>
        /// Creates a new instance of template
        /// generation object.
        /// </summary>
        /// <returns>New instance of template
        /// generation object.</returns>
        public IAtTemplateInstance NewInstance()
        {
            return this.template.NewInstance();
        }

        /// <summary>
        /// Unload application domain created by this instance and
        /// clean temp files.
        /// </summary>
        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            string assemblyFile = this.template.GeneratedAssemblyPath;

            AppDomain.Unload(this.domain);
            this.DeleteTempFiles(assemblyFile);
            this.UnRegisterTempFiles(assemblyFile);

            this.isDisposed = true;
        }

        private void CreateDomain()
        {
            this.domain = AppDomain.CreateDomain(
                "dcg",
                AppDomain.CurrentDomain.Evidence,
                new AppDomainSetup()
                {
                    ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                });
        }

        private void DeleteTempFiles(string assemblyFile)
        {
            if (assemblyFile != null)
            {
                try
                {
                    File.Delete(assemblyFile);
                    File.Delete(
                        assemblyFile.Substring(0, assemblyFile.Length - 4)
                        + ".pdb");
                    File.Delete(assemblyFile + ".cs");
                }
                catch
                {
                    // Ignore undeletable files.
                }
            }
        }

        private void UnRegisterTempFiles(string assemblyFile)
        {
            if (assemblyFile != null)
            {
                TempFileManager.UnRegisterFile(assemblyFile);
                TempFileManager.UnRegisterFile(
                    assemblyFile.Substring(0, assemblyFile.Length - 4)
                    + ".pdb");
                TempFileManager.UnRegisterFile(assemblyFile + ".cs");
            }
        }
    }
}
