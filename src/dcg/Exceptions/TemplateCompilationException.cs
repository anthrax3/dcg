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
using System.Runtime.Serialization;
using System.Text;
using Cavingdeep.Dcg.Properties;

namespace Cavingdeep.Dcg
{
    /// <summary>
    /// Thrown when template's dynamic code cannot be compiled.
    /// </summary>
    [Serializable]
    public class TemplateCompilationException : TemplateException, ISerializable
    {
        private const string ErrorCollection = "errors";

        private CompilerErrorCollection errors;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="errors">A collection containing compilation
        /// errors.</param>
        public TemplateCompilationException(CompilerErrorCollection errors)
        {
            Initialize(errors);
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="message">A message that describes the
        /// exception.</param>
        /// <param name="errors">A collection containing compilation
        /// errors.</param>
        public TemplateCompilationException(string message, CompilerErrorCollection errors)
            : base(message)
        {
            Initialize(errors);
        }

        /// <summary>
        /// Creates a new instance with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized
        /// object data.</param>
        /// <param name="context">The contextual information about
        /// the source or destination.</param>
        protected TemplateCompilationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.errors = (CompilerErrorCollection) info.GetValue(
                ErrorCollection,
                typeof(CompilerErrorCollection));
        }

        /// <summary>
        /// Gets compilation errors.
        /// </summary>
        public CompilerErrorCollection Errors
        {
            get
            {
                return this.errors;
            }
        }

        /// <summary>
        /// A string representing compilation errors.
        /// </summary>
        /// <returns>String representation of compilation
        /// errors.</returns>
        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder(base.ToString());
            buffer.Append(Environment.NewLine);

            foreach (CompilerError error in this.errors)
            {
                buffer.Append(error);
                buffer.Append(Environment.NewLine);
            }

            return buffer.ToString();
        }

        #region ISerializable Members

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(
                ErrorCollection,
                this.errors,
                typeof(CompilerErrorCollection));

            GetObjectData(info, context);
        }

        #endregion

        private void Initialize(CompilerErrorCollection errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException("errors");
            }

            if (errors.Count == 0)
            {
                throw new ArgumentException(Resources.ErrorsCount, "errors");
            }

            this.errors = errors;
        }
    }
}
