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

namespace Cavingdeep.Dcg.At.Parsing
{
    [Serializable]
    internal class StaticText : Directive
    {
        public StaticText(string value, int line)
        {
            this.Value = value;
            this.Line = line;
            this.IsStartOfLine = false;
            this.IsNewLine = true;
        }

        public string Value
        {
            get;
            set;
        }

        public int Line
        {
            get;
            set;
        }

        public bool IsStartOfLine
        {
            get;
            set;
        }

        public bool IsNewLine
        {
            get;
            set;
        }
    }
}
