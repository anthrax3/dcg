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

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Cavingdeep.Dcg.At.Parsing;

namespace Cavingdeep.Dcg.At.Walkers
{
    internal class SourceGenerator : IAstWalker
    {
        private AtTemplateAst ast;
        private StringBuilder builder = new StringBuilder(300);
        private bool isNewLine = true;

        public bool Debugging
        {
            get;
            set;
        }

        public string TemplateFile
        {
            get;
            set;
        }

        public string SourceCode
        {
            get;
            set;
        }

        public AtTemplateAst Ast
        {
            get
            {
                return this.ast;
            }

            set
            {
                Debug.Assert(value != null);

                this.ast = value;
            }
        }

        public void Walk()
        {
            GenerateHeader();

            ProcessDirectives(this.ast.Body.Directives, AtTemplate.MainOutputKey);

            GenerateFooter();

            this.SourceCode = this.builder.ToString();
        }

        private void GenerateHeader()
        {
            StringBuilder builder = this.builder;

            builder.AppendLine("using System;");
            builder.AppendLine();

            foreach (Import import in this.ast.Head.Imports)
            {
                builder.Append("using ");
                builder.Append(import.Value);
                builder.AppendLine(";");
            }

            builder.AppendLine();
            builder.AppendLine("namespace Cavingdeep.Generated");
            builder.AppendLine("{");
            builder.AppendLine("    public class Generator");
            builder.AppendLine("    {");
            builder.AppendLine("        private System.Collections.Generic.IDictionary<string, System.IO.TextWriter> writers;");
            builder.AppendLine("        private InnerDcg dcg;");
            builder.AppendLine();

            foreach (Parameter p in this.ast.Head.Parameters)
            {
                GenerateDebugLine(builder, p.Line);

                builder.Append(p.Type);
                builder.Append(' ');
                builder.Append(p.Name);
                builder.AppendLine(";");
            }

            builder.AppendLine();

            builder.AppendLine(this.ast.Head.Global.ToString());

            builder.AppendLine();

            builder.AppendLine("        private InnerDcg Dcg");
            builder.AppendLine("        {");
            builder.AppendLine("            get {return this.dcg;}");
            builder.AppendLine("        }");
            builder.AppendLine();

            builder.AppendLine("        public void Generate(");
            builder.Append("            System.Collections.Generic.IDictionary<string, System.IO.TextWriter> writers");
            if (this.ast.Head.Parameters.Count > 0)
            {
                builder.AppendLine(",");

                for (int i = 0; i < this.ast.Head.Parameters.Count; i++)
                {
                    Parameter p = this.ast.Head.Parameters[i];

                    GenerateDebugLine(this.builder, p.Line);

                    builder.Append(p.Type);
                    builder.Append(' ');
                    builder.Append(p.Name);

                    if (i + 1 < this.ast.Head.Parameters.Count)
                    {
                        builder.AppendLine(",");
                    }
                    else
                    {
                        builder.AppendLine(")");
                    }
                }
            }
            else
            {
                builder.AppendLine(")");
            }

            builder.AppendLine("        {");
            builder.AppendLine("            this.writers = writers;");
            builder.AppendLine();

            foreach (Parameter p in this.ast.Head.Parameters)
            {
                builder.Append("            this.");
                builder.Append(p.Name);
                builder.Append(" = ");
                builder.Append(p.Name);
                builder.AppendLine(";");
            }

            builder.AppendLine();
            builder.AppendLine("            this.Generate();");
            builder.AppendLine("        }");
            builder.AppendLine();

            builder.AppendLine("        public Generator()");
            builder.AppendLine("        {");
            builder.AppendLine("            this.dcg = new InnerDcg(this);");
            builder.AppendLine("        }");
            builder.AppendLine();

            builder.AppendLine("        private void Generate()");
            builder.AppendLine("        {");
        }

