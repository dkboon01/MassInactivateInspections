using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Data.Sql;
using System.Web.Mvc;
using MassInactiveInspections.Models;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;


namespace MassInactiveInspections.Controllers
{

    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CheckForNumber(CustomerNumber customerNumber)
        {
            string strResult = string.Empty;
            var result = Helper.ExistingCustomerNumber<DoesCustomerNumberExist_Result>(customerNumber.customerNum.ToString());
            var results = Helper.GetCustomerInspectionInfo<GetCustomerInspectionInformation_Result>(customerNumber.customerNum.ToString());
            var customerInfo = new CustomerInfo();
            var customerlist = new CustomerInfoList();
            customerlist.CustomerInfoListView = new List<CustomerInfo>();
            

            //var customerInfoList = new CustomerInfoList();


            if (ModelState.IsValid)
            {               
                int? intCustomer = 0;

                foreach (var item in result)
                {
                    intCustomer = item.Does_Exist;
                }
                if (intCustomer == 1)
                {
                    foreach (var customer in results)
                    {
                        customerInfo.Active = customer.Active;
                        customerInfo.BusinessName = customer.Business_Name;
                        customerInfo.CustomerName = customer.Customer_Name;
                        customerInfo.CustomerNumber = customer.Customer_Number;
                        customerInfo.InspectionCycleDescription = customer.Inspection_Cycle_Description;
                        customerInfo.LastInspectionDate = customer.Last_Inspection_Date;
                        customerInfo.RouteCode = customer.Route_Code;
                        customerInfo.RouteID = customer.Route_ID;
                        customerInfo.SiteNumber = customer.Site_Number;
                        customerInfo.SystemCode = customer.System_Code;

                        customerlist.CustomerInfoListView.Add(customerInfo);

                   
                    }
                }
                else
                {
                    ViewBag.result = "Not a valid number";
                }
            }
            return View("ViewCustomerInfo", (object)customerlist);
        }
        public ActionResult ViewCustomerInfo()
        {
            return View();
        }
    }
}


