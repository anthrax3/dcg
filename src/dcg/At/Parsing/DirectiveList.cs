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
using System.Diagnostics;

namespace Cavingdeep.Dcg.At.Parsing
{
    [Serializable]
    internal class DirectiveList : IList<Directive>
    {
        private List<Directive> innerList = new List<Directive>();
        private Directive owner;

        public DirectiveList(Directive owner)
        {
            Debug.Assert(owner != null, "owner cannot be null.");

            this.owner = owner;
        }

        #region IList<Directive> Members

        public int IndexOf(Directive item)
        {
            return this.innerList.IndexOf(item);
        }

        public void Insert(int index, Directive item)
        {
            this.innerList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            Directive directive = this.innerList[index];
            this.innerList.RemoveAt(index);
            directive.Parent = null;
        }

        public Directive this[int index]
        {
            get
            {
                return this.innerList[index];
            }

            set
            {
                this.innerList[index] = value;
            }
        }

        #endregion

        #region ICollection<Directive> Members

        public void Add(Directive item)
        {
            this.innerList.Add(item);
            item.Parent = this.owner;
        }

        public void Clear()
        {
            this.innerList.Clear();
        }

        public bool Contains(Directive item)
        {
            return this.innerList.Contains(item);
        }

        public void CopyTo(Directive[] array, int arrayIndex)
        {
            this.innerList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return this.innerList.Count;
            }
        }

        bool ICollection<Directive>.IsReadOnly
        {
            get
            {
                return ((ICollection<Directive>) this.innerList).IsReadOnly;
            }
        }

        public bool Remove(Directive item)
        {
            bool succeed = this.innerList.Remove(item);

            if (succeed)
            {
                item.Parent = null;
            }

            return succeed;
        }

        #endregion

        #region IEnumerable<Directive> Members

        public IEnumerator<Directive> GetEnumerator()
        {
            return this.innerList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable) this.innerList).GetEnumerator();
        }

        #endregion
    }
}
