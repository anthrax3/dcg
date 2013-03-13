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
using System.Text.RegularExpressions;
using Cavingdeep.Dcg.At.Lexing;
using Cavingdeep.Dcg.At.Parsing;
using Cavingdeep.Dcg.At.Walkers;
using Cavingdeep.Dcg.Properties;
using Assertion = System.Diagnostics.Debug;

namespace Cavingdeep.Dcg.At
{
    /// <summary>
    /// At syntax parser.
    /// </summary>
    [Serializable]
    internal class AtParser
    {
        private const string ReferenceDirective = "@reference";
        private const string ImportDirective = "@import";
        private const string ParamDirective = "@param";
        private const string GlobalStartDirective = "@global";
        private const string GlobalEndDirective = "@end_global";
        private const string CommentDirective = "@#";
        private const char WhiteDirective = '@';
        private const string CodeStartDirective = "@code";
        private const string CodeEndDirective = "@end_code";
        private const string TextStartDirective = "@text";
        private const string TextEndDirective = "@end_text";
        private const string ExecutionDirective = "@!";
        private const string OutputStartDirective = "@output";
        private const string OutputEndDirective = "@end_output";
        private const string MultilineEvalutionDirective = "@=";
        private const string BetweenStartDirective = "@{";
        private const string BetweenEndDirective = "@}";
        private const string SectionRefDirective = "@+";
        private const string SectionDefStartDirective = "@section";
        private const string SectionDefEndDirective = "@end_section";

        private static readonly Regex paramExpr = new Regex(
            @"(?<paramName>[^:\s]+)\s*:\s*(?<paramType>[^:\s]+)");

        private static readonly Regex whiteHandlingExpr = new Regex(
            @"[^@](?:@@)*@\s*$");

        private static readonly Regex betweenStartHandlingExpr = new Regex(
            @"(?:^|[^@])(?:@@)*@{[ \t]*$");

        private static readonly Regex sectionExp = new Regex(
            @"^(?<name>[a-z_][a-z0-9_]*)\s*(?:\((?<param>(?<pname>[a-z_][a-z0-9_]*)\s*:\s*(?<ptype>[a-z_][a-z0-9_\[\]<>]*)(?:\s*,\s*)?)*\))?\s*$",
            RegexOptions.IgnoreCase);

        private static readonly Regex nameExp =
            new Regex(@"^\s*(?<name>[a-z_][a-z0-9_]*)(?<rest>.*)", RegexOptions.IgnoreCase);

        private TextReader reader;
        private string sourceCode;
        private bool debug;
        private string templateFile;
        private int lineCount;
        private bool isInGlobalBlock;
        private Stack<OutputContext> outputStack;
        private ContextStack contextStack;
        private string[] references;
        private AtTemplateAst ast;
        private Directive currentDirective;
        private IDictionary<string, object> sections;

        public TextReader Reader
        {
            get
            {
                return this.reader;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.reader = value;
            }
        }

        public string SourceCode
        {
            get
            {
                return this.sourceCode;
            }
        }

