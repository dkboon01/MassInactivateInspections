using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MassInactiveInspections.Models
{
    public class AdditionalCustomerInfoList
    {
        public List<AdditionalCustomerInfo> additionalCustomerInfoListView { get; set; }

        public List<SearchInspections> sysinsp { get; set; }

        public List<Competitors> competitorslist { get; set; }

        //[Required(ErrorMessage = "Competitor is Required")]
        public string Selectedcompetitor { get; set; }


        //[Required(ErrorMessage = "Field must be filled in")]
        public string competitorothertxt { get; set; }
    }
}