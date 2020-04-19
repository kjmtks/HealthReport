using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCVC.App.Models
{
    public class TimeFrame
    {
        public string Name { get; set; }
        public Time Start { get; set; }
        public Time End { get; set; }

        public bool IsIn(DateTime time)
        {
            var a = Start.GetSeconds();
            var b = time.Hour * 60 * 60 + time.Minute * 60 + time.Second;
            var c = End.GetSeconds();
            return a <= b && b <= c;
        }
    }

    public class Time
    {
        public int Hour { get; set; }
        public int Minite { get; set; }
        public int Second { get; set; }

        public int GetSeconds()
        {
            return Hour * 60 * 60 + Minite * 60 + Second;
        }
    }
}
