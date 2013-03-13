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
using System.Runtime.Serialization;

namespace Cavingdeep.Dcg
{
    /// <summary>
    /// This is thrown when a <see cref="ITemplate"/> instance
    /// cannot be successfully parsed.
    /// </summary>
    [Serializable]
    public class TemplateParsingException : TemplateException
    {
        private int sourceLine;
        private string sourceFileName;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public TemplateParsingException()
        {
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="message">A message that describes the
        /// exception.</param>
        public TemplateParsingException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="message">A message that describes the
        /// exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the innerException parameter is not a null reference,
        /// the current exception is raised in a catch
        /// block that handles the inner exception.</param>
        public TemplateParsingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a new instance with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized
        /// object data.</param>
        /// <param name="context">The contextual information about the
        /// source or destination.</param>
        protected TemplateParsingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets/Sets the source line number where this exception occurred.
        /// </summary>
        /// <value>Source line number.</value>
        public int SourceLine
        {
            get
            {
                return this.sourceLine;
            }

            set
            {
                this.sourceLine = value;
            }
        }

        /// <summary>
        /// Gets/Sets the template's file name where this exception occurred.
        /// If template is not read from file, this returns null.
        /// </summary>
        /// <value>Template file name.</value>
        public string SourceFileName
        {
            get
            {
                return this.sourceFileName;
            }

            set
            {
                this.sourceFileName = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString() + "\r\n"
                   + "File: " + this.sourceFileName + "\r\n"
                   + "Line: " + this.sourceLine + "\r\n";
        }
    }
}