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
        [HttpPost]
        public ActionResult CheckForNumber(CustomerNumber customerNumber, string LostButton, string OOBButton, string RefusedButton)
        {
            var lstButtons = new List<string> { LostButton, OOBButton, RefusedButton };
            string strSelectedButton = GetReasonForLeaving(lstButtons);

            string strResult = string.Empty;
            var result = Helper.ExistingCustomerNumber<DoesCustomerNumberExist_Result>(customerNumber.customerNum.ToString());
            var results = Helper.GetCustomerInspectionInfo<GetCustomerInspectionInformation_Result>(customerNumber.customerNum.ToString());
            var customerInfo = new AdditionalCustomerInfo();
            var customerlist = new AdditionalCustomerInfoList();
            customerlist.additionalCustomerInfoListView = new List<AdditionalCustomerInfo>();

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
                        customerlist.additionalCustomerInfoListView.Add(new AdditionalCustomerInfo()
                        {
                            Active = customer.Active,
                            BusinessName = customer.Business_Name,
                            CustomerName = customer.Customer_Name,
                            CustomerNumber = customer.Customer_Number,
                            InspectionCycleDescription = customer.Inspection_Cycle_Description,
                            LastInspectionDate = customer.Last_Inspection_Date,
                            RouteCode = customer.Route_Code,
                            RouteID = customer.Route_ID,
                            SiteNumber = customer.Site_Number,
                            SystemCode = customer.System_Code,
                            CustomerSystemId = customer.Customer_System_ID,
                            IsSelected = false,
                            InspectionId = customer.Inspection_ID,
                            ReasonForLeaving = strSelectedButton,
                    });
                    }
                    customerlist.competitorslist = GetCompetitors(lstButtons);
                }
                else
                {
                    ViewBag.Validation = "Invalid Customer Number";
                    return View("Index");
                }
            }
            else
            {
                return View("Index");
            }
            return View("ViewCustomerInfo", customerlist);
        }

        private static string GetReasonForLeaving(List<string> lstButtons)
        {
            var strSelectedButton = string.Empty;

            if (lstButtons.Contains("Lost")) { strSelectedButton = "Lost"; }
            else if (lstButtons.Contains("OOB")) { strSelectedButton = "OOB"; }
            else { strSelectedButton = "Refused"; }

            return strSelectedButton;
        }

        [HttpPost]
        public ActionResult ViewCustomerInfo(AdditionalCustomerInfoList customerList)
        {
            bool success = false;
            List<string> lstrButton = new List<string> {};
            SuccessFailViewModel sf = new SuccessFailViewModel();
            string strCompetitor = string.Empty;

            if (ModelState.IsValid)
            {
                foreach (var item in customerList.additionalCustomerInfoListView)
                {
                    if (item.IsSelected)
                    {
                        int cstsystemid = Convert.ToInt32(item.CustomerSystemId);
                        string competitorsel = customerList.Selectedcompetitor;

                        // Select Competitor for the log.
                        if (customerList.Selectedcompetitor == "Refused" || customerList.Selectedcompetitor == "Other")
                        {
                            strCompetitor = customerList.competitorothertxt;
                        }
                        else
                        {
                            strCompetitor = customerList.Selectedcompetitor;
                        }

                        string competitorothertxt = customerList.competitorothertxt;

                        sf.CustomerNumber = item.CustomerNumber;

                        // a) Update the customer system userdef record with "Lost" competitor informatoin 
                        try
                        {
                            success = UpdARCustomerSystemUserDef(cstsystemid, competitorsel, competitorothertxt);
                        }
                        catch
                        {
                            success = false;
                        }

                        if (success)
                        {   // b) update the inspection 
                            try
                            {
                                //************6/8 dkb
                                success = UpdInspection(item.InspectionId);

                                if (success)
                                {
                                    success = UpdEditLog(item.InspectionCycleDescription, strCompetitor, item.ReasonForLeaving, item.SiteNumber, item.CustomerNumber, item.SystemCode);
                                    //            //Part 2. check to see if any other checkbox for inspections is true then process them
                                    //            // checking to make sure there are other inspections
                                    success = GetInspectionData(customerList, success, item, competitorsel, competitorothertxt);
                                }
                                else
                                {
                                    success = false;
                                }
                            }
                            catch
                            {
                                success = false;
                            }

                        }

                    }
                }

                if (success)
                {
                    return View("Success", sf);
                }
                else
                {
                    return View("Failed", sf);
                }
            }
            else
            {
                lstrButton = GetSelectedButtons(customerList.additionalCustomerInfoListView[0].ReasonForLeaving);
                customerList.competitorslist = GetCompetitors(lstrButton);
                return View("ViewCustomerInfo", customerList);
            }

        }

        private static List<string> GetSelectedButtons(string strButton)
        {
            List<string> returnStrings = new List<string>();
            switch (strButton)
            {
                case "Lost":
                    returnStrings.Add("Lost");
                    returnStrings.Add(null);
                    returnStrings.Add(null);
                    break;
                case "OOB":
                    returnStrings.Add(null);
                    returnStrings.Add("OOB");
                    returnStrings.Add(null);
                    break;
                case "Refused":
                    returnStrings.Add(null);
                    returnStrings.Add(null);
                    returnStrings.Add("Refused");
                    break;
                default:
                    break;
            }

            return returnStrings;
        }

        private static bool GetInspectionData(AdditionalCustomerInfoList customerInfoList, bool success, AdditionalCustomerInfo item, string competitorsel, string competitorothertxt)
        {
            if (customerInfoList.sysinsp != null && customerInfoList.sysinsp.Any())
            {
                foreach (SearchInspections insp in customerInfoList.sysinsp)
                {
                    if (insp.IsSelected == true)  //is an inspection selected
                    {
                        if (item.InspectionId != insp.inspectionid)    // make sure we have not already updated the inspection
                        {
                            try
                            {
                                success = UpdARCustomerSystemUserDef(insp.customer_system_id, competitorsel, competitorothertxt);
                                try
                                {
                                    success = UpdInspection(insp.inspectionid);
                                    try
                                    {
                                        success = UpdEditLog(insp.inspcycldesc, competitorsel, "Lost", item.SiteNumber, item.CustomerNumber, insp.syscode);
                                    }
                                    catch
                                    {
                                        success = false;
                                    }

                                }
                                catch
                                {
                                    success = false;
                                }

                            }

                            catch
                            {
                                success = false;
                            }
                        }


                    }

                }
            }
            else
            {
                success = true;
            }

            return success;
        }

        private static List<Competitors> GetCompetitors(List<string> lstrButtons)
        {

            List<Competitors> comp = new List<Competitors>();
            List<Competitors> complist = new List<Competitors>();
            Byte[] documentBytes;  //holds the post body information in bytes



            string uri = "https://silcosedonacustomapi.silcofs.com/api/AR_Userdef_8";


            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            string authInfo = ConfigurationManager.AppSettings["apilgusrin"] + ":" + ConfigurationManager.AppSettings["apilgusrps"];
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;
            request.AllowAutoRedirect = true;
            request.PreAuthenticate = true;

            request.AutomaticDecompression = DecompressionMethods.GZip;

            request.Method = "GET";
            request.ContentType = "text/json";

            //Call store proc for the information for a user 
            string body = "";
            // string ticketno;

            System.Text.ASCIIEncoding obj = new System.Text.ASCIIEncoding();
            documentBytes = obj.GetBytes(body); //convert string to bytes
            request.ContentLength = documentBytes.Length;


            HttpWebResponse responsemsg = (HttpWebResponse)request.GetResponse();
            if (responsemsg.StatusCode == HttpStatusCode.OK)
            {
                // success = true;


                using (StreamReader reader = new StreamReader(responsemsg.GetResponseStream()))
                {
                    var responsedata = reader.ReadToEnd();
                    comp = JsonConvert.DeserializeObject<List<Competitors>>(responsedata);
                    var compl = comp.ToList();
                    foreach (var c in comp)
                    {
                        if (lstrButtons[0] != null)   //Lost Button  - competitor list does not neeed OOB or Refused 
                        {
                            if (!c.Description.Contains("N/A") && !c.Description.Contains("OOB") && !c.Description.Contains("Refused") && !c.Inactive.Contains("Y"))
                            {
                                complist.Add(c);
                            }
                        }

                        else
                            if (lstrButtons[1] != null)
                        {
                            if (c.Description.Contains("OOB - Not Confirmed"))  // only show the OOB not confirmed
                            {
                                complist.Add(c);
                            }
                        }
                        else
                                if (lstrButtons[2] != null)  // only show Refused  d 
                        {
                            if (c.Description.Contains("Refused"))
                            {
                                complist.Add(c);
                            }
                        }

                        //  System.Diagnostics.Debug.Write(responsedata);

                    }
                }

            }

            return complist;
        }

        private static bool UpdARCustomerSystemUserDef(int systemid, string competitortxt, string competitorothertxt)
        {
            bool success = false;

            string environment = ConfigurationManager.AppSettings["environment"];


            string username = ConfigurationManager.AppSettings["lgusrin"];
            string password = ConfigurationManager.AppSettings["lgusrps"];
            Byte[] documentBytes;  //holds the post body information in bytes

            //Prepare the request 
            CredentialCache credentialCache = new CredentialCache();

            credentialCache.Add(new Uri("https://sedoffapi.silcofs.com"), "Basic", new NetworkCredential(username, password));


            string uri;

            uri = "https://sedoffapi.silcofs.com/api/CustomerSystemUserdef/" + systemid;


            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AllowAutoRedirect = true;
            request.PreAuthenticate = true;
            request.Credentials = credentialCache;
            request.AutomaticDecompression = DecompressionMethods.GZip;


            request.Method = "PUT";
            request.ContentType = "application/json";

            //Call store proc for the information for a user 
            string body = "{\"CustomerSystemId\":" + systemid.ToString() + ",\"Table8Code\": \"" + competitortxt + "\" " + ",\"Text4\" : \"" + competitorothertxt + "\"" + ",\"Date2\" : \"" + DateTime.Today.ToString() + "\"}";
            // string ticketno;


            System.Text.ASCIIEncoding obj = new System.Text.ASCIIEncoding();
            documentBytes = obj.GetBytes(body); //convert string to bytes
            request.ContentLength = documentBytes.Length;
            //writing the stream
            using (Stream requestStream = request.GetRequestStream())
            {

                requestStream.Write(documentBytes, 0, documentBytes.Length);
                requestStream.Flush();
                requestStream.Close();
            }

            //Read the response back after writing the stream
            try
            {
                HttpWebResponse responsemsg = (HttpWebResponse)request.GetResponse();
                if ((responsemsg.StatusCode == HttpStatusCode.OK) || (responsemsg.StatusCode == HttpStatusCode.NoContent))
                {
                    success = true;

                }
                else
                {
                    success = false;
                }
            }
            catch
            {
                success = false;


            }
            //  HttpWebResponse responsemsg = (HttpWebResponse)request.GetResponse();

            return success;
        }

        private static bool UpdInspection(int inspectionid)
        {
            bool success = false;

            //Prepare the request 
            CredentialCache credentialCache = new CredentialCache();

            //  string uri = "https://silcosedonacustomapi/api/SV_Inspection/" + inspectionid;

            string uri = "https://silcosedonacustomapi.silcofs.com/api/SV_Inspection/";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            string authInfo = ConfigurationManager.AppSettings["apilgusrin"] + ":" + ConfigurationManager.AppSettings["apilgusrps"];
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;
            request.AllowAutoRedirect = true;
            request.PreAuthenticate = true;
            request.Credentials = credentialCache;
            request.AutomaticDecompression = DecompressionMethods.GZip;


            request.Method = "PUT";
            request.ContentType = "application/json";

            //Call store proc for the information for a user
            try
            {
                using (var streamWtr = new StreamWriter(request.GetRequestStream()))
                {
                    string body = "{\"Inspection_Id\":" + inspectionid + ",\"Next_Inspection_Date\": \"" + "1899-12-30T00:00:00" + "\"" + ",\"Route_Id\" : " + 1072 + "}";

                    streamWtr.Write(body);
                    streamWtr.Flush();
                    streamWtr.Close();
                }
            }
            catch (Exception ex)
            {

            }



            System.Text.ASCIIEncoding obj = new System.Text.ASCIIEncoding();



            try
            {
                HttpWebResponse responsemsg = (HttpWebResponse)request.GetResponse();
                if ((responsemsg.StatusCode == HttpStatusCode.OK) || (responsemsg.StatusCode == HttpStatusCode.NoContent))
                {
                    success = true;

                }
                else
                {
                    success = false;
                }
            }
            catch
            {
                success = false;


            }

            return success;
        }

        private static bool UpdEditLog(string inspcycledesc, string competitor, string action, string siteno, string customernumber, string systemcode)
        {
            string puser = System.Web.HttpContext.Current.Session["sessionLoginName"].ToString();

            bool success = false;


            string uri;

            uri = "https://silcosedonacustomapi.silcofs.com/api/EditLog/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AllowAutoRedirect = true;
            request.PreAuthenticate = true;
            // request.Credentials = credentialCache;
            string authInfo = ConfigurationManager.AppSettings["apilgusrin"] + ":" + ConfigurationManager.AppSettings["apilgusrps"];
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Method = "POST";
            request.ContentType = "application/json";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                //STOPPED NOT RETURNING A RESPONSE 

                string edtlg = "{\"user\" : \"" + puser + "\"" + ", \"inspectiontype\": \"" + inspcycledesc + "\"" + ", \"systemcode\": \"" + systemcode + "\"" + ", \"sitecode\": \"" + siteno + "\"" + ", \"action\": \"" + action + "\"" + ", \"code\": \"" + competitor + "\"" + ", \"customernumber\": \"" + customernumber + "\"" + "}";

                streamWriter.Write(edtlg);
                streamWriter.Flush();
                streamWriter.Close();
            }

            HttpWebResponse responsemsg = (HttpWebResponse)request.GetResponse();
            if (responsemsg.StatusCode == HttpStatusCode.OK)
            {
                success = true;
            }
            return success;
        }

        private static List<SearchInspections> SearchInspection(int siteid, int servcomp)
        {
            // SearchInspections j = new SearchInspections();
            List<SearchInspections> inspcont = new List<SearchInspections>();
            // string environment = ConfigurationManager.AppSettings["environment"];



            Byte[] documentBytes;  //holds the post body information in bytes


            string uri = "https://silcosedonacustomapi.silcofs.com/api/search?siteid=" + siteid + "&servcomp=" + servcomp;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);


            string authInfo = ConfigurationManager.AppSettings["apilgusrin"] + ":" + ConfigurationManager.AppSettings["apilgusrps"];
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;


            // HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AllowAutoRedirect = true;
            request.PreAuthenticate = true;

            request.AutomaticDecompression = DecompressionMethods.GZip;

            request.Method = "GET";
            request.ContentType = "text/json";

            //Call store proc for the information for a user 
            string body = "";
            // string ticketno;

            System.Text.ASCIIEncoding obj = new System.Text.ASCIIEncoding();
            documentBytes = obj.GetBytes(body); //convert string to bytes
            request.ContentLength = documentBytes.Length;


            HttpWebResponse responsemsg = (HttpWebResponse)request.GetResponse();
            if (responsemsg.StatusCode == HttpStatusCode.OK)
            {
                // success = true;


                using (StreamReader reader = new StreamReader(responsemsg.GetResponseStream()))
                {
                    var responsedata = reader.ReadToEnd();
                    inspcont = JsonConvert.DeserializeObject<List<SearchInspections>>(responsedata);

                    // System.Diagnostics.Debug.Write(responsedata);


                }

            }
            return inspcont;
        }
    }
}