        public string[] References
        {
            get
            {
                return this.references;
            }
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

        public string TemplateFile
        {
            get
            {
                return this.templateFile;
            }

            set
            {
                this.templateFile = value;
            }
        }

        public string LineEnding
        {
            get;
            set;
        }

        public Directive CurrentDirective
        {
            get
            {
                return this.currentDirective;
            }
        }

        public AtParser()
        {
            this.LineEnding = Environment.NewLine;
        }

        public void Parse()
        {
            try
            {
                InitParsing();
                BuildAst();
                WalkAst();
                this.references = this.ast.Head.References
                                 .ConvertAll(reference => reference.Value)
                                 .ToArray();
            }
            finally
            {
                this.ast = null;
            }
        }

        private void InitParsing()
        {
            this.ast = new AtTemplateAst();

            this.ast.Head.References.Add(new Reference("System.dll"));
            this.ast.Head.References.Add(new Reference(Assembly.GetExecutingAssembly().Location));

            this.currentDirective = this.ast.Body;

            if (this.outputStack == null)
            {
                this.outputStack = new Stack<OutputContext>();
            }
            else
            {
                this.outputStack.Clear();
            }

            if (this.contextStack == null)
            {
                this.contextStack = new ContextStack();
            }
            else
            {
                this.contextStack.Clear();
            }

            if (this.sections == null)
            {
                this.sections = new Dictionary<string, object>();
            }
            else
            {
                this.sections.Clear();
            }

            this.contextStack.Push(new Context(TemplateMode.Static, string.Empty));

            this.lineCount = 0;
            this.isInGlobalBlock = false;
        }

        private void BuildAst()
        {
            string line;

            while ((line = this.reader.ReadLine()) != null)
            {
                this.lineCount++;
                ParseLine(line);
            }

            if (this.isInGlobalBlock)
            {
                TemplateParsingException ex =
                    new TemplateParsingException(
                        Resources.GlobalDirectiveWrong);
                FillParseError(ex);
                throw ex;
            }
        }

        private void WalkAst()
        {
            SourceGenerator sourceGenerator = new SourceGenerator();
            sourceGenerator.Ast = this.ast;
            sourceGenerator.Debugging = this.debug;
            sourceGenerator.TemplateFile = this.templateFile;
            sourceGenerator.Walk();

            this.sourceCode = sourceGenerator.SourceCode;
        }

        private void ParseLine(string line)
        {
            Assertion.Assert(line != null, "line cannot be null");

            if (this.contextStack.Peek().Mode == TemplateMode.Static)
            {
                StaticModeParse(line);
            }
            else
            {
                DynamicModeParse(line);
            }
        }

        private void StaticModeParse(string line)
        {
            Assertion.Assert(line != null, "line cannot be null");

            if (!ProcessHeaderDirectives(line))
            {
                ProcessBodyDirectives(line);
            }
        }

        private bool ProcessHeaderDirectives(string line)
        {
            Assertion.Assert(line != null, "line cannot be null");

            bool processed = true;

            if (line.StartsWith(ReferenceDirective))
            {
                ParseReference(line);
            }
            else if (line.StartsWith(ImportDirective))
            {
                ParseImport(line);
            }
            else if (line.StartsWith(ParamDirective))
            {
                ParseParam(line);
            }
            else if (line.TrimEnd().Equals(GlobalStartDirective))
            {
                ParseGlobalStart(line);
            }
            else if (line.TrimEnd().Equals(GlobalEndDirective))
            {
                ParseGlobalEnd(line);
            }
            else if (line.TrimStart().StartsWith(CommentDirective) ||
                     line.Trim().Equals(WhiteDirective.ToString()))
            {
                // Ignore comment line and empty whitespace line.
            }
            else if (this.isInGlobalBlock)
            {
                this.ast.Head.Global.AppendLine(line);
            }
            else
            {
                processed = false;
            }

            return processed;
        }

        private void ProcessBodyDirectives(string line)
        {
            Assertion.Assert(line != null, "line cannot be null");

            string trimmedLine = line.Trim();

            if (trimmedLine.Equals(TextEndDirective))
            {
                ParseTextEnd();
            }
            else if (trimmedLine.Equals(CodeStartDirective))
            {
                line = HandleLineLeadingSpaces(line);
                ParseCodeStart(line);
            }
            else if (trimmedLine.Equals(SectionDefEndDirective))
            {
                ParseSectionEnd();
            }
            else if (trimmedLine.Equals(OutputEndDirective))
            {
                ParseOutputEnd();
            }
            else if (line.TrimStart().StartsWith(ExecutionDirective))
            {
                ParseExecutionDirective(line);
            }
            else if (line.TrimStart().StartsWith(SectionDefStartDirective))
            {
                ParseSectionStart(line);
            }
            else if (line.TrimStart().StartsWith(SectionRefDirective))
            {
                line = HandleLineLeadingSpaces(line);
                ParseSectionRef(line);
            }
            else if (line.TrimStart().StartsWith(MultilineEvalutionDirective))
            {
                line = HandleLineLeadingSpaces(line);
                ParseMultilineEvaluation(line);
            }
            else if (line.TrimStart().StartsWith(OutputStartDirective))
            {
                ParseOutputStart(line);
            }
            else
            {
                line = HandleLineLeadingSpaces(line);
                ParseTextLine(line);
            }
        }

        private void DynamicModeParse(string line)
        {
            Assertion.Assert(line != null, "line cannot be null");

            line = HandleOutputLeadingSpaces(line);

            if (line.Trim().Equals(CodeEndDirective))
            {
                ParseCodeEnd();
            }
            else if (line.Trim().Equals(TextStartDirective))
            {
                ParseTextStart(line);
            }
            else if (line.TrimStart().StartsWith(BetweenEndDirective))
            {
                ParseBetweenEnd(line);
            }
            else
            {
                this.currentDirective.Directives.Add(new DynamicText(line, this.lineCount));
            }
        }

        private void ParseTextLine(string line)
        {
            bool isNewLine = true;
            string textLine = HandleLineEnding(line);

            if (whiteHandlingExpr.IsMatch(textLine))
            {
                isNewLine = false;
                textLine = textLine.Substring(0, textLine.LastIndexOf(WhiteDirective));
            }

            List<IDirectiveParser> parsers = MatchDirectives(textLine);

            parsers.Sort(
                delegate(IDirectiveParser x, IDirectiveParser y)
                {
                    if (x.Index < y.Index)
                    {
                        return -1;
                    }
                    else if (x.Index > y.Index)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                });

            int index = 0;
            StaticText staticText;

            for (int i = 0; i < parsers.Count; i++)
            {
                IDirectiveParser parser = parsers[i];

                staticText =
                    new StaticText(
                        textLine.Substring(index, parser.Index - index).Replace("@@", "@"),
                        this.lineCount);

                if (i == 0)
                {
                    staticText.IsStartOfLine = true;
                }

                this.currentDirective.Directives.Add(staticText);
                parser.Parse();
                index = parser.Index + parser.Length;
            }

            staticText =
                new StaticText(
                    textLine.Substring(index, textLine.Length - index).Replace("@@", "@"),
                    this.lineCount);
            staticText.IsNewLine = isNewLine;

            if (parsers.Count == 0)
            {
                staticText.IsStartOfLine = true;
            }

            this.currentDirective.Directives.Add(staticText);

            ParseBetweenStartIfAny(line);
        }

        private List<IDirectiveParser> MatchDirectives(string line)
        {
            Assertion.Assert(line != null, "line cannot be null");

            List<IDirectiveParser> parsers = new List<IDirectiveParser>();

            MatchEvaluationDirective(line, parsers);

            return parsers;
        }

        private void MatchEvaluationDirective(
            string line, List<IDirectiveParser> parsers)
        {
            //// @(text)

            Assertion.Assert(line != null);
            Assertion.Assert(parsers != null);

            int sharpCount = 0;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '@')
                {
                    sharpCount++;
                }

                if (line[i] == '@' &&
                    i + 1 < line.Length &&
                    line[i + 1] == '(' &&
                    sharpCount % 2 == 1)
                {
                    int index = i;
                    int lparen = 0;
                    bool matched = false;

                    for (i = i + 2; i < line.Length; i++)
                    {
                        if (line[i] == '(')
                        {
                            lparen++;
                        }
                        else if (line[i] == ')')
                        {
                            if (lparen > 0)
                            {
                                lparen--;
                            }
                            else
                            {
                                int length = i + 1 - index;
                                parsers.Add(
                                    new EvaluationDirectiveParser(
                                        index,
                                        length,
                                        line.Substring(index + 2, length - 3),
                                        this.lineCount,
                                        this));
                                matched = true;

                                sharpCount = 0;
                                break;
                            }
                        }
                    }

                    if (!matched)
                    {
                        TemplateParsingException ex =
                            new TemplateParsingException(
                                Resources.WrongEvaluationDirective);
                        FillParseError(ex);

                        throw ex;
                    }
                }
            }
        }

