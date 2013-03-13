using System;

//{imports}

namespace Cavingdeep.Tests.Generated
{
    public class Generator
    {
        private System.Collections.Generic.IDictionary<string, System.IO.TextWriter> writers;
        private InnerDcg dcg;

        //{fields}

        //{globals}

        string p1;
        string p2;

        string Captalize(string text)
        {
            return text;
        }

        private InnerDcg Dcg
        {
            get {return this.dcg;}
        }

        public void Generate(
            System.Collections.Generic.IDictionary<string, System.IO.TextWriter> writers,
            string p1, string p2)
        {
            this.writers = writers;

            //{parameters_set}
            this.p1 = p1;
            this.p2 = p2;

            this.Generate();
        }

        public Generator()
        {
            this.dcg = new InnerDcg(this);
        }

        private void Generate()
        {
            //{body}
            this.writers["_main_"].Write(@"1 + 1 = ");
            this.writers["_main_"].Write(1 + 1);

            Dcg.Write("Appended by Dcg.Write");

            foreach (System.IO.TextWriter writer in this.writers.Values)
            {
                writer.Flush();
            }
        }

        private void FillSpaces(string spaces, string key, string text)
        {
            using (System.IO.StringReader reader = new System.IO.StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    this.writers[key].Write(spaces);
                    this.writers[key].Write(line);
                    this.writers[key].WriteLine();
                }
            }
        }

        private class InnerDcg
        {
            private static System.Collections.Generic.Dictionary<string, Cavingdeep.Dcg.At.AtTemplate> cache =
                new System.Collections.Generic.Dictionary<string, Cavingdeep.Dcg.At.AtTemplate>();

            public static readonly System.IO.FileInfo fileInfo =
                null;

            private Generator owner;

            public InnerDcg(Generator owner)
            {
                this.owner = owner;
            }

            public System.IO.FileInfo FileInfo
            {
                get {return fileInfo;}
            }

            public void Write(object obj)
            {
                Write(obj, "_main_");
            }

            public void Write(object obj, string writerKey)
            {
                if (string.IsNullOrEmpty(writerKey))
                {
                    throw new ArgumentNullException("writerKey");
                }
                if (!this.owner.writers.ContainsKey(writerKey))
                {
                    throw new ArgumentOutOfRangeException("writerKey");
                }

                this.owner.writers[writerKey].Write(obj.ToString());
            }

            public string CallTemplate(
                string templateFile, System.Text.Encoding encoding,
                params object[] values)
            {
                if (!System.IO.Path.IsPathRooted(templateFile) &&
                    this.FileInfo != null)
                {
                    templateFile = System.IO.Path.Combine(
                        this.FileInfo.DirectoryName, templateFile);
                }

                Cavingdeep.Dcg.At.AtTemplate template;
                if (cache.ContainsKey(templateFile))
                {
                    template = cache[templateFile];
                }
                else
                {
                    template = new Cavingdeep.Dcg.At.AtTemplate(templateFile, encoding);
                    template.Parse();
                    cache.Add(templateFile, template);
                }
                template.Context = values;
                return template.Render();
            }
        }
    }
}
