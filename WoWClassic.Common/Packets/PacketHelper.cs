using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WoWClassic.Common.Packets
{
    public static class PacketHelper
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

        public static byte[] Build<T>(T instance) where T : class, new()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                Build(bw, typeof(T), instance);

                return ms.ToArray();
            }
        }

        private static void Build(BinaryWriter bw, Type type, object instance)
        {
            foreach (var fi in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).OrderBy(f => f.MetadataToken))
                BuildField(bw, fi.FieldType, fi.GetValue(instance), fi.GetCustomAttributes());
        }

        private static void BuildField(BinaryWriter bw, Type fieldType, object value, IEnumerable<Attribute> attributes)
        {
            if (value == null)
                return; // TODO: Error handling?

            if (fieldType.IsEnum)
                BuildPrimitive(bw, Type.GetTypeCode(fieldType.GetEnumUnderlyingType()), value, attributes);
            else if (fieldType.IsClass && (!fieldType.IsArray && fieldType != typeof(string)))
                Build(bw, fieldType, value);
            else if (fieldType.IsArray)
                BuildArray(bw, (Array)value, attributes);
            else if (fieldType.IsPrimitive || fieldType == typeof(string))
                BuildPrimitive(bw, Type.GetTypeCode(fieldType), value, attributes);
            else
                throw new Exception("BuildField unable to determine data type");
        }

        private static void BuildArray(BinaryWriter bw, Array arr, IEnumerable<Attribute> attributes)
        {
            var eleType = arr.GetType().GetElementType();
            foreach (var ele in arr)
                BuildField(bw, eleType, ele, attributes);
        }

        private static void BuildPrimitive(BinaryWriter bw, TypeCode primitiveType, object value, IEnumerable<Attribute> attributes)
        {
            switch (primitiveType)
            {
                case TypeCode.Boolean:
                    bw.Write((bool)value);
                    break;
                case TypeCode.Byte:
                    bw.Write((byte)value);
                    break;
                case TypeCode.Char:
                    bw.Write((char)value);
                    break;
                case TypeCode.Double:
                    bw.Write((double)value);
                    break;
                case TypeCode.Int16:
                    bw.Write((short)value);
                    break;
                case TypeCode.Int32:
                    bw.Write((int)value);
                    break;
                case TypeCode.Int64:
                    bw.Write((long)value);
                    break;
                case TypeCode.SByte:
                    bw.Write((sbyte)value);
                    break;
                case TypeCode.Single:
                    bw.Write((float)value);
                    break;
                case TypeCode.UInt16:
                    bw.Write((ushort)value);
                    break;
                case TypeCode.UInt32:
                    bw.Write((uint)value);
                    break;
                case TypeCode.UInt64:
                    bw.Write((ulong)value);
                    break;
                case TypeCode.String:
                    var sAttr = attributes.OfType<PacketStringAttribute>().FirstOrDefault();
                    if (sAttr == null) throw new Exception("BuildPrimitive<string> Missing attribute");

                    switch (sAttr.StringType)
                    {
                        case StringTypes.CString:
                            bw.Write(Encoding.ASCII.GetBytes((string)value + '\0'));
                            break;
                        case StringTypes.PrefixedLength:
                            var str = (string)value;
                            bw.Write((byte)str.Length);
                            bw.Write(Encoding.ASCII.GetBytes(str));
                            break;
                        case StringTypes.FixedLength:
                            var aAttr = attributes.OfType<PacketArrayLengthAttribute>().FirstOrDefault();
                            if (aAttr == null) throw new Exception("BuildPrimitive<string> Missing fixed-length attribute");

                            bw.Write(Encoding.ASCII.GetBytes((string)value).Pad(aAttr.Length));
                            break;
                    }
                    break;
                default:
                    // Throw exception?
                    // Means we got a type we don't know how to parse
                    throw new Exception($"BuildPrimitive unhandled primitive '{primitiveType}'");
            }
        }
    }

    public enum StringTypes
    {
        CString,
        PrefixedLength,
        FixedLength
    };

    [AttributeUsage(AttributeTargets.Field)]
    public class PacketStringAttribute : Attribute
    {
        public PacketStringAttribute(StringTypes stringType)
        {
            StringType = stringType;
        }

        public StringTypes StringType { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class PacketArrayLengthAttribute : Attribute
    {
        public PacketArrayLengthAttribute(int length)
        {
            Length = length;
        }

        public int Length { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class PacketArrayReverseAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public class PacketBigEndianAttribute : Attribute { }
}
