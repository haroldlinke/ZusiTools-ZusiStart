/*
 * Copyright 2018-2021 Holger Maaß
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
 * 
 * 4.3 - 01.01.2021
 * ParseXml: Parameter 'throwOnError' hinzugefügt
*/

using log4net;
using Sovoma;
using Sovoma.WPF;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ZusiKlassenLib2.Common
{
  [Serializable]
  public abstract class ZusiDocumentBase : DirtyObjectBase, IZusiObjectParent
  {
    [Flags]
    public enum WriteFlags
    {
      None,
      CreateOrigin = (1 << 0),
      CreateBackup = (1 << 1),
      WriteEncoding = (1 << 2),
      WriteComment = (1 << 3)
    }

    protected static readonly ILog _log = LogManager.GetLogger(typeof(ZusiDocumentBase));

    private readonly IZusiObjectParent _parent;
    private string _comment;
    private string _filename;
    private string _path;
    private readonly bool _readonly;
    private WriteFlags _writeFlags;

    protected Info _info;
    protected Encoding _encoding;
    protected string _startElement = "Zusi";

    //---------------------------------------------------------------------
    public string Comment
    {
      get { return _comment; }
      set { _comment = value; }
    }

    //---------------------------------------------------------------------
    public Encoding Encoding
    {
      get { return _encoding; }
      set { _encoding = value; }
    }

    //---------------------------------------------------------------------
    public string Filename { get { return _filename; } }

    //---------------------------------------------------------------------
    public Info Info { get { return _info; } }

    //---------------------------------------------------------------------
    public bool IsWritable { get { return !_readonly; } }

    //---------------------------------------------------------------------
    public string Path
    {
      get { return _path; }
      set { SetPath(value); }
    }

    //---------------------------------------------------------------------
    public IZusiObjectParent Parent { get => _parent; }

    //---------------------------------------------------------------------
    public WriteFlags WriteOptions => _writeFlags;

    //---------------------------------------------------------------------
    public ZusiDocumentBase(IZusiObjectParent parent, string filename, Encoding encoding)
    {
      _parent = parent;
      _readonly = true;
      _filename = filename;
      _encoding = encoding;
      if (!string.IsNullOrEmpty(_filename))
      {
        _path = System.IO.Path.GetDirectoryName(filename).EnsureTrailingBackslash();
      }
    }

    //---------------------------------------------------------------------
    public ZusiDocumentBase(string filename, WriteFlags flags)
    {
      _readonly = false;
      _writeFlags = flags;
      _filename = filename;
      _encoding = Encoding.UTF8;
      if (!string.IsNullOrEmpty(_filename))
      {
        _path = System.IO.Path.GetDirectoryName(filename).EnsureTrailingBackslash();
      }
    }

    //---------------------------------------------------------------------
    public ZusiDocumentBase(ZusiDocumentBase source)
        : base(source)
    {
      _comment = source._comment;
      _encoding = source._encoding;
      _filename = source._filename;
      _path = source._path;
      _readonly = source._readonly;
      _writeFlags = source._writeFlags;
    }

    //---------------------------------------------------------------------
    public virtual void AddMyAuthorEntry()
    { }

    //---------------------------------------------------------------------
    public T FindParent<T>() where T : IZusiObjectParent
    {
      return default;
    }

    //---------------------------------------------------------------------
    public void Parse()
    {
      //Parse(false);
      Parse(true);

    }

    //---------------------------------------------------------------------
    public void Parse(bool throwOnError)
    {
      System.Diagnostics.Debug.Assert(_encoding != null, "This file has no encoding");

      try
      {
        using FileStream fs = new(_filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        using StreamReader sr = new(fs, _encoding);
        ParseXml(sr.ReadToEnd(), throwOnError);
      }
      catch (Exception ex)
      {
        if (throwOnError)
        {
          throw;
        }
        else
        {
#if DEBUG
          _log.Error(ex.ToString());
#else
                    _log.Error(ex.Message);
#endif
        }
      }
    }

    //---------------------------------------------------------------------
    public void ParseXml(string xml, bool throwOnError = false)
    {
      if (xml != null)
      {
        try
        {
          using StringReader sr = new(xml);
          ParseDocument(XDocument.Load(XmlReader.Create(sr)));
        }
        catch (Exception ex)
        {
          _log.Error($"{_filename}: {ex.Message}");
          if (throwOnError) throw;
        }
      }
    }

    //---------------------------------------------------------------------
    public void Save() => Save(false);

    //---------------------------------------------------------------------
    public void Save(bool force)
    {
      if (force || IsDirty)
      {
        if (_writeFlags.HasFlag(WriteFlags.CreateOrigin))
        {
          CreateOriginal();
        }
        if (_writeFlags.HasFlag(WriteFlags.CreateBackup))
        {
          CreateBackup();
        }

        XmlWriterSettings xmlWriterSettings = new()
        {
          Indent = true,
          OmitXmlDeclaration = false,
        };
        if (_writeFlags.HasFlag(WriteFlags.WriteEncoding) && _encoding != null)
        {
          xmlWriterSettings.Encoding = _encoding;
        }

        EnsureDirectory();

        using XmlWriter writer = XmlWriter.Create(_filename, xmlWriterSettings);
        writer.WriteStartDocument(true);
        if (_writeFlags.HasFlag(WriteFlags.WriteComment) && !string.IsNullOrEmpty(_comment))
        {
          writer.WriteComment(_comment);
        }
        writer.WriteStartElement(_startElement);
        SaveDocument(writer);
        writer.WriteEndElement();
        writer.WriteEndDocument();
      }
    }

    //---------------------------------------------------------------------
    public void Save(WriteFlags flags)
    {
      _writeFlags = flags;
      Save(false);
    }

    //---------------------------------------------------------------------
    public virtual void SaveAs(string filename) => SaveAs(filename, WriteFlags.None);

    //---------------------------------------------------------------------
    public void SaveAs(string filename, WriteFlags flags)
    {
      _filename = filename;
      _path = System.IO.Path.GetDirectoryName(filename).EnsureTrailingBackslash();
      _writeFlags = flags;
      Save(true);
    }

    protected abstract void ParseDocument(XDocument doc);
    protected abstract void SaveDocument(XmlWriter writer);

    //---------------------------------------------------------------------
    protected virtual void SetPath(string path)
    {
      if (string.Compare(_path, path, true) != 0)
      {
        _path = path;
        RaisePropertyChanged(nameof(Path));
      }
    }

    //---------------------------------------------------------------------
    private void CreateBackup()
    {
      if (File.Exists(_filename))
      {
        string ext = "~" + System.IO.Path.GetExtension(_filename);
        string bak = System.IO.Path.ChangeExtension(_filename, ext);
        if (File.Exists(bak))
        {
          File.Delete(bak);
        }
        if (File.Exists(_filename))
        {
          File.Move(_filename, bak);
        }
      }
    }

    //---------------------------------------------------------------------
    private void CreateOriginal()
    {
      if (File.Exists(_filename))
      {
        string org = _filename + ".org";
        if (!System.IO.File.Exists(org))
        {
          System.IO.File.Copy(_filename, org);
        }
      }
    }

    //---------------------------------------------------------------------
    private void EnsureDirectory()
    {
      string dir = System.IO.Path.GetDirectoryName(_filename);
      if (!Directory.Exists(dir))
      {
        Directory.CreateDirectory(dir);
      }
    }

#if ZusiPatsch
        //---------------------------------------------------------------------
        private string TryCorrectError(string error, string xmlString)
        {
            // error   "'PerZufallUebernehmen' ist ein doppelter Attributname. Zeile 14, Position 65." string

            string s = xmlString;
            if (error.Contains("doppelter Attributname"))
            {
                string pattern = "(\\w+(?:=\"[0|1]\"))\\s+\\1\\s";
                Regex rxrep = new Regex(pattern);

                Match mm;
                do
                {
                    mm = rxrep.Match(s);
                    if (mm.Success)
                    {
                        s = rxrep.Replace(s, $"{mm.Groups[1].Value} ");
                    }
                } while (mm.Success);

                string pathErr = $"{this.Filename}.err";
                if (!File.Exists(pathErr))
                {
                    File.Move(Filename, pathErr);

                    using (StreamWriter sw = new StreamWriter(Filename, false, Encoding.UTF8))
                    {
                        sw.Write(s);
                        sw.Flush();
                    }
                }

                WasErrorneous = true;
            }

            return s;
        }
#endif
  }
}
