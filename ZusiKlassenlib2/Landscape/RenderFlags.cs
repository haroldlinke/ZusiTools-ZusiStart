using Sovoma;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Landscape
{
    public enum RenderFlagsType
    {
        Individual,                 //  0: individuell
        Default,                    //  1: Standard, eine Textur
        Transparency,               //  2: Standard, eine Textur, Volltransparenz
        Default_Transparency,       //  3: Tex 1 Standard, Tex 2 transparent
        HalfTransparency1,          //  4: Standard, eine Textur, Halbtransparenz
        Default_Lit,                //  5: Tex 1 Standard, Tex 2 transparent/leuchtend
        SignalMask,                 //  6: Signalblende, durchleuchtet
        SignalLamp,                 //  7: Signallampe dimmbar, mit Halbtransparenz
        HalfTransparency2,          //  8: Halbtransparenz für Laub.ähnliche Strukturen
        WindowOverlay_Dimmable,     //  9: überlagertes Fenster, tagsüber ausdimmend
        WindowOverlay_Switchable,   // 10: überlagertes Fenster, tagsüber ausschaltend
        MistZone                    // 11: Nebelwand
    }

    public class RenderFlags : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "SHADEMODE",
            "DESTBLEND",
            "SRCBLEND",
            "ALPHATESTENABLE",
            "ALPHABLENDENABLE",
            "ALPHAREF",
            "TexVoreinstellung"
        };

        private static readonly string[] _knownElems =
        {
            "SubSetTexFlags",
            "SubSetTexFlags2",
            "SubSetTexFlags3"
        };
#pragma warning restore IDE0052
        #endregion

        private static readonly List<int> _semiTransparents = new() { 4, 7, 8 };

        private readonly string _shadeMode;
        /*
         *  0: individuell
         *  1: Standard, eine Textur
         *  2: Standard, eine Textur, Volltransparenz
         *  3: Tex 1 Standard, Tex 2 transparent
         *  4: Standard, eine Textur, Halbtransparenz
         *  5: Tex 1 Standard, Tex 2 transparent/leuchtend
         *  6: Signalblende, durchleuchtet
         *  7: Signallampe dimmbar, mit Halbtransparenz
         *  8: Halbtransparenz für Laub.ähnliche Strukturen
         *  9: überlagertes Fenster, tagsüber ausdimmend
         * 10: überlagertes Fenster, tagsüber ausschaltend
         * 11: Nebelwand
         */
        private readonly RenderFlagsType _flags;
        private readonly List<SubSetTexFlags> _texFlags = new();

        public RenderFlagsType Flags => _flags;
        public bool RenderSemiTransparent { get { return _semiTransparents.Contains((int)_flags); } }
        [Obsolete("Use Flags instead")]
        public int TexVoreinstellung => (int)_flags;

        //---------------------------------------------------------------------
        public RenderFlags(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _shadeMode = x.GetAttrValue("SHADEMODE", "");
            _flags = x.GetAttrEnum<RenderFlagsType>("TexVoreinstellung", RenderFlagsType.Individual);

            for (int i = 0; i < 3; i++)
            {
                string e = "SubSetTexFlags" + i switch
                {
                    1 => "2",
                    2 => "3",
                    _ => ""
                };
                SubSetTexFlags f = GetOptionalObject<SubSetTexFlags>(this, x.Element(e));
                if (f != null)
                {
                    _texFlags.Add(f);
                }
            }
        }

        //---------------------------------------------------------------------
        public RenderFlags(IZusiObjectParent parent, RenderFlags source)
            : base(parent, source)
        {
            _shadeMode = source._shadeMode;
            _flags = source._flags;

            source._texFlags.ForEach(f => _texFlags.Add(new SubSetTexFlags(this, f)));
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeStringIfNotEmpty("SHADEMODE", _shadeMode);
            writer.WriteAttributeIf(_flags != RenderFlagsType.Individual, "TexVoreinstellung", (int)_flags);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _texFlags.ForEach(f => f.Save(writer));
        }

        //---------------------------------------------------------------------
        public static explicit operator RenderFlagsType(RenderFlags f)
        {
            return f.Flags;
        }
    }
}
