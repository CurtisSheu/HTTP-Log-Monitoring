using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HTTP_Log_Monitoring
{
    static class randomGen
    {
        public static Random rnd = new Random();
    }

    class Program
    {
        private const int readTimeInSeconds = 10;
        private const int alertAmount = 38;
        private const string filePath = "../../../log.txt";

        static void Main(string[] args)
        {
            entryGenerator eg = new entryGenerator(filePath, 3);
            Thread generateLog = new Thread(new ThreadStart(eg.threadedGenerateEntries));
            generateLog.Start();

            logReader lg = new logReader(filePath, readTimeInSeconds, alertAmount);
            Thread readLog = new Thread(new ThreadStart(lg.threadedRead));
            readLog.Start();
        }

        private static void readLog()
        {
            while (true)
            {
                Console.WriteLine("Test");
                Thread.Sleep(1000 * readTimeInSeconds);
            }
        }
    }
}
