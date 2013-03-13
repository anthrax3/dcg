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
using System.IO;

namespace Cavingdeep.Dcg.Clean
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            if (args != null && args.Length > 1)
            {
                try
                {
                    int processId = int.Parse(args[0]);
                    Process p = Process.GetProcessById(processId);
                    p.WaitForExit();
                }
                catch
                {
                    // Ignore any potential error.
                }
                finally
                {
                    DeleteFiles(args);
                }
            }
        }

        private static void DeleteFiles(string[] args)
        {
            for (int i = 1; i < args.Length; i++)
            {
                string file = args[i];

                if (!string.IsNullOrEmpty(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore files that can't be deleted.
                    }
                }
            }
        }
    }
}
