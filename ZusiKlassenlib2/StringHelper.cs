/*
 * Copyright 2017-2021 Holger Maaß
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;

namespace ZusiKlassenLib2
{
    public static class StringHelper
    {
        //---------------------------------------------------------------------
        private static int LongestAccordance(string a, int startIndexA, string b, int startIndexB)
        {
            int n = Math.Min(a.Length - startIndexA, b.Length - startIndexB);
            for (int i = 0, aa = startIndexA, bb = startIndexB; i < n; i++, aa++, bb++)
            {
                if (a[aa] != b[bb])
                    return i;
            }

            return n;
        }

        //---------------------------------------------------------------------
        public static double Accordance(this string me, string other)
        {
            return Accordance(me, other, false);
        }

        //---------------------------------------------------------------------
        public static double Accordance(this string me, string other, bool ignoreCase)
        {
            double la, laa;

            string a = ignoreCase ? me.ToLower() : me;
            string b = ignoreCase ? other.ToLower() : other;

            if (a.Length == b.Length)
            {
                laa = LongestAccordance(me, 0, other, 0);
                la = laa == 0.0 ? 0.0 : me.Length / laa;
            }
            else
            {
                double sgn = 1.0;
                if (a.Length < b.Length)
                {
                    string c = a;
                    a = b;
                    b = c;
                    sgn = -1.0;
                }
                // b is the shorter string
                int n = int.MinValue;
                int lx = a.Length - b.Length + 1;
                for (int i = 0; i < lx; i++)
                {
                    int k = LongestAccordance(a, i, b, 0);
                    if (k > n)
                        n = k;
                }
                la = n == 0 ? 0.0 : ((double)a.Length / n) * sgn;
            }
            return la;
        }

        //---------------------------------------------------------------------
        public static bool IsVariable(this string s)
        {
            return s.StartsWith("%") && s.EndsWith("%");
        }

        //---------------------------------------------------------------------
        public static void TrimStringArrayMembers(string[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = array[i].Trim();
            }
        }
    }
}
