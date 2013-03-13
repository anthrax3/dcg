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
using System.IO;
using System.Reflection;
using System.Text;

namespace Cavingdeep.Dcg
{
    /// <summary>
    /// Manages files that DCG generates.
    /// </summary>
    internal static class TempFileManager
    {
        private const string CleanBat = "clean.bat";
        private const string CleanExe = "Clean.exe";

        private static readonly IDictionary<string, object> files =
            new Dictionary<string, object>();

        static TempFileManager()
        {
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        }

        public static void RegisterFile(string file)
        {
            Debug.Assert(!string.IsNullOrEmpty(file), "file cannot be null or empty.");

            if (!files.ContainsKey(file))
            {
                files.Add(file, null);
            }
        }

        public static void UnRegisterFile(string file)
        {
            Debug.Assert(!string.IsNullOrEmpty(file), "file cannot be null or empty.");

            if (files.ContainsKey(file))
            {
                files.Remove(file);
            }
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string dir = Path.GetDirectoryName(asm.Location);

            OutputCleanBat(asm, dir);
            OutputCleanExe(asm, dir);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = dir;
            startInfo.FileName = CleanBat;
            StringBuilder buffer = new StringBuilder();
            buffer.Append(Process.GetCurrentProcess().Id);
            buffer.Append(' ');
            foreach (string file in files.Keys)
            {
                buffer.Append("\"" + file + "\" ");
            }

            startInfo.Arguments = buffer.ToString().TrimEnd();
            startInfo.UseShellExecute = false;

            Process.Start(startInfo);
        }

        private static void OutputCleanExe(Assembly asm, string dir)
        {
            using (FileStream file = new FileStream(
                Path.Combine(dir, CleanExe),
                FileMode.OpenOrCreate,
                FileAccess.Write))
            using (Stream stream = asm.GetManifestResourceStream(CleanExe))
            {
                int b;
                while ((b = stream.ReadByte()) >= 0)
                {
                    file.WriteByte((byte) b);
                }
            }
        }

        private static void OutputCleanBat(Assembly asm, string dir)
        {
            using (StreamWriter writer = new StreamWriter(
                Path.Combine(dir, CleanBat), false))
            using (StreamReader reader = new StreamReader(
                asm.GetManifestResourceStream(CleanBat)))
            {
                writer.Write(reader.ReadToEnd());
            }
        }
    }
}
