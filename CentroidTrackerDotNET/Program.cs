using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CentroidTrackerDotNET
{
    class Program
    {
        static void Main(string[] args)
        {
            Detection a = new Detection();
            a.Detect();
        }
    }
}
