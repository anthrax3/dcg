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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Cavingdeep.Dcg.Properties;

namespace Cavingdeep.Dcg.At.Lexing
{
    [Serializable]
    internal class ContextStack
        : ICollection,
          IEnumerable<Context>,
          IEnumerable
    {
        private List<Context> innerList;

        public ContextStack()
        {
            this.innerList = new List<Context>();
        }

        public int Count
        {
            get
            {
                return this.innerList.Count;
            }
        }

        public void Push(Context context)
        {
            this.innerList.Add(context);
        }

        public Context Pop()
        {
            Debug.Assert(this.innerList.Count > 0, Resources.StackEmpty);

            Context lastOne = this.innerList[this.innerList.Count - 1];
            this.innerList.RemoveAt(this.innerList.Count - 1);
            return lastOne;
        }

        public Context Peek()
        {
            Debug.Assert(this.innerList.Count > 0, Resources.StackEmpty);

            return this.innerList[this.innerList.Count - 1];
        }

        public Context PeekPrevious()
        {
            Debug.Assert(this.innerList.Count >= 2, Resources.StackEmpty);

            return this.innerList[this.innerList.Count - 2];
        }

        public bool Contains(Context context)
        {
            return this.innerList.Contains(context);
        }

        public void Clear()
        {
            this.innerList.Clear();
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<Context>) this).GetEnumerator();
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return ((ICollection) this.innerList).IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection) this.innerList).SyncRoot;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection) this.innerList).CopyTo(array, index);
        }

        IEnumerator<Context> IEnumerable<Context>.GetEnumerator()
        {
            return this.innerList.GetEnumerator();
        }
    }
}
