/*
 * Copyright 2017 Holger Maaß
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

namespace ZusiKlassenLib2.Vehicle
{
    //---------------------------------------------------------------------
    [Serializable]
    public class DynbremseElektrReihenschluss : DynbremseE
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Bremswiderstand",
            "Fremderregt"
        };

    private static readonly string[] _knownElems =
        {
            "BremsePE",
            "BremseRE",
            "BremseE160"
        };
#pragma warning restore IDE0052

    public DynbremseElektrReihenschluss(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
