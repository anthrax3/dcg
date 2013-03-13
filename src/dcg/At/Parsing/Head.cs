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
using System.Text;

namespace Cavingdeep.Dcg.At.Parsing
{
    [Serializable]
    internal class Head
    {
        public Head()
        {
            this.References = new List<Reference>();
            this.Imports = new List<Import>();
            this.Parameters = new List<Parameter>();
            this.Global = new StringBuilder();
        }

        public List<Reference> References
        {
            get;
            private set;
        }

        public List<Import> Imports
        {
            get;
            private set;
        }

        public List<Parameter> Parameters
        {
            get;
            private set;
        }

        public StringBuilder Global
        {
            get;
            private set;
        }
    }
}
