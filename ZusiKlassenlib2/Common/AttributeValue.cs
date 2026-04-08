using Sovoma;
using System;
using System.Xml;
using System.Xml.Linq;

namespace ZusiKlassenLib2.Common
{
    //=========================================================================
    [Serializable]
    public abstract class AttributeValue<T> : ICloneable
    {
        protected string _name;
        protected T _value;
        protected bool _writeAlways;

        //---------------------------------------------------------------------
        public string Name { get { return _name; } }

        //---------------------------------------------------------------------
        public T Value
        {
            get { return _value; }
            set { _value = value; }
        }

        //---------------------------------------------------------------------
        public AttributeValue(string name, T value, bool writeAlways = false)
        {
            _name = name;
            _value = value;
            _writeAlways = writeAlways;
        }

        //---------------------------------------------------------------------
        public AttributeValue(XElement x, string name, bool writeAlways = false)
        {
            _name = name;
            _value = x.GetAttrValue(_name, default(T));
            _writeAlways = writeAlways;
        }

        //---------------------------------------------------------------------
        public AttributeValue(XElement x, string name, T defaultValue, bool writeAlways = false)
        {
            _name = name;
            _value = x.GetAttrValue(_name, defaultValue);
            _writeAlways = writeAlways;
        }

        //---------------------------------------------------------------------
        public AttributeValue(AttributeValue<T> source)
        {
            _name = source._name;
            _value = source._value;
            _writeAlways = source._writeAlways;
        }

        //---------------------------------------------------------------------
        protected AttributeValue(string name, bool writeAlways = false)
        {
            _name = name;
            _writeAlways = writeAlways;
        }

        //---------------------------------------------------------------------
        private AttributeValue()
        { }

        //---------------------------------------------------------------------
        public object Clone()
        {
            AttributeValue<T> clone = (AttributeValue<T>)Activator.CreateInstance(GetType());
            clone._name = _name;
            clone._value = _value;
            clone._writeAlways = _writeAlways;
            return clone;
        }

        //---------------------------------------------------------------------
        public override string ToString()
        {
            return _value.ToString();
        }

        //---------------------------------------------------------------------
        public abstract void Write(XmlWriter writer);

        //---------------------------------------------------------------------
        public virtual void Write(XmlWriter writer, T defaultValue)
        {
            throw new NotImplementedException();
        }
    }

    //=========================================================================
    [Serializable]
    public class BoolAttribute : AttributeValue<bool>
    {
        //---------------------------------------------------------------------
        public BoolAttribute(string name, bool value, bool writeAlways = false)
            : base(name, value, writeAlways)
        { }

        //---------------------------------------------------------------------
        public BoolAttribute(XElement x, string name, bool writeAlways = false)
            : base(name, writeAlways)
        {
            _value = x.GetAttrValue(name, false);
        }

        //---------------------------------------------------------------------
        public BoolAttribute(XElement x, string name, bool defaultValue, bool writeAlways = false)
            : base(name, defaultValue, writeAlways)
        {
            _value = x.GetAttrValue(name, defaultValue);
        }

        //---------------------------------------------------------------------
        public BoolAttribute(BoolAttribute source)
            : base(source)
        { }

        //---------------------------------------------------------------------
        public override void Write(XmlWriter writer)
        {
            writer.WriteAttributeIf(_writeAlways || _value, _name, 1);
        }

        //---------------------------------------------------------------------
        public static implicit operator bool(BoolAttribute ba)
        {
            return ba.Value;
        }
    }

    //=========================================================================
    [Serializable]
    public class EnumAttribute<T> : AttributeValue<T>
    {
        public EnumAttribute(string name, T value, bool writeAlways = false)
            : base(name, value, writeAlways)
        { }

        //---------------------------------------------------------------------
        public EnumAttribute(XElement x, string name, bool writeAlways = false)
            : base(name, writeAlways)
        {
            int i = x.GetAttrValue(name, -1);
            _value = i == -1 ? default : (T)(object)i;
        }

        //---------------------------------------------------------------------
        public EnumAttribute(EnumAttribute<T> source)
            : base(source)
        { }

        //---------------------------------------------------------------------
        public override void Write(XmlWriter writer)
        {
            Write(writer, default);
        }

