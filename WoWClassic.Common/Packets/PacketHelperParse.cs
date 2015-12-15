using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WoWClassic.Common.Packets
{
    public static partial class PacketHelper
    {
        public static T Parse<T>(BinaryReader br) where T : class, new()
        {
            return (T)Parse(br, typeof(T));
        }

        private static object Parse(BinaryReader br, Type type)
        {
            var instance = Activator.CreateInstance(type);
            foreach (var fi in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).OrderBy(f => f.MetadataToken))
                fi.SetValue(instance, ParseField(br, fi.FieldType, fi.GetCustomAttributes()));
            return instance;
        }

        private static object ParseField(BinaryReader br, Type fieldType, IEnumerable<Attribute> attributes)
        {
            if (fieldType.IsArray)
                return ParseArray(br, fieldType, attributes);
            if (fieldType.IsEnum)
                return ParsePrimitive(br, Type.GetTypeCode(fieldType.GetEnumUnderlyingType()), attributes);
            if (fieldType.IsPrimitive || fieldType == typeof(string))
                return ParsePrimitive(br, Type.GetTypeCode(fieldType), attributes);
            if (fieldType.IsClass)
                return Parse(br, fieldType);
            throw new Exception("ParseField returned no value");
        }

        private static object ParseArray(BinaryReader br, Type arrType, IEnumerable<Attribute> attributes)
        {
            var alAttr = attributes.OfType<PacketArrayLengthAttribute>().FirstOrDefault();
            if (alAttr == null) throw new Exception("Array missing ArrayLength attribute");

            var eleType = arrType.GetElementType();
            var newArr = Array.CreateInstance(eleType, alAttr.Length);
            for (var i = 0; i < newArr.Length; i++)
                newArr.SetValue(ParseField(br, eleType, attributes), i);
            return newArr;
        }

        private static object ParsePrimitive(BinaryReader br, TypeCode typeCode, IEnumerable<Attribute> attributes)
        {
            var isBigEndian = attributes.OfType<PacketStringAttribute>().FirstOrDefault() != null;
            switch (typeCode)
            {
                case TypeCode.Boolean: return br.ReadBoolean();
                case TypeCode.Byte: return br.ReadByte();
                case TypeCode.Char: return br.ReadChar();
                case TypeCode.Double: return br.ReadDouble();
                case TypeCode.Int16:
                    if (isBigEndian) return BitConverter.ToInt16(br.ReadBytes(2), 0);
                    else return br.ReadInt16();
                case TypeCode.Int32:
                    if (isBigEndian) return BitConverter.ToInt32(br.ReadBytes(4), 0);
                    else return br.ReadInt32();
                case TypeCode.Int64:
                    if (isBigEndian) return BitConverter.ToInt64(br.ReadBytes(8), 0);
                    else return br.ReadInt64();
                case TypeCode.SByte: return br.ReadSByte();
                case TypeCode.Single: return br.ReadSingle();
                case TypeCode.UInt16:
                    if (isBigEndian) return BitConverter.ToUInt16(br.ReadBytes(2), 0);
                    else return br.ReadUInt16();
                case TypeCode.UInt32:
                    if (isBigEndian) return BitConverter.ToUInt32(br.ReadBytes(4), 0);
                    else return br.ReadUInt32();
                case TypeCode.UInt64:
                    if (isBigEndian) return BitConverter.ToUInt64(br.ReadBytes(8), 0);
                    else return br.ReadUInt64();
                case TypeCode.String:
                    var sAttr = attributes.OfType<PacketStringAttribute>().FirstOrDefault();
                    if (sAttr == null) throw new Exception("ParsePrimitive<string> Missing attribute");

                    switch (sAttr.StringType)
                    {
                        case StringTypes.CString:
                            return br.ReadCString();
                        case StringTypes.PrefixedLength:
                            var length = br.ReadByte();
                            return new string(br.ReadChars(length));
                        case StringTypes.FixedLength:
                            var aAttr = attributes.OfType<PacketArrayLengthAttribute>().FirstOrDefault();
                            if (aAttr == null) throw new Exception("ParsePrimitive<string> Missing fixed-length attribute");
                            return new string(br.ReadChars(aAttr.Length));

                    }
                    break;
            }

            throw new Exception("ParsePrimitive returned no value");
        }        
    }
}
