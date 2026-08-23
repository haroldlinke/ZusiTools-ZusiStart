using System.Globalization;
using System.Windows.Media;

namespace ZusiKlassenLib2.Common
{
    public static class ZusiColor
    {
        //---------------------------------------------------------------------
        public static Color Coalescence(this Color self, Color other)
        {
            byte r = (byte)((self.R + other.R) & 0xff);
            byte g = (byte)((self.G + other.G) & 0xff);
            byte b = (byte)((self.B + other.B) & 0xff);
            return Color.FromArgb(self.A, r, g, b);
        }

        //---------------------------------------------------------------------
        public static bool TryParse(string s, out Color color)
        {
            color = default;

            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            return ToColor(s, false, out color);
        }

        //---------------------------------------------------------------------
        public static bool TryParse(string s, bool hasAlpha, out Color color)
        {
            color = default;

            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            return ToColor(s, hasAlpha, out color);
        }

        //---------------------------------------------------------------------
        public static bool TryParse(string s, bool rgba, bool hasAlpha, out Color color)
        {
            color = default;

            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            if (rgba)
            {
                return ToColorRGBA(s, out color);
            }
            else
            {
                return ToColor(s, hasAlpha, out color);
            }
        }

        //---------------------------------------------------------------------
        public static bool IsTransparent(this Color clr)
        {
            return clr.A == 0;
        }

        //---------------------------------------------------------------------
        public static bool IsBlack(this Color clr)
        {
            return clr.R == 0 && clr.G == 0 && clr.B == 0;
        }

        //---------------------------------------------------------------------
        public static bool IsTrueBlack(this Color clr)
        {
            return clr.A == 255 && clr.R == 0 && clr.G == 0 && clr.B == 0;
        }

        //---------------------------------------------------------------------
        public static bool IsTrueWhite(this Color clr)
        {
            return clr.A == 255 && clr.R == 255 && clr.G == 255 && clr.B == 255;
        }

        //---------------------------------------------------------------------
        public static bool IsWhite(this Color clr)
        {
            return clr.R == 255 && clr.G == 255 && clr.B == 255;
        }

        //---------------------------------------------------------------------
        private static bool ToColor(string s, bool hasAlpha, out Color color)
        {
            bool res = false;
            color = Colors.Transparent;

            if (s.Length >= 8)
            {
                byte g = 0;
                byte r = 0;
                byte a = 255;
                byte b;
                while (true)
                {
                    if (!byte.TryParse(s.Substring(s.Length - 2, 2), NumberStyles.AllowHexSpecifier, null, out b)) break;
                    if (!byte.TryParse(s.Substring(s.Length - 4, 2), NumberStyles.AllowHexSpecifier, null, out g)) break;
                    if (!byte.TryParse(s.Substring(s.Length - 6, 2), NumberStyles.AllowHexSpecifier, null, out r)) break;
                    if (hasAlpha)
                    {
                        byte.TryParse(s.Substring(s.Length - 8, 2), NumberStyles.AllowHexSpecifier, null, out a);
                    }
                    res = true;
                    break;
                }
                color = Color.FromArgb(a, r, g, b);
            }
            return res;
        }

        //---------------------------------------------------------------------
        private static bool ToColorRGBA(string s, out Color color)
        {
            bool res = false;
            color = Colors.Transparent;

            if (s.Length >= 8)
            {
                byte a = 255;
                byte b = 0;
                byte g = 0;
                byte r;
                while (true)
                {
                    if (!byte.TryParse(s.Substring(s.Length - 2, 2), NumberStyles.AllowHexSpecifier, null, out r)) break;
                    if (!byte.TryParse(s.Substring(s.Length - 4, 2), NumberStyles.AllowHexSpecifier, null, out g)) break;
                    if (!byte.TryParse(s.Substring(s.Length - 6, 2), NumberStyles.AllowHexSpecifier, null, out b)) break;
                    byte.TryParse(s.Substring(s.Length - 8, 2), NumberStyles.AllowHexSpecifier, null, out a);
                    res = true;
                    break;
                }
                color = Color.FromArgb((byte)(255 - a), r, g, b);
            }
            return res;
        }
    }
}