        //---------------------------------------------------------------------
        public override void Write(XmlWriter writer, T defaultValue)
        {
            writer.WriteAttributeIf(!_value.Equals(defaultValue) || _writeAlways, _name, (int)(object)_value);
        }
    }

    //=========================================================================
    [Serializable]
    public class FloatAttribute : AttributeValue<float>
    {
        //---------------------------------------------------------------------
        public FloatAttribute(string name, float value, bool writeAlways = false)
            : base(name, value, writeAlways)
        { }

        //---------------------------------------------------------------------
        public FloatAttribute(XElement x, string name, bool writeAlways = false)
            : base(name, writeAlways)
        {
            _value = x.GetAttrValue(name, 0.0f);
        }

        //---------------------------------------------------------------------
        public FloatAttribute(XElement x, string name, float defaultValue, bool writeAlways = false)
            : base(name, defaultValue, writeAlways)
        {
            _value = x.GetAttrValue(name, defaultValue);
        }

        //---------------------------------------------------------------------
        public FloatAttribute(FloatAttribute source)
            : base(source)
        { }

        //---------------------------------------------------------------------
        public override void Write(XmlWriter writer)
        {
            writer.WriteAttributeFloatIf(_value != 0.0f || _writeAlways, _name, _value, -1);
        }

        //---------------------------------------------------------------------
        public void Write(XmlWriter writer, int decimals)
        {
            writer.WriteAttributeFloatIf(_writeAlways || _value != 0.0f, _name, _value, decimals);
        }

        //---------------------------------------------------------------------
        public static implicit operator float(FloatAttribute ba)
        {
            return ba.Value;
        }
    }

    //=========================================================================
    [Serializable]
    public class IntAttribute : AttributeValue<int>
    {
        //---------------------------------------------------------------------
        public IntAttribute(string name, int value, bool writeAlways = false)
            : base(name, value, writeAlways)
        { }

        //---------------------------------------------------------------------
        public IntAttribute(XElement x, string name, bool writeAlways = false)
            : base(x, name, writeAlways)
        { }

        //---------------------------------------------------------------------
        public IntAttribute(XElement x, string name, int defaultValue, bool writeAlways = false)
            : base(x, name, defaultValue, writeAlways)
        { }

        //---------------------------------------------------------------------
        public IntAttribute(IntAttribute source)
            : base(source)
        { }

        //---------------------------------------------------------------------
        public override void Write(XmlWriter writer)
        {
            writer.WriteAttributeIf(_writeAlways || _value != 0, _name, _value);
        }

        //---------------------------------------------------------------------
        public override void Write(XmlWriter writer, int defaultValue)
        {
            writer.WriteAttributeIf(_writeAlways || _value != defaultValue, _name, _value);
        }

        //---------------------------------------------------------------------
        public static implicit operator int(IntAttribute ba)
        {
            return ba?.Value ?? 0;
        }
    }

    //=========================================================================
    [Serializable]
    public class StringAttribute : AttributeValue<string>
    {
        //---------------------------------------------------------------------
        public StringAttribute(string name, string value, bool writeAlways = false)
            : base(name, value, writeAlways)
        { }

        //---------------------------------------------------------------------
        public StringAttribute(XElement x, string name, bool writeAlways = false)
            : base(name, writeAlways)
        {
            _value = x.GetAttrValue(name, "");
        }

        //---------------------------------------------------------------------
        public StringAttribute(XElement x, string name, string defaultValue, bool writeAlways = false)
            : base(name, defaultValue, writeAlways)
        {
            _value = x.GetAttrValue(name, defaultValue);
        }

        //---------------------------------------------------------------------
        public StringAttribute(StringAttribute source)
            : base(source)
        { }

        //---------------------------------------------------------------------
        public override void Write(XmlWriter writer)
        {
            writer.WriteAttributeStringIf(_writeAlways || !string.IsNullOrEmpty(_value), _name, _value);
        }

        //---------------------------------------------------------------------
        public override string ToString()
        {
            return _value;
        }

        //---------------------------------------------------------------------
        public static implicit operator string(StringAttribute ba)
        {
            return ba.Value;
        }
    }
}
