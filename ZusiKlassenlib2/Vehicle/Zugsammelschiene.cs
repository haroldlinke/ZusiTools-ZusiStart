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

namespace ZusiKlassenLib2.Vehicle
{
    [Flags]
    public enum Zugsammelschiene
    {
        Unbekannt = 0,
        V1000AC = (1 << 0),
        V1000DC = (1 << 1),
        V1500AC = (1 << 2),
        V1500DC = (1 << 3),
        V3000AC = (1 << 4),
        V3000DC = (1 << 5),
        Andere = (1 << 6),
    }
}