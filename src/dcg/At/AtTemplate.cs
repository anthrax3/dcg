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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Assertion = System.Diagnostics.Debug;

namespace Cavingdeep.Dcg.At
{
    /// <summary>
    /// At language based template.
    /// </summary>
    /// <remarks>
    /// <para>At language is a powerful yet easy to understand template
    /// language, here is an usage example.</para>
    /// <example>
    /// Sample hello.al template
    /// <code>
    /// @param name: string
    /// @
    /// Hello @(name).
    /// </code>
    /// Sample usage
    /// <code>
    /// public static void Main()
    /// {
    ///     IAtTemplate template = new AtTemplate("hello.al", null);
    ///     template.Parse();
    ///     template.Context = new object[] {"Teddy"};
    /// 
    ///     Console.WriteLine(template.Render());
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    [Serializable]
    public class AtTemplate : MarshalByRefObject, IAtTemplate, ITemplate
    {
        /// <summary>
        /// Main render output key.
        /// </summary>
        /// <remarks>
        /// Use this when you need to reference the main
        /// output in multi-output rendering.
        /// </remarks>
        public const string MainOutputKey = "_main_";

        private Encoding encoding;
        private string outFilePath;
        private Type generatorType;
        private AtParser parser = new AtParser();
        private TemplateCompiler compiler;
        private AtTemplateInstance defaultInstance;

        /// <summary>
        /// Creates a template with specified text.
        /// </summary>
        /// <param name="text">Template text.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="text"/> is null or empty.
        /// </exception>
        public AtTemplate(string text) : this(text, (string) null)
        {
        }

        /// <summary>
        /// Creates a template with specified text.
        /// </summary>
        /// <param name="text">Template text.</param>
        /// <param name="compilerVersion">Indicates the compiler version to use.
        /// Such as "v3.5" or "v2.0".</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="text"/> is null or empty.
        /// </exception>
        public AtTemplate(string text, string compilerVersion)
        {
            if (text == null)
            {
                throw new ArgumentNullException("templateText");
            }

            if (string.IsNullOrEmpty(compilerVersion))
            {
                this.compiler = new TemplateCompiler();
            }
            else
            {
                this.compiler = new TemplateCompiler(compilerVersion);
            }

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\n')
                {
                    this.parser.LineEnding = "\n";
                    break;
                }
                else if (c == '\r')
                {
                    if (i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        this.parser.LineEnding = "\r\n";
                    }
                    else
                    {
                        this.parser.LineEnding = "\r";
                    }
                    break;
                }
            }

            this.parser.Reader = new StringReader(text);
        }

        /// <summary>
        /// Creates a template with reading from a file.
        /// </summary>
        /// <param name="templateFile">Template file.</param>
        /// <param name="encoding">Encoding used to read
        /// file content, if NULL the system default encoding
        /// is used.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="templateFile"/> is null or empty.
        /// </exception>
        public AtTemplate(string templateFile, Encoding encoding)
            : this(templateFile, encoding, (string) null)
        {
        }

        /// <summary>
        /// Creates a template with reading from a file.
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
        public AtTemplate(string templateFile, Encoding encoding, string compilerVersion)
        {
            if (string.IsNullOrEmpty(templateFile))
            {
                throw new ArgumentNullException("templateFile");
            }

            if (string.IsNullOrEmpty(compilerVersion))
            {
                this.compiler = new TemplateCompiler();
            }
            else
            {
                this.compiler = new TemplateCompiler(compilerVersion);
            }

            this.parser.TemplateFile = Path.GetFullPath(templateFile);
            this.encoding = (encoding == null ? Encoding.Default : encoding);
        }

        /// <summary>
        /// Context information (parameter values) template
        /// requires.
        /// </summary>
        public object[] Context
        {
            get
            {
                return this.defaultInstance.Context;
            }

            set
            {
                this.defaultInstance.Context = value;
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
                return this.parser.Debug;
            }

            set
            {
                this.parser.Debug = value;
            }
        }

        /// <summary>
        /// Generated source code for template.
        /// </summary>
        public string SourceCode
        {
            get
            {
                return this.parser.SourceCode;
            }
        }

        /// <summary>
        /// Generated assembly path.
        /// </summary>
        public string GeneratedAssemblyPath
        {
            get
            {
                return this.outFilePath;
            }
        }