        private void GenerateFooter()
        {
            if (this.builder.Length == 0)
            {
                GenerateHeader();
            }

            this.builder.AppendLine();
            this.builder.AppendLine("            foreach (System.IO.TextWriter writer in this.writers.Values)");
            this.builder.AppendLine("            {");
            this.builder.AppendLine("                writer.Flush();");
            this.builder.AppendLine("            }");
            this.builder.AppendLine("        }");
            this.builder.AppendLine();

            this.builder.AppendLine("        private void FillSpaces(string spaces, string key, string text)");
            this.builder.AppendLine("        {");
            this.builder.AppendLine("            using (System.IO.StringReader reader = new System.IO.StringReader(text))");
            this.builder.AppendLine("            {");
            this.builder.AppendLine("                string line;");
            this.builder.AppendLine("                while ((line = reader.ReadLine()) != null)");
            this.builder.AppendLine("                {");
            this.builder.AppendLine("                    this.writers[key].Write(spaces);");
            this.builder.AppendLine("                    this.writers[key].Write(line);");
            this.builder.AppendLine("                    this.writers[key].WriteLine();");
            this.builder.AppendLine("                }");
            this.builder.AppendLine("            }");
            this.builder.AppendLine("        }");
            this.builder.AppendLine();

            GenerateSectionDefinitions();

            this.builder.AppendLine("        private class InnerDcg");
            this.builder.AppendLine("        {");
            this.builder.AppendLine("            private static System.Collections.Generic.Dictionary<string, Cavingdeep.Dcg.At.AtTemplate> cache =");
            this.builder.AppendLine("                new System.Collections.Generic.Dictionary<string, Cavingdeep.Dcg.At.AtTemplate>();");
            this.builder.AppendLine();
            this.builder.AppendLine("            public static readonly System.IO.FileInfo fileInfo =");

            if (string.IsNullOrEmpty(this.TemplateFile))
            {
                this.builder.AppendLine("                null;");
            }
            else
            {
                this.builder.Append("                new System.IO.FileInfo(@\"");
                this.builder.Append(this.TemplateFile);
                this.builder.AppendLine("\");");
            }

            this.builder.AppendLine();
            this.builder.AppendLine("            private Generator owner;");
            this.builder.AppendLine();
            this.builder.AppendLine("            public InnerDcg(Generator owner)");
            this.builder.AppendLine("            {");
            this.builder.AppendLine("                this.owner = owner;");
            this.builder.AppendLine("            }");
            this.builder.AppendLine();
            this.builder.AppendLine("            public System.IO.FileInfo FileInfo");
            this.builder.AppendLine("            {");
            this.builder.AppendLine("                get {return fileInfo;}");
            this.builder.AppendLine("            }");
            this.builder.AppendLine();
            this.builder.AppendLine("            public void Write(object obj)");
            this.builder.AppendLine("            {");
            this.builder.AppendLine("                Write(obj, \"_main_\");");
            this.builder.AppendLine("            }");
            this.builder.AppendLine();

            this.builder.AppendLine("            public void Write(object obj, string writerKey)");
            this.builder.AppendLine("            {");
            this.builder.AppendLine("                if (string.IsNullOrEmpty(writerKey))");
            this.builder.AppendLine("                {");
            this.builder.AppendLine("                    throw new ArgumentNullException(\"writerKey\");");
            this.builder.AppendLine("                }");
            this.builder.AppendLine("                if (!this.owner.writers.ContainsKey(writerKey))");
            this.builder.AppendLine("                {");
            this.builder.AppendLine("                    throw new ArgumentOutOfRangeException(\"writerKey\");");
            this.builder.AppendLine("                }");
            this.builder.AppendLine();

            this.builder.AppendLine("                this.owner.writers[writerKey].Write(obj.ToString());");
            this.builder.AppendLine("            }");
            this.builder.AppendLine();
            this.builder.AppendLine("            public string CallTemplate(");
            this.builder.AppendLine("                string templateFile, System.Text.Encoding encoding, string compilerVersion,");
            this.builder.AppendLine("                params object[] values)");
            this.builder.AppendLine("            {");
            this.builder.AppendLine("                if (!System.IO.Path.IsPathRooted(templateFile) &&");
            this.builder.AppendLine("                    this.FileInfo != null)");
            this.builder.AppendLine("                {");
            this.builder.AppendLine("                    templateFile = System.IO.Path.Combine(");
            this.builder.AppendLine("                        this.FileInfo.DirectoryName, templateFile);");
            this.builder.AppendLine("                }");
            this.builder.AppendLine();
            this.builder.AppendLine("                Cavingdeep.Dcg.At.AtTemplate template;");
            this.builder.AppendLine("                if (cache.ContainsKey(templateFile))");
            this.builder.AppendLine("                {");
            this.builder.AppendLine("                    template = cache[templateFile];");
            this.builder.AppendLine("                }");
            this.builder.AppendLine("                else");
            this.builder.AppendLine("                {");
            this.builder.AppendLine("                    template = new Cavingdeep.Dcg.At.AtTemplate(templateFile, encoding, compilerVersion);");

            if (this.Debugging)
            {
                this.builder.AppendLine("                    template.Debug = true;");
            }

            this.builder.AppendLine("                    template.Parse();");
            this.builder.AppendLine("                    cache.Add(templateFile, template);");
            this.builder.AppendLine("                }");
            this.builder.AppendLine("                template.Context = values;");
            this.builder.AppendLine("                return template.Render();");
            this.builder.AppendLine("            }");
            this.builder.AppendLine("        }");
            this.builder.AppendLine("    }");
            this.builder.AppendLine("}");
        }

