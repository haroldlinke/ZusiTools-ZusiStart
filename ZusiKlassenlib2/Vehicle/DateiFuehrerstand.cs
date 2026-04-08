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
using System.Xml.Linq;
using ZusiKlassenLib2.Common;
//using ZusiKlassenLib2.Cab;

namespace ZusiKlassenLib2.Vehicle
{
    //---------------------------------------------------------------------
    [Serializable]
    public class DateiFuehrerstand : Datei //ZusiDocument<DriversCab>
  {
    public DateiFuehrerstand(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    { }

    public DateiFuehrerstand(IZusiObjectParent parent, XElement x, string nodeName)
        : base(parent, x, nodeName)
    { }
    //---------------------------------------------------------------------
    //public DateiFuehrerstand(IZusiObjectParent parent, string path)
    //    : base(parent, path, null, "Fuehrerstand")
    //{ }

    ////---------------------------------------------------------------------
    //public DateiFuehrerstand(DateiFuehrerstand source, string filename)
    //    : base(source, filename)
    //{ }
  }
}
