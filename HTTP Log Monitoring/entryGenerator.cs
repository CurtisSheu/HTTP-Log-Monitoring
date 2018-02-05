using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace HTTP_Log_Monitoring
{
    class entryGenerator
    {
        private string logPath;
        private int rate;
        
        string[] methods = { "GET", "GET", "GET", "POST", "POST", "PUT"};
        string[] sections = { "img", "exam", "foo", "bar", "login", "master", "blob" };
        string[] codes = { "200", "304", "403", "404" };

        public entryGenerator(string path, int r)
        {
            if (!File.Exists(path))
            {
                StreamWriter sw = File.CreateText(path);
                sw.Close();
            }

            logPath = path;
            rate = r;
        }

        public void threadedGenerateEntries()
        {
            while (true)
            {
                try
                {
                    StreamWriter temp = File.AppendText(logPath);
                    temp.WriteLine(generateEntry());
                    temp.Close();
                }
                catch(Exception e)
                {
                    Console.WriteLine("Could not write to file");
                }
                Thread.Sleep(1000 * rate);
            }
        }

        private string generateEntry()
        {
            string ip = randomIP();
            string entryTime = DateTime.Now.ToString("dd/MMM/yyyy:HH:mm:ss zzz");
            string method = methods.RandomItem();
            string section = randomSection();
            string code = codes.RandomItem();

            string entry = String.Format("{0} user-identifier userid [{1}] \"{2} {3} HTTP/1.1\" {4} 200", ip, entryTime, method, section, code);

            return entry;
        }

        private string randomIP()
        {
            return randomGen.rnd.Next(0, 255) + "." + randomGen.rnd.Next(0, 255) + "." + randomGen.rnd.Next(0, 255) + "." + randomGen.rnd.Next(0, 255);
        }

        private string randomSection()
        {
            string section = "/" + sections.RandomItem();

            for (int i = 0; i < 3; i++)
                if (randomGen.rnd.Next(0, 2) == 0)
                    section += "/" + sections.RandomItem();

            section += ".html";

            return section;
        }

    }

    public static class ArrayExtensions
    {
        public static T RandomItem<T>(this T[] array)
        {
            return array[randomGen.rnd.Next(0, array.Length)];
        }
    }
}
