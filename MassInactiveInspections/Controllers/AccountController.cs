using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using MassInactiveInspections.Models;
using System.Web.Security;
using MassInactiveInspections;


namespace MassInactiveInspections.Controllers
{
    public class AccountController : System.Web.Mvc.Controller
    {
        private UserRepository _userRepository;
        private System.Web.Security.FormsAuthentication _formsAuthentication;
        public UserManager<ApplicationUser> UserManager { get; private set; }

        public AccountController()
              : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
            _userRepository = new UserRepository();
            _formsAuthentication = new FormsAuthentication();
        }

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            _userRepository = new UserRepository();
            _formsAuthentication = new FormsAuthentication();
            UserManager = userManager;
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            Session.Clear();
            Response.Cookies.Clear();
            //Roles.DeleteCookie();

            // clear authentication cookie
            HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            cookie1.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie1);

            // clear session cookie (not necessary for your current problem but i would recommend you do it anyway)
            HttpCookie cookie2 = new HttpCookie("ASP.NET_SessionId", "");
            cookie2.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie2);
          //  return View();
           return RedirectToAction("Login", "Account");
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {

            //    ActionResult result = View("LogOn");
            System.Web.HttpContext.Current.Session["ProcessPastDue"] = 0;
            ActionResult result = View("Login");
            UserInfo user = _userRepository.Authenticate(model.UserName, model.Password);

            if (ModelState.IsValid)
            {
                if (user != null)
                {
                    if (_userRepository.IsInPermittedGroup(model.UserName))
                    {
                        
                        // The user has authenticated correctly and is in an appropriate group
                        // Go ahead and create auth tokens.
                        System.Web.Security.FormsAuthenticationTicket authTicket = new System.Web.Security.FormsAuthenticationTicket(1, model.UserName, DateTime.Now, DateTime.Now.AddMinutes(15), false, model.UserName);
                        string encryptedTicket = System.Web.Security.FormsAuthentication.Encrypt(authTicket);
                        HttpCookie authCookie = new HttpCookie(System.Web.Security.FormsAuthentication.FormsCookieName, encryptedTicket);
                        Response.Cookies.Add(authCookie);
                        System.Web.HttpContext.Current.Session["sessionLoginName"] = model.UserName;
                        System.Web.HttpContext.Current.Session["department"] = _userRepository.IsInDepartment(model.UserName, model.Password);
                        System.Web.HttpContext.Current.Session["E-mail"] = _userRepository.HasEmail(model.UserName, model.Password);
                        System.Web.HttpContext.Current.Session["displayname"] = _userRepository.HasDisplayName(model.UserName, model.Password);
                        System.Web.HttpContext.Current.Session["title"] = _userRepository.HasTitle(model.UserName, model.Password);
                        System.Web.HttpContext.Current.Session["appid"] = 13;

                        var seclist = MassInactiveInspections.Models.Helper.FindSecurity<FindSecurityMassInact_Result>(Convert.ToInt32(System.Web.HttpContext.Current.Session["appid"]), System.Web.HttpContext.Current.Session["title"].ToString(), System.Web.HttpContext.Current.Session["department"].ToString(), " ").ToList();

                        if (seclist.Count() > 0)
                        {
                            foreach (var sc in seclist)
                            {
                                System.Web.HttpContext.Current.Session["MassInact"] = sc.canprocessmassinact;
                                //System.Web.HttpContext.Current.Session["CanViewProspects"] = sc.canviewprospects.Value;
                                //System.Web.HttpContext.Current.Session["CanViewSiteLookup"] = sc.canviewmaster.Value;
                                //System.Web.HttpContext.Current.Session["CanViewPR"] = sc.canviewpr.Value;
                                //System.Web.HttpContext.Current.Session["CanViewDepts"] = sc.canviewdepts.Value;
                                
                            }
                        }
                        else
                        {
                            System.Web.HttpContext.Current.Session["ProcessPastDue"] = 0;
                            //System.Web.HttpContext.Current.Session["CanViewProspects"] = 0;
                            //System.Web.HttpContext.Current.Session["CanViewSiteLookup"] = 0;
                            //System.Web.HttpContext.Current.Session["CanViewPR"] = 0;
                            //System.Web.HttpContext.Current.Session["CanViewDepts"] = 0;
                        }
  
               
              
                        //

                        // Redirect the user to desired page
                        if (returnUrl == null)
                        {
                            result = RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            result = _redirectToLocal(returnUrl);
                        }

                        return result;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unauthorized: Not Allowed Access to Process Past Due.");

                        // Log this to database

                    }
                }
                else
                {
                    string accountStatus = _userRepository.CheckAccountStatus(model.UserName);
                    ModelState.AddModelError("", accountStatus);
                }
            }

            // If we got this far, something failed, redisplay form
            return View("Login", model);
        }

        private ActionResult _redirectToLocal(string returnUrl)
        {
            // Simply redirects the user to the desired page.
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}