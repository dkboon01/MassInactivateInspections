using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;

using System.Net;
using System.IO;
using System.Configuration;
using System.Data;
using System.Collections;
using MassInactiveInspections;

using System.Data.Entity;

using System.Data.SqlClient;
using MassInactiveInspections.Models;
using System.Globalization;

namespace MassInactiveInspections.Models
{

    public class Helper
    {
        //public static List<GetAuthorizationForLeads_Result> GetAuthorizationforLeads<T>(string username, int appid)

        //where T : class
        //{
           
        //    var db = new Cust_SilcoEntities();
        //    List<GetAuthorizationForLeads_Result> authorizations = db.GetAuthorizationForLeads(username, appid).ToList();
        //    return authorizations;
        //}
        public static List<GetADGroups_Result> GetAD_Groups<T>(int appid) where T : class
        {
            var db = new Cust_SilcoEntities();
            List<GetADGroups_Result> groups = db.GetADGroups(appid).ToList();
            //foreach (var grp in groups)
            //{
            //    grp.adsecuritygroupallowed
            //}
            // List<GetAuthorizationForLeads_Result> groups = db.GetAuthorizationForLeads(appid).ToList();  
            return groups;
        }
        public static List<FindSecurityMassInact_Result> FindSecurity<T>(int appid, string title, string dept, string account) where T : class
        {

            var sb = new Cust_SilcoEntities();

            List<FindSecurityMassInact_Result> seclist = sb.FindSecurityMassInact(appid, title, dept, account).ToList();
            return seclist;
        }

        public static List<DoesCustomerNumberExist_Result> ExistingCustomerNumber<T>( string results) where T : class
        {

            var sb = new Cust_SilcoEntities();

            List<DoesCustomerNumberExist_Result> seclist = sb.DoesCustomerNumberExist(results).ToList();
            return seclist;
        }

        public static List<GetCustomerInspectionInformation_Result> GetCustomerInspectionInfo<T>(string customerNumber) where T : class
        {

            var sb = new Cust_SilcoEntities();

            List<GetCustomerInspectionInformation_Result> seclist = sb.GetCustomerInspectionInformation(customerNumber).ToList();
            return seclist;
        }

        //public static int ExistingCustomerNumber(string customerNumber)
        //{
        //    var sb = new Cust_SilcoEntities();

        //    var custnum = sb.DoesCustomerNumberExist(customerNumber);

        //    return 0;
        //}

        //public static List<> ExistingCustomer<T>(string customer) where T : class
        //{

        //    var sb = new Cust_SilcoEntities();

        //    List<DoesCustomerNumberExist_Result> cust = sb.DoesCustomerNumberExist(customer).ToList();

        //    return cust;
        //}

    }
}


