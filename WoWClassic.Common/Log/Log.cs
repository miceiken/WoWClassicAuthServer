using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWClassic.Common.Log
{
    public static class Log
    {
        const string TIMESTAMP_FORMAT = "hh:mm:ss";

        private static Dictionary<string, Logger> s_Loggers = new Dictionary<string, Logger>();

        public static void CreateLogger<T>()
        {
            if (!typeof(T).IsEnum)
                throw new Exception("T is not enum");
            foreach (var name in Enum.GetNames(typeof(T)))
                s_Loggers.Add(name, new Logger());
        }

        public static void WriteLine<T>(T type, string entry)
        {
            foreach (var logger in s_Loggers[Enum.GetName(typeof(T), type)]?.Subscribers)
                logger.WriteLine(entry);
        }

        public static void WriteLine<T>(T type, string format, params object[] args) => WriteLine(type, $"{DateTime.Now.ToString(TIMESTAMP_FORMAT)} " + string.Format(format, args));
        public static void WriteLine<T>(T type, params object[] args) => WriteLine(type, $"{DateTime.Now.ToString(TIMESTAMP_FORMAT)} " + string.Concat(args));

        public static void AddSubscriber<T>(T type, ILogSubscriber subscriber) => s_Loggers[Enum.GetName(typeof(T), type)].Subscribers.Add(subscriber);
        public static void RemoveSubscriber<T>(T type, ILogSubscriber subscriber) => s_Loggers[Enum.GetName(typeof(T), type)].Subscribers.Remove(subscriber);
    }

    public class Logger : ILogSubscriber
    {
        public Logger() { Subscribers.Add(this); }

        public List<string> Entries { get; private set; } = new List<string>();
        public List<ILogSubscriber> Subscribers { get; private set; } = new List<ILogSubscriber>();

        public void WriteLine(string entry) => Entries.Add(entry);
    }

    public interface ILogSubscriber
    {
        void WriteLine(string entry);
    }

    public class FileLogSubscriber : ILogSubscriber
    {
        public FileLogSubscriber(string filename, string prefix = null)
        {
            Filename = filename;
            Prefix = prefix;
        }

        public string Filename { get; private set; }
        public string Prefix { get; private set; }

        public void WriteLine(string entry)
        {
            using (var file = File.AppendText(Filename))
                file.WriteLine((Prefix ?? string.Empty) + entry);
        }
    }

    public class ConsoleLogSubscriber : ILogSubscriber
    {
        public ConsoleLogSubscriber(string prefix = null)
        {
            Prefix = prefix;
        }

        public string Prefix { get; private set; }

        public void WriteLine(string entry)
        {
            Console.WriteLine((Prefix ?? string.Empty) + entry);
        }
    }
}
