using Sovoma;
using System;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2
{
    public class Face : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "i"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly int[] _i = new int[3];

        public int this[int index]
        {
            get
            {
                System.Diagnostics.Debug.Assert(index >= 0 && index <= 2, "index out of range [0..2]");
                return _i[index];
            }
        }

        //---------------------------------------------------------------------
        public Face(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            string i = x.GetAttrValue("i", "");
            if (!string.IsNullOrEmpty(i))
            {
                string[] ss = i.Split(new char[] { ';' });
                if (ss.Length < 3)
                {
                    throw new Exception($"Face: invalid attribute value 'i': {i}");
                }
                for (int k = 0; k < 3; k++)
                {
                    _i[k] = int.Parse(ss[k]);
                }
            }
            else
            {
                throw new Exception("Face: attribute i may not be empty");
            }
        }

        //---------------------------------------------------------------------
        public Face(IZusiObjectParent parent, Face source)
            : base(parent, source)
        {
            for (int k = 0; k < 3; k++)
            {
                _i[k] = source._i[k];
            }
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            string s = string.Format("{0};{1};{2};", _i[0], _i[1], _i[2]);
            writer.WriteAttributeString("i", s);
        }
    }
}