        /// <summary>
        /// Initializing and controlling life time.
        /// </summary>
        public override object InitializeLifetimeService()
        {
            return null;
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
            this.defaultInstance = new AtTemplateInstance(this);

            if (!string.IsNullOrEmpty(this.parser.TemplateFile))
            {
                using (StreamReader reader = new StreamReader(this.parser.TemplateFile, this.encoding))
                {
                    int c;
                    while ((c = reader.Read()) >= 0)
                    {
                        if ((char) c == '\n')
                        {
                            this.parser.LineEnding = "\n";
                            break;
                        }
                        else if ((char) c == '\r')
                        {
                            if ((c = reader.Read()) >= 0 && (char) c == '\n')
                            {
                                this.parser.LineEnding = "\r\n";
                            }
                            else
                            {
                                this.parser.LineEnding = "\r";
                            }
                            break;
                        }
                    }
                }

                this.parser.Reader = new StreamReader(this.parser.TemplateFile, this.encoding);
            }

            try
            {
                ParseTemplate();
                Compile();
                LoadAssembly();
            }
            finally
            {
                this.parser.Reader.Close();
            }
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
            return this.defaultInstance.Render();
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
            this.defaultInstance.Render(writer);
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
        public void Render(IDictionary<string, TextWriter> writers)
        {
            this.defaultInstance.Render(writers);
        }

        /// <summary>
        /// Creates a new instance of template
        /// generation object.
        /// </summary>
        /// <returns>New instance of template
        /// generation object.</returns>
        public IAtTemplateInstance NewInstance()
        {
            return new AtTemplateInstance(this);
        }

        void IDisposable.Dispose()
        {
            // No disposable behavior needed.
        }

        private void LoadAssembly()
        {
            Assembly asm = Assembly.LoadFile(this.outFilePath);
            this.generatorType = asm.GetType("Cavingdeep.Generated.Generator", true);
        }

        private void Compile()
        {
            string outFilePath = GetOutputFileName();

            this.compiler.SourceCode = this.parser.SourceCode;
            this.compiler.References = this.parser.References;
            this.compiler.Debug = this.parser.Debug;
            this.compiler.OutputFile = outFilePath;

            try
            {
                this.compiler.Compile();
            }
            catch (TemplateCompilationException ex)
            {
                bool fileAccessDenied = false;

                foreach (CompilerError error in ex.Errors)
                {
                    // Output file access denied, means it is been locked.
                    if (error.ErrorNumber == "CS0016")
                    {
                        fileAccessDenied = true;
                        break;
                    }
                }

                if (fileAccessDenied)
                {
                    // Compile again with a different file name.
                    Compile();
                    return;
                }
                else
                {
                    throw;
                }
            }

            File.SetAttributes(outFilePath, FileAttributes.Hidden);

            this.outFilePath = outFilePath;
        }

        private string GetOutputFileName()
        {
            if (!string.IsNullOrEmpty(this.parser.TemplateFile))
            {
                return GetNameFromTemplateFile();
            }
            else
            {
                return Path.GetTempFileName();
            }
        }

        private string GetNameFromTemplateFile()
        {
            int count = 0;

            string[] files = Directory.GetFiles(
                Path.GetDirectoryName(this.parser.TemplateFile),
                "*.*.generated.dll",
                SearchOption.TopDirectoryOnly);

            if (files.Length > 0)
            {
                string lastFile = Path.GetFileName(files[files.Length - 1]);
                count = Convert.ToInt32(lastFile.Substring(0, lastFile.IndexOf('.')));
            }

            while (File.Exists(GetFileNameFromCount(count++)))
            {
            }

            return GetFileNameFromCount(count - 1);
        }

        private string GetFileNameFromCount(int count)
        {
            return Path.Combine(
                Path.GetDirectoryName(this.parser.TemplateFile),
                count + "." + Path.GetFileName(this.parser.TemplateFile) + ".generated.dll");
        }

        private void ParseTemplate()
        {
            try
            {
                this.parser.Parse();
            }
            catch (TemplateParsingException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new TemplateParsingException(ex.Message, ex);
            }
        }

        private void FillRuntimeError(TemplateRuntimeException ex)
        {
            if (this.parser.TemplateFile != null)
            {
                ex.SourceFileName = this.parser.TemplateFile;
            }
        }

        private class AtTemplateInstance : MarshalByRefObject, IAtTemplateInstance
        {
            private AtTemplate template;
            private object[] context;
            private object generatorInstace;

            public AtTemplateInstance(AtTemplate template)
            {
                Assertion.Assert(template != null, "template cannot be null.");

                this.template = template;
            }

            public IAtTemplate Template
            {
                get
                {
                    return this.template;
                }
            }

            public object[] Context
            {
                get
                {
                    return this.context;
                }

                set
                {
                    this.context = value;
                }
            }

            private object GeneratorInstance
            {
                get
                {
                    if (this.generatorInstace == null)
                    {
                        this.generatorInstace = Activator.CreateInstance(
                            this.template.generatorType);
                    }

                    return this.generatorInstace;
                }
            }

            /// <summary>
            /// Initializing and controlling life time.
            /// </summary>
            public override object InitializeLifetimeService()
            {
                return null;
            }

            public string Render()
            {
                StringWriter writer = new StringWriter();
                Render(writer);
                return writer.ToString();
            }

            public void Render(TextWriter writer)
            {
                if (writer == null)
                {
                    throw new ArgumentNullException("writer");
                }

                IDictionary<string, TextWriter> writers =
                    new Dictionary<string, TextWriter>();

                writers.Add(AtTemplate.MainOutputKey, writer);

                this.Render(writers);
            }

            public void Render(IDictionary<string, TextWriter> writers)
            {
                if (writers == null)
                {
                    throw new ArgumentNullException("writers");
                }

                object[] parameters;

                if (this.context == null)
                {
                    parameters = new object[1];
                }
                else
                {
                    parameters = new object[this.context.Length + 1];
                }

                parameters[0] = writers;

                for (int i = 1; i < parameters.Length; i++)
                {
                    parameters[i] = this.context[i - 1];
                }

                try
                {
                    this.template.generatorType.InvokeMember(
                        "Generate",
                        BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                        null,
                        this.GeneratorInstance,
                        parameters);
                }
                catch (TargetInvocationException ex)
                {
                    TemplateRuntimeException e = new TemplateRuntimeException(
                        ex.InnerException.Message,
                        ex.InnerException);
                    this.FillRuntimeError(e);
                    throw e;
                }
                catch (Exception ex)
                {
                    TemplateRuntimeException e = new TemplateRuntimeException(
                        ex.Message,
                        ex);
                    this.FillRuntimeError(e);
                    throw e;
                }
            }

            private void FillRuntimeError(TemplateRuntimeException ex)
            {
                if (this.template.parser.TemplateFile != null)
                {
                    ex.SourceFileName = this.template.parser.TemplateFile;
                }
            }
        }
    }
}
