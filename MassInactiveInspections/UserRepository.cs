using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;  //add this by using add reference not nuget
using System.Text;
using ActiveDs;  //add this by adding a reference look under com objects Active DS type library

namespace MassInactiveInspections
{
    public class UserRepository
    {
        // Get LDAP path from AppSettings in Web.config
        private string _path;

        // Will retrieve groups that have permission to access this service
     //   private PermissionRetriever _permissionRetreiver;
        private string[] _permittedGroups;

        public UserRepository()
        {
          //  _permissionRetreiver = new PermissionRetriever();
            _path = System.Configuration.ConfigurationManager.AppSettings.GetValues("LDAPPath")[0];   //value in webconfig
            _permittedGroups = GetGroups();
        }

        public UserInfo Authenticate(string userName, string password)
        {
            // Authenticate against the supplied user info and return the retrieved properties

            // LDAP properties
            DirectoryEntry root = new DirectoryEntry("LDAP://" + _path, userName, password);
            DirectorySearcher search = new DirectorySearcher(root);

            UserInfo user = null;

            try
            {
                // Search AD for the supplied username
                search.Filter = "(sAMAccountName=" + userName + ")";
                search.PropertiesToLoad.Add("cn");
                SearchResult result = search.FindOne();

                if (result != null)
                    user = _GetUser(result.Path);
            }
            catch { user = null; }

            return user;
        }

        private UserInfo _GetUser(string userPath)
        {
            DirectoryEntry entry = new DirectoryEntry(userPath);
            UserInfo user = new UserInfo();

            user.userName = entry.Properties["sAMAccountName"].Value.ToString();

            entry.Close();

            return user;
        }

        public string CheckAccountStatus(string userName)
        {
            // If authentication failed:
            // Check if the user's account is locked out. If it is, let them know.
            string accountStatus = "Invalid username or password.";
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "silcofireprotection.com");
            UserPrincipal user = UserPrincipal.FindByIdentity(ctx, userName);

            if (user != null)
            {
                if (user.IsAccountLockedOut()) accountStatus = "Account is locked out. ";
                else
                {
                    DirectoryEntry entry = new DirectoryEntry("LDAP://" + user.DistinguishedName);
                   // IADsUser native = (IADsUser)entry.NativeObject;
                   // if (native.PasswordExpirationDate.CompareTo(DateTime.Now) < 1) accountStatus = "Password Invalid Make sure Caps Lock is not on";
                }

            }
            return accountStatus;
        }

        public bool IsInPermittedGroup(string userName)
        {
            // Check that the user is in a group with permissions to access application
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "silcofireprotection.com");
            UserPrincipal user = UserPrincipal.FindByIdentity(ctx, userName);

            foreach (string group in _permittedGroups)
            {
                if (user.IsMemberOf(GroupPrincipal.FindByIdentity(ctx, group)))
                {
                    return true;
                }
            }

            return false;
        }

        public string[] GetGroups()
        {
            string[] arr1 = new string[] { "SilcoAppMassInact" };


            _permittedGroups = arr1;

            return _permittedGroups;
        }


        public string IsInDepartment(string userName, string password)
        {
            // Authenticate against the supplied user info and return the retrieved properties

            // LDAP properties
            DirectoryEntry root = new DirectoryEntry("LDAP://" + _path, userName, password);


            DirectorySearcher search = new DirectorySearcher(root);

            //   UserInfo user = null;

            try
            {
                // Search AD for the supplied username
                search.Filter = "(sAMAccountName=" + userName + ")";
                search.PropertiesToLoad.Add("cn");
                search.PropertiesToLoad.Add("department");

                SearchResult result = search.FindOne();
                //    GetProperty(result.AD)
                //  result.GetDirectoryEntry();
                //  result.Properties.de
                if (result.Properties.Contains("department"))
                {
                    string dpt = result.Properties["department"][0].ToString();
                    // string dpt = result.Properties.Contains("Department");
                    return dpt;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
        public string HasEmail(string userName, string password)
        {
            // Authenticate against the supplied user info and return the retrieved properties

            // LDAP properties
            DirectoryEntry root = new DirectoryEntry("LDAP://" + _path, userName, password);


            DirectorySearcher search = new DirectorySearcher(root);

            //   UserInfo user = null;

            try
            {
                // Search AD for the supplied username
                search.Filter = "(sAMAccountName=" + userName + ")";
                search.PropertiesToLoad.Add("cn");
                search.PropertiesToLoad.Add("mail");

                SearchResult result = search.FindOne();
                //    GetProperty(result.AD)
                //  result.GetDirectoryEntry();
                //  result.Properties.de
                if (result.Properties.Contains("mail"))
                {
                    string dpt = result.Properties["mail"][0].ToString();
                  //  string dspname = result.Properties["displayname"][0].ToString();
                    // string dpt = result.Properties.Contains("Department");
                    return dpt;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
        public string HasDisplayName(string userName, string password)
        {
            // Authenticate against the supplied user info and return the retrieved properties

            // LDAP properties
            DirectoryEntry root = new DirectoryEntry("LDAP://" + _path, userName, password);


            DirectorySearcher search = new DirectorySearcher(root);

            //   UserInfo user = null;

            try
            {
                // Search AD for the supplied username
                search.Filter = "(sAMAccountName=" + userName + ")";
                search.PropertiesToLoad.Add("cn");
                search.PropertiesToLoad.Add("displayname");

                SearchResult result = search.FindOne();

                if (result.Properties.Contains("displayname"))
                {
                    string dspname = result.Properties["displayname"][0].ToString();
            
                    return dspname;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
          


        }
        public string HasTitle(string userName, string password)
        {
            // Authenticate against the supplied user info and return the retrieved properties

            // LDAP properties
            DirectoryEntry root = new DirectoryEntry("LDAP://" + _path, userName, password);


            DirectorySearcher search = new DirectorySearcher(root);

            //   UserInfo user = null;

            try
            {
                // Search AD for the supplied username
                search.Filter = "(sAMAccountName=" + userName + ")";
                search.PropertiesToLoad.Add("cn");
                search.PropertiesToLoad.Add("title");

                SearchResult result = search.FindOne();
                //    GetProperty(result.AD)
                //  result.GetDirectoryEntry();
                //  result.Properties.de
                if (result.Properties.Contains("title"))
                {
                    //string dpt = result.Properties["mail"][0].ToString();
                    string title = result.Properties["title"][0].ToString();
                    // string dpt = result.Properties.Contains("Department");
                    return title;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }



        }
    }
}