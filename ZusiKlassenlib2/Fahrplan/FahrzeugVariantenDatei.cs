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
    /* .trn.xml-Datei
     */
    [Serializable]
    public class FahrzeugVariantenDatei : ZusiDocument<FahrzeugVarianten>
    {
        //---------------------------------------------------------------------
        public FahrzeugVariantenDatei(IZusiObjectParent parent, string path)
            : base(parent, path, null, "FahrzeugVarianten")
        { }

        //---------------------------------------------------------------------
        public FahrzeugVariantenDatei(string path, FahrzeugVarianten fv)
            : base(path, fv, "FahrzeugVarianten", true)
        { }

        //---------------------------------------------------------------------
        public FahrzeugVariantenDatei(FahrzeugVariantenDatei source, FahrzeugVarianten fv)
            : base(source, fv)
        { }

        //---------------------------------------------------------------------
        public FahrzeugVariantenDatei(string path, FahrzeugVarianten fv, string docElementTag, bool changeParent = false)
            : base(path, fv, docElementTag, changeParent)
        { }
    }
}
