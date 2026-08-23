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
  public abstract class AntriebsmodellElektrisch : Antriebsmodell
  {
    #region attributes & elements
#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "Volt",
            "etaTrafo",
            "GrenzdruckHBL",
            "MaxOberstrom",
            "GrenzwertUeberwacht"
        };

    private static readonly string[] _knownElems =
    {
            "SoundLuefter"
        };
#pragma warning restore IDE0052
    #endregion

    public AntriebsmodellElektrisch(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    { }
  }

  //---------------------------------------------------------------------
  [Serializable]
  public class AntriebsmodellElektrischReihenschluss : AntriebsmodellElektrisch
  {
#pragma warning disable IDE0052
    private static readonly string[] _knownElems =
    {
            "Reihenschlussmotor"
        };
#pragma warning restore IDE0052

    public AntriebsmodellElektrischReihenschluss(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    { }

    protected override DrivetrainType GetDrivetrainType() => DrivetrainType.ElectricSeriesMotor;

    protected override string GetName() => "elektrisch (Reihenschlussmotor)";
  }

  //---------------------------------------------------------------------
  [Serializable]
  public class AntriebsmodellElektrischDrehstrom : AntriebsmodellElektrisch
  {
#pragma warning disable IDE0052

#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "HauptschalterVerzoegerungEin",
            "HauptschalterVerzoegerungZugkraft"
        };

    private static readonly string[] _knownElems =
    {
            "Drehstrommotor"
        };
#pragma warning restore IDE0052

    public AntriebsmodellElektrischDrehstrom(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    { }

    protected override DrivetrainType GetDrivetrainType() => DrivetrainType.ElectricThreePhaseMotor;

    protected override string GetName() => "elektrisch (Drehstrommotor)";
  }

  //---------------------------------------------------------------------
  [Serializable]
  public class AntriebsmodellDieselElektrischDrehstrom : AntriebsmodellElektrisch
  {
#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "HauptschalterVerzoegerungEin",
            "HauptschalterVerzoegerungZugkraft",
            "OhneHS"
        };

    private static readonly string[] _knownElems =
    {
            "Drehstrommotor",
            "Dieselmotor"
        };
#pragma warning restore IDE0052

    public AntriebsmodellDieselElektrischDrehstrom(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    { }

    protected override DrivetrainType GetDrivetrainType() => DrivetrainType.DieselElectricThreePhaseMotor;

    protected override string GetName() => "dieselelektrisch (Drehstrommotor)";
  }
}
