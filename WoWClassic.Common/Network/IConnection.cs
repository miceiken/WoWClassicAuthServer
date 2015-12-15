using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWClassic.Common.Network
{
    public interface IConnection
    {
        void Process(byte[] data, int length);
        void Send(byte[] data);
    }
}
