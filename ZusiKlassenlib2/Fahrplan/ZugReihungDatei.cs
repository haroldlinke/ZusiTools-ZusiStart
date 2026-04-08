using System;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Fahrplan
{
    [Serializable]
    public class ZugReihungDatei : ZusiDocument<ZugReihung>
    {
        public ZugReihungDatei(string path)
            : base(null, path, null, "FahrzeugVarianten")
        { }
    }
}
