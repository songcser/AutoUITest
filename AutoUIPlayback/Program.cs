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
        private static string filePath = "E:\\GitHub\\AutoUITest\\Log\\log1.txt" ;

        

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Please input arguments path");
            }
            filePath = args[0];
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
