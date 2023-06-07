using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBot
{
     class EndTime
    {

        TimeSpan etalon = TimeSpan.FromMilliseconds(10000);
        public bool EndTimer(DateTime start,DateTime end) 
        {   
            
             if(start - end < etalon)
             {
                return true;
             }
            else
                return false;
        }
    }
}
