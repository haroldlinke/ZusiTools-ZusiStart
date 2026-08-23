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
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Fahrplan
{
    //---------------------------------------------------------------------
    /* .trn-Datei
     */
    [Serializable]
    public class ZugDatei : ZusiDocument<Zug>
    {
        //---------------------------------------------------------------------
        public ZugDatei(IZusiObjectParent parent, string path)
            : base(parent, path, null, "Zug")
        { }

        //---------------------------------------------------------------------
        public ZugDatei(string path, Zug train)
            : base(path, train, "Zug", true)
        { }

        //---------------------------------------------------------------------
        public ZugDatei(ZugDatei source, Zug zug)
            : base(source, zug)
        { }

        //---------------------------------------------------------------------
        public ZugDatei(string path, Zug zug, string docElementTag, bool changeParent = false)
            : base(path, zug, docElementTag, changeParent)
        { }
    }
}
