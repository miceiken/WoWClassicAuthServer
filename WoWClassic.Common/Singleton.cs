using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWClassic.Common
{
    public class Singleton<T> where T : class, new()
    {
        private static T s_Instance;
        public static T Instance { get { return s_Instance ?? (s_Instance = new T()); } }
    }
}
