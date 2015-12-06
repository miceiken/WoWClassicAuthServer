using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace WoWClassic.Common
{
    public class GenericWriter : IDisposable
    {
        public GenericWriter(Stream stream)
        {
            BaseStream = stream;
            m_Writer = new BinaryWriter(BaseStream);
        }

        public void Dispose()
        {
            m_Writer.Dispose();
        }

        private BinaryWriter m_Writer;

        public Stream BaseStream { get; private set; }

        public void Write<T>(T value)
        {
            object obj = value;
            if (typeof(T) == typeof(byte[]))
            {
                m_Writer.Write((byte[])obj);
                return;
            }

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    m_Writer.Write((bool)obj);
                    break;
                case TypeCode.Byte:
                    m_Writer.Write((byte)obj);
                    break;
                case TypeCode.Char:
                    m_Writer.Write((char)obj);
                    break;
                case TypeCode.Double:
                    m_Writer.Write((double)obj);
                    break;
                case TypeCode.Int16:
                    m_Writer.Write((short)obj);
                    break;
                case TypeCode.Int32:
                    m_Writer.Write((int)obj);
                    break;
                case TypeCode.Int64:
                    m_Writer.Write((long)obj);
                    break;
                case TypeCode.SByte:
                    m_Writer.Write((sbyte)obj);
                    break;
                case TypeCode.String:
                    m_Writer.Write((string)obj);
                    break;
                case TypeCode.Single:
                    m_Writer.Write((float)obj);
                    break;
                case TypeCode.UInt16:
                    m_Writer.Write((ushort)obj);
                    break;
                case TypeCode.UInt32:
                    m_Writer.Write((uint)obj);
                    break;
                case TypeCode.UInt64:
                    m_Writer.Write((ulong)obj);
                    break;
                case TypeCode.Empty:
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
