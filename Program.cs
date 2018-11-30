using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Threading;

namespace HamrChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Input day offset (e.g. 0 for today, 1 for tomorrow):");
            var dayOffset = Int32.Parse(Console.ReadLine());

            Console.WriteLine("Input start (e.g. 18:30):");
            var startCell = TimeToCell(Console.ReadLine());

            Console.WriteLine("Input end:");
            var endCell = TimeToCell(Console.ReadLine());

            Console.WriteLine("Input session cookie:");
            var sessionId = Console.ReadLine();

            while (true)
            {
                Console.Write("Checking ... ");

                try
                {
                    var cells = Enumerable.Range(startCell, endCell - startCell + 1);
                    var data = GetData(sessionId);
                    if (cells.Any(c => IsFree(data, dayOffset, c)))
                    {
                        Console.WriteLine("JACKPOT");

                        for (var i = 0; i < 5; i++)
                        {
                            SystemSounds.Beep.Play();
                            Thread.Sleep(2000);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Nothing.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error getting data: " + e.Message);
                }

                Thread.Sleep(60 * 1000);
            }
        }

        private static int TimeToCell(string time)
        {
            var parts = time.Split(':');
            var hour = Int32.Parse(parts.First());
            var half = parts.ElementAt(1).Equals("30") ? 1 : 0;
            return (hour - 7) * 2 + half;
        }

        private static string GetData(string sessionId)
        {
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(new Cookie("HamrOnline$SessionId", sessionId, "/", "hodiny.hamrsport.cz"));

            var httpWebRequest = WebRequest.CreateHttp("https://hodiny.hamrsport.cz/Default.aspx");
            httpWebRequest.CookieContainer = cookieContainer;

            using (var stream = httpWebRequest.GetResponse().GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static bool IsFree(string data, int dayOffset, int cell)
        {
            var lines = data.Split('\n');
            var line = lines.FirstOrDefault(l => l.Contains($"rgI_{dayOffset}_{cell}")) ?? "";
            return line.Contains("free");
        }
    }
}
