using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MassInactiveInspections
{
    public class UserInfo
    {
        // This class is mostly just extra fluff right now, but in the event
        // that we need to track more user attributes later or flag the user
        // in some way, this class can be expanded as necessary.


        private string _userName;

        public string userName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        public UserInfo()
        {
            this.userName = "";
        }
    }
}