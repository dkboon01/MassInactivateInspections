using System.Collections.Generic;
using System.Linq;

namespace MassInactiveInspections.Models
{

    public class Helper
    {
        public static List<GetADGroups_Result> GetAD_Groups<T>(int appid) where T : class
        {
            var db = new Cust_SilcoEntities();
            List<GetADGroups_Result> groups = db.GetADGroups(appid).ToList();
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
    }
}


