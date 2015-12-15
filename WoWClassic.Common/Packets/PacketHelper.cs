using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace WoWClassic.Common.Packets
{
    public static partial class PacketHelper
    {
        public static Dictionary<T, CommandHandler> RegisterHandlers<T>(Type type)
        {
            var ret = new Dictionary<T, CommandHandler>();
            foreach (var method in type.GetMethods())
            {
                var attr = method.GetCustomAttributes(typeof(PacketHandlerAttribute), false).Cast<PacketHandlerAttribute>().FirstOrDefault();
                if (attr == null) continue;
                ret.Add((T)(object)attr.PacketId, (CommandHandler)method.CreateDelegate(typeof(CommandHandler), null));
            }
            return ret;
        }

        public static Dictionary<T, CommandHandler> RegisterHandlers<T>()
        {
            return RegisterHandlers<T>(Assembly.GetExecutingAssembly().GetType());
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
