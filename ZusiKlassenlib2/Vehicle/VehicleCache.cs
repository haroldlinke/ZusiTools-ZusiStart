using Sovoma;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Vehicle
{
  public class VehicleCache : Singleton<VehicleCache>, ISingletonBase
  {
    private readonly ConcurrentDictionary<string, Fahrzeug> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    //---------------------------------------------------------------------
    internal static void Add(string path, Fahrzeug fahrzeug)
    {
      Instance.Add_impl(path, fahrzeug);
    }

    //---------------------------------------------------------------------
    public static Fahrzeug GetFahrzeug(Datei datei)
    {
      return Instance.GetFahrzeug_impl(datei);
    }

    //---------------------------------------------------------------------
    public static Fahrzeug GetFahrzeug(string fullPath)
    {
      return Instance.GetFahrzeug_impl(fullPath);
    }

    //---------------------------------------------------------------------
    public void Initialize()
    { }

    //---------------------------------------------------------------------
    internal static void RemoveFahrzeug(string path)
    {
      Instance.RemoveFahrzeug_impl(path);
    }

    //---------------------------------------------------------------------
    private void Add_impl(string path, Fahrzeug fahrzeug)
    {
      if (string.IsNullOrEmpty(path)) return;
      // allow replacement; do not add null values
      if (fahrzeug is null)
        _cache.TryRemove(path, out _);
      else
        _cache[path] = fahrzeug;
    }

    //---------------------------------------------------------------------
    private Fahrzeug GetFahrzeug_impl(Datei datei)
    {
      return GetFahrzeug_impl(datei.FullPath);
    }

    //---------------------------------------------------------------------
    private Fahrzeug GetFahrzeug_impl(string path)
    {
      if (string.IsNullOrEmpty(path)) return null;

      // ConcurrentDictionary uses a case-insensitive comparer,
      // so no explicit ToLowerInvariant required.
      if (_cache.TryGetValue(path, out var existing))
        return existing;

      // parse outside dictionary operations to avoid storing nulls
      var fd = new FahrzeugDatei(path);
      fd.Parse();
      if (fd.Root == null)
        return null;

      // ensure only one thread stores the parsed object
      return _cache.GetOrAdd(path, fd.Root);
    }

    //---------------------------------------------------------------------
    private void RemoveFahrzeug_impl(string path)
    {
      if (!string.IsNullOrEmpty(path))
      {
        _cache.TryRemove(path, out _);
      }
    }
  }
}