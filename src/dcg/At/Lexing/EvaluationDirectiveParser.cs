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
using System.Diagnostics;
using System.Text;
using Cavingdeep.Dcg.At.Parsing;

namespace Cavingdeep.Dcg.At.Lexing
{
    /// <summary>
    /// A evaluation directive parser.
    /// </summary>
    [Serializable]
    internal class EvaluationDirectiveParser : IDirectiveParser
    {
        private string sourceCode;
        private int index;
        private int length;
        private int line;
        private AtParser parser;

        public EvaluationDirectiveParser(
            int index,
            int length,
            string sourceCode,
            int line,
            AtParser parser)
        {
            Debug.Assert(!string.IsNullOrEmpty(sourceCode));
            Debug.Assert(parser != null);

            this.index = index;
            this.length = length;
            this.sourceCode = sourceCode;
            this.line = line;
            this.parser = parser;
        }

        public int Index
        {
            get
            {
                return this.index;
            }
        }

        public int Length
        {
            get
            {
                return this.length;
            }
        }

        public int Line
        {
            get
            {
                return this.line;
            }
        }

        public void Parse()
        {
            this.parser.CurrentDirective.Directives.Add(
                new Evaluation(this.sourceCode, this.line));
        }
    }
}
