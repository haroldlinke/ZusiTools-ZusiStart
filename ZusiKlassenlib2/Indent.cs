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

using System.IO;

namespace ZusiKlassenLib2
{
    public class Indent
    {
        public int Indentation { get; set; }
        public int Value { get; set; }

        public Indent()
            : this(0, 2)
        { }

        public Indent(int value, int indentation)
        {
            Indentation = indentation;
            Value = value;
        }

        public void Increase()
        {
            Value += Indentation;
        }

        public void Decrease()
        {
            if (Value >= Indentation) Value -= Indentation;
        }

        public void Write(TextWriter writer)
        {
            for (int i = 0; i < Value; i++)
            {
                writer.Write(' ');
            }
        }

        public override string ToString()
        {
            return new string(' ', Value);
        }
    }
}