        private void ProcessDirectives(IEnumerable<Directive> directives, string writerKey)
        {
            foreach (Directive directive in directives)
            {
                if (directive is StaticText)
                {
                    ProcessStaticText((StaticText) directive, writerKey);
                }
                else if (directive is Evaluation)
                {
                    ProcessEvaluation((Evaluation) directive, writerKey);
                }
                else if (directive is Code)
                {
                    ProcessCode((Code) directive, writerKey);
                }
                else if (directive is Between)
                {
                    ProcessBetween((Between) directive, writerKey);
                }
                else if (directive is Execution)
                {
                    ProcessExecution((Execution) directive);
                }
                else if (directive is SectionReference)
                {
                    ProcessSectionRef((SectionReference) directive, writerKey);
                }
                else if (directive is MultiLineEvaluation)
                {
                    ProcessMultiLineEvaluation((MultiLineEvaluation) directive, writerKey);
                }
                else if (directive is Output)
                {
                    ProcessOutput((Output) directive);
                }
            }
        }

        private void ProcessStaticText(StaticText directive, string writerKey)
        {
            GenerateDebugLine(this.builder, directive.Line);
            this.builder.Append("this.writers[\"");
            this.builder.Append(writerKey);
            this.builder.Append("\"].Write(@\"");

            if (directive.IsStartOfLine)
            {
                if (this.isNewLine)
                {
                    this.builder.Append(EscapeStaticText(IndentDirective(directive, directive.Value))); 
                }
                else
                {
                    this.builder.Append(EscapeStaticText(directive.Value)); 
                }
            }
            else
            {
                this.builder.Append(EscapeStaticText(directive.Value));
            }
            
            this.builder.AppendLine("\");");

            this.isNewLine = directive.IsNewLine;
        }

        private void ProcessDynamicText(DynamicText directive)
        {
            GenerateDebugLine(this.builder, directive.Line);
            this.builder.AppendLine(directive.Value);
        }

        private void ProcessMultiLineEvaluation(MultiLineEvaluation directive, string writerKey)
        {
            GenerateDebugLine(this.builder, directive.Line);
            this.builder.AppendFormat(
                "FillSpaces(\"{0}\", \"{1}\", {2});",
                EscapeToCSharp(IndentDirective(directive, directive.Spaces)),
                writerKey,
                directive.Expression);
            this.builder.AppendLine();
        }

        private void ProcessEvaluation(Evaluation evaluation, string writerKey)
        {
            GenerateDebugLine(this.builder, evaluation.Line);
            this.builder.Append("this.writers[\"");
            this.builder.Append(writerKey);
            this.builder.Append("\"].Write(");
            this.builder.Append(evaluation.Expression);
            this.builder.AppendLine(");");
        }

        private void ProcessExecution(Execution execution)
        {
            GenerateDebugLine(this.builder, execution.Line);
            this.builder.AppendLine(execution.Statement);
        }

        private void ProcessCode(Code code, string writerKey)
        {
            foreach (Directive directive in code.Directives)
            {
                if (directive is DynamicText)
                {
                    ProcessDynamicText((DynamicText) directive);
                }
                else if (directive is Text)
                {
                    ProcessText((Text) directive, writerKey);
                }
            }
        }

        private void ProcessText(Text text, string writerKey)
        {
            ProcessDirectives(text.Directives, writerKey);
        }

        private void ProcessBetween(Between between, string writerKey)
        {
            foreach (Directive directive in between.Directives)
            {
                if (directive is DynamicText)
                {
                    ProcessDynamicText((DynamicText) directive);
                }
                else if (directive is Text)
                {
                    ProcessText((Text) directive, writerKey);
                }
            }
        }

        private void ProcessOutput(Output output)
        {
            ProcessDirectives(output.Directives, EscapeToCSharp(output.WriterKey));
        }

        private void ProcessSectionRef(SectionReference directive, string writerKey)
        {
            GenerateDebugLine(this.builder, directive.Line);

            if (string.IsNullOrEmpty(directive.Rest))
            {
                this.builder.Append(directive.Name);
                this.builder.Append("(\"");
                this.builder.Append(EscapeToCSharp(directive.Spaces));
                this.builder.Append("\", \"");
                this.builder.Append(writerKey);
                this.builder.Append("\");");
            }
            else
            {
                this.builder.Append(directive.Name);
                this.builder.Append(directive.Rest);
                this.builder.Remove(this.builder.Length - 1, 1);
                if (this.builder[this.builder.Length - 1] != '(')
                {
                    this.builder.Append(", ");
                }
                this.builder.Append('"');
                this.builder.Append(EscapeToCSharp(directive.Spaces));
                this.builder.Append("\", \"");
                this.builder.Append(writerKey);
                this.builder.Append("\");");
            }

            this.builder.AppendLine();
        }

        private void GenerateSectionDefinitions()
        {
            foreach (Directive directive in this.ast.Body.Directives)
            {
                if (directive is SectionDefinition)
                {
                    ProcessSectionDef((SectionDefinition) directive);
                }
            }
        }

        private void ProcessSectionDef(SectionDefinition directive)
        {
            this.builder.Append("private void ");
            this.builder.Append(directive.Name);
            this.builder.Append('(');

            for (int i = 0; i < directive.Parameters.Count; i++)
            {
                KeyValuePair<string, string> p = directive.Parameters[i];
                this.builder.Append(p.Value);
                this.builder.Append(' ');
                this.builder.Append(p.Key);
            }

            if (directive.Parameters.Count == 0)
            {
                this.builder.Append("string _spaces_, string writerKey");
            }
            else
            {
                this.builder.Append(", string _spaces_, string writerKey");
            }

            this.builder.AppendLine(") {");
            ProcessSectionDirectives(directive.Directives);
            this.builder.AppendLine("}");
            this.builder.AppendLine();
        }

        private void ProcessSectionDirectives(IEnumerable<Directive> directives)
        {
            foreach (Directive directive in directives)
            {
                if (directive is StaticText)
                {
                    ProcessSectionStaticText((StaticText) directive);
                }
                else if (directive is Evaluation)
                {
                    ProcessSectionEvaluation((Evaluation) directive);
                }
                else if (directive is Code)
                {
                    ProcessSectionCode((Code) directive);
                }
                else if (directive is Between)
                {
                    ProcessSectionBetween((Between) directive);
                }
                else if (directive is Execution)
                {
                    ProcessExecution((Execution) directive);
                }
                else if (directive is SectionReference)
                {
                    ProcessSectionSectionRef((SectionReference) directive);
                }
                else if (directive is MultiLineEvaluation)
                {
                    ProcessSectionMultiLineEvaluation((MultiLineEvaluation) directive);
                }
                else if (directive is Output)
                {
                    ProcessOutput((Output) directive);
                }
            }
        }

