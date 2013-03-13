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

namespace Cavingdeep.Dcg.At
{
    /// <summary>
    /// Template generation object interface.
    /// </summary>
    public interface IAtTemplateInstance
    {
        /// <summary>
        /// This instance's template.
        /// </summary>
        IAtTemplate Template
        {
            get;
        }

        /// <summary>
        /// Context information (parameter values) template
        /// requires.
        /// </summary>
        object[] Context
        {
            get;
            set;
        }

        /// <summary>
        /// Renders template as single output template.
        /// </summary>
        /// <returns>Output of template.</returns>
        string Render();

        /// <summary>
        /// Renders template by passsing explicitly a writer.
        /// </summary>
        /// <param name="writer">Writer used to write template
        /// output.</param>
        void Render(TextWriter writer);

        /// <summary>
        /// Renders template as multi-output template.
        /// </summary>
        /// <param name="writers">Dictionary of writers
        /// that template uses for output.</param>
        void Render(IDictionary<string, TextWriter> writers);
    }
}
