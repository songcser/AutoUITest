using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace AutoUIPlayback
{
    class Program
    {
        private static string filePath = "E:\\GitHub\\AutoUITest\\log.txt" ;

        

        static void Main(string[] args)
        {
            Analysis ana = new Analysis();
            using (StreamReader sr = File.OpenText(filePath))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    //Thread.Sleep(200);
                    if (!ana.StartAnalysis(s))
                    {
                        break;
                    }
                    
                }
            }

           // Console.Read();
        }
    }
}
