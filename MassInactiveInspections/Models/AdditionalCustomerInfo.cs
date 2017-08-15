using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MassInactiveInspections.Models
{
    public class AdditionalCustomerInfo
    {
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string SiteNumber { get; set; }
        public string BusinessName { get; set; }
        public string SystemCode { get; set; }
        public int CustomerSystemId { get; set; }
        public string InspectionCycleDescription { get; set; }
        public string LastInspectionDate { get; set; }
        public int? RouteID { get; set; }
        public string RouteCode { get; set; }
        public string Active { get; set; }
        public bool IsSelected { get; set; }
        public int InspectionId { get; set; }
        public string Inspectdesc { get; set; }
        public string ReasonForLeaving{ get; set; }
    }
}