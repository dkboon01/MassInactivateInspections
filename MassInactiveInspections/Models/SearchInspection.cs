using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MassInactiveInspections.Models
{
    public class  SearchInspections
    {
            //public bool chkboxselected { get; set; }
            public string inspcycldesc { get; set; }
            public string syscode { get; set; }
            public string servcomp { get; set; }
            public string nextinpdate { get; set; }
            public string lastinpdate { get; set; }
            public int inspectionid { get; set; }
            public bool IsSelected { get; set; }
            public int customer_system_id { get; set; }
        
    }
   
}