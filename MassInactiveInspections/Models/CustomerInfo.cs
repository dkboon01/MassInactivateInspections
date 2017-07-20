using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MassInactiveInspections.Models
{
    public class CustomerInfo
    {
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string SiteNumber { get; set; }
        public string BusinessName { get; set; }
        public string SystemCode { get; set; }
        public string InspectionCycleDescription { get; set; }
        public string LastInspectionDate { get; set; }
        public int? RouteID { get; set; }
        public string RouteCode { get; set; }
        public string Active { get; set; }
    }
}