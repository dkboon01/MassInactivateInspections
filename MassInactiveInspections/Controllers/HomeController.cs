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
                        customerlist.additionalCustomerInfoListView.Add(new AdditionalCustomerInfo() {
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
                            InspectionId = customer.Inspection_ID });
                    }
                    customerlist.competitorslist = GetCompetitors(LostButton, OOBButton, RefusedButton);
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
        [HttpPost]
        public ActionResult ViewCustomerInfo(AdditionalCustomerInfoList customerInfoList)
        {
            return View("SelectedInspections", (object)customerInfoList);
        }

        [HttpPost]
        public ActionResult Lost(AdditionalCustomerInfoList customerInfoLis)
        {
            int cstsystemid = Convert.ToInt32(customerInfoLis.additionalCustomerInfoListView.FirstOrDefault().CustomerSystemId);
            //int cstinspectionid = Convert.ToInt32(ticketmodel.apit.InspectionId);
            string competitorsel = customerInfoLis.Selectedcompetitor;
            string competitorothertxt = customerInfoLis.competitorothertxt;
            //int ticketid = ticketmodel.apit.ServiceTicketId;

            SuccessFailViewModel sf = new SuccessFailViewModel();
            sf.CustomerNumber = customerInfoLis.additionalCustomerInfoListView.FirstOrDefault().CustomerNumber;
            bool success = false;
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
                    success = UpdInspection(customerInfoLis.additionalCustomerInfoListView.FirstOrDefault().InspectionId);

                    if (success)
                    {
                                    success = UpdEditLog(customerInfoLis.additionalCustomerInfoListView.FirstOrDefault().InspectionCycleDescription, competitorsel, "Lost", customerInfoLis.additionalCustomerInfoListView.FirstOrDefault().SiteNumber, customerInfoLis.additionalCustomerInfoListView.FirstOrDefault().CustomerNumber, customerInfoLis.additionalCustomerInfoListView.FirstOrDefault().SystemCode);
                        //            //Part 2. check to see if any other checkbox for inspections is true then process them
                        //            // checking to make sure there are other inspections
                        //            if (ticketmodel.sysinsp != null && ticketmodel.sysinsp.Any())
                        //            {
                        //                foreach (SearchInspections insp in ticketmodel.sysinsp)
                        //                {
                        //                    if (insp.IsSelected == true)  //is an inspection selected
                        //                    {
                        //                        if (ticketmodel.apit.InspectionId != insp.inspectionid)    // make sure we have not already updated the inspection
                        //                        {
                        //                            try
                        //                            {
                        //                                success = UpdARCustomerSystemUserDef(insp.customer_system_id, competitorsel, competitorothertxt);
                        //                                try
                        //                                {
                        //                                    success = UpdInspection(insp.inspectionid);
                        //                                    try
                        //                                    {
                        //                                        success = UpdEditLog(insp.inspcycldesc, competitorsel, "Lost", ticketmodel.apit.Site.SiteNumber, ticketmodel.apit.Customer.CustomerNumber, insp.syscode);
                        //                                    }
                        //                                    catch
                        //                                    {
                        //                                        success = false;
                        //                                    }

                        //                                }
                        //                                catch
                        //                                {
                        //                                    success = false;
                        //                                }

                        //                            }

                        //                            catch
                        //                            {
                        //                                success = false;
                        //                            }
                        //                        }


                        //                    }

                        //                }
                        //            }
                        //            else
                        //            {
                        //                success = true;
                        //            }
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
            if (success)
            {
                return View("Success", sf);
            }
            else
            {
                return View("Failed", sf);
            }
            
        }

        private static List<Competitors> GetCompetitors(string LostButton, string OOBButton, string RefusedButton)
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
                        if (LostButton != null)   //Lost Button  - competitor list does not neeed OOB or Refused 
                        {
                            if (!c.Description.Contains("N/A") && !c.Description.Contains("OOB") && !c.Description.Contains("Refused") && !c.Inactive.Contains("Y"))
                            {
                                complist.Add(c);
                            }
                        }

                        else
                            if (OOBButton != null)
                        {
                            if (c.Description.Contains("OOB - Not Confirmed"))  // only show the OOB not confirmed
                            {
                                complist.Add(c);
                            }
                        }
                        else
                                if (RefusedButton != null)  // only show Refused  d 
                        {
                            if (c.Description.Contains("OOB"))
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

            credentialCache.Add(new Uri("https://testsedoffapi.silcofs.com"), "Basic", new NetworkCredential(username, password));


            string uri;

            uri = "https://testsedoffapi.silcofs.com/api/CustomerSystemUserdef/" + systemid;


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

            //  string uri = "https://testsilcosedonacustomapi/api/SV_Inspection/" + inspectionid;

            string uri = "https://testsilcosedonacustomapi.silcofs.com/api/SV_Inspection/";
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

            uri = "https://testsilcosedonacustomapi.silcofs.com/api/EditLog/";

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
    }
}


