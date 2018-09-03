using System;
using System.Collections.Generic;
using System.Text;

namespace Vultrue.Communication
{
    public class Encodings
    {
        public string Name { get; set; }
        public Encoding Value { get; set; }

        public static Encodings[] GetEnCodings()
        {
            return new Encodings[] {
                new Encodings() { Name = "ASCII", Value = Encoding.ASCII },
                new Encodings() { Name = "UTF8", Value = Encoding.UTF8 },
                new Encodings() { Name = "UTF7", Value = Encoding.UTF7 },
                new Encodings() { Name = "UTF32", Value = Encoding.UTF32 },
                new Encodings() { Name = "Unicode", Value = Encoding.Unicode },
                new Encodings() { Name = "BigEndianUnicode", Value = Encoding.BigEndianUnicode }
            };
        }
    }
}
