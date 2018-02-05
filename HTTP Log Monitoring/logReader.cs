using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Globalization;

namespace HTTP_Log_Monitoring
{
    class logReader
    {
        private string logPath;
        private int rate;
        private int alertAmount;
        private bool alertStatus = false;
        private DateTime lastTime;

        List<string> alertMessages = new List<string>();
        Dictionary<string, int> sectionCounts = new Dictionary<string, int>();
        List<DateTime> lastTwoMinutes = new List<DateTime>();

        public logReader(string path, int r, int a)
        {
            logPath = path;
            rate = r;
            alertAmount = a;
        }

        public void threadedRead()
        {
            lastTime = DateTime.Now;
            while (true)
            {
                readLastLinesAfterSpecificiedDateTime(Encoding.UTF8);
                Thread.Sleep(1000 * rate);
            }
        }

        private int findSecondSlash(string input)
        {
            int first = input.IndexOf('/');

            if (first == -1)
                return -1;

            return input.IndexOf('/', first + 1);
        }

        private bool interpretLine(string line)
        {
            try
            {
                string[] tokens = line.Split(' ');

                string dateString = tokens[3] + " " + tokens[4];

                DateTime lineDateTime = DateTime.ParseExact(dateString, "[dd/MMM/yyyy:HH:mm:ss zzz]", CultureInfo.CurrentCulture);

                if (lineDateTime < lastTime)
                    return false; //This entry has already been checked since it is before the last time we checked
                else
                {
                    lastTwoMinutes.Add(lineDateTime);

                    string sectionToken = tokens[6];
                    string section;
                    int secondSlash = findSecondSlash(sectionToken);
                    if (secondSlash == -1)
                        section = sectionToken;
                    else
                        section = sectionToken.Substring(0, secondSlash);

                    if (sectionCounts.ContainsKey(section))
                        sectionCounts[section]++;
                    else
                        sectionCounts.Add(section, 1);

                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Malformed log file: " + line);
                return false;
            }
        }

        private void readLastLinesAfterSpecificiedDateTime(Encoding encoding) //Start at the end of the file and work our way up since we only have to check for entries since the last time we checked.
        {
            int sizeOfChar = encoding.GetByteCount("\n");
            byte[] buffer = encoding.GetBytes("\n");

            DateTime tempDateTime = DateTime.Now; //Set a datetime to replace the current date time, but don't set it yet to account for processing time. Once we're done, we'll reset it.

            try
            {
                using (FileStream fs = new FileStream(logPath, FileMode.Open))
                {
                    Int64 endPosition = fs.Length / sizeOfChar;
                    Int64 lastPosition = fs.Length;

                    fs.Seek(0, SeekOrigin.End); //Set seek origin to the end
                    fs.Seek(-sizeOfChar, SeekOrigin.Current); //Skip the last \n in the file

                    while (fs.Position > 0)
                    {
                        fs.Seek(-sizeOfChar, SeekOrigin.Current);
                        fs.Read(buffer, 0, buffer.Length);

                        if (encoding.GetString(buffer) == "\n")
                        {
                            byte[] line = new byte[lastPosition - fs.Position];
                            fs.Read(line, 0, line.Length);
                            if (!interpretLine(encoding.GetString(line))) //False here means that the line was not added because it was before the last time, thus we're done.
                                break;
                            fs.Seek(-line.Length, SeekOrigin.Current);
                            fs.Seek(-sizeOfChar, SeekOrigin.Current); //Reset seek to contiune checking
                            lastPosition = fs.Position;
                        }
                        else
                            fs.Seek(-buffer.Length, SeekOrigin.Current); //Reset the seek position
                    }

                    if (fs.Position == 0) //Beginning of file was reached, run the function on the first line
                    {
                        byte[] line = new byte[lastPosition];
                        fs.Read(line, 0, line.Length);
                        interpretLine(encoding.GetString(line));
                    }
                }

                Console.Clear();
                checkLastTwoMinutes(tempDateTime);
                printStatistics();
                
                lastTime = tempDateTime;
            }
            catch(Exception e)
            {
                Console.WriteLine("Error accessing log file");
            }
        }

        private void checkLastTwoMinutes(DateTime checkTime)
        {
            TimeSpan twoMinutes = new TimeSpan(0, 2, 0);
            DateTime twoMinutesAgo = checkTime.Subtract(twoMinutes);

            for (int i = 0; i < lastTwoMinutes.Count; )
            {
                if (lastTwoMinutes[i] < twoMinutesAgo)
                    lastTwoMinutes.RemoveAt(i);
                else
                    i++;
            }
            
            if (!alertStatus && lastTwoMinutes.Count >= alertAmount)
            {
                alertStatus = true;
                alertMessages.Add(String.Format("High traffic generated an alert - hits = {0}, triggered at {1}", lastTwoMinutes.Count, checkTime));
            }
            else if (alertStatus && lastTwoMinutes.Count < alertAmount)
            {
                alertStatus = false;
                alertMessages.Add(String.Format("Traffic returned to normal at {0}", checkTime));
            }

            foreach (string s in alertMessages)
                Console.WriteLine(s);
        }

        private void printStatistics()
        {
            int max = 0;
            string maxSection = String.Empty;
            
            Console.WriteLine("Hits per section:");
            Console.WriteLine("-----------------");

            foreach(KeyValuePair<string, int> sectionPair in sectionCounts)
            {
                if (sectionPair.Value > max)
                {
                    max = sectionPair.Value;
                    maxSection = sectionPair.Key;
                }

                Console.WriteLine(sectionPair.Key + ": " + sectionPair.Value.ToString());
            }

            Console.WriteLine("-----------------");
            Console.WriteLine(String.Format("Most Hits: {0} with {1} Hit(s)", maxSection, max));
        }
    }
}
