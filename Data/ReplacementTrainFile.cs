using Sovoma;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace ZusiStart.Classes
{
    public class ReadTrainFileEventArgs : EventArgs
    {
        public XElement Trains { get; private set; }
        public XNamespace Namespace { get; private set; }

        public ReadTrainFileEventArgs(XNamespace ns, XElement xTrains)
        {
            Namespace = ns;
            Trains = xTrains;
        }
    }

    public delegate void ReadTrainFileEventHandler(object sender, ReadTrainFileEventArgs e);

    public class SaveTrainFileEventArgs : EventArgs
    {
        public XmlWriter Writer { get; private set; }

        public SaveTrainFileEventArgs(XmlWriter writer)
        {
            Writer = writer;
        }
    }

    public delegate void SaveTrainFileEventHandler(object sender, SaveTrainFileEventArgs e);

    class ReplacementTrainFile
    {
        private const float VERSION = 1.0f;

        private Encoding? _encoding;// = Encoding.GetEncoding("ISO-8859-1");
        private readonly string _filename;

        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        public string Filename { get { return _filename; } }

        public event EventHandler? ParseCompleted;

        public event ReadTrainFileEventHandler? ParseDocument;

        public event SaveTrainFileEventHandler? SaveDocument;

        //---------------------------------------------------------------------
        public ReplacementTrainFile(string filename)
        {
            _filename = filename;
        }

        //---------------------------------------------------------------------
        public void Parse()
        {
            XmlReaderSettings xrs = new XmlReaderSettings()
            {
                ValidationType = ValidationType.Schema
            };
            xrs.Schemas.Add(null, "ReplacementTrain.xsd"); // **HLI
            xrs.ValidationType = ValidationType.Schema;   // **HLI
            xrs.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            xrs.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            xrs.ValidationEventHandler += (s, e) => { throw new Exception(e.Message); };

            try
            {
                using (FileStream fs = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    XDocument xdoc = XDocument.Load(XmlReader.Create(fs, xrs));
                    XElement xroot = xdoc.Root;
                    XElement xTrains = xroot.Element("replacementtrains");
                    OnParseDocument(xroot.Name.Namespace, xTrains);
                }

                OnParseCompleted();
            }
            catch (Exception ex)
            {
                if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                {
                    string msg = string.Format("Die Datei\n{0}\nist beschädigt und kann nicht verwendet werden.\n\nInfo: {1}",
                        _filename, ex.Message);
                    MessageBox.Show(msg, "Beschädigte Datei", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //---------------------------------------------------------------------
        public void Save()
        {
            CreateOriginal();
            CreateBackup();

            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = false
            };
            if (_encoding != null)
                xmlWriterSettings.Encoding = _encoding;

            using (XmlWriter writer = XmlWriter.Create(_filename, xmlWriterSettings))
            {
                writer.WriteStartDocument(false);

                writer.WriteStartElement("ZusiStart");
                writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, "replacementTrain.xsd");
                writer.WriteAttributeFloat("version", VERSION, 1);

                writer.WriteStartElement("replacementtrains");

                OnSaveDocument(writer);

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        //---------------------------------------------------------------------
        private void CreateBackup()
        {
            if (File.Exists(_filename))
            {
                string ext = Path.GetExtension(_filename);
                ext.Insert(ext.StartsWith(".") ? 1 : 0, "~");
                string bak = Path.ChangeExtension(_filename, ext);
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
                if (!File.Exists(org))
                {
                    File.Copy(_filename, org);
                }
            }
        }

        //---------------------------------------------------------------------
        private void OnParseCompleted()
        {
            ParseCompleted?.Invoke(this, EventArgs.Empty);
        }

        //---------------------------------------------------------------------
        private void OnParseDocument(XNamespace ns, XElement xTrains)
        {
            ParseDocument?.Invoke(this, new ReadTrainFileEventArgs(ns, xTrains));
        }

        //---------------------------------------------------------------------
        private void OnSaveDocument(XmlWriter writer)
        {
            SaveDocument?.Invoke(this, new SaveTrainFileEventArgs(writer));
        }
    }
}