        private void ParseGlobalStart(string line)
        {
            AppendDebugLine(this.ast.Head.Global, this.lineCount + 1);
            this.isInGlobalBlock = true;
        }

        private void ParseGlobalEnd(string line)
        {
            if (!this.isInGlobalBlock)
            {
                TemplateParsingException ex =
                    new TemplateParsingException(
                        Resources.GlobalDirectiveWrong);
                FillParseError(ex);
                throw ex;
            }

            this.isInGlobalBlock = false;
        }

        private void ParseReference(string line)
        {
            Assertion.Assert(line != null, "line cannot be null.");

            if (line.Length <= ReferenceDirective.Length + 1 ||
                line[ReferenceDirective.Length] != ' ')
            {
                TemplateParsingException ex =
                    new TemplateParsingException(
                        Resources.ReferenceDirectiveWrong);
                FillParseError(ex);
                throw ex;
            }

            string reference = line.Substring(ReferenceDirective.Length + 1);

            if (string.IsNullOrEmpty(this.templateFile))
            {
                reference = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    reference);
            }
            else
            {
                reference = Path.Combine(
                    Path.GetDirectoryName(this.templateFile),
                    reference);
            }

            if (!File.Exists(reference))
            {
                reference = Path.GetFileName(reference);
            }

            if (!this.ast.Head.References.Exists(r => r.Value == reference))
            {
                this.ast.Head.References.Add(new Reference(reference));
            }
        }

        private void ParseParam(string line)
        {
            Assertion.Assert(line != null, "line cannot be null.");

            if (line.Length <= ParamDirective.Length + 1 ||
                line[ParamDirective.Length] != ' ')
            {
                TemplateParsingException ex =
                    new TemplateParsingException(
                        Resources.ParamDirectiveWrong);
                FillParseError(ex);
                throw ex;
            }

            Match m = AtParser.paramExpr.Match(line, ParamDirective.Length);

            if (!m.Success)
            {
                TemplateParsingException ex = new TemplateParsingException(
                    Resources.ParamDirectiveWrong);
                FillParseError(ex);
                throw ex;
            }

            Parameter p = new Parameter(
                m.Groups["paramName"].Value,
                m.Groups["paramType"].Value.TrimEnd());
            this.ast.Head.Parameters.Add(p);

            if (this.debug)
            {
                p.Line = this.lineCount;
            }
        }

        private void ParseImport(string line)
        {
            Assertion.Assert(line != null, "line cannot be null.");

            if (line.Length <= ImportDirective.Length + 1 ||
                line[ImportDirective.Length] != ' ')
            {
                TemplateParsingException ex =
                    new TemplateParsingException(
                        Resources.ImportDirectiveWrong);
                FillParseError(ex);
                throw ex;
            }

            string import = line.Substring(ImportDirective.Length + 1);

            if (!this.ast.Head.Imports.Exists(i => i.Value == import))
            {
                this.ast.Head.Imports.Add(new Import(import));
            }
        }

        private void ParseExecutionDirective(string line)
        {
            Assertion.Assert(line != null, "line cannot be null.");

            int startIndex = line.IndexOf(ExecutionDirective);

            this.currentDirective.Directives.Add(
                new Execution(
                    line.Substring(startIndex + ExecutionDirective.Length),
                    this.lineCount));
        }

        private void ParseOutputStart(string line)
        {
            Assertion.Assert(line != null, "line cannot be null.");

            string trimedLine = line.TrimStart();
            string key = trimedLine.Substring(OutputStartDirective.Length).Trim();

            if (trimedLine.Length == OutputStartDirective.Length ||
                trimedLine[OutputStartDirective.Length] != ' ' ||
                key.Length == 0)
            {
                TemplateParsingException ex =
                    new TemplateParsingException(
                        Resources.NoOutputKeyProvided);
                FillParseError(ex);
                throw ex;
            }

            Output output = new Output(key);
            this.currentDirective.Directives.Add(output);
            this.currentDirective = output;

            this.outputStack.Push(
                new OutputContext(
                    key,
                    line.Substring(0, CountLeadingWhiteSpace(line))));

            this.contextStack.Push(new Context(TemplateMode.Static, ""));
        }

        private void ParseOutputEnd()
        {
            if (this.outputStack.Count == 0)
            {
                TemplateParsingException ex =
                    new TemplateParsingException(
                        Resources.OutputDirectiveWrong);
                FillParseError(ex);
                throw ex;
            }

            this.currentDirective = this.currentDirective.Parent;

            this.outputStack.Pop();
            this.contextStack.Pop();
        }

        private void ParseCodeStart(string line)
        {
            Assertion.Assert(line != null, "line cannot be null.");

            string spaces = line.Substring(0, line.IndexOf(CodeStartDirective));
            Code code = new Code(spaces);
            this.currentDirective.Directives.Add(code);
            this.currentDirective = code;

            this.contextStack.Push(
                new Context(
                    TemplateMode.Dynamic,
                    line.Substring(0, CountLeadingWhiteSpace(line))));
        }

        private void ParseCodeEnd()
        {
            if (this.contextStack.Count <= 1 ||
                this.contextStack.Peek().Mode != TemplateMode.Dynamic)
            {
                TemplateParsingException ex =
                    new TemplateParsingException(
                        Resources.CodeDirectiveWrong);
                FillParseError(ex);
                throw ex;
            }

            this.currentDirective = this.currentDirective.Parent;

            this.contextStack.Pop();
        }

        private void ParseTextStart(string line)
        {
            Assertion.Assert(line != null, "line cannot be null.");

            Text text = new Text();
            this.currentDirective.Directives.Add(text);
            this.currentDirective = text;

            this.contextStack.Push(
                new Context(
                    TemplateMode.Static,
                    line.Substring(0, CountLeadingWhiteSpace(line))));
        }

        private void ParseTextEnd()
        {
            if (this.contextStack.Count <= 1 ||
                this.contextStack.Peek().Mode != TemplateMode.Static)
            {
                TemplateParsingException ex =
                    new TemplateParsingException(
                        Resources.TextDirectiveWrong);
                FillParseError(ex);
                throw ex;
            }

            this.currentDirective = this.currentDirective.Parent;

            this.contextStack.Pop();
        }

        private void ParseBetweenStartIfAny(string line)
        {
            Assertion.Assert(line != null, "line cannot be null.");

            if (betweenStartHandlingExpr.IsMatch(line))
            {
                Between between = new Between();
                this.currentDirective.Directives.Add(between);
                this.currentDirective = between;

                this.contextStack.Push(new Context(
                    TemplateMode.Dynamic, string.Empty));
            }
        }

        private void ParseBetweenEnd(string line)
        {
            if (this.contextStack.Count <= 1 ||
                this.contextStack.Peek().Mode != TemplateMode.Dynamic)
            {
                TemplateParsingException ex =
                    new TemplateParsingException(Resources.BetweenDirectiveWrong);
                FillParseError(ex);
                throw ex;
            }

            this.currentDirective = this.currentDirective.Parent;

            this.contextStack.Pop();

            this.ParseTextLine(
                line.Substring(
                    line.IndexOf(BetweenEndDirective) + BetweenEndDirective.Length));
        }

        private void ParseMultilineEvaluation(string line)
        {
            Assertion.Assert(line != null, "line cannot be null.");

            int index = line.IndexOf(MultilineEvalutionDirective);
            string spaces = line.Substring(0, index);
            string text = line.Substring(
                index + MultilineEvalutionDirective.Length);

            this.currentDirective.Directives.Add(
                new MultiLineEvaluation(text, spaces, this.lineCount));
        }

        private void ParseSectionStart(string line)
        {
            Assertion.Assert(line != null, "line cannot be null.");

            string trimedLine = line.TrimStart();
            string sectionDef = trimedLine.Substring(SectionDefStartDirective.Length).Trim();

            if (trimedLine.Length == SectionDefStartDirective.Length ||
                trimedLine[SectionDefStartDirective.Length] != ' ' ||
                sectionDef.Length == 0)
            {
                TemplateParsingException ex =
                    new TemplateParsingException(
                        Resources.BadSectionDef);
                FillParseError(ex);
                throw ex;
            }

            if (!(this.currentDirective is Body))
            {
                TemplateParsingException ex =
                    new TemplateParsingException(
                        Resources.SectionDefNotTopLevel);
                FillParseError(ex);
                throw ex;
            }

            Match match = sectionExp.Match(sectionDef);

            if (!match.Success)
            {
                TemplateParsingException ex =
                    new TemplateParsingException(
                        Resources.BadSectionDef);
                FillParseError(ex);
                throw ex;
            }

            if (this.sections.ContainsKey(match.Groups["name"].Value))
            {
                TemplateParsingException ex =
                    new TemplateParsingException(
                        String.Format(
                            Resources.SectionNameNotUnique,
                            match.Groups["name"].Value));
                FillParseError(ex);
                throw ex;
            }
            else
            {
                this.sections.Add(match.Groups["name"].Value, null);
            }

            SectionDefinition def = new SectionDefinition(match.Groups["name"].Value);

            for (int i = 0; i < match.Groups["pname"].Captures.Count; i++)
            {
                string pname = match.Groups["pname"].Captures[i].Value;
                string ptype = match.Groups["ptype"].Captures[i].Value;
                def.Parameters.Add(new KeyValuePair<string, string>(pname, ptype));
            }

            this.currentDirective.Directives.Add(def);
            this.currentDirective = def;
        }

        private void ParseSectionEnd()
        {
            if (!(this.currentDirective is SectionDefinition))
            {
                TemplateParsingException ex =
                    new TemplateParsingException(
                        Resources.SectionDefDirectiveWrong);
                FillParseError(ex);
                throw ex;
            }

            this.currentDirective = this.currentDirective.Parent;
        }

        private void ParseSectionRef(string line)
        {
            Assertion.Assert(line != null, "line cannot be null.");

            int index = line.IndexOf(SectionRefDirective);
            string spaces = line.Substring(0, index);
            string text = line.Substring(
                index + SectionRefDirective.Length);

            Match match = nameExp.Match(text);

            if (!match.Success)
            {
                TemplateParsingException ex =
                    new TemplateParsingException(
                        Resources.BadSectionRef);
                FillParseError(ex);
                throw ex;
            }

            this.currentDirective.Directives.Add(
                new SectionReference(
                    match.Groups["name"].Value,
                    match.Groups["rest"].Value.Trim(),
                    spaces,
                    this.lineCount));
        }

        private int CountLeadingWhiteSpace(string line)
        {
            Assertion.Assert(line != null, "line cannot be null.");

            int whiteSpaceCount = 0;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] != ' ' && line[i] != '\t')
                {
                    break;
                }

                whiteSpaceCount++;
            }

            return whiteSpaceCount;
        }

        private string HandleLineLeadingSpaces(string line)
        {
            Assertion.Assert(line != null, "line cannot be null.");

            line = this.HandleOutputLeadingSpaces(line);
            line = this.HandleContextLeadingSpaces(line);

            return line;
        }

        private string HandleContextLeadingSpaces(string line)
        {
            Assertion.Assert(line != null, "line cannot be null.");

            string spaces = this.contextStack.Peek().Spaces;

            if (spaces.Length > 0)
            {
                if (!line.StartsWith(spaces))
                {
                    TemplateParsingException ex =
                        new TemplateParsingException(
                            Resources.TextDirectiveSpaceWrong);
                    FillParseError(ex);
                    throw ex;
                }

                line = line.Substring(spaces.Length);
            }

            return line;
        }

        private string HandleOutputLeadingSpaces(string line)
        {
            Assertion.Assert(line != null, "line cannot be null.");

            if (this.outputStack.Count > 0)
            {
                string spaces = this.outputStack.Peek().Spaces;

                if (spaces.Length > 0)
                {
                    if (!line.StartsWith(spaces))
                    {
                        TemplateParsingException ex =
                            new TemplateParsingException(Resources.OutputDirectiveSpaceWrong);
                        FillParseError(ex);
                        throw ex;
                    }

                    line = line.Substring(spaces.Length);
                }
            }

            return line;
        }

        private void FillParseError(TemplateParsingException ex)
        {
            Assertion.Assert(ex != null, "ex cannot be null.");

            ex.SourceLine = this.lineCount;

            if (this.templateFile != null)
            {
                ex.SourceFileName = this.templateFile;
            }
        }

        private string HandleLineEnding(string line)
        {
            Assertion.Assert(line != null, "line cannot be null.");

            if (betweenStartHandlingExpr.IsMatch(line))
            {
                return line.Substring(
                    0, line.LastIndexOf(BetweenStartDirective));
            }

            return line + this.LineEnding;
        }

        private void AppendDebugLine(StringBuilder builder, int lineCount)
        {
            Assertion.Assert(builder != null, "builder cannot be null.");

            if (!this.debug)
            {
                return;
            }

            if (lineCount <= 0)
            {
                return;
            }

            if (this.templateFile != null)
            {
                builder.AppendLine(
                    "#line " + lineCount + " \"" + this.templateFile + "\"");
            }
            else
            {
                builder.AppendLine("#line " + lineCount);
            }
        }
    }
}