        private void ProcessSectionStaticText(StaticText directive)
        {
            if (directive.IsStartOfLine)
            {
                GenerateDebugLine(this.builder, directive.Line);
                if (this.isNewLine)
                {
                    this.builder.AppendLine("this.writers[writerKey].Write(_spaces_);"); 
                }
                GenerateDebugLine(this.builder, directive.Line);
                this.builder.Append("this.writers[writerKey].Write(@\"");
                this.builder.Append(EscapeStaticText(IndentDirective(directive, directive.Value)));
            }
            else
            {
                GenerateDebugLine(this.builder, directive.Line);
                this.builder.Append("this.writers[writerKey].Write(@\"");
                this.builder.Append(EscapeStaticText(directive.Value));
            }
            
            this.builder.AppendLine("\");");

            this.isNewLine = directive.IsNewLine;
        }

        private void ProcessSectionEvaluation(Evaluation evaluation)
        {
            GenerateDebugLine(this.builder, evaluation.Line);
            this.builder.Append("this.writers[writerKey].Write(");
            this.builder.Append(evaluation.Expression);
            this.builder.AppendLine(");");
        }

        private void ProcessSectionCode(Code code)
        {
            foreach (Directive directive in code.Directives)
            {
                if (directive is DynamicText)
                {
                    ProcessDynamicText((DynamicText) directive);
                }
                else if (directive is Text)
                {
                    ProcessSectionText((Text) directive);
                }
            }
        }

        private void ProcessSectionText(Text text)
        {
            ProcessSectionDirectives(text.Directives);
        }

        private void ProcessSectionBetween(Between between)
        {
            foreach (Directive directive in between.Directives)
            {
                if (directive is DynamicText)
                {
                    ProcessDynamicText((DynamicText) directive);
                }
                else if (directive is Text)
                {
                    ProcessSectionText((Text) directive);
                }
            }
        }

        private void ProcessSectionSectionRef(SectionReference directive)
        {
            GenerateDebugLine(this.builder, directive.Line);

            if (string.IsNullOrEmpty(directive.Rest))
            {
                this.builder.Append(directive.Name);
                this.builder.Append("(_spaces_ + \"");
                this.builder.Append(EscapeToCSharp(directive.Spaces));
                this.builder.Append("\", writerKey);");
            }
            else
            {
                this.builder.Append(directive.Name);
                this.builder.Append(directive.Rest);
                this.builder.Remove(this.builder.Length - 1, 1);
                if (this.builder[this.builder.Length - 1] != '(')
                {
                    this.builder.Append(", ");
                }
                this.builder.Append("_spaces_ + \"");
                this.builder.Append(EscapeToCSharp(directive.Spaces));
                this.builder.Append("\", writerKey);");
            }

            this.builder.AppendLine();
        }

        private void ProcessSectionMultiLineEvaluation(MultiLineEvaluation directive)
        {
            GenerateDebugLine(this.builder, directive.Line);
            this.builder.AppendFormat(
                "FillSpaces(_spaces_, writerKey, {0});",
                directive.Expression);
            this.builder.AppendLine();
        }

        private string IndentDirective(Directive directive, string text)
        {
            StringBuilder builder = new StringBuilder(4);

            for (Directive parent = directive.Parent; !(parent is IBorder); parent = parent.Parent)
            {
                IIndentScope scope = parent as IIndentScope;
                if (scope != null)
                {
                    builder.Insert(0, scope.Spaces);
                }
            }

            return builder.Append(text).ToString();
        }

        private void GenerateDebugLine(StringBuilder builder, int lineCount)
        {
            Debug.Assert(builder != null, "builder cannot be null.");

            if (!this.Debugging)
            {
                return;
            }

            if (lineCount <= 0)
            {
                return;
            }

            if (this.TemplateFile != null)
            {
                builder.AppendLine(
                    "#line " + lineCount + " \"" + this.TemplateFile + "\"");
            }
            else
            {
                builder.AppendLine("#line " + lineCount);
            }
        }

        private string EscapeStaticText(string text)
        {
            Debug.Assert(text != null, "text cannot be null.");

            if (text.Length == 0)
            {
                return text;
            }

            text = text.Replace("\"", "\"\"");

            return text;
        }

        private string EscapeToCSharp(string text)
        {
            Debug.Assert(text != null, "text cannot be null.");

            if (text.Length == 0)
            {
                return text;
            }

            text = text.Replace("\\", "\\\\");
            text = text.Replace("\t", "\\t");
            text = text.Replace("\"", "\\\"");

            return text;
        }
    }
}
