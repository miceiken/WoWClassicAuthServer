using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;

namespace WoWClassic.Common.Packets
{
    public static class PacketHelper
    {
        public static T Parse<T>(BinaryReader br) where T : class, new()
        {
            var type = typeof(T);
            T instance = new T();
            //using (var ms = new MemoryStream(data))
            //using (var br = new BinaryReader(ms))
            //{
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).OrderBy(f => Marshal.OffsetOf(type, f.Name).ToInt32()))
            {
                if (field.FieldType == typeof(byte[]))
                {
                    var attr = field.GetCustomAttributes(typeof(ArrayLengthAttribute), false).Cast<ArrayLengthAttribute>().FirstOrDefault();
                    if (attr == null) throw new Exception("Missing attribute");
                    if (field.GetCustomAttributes(typeof(StringParseAttribute), false).Cast<StringParseAttribute>().FirstOrDefault() != null)
                        field.SetValue(instance, br.ReadBytes(attr.Length).Reverse());
                    else
                        field.SetValue(instance, br.ReadBytes(attr.Length));
                    continue;
                }

                switch (Type.GetTypeCode(field.FieldType))
                {
                    case TypeCode.Boolean:
                        field.SetValue(instance, br.ReadBoolean());
                        break;
                    case TypeCode.Byte:
                        field.SetValue(instance, br.ReadByte());
                        break;
                    case TypeCode.Char:
                        field.SetValue(instance, br.ReadChar());
                        break;
                    case TypeCode.Double:
                        field.SetValue(instance, br.ReadDouble());
                        break;
                    case TypeCode.Int16:
                        if (IsBigEndian(field)) field.SetValue(instance, BitConverter.ToInt16(br.ReadBytes(2), 0));
                        else field.SetValue(instance, br.ReadInt16());
                        break;
                    case TypeCode.Int32:
                        if (IsBigEndian(field)) field.SetValue(instance, BitConverter.ToInt32(br.ReadBytes(4), 0));
                        else field.SetValue(instance, br.ReadInt32());
                        break;
                    case TypeCode.Int64:
                        if (IsBigEndian(field)) field.SetValue(instance, BitConverter.ToInt64(br.ReadBytes(8), 0));
                        else field.SetValue(instance, br.ReadInt64());
                        break;
                    case TypeCode.SByte:
                        field.SetValue(instance, br.ReadSByte());
                        break;
                    case TypeCode.Single:
                        field.SetValue(instance, br.ReadSingle());
                        break;
                    case TypeCode.UInt16:
                        if (IsBigEndian(field)) field.SetValue(instance, BitConverter.ToUInt16(br.ReadBytes(2), 0));
                        else field.SetValue(instance, br.ReadUInt16());
                        break;
                    case TypeCode.UInt32:
                        if (IsBigEndian(field)) field.SetValue(instance, BitConverter.ToUInt32(br.ReadBytes(4), 0));
                        else field.SetValue(instance, br.ReadUInt32());
                        break;
                    case TypeCode.UInt64:
                        if (IsBigEndian(field)) field.SetValue(instance, BitConverter.ToUInt64(br.ReadBytes(8), 0));
                        else field.SetValue(instance, br.ReadUInt64());
                        break;
                    case TypeCode.String:
                        var sAttr = field.GetCustomAttributes(typeof(StringParseAttribute), false).Cast<StringParseAttribute>().FirstOrDefault();
                        if (sAttr == null) throw new Exception("Missing attribute");
                        switch (sAttr.StringType)
                        {
                            case StringTypes.CString:
                                field.SetValue(instance, br.ReadCString());
                                break;
                            case StringTypes.PrefixedLength:
                                var length = br.ReadByte();
                                field.SetValue(instance, new string(br.ReadChars(length)));
                                break;
                            case StringTypes.FixedLength:
                                var aAttr = field.GetCustomAttributes(typeof(ArrayLengthAttribute), false).Cast<ArrayLengthAttribute>().FirstOrDefault();
                                if (aAttr == null) throw new Exception("Missing attribute");
                                field.SetValue(instance, new string(br.ReadChars(aAttr.Length)));
                                break;

                        }
                        break;
                    default:
                        // Throw exception?
                        // Means we got a type we don't know how to parse
                        return null;
                }
            }
            //}

            return instance;
        }

        public static byte[] Build<T>(T instance) where T : class, new()
        {
            var type = typeof(T);
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).OrderBy(f => Marshal.OffsetOf(type, f.Name).ToInt32()))
                {
                    if (field.FieldType == typeof(byte[]))
                    {
                        bw.Write((byte[])field.GetValue(instance));
                        continue;
                    }

                    switch (Type.GetTypeCode(field.FieldType))
                    {
                        case TypeCode.Boolean:
                            bw.Write((bool)field.GetValue(instance));
                            break;
                        case TypeCode.Byte:
                            bw.Write((byte)field.GetValue(instance));
                            break;
                        case TypeCode.Char:
                            bw.Write((char)field.GetValue(instance));
                            break;
                        case TypeCode.Double:
                            bw.Write((double)field.GetValue(instance));
                            break;
                        case TypeCode.Int16:
                            bw.Write((short)field.GetValue(instance));
                            break;
                        case TypeCode.Int32:
                            bw.Write((int)field.GetValue(instance));
                            break;
                        case TypeCode.Int64:
                            bw.Write((long)field.GetValue(instance));
                            break;
                        case TypeCode.SByte:
                            bw.Write((sbyte)field.GetValue(instance));
                            break;
                        case TypeCode.Single:
                            bw.Write((float)field.GetValue(instance));
                            break;
                        case TypeCode.UInt16:
                            bw.Write((ushort)field.GetValue(instance));
                            break;
                        case TypeCode.UInt32:
                            bw.Write((uint)field.GetValue(instance));
                            break;
                        case TypeCode.UInt64:
                            bw.Write((ulong)field.GetValue(instance));
                            break;
                        case TypeCode.String:
                            var sAttr = field.GetCustomAttributes(typeof(StringParseAttribute), false).Cast<StringParseAttribute>().FirstOrDefault();
                            if (sAttr == null) throw new Exception("Missing attribute");
                            switch (sAttr.StringType)
                            {
                                case StringTypes.CString:
                                    bw.Write(Encoding.ASCII.GetBytes((string)field.GetValue(instance) + '\0'));
                                    break;
                                case StringTypes.PrefixedLength:
                                    var str = (string)field.GetValue(instance);
                                    bw.Write((byte)str.Length);
                                    bw.Write(Encoding.ASCII.GetBytes(str));
                                    break;
                            }
                            break;
                        default:
                            // Throw exception?
                            // Means we got a type we don't know how to parse
                            return null;
                    }
                }

                return ms.ToArray();
            }
        }

        private static bool IsBigEndian(FieldInfo field)
        {
            return field.GetCustomAttributes(typeof(BigEndianAttribute), false).Cast<BigEndianAttribute>().FirstOrDefault() != null;
        }
    }

    public enum StringTypes
    {
        CString,
        PrefixedLength,
        FixedLength
    };

    public class StringParseAttribute : Attribute
    {
        public StringParseAttribute(StringTypes stringType)
        {
            StringType = stringType;
        }

        public StringTypes StringType { get; private set; }
    }

    public class ArrayLengthAttribute : Attribute
    {
        public ArrayLengthAttribute(int length)
        {
            Length = length;
        }

        public int Length { get; private set; }
    }

    public class ReverseArrayAttribute : Attribute { }
    public class BigEndianAttribute : Attribute { }
}
