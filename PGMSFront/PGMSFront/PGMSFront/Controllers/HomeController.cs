using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using PGMSFront.Common;
using PGMSFront.Models;
using PGMSFront.WCFPGMSRef;

namespace PGMSFront.Controllers
{
    public class HomeController : Controller
    {

        #region Global Variables
        Service1Client objServiceClient = new Service1Client();
        ClassUserFunctions objClassUserFunctions = new ClassUserFunctions();
        static string strRptURL = System.Configuration.ConfigurationManager.AppSettings["ReportUrl"];
        static string strPOURL = System.Configuration.ConfigurationManager.AppSettings["strPOURL"];
        static string strbookingsupportdoc = System.Configuration.ConfigurationManager.AppSettings["strbookingsupportdoc"];
        // static string strFromEmail = System.Configuration.ConfigurationManager.AppSettings["strFromEmailId"];
        // static string strFromPwd = System.Configuration.ConfigurationManager.AppSettings["strFromPwd"];
        static string strNATRAXAdminEmailId = System.Configuration.ConfigurationManager.AppSettings["strNATRAXAdminEmailId"];
        static string strTrackNewBookingSubmitMailNotification = System.Configuration.ConfigurationManager.AppSettings["strTrackNewBookingSubmitMailNotification"];
        static string strLabNewBookingSubmitMailNotification = System.Configuration.ConfigurationManager.AppSettings["strLabNewBookingSubmitMailNotification"];
        static string strRegistrationUploadDocument = System.Configuration.ConfigurationManager.AppSettings["strRegistrationUploadDocument"];
        static string strPOUploadedEmailNATRAX = System.Configuration.ConfigurationManager.AppSettings["strPOUploadedEmailNATRAX"];
        static string strPOUploadedCCEmailNATRAX = System.Configuration.ConfigurationManager.AppSettings["strPOUploadedCCEmailNATRAX"];

        internal string strFromEmail = System.Configuration.ConfigurationManager.AppSettings["strLiveMailId"];
        internal string strFromPwd = System.Configuration.ConfigurationManager.AppSettings["strLiveMailPWD"];




        #endregion

        #region Login
        public ActionResult Index()
        {
            LoginModel model = new LoginModel();
            try
            {
                Session.Abandon();
                Session.Clear();
            }
            catch
            {
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginModel model)
        {
            try
            {
                ViewData["Resend"] = 0;
                string strMSG = "";
                if (model.LoginId == null || model.LoginId.Trim() == "")
                    strMSG = strMSG + "User Name \n";
                if (model.Password == null || model.Password.Trim() == "")
                    strMSG = strMSG + "Password \n";

                if (strMSG.Trim() == "")
                {
                    string strPSW = DecryptStringAES(model.Password);

                    //Get User By LoginId and Password
                    returndbmlUser objreturndbmlUser = objServiceClient.UserGetByLoginId(model.LoginId, strPSW);

                    //Check User Valid or Not
                    if (objreturndbmlUser.objdbmlStatus != null && objreturndbmlUser.objdbmlStatus.StatusId == 1 && objreturndbmlUser.objdbmlUserView.Count > 0)
                    {

                        dbmlUserView objdbmlUserView = objreturndbmlUser.objdbmlUserView.FirstOrDefault();

                        //Create Session Variable for User 
                        Session["UserId"] = objdbmlUserView.UserId;
                        Session["UserTypePropId"] = objdbmlUserView.UserTypePropId;
                        Session["ZZCompanyId"] = objdbmlUserView.CustomerMasterId;
                        Session["CompanyDepartmentId"] = objdbmlUserView.CustomerDepartmentId;
                        Session["UserName"] = objdbmlUserView.UserName;
                        Session["EmailId"] = objdbmlUserView.EmailId;
                        Session["LoginId"] = objdbmlUserView.LoginId;
                        Session["ZZUserType"] = objdbmlUserView.ZZUserType;
                        Session["UserCode"] = objdbmlUserView.UserCode;
                        Session["StateId"] = objdbmlUserView.ZZStateId;
                        Session["ZZCompanyName"] = objdbmlUserView.ZZCompanyName;
                        Session["MobileNo"] = objdbmlUserView.MobileNo;
                        returndbmlProperty objreturndbmlProperty = objServiceClient.PropertiesGetAll();
                        if (objreturndbmlProperty.objdbmlStatus.StatusId == 1 && objreturndbmlProperty.objdbmlProperty.Count > 0)
                        {
                            Session["Properties"] = objreturndbmlProperty.objdbmlProperty;
                        }

                        returndbmlLablinkVorC objreturndbmlLablinkVorC = objServiceClient.LablinkVorCGetAll();
                        if (objreturndbmlLablinkVorC.objdbmlStatus.StatusId == 1 && objreturndbmlLablinkVorC.objdbmlLablinkVorC.Count > 0)
                        {
                            Session["LablinkVorC"] = objreturndbmlLablinkVorC.objdbmlLablinkVorC;
                        }

                        //returndbmlCompanyView objreturndbmlCompanyView = objServiceClient.CompanyViewGetByCompanyId(Convert.ToInt32(objdbmlUserView.CustomerMasterId));
                        //if (objreturndbmlCompanyView.objdbmlStatus.StatusId == 1 && objreturndbmlCompanyView.objdbmlCompanyView.Count > 0)
                        //{
                        //    Session["Company"] = objreturndbmlCompanyView.objdbmlCompanyView.FirstOrDefault();
                        //    Session["StateId"] = objreturndbmlCompanyView.objdbmlCompanyView.FirstOrDefault().StateId;
                        //}

                        return RedirectToAction("Dashboard", "Home");

                    }
                    else
                    {
                        string strErrMSG = objreturndbmlUser.objdbmlStatus.Status;

                        if (objreturndbmlUser.objdbmlStatus.StatusId == 20 || objreturndbmlUser.objdbmlStatus.StatusId == 30)
                        {
                            ViewData["Resend"] = 1;
                            dbmlUserView objdbmlUserView = objreturndbmlUser.objdbmlUserView.FirstOrDefault();
                            Session["dbmlUserView"] = objdbmlUserView;
                            strErrMSG += "\nClick 'OK' to go back to Login Page or 'RESEND' if you have not received the verification link";
                        }

                        //model.Message = strErrMSG;
                        ViewData["MSG"] = strErrMSG;
                    }
                }
                else
                {
                    //model.Message = "Please enter data for Mandatory fields  \n" + strMSG;
                    ViewData["MSG"] = "Please enter data for Mandatory fields  \n" + strMSG;
                }
            }
            catch (Exception ex)
            {
                // model.Message = ex.Message;
                ViewData["MSG"] = ex.Message;
            }

            return View(model);
        }

        [ValidateAntiForgeryToken]
        public ActionResult ResendVerifyLink()
        {
            int intStatusId = 99;
            string strStatus = "Invalid User Details";
            try
            {
                if (Session["dbmlUserView"] != null)
                {
                    dbmlUserView objdbmlUserView = new dbmlUserView();
                    GeneralColl<dbmlUserView>.CopyObject(Session["dbmlUserView"] as dbmlUserView, objdbmlUserView);

                    string strHost = System.Configuration.ConfigurationManager.AppSettings["strHostName"]; //"https://localhost:44307/";
                    string strLink = strHost + "Home/VerifyeMail?xyz=0dfs,ktgbdas,hdffg.khdfrhdduihdgtymdmpxjidgndlxcmhdgmdpldjn,dlkchgj,d,.fddfyre,hjlhhjhjlhjljhjlhdkjdhhdk,dmdhhnd,dkmdndhnndmdkkfbhjyhnhhfssdfgngfgfghgfjfgjgffbgfhfhfhdffdsfdgfdfhfhgfhgfjfwrtwfghkyredcbnmkiufssfgyhvgdrfrthhhjhmjmd&abc="
                                        + objdbmlUserView.UserId
                                        + "&lmn=0dshffn56tgrehbncv6nwyuwgkliurscvjl'ljugbmkl;lkitgn;''lkjhhhjl;llkyhfcfbmkkdfhdfgffhf561g4d5bvgdf1bbdfbdvfgbnvbncvncvbbnxcdgfcbcb";
                    string strReplyTo = "",
                        strTo = objdbmlUserView.EmailId,
                        strBcc = string.Empty,
                        strCc = string.Empty,
                        strSubject = string.Empty, strBody = string.Empty;


                    strSubject = "Natrax - Verify email";
                    strBody = "Hello, ";

                    strBody += "<br /><br /><b>" + objdbmlUserView.UserName + "</b>";
                    strBody += "<br /><b>" + objdbmlUserView.ZZCompanyName + "</b>";

                    strBody += "<br /><br />You have successfully registered on Natrax with Login-ID <b> '" + objdbmlUserView.LoginId + "</b>'";
                    strBody += ", please click on the below link to verify your email and create passwords";
                    strBody += "<br /><b><a href='" + strLink + "'>Click Here</a></b>";



                    strBody += "<br /><br /><br />Regards";
                    strBody += "<br /><br /><span style='font-weight:bold;font-family:Trebuchet MS;font-style:italic'>Natrax Administrator</span>";

                    bool blnSentMail = objClassUserFunctions.SendMailMessage(strFromEmail, strFromPwd, strTo, strReplyTo, strBcc, strCc, strSubject, strBody, null, "");
                    strStatus = "eMail has been resent successfully";
                    intStatusId = 1;
                }

            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ForgotPassword()
        {
            LoginModel model = new LoginModel();
            try
            {

            }
            catch
            {
            }

            return View(model);
        }

        [ValidateAntiForgeryToken]
        public ActionResult ForgotPasswordReset(dbmlUserView model)
        {
            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlUser objreturndbmlUser = new returndbmlUser();
            try
            {

                objreturndbmlUser = objServiceClient.UserPaswordForgot(model.LoginId, model.EmailId);
                if (objreturndbmlUser != null && objreturndbmlUser.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = objreturndbmlUser.objdbmlStatus.Status;
                }
                else
                {
                    strStatus = objreturndbmlUser.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId }, JsonRequestBehavior.AllowGet);
        }

        public static string DecryptStringAES(string cipherText)
        {

            var keybytes = Encoding.UTF8.GetBytes("A51f7e2h2j58r2d5");
            var iv = Encoding.UTF8.GetBytes("A51f7e2h2j58r2d5");

            var encrypted = Convert.FromBase64String(cipherText);
            var decriptedFromJavascript = DecryptStringFromBytes(encrypted, keybytes, iv);
            return string.Format(decriptedFromJavascript);
        }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (var rijAlg = new RijndaelManaged())
            {
                //Settings
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
                try
                {
                    // Create the streams used for decryption.
                    using (var msDecrypt = new MemoryStream(cipherText))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {

                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                // Read the decrypted bytes from the decrypting stream
                                // and place them in a string.
                                plaintext = srDecrypt.ReadToEnd();

                            }

                        }
                    }
                }
                catch
                {
                    plaintext = "keyError";
                }
            }

            return plaintext;
        }

        #endregion

        #region Dashboard
        public ActionResult Dashboard()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.StateId = Convert.ToInt32(Session["StateId"]);
                model.ReportURL = strRptURL;
            }
            catch
            {
            }

            return View(model);
        }

        public ActionResult LoadDashboardInfo()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlDashBoardWorkFlowViewFront objreturndbmlDashBoardWorkFlowViewFront = new returndbmlDashBoardWorkFlowViewFront();
            try
            {
                objreturndbmlDashBoardWorkFlowViewFront = objServiceClient.DashBoardWorkFlowCount(Convert.ToInt32(Session["UserId"]), Convert.ToInt32(Session["ZZCompanyId"]));
                intStatusId = (int)objreturndbmlDashBoardWorkFlowViewFront.objdbmlStatus.StatusId;
                strStatus = objreturndbmlDashBoardWorkFlowViewFront.objdbmlStatus.Status;
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, DashboardList = objreturndbmlDashBoardWorkFlowViewFront.objdbmlDashBoardWorkFlowViewFront }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DashboardLog()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlDashBoardWorkFlowViewFront objreturndbmlDashBoardWorkFlowViewFront = new returndbmlDashBoardWorkFlowViewFront();
            try
            {
                objreturndbmlDashBoardWorkFlowViewFront = objServiceClient.DashboardLog(Convert.ToInt32(Session["UserId"]), Convert.ToInt32(Session["ZZCompanyId"]));
                intStatusId = (int)objreturndbmlDashBoardWorkFlowViewFront.objdbmlStatus.StatusId;
                strStatus = objreturndbmlDashBoardWorkFlowViewFront.objdbmlStatus.Status;
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, DashboardList = objreturndbmlDashBoardWorkFlowViewFront.objdbmlDashBoardWorkFlowViewFront }, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region Booking

        #region Booking Search/New/Inprocess
        public ActionResult ManageBooking()
        {
            CommonModel model = new CommonModel();
            try
            {
                Session["SessBookingType"] = "Track";
                Session["SessHistoryType"] = "ManageBooking";

                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["objdbmlBooking"] = null;

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);

                ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
                ViewBag.BookingType = GetBookingType();
                ViewBag.BookingStatus = GetBookingStatus();
            }
            catch
            {
            }

            return View(model);
        }

        [ValidateAntiForgeryToken]
        public ActionResult BookingGetByCompanyIdStatusPropId()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlBooking objreturndbmlBooking = new returndbmlBooking();

            try
            {
                int intCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                int intStatusPropId = 40;

                objreturndbmlBooking = objServiceClient.BookingViewGetByCompanyIdStatusPropId(intCompanyId, intStatusPropId);

                if (objreturndbmlBooking != null && objreturndbmlBooking.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objreturndbmlBooking.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, BookingList = objreturndbmlBooking.objdbmlBookingList }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult BookingSearchViewGetByDepartmentBookinStatus(int intDepartmentId, int intBookingTypeId, int intStatusPropId)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            int intUserId = Convert.ToInt32(Session["UserId"]);
            string strStatus = "Invalid";

            returndbmlBookingSearchView objreturndbmlBookingSearchView = new returndbmlBookingSearchView();

            try
            {
                //objreturndbmlBookingSearchView = objServiceClient.BookingSearchViewGetByCompanyIdFromDateToDateFront(intDepartmentId, DateTime.Now.Date, DateTime.Now.Date, intBookingTypeId, intStatusPropId);
                objreturndbmlBookingSearchView = objServiceClient.BookingSearchViewGetByCompanyIdFromDateToDateFrontNew(intDepartmentId, DateTime.Now.Date, DateTime.Now.Date, intBookingTypeId, intStatusPropId, intUserId);
                if (objreturndbmlBookingSearchView != null && objreturndbmlBookingSearchView.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objreturndbmlBookingSearchView.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, BookingList = objreturndbmlBookingSearchView.objdbmlBookingSearchView }, JsonRequestBehavior.AllowGet);
        }

        //[ValidateAntiForgeryToken]
        public ActionResult BookingGetByBookingId(int intBookingId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            returndbmlBooking objreturndbmlBooking = new returndbmlBooking();
            try
            {
                objreturndbmlBooking = objServiceClient.BookingViewGetByBookingId(intBookingId);

                if (objreturndbmlBooking != null && objreturndbmlBooking.objdbmlStatus.StatusId == 1)
                {
                    Session["objdbmlBooking"] = objreturndbmlBooking.objdbmlBookingList.FirstOrDefault();
                    Session["BPId"] = objreturndbmlBooking.objdbmlBookingList.Count() > 0 ? objreturndbmlBooking.objdbmlBookingList.FirstOrDefault().BPId : 0;
                }
            }
            catch
            {

            }

            return RedirectToAction("Basic", "Home");
        }

        public ActionResult BookingGetByBookingIdToLoad(int intBookingId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            returndbmlBooking objreturndbmlBooking = new returndbmlBooking();
            try
            {
                objreturndbmlBooking = objServiceClient.BookingViewGetByBookingId(intBookingId);

                if (objreturndbmlBooking != null && objreturndbmlBooking.objdbmlStatus.StatusId == 1)
                {
                    Session["objdbmlBooking"] = objreturndbmlBooking.objdbmlBookingList.FirstOrDefault();
                    Session["BPId"] = objreturndbmlBooking.objdbmlBookingList.Count() > 0 ? objreturndbmlBooking.objdbmlBookingList.FirstOrDefault().BPId : 0;
                }
            }
            catch
            {

            }

            return Json(new { Status = "Successful", StatusId = 1, BookingList = objreturndbmlBooking.objdbmlBookingList.FirstOrDefault() }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult NewBooking(int intBPId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                Session["BPId"] = intBPId;
            }
            catch
            {

            }

            return RedirectToAction("Basic", "Home");
        }

        public List<SelectListItem> GetBookingType()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            try
            {
                Items.Add(new SelectListItem { Text = "Booking", Value = Convert.ToString(Convert.ToInt32(HardCodeValues.BookingBPId)) });
                Items.Add(new SelectListItem { Text = "RFQ - Confidential", Value = Convert.ToString(Convert.ToInt32(HardCodeValues.RFQConfBPId)) });
                Items.Add(new SelectListItem { Text = "RFQ - Regular", Value = Convert.ToString(Convert.ToInt32(HardCodeValues.RFQRegBPId)) });
            }
            catch
            {

            }
            return Items;
        }

        public List<SelectListItem> GetLabBookingType()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            try
            {
                Items.Add(new SelectListItem { Text = "Lab Booking", Value = Convert.ToString(Convert.ToInt32(HardCodeValues.LabBookingBPId)) });
                Items.Add(new SelectListItem { Text = "Lab RFQ", Value = Convert.ToString(Convert.ToInt32(HardCodeValues.LabRFQRegBPId)) });
            }
            catch
            {

            }
            return Items;
        }

        public List<SelectListItem> GetBookingStatus()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            try
            {
                Items.Add(new SelectListItem { Text = "All", Value = "0" });
                Items.Add(new SelectListItem { Text = "In progress", Value = "40" });
                Items.Add(new SelectListItem { Text = "Approve", Value = "38" });
            }
            catch
            {

            }
            return Items;
        }

        //public List<SelectListItem> GetBookingWorkFlowStatus(int intBPId)
        //{
        //    List<SelectListItem> Items = new List<SelectListItem>();
        //    try
        //    {
        //        ObservableCollection<dbmlWorkFlowView> objdbmlWorkFlowView = WorkFlowViewGetByBPId(intBPId, 0);

        //        foreach (var itm in objdbmlWorkFlowView)
        //        {
        //            Items.Add(new SelectListItem { Text = itm.WorkFlowName, Value = itm.WorkFlowId.ToString() });
        //        }
        //    }
        //    catch
        //    {

        //    }
        //    return Items;
        //}

        public ActionResult TrackBookingHistory()
        {
            CommonModel model = new CommonModel();
            try
            {
                Session["SessBookingType"] = "Track";

                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["objdbmlBooking"] = null;

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);

                ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
                ViewBag.BookingType = GetBookingType();
                //ViewBag.BookingStatus = GetBookingStatus();
            }
            catch
            {
            }

            return View(model);
        }

        public ActionResult LabBookingHistory()
        {
            CommonModel model = new CommonModel();
            try
            {
                Session["SessBookingType"] = "Lab";

                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["objdbmlBooking"] = null;

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);

                ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
                ViewBag.BookingType = GetLabBookingType();
                //ViewBag.BookingStatus = GetBookingStatus();
            }
            catch
            {
            }

            return View(model);
        }

        [ValidateAntiForgeryToken]
        public ActionResult GetBookingWorkFlowStatusGetByBPId(int intBPId)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            ObservableCollection<dbmlWorkFlowView> objdbmlWorkFlowView = WorkFlowViewGetByBPId(intBPId, 0);
            intStatusId = 1;
            strStatus = "Success";

            return Json(new { Status = strStatus, StatusId = intStatusId, WorkFlowViewList = objdbmlWorkFlowView }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Track/Lab RFQ
        public ActionResult TrackBookingsAndRFQ()
        {
            CommonModel model = new CommonModel();
            try
            {
                Session["SessBookingType"] = "Track";
                Session["SessHistoryType"] = "RFQ";

                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["objdbmlBooking"] = null;

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.StateId = Convert.ToInt32(Session["StateId"]);
                model.ReportURL = strRptURL;

                //ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
                //ViewBag.BookingType = GetBookingType();
                //ViewBag.BookingStatus = GetBookingStatus();
            }
            catch
            {
            }

            return View(model);
        }

        public ActionResult LabBookingsAndRFQ()
        {
            CommonModel model = new CommonModel();
            try
            {
                Session["SessBookingType"] = "Lab";
                Session["SessHistoryType"] = "RFQ";

                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["objdbmlBooking"] = null;

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.StateId = Convert.ToInt32(Session["StateId"]);
                model.ReportURL = strRptURL;

            }
            catch
            {
            }

            return View(model);
        }

        public ActionResult BookingStandard()
        {
            CommonModel model = new CommonModel();
            try
            {
                Session["SessBookingType"] = "Track";

                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["objdbmlBooking"] = null;

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);

                ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
            }
            catch
            {
            }

            return View(model);
        }

        public ActionResult BookingAgainstRFQ()
        {
            CommonModel model = new CommonModel();
            try
            {
                Session["SessBookingType"] = "Track";

                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["objdbmlBooking"] = null;

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);

                ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
            }
            catch
            {
            }

            return View(model);
        }

        public ActionResult BookingAgainstRFQConf()
        {
            CommonModel model = new CommonModel();
            try
            {
                Session["SessBookingType"] = "Track";

                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["objdbmlBooking"] = null;

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);

                ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
                //ViewBag.BookingType = GetBookingType();
                //ViewBag.BookingStatus = GetBookingStatus();
            }
            catch
            {
            }

            return View(model);
        }

        public ActionResult BookingAgainstContract()
        {
            CommonModel model = new CommonModel();
            try
            {
                Session["SessBookingType"] = "Track";

                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["objdbmlBooking"] = null;

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);

                ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
                //ViewBag.BookingType = GetBookingType();
                //ViewBag.BookingStatus = GetBookingStatus();
            }
            catch
            {
            }

            return View(model);
        }

        [ValidateAntiForgeryToken]
        public ActionResult RFQBookingSearchViewGetByDepartmentBookinStatus(int intDepartmentId, int intBookingTypeId, int intStatusPropId)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlBookingSearchView objreturndbmlBookingSearchView = new returndbmlBookingSearchView();

            try
            {
                objreturndbmlBookingSearchView = objServiceClient.RFQBookingSearchViewFrontGetByCompanyIdFromDateToDate(intDepartmentId, DateTime.Now.Date, DateTime.Now.Date, intBookingTypeId, intStatusPropId);

                if (objreturndbmlBookingSearchView != null && objreturndbmlBookingSearchView.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objreturndbmlBookingSearchView.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, BookingList = objreturndbmlBookingSearchView.objdbmlBookingSearchView }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RFQBookingDetailInsert(int intRFQBookingId, int intRFQBPId, int intBPId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            returndbmlBooking objreturndbmlBooking = new returndbmlBooking();
            try
            {
                objreturndbmlBooking = objServiceClient.RFQBookingDetailInsertByBookingIdBPId(intRFQBookingId, intRFQBPId, intBPId, Convert.ToInt32(Session["UserId"]), Convert.ToInt32(Session["ZZCompanyId"]));

                if (objreturndbmlBooking != null && objreturndbmlBooking.objdbmlStatus.StatusId == 1)
                {
                    Session["objdbmlBooking"] = objreturndbmlBooking.objdbmlBookingList.FirstOrDefault();
                    Session["BPId"] = objreturndbmlBooking.objdbmlBookingList.FirstOrDefault().BPId;
                }
                else
                {
                    return Json(new { Status = objreturndbmlBooking.objdbmlStatus.Status.ToString(), StatusId = -99 }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { Status = ex.Message, StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            return RedirectToAction("Basic", "Home");
        }

        public ActionResult BookingRFQ()
        {
            CommonModel model = new CommonModel();
            try
            {
                Session["SessBookingType"] = "Track";

                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["objdbmlBooking"] = null;

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);

                ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
            }
            catch
            {
            }

            return View(model);
        }

        public ActionResult BookingRFQConf()
        {
            CommonModel model = new CommonModel();
            try
            {
                Session["SessBookingType"] = "Track";

                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["objdbmlBooking"] = null;

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);

                ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
            }
            catch
            {
            }

            return View(model);
        }

        public ActionResult BookingContract()
        {
            CommonModel model = new CommonModel();
            try
            {
                Session["SessBookingType"] = "Track";

                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["objdbmlBooking"] = null;

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);

                ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
            }
            catch
            {
            }

            return View(model);
        }

        public ActionResult LabBookingStandard()
        {
            CommonModel model = new CommonModel();
            try
            {
                Session["SessBookingType"] = "Track";

                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["objdbmlBooking"] = null;

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);

                ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
            }
            catch
            {
            }

            return View(model);
        }

        public ActionResult LabBookingRFQ()
        {
            CommonModel model = new CommonModel();
            try
            {
                Session["SessBookingType"] = "Track";

                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["objdbmlBooking"] = null;

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);

                ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
            }
            catch
            {
            }

            return View(model);
        }

        public ActionResult LabBookingAgainstRFQ()
        {
            CommonModel model = new CommonModel();
            try
            {
                Session["SessBookingType"] = "Track";

                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["objdbmlBooking"] = null;

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);

                ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
            }
            catch
            {
            }

            return View(model);
        }

        [ValidateAntiForgeryToken]
        public ActionResult DashBoardDocumentGetByBPIdWorkFlowIdStatusPropertyId(int intBPId, string strWorkFlowId, string strStatusPropId, int intDashboardId)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlDashBoardDocumentViewFront objreturndbmlDashBoardDocumentViewFront = new returndbmlDashBoardDocumentViewFront();

            try
            {

                objreturndbmlDashBoardDocumentViewFront = objServiceClient.DashBoardDocumentGetByBPIdWorkFlowIdStatusPropertyId(intBPId, strWorkFlowId, strStatusPropId, Convert.ToInt32(Session["UserId"]), intDashboardId);

                if (objreturndbmlDashBoardDocumentViewFront != null && objreturndbmlDashBoardDocumentViewFront.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objreturndbmlDashBoardDocumentViewFront.objdbmlStatus.Status;
                }
            }

            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, DashBoardDocumentList = objreturndbmlDashBoardDocumentViewFront.objdbmlDashBoardDocumentViewFront }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult ToDoBookingSearchViewGetByCompanyIdFromDateToDateFront()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlBookingSearchView objreturndbmlBookingSearchView = new returndbmlBookingSearchView();

            try
            {
                objreturndbmlBookingSearchView = objServiceClient.ToDoBookingSearchViewGetByCompanyIdFromDateToDateFront(Convert.ToInt32(Session["ZZCompanyId"]), Convert.ToInt32(Session["UserId"]), DateTime.Now.Date, DateTime.Now.Date, 0, 0);

                if (objreturndbmlBookingSearchView != null && objreturndbmlBookingSearchView.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objreturndbmlBookingSearchView.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, BookingList = objreturndbmlBookingSearchView.objdbmlBookingSearchView }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Basic
        public ActionResult Basic()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.StateId = Convert.ToInt32(Session["StateId"]);
                model.BPId = Convert.ToInt32(Session["BPId"]);
                model.ReportURL = strRptURL;

                ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));

                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);
                    model.DocDate = objdbmlBooking.ZZBookingDate;
                    model.DocNo = objdbmlBooking.BookingNo;
                    model.DocType = objdbmlBooking.ZZBookingType;
                    model.WorkFlowId = Convert.ToInt32(objdbmlBooking.ZZWorkFlowId);
                    model.WorkFlowStatusId = objdbmlBooking.ZZStatusWorkflowId;
                    model.StatusPropId = Convert.ToInt32(objdbmlBooking.StatusPropId);
                    model.DocId = objdbmlBooking.BookingId;
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), objdbmlBooking.BookingId);
                    model.POURL = strPOURL + objdbmlBooking.PODocPath;
                    model.SupportDocPathURL = strbookingsupportdoc + objdbmlBooking.ZZSupportDocPath;
                    model.RFQId = Convert.ToInt32(objdbmlBooking.RFQId);
                    model.RFQBPId = Convert.ToInt32(objdbmlBooking.ZZRFQBPId);
                    model.BookingId = Convert.ToInt32(objdbmlBooking.BookingId);
                    model.RFQBookingNo = objdbmlBooking.ZZRFQBookingNo;
                    ViewBag.WorkflowRemark = objdbmlBooking.ZZWorkflowRemark;
                    switch (Convert.ToInt32(Session["BPId"]))
                    {
                        case 21:
                            model.DocType = "Track Booking";
                            Session["SessBookingType"] = "Track";
                            break;
                        case 98:
                            model.DocType = "Track RFQ - Confidential";
                            Session["SessBookingType"] = "Track";
                            break;
                        case 46:
                            model.DocType = "Track RFQ - Regular";
                            Session["SessBookingType"] = "Track";
                            break;
                        case 90:
                            model.DocType = "Lab Booking";
                            Session["SessBookingType"] = "Lab";
                            break;
                        case 91:
                            model.DocType = "Lab RFQ - Regular";
                            Session["SessBookingType"] = "Lab";
                            break;
                    }
                }
                else
                {
                    model.DocDate = "To be allotted";
                    model.DocNo = "To be allotted";

                    switch (Convert.ToInt32(Session["BPId"]))
                    {
                        case 21:
                            model.WorkFlowId = Convert.ToInt32(HardCodeValues.BookingWFId);
                            model.DocType = "Track Booking";
                            Session["SessBookingType"] = "Track";
                            break;
                        case 98:
                            model.WorkFlowId = Convert.ToInt32(HardCodeValues.RFQConfWFId);
                            model.DocType = "Track RFQ - Confidential";
                            Session["SessBookingType"] = "Track";
                            break;
                        case 46:
                            model.WorkFlowId = Convert.ToInt32(HardCodeValues.RFQRegWFId);
                            model.DocType = "Track RFQ - Regular";
                            Session["SessBookingType"] = "Track";
                            break;
                        case 90:
                            model.WorkFlowId = Convert.ToInt32(HardCodeValues.LabBookingWFId);
                            model.DocType = "Lab Booking";
                            Session["SessBookingType"] = "Lab";
                            break;
                        case 91:
                            model.WorkFlowId = Convert.ToInt32(HardCodeValues.LabRFQRegWFId);
                            model.DocType = "Lab RFQ - Regular";
                            Session["SessBookingType"] = "Lab";
                            break;
                    }

                    model.StatusPropId = Convert.ToInt32(HardCodeValues.OpenStatusId);
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), 0);
                    model.RFQId = 0;
                    model.RFQBPId = 0;
                    model.RFQBookingNo = "";
                }
            }
            catch
            {
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Basic(CommonModel model, string btnPrevNext)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (btnPrevNext.ToLower() == "next")
                {
                    return RedirectToAction("Vehicle", "Home");
                }
            }
            catch
            {
            }

            return RedirectToAction("Basic", "Home");
        }

        [ValidateAntiForgeryToken]
        public ActionResult LoadBasicInfo()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            ObservableCollection<dbmlBookingView> objdbmlBookingList = new ObservableCollection<dbmlBookingView>();
            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    objdbmlBookingList.Add(objdbmlBooking);

                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, BookingList = objdbmlBookingList }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult BookingSave(dbmlBookingView model)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlBooking objreturndbmlBooking = new returndbmlBooking();
            dbmlGeneral objGeneral = new dbmlGeneral();

            try
            {
                model.BookingDate = DateTime.Now.Date;//objClassUserFunctions.ToDateTimeNotNull(model.ZZBookingDate);
                model.BPId = Convert.ToInt32(Session["BPId"]);
                model.CompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.CreateId = Convert.ToInt32(Session["UserId"]);
                model.CreateDate = DateTime.Now;
                model.UpdateId = Convert.ToInt32(Session["UserId"]);
                model.UpdateDate = DateTime.Now;
                model.StatusPropId = Convert.ToInt32(HardCodeValues.OpenStatusId);
                if (model.BookingId <= 0)
                {
                    model.BookingNo = "Temp";
                }

                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);
                    model.TabStatusId = objdbmlBooking.TabStatusId;
                }
                else
                {
                    model.TabStatusId = 0;
                }


                returndbmlBooking objreturndbmlBookingTemp = new returndbmlBooking();
                ObservableCollection<dbmlBookingView> objdbmlBookingList = new ObservableCollection<dbmlBookingView>();
                objdbmlBookingList.Add(model);
                objreturndbmlBookingTemp.objdbmlBookingList = objdbmlBookingList;

                dbmlGeneral obj = new dbmlGeneral();
                obj.IntOne = 1;
                if (model.BPId == 21 || model.BPId == 90)
                {
                    returndbmlCompanyDepartment objreturndbmlCompanyDepartment = objServiceClient.CompanyDepartmentGetByCustomerMasterId(model.CompanyId);
                    if (objreturndbmlCompanyDepartment != null && objreturndbmlCompanyDepartment.objdbmlStatus.StatusId == 1)
                    {
                        if (objreturndbmlCompanyDepartment.objdbmlCompanyDepartment.Where(i => i.CompanyDepartmentId == model.DepartmentId).Count() > 0)
                        {
                            int AccountId = objreturndbmlCompanyDepartment.objdbmlCompanyDepartment.Where(i => i.CompanyDepartmentId == model.DepartmentId).FirstOrDefault().ZZAccountId;

                            objGeneral.IntOne = AccountId;
                            objGeneral.StrOne = DateTime.Now.ToString();
                            obj = objServiceClient.AccountCreditLimitCheckBalanceGetByAccountIdWEFDate(objGeneral);
                        }
                    }
                }
                if (obj.IntOne == 1)
                {
                    if (model.BookingId <= 0)
                    {
                        objreturndbmlBooking = objServiceClient.BookingInsert(objreturndbmlBookingTemp);
                    }
                    else
                    {
                        objreturndbmlBooking = objServiceClient.BookingUpdate(objreturndbmlBookingTemp);
                    }

                    if (objreturndbmlBooking != null && objreturndbmlBooking.objdbmlStatus.StatusId == 1)
                    {
                        Session["objdbmlBooking"] = objreturndbmlBooking.objdbmlBookingList.FirstOrDefault();
                        intStatusId = 1;
                        strStatus = "Data Saved Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlBooking.objdbmlStatus.Status;
                    }
                }
                else if (obj.IntOne == 2)
                {
                    strStatus = obj.StrOne;
                }

            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, BookingList = objreturndbmlBooking.objdbmlBookingList }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult BookingDelete(int intBookingId)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlStatus objreturndbmlStatus = new returndbmlStatus();

            try
            {
                objreturndbmlStatus = objServiceClient.BookingDeleteAllByBookingId(intBookingId);

                if (objreturndbmlStatus != null && objreturndbmlStatus.objdbmlStatus.StatusId == 1)
                {
                    Session["objdbmlBooking"] = null;
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objreturndbmlStatus.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId }, JsonRequestBehavior.AllowGet);
        }

        public List<SelectListItem> CompanyDepartmentGetByCustomerMasterId(int intCustomerMasterId)
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            try
            {
                returndbmlCompanyDepartment objreturndbmlCompanyDepartment = objServiceClient.CompanyDepartmentGetByCustomerMasterId(intCustomerMasterId);
                if (objreturndbmlCompanyDepartment != null && objreturndbmlCompanyDepartment.objdbmlStatus.StatusId == 1)
                {
                    foreach (var itm in objreturndbmlCompanyDepartment.objdbmlCompanyDepartment)
                    {
                        ViewBag.AccountId = itm.ZZAccountId;
                        if (Convert.ToInt32(Session["UserTypePropId"]) == 167 || Convert.ToInt32(Session["UserTypePropId"]) == 168)
                        {
                            if (itm.CompanyDepartmentId == Convert.ToInt32(Session["CompanyDepartmentId"]))
                                Items.Add(new SelectListItem { Text = itm.Department, Value = itm.CompanyDepartmentId.ToString(), Selected = false });
                        }
                        else
                        {
                            Items.Add(new SelectListItem { Text = itm.Department, Value = itm.CompanyDepartmentId.ToString(), Selected = false });
                        }
                    }
                }
            }
            catch
            {

            }
            return Items;
        }

        public ObservableCollection<dbmlWorkFlowView> WorkFlowViewGetByBPId(int intBPId, int intDocId)
        {
            ObservableCollection<dbmlWorkFlowView> objdbmlWorkFlowView = new ObservableCollection<dbmlWorkFlowView>();
            try
            {
                //if (Session["WorkFlowView"] != null)
                //{
                //    GeneralColl<dbmlWorkFlowView>.CopyCollection(Session["WorkFlowView"] as ObservableCollection<dbmlWorkFlowView>, objdbmlWorkFlowView);
                //}
                //else
                {
                    returndbmlWorkFlowView objreturndbmlWorkFlowView = objServiceClient.WorkFlowViewGetByBPId(intBPId, intDocId);
                    if (objreturndbmlWorkFlowView.objdbmlStatus.StatusId == 1 && objreturndbmlWorkFlowView.objdbmlWorkFlowView.Count > 0)
                    {
                        Session["WorkFlowView"] = objreturndbmlWorkFlowView.objdbmlWorkFlowView;
                        objdbmlWorkFlowView = objreturndbmlWorkFlowView.objdbmlWorkFlowView;
                    }
                }
            }
            catch
            {

            }
            return objdbmlWorkFlowView;
        }

        [ValidateAntiForgeryToken]
        public ActionResult SubmitDoc(int intQuotFlag)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";
            returndbmlBooking objreturndbmlBooking = new returndbmlBooking();
            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);
                    returndbmlStatus objreturndbmlStatus = new returndbmlStatus();
                    objreturndbmlStatus.objdbmlStatus = new dbmlStatus();
                    objreturndbmlStatus.objdbmlStatus.StatusId = 1;
                    if (intQuotFlag == 1)
                    {
                        objreturndbmlStatus = objServiceClient.BookingQuotationPIDetailInsertByBookingId(objdbmlBooking.BookingId);
                    }
                    if (intQuotFlag == 5 && objdbmlBooking.PODocPath == null || objdbmlBooking.PODocPath == string.Empty)
                    {
                        intStatusId = 5;
                        objreturndbmlStatus.objdbmlStatus.Status = "Please upload PO";
                        objreturndbmlStatus.objdbmlStatus.StatusId = 5;
                    }

                    // Check on Submit
                    if (objreturndbmlStatus.objdbmlStatus.StatusId == 1 && intQuotFlag != 5 && objdbmlBooking.BPId == 21)
                    {
                        returndbmlStatus objreturndbmlStatusOnSubmit = new returndbmlStatus();
                        objreturndbmlStatusOnSubmit = objServiceClient.CheckBookingDetailsAndMainSheduleGetByBPIdDocId(objdbmlBooking.BPId, objdbmlBooking.BookingId);
                        if (objreturndbmlStatusOnSubmit != null)
                        {
                            objreturndbmlStatus.objdbmlStatus.StatusId = Convert.ToInt32(objreturndbmlStatusOnSubmit.objdbmlStatus.StatusId);
                            objreturndbmlStatus.objdbmlStatus.Status = objreturndbmlStatusOnSubmit.objdbmlStatus.Status;
                        }
                    }

                    if (objreturndbmlStatus.objdbmlStatus.StatusId == 1)
                    {
                        //if (objdbmlBooking.BPId == 90 && (objdbmlBooking.ZZSupportDocPath == "" || objdbmlBooking.ZZSupportDocPath == null))
                        //{
                        //    strStatus = "Please upload AIS-007 Form. (Lab-Section)";
                        //}
                        //else
                        //{
                        objreturndbmlBooking = objServiceClient.WorkFlowActivityInsert(objdbmlBooking.BookingId, Convert.ToInt32(Session["BPId"]), Convert.ToInt32(objdbmlBooking.ZZWorkFlowId), Convert.ToInt32(HardCodeValues.SubmitStatusId), "", Convert.ToInt32(Session["UserId"]));
                        if (objreturndbmlBooking != null && objreturndbmlBooking.objdbmlStatus.StatusId == 1)
                        {
                            Session["objdbmlBooking"] = objreturndbmlBooking.objdbmlBookingList.FirstOrDefault();
                            intStatusId = 1;
                            strStatus = "Data Saved Successfully";

                            if (intQuotFlag == 5) //SubmitDoc PO
                            {
                                string strTo = string.Empty, strBcc = string.Empty, strSubject = string.Empty, strBody = string.Empty, strAttachDocPath = string.Empty;
                                string[] strCc = strPOUploadedCCEmailNATRAX.Split(';');
                                string strFileName = objdbmlBooking.PODocPath;
                                strSubject = "Booking No.: " + Convert.ToString(objdbmlBooking.BookingNo) + ",Dated: " + objdbmlBooking.ZZBookingDate + "-" + objdbmlBooking.ZZCompany;
                                // strBody = "Dear Customer,";
                                strBody += "<br /><br /> Please find Attached PO Uploaded by " + "<b>" + Convert.ToString(objdbmlBooking.ZZCompany) + "</b>" + " Against Booking No. " + "<b>" + Convert.ToString(objdbmlBooking.BookingNo) + "</b>";
                                strBody += "<br /><br />Regards";
                                strBody += "<br /><span style='font-weight:bold;font-family:Trebuchet MS;font-style:italic'>NATRAX</span>";
                                string strEmailid = string.Empty;
                                strEmailid = strPOUploadedEmailNATRAX + ";" + objdbmlBooking.PMEmailId;
                                if (strEmailid.Length > 1 && strEmailid.Contains(";"))
                                {
                                    SendAttachmentUploadPOMailMessageAsync(strFromEmail, strFromPwd, strEmailid, strBcc, strCc, strSubject, strBody, strFileName);
                                }
                            }

                            //Track Booking =6, LAB Booing=36
                            #region old mail body

                            //if ((objdbmlBooking.ZZWorkFlowId == 6 || objdbmlBooking.ZZWorkFlowId == 36)
                            //    && (objdbmlBooking.BPId == 21 || objdbmlBooking.BPId == 90))
                            //{
                            //    returndbmlBookingSubmitEmailNotification objreturnSubmitEmail = new returndbmlBookingSubmitEmailNotification();
                            //    objreturnSubmitEmail = objServiceClient.BookingSubmitEmailNotificationbyBookingIdBPId(objdbmlBooking.BookingId, objdbmlBooking.BPId);
                            //    if (objreturnSubmitEmail.objdbmlWorkFlowView.Count > 0 && objreturnSubmitEmail.objdbmlStatus.StatusId == 1)
                            //    {
                            //        //  objreturnSubmitEmail.objdbmlWorkFlowView;
                            //        string strLink = "http://pgmsbackoffice.azurewebsites.net/";
                            //        string strTo = string.Empty, strBcc = string.Empty, strSubject = string.Empty, strBody = string.Empty, strAttachDocPath = string.Empty;
                            //        string[] strCc = { };
                            //        string strFileName = objdbmlBooking.PODocPath;
                            //        strSubject = "New Booking request Booking No.: " + Convert.ToString(objdbmlBooking.BookingNo) + ",Dated: " + objdbmlBooking.ZZBookingDate + "-" + objdbmlBooking.ZZCompany;
                            //        strBody = "Hello Sir,";

                            //        strBody += "<br /><br /> New Booking request has been received with below services. Company Name : " + "<b>" + Convert.ToString(objdbmlBooking.ZZCompany) + "</b>" + ", Department : " + "<b>" + objreturnSubmitEmail.objdbmlWorkFlowView.FirstOrDefault().Department + "</b>" + " Against Booking No.: " + "<b>" + Convert.ToString(objdbmlBooking.BookingNo) + "</b>";
                            //        strBody += "<br /><br />";
                            //        var gridData = "<table id='tblCallWrap' style='width: 100%;verticle-align:top; padding: 0px; margin: 0px;' cellspacing='0px'>";
                            //        gridData = gridData + "<tr>";
                            //        gridData = gridData + "<td style='width: 30px; text-align: left; border-top: solid 1px lightgray; border-left: solid 1px lightgray; font-weight: bold; background-color: #c3c3c4;font-family: Arial;font-size: 12px;font-weight: bold;border-right: solid 1px lightgray;'>Sr. No.</td>";
                            //        gridData = gridData + "<td style='width: 250px; text-align: left; border-top: solid 1px lightgray; border-left: solid 1px lightgray; font-weight: bold; background-color: #c3c3c4;font-family: Arial;font-size: 12px;font-weight: bold;border-right: solid 1px lightgray;'>Booked Services</td>";
                            //        gridData = gridData + "<td style='width: 30px; text-align: left; border-top: solid 1px lightgray; border-left: solid 1px lightgray; font-weight: bold; background-color: #c3c3c4;font-family: Arial;font-size: 12px;font-weight: bold;border-right: solid 1px lightgray;'>Date</td>";
                            //        gridData = gridData + "<td style='width: 100px; text-align: left; border-top: solid 1px lightgray; border-left: solid 1px lightgray; font-weight: bold; background-color: #c3c3c4;font-family: Arial;font-size: 12px;font-weight: bold;border-right: solid 1px lightgray;'>Vehicle No.</td>";
                            //        gridData = gridData + "<td style='width: 30px; text-align: left; border-top: solid 1px lightgray; border-left: solid 1px lightgray; font-weight: bold; background-color: #c3c3c4;font-family: Arial;font-size: 12px;font-weight: bold;border-right: solid 1px lightgray;'>Unit</td>";
                            //        gridData = gridData + "<td style='width: 50px; text-align: right; border-top: solid 1px lightgray; border-left: solid 1px lightgray; font-weight: bold; background-color: #c3c3c4;font-family: Arial;font-size: 12px;font-weight: bold;border-right: solid 1px lightgray;'>Estimate Usage Qty.</td>";
                            //        gridData = gridData + "<td style='width: 50px; text-align: right; border-top: solid 1px lightgray; border-left: solid 1px lightgray; font-weight: bold; background-color: #c3c3c4;font-family: Arial;font-size: 12px;font-weight: bold;border-right: solid 1px lightgray;'>Billing Qty.</td>";
                            //        gridData = gridData + "</tr>";

                            //        foreach (var item in objreturnSubmitEmail.objdbmlWorkFlowView)
                            //        {
                            //            gridData = gridData + "<tr>";
                            //            gridData = gridData + "<td style='width: 30Px; text-align: left; border-top: solid 1px lightgray; border-bottom: solid 1px lightgray; border-left: solid 1px lightgray; font-weight: bold; border-right: solid 1px lightgray;'>" + item.Srno + "</td>";
                            //            gridData = gridData + "<td style='width: 250px; text-align:left; border-top: solid 1px lightgray; border-bottom: solid 1px lightgray; border-left: solid 1px lightgray; font-weight: bold; border-right: solid 1px lightgray;'>" + item.ServiceDescription + "</td>";
                            //            gridData = gridData + "<td style='width: 30px; text-align: left; border-top: solid 1px lightgray; border-bottom: solid 1px lightgray; border-left: solid 1px lightgray; font-weight: bold; border-right: solid 1px lightgray;'>" + item.Date + "</td>";
                            //            gridData = gridData + "<td style='width: 100px; text-align: left; border-top: solid 1px lightgray; border-bottom: solid 1px lightgray; border-left: solid 1px lightgray; font-weight: bold; border-right: solid 1px lightgray;'>" + item.VehicleNo + "</td>";
                            //            gridData = gridData + "<td style='width: 30px; text-align: left; border-top: solid 1px lightgray; border-bottom: solid 1px lightgray; border-left: solid 1px lightgray; font-weight: bold;border-right: solid 1px lightgray;'>" + item.UOM + "</td>";
                            //            gridData = gridData + "<td style='width: 50px; text-align: right; border-top: solid 1px lightgray; border-bottom: solid 1px lightgray; border-left: solid 1px lightgray; font-weight: bold;border-right: solid 1px lightgray;'>" + item.RoundOffHrs + "</td>";
                            //            gridData = gridData + "<td style='width: 50px; text-align: right; border-top: solid 1px lightgray; border-bottom: solid 1px lightgray; border-left: solid 1px lightgray; font-weight: bold;border-right: solid 1px lightgray;'>" + item.BillingHrs + "</td>";
                            //            gridData = gridData + "</tr>";
                            //        }

                            //        gridData = gridData + "</table>";
                            //        strBody += "<br /><br />" + gridData + " ";

                            //        strBody += "<br /><br /> Please login to <a href='" + strLink + "'>PGMS</a> for further process.";
                            //        strBody += "<br /><br />Regards";
                            //        strBody += "<br /><span style='font-weight:bold;font-family:Trebuchet MS;font-style:italic'>NATRAX</span>";
                            //        string strEmailid = string.Empty;
                            //        if (objdbmlBooking.BPId == 21)
                            //        {
                            //            strEmailid = strTrackNewBookingSubmitMailNotification;
                            //        }
                            //        else if (objdbmlBooking.BPId == 90)
                            //        {
                            //            strEmailid = strLabNewBookingSubmitMailNotification;
                            //        }
                            //        if (strEmailid.Length > 1 && strEmailid.Contains(";"))
                            //        {
                            //            SendMailMessageAsync(strFromEmail, strFromPwd, strEmailid, strBcc, strCc, strSubject, strBody, strFileName);
                            //        }
                            //    }


                            //}

                            #endregion

                            if ((objdbmlBooking.ZZWorkFlowId == 6 || objdbmlBooking.ZZWorkFlowId == 36)
                               && (objdbmlBooking.BPId == 21 || objdbmlBooking.BPId == 90))
                            {
                                returndbmlBookingSubmitEmailNotification objreturnSubmitEmail = new returndbmlBookingSubmitEmailNotification();
                                objreturnSubmitEmail = objServiceClient.BookingSubmitEmailNotificationNewbyBookingIdBPId(objdbmlBooking.BookingId, objdbmlBooking.BPId);
                                if (objreturnSubmitEmail.objdbmlWorkFlowView.Count > 0 && objreturnSubmitEmail.objdbmlStatus.StatusId == 1)
                                {
                                    string strLink = "http://pgmsbackoffice1.azurewebsites.net/";
                                    string strTo = string.Empty, strBcc = string.Empty, strSubject = string.Empty, strBody = string.Empty, strAttachDocPath = string.Empty;
                                    string[] strCc = { };
                                    string strFileName = objdbmlBooking.PODocPath;

                                    strSubject = "New one Booking request Booking No.: " + Convert.ToString(objdbmlBooking.BookingNo) + ",Dated: " + objdbmlBooking.ZZBookingDate + "-" + objdbmlBooking.ZZCompany;

                                    var gridData = "<table align='center' cellpadding='0' cellspacing='0' width='100%'>";
                                    gridData = gridData + "<tbody>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td style='background-color:#ffffff;border:solid 1px #ccc'>";
                                    gridData = gridData + "<table border='0' cellpadding='0' cellspacing='0' width='100%'>";
                                    gridData = gridData + "<tbody>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left'>";
                                    gridData = gridData + "<table border='0' cellpadding='0' cellspacing='0' width='100%'>";
                                    gridData = gridData + "<tbody>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td colspan='3' align='left' bgcolor='#f9ec00' height='10' style='width:74%!important;height:10px!important' width='74'>&nbsp;</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "</tbody>";
                                    gridData = gridData + "</table>";
                                    gridData = gridData + "</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left'>";
                                    gridData = gridData + "<table border='0' cellpadding='0' cellspacing='0' width='100%'>";
                                    gridData = gridData + "<tbody>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left' bgcolor='#FFFFFF' style='width:10px' valign='top' width='10'>&nbsp;</td>";
                                    gridData = gridData + "<td align='center' valign='top'>";
                                    gridData = gridData + "<table border='0' cellpadding='0' cellspacing='0' width='100%'>";
                                    gridData = gridData + "<tbody>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left' bgcolor='#FFFFFF' style='width:10px' valign='top' width='10'>&nbsp;</td>";
                                    gridData = gridData + "<td align='center' valign='top'>";
                                    gridData = gridData + "<table border='0' cellpadding='0' cellspacing='0' width='100%'>";
                                    gridData = gridData + "<tbody>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left' height='10' style='height:10px' valign='top'>&nbsp;</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left' valign='top'>";
                                    gridData = gridData + "<table border='0' cellpadding='0' cellspacing='0' width='100%'>";
                                    gridData = gridData + "<tbody>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left' style='line-height:0px!important' valign='middle' width='112'>";
                                    gridData = gridData + "<img border='0' style='margin:0;border:0;padding:0;height:auto!important;' src='https://www.natrax.in/wp-content/uploads/2020/01/natrax-lg-1-1-1-1.png' class='CToWUd'>";
                                    gridData = gridData + "</td>";
                                    gridData = gridData + "<td align='right' valign='middle' style='font-size: 20px; font-weight: 800; font-family:Calibri;'>";
                                    gridData = gridData + "National Automotive Test Tracks";
                                    gridData = gridData + "</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "</tbody>";
                                    gridData = gridData + "</table>";
                                    gridData = gridData + "</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left' height='10' style='height:10px' valign='top'>&nbsp;</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "</tbody>";
                                    gridData = gridData + "</table>";
                                    gridData = gridData + "</td>";
                                    gridData = gridData + "<td align='right' bgcolor='#FFFFFF' style='width:10px' valign='top' width='10'>&nbsp;</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "</tbody>";
                                    gridData = gridData + "</table>";
                                    gridData = gridData + "</td>";
                                    gridData = gridData + "<td align='right' bgcolor='#FFFFFF' style='width:10px' valign='top' width='10'>&nbsp;</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "</tbody>";
                                    gridData = gridData + "</table>";
                                    gridData = gridData + "</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left' valign='top'>";
                                    gridData = gridData + "<table align='center' border='0' cellpadding='0' cellspacing='0' width='100%'>";
                                    gridData = gridData + "<tbody>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td width='10'>&nbsp;</td>";
                                    gridData = gridData + "<td>";
                                    gridData = gridData + "<table align='center' border='0' cellpadding='0' cellspacing='0' width='100%'>";
                                    gridData = gridData + "<tbody>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td bgcolor='#E6E6E6' width='10'>&nbsp;</td>";
                                    gridData = gridData + "<td bgcolor='#E6E6E6' height='10'>&nbsp;</td>";
                                    gridData = gridData + "<td bgcolor='#E6E6E6' width='10'>&nbsp;</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td bgcolor='#E6E6E6' width='10'>&nbsp;</td>";
                                    gridData = gridData + "<td>";
                                    gridData = gridData + "<table align='center' border='0' cellpadding='0' cellspacing='0' width='100%'>";
                                    gridData = gridData + "<tbody>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td width='10'>&nbsp;</td>";
                                    gridData = gridData + "<td height='10'>&nbsp;</td>";
                                    gridData = gridData + "<td width='10'>&nbsp;</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td width='10'>&nbsp;</td>";
                                    gridData = gridData + "<td>";
                                    gridData = gridData + "<table border='0' cellpadding='0' cellspacing='0' width='100%'>";
                                    gridData = gridData + "<tbody>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td bgcolor='#008640' style='font-family:Calibri;font-size:16px;color:#fff;text-align:left;padding:5px 10px 5px 10px;line-height:normal;border-radius:4px;text-align:center'>Booking Details</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left' height='15' valign='top'>&nbsp;</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left' style='font-family:Calibri;font-size:16px;color:#333;text-align:left;padding:0px;line-height:normal' valign='top'>Hello Sir,</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left' height='10' valign='top'>&nbsp;</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left' style='font-family:Calibri;font-size:16px;color:#333;text-align:left;padding:0px;line-height:normal' valign='top'>";
                                    gridData = gridData + "New Booking request has been received with below services.";
                                    gridData = gridData + "</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left' style='font-family:Calibri;font-size:16px;color:#333;text-align:left;padding:0px;line-height:normal' valign='top'>";
                                    gridData = gridData + "Company Name : <b>" + objreturnSubmitEmail.objdbmlWorkFlowView.FirstOrDefault().CompanyName + " </b>, Department : <b>" + objreturnSubmitEmail.objdbmlWorkFlowView.FirstOrDefault().Department + " </b>, Created By: <b>" + objreturnSubmitEmail.objdbmlWorkFlowView.FirstOrDefault().CreatedBy + " </b>, Against Booking No.: <b>" + objreturnSubmitEmail.objdbmlWorkFlowView.FirstOrDefault().BookingNo + " </b>";
                                    gridData = gridData + "</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left' height='10' valign='top'>&nbsp;</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td style='font-family:Calibri;font-size:16px;color:#333;padding:0px;line-height:normal' valign='top'>";
                                    // Logic Start
                                    gridData = gridData + "<table border='1' style='font-family:Calibri !important; width: 100%;' cellpadding='0' cellspacing='0'>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<th style='background: #EEE; text-align: center;'>Sr. No.</th>";
                                    gridData = gridData + "<th style='background: #EEE; text-align: center;'>Vehicle No.</th>";
                                    gridData = gridData + "<th style='background: #EEE; text-align: center;'>From Date</th>";
                                    gridData = gridData + "<th style='background: #EEE; text-align: center;'>To Date</th>";
                                    gridData = gridData + "<th style='background: #EEE; text-align: left;'>Booked Services</th>";
                                    gridData = gridData + "<th style='background: #EEE; text-align: left;'>Not Use in Date</th>";

                                    var distinctVehiclelst =
                                  objreturnSubmitEmail.objdbmlWorkFlowView.GroupBy(s => new { s.VehicleNo, s.Srno })
                                  .Select(g =>
                                   new
                                   {
                                       VehicleNo = g.Key.VehicleNo,
                                       Srno = g.Key.Srno
                                   });

                                    foreach (var Vitem in distinctVehiclelst)
                                    {
                                        gridData = gridData + "<tr>";
                                        int RowSpanCnt = objreturnSubmitEmail.objdbmlWorkFlowView.Where(mi => mi.VehicleNo == Vitem.VehicleNo).ToList().Count() + 1;
                                        gridData = gridData + "<tr>";
                                        gridData = gridData + "<td rowspan='" + RowSpanCnt + "' style='text-align: center'>" + Vitem.Srno + "</td>";
                                        gridData = gridData + "<td rowspan='" + RowSpanCnt + "' style='text-align: center'>" + Vitem.VehicleNo + "</td>";
                                        gridData = gridData + "<td rowspan='" + RowSpanCnt + "' style='text-align: center'>" + objreturnSubmitEmail.objdbmlWorkFlowView.Where(i => i.VehicleNo == Vitem.VehicleNo).FirstOrDefault().FromDate + "</td>";
                                        gridData = gridData + "<td rowspan='" + RowSpanCnt + "' style='text-align: center'>" + objreturnSubmitEmail.objdbmlWorkFlowView.Where(i => i.VehicleNo == Vitem.VehicleNo).FirstOrDefault().ToDate + "</td>";
                                        gridData = gridData + "</tr>";
                                        foreach (var item in objreturnSubmitEmail.objdbmlWorkFlowView.Where(i => i.VehicleNo == Vitem.VehicleNo))
                                        {
                                            gridData = gridData + "<tr>";
                                            gridData = gridData + "<td style='text-align: left;'>" + item.ServiceDescription + "</td>";
                                            gridData = gridData + "<td style='text-align: left;'>" + item.NotUsedInDate + "</td>";
                                            gridData = gridData + "</tr>";
                                        }
                                        gridData = gridData + "<tr>";
                                    }
                                    gridData = gridData + "</table>";

                                    gridData = gridData + "</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left' height='15' valign='top'>&nbsp;</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left' style='font-family:Calibri;font-size:16px;color:#333;text-align:justify;padding:0px;line-height:normal; font-weight: 700;' valign='top'>Thanks &amp; Regards</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left' style='font-family:Calibri;font-size:16px;color:#333;text-align:justify;padding:0px;line-height:normal; font-weight: 700;' valign='top'>NATRAX</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "</tbody>";
                                    gridData = gridData + "</table>";
                                    gridData = gridData + "</td>";
                                    gridData = gridData + "<td width='10'>&nbsp;</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td width='10'>&nbsp;</td>";
                                    gridData = gridData + "<td height='10'>&nbsp;</td>";
                                    gridData = gridData + "<td width='10'>&nbsp;</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "</tbody>";
                                    gridData = gridData + "</table>";
                                    gridData = gridData + "</td>";
                                    gridData = gridData + "<td bgcolor='#E6E6E6' width='10'>&nbsp;</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td bgcolor='#E6E6E6' width='10'>&nbsp;</td>";
                                    gridData = gridData + "<td bgcolor='#E6E6E6' height='10'>&nbsp;</td>";
                                    gridData = gridData + "<td bgcolor='#E6E6E6' width='10'>&nbsp;</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "</tbody>";
                                    gridData = gridData + "</table";
                                    gridData = gridData + "</td>";
                                    gridData = gridData + "<td width='10'>&nbsp;</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "</tbody>";
                                    gridData = gridData + "</table>";
                                    gridData = gridData + "</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td align='left'>";
                                    gridData = gridData + "<table border='0' cellpadding='0' cellspacing='0' width='100%'>";
                                    gridData = gridData + "<tbody>";
                                    gridData = gridData + "<tr>";
                                    gridData = gridData + "<td colspan='3' align='left' bgcolor='#f9ec00' height='10' style='width:74%!important;height:10px!important' width='74'>&nbsp;</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "</tbody>";
                                    gridData = gridData + "</table>";
                                    gridData = gridData + "</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "</tbody>";
                                    gridData = gridData + "</table>";
                                    gridData = gridData + "</td>";
                                    gridData = gridData + "</tr>";
                                    gridData = gridData + "</tbody>";
                                    gridData = gridData + "</table>";

                                    strBody = gridData;
                                    string strEmailid = string.Empty;
                                    if (objdbmlBooking.BPId == 21)
                                    {
                                        strEmailid = strTrackNewBookingSubmitMailNotification;
                                    }
                                    else if (objdbmlBooking.BPId == 90)
                                    {
                                        strEmailid = strLabNewBookingSubmitMailNotification;
                                    }
                                    if (strEmailid.Length > 1 && strEmailid.Contains(";"))
                                    {
                                        SendMailMessageAsync(strFromEmail, strFromPwd, strEmailid, strBcc, strCc, strSubject, strBody, strFileName);
                                    }
                                }
                            }

                        }
                        else
                        {
                            strStatus = objreturndbmlBooking.objdbmlStatus.Status;
                        }
                        //}
                    }
                    else
                    {
                        strStatus = objreturndbmlStatus.objdbmlStatus.Status;
                    }
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, BookingList = objreturndbmlBooking.objdbmlBookingList }, JsonRequestBehavior.AllowGet);
        }

        public bool IsValidEmail(string email)
        {
            Regex rx = new Regex(
            @"^[-!#$%&'*+/0-9=?A-Z^_a-z{|}~](\.?[-!#$%&'*+/0-9=?A-Z^_a-z{|}~])*@[a-zA-Z](-?[a-zA-Z0-9])*(\.[a-zA-Z](-?[a-zA-Z0-9])*)+$");
            return rx.IsMatch(email);
        }

        public async Task SendAttachmentUploadPOMailMessageAsync(string strFrom, string strPSW, string strTo, string strBcc,
                        string[] strCc, string strSubject, string strBody, string strFileName)
        {
            try
            {
                await Task.Run(() =>
                {
                    MailMessage mMailMessage = new MailMessage();
                    mMailMessage.From = new MailAddress(strFrom);
                    //mMailMessage.To.Add(new MailAddress(strTo));
                    if (strTo.Contains(";"))
                    {
                        foreach (var address in strTo.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            mMailMessage.To.Add(new MailAddress(address));
                        }
                    }
                    else
                    {
                        mMailMessage.To.Add(new MailAddress(strTo));
                    }
                    if ((strBcc != null) && (strBcc != string.Empty))
                    {
                        if (strBcc != null && strBcc.Trim().Length > 4 && IsValidEmail(strBcc) == true)
                        {
                            mMailMessage.Bcc.Add(new MailAddress(strBcc));
                        }
                    }

                    if ((strCc != null) && (strCc.Length > 0))
                    {
                        foreach (string strEmail in strCc)
                        {
                            if (strEmail != null && strEmail.Trim().Length > 4 && IsValidEmail(strEmail) == true)
                            {
                                mMailMessage.CC.Add(new MailAddress(strEmail));
                            }
                        }
                    }
                    mMailMessage.Subject = strSubject;
                    mMailMessage.Body = strBody;
                    mMailMessage.IsBodyHtml = true;
                    mMailMessage.Priority = MailPriority.Normal;

                    Byte[] bytes;

                    string strAccountName = System.Configuration.ConfigurationManager.AppSettings["strBlobAccount"];
                    string strKey = System.Configuration.ConfigurationManager.AppSettings["strAccountKey"];
                    string strContainerName = "booking";//System.Configuration.ConfigurationManager.AppSettings["strPOUploadContainer"];

                    StorageCredentials sc = new StorageCredentials(strAccountName, strKey);

                    CloudStorageAccount storageAccount = new CloudStorageAccount(sc, true);

                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                    CloudBlobContainer container = blobClient.GetContainerReference(strContainerName);

                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(strFileName);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        blockBlob.DownloadToStream(ms);
                        bytes = ms.ToArray();
                    }

                    if (blockBlob.Exists())
                    {
                        Attachment attachedDoc = new Attachment(new MemoryStream(bytes), strFileName);
                        mMailMessage.Attachments.Add(attachedDoc);
                        SmtpClient mSmtpClient = new SmtpClient();
                        mSmtpClient.EnableSsl = true;
                        mSmtpClient.Credentials = new NetworkCredential(strFrom, strPSW);
                        mSmtpClient.Host = "smtp.gmail.com";
                        mSmtpClient.Port = 587;
                        mSmtpClient.Send(mMailMessage);
                    }
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task SendMailMessageAsync(string strFrom, string strPSW, string strTo, string strBcc,
                       string[] strCc, string strSubject, string strBody, string strFileName)
        {
            try
            {
                await Task.Run(() =>
                {
                    MailMessage mMailMessage = new MailMessage();
                    mMailMessage.From = new MailAddress(strFrom);
                    //mMailMessage.To.Add(new MailAddress(strTo));
                    if (strTo.Contains(";"))
                    {
                        foreach (var address in strTo.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            mMailMessage.To.Add(new MailAddress(address));
                        }
                    }
                    else
                    {
                        mMailMessage.To.Add(new MailAddress(strTo));
                    }
                    if ((strBcc != null) && (strBcc != string.Empty))
                    {
                        if (strBcc != null && strBcc.Trim().Length > 4 && IsValidEmail(strBcc) == true)
                        {
                            mMailMessage.Bcc.Add(new MailAddress(strBcc));
                        }
                    }

                    if ((strCc != null) && (strCc.Length > 0))
                    {
                        foreach (string strEmail in strCc)
                        {
                            if (strEmail != null && strEmail.Trim().Length > 4 && IsValidEmail(strEmail) == true)
                            {
                                mMailMessage.CC.Add(new MailAddress(strEmail));
                            }
                        }
                    }
                    mMailMessage.Subject = strSubject;
                    mMailMessage.Body = strBody;
                    mMailMessage.IsBodyHtml = true;
                    mMailMessage.Priority = MailPriority.Normal;

                    //Attachment attachedDoc = new Attachment(new MemoryStream(bytes), strFileName);
                    // mMailMessage.Attachments.Add(attachedDoc);
                    SmtpClient mSmtpClient = new SmtpClient();
                    mSmtpClient.EnableSsl = true;
                    mSmtpClient.Credentials = new NetworkCredential(strFrom, strPSW);
                    mSmtpClient.Host = "smtp.gmail.com";
                    mSmtpClient.Port = 587;
                    mSmtpClient.Send(mMailMessage);
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [ValidateAntiForgeryToken]
        public ActionResult BookingQuotationPI()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlBooking objreturndbmlBooking = new returndbmlBooking();

            try
            {

                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    returndbmlStatus objreturndbmlStatus = objServiceClient.BookingQuotationPIDetailInsertByBookingId(objdbmlBooking.BookingId);
                    if (objreturndbmlStatus.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Success";
                    }
                    else
                    {
                        strStatus = objreturndbmlStatus.objdbmlStatus.Status;
                    }
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult POUpload()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }
            returndbmlBooking objreturndbmlBooking = new returndbmlBooking();
            int intStatusId = 99;
            string strStatus = "Invalid";
            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    HttpPostedFileBase file = Request.Files["ImageData"];
                    if (file.InputStream.Length > 0)
                    {
                        string strFileExtention = file.ContentType;
                        if (strFileExtention.ToLower() == "image/jpeg" || strFileExtention.ToLower() == "application/pdf")
                        {
                            if ((file.ContentLength / 1024) > 0 && (file.ContentLength / 1024) <= 2048)
                            {
                                strFileExtention = strFileExtention.Substring(strFileExtention.IndexOf('/') + 1);
                                byte[] byteImage = objClassUserFunctions.ConvertToBytes(file.InputStream);
                                bool blnStatus = false;
                                string strFTPUserName = System.Configuration.ConfigurationManager.AppSettings["strFTPUserName"];
                                string strFTPUserPSW = System.Configuration.ConfigurationManager.AppSettings["strFTPUserPassword"];
                                string strFTPUrl = System.Configuration.ConfigurationManager.AppSettings["strFTPServer"];
                                string strFTPRoot = System.Configuration.ConfigurationManager.AppSettings["strFTPRoot"];

                                string strBlobAccount = System.Configuration.ConfigurationManager.AppSettings["strBlobAccount"];
                                string strAccountKey = System.Configuration.ConfigurationManager.AppSettings["strAccountKey"];
                                string strFileStorage = System.Configuration.ConfigurationManager.AppSettings["strFileStorage"];

                                string strImageName = "PO_" + Convert.ToString(objdbmlBooking.BookingId) + "." + strFileExtention;
                                string strImageContainerName = "booking";
                                string strImageURL = "";
                                string strFTPFilePath = "";
                                if (strFileStorage == "FTP")
                                {
                                    ////////////////////// For FTP /////////////////////////////////////////////                       
                                    strFTPFilePath = strFTPRoot + strImageName;

                                    blnStatus = objClassUserFunctions.UploadImageToFTPFromWEBCLIENT(strFTPUrl, strFTPUserName, strFTPUserPSW, byteImage, strFTPFilePath);
                                }
                                else if (strFileStorage == "AzureBlob")
                                {
                                    /////////////////////// For Azure Blob ///////////////////////////////////////////////////                            
                                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11;
                                    strImageURL = objClassUserFunctions.UploadFileStreamToAzureBlob(strBlobAccount, strAccountKey, strImageContainerName, strImageName, byteImage);
                                    if (strImageURL != "")
                                        blnStatus = true;
                                }
                                if (blnStatus)
                                {
                                    objdbmlBooking.PODocPath = strImageName;
                                    objdbmlBooking.UpdateId = Convert.ToInt32(Session["UserId"]);
                                    objdbmlBooking.UpdateDate = DateTime.Now;

                                    returndbmlBooking objreturndbmlBookingTemp = new returndbmlBooking();
                                    ObservableCollection<dbmlBookingView> objdbmlBookingViewList = new ObservableCollection<dbmlBookingView>();
                                    objdbmlBookingViewList.Add(objdbmlBooking);
                                    objreturndbmlBookingTemp.objdbmlBookingList = objdbmlBookingViewList;

                                    objreturndbmlBooking = objServiceClient.BookingUpdate(objreturndbmlBookingTemp);
                                    if (objreturndbmlBooking != null && objreturndbmlBooking.objdbmlStatus.StatusId == 1)
                                    {
                                        Session["objdbmlBooking"] = objreturndbmlBooking.objdbmlBookingList.FirstOrDefault();
                                        intStatusId = 1;
                                        strStatus = "PO Uploaded Successfully";
                                    }
                                    else
                                    {
                                        if (strFileStorage == "FTP")
                                        {
                                            bool delStatus = objClassUserFunctions.DeleteFileFromFTPFromWEBCLIENT(strFTPUrl, strFTPUserName, strFTPUserPSW, strFTPFilePath);
                                        }
                                        else if (strFileStorage == "AzureBlob")
                                        {
                                            bool delStatus = objClassUserFunctions.DeleteFileFromAzureBlob(strBlobAccount, strAccountKey, strImageContainerName, strImageName);
                                        }

                                        strStatus = "PO Uploading Process Failed!";
                                        //strStatus = objreturndbmlBooking.objdbmlStatus.Status;
                                    }
                                }
                                else
                                {
                                    strStatus = "PO Uploading Process Failed!";
                                }
                            }
                            else
                            {
                                strStatus = "PO File Size between 1KB to 2 MB can be accepted!";
                            }
                        }
                        else
                        {
                            strStatus = "Only (JPEG, PDF) format can be accepted!";
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }

            return Json(new { StatusId = intStatusId, Status = strStatus }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult LoadServiceDates()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlServiceDateViewFront objreturndbmlServiceDateViewFront = new returndbmlServiceDateViewFront();
            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    objreturndbmlServiceDateViewFront = objServiceClient.ServiceDateViewFrontGetByBookingId(objdbmlBooking.BookingId);
                    if (objreturndbmlServiceDateViewFront.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Success";
                    }
                    else
                    {
                        strStatus = objreturndbmlServiceDateViewFront.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, ServiceDateList = objreturndbmlServiceDateViewFront.objdbmlServiceDateViewFront }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult UpdateServiceDates(ObservableCollection<dbmlServiceDateViewFront> model)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlServiceDateViewFront objreturndbmlServiceDateViewFront = new returndbmlServiceDateViewFront();
            objreturndbmlServiceDateViewFront.objdbmlServiceDateViewFront = model;
            try
            {
                if (Session["objdbmlBooking"] != null && model != null && model.Count > 0)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    returndbmlBooking objreturndbmlBooking = objServiceClient.UpdateServiceDateFrontByBookingIdDayDates(objreturndbmlServiceDateViewFront);

                    if (objreturndbmlBooking != null && objreturndbmlBooking.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Service Dates Updated Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlBooking.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Vehicle

        public ActionResult Vehicle()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.StatusPropId = 0;
                model.StateId = Convert.ToInt32(Session["StateId"]);
                ViewBag.VehicleType = GetVehicleType();

                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);
                    model.DocDate = objdbmlBooking.ZZBookingDate;
                    model.DocNo = objdbmlBooking.BookingNo;
                    model.DocType = objdbmlBooking.ZZBookingType;
                    model.WorkFlowId = Convert.ToInt32(objdbmlBooking.ZZWorkFlowId);
                    model.WorkFlowStatusId = objdbmlBooking.ZZStatusWorkflowId;
                    model.StatusPropId = Convert.ToInt32(objdbmlBooking.StatusPropId);
                    model.BPId = Convert.ToInt32(Session["BPId"]);
                    model.ReportURL = strRptURL;
                    model.DocId = objdbmlBooking.BookingId;
                    model.POURL = strPOURL + objdbmlBooking.PODocPath;

                    //if (Convert.ToInt32(objdbmlBooking.TabStatusId) + 10 < 10)
                    //{
                    //    return RedirectToActionByStatusId(Convert.ToInt32(objdbmlBooking.TabStatusId));
                    //}
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), objdbmlBooking.BookingId);
                    model.RFQId = Convert.ToInt32(objdbmlBooking.RFQId);
                    model.RFQBPId = Convert.ToInt32(objdbmlBooking.ZZRFQBPId);
                    model.RFQBookingNo = objdbmlBooking.ZZRFQBookingNo;
                }
                else
                {
                    return RedirectToAction("Basic", "Home");
                }
            }
            catch
            {
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Vehicle(CommonModel model, string btnPrevNext)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (btnPrevNext.ToLower() == "prev")
                {
                    return RedirectToAction("Basic", "Home");
                }

                if (btnPrevNext.ToLower() == "next")
                {
                    if (Convert.ToInt32(Session["BPId"]) == 90 || Convert.ToInt32(Session["BPId"]) == 91)
                    {
                        return RedirectToAction("Component", "Home");
                    }
                    else
                    {
                        return RedirectToAction("MainTrackBookingNew", "Home");
                    }
                }
            }
            catch
            {
            }

            return RedirectToAction("Vehicle", "Home");
        }

        [ValidateAntiForgeryToken]
        public ActionResult LoadVehicleInfo()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlListOfVehicleComponent objreturndbmlVehicle = new returndbmlListOfVehicleComponent();
            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    objreturndbmlVehicle = objServiceClient.ListOfVehicleComponentGetByDocId(objdbmlBooking.BookingId);
                    if (objreturndbmlVehicle.objdbmlStatus.StatusId == 1)
                    {
                        objreturndbmlVehicle.objdbmlListOfVehicleComponent = new ObservableCollection<dbmlListOfVehicleComponent>(objreturndbmlVehicle.objdbmlListOfVehicleComponent.Where(itm => itm.GroupId == Convert.ToInt32(HardCodeValues.VehicleGrpPropId)));
                        intStatusId = 1;
                        strStatus = "Success";
                    }
                    else
                    {
                        strStatus = objreturndbmlVehicle.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, VehicleList = objreturndbmlVehicle.objdbmlListOfVehicleComponent }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult VehicleSaveMultiple(ObservableCollection<dbmlListOfVehicleComponent> model)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlListOfVehicleComponent objreturndbmlVehicle = new returndbmlListOfVehicleComponent();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    returndbmlListOfVehicleComponent objreturndbmlVehicleTemp = new returndbmlListOfVehicleComponent();
                    ObservableCollection<dbmlListOfVehicleComponent> objdbmlVehicle = new ObservableCollection<dbmlListOfVehicleComponent>();
                    foreach (var itm in model)
                    {
                        itm.DocId = objdbmlBooking.BookingId;
                        itm.BPId = Convert.ToInt32(Session["BPId"]);
                        itm.GroupId = Convert.ToInt32(HardCodeValues.VehicleGrpPropId);
                        itm.GroupName = "Vehicle";
                        itm.CreateId = Convert.ToInt32(Session["UserId"]);
                        itm.CreateDate = DateTime.Now;
                        itm.UpdateId = Convert.ToInt32(Session["UserId"]);
                        itm.UpdateDate = DateTime.Now;

                        //returndbmlListOfVehicleComponent objreturndbmlVehicleTemp = new returndbmlListOfVehicleComponent();
                        //ObservableCollection<dbmlListOfVehicleComponent> objdbmlVehicle = new ObservableCollection<dbmlListOfVehicleComponent>();
                        //objdbmlVehicle.Add(model);
                        //objreturndbmlVehicleTemp.objdbmlListOfVehicleComponent = objdbmlVehicle;
                    }

                    objreturndbmlVehicleTemp.objdbmlListOfVehicleComponent = model;

                    objreturndbmlVehicle = objServiceClient.ListOfVehicleComponentInsert(objreturndbmlVehicleTemp);

                    if (objreturndbmlVehicle != null && objreturndbmlVehicle.objdbmlStatus.StatusId == 1)
                    {
                        objreturndbmlVehicle.objdbmlListOfVehicleComponent = new ObservableCollection<dbmlListOfVehicleComponent>(objreturndbmlVehicle.objdbmlListOfVehicleComponent.Where(itm => itm.GroupId == Convert.ToInt32(HardCodeValues.VehicleGrpPropId)));
                        intStatusId = 1;
                        strStatus = "Vehicle Saved Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlVehicle.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, VehicleList = objreturndbmlVehicle.objdbmlListOfVehicleComponent }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult VehicleSave(dbmlListOfVehicleComponent model)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlListOfVehicleComponent objreturndbmlVehicle = new returndbmlListOfVehicleComponent();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    model.DocId = objdbmlBooking.BookingId;
                    model.BPId = Convert.ToInt32(Session["BPId"]);
                    model.GroupId = Convert.ToInt32(HardCodeValues.VehicleGrpPropId);
                    model.GroupName = "Vehicle";
                    model.CreateId = Convert.ToInt32(Session["UserId"]);
                    model.CreateDate = DateTime.Now;
                    model.UpdateId = Convert.ToInt32(Session["UserId"]);
                    model.UpdateDate = DateTime.Now;

                    returndbmlListOfVehicleComponent objreturndbmlVehicleTemp = new returndbmlListOfVehicleComponent();
                    ObservableCollection<dbmlListOfVehicleComponent> objdbmlVehicle = new ObservableCollection<dbmlListOfVehicleComponent>();
                    objdbmlVehicle.Add(model);
                    objreturndbmlVehicleTemp.objdbmlListOfVehicleComponent = objdbmlVehicle;

                    if (model.VehCompId < 0)
                    {
                        objreturndbmlVehicle = objServiceClient.ListOfVehicleComponentInsert(objreturndbmlVehicleTemp);
                    }
                    else
                    {
                        objreturndbmlVehicle = objServiceClient.ListOfVehicleComponentUpdate(objreturndbmlVehicleTemp);
                    }

                    if (objreturndbmlVehicle != null && objreturndbmlVehicle.objdbmlStatus.StatusId == 1)
                    {
                        objreturndbmlVehicle.objdbmlListOfVehicleComponent = new ObservableCollection<dbmlListOfVehicleComponent>(objreturndbmlVehicle.objdbmlListOfVehicleComponent.Where(itm => itm.GroupId == Convert.ToInt32(HardCodeValues.VehicleGrpPropId)));
                        intStatusId = 1;
                        strStatus = "Vehicle Saved Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlVehicle.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, VehicleList = objreturndbmlVehicle.objdbmlListOfVehicleComponent }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult VehicleDelete(int intVehCompId)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlListOfVehicleComponent objreturndbmlVehicle = new returndbmlListOfVehicleComponent();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    objreturndbmlVehicle = objServiceClient.ListOfVehicleComponentDeleteByDocIdCompId(objdbmlBooking.BookingId, intVehCompId);

                    if (objreturndbmlVehicle != null && objreturndbmlVehicle.objdbmlStatus.StatusId == 1)
                    {
                        objreturndbmlVehicle.objdbmlListOfVehicleComponent = new ObservableCollection<dbmlListOfVehicleComponent>(objreturndbmlVehicle.objdbmlListOfVehicleComponent.Where(itm => itm.GroupId == Convert.ToInt32(HardCodeValues.VehicleGrpPropId)));
                        intStatusId = 1;
                        strStatus = "Vehicle Deleted Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlVehicle.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, VehicleList = objreturndbmlVehicle.objdbmlListOfVehicleComponent }, JsonRequestBehavior.AllowGet);
        }

        public List<SelectListItem> GetVehicleType()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            try
            {
                ObservableCollection<dbmlProperty> objProp = new ObservableCollection<dbmlProperty>();
                if (Session["Properties"] != null)
                {
                    GeneralColl<dbmlProperty>.CopyCollection(Session["Properties"] as ObservableCollection<dbmlProperty>, objProp);
                }
                else
                {
                    returndbmlProperty objreturndbmlProperty = objServiceClient.PropertiesGetAll();
                    if (objreturndbmlProperty.objdbmlStatus.StatusId == 1 && objreturndbmlProperty.objdbmlProperty.Count > 0)
                    {
                        Session["Properties"] = objreturndbmlProperty.objdbmlProperty;
                        objProp = objreturndbmlProperty.objdbmlProperty;
                    }
                }

                if (objProp != null && objProp.Count > 0)
                {
                    ObservableCollection<dbmlProperty> objPropList = new ObservableCollection<dbmlProperty>(objProp.Where(itm => itm.PropertyTypeId == Convert.ToInt32(HardCodeValues.VehicleTypePropId)));
                    foreach (var itm in objPropList)
                    {
                        Items.Add(new SelectListItem { Text = itm.Property, Value = itm.PropertyId.ToString(), Selected = false });
                    }
                }
            }
            catch
            {

            }
            return Items;
        }
        #endregion

        #region Component

        public ActionResult Component()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.StatusPropId = 0;
                model.StateId = Convert.ToInt32(Session["StateId"]);
                ViewBag.VehicleType = GetComponentType();

                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);
                    model.DocDate = objdbmlBooking.ZZBookingDate;
                    model.DocNo = objdbmlBooking.BookingNo;
                    model.DocType = objdbmlBooking.ZZBookingType;
                    model.WorkFlowId = Convert.ToInt32(objdbmlBooking.ZZWorkFlowId);
                    model.WorkFlowStatusId = objdbmlBooking.ZZStatusWorkflowId;
                    model.StatusPropId = Convert.ToInt32(objdbmlBooking.StatusPropId);
                    model.BPId = Convert.ToInt32(Session["BPId"]);
                    model.ReportURL = strRptURL;
                    model.DocId = objdbmlBooking.BookingId;
                    model.POURL = strPOURL + objdbmlBooking.PODocPath;

                    //if (Convert.ToInt32(objdbmlBooking.TabStatusId) + 10 < 10)
                    //{
                    //    return RedirectToActionByStatusId(Convert.ToInt32(objdbmlBooking.TabStatusId));
                    //}
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), objdbmlBooking.BookingId);
                    model.RFQId = Convert.ToInt32(objdbmlBooking.RFQId);
                    model.RFQBPId = Convert.ToInt32(objdbmlBooking.ZZRFQBPId);
                    model.RFQBookingNo = objdbmlBooking.ZZRFQBookingNo;
                }
                else
                {
                    return RedirectToAction("Basic", "Home");
                }
            }
            catch
            {
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Component(CommonModel model, string btnPrevNext)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (btnPrevNext.ToLower() == "prev")
                {
                    return RedirectToAction("Vehicle", "Home");
                }

                if (btnPrevNext.ToLower() == "next")
                {
                    return RedirectToAction("MainLabBooking", "Home");
                }
            }
            catch
            {
            }

            return RedirectToAction("Vehicle", "Home");
        }

        [ValidateAntiForgeryToken]
        public ActionResult LoadComponentInfo()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlListOfVehicleComponent objreturndbmlVehicle = new returndbmlListOfVehicleComponent();
            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    objreturndbmlVehicle = objServiceClient.ListOfVehicleComponentGetByDocId(objdbmlBooking.BookingId);
                    if (objreturndbmlVehicle.objdbmlStatus.StatusId == 1)
                    {
                        objreturndbmlVehicle.objdbmlListOfVehicleComponent = new ObservableCollection<dbmlListOfVehicleComponent>(objreturndbmlVehicle.objdbmlListOfVehicleComponent.Where(itm => itm.GroupId == Convert.ToInt32(HardCodeValues.CompGrpPropId)));
                        intStatusId = 1;
                        strStatus = "Success";
                    }
                    else
                    {
                        strStatus = objreturndbmlVehicle.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, VehicleList = objreturndbmlVehicle.objdbmlListOfVehicleComponent }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult ComponentSave(dbmlListOfVehicleComponent model)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlListOfVehicleComponent objreturndbmlVehicle = new returndbmlListOfVehicleComponent();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    model.DocId = objdbmlBooking.BookingId;
                    model.BPId = Convert.ToInt32(Session["BPId"]);
                    model.GroupId = Convert.ToInt32(HardCodeValues.CompGrpPropId);
                    model.GroupName = "Component";
                    model.CreateId = Convert.ToInt32(Session["UserId"]);
                    model.CreateDate = DateTime.Now;
                    model.UpdateId = Convert.ToInt32(Session["UserId"]);
                    model.UpdateDate = DateTime.Now;

                    returndbmlListOfVehicleComponent objreturndbmlVehicleTemp = new returndbmlListOfVehicleComponent();
                    ObservableCollection<dbmlListOfVehicleComponent> objdbmlVehicle = new ObservableCollection<dbmlListOfVehicleComponent>();
                    objdbmlVehicle.Add(model);
                    objreturndbmlVehicleTemp.objdbmlListOfVehicleComponent = objdbmlVehicle;

                    if (model.VehCompId < 0)
                    {
                        objreturndbmlVehicle = objServiceClient.ListOfVehicleComponentInsert(objreturndbmlVehicleTemp);
                    }
                    else
                    {
                        objreturndbmlVehicle = objServiceClient.ListOfVehicleComponentUpdate(objreturndbmlVehicleTemp);
                    }

                    if (objreturndbmlVehicle != null && objreturndbmlVehicle.objdbmlStatus.StatusId == 1)
                    {
                        objreturndbmlVehicle.objdbmlListOfVehicleComponent = new ObservableCollection<dbmlListOfVehicleComponent>(objreturndbmlVehicle.objdbmlListOfVehicleComponent.Where(itm => itm.GroupId == Convert.ToInt32(HardCodeValues.CompGrpPropId)));
                        intStatusId = 1;
                        strStatus = "Data Saved Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlVehicle.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, VehicleList = objreturndbmlVehicle.objdbmlListOfVehicleComponent }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult ComponentDelete(int intVehCompId)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlListOfVehicleComponent objreturndbmlVehicle = new returndbmlListOfVehicleComponent();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    objreturndbmlVehicle = objServiceClient.ListOfVehicleComponentDeleteByDocIdCompId(objdbmlBooking.BookingId, intVehCompId);

                    if (objreturndbmlVehicle != null && objreturndbmlVehicle.objdbmlStatus.StatusId == 1)
                    {
                        objreturndbmlVehicle.objdbmlListOfVehicleComponent = new ObservableCollection<dbmlListOfVehicleComponent>(objreturndbmlVehicle.objdbmlListOfVehicleComponent.Where(itm => itm.GroupId == Convert.ToInt32(HardCodeValues.CompGrpPropId)));
                        intStatusId = 1;
                        strStatus = "Component Deleted Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlVehicle.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, VehicleList = objreturndbmlVehicle.objdbmlListOfVehicleComponent }, JsonRequestBehavior.AllowGet);
        }

        public List<SelectListItem> GetComponentType()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            try
            {
                returndbmlOptionList objreturndbmlOptionList = objServiceClient.OptionListGetByPropertyId(Convert.ToInt32(HardCodeValues.CompOptPropId));
                if (objreturndbmlOptionList.objdbmlStatus.StatusId == 1 && objreturndbmlOptionList.objdbmlOptionList.Count > 0)
                {
                    foreach (var itm in objreturndbmlOptionList.objdbmlOptionList)
                    {
                        Items.Add(new SelectListItem { Text = itm.OptionName, Value = itm.OptionListId.ToString(), Selected = false });
                    }
                }
            }
            catch
            {

            }
            return Items;
        }
        #endregion

        #region Driver
        public ActionResult Driver()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.StateId = Convert.ToInt32(Session["StateId"]);

                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);
                    model.DocDate = objdbmlBooking.ZZBookingDate;
                    model.DocNo = objdbmlBooking.BookingNo;
                    model.DocType = objdbmlBooking.ZZBookingType;
                    model.WorkFlowId = Convert.ToInt32(objdbmlBooking.ZZWorkFlowId);
                    model.WorkFlowStatusId = objdbmlBooking.ZZStatusWorkflowId;
                    model.StatusPropId = Convert.ToInt32(objdbmlBooking.StatusPropId);
                    model.DocId = objdbmlBooking.BookingId;

                    //if (Convert.ToInt32(objdbmlBooking.TabStatusId) + 10 < 20)
                    //{
                    //    return RedirectToActionByStatusId(Convert.ToInt32(objdbmlBooking.TabStatusId));
                    //}
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), objdbmlBooking.BookingId);
                }
                else
                {
                    return RedirectToAction("Basic", "Home");
                }
            }
            catch
            {
            }

            return View(model);
        }
        #endregion

        #region Attendee
        public ActionResult Attendee()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.StateId = Convert.ToInt32(Session["StateId"]);

                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);
                    model.DocDate = objdbmlBooking.ZZBookingDate;
                    model.DocNo = objdbmlBooking.BookingNo;
                    model.DocType = objdbmlBooking.ZZBookingType;
                    model.WorkFlowId = Convert.ToInt32(objdbmlBooking.ZZWorkFlowId);
                    model.WorkFlowStatusId = objdbmlBooking.ZZStatusWorkflowId;
                    model.StatusPropId = Convert.ToInt32(objdbmlBooking.StatusPropId);
                    model.DocId = objdbmlBooking.BookingId;

                    //if (Convert.ToInt32(objdbmlBooking.TabStatusId) + 10 < 30)
                    //{
                    //    return RedirectToActionByStatusId(Convert.ToInt32(objdbmlBooking.TabStatusId));
                    //}
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), objdbmlBooking.BookingId);
                }
                else
                {
                    return RedirectToAction("Basic", "Home");
                }
            }
            catch
            {
            }

            return View(model);
        }
        #endregion

        #region Tracks
        public ActionResult MainTrackBookingNew()
        {
            CommonModel model = new CommonModel();
            string strStatus = "";
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.TrackGroup = "Track Booking";
                model.ViewTitle = "Track Booking";
                model.StateId = Convert.ToInt32(Session["StateId"]);

                try
                {
                    returndbmlServicesView objreturndbmlServicesView = objServiceClient.ServicesGetByBPId(Convert.ToInt32(HardCodeValues.ServiceBPIdTrack));
                    if (objreturndbmlServicesView.objdbmlStatus.StatusId == 1 && objreturndbmlServicesView.objdbmlServicesView.Count > 0)
                    {
                        Session["Services"] = objreturndbmlServicesView.objdbmlServicesView;
                    }
                }
                catch (Exception ex)
                {

                }

                //Session["TrackGroupId"] = model.TrackGroupId;
                //Session["TrackGroup"] = model.TrackGroup;
                ViewBag.ServiveLookup = GetServiveLookup(Convert.ToInt32(HardCodeValues.ServiceBPIdTrack));
                if (Convert.ToInt32(Session["BPId"]) == 98)
                {
                    ViewBag.TimeSlot = GetTimeSlotForConfidential();
                }
                else
                {
                    ViewBag.TimeSlot = GetTimeSlot();
                }

                ViewBag.ServiveCategory = GetServiveCategory(model.TrackGroupId);

                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);
                    model.DocDate = objdbmlBooking.ZZBookingDate;
                    model.DocNo = objdbmlBooking.BookingNo;
                    model.DocType = objdbmlBooking.ZZBookingType;
                    model.WorkFlowId = Convert.ToInt32(objdbmlBooking.ZZWorkFlowId);
                    model.WorkFlowStatusId = objdbmlBooking.ZZStatusWorkflowId;
                    model.StatusPropId = Convert.ToInt32(objdbmlBooking.StatusPropId);
                    model.BPId = Convert.ToInt32(Session["BPId"]);
                    model.ReportURL = strRptURL;
                    model.DocId = objdbmlBooking.BookingId;
                    model.POURL = strPOURL + objdbmlBooking.PODocPath;

                    //if (Convert.ToInt32(objdbmlBooking.TabStatusId) + 10 < 40)
                    //{
                    //    return RedirectToActionByStatusId(Convert.ToInt32(objdbmlBooking.TabStatusId));
                    //}
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), objdbmlBooking.BookingId);
                    model.RFQId = Convert.ToInt32(objdbmlBooking.RFQId);
                    model.RFQBPId = Convert.ToInt32(objdbmlBooking.ZZRFQBPId);
                    model.RFQBookingNo = objdbmlBooking.ZZRFQBookingNo;
                }
                else
                {
                    return RedirectToAction("Basic", "Home");
                }

            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }

            return View("MainTrackBookingNew", model);
        }
        public ActionResult MainTrackBooking()
        {
            CommonModel model = new CommonModel();
            string strStatus = "";
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.TrackGroup = "Track Booking";
                model.ViewTitle = "Track Booking";
                model.StateId = Convert.ToInt32(Session["StateId"]);

                returndbmlServicesView objreturndbmlServicesView = objServiceClient.ServicesGetByBPId(Convert.ToInt32(HardCodeValues.ServiceBPIdTrack));
                if (objreturndbmlServicesView.objdbmlStatus.StatusId == 1 && objreturndbmlServicesView.objdbmlServicesView.Count > 0)
                {
                    Session["Services"] = objreturndbmlServicesView.objdbmlServicesView;
                }

                //Session["TrackGroupId"] = model.TrackGroupId;
                //Session["TrackGroup"] = model.TrackGroup;
                ViewBag.ServiveLookup = GetServiveLookup(Convert.ToInt32(HardCodeValues.ServiceBPIdTrack));
                ViewBag.TimeSlot = GetTimeSlot();
                ViewBag.ServiveCategory = GetServiveCategory(model.TrackGroupId);

                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);
                    model.DocDate = objdbmlBooking.ZZBookingDate;
                    model.DocNo = objdbmlBooking.BookingNo;
                    model.DocType = objdbmlBooking.ZZBookingType;
                    model.WorkFlowId = Convert.ToInt32(objdbmlBooking.ZZWorkFlowId);
                    model.WorkFlowStatusId = objdbmlBooking.ZZStatusWorkflowId;
                    model.StatusPropId = Convert.ToInt32(objdbmlBooking.StatusPropId);
                    model.BPId = Convert.ToInt32(Session["BPId"]);
                    model.ReportURL = strRptURL;
                    model.DocId = objdbmlBooking.BookingId;
                    model.POURL = strPOURL + objdbmlBooking.PODocPath;

                    //if (Convert.ToInt32(objdbmlBooking.TabStatusId) + 10 < 40)
                    //{
                    //    return RedirectToActionByStatusId(Convert.ToInt32(objdbmlBooking.TabStatusId));
                    //}
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), objdbmlBooking.BookingId);
                    model.RFQId = Convert.ToInt32(objdbmlBooking.RFQId);
                    model.RFQBPId = Convert.ToInt32(objdbmlBooking.ZZRFQBPId);
                    model.RFQBookingNo = objdbmlBooking.ZZRFQBookingNo;
                }
                else
                {
                    return RedirectToAction("Basic", "Home");
                }

            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }

            return View("MainTrackBooking", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MainTrackBooking(CommonModel model, string btnPrevNext)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (btnPrevNext.ToLower() == "prev")
                {
                    return RedirectToAction("Vehicle", "Home");
                }
                else if (btnPrevNext.ToLower() == "next")
                {
                    return RedirectToAction("TrackWorkshopBooking", "Home");
                }
            }
            catch
            {
            }

            return RedirectToAction("MainTrackBooking", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MainTrackBookingNew(CommonModel model, string btnPrevNext)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (btnPrevNext.ToLower() == "prev")
                {
                    return RedirectToAction("Vehicle", "Home");
                }
                else if (btnPrevNext.ToLower() == "next")
                {
                    return RedirectToAction("TrackWorkshopBooking", "Home");
                }
            }
            catch
            {
            }

            return RedirectToAction("MainTrackBooking", "Home");
        }

        [ValidateAntiForgeryToken]
        public ActionResult LoadTrackInfo(int intTrackGroupId)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";
            //int intTrackGroupId = Convert.ToInt32(Session["TrackGroupId"]);

            List<SelectListItem> lstCategory = GetServiveCategory(intTrackGroupId);

            ObservableCollection<dbmlServicesView> objdbmlServicesView = new ObservableCollection<dbmlServicesView>();
            if (Session["Services"] != null)
            {
                GeneralColl<dbmlServicesView>.CopyCollection(Session["Services"] as ObservableCollection<dbmlServicesView>, objdbmlServicesView);
            }
            if (intTrackGroupId > 0)
            {
                objdbmlServicesView = new ObservableCollection<dbmlServicesView>(objdbmlServicesView.Where(itm => itm.TrackGroupId == intTrackGroupId).OrderBy(itm => itm.SrNo));
            }
            else
            {
                objdbmlServicesView = new ObservableCollection<dbmlServicesView>(objdbmlServicesView.Where(itm => itm.TrackGroupId > 0).OrderBy(itm => itm.SrNo));
            }

            returndbmlTrackBookingDetail objreturndbmlTrackBookingDetail = new returndbmlTrackBookingDetail();
            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);


                    objreturndbmlTrackBookingDetail = objServiceClient.TrackBookingDetailGetByBookingIdTrackGroupId(objdbmlBooking.BookingId, intTrackGroupId);
                    if (objreturndbmlTrackBookingDetail.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Success";
                    }
                    else
                    {
                        strStatus = objreturndbmlTrackBookingDetail.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, ServiceCategoryList = lstCategory, ServicesList = objdbmlServicesView, TrackBookingDetailList = objreturndbmlTrackBookingDetail.objdbmlTrackBookingDetail, TrackBookingTimeDetailList = objreturndbmlTrackBookingDetail.objdbmlTrackBookingTimeDetail, TrackBookingTimeSummaryList = objreturndbmlTrackBookingDetail.objdbmlTrackBookingTimeSummary }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult TrackBookingDetailSave(ObservableCollection<dbmlTrackBookingTimeDetail> model)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";
            //int intTrackGroupId = Convert.ToInt32(Session["TrackGroupId"]);

            returndbmlTrackBookingDetail objreturndbmlTrackBookingDetail = new returndbmlTrackBookingDetail();
            returndbmlTrackBookingDetail objreturndbmlTrackBookingDetailTemp = new returndbmlTrackBookingDetail();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);
                    ObservableCollection<dbmlServicesView> objdbmlServicesView = new ObservableCollection<dbmlServicesView>();
                    if (Session["Services"] != null)
                    {
                        GeneralColl<dbmlServicesView>.CopyCollection(Session["Services"] as ObservableCollection<dbmlServicesView>, objdbmlServicesView);
                    }

                    objdbmlServicesView = new ObservableCollection<dbmlServicesView>(objdbmlServicesView.Where(itm => itm.ServiceId == model.FirstOrDefault().ServiceId));

                    if (Convert.ToString(objdbmlServicesView.FirstOrDefault().GroupforMinBillingHours).ToUpper() == "YES")
                    {
                        int intMinBillHrs = Convert.ToInt32(model.FirstOrDefault().BillingHrs);
                        int intGroupRoundHrs = 0;
                        int intItmCount = 0;
                        int intUsageHrs = -1;
                        int intUsageMin = -1;
                        int intFlag = 0;

                        foreach (var itm in model)
                        {
                            itm.BookingId = objdbmlBooking.BookingId;
                            itm.BookingDetailId = 0;
                            itm.BPId = Convert.ToInt32(HardCodeValues.ServiceBPIdTrack);
                            //if (objdbmlBooking.BPId == 46 || objdbmlBooking.BPId == 91)
                            //{
                            //    itm.Date = objdbmlBooking.BookingDate.AddDays(Convert.ToInt32(itm.BookingDay) - 1);
                            //}
                            //else
                            //{
                            itm.Date = objClassUserFunctions.ToDateTimeNotNull(itm.ZZDate);
                            //}

                            itm.CreateId = Convert.ToInt32(Session["UserId"]);
                            itm.CreateDate = DateTime.Now;
                            itm.UpdateId = Convert.ToInt32(Session["UserId"]);
                            itm.UpdateDate = DateTime.Now;

                            int intRoundOffHrs = Convert.ToInt32(itm.TotalHours);
                            int intRoundOffMin = Convert.ToInt32(itm.TotalMinutes);

                            if ((intRoundOffHrs > 0 && intRoundOffMin >= 30) || (intRoundOffHrs == 0 && intRoundOffMin >= 1))
                            {
                                intRoundOffHrs = intRoundOffHrs + 1;
                                intRoundOffMin = 0;
                            }

                            itm.RoundOffHrs = intRoundOffHrs;
                            itm.RoundOffMin = intRoundOffMin;
                            itm.BillingHrs = intRoundOffHrs;
                            intGroupRoundHrs += intRoundOffHrs;

                            itm.TotalHours = Convert.ToInt32(itm.TotalHours);
                            itm.TotalMinutes = Convert.ToInt32(itm.TotalMinutes);

                            if (intUsageHrs == Convert.ToInt32(itm.TotalHours) && intUsageMin == Convert.ToInt32(itm.TotalMinutes))
                            {
                                intFlag = 1;
                            }
                            else
                            {
                                intFlag = 0;
                            }
                            intUsageHrs = Convert.ToInt32(itm.TotalHours);
                            intUsageMin = Convert.ToInt32(itm.TotalMinutes);

                            intItmCount++;
                        }

                        if (intGroupRoundHrs < intMinBillHrs)
                        {
                            if (intItmCount == 2 && intFlag == 1 && (intMinBillHrs - intGroupRoundHrs) == 2)
                            {
                                model[0].BillingHrs += 1;
                                model[1].BillingHrs += 1;
                            }
                            else
                            {
                                int intServiceId = Convert.ToInt32(model.OrderByDescending(itm => Convert.ToInt32(itm.TotalHours)).ThenByDescending(itm => Convert.ToInt32(itm.TotalMinutes)).ThenBy(itm => Convert.ToDecimal(itm.Rate)).ThenBy(itm => Convert.ToInt32(itm.SrNo)).First().ServiceId);
                                model.FirstOrDefault(itm => itm.ServiceId == intServiceId).BillingHrs += (intMinBillHrs - intGroupRoundHrs);
                            }
                        }
                    }
                    else
                    {
                        // int i = 0;
                        foreach (var itm in model)
                        {
                            itm.BookingId = objdbmlBooking.BookingId;
                            itm.BookingDetailId = 0;
                            itm.BPId = Convert.ToInt32(HardCodeValues.ServiceBPIdTrack);
                            //if (objdbmlBooking.BPId == 46 || objdbmlBooking.BPId == 91)
                            //{
                            //    itm.Date = objdbmlBooking.BookingDate.AddDays(Convert.ToInt32(itm.BookingDay) - 1);
                            //}
                            //else
                            //{
                            itm.Date = objClassUserFunctions.ToDateTimeNotNull(itm.ZZDate);
                            //}
                            itm.CreateId = Convert.ToInt32(Session["UserId"]);
                            itm.CreateDate = DateTime.Now;
                            itm.UpdateId = Convert.ToInt32(Session["UserId"]);
                            itm.UpdateDate = DateTime.Now;

                            int intRoundOffHrs = Convert.ToInt32(itm.TotalHours);
                            int intRoundOffMin = Convert.ToInt32(itm.TotalMinutes);

                            if ((intRoundOffHrs > 0 && intRoundOffMin >= 30) || (intRoundOffHrs == 0 && intRoundOffMin >= 1))
                            {
                                intRoundOffHrs = intRoundOffHrs + 1;
                                intRoundOffMin = 0;
                            }

                            itm.RoundOffHrs = intRoundOffHrs;
                            itm.RoundOffMin = intRoundOffMin;

                            if (intRoundOffHrs > Convert.ToInt32(itm.BillingHrs))
                            {
                                //if (Convert.ToInt32(Session["BPId"]) == 98)
                                //{
                                //    if (i == 0)
                                //    {
                                //        itm.BillingHrs = intRoundOffHrs;
                                //    }
                                //    else
                                //    {
                                //        itm.BillingHrs = 0;
                                //    }
                                //}
                                //else
                                //{
                                //    itm.BillingHrs = intRoundOffHrs;
                                //}
                                itm.BillingHrs = intRoundOffHrs;
                            }
                            // int++
                        }
                    }

                    objreturndbmlTrackBookingDetailTemp.objdbmlTrackBookingTimeDetail = model;


                    objreturndbmlTrackBookingDetail = objServiceClient.TrackBookingDetailInsertFront(objreturndbmlTrackBookingDetailTemp);
                    if (objreturndbmlTrackBookingDetail.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Data Saved Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlTrackBookingDetail.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, TrackBookingDetailList = objreturndbmlTrackBookingDetail.objdbmlTrackBookingDetail, TrackBookingTimeDetailList = objreturndbmlTrackBookingDetail.objdbmlTrackBookingTimeDetail, TrackBookingTimeSummaryList = objreturndbmlTrackBookingDetail.objdbmlTrackBookingTimeSummary }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult TrackBookingDetailInsertFrontRepeatRow(dbmlGeneral model)
        {

            //string strBookingId, string strTrackBookingTimeDetailId, string strFromDate, string strToDate
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";
            //int intTrackGroupId = Convert.ToInt32(Session["TrackGroupId"]);
            model.dtFromDate = Convert.ToDateTime(DateTime.ParseExact(model.StrOne, "dd-MM-yyyy", CultureInfo.InvariantCulture)).Add(DateTime.Now.TimeOfDay);
            model.dtToDate = Convert.ToDateTime(DateTime.ParseExact(model.StrTwo, "dd-MM-yyyy", CultureInfo.InvariantCulture)).Add(DateTime.Now.TimeOfDay);

            returndbmlTrackBookingDetail objreturndbmlTrackBookingDetail = new returndbmlTrackBookingDetail();
            returndbmlTrackBookingDetail objreturndbmlTrackBookingDetailTemp = new returndbmlTrackBookingDetail();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    objreturndbmlTrackBookingDetail = objServiceClient.TrackBookingDetailInsertFrontRepeatRow(model);
                    if (objreturndbmlTrackBookingDetail.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Data Saved Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlTrackBookingDetail.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, TrackBookingDetailList = objreturndbmlTrackBookingDetail.objdbmlTrackBookingDetail, TrackBookingTimeDetailList = objreturndbmlTrackBookingDetail.objdbmlTrackBookingTimeDetail, TrackBookingTimeSummaryList = objreturndbmlTrackBookingDetail.objdbmlTrackBookingTimeSummary }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult BookingStatusGetByServiceIdTimeSlotPropIdWEFDate(int[] intlstServiceId, int intTimeSlotId, string strWED)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlBookingStatusTimeSlotView objreturndbmlBookingStatusTimeSlotView = new returndbmlBookingStatusTimeSlotView();

            try
            {
                dbmlBookingView objdbmlBooking = new dbmlBookingView();
                GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                DateTime dtWED = DateTime.Now;
                //if (objdbmlBooking.BPId == 46 || objdbmlBooking.BPId == 91)
                //{
                //    dtWED = objdbmlBooking.BookingDate.AddDays(Convert.ToInt32(strWED) - 1);
                //}
                //else
                //{
                dtWED = objClassUserFunctions.ToDateTimeNotNull(strWED);
                //}

                objreturndbmlBookingStatusTimeSlotView = objServiceClient.BookingStatusGetByServiceIdTimeSlotPropIdWEFDate(intlstServiceId, intTimeSlotId, dtWED);

                if (objreturndbmlBookingStatusTimeSlotView != null && objreturndbmlBookingStatusTimeSlotView.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objreturndbmlBookingStatusTimeSlotView.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, BookingStatusList = objreturndbmlBookingStatusTimeSlotView.objdbmlBookingStatusTimeSlotView }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult TrackBookingDetailDelete(int intVehicleId, string strDate, int intServiceId, int intTimeSlotId, int intTrackGroupId)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlTrackBookingDetail objreturndbmlTrackBookingDetail = new returndbmlTrackBookingDetail();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    //int intTrackGroupId = Convert.ToInt32(Session["TrackGroupId"]);
                    DateTime dtDate = objClassUserFunctions.ToDateTimeNotNull(strDate);

                    objreturndbmlTrackBookingDetail = objServiceClient.TrackBookingTimeDetailDeleteFrontByServiceId(objdbmlBooking.BookingId, intTrackGroupId, intVehicleId, dtDate, intServiceId, intTimeSlotId);
                    if (objreturndbmlTrackBookingDetail.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Data Deleted Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlTrackBookingDetail.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, TrackBookingDetailList = objreturndbmlTrackBookingDetail.objdbmlTrackBookingDetail, TrackBookingTimeDetailList = objreturndbmlTrackBookingDetail.objdbmlTrackBookingTimeDetail, TrackBookingTimeSummaryList = objreturndbmlTrackBookingDetail.objdbmlTrackBookingTimeSummary }, JsonRequestBehavior.AllowGet);
        }

        public List<SelectListItem> GetTimeSlot()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            try
            {
                ObservableCollection<dbmlProperty> objProp = new ObservableCollection<dbmlProperty>();
                if (Session["Properties"] != null)
                {
                    GeneralColl<dbmlProperty>.CopyCollection(Session["Properties"] as ObservableCollection<dbmlProperty>, objProp);
                }
                else
                {
                    returndbmlProperty objreturndbmlProperty = objServiceClient.PropertiesGetAll();
                    if (objreturndbmlProperty.objdbmlStatus.StatusId == 1 && objreturndbmlProperty.objdbmlProperty.Count > 0)
                    {
                        Session["Properties"] = objreturndbmlProperty.objdbmlProperty;
                        objProp = objreturndbmlProperty.objdbmlProperty;
                    }
                }

                if (objProp != null && objProp.Count > 0)
                {
                    ObservableCollection<dbmlProperty> objPropList = new ObservableCollection<dbmlProperty>(objProp.Where(itm => itm.PropertyTypeId == Convert.ToInt32(HardCodeValues.TimeSlotPropId)).OrderBy(O => O.Sequence));
                    foreach (var itm in objPropList)
                    {
                        Items.Add(new SelectListItem { Text = itm.Property, Value = itm.PropertyId.ToString(), Selected = false });
                    }
                }
            }
            catch
            {

            }
            return Items;
        }

        public List<SelectListItem> GetTimeSlotForConfidential()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            try
            {
                ObservableCollection<dbmlProperty> objProp = new ObservableCollection<dbmlProperty>();
                if (Session["Properties"] != null)
                {
                    GeneralColl<dbmlProperty>.CopyCollection(Session["Properties"] as ObservableCollection<dbmlProperty>, objProp);
                }
                else
                {
                    returndbmlProperty objreturndbmlProperty = objServiceClient.PropertiesGetAll();
                    if (objreturndbmlProperty.objdbmlStatus.StatusId == 1 && objreturndbmlProperty.objdbmlProperty.Count > 0)
                    {
                        Session["Properties"] = objreturndbmlProperty.objdbmlProperty;
                        objProp = objreturndbmlProperty.objdbmlProperty;
                    }
                }

                if (objProp != null && objProp.Count > 0)
                {
                    ObservableCollection<dbmlProperty> objPropList = new ObservableCollection<dbmlProperty>(objProp.Where(itm => itm.PropertyTypeId == Convert.ToInt32(HardCodeValues.TimeSlotPropId)).OrderBy(O => O.Sequence));

                    foreach (var itm in objPropList)
                    {
                        Items.Add(new SelectListItem { Text = itm.Property + " (" + itm.Description + ") ", Value = itm.PropertyId.ToString(), Selected = false });
                    }
                }
            }
            catch
            {

            }
            return Items;
        }

        public List<SelectListItem> GetServiveCategory(int intTrackGroupId)
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            try
            {
                ObservableCollection<dbmlServicesView> objdbmlServicesView = new ObservableCollection<dbmlServicesView>();
                if (Session["Services"] != null)
                {
                    GeneralColl<dbmlServicesView>.CopyCollection(Session["Services"] as ObservableCollection<dbmlServicesView>, objdbmlServicesView);
                }
                else
                {
                    returndbmlServicesView objreturndbmlServicesView = objServiceClient.ServicesGetByBPId((int)HardCodeValues.ServiceBPIdTrack);
                    if (objreturndbmlServicesView.objdbmlStatus.StatusId == 1 && objreturndbmlServicesView.objdbmlServicesView.Count > 0)
                    {
                        Session["Services"] = objreturndbmlServicesView.objdbmlServicesView;
                        objdbmlServicesView = objreturndbmlServicesView.objdbmlServicesView;
                    }
                }

                if (objdbmlServicesView != null && objdbmlServicesView.Count > 0)
                {
                    ObservableCollection<dbmlServicesView> objdbmlServicesViewList = new ObservableCollection<dbmlServicesView>(objdbmlServicesView.Where(itm => itm.TrackGroupId == intTrackGroupId).OrderBy(itm => itm.SrNo));
                    foreach (var itm in objdbmlServicesViewList)
                    {
                        if (Items.FirstOrDefault(Category => Convert.ToInt32(Category.Value) == itm.CategoryPropId) == null)
                        {
                            Items.Add(new SelectListItem { Text = itm.Category, Value = itm.CategoryPropId.ToString(), Selected = false });
                        }
                    }
                }
            }
            catch
            {

            }
            return Items;
        }

        public List<SelectListItem> GetServiveLookup(int intBPId)
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            try
            {
                ObservableCollection<dbmlServicesView> objdbmlServicesView = new ObservableCollection<dbmlServicesView>();
                if (Session["Services"] != null)
                {
                    GeneralColl<dbmlServicesView>.CopyCollection(Session["Services"] as ObservableCollection<dbmlServicesView>, objdbmlServicesView);
                }
                else
                {
                    returndbmlServicesView objreturndbmlServicesView = objServiceClient.ServicesGetByBPId(intBPId);
                    if (objreturndbmlServicesView.objdbmlStatus.StatusId == 1 && objreturndbmlServicesView.objdbmlServicesView.Count > 0)
                    {
                        Session["Services"] = objreturndbmlServicesView.objdbmlServicesView;
                        objdbmlServicesView = objreturndbmlServicesView.objdbmlServicesView;
                    }
                }

                if (objdbmlServicesView != null && objdbmlServicesView.Count > 0)
                {
                    ObservableCollection<dbmlServicesView> objdbmlServicesViewList = new ObservableCollection<dbmlServicesView>(objdbmlServicesView.Where(itm => itm.TrackGroupId > 0 && itm.IsActive == true).OrderBy(itm => itm.SrNo));
                    foreach (var itm in objdbmlServicesViewList)
                    {
                        if (Items.FirstOrDefault(itmTrack => Convert.ToInt32(itmTrack.Value) == itm.TrackGroupId) == null)
                        {
                            Items.Add(new SelectListItem { Text = itm.ZZTrackGroup, Value = itm.TrackGroupId.ToString(), Selected = false });
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Items;
        }


        #endregion

        #region Workshop Booking

        public ActionResult TrackWorkshopBooking()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.TrackGroup = "Track Booking";
                model.ViewTitle = "Track Booking";
                model.StateId = Convert.ToInt32(Session["StateId"]);

                returndbmlServicesView objreturndbmlServicesView = objServiceClient.ServicesGetByBPId(Convert.ToInt32(HardCodeValues.ServiceBPIdWorkShop));
                if (objreturndbmlServicesView.objdbmlStatus.StatusId == 1 && objreturndbmlServicesView.objdbmlServicesView.Count > 0)
                {
                    Session["WorkShopServices"] = objreturndbmlServicesView.objdbmlServicesView;
                }

                ViewBag.ServiveLookup = GetWorkShopServiveLookup(Convert.ToInt32(HardCodeValues.ServiceBPIdWorkShop));

                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);
                    model.DocDate = objdbmlBooking.ZZBookingDate;
                    model.DocNo = objdbmlBooking.BookingNo;
                    model.DocType = objdbmlBooking.ZZBookingType;
                    model.WorkFlowId = Convert.ToInt32(objdbmlBooking.ZZWorkFlowId);
                    model.WorkFlowStatusId = objdbmlBooking.ZZStatusWorkflowId;
                    model.StatusPropId = Convert.ToInt32(objdbmlBooking.StatusPropId);
                    model.BPId = Convert.ToInt32(Session["BPId"]);
                    model.ReportURL = strRptURL;
                    model.DocId = objdbmlBooking.BookingId;
                    model.POURL = strPOURL + objdbmlBooking.PODocPath;

                    //if (Convert.ToInt32(objdbmlBooking.TabStatusId) + 10 < 40)
                    //{
                    //    return RedirectToActionByStatusId(Convert.ToInt32(objdbmlBooking.TabStatusId));
                    //}
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), objdbmlBooking.BookingId);
                    model.RFQId = Convert.ToInt32(objdbmlBooking.RFQId);
                    model.RFQBPId = Convert.ToInt32(objdbmlBooking.ZZRFQBPId);
                    model.RFQBookingNo = objdbmlBooking.ZZRFQBookingNo;
                }
                else
                {
                    return RedirectToAction("Basic", "Home");
                }

            }
            catch
            {
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TrackWorkshopBooking(CommonModel model, string btnPrevNext)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (btnPrevNext.ToLower() == "prev")
                {
                    if (Convert.ToInt32(Session["BPId"]) == 90 || Convert.ToInt32(Session["BPId"]) == 91)
                    {
                        return RedirectToAction("MainLabBooking", "Home");
                    }
                    else
                    {
                        return RedirectToAction("MainTrackBookingNew", "Home");
                    }

                }

                if (btnPrevNext.ToLower() == "next")
                {
                    return RedirectToAction("TrackAddOnServicesBooking", "Home");
                }
            }
            catch
            {
            }

            return RedirectToAction("TrackWorkshopBooking", "Home");
        }

        [ValidateAntiForgeryToken]
        public ActionResult LoadWorkshopBookingInfo()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlWorkshopBookingDetailViewFront objreturndbmlWorkshopBookingDetailViewFront = new returndbmlWorkshopBookingDetailViewFront();
            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    objreturndbmlWorkshopBookingDetailViewFront = objServiceClient.WorkshopBookingDetailViewFrontGetByBookingId(objdbmlBooking.BookingId);
                    if (objreturndbmlWorkshopBookingDetailViewFront.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Success";
                    }
                    else
                    {
                        strStatus = objreturndbmlWorkshopBookingDetailViewFront.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, WorkshopBookingDetailList = objreturndbmlWorkshopBookingDetailViewFront.objdbmlWorkshopBookingDetailViewFront }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult WorkshopBookingSave(dbmlWorkshopBookingDetailViewFront model)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlWorkshopBookingDetailViewFront objreturndbmlWorkshopBookingDetailViewFront = new returndbmlWorkshopBookingDetailViewFront();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    model.BookingId = objdbmlBooking.BookingId;
                    model.RefServiceBPId = Convert.ToInt32(HardCodeValues.ServiceBPIdWorkShop);
                    //if (objdbmlBooking.BPId == 46 || objdbmlBooking.BPId == 91)
                    //{
                    //    model.UsageDate = objdbmlBooking.BookingDate.AddDays(Convert.ToInt32(model.BookingDay) - 1);
                    //}
                    //else
                    //{
                    model.UsageDate = objClassUserFunctions.ToDateTimeNotNull(model.ZZUsageDate);
                    //}
                    model.CreateId = Convert.ToInt32(Session["UserId"]);
                    model.CreateDate = DateTime.Now;
                    model.UpdateId = Convert.ToInt32(Session["UserId"]);
                    model.UpdateDate = DateTime.Now;

                    returndbmlWorkshopBookingDetailViewFront objreturndbmlWorkshopBookingDetailViewFrontTemp = new returndbmlWorkshopBookingDetailViewFront();
                    ObservableCollection<dbmlWorkshopBookingDetailViewFront> objdbmlWorkshopBookingDetailViewFront = new ObservableCollection<dbmlWorkshopBookingDetailViewFront>();
                    objdbmlWorkshopBookingDetailViewFront.Add(model);
                    objreturndbmlWorkshopBookingDetailViewFrontTemp.objdbmlWorkshopBookingDetailViewFront = objdbmlWorkshopBookingDetailViewFront;

                    objreturndbmlWorkshopBookingDetailViewFront = objServiceClient.WorkshopBookingDetailInsertFront(objreturndbmlWorkshopBookingDetailViewFrontTemp);

                    if (objreturndbmlWorkshopBookingDetailViewFront != null && objreturndbmlWorkshopBookingDetailViewFront.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Workshop Detail Saved Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlWorkshopBookingDetailViewFront.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, WorkshopBookingDetailList = objreturndbmlWorkshopBookingDetailViewFront.objdbmlWorkshopBookingDetailViewFront }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult WorkshopBookingInsert(dbmlWorkshopBookingDetailViewFront model, dbmlGeneral modelDate)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlWorkshopBookingDetailViewFront objreturndbmlWorkshopBookingDetailViewFront = new returndbmlWorkshopBookingDetailViewFront();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    model.BookingId = objdbmlBooking.BookingId;
                    model.RefServiceBPId = Convert.ToInt32(HardCodeValues.ServiceBPIdWorkShop);
                    //if (objdbmlBooking.BPId == 46 || objdbmlBooking.BPId == 91)
                    //{
                    //    model.UsageDate = objdbmlBooking.BookingDate.AddDays(Convert.ToInt32(model.BookingDay) - 1);
                    //}
                    //else
                    //{
                    model.UsageDate = objClassUserFunctions.ToDateTimeNotNull(model.ZZUsageDate);
                    //}
                    model.CreateId = Convert.ToInt32(Session["UserId"]);
                    model.CreateDate = DateTime.Now;
                    model.UpdateId = Convert.ToInt32(Session["UserId"]);
                    model.UpdateDate = DateTime.Now;

                    modelDate.dtFromDate = objClassUserFunctions.ToDateTimeNotNull(modelDate.StrOne);
                    modelDate.dtToDate = objClassUserFunctions.ToDateTimeNotNull(modelDate.StrTwo);

                    returndbmlWorkshopBookingDetailViewFront objreturndbmlWorkshopBookingDetailViewFrontTemp = new returndbmlWorkshopBookingDetailViewFront();
                    ObservableCollection<dbmlWorkshopBookingDetailViewFront> objdbmlWorkshopBookingDetailViewFront = new ObservableCollection<dbmlWorkshopBookingDetailViewFront>();
                    objdbmlWorkshopBookingDetailViewFront.Add(model);
                    objreturndbmlWorkshopBookingDetailViewFrontTemp.objdbmlWorkshopBookingDetailViewFront = objdbmlWorkshopBookingDetailViewFront;
                    objreturndbmlWorkshopBookingDetailViewFrontTemp.objdbmlGeneral = modelDate; // irfan
                    objreturndbmlWorkshopBookingDetailViewFront = objServiceClient.WorkshopBookingDetailInsertFrontRepeatRowNew(objreturndbmlWorkshopBookingDetailViewFrontTemp);

                    if (objreturndbmlWorkshopBookingDetailViewFront != null && objreturndbmlWorkshopBookingDetailViewFront.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Workshop Detail Saved Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlWorkshopBookingDetailViewFront.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, WorkshopBookingDetailList = objreturndbmlWorkshopBookingDetailViewFront.objdbmlWorkshopBookingDetailViewFront }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult WorkshopBookingDetailInsertFrontRepeatRow(dbmlGeneral model)
        {

            //string strBookingId, string strTrackBookingTimeDetailId, string strFromDate, string strToDate
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";
            //int intTrackGroupId = Convert.ToInt32(Session["TrackGroupId"]);
            model.dtFromDate = Convert.ToDateTime(DateTime.ParseExact(model.StrOne, "dd-MM-yyyy", CultureInfo.InvariantCulture)).Add(DateTime.Now.TimeOfDay);
            model.dtToDate = Convert.ToDateTime(DateTime.ParseExact(model.StrTwo, "dd-MM-yyyy", CultureInfo.InvariantCulture)).Add(DateTime.Now.TimeOfDay);

            returndbmlWorkshopBookingDetailViewFront objreturndbmlWorkshopBookingDetailViewFront = new returndbmlWorkshopBookingDetailViewFront();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    objreturndbmlWorkshopBookingDetailViewFront = objServiceClient.WorkshopBookingDetailInsertFrontRepeatRow(model);
                    if (objreturndbmlWorkshopBookingDetailViewFront.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Data Saved Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlWorkshopBookingDetailViewFront.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, WorkshopBookingDetailList = objreturndbmlWorkshopBookingDetailViewFront.objdbmlWorkshopBookingDetailViewFront }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult WorkshopBookingDelete(int intWorkshopBookingDetailId)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlWorkshopBookingDetailViewFront objreturndbmlWorkshopBookingDetailViewFront = new returndbmlWorkshopBookingDetailViewFront();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    objreturndbmlWorkshopBookingDetailViewFront = objServiceClient.WorkshopBookingDetailDelete(objdbmlBooking.BookingId, intWorkshopBookingDetailId);

                    if (objreturndbmlWorkshopBookingDetailViewFront != null && objreturndbmlWorkshopBookingDetailViewFront.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Workshop Details Deleted Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlWorkshopBookingDetailViewFront.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, WorkshopBookingDetailList = objreturndbmlWorkshopBookingDetailViewFront.objdbmlWorkshopBookingDetailViewFront }, JsonRequestBehavior.AllowGet);
        }

        public List<SelectListItem> GetWorkShopServiveLookup(int intBPId)
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            try
            {
                ObservableCollection<dbmlServicesView> objdbmlServicesView = new ObservableCollection<dbmlServicesView>();
                if (Session["WorkShopServices"] != null)
                {
                    GeneralColl<dbmlServicesView>.CopyCollection(Session["WorkShopServices"] as ObservableCollection<dbmlServicesView>, objdbmlServicesView);
                }
                else
                {
                    returndbmlServicesView objreturndbmlServicesView = objServiceClient.ServicesGetByBPId(intBPId);
                    if (objreturndbmlServicesView.objdbmlStatus.StatusId == 1 && objreturndbmlServicesView.objdbmlServicesView.Count > 0)
                    {
                        Session["WorkShopServices"] = objreturndbmlServicesView.objdbmlServicesView;
                        objdbmlServicesView = objreturndbmlServicesView.objdbmlServicesView;
                    }
                }

                if (objdbmlServicesView != null && objdbmlServicesView.Count > 0)
                {
                    ObservableCollection<dbmlServicesView> objdbmlServicesViewList = new ObservableCollection<dbmlServicesView>(objdbmlServicesView.Where(itm => itm.ServiceId > 0).OrderBy(itm => itm.SrNo));
                    foreach (var itm in objdbmlServicesViewList)
                    {
                        if (Items.FirstOrDefault(itmTrack => Convert.ToInt32(itmTrack.Value) == itm.ServiceId) == null)
                        {
                            if (intBPId == 13)
                            {
                                Items.Add(new SelectListItem { Text = itm.ServiceSpecification, Value = itm.ServiceId.ToString(), Selected = false });
                            }
                            else
                            {
                                Items.Add(new SelectListItem { Text = itm.ServiceName + " " + itm.ServiceSpecification, Value = itm.ServiceId.ToString(), Selected = false });
                            }
                        }
                    }
                }
            }
            catch
            {

            }
            return Items;
        }


        #endregion

        #region AddOn Services Booking

        public ActionResult TrackAddOnServicesBooking()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.TrackGroup = "Track Booking";
                model.ViewTitle = "Track Booking";
                model.StateId = Convert.ToInt32(Session["StateId"]);

                returndbmlServicesView objreturndbmlServicesView = objServiceClient.ServicesGetByBPId(Convert.ToInt32(HardCodeValues.ServiceBPIdAddOn));
                if (objreturndbmlServicesView.objdbmlStatus.StatusId == 1 && objreturndbmlServicesView.objdbmlServicesView.Count > 0)
                {
                    Session["AddOnServices"] = objreturndbmlServicesView.objdbmlServicesView;
                }

                ViewBag.ServiveLookup = GetAddOnServiveLookup(Convert.ToInt32(HardCodeValues.ServiceBPIdAddOn));

                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);
                    model.DocDate = objdbmlBooking.ZZBookingDate;
                    model.DocNo = objdbmlBooking.BookingNo;
                    model.DocType = objdbmlBooking.ZZBookingType;
                    model.WorkFlowId = Convert.ToInt32(objdbmlBooking.ZZWorkFlowId);
                    model.WorkFlowStatusId = objdbmlBooking.ZZStatusWorkflowId;
                    model.StatusPropId = Convert.ToInt32(objdbmlBooking.StatusPropId);
                    model.BPId = Convert.ToInt32(Session["BPId"]);
                    model.ReportURL = strRptURL;
                    model.DocId = objdbmlBooking.BookingId;
                    model.POURL = strPOURL + objdbmlBooking.PODocPath;

                    //if (Convert.ToInt32(objdbmlBooking.TabStatusId) + 10 < 40)
                    //{
                    //    return RedirectToActionByStatusId(Convert.ToInt32(objdbmlBooking.TabStatusId));
                    //}
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), objdbmlBooking.BookingId);
                    model.RFQId = Convert.ToInt32(objdbmlBooking.RFQId);
                    model.RFQBPId = Convert.ToInt32(objdbmlBooking.ZZRFQBPId);
                    model.RFQBookingNo = objdbmlBooking.ZZRFQBookingNo;
                }
                else
                {
                    return RedirectToAction("Basic", "Home");
                }

            }
            catch
            {
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TrackAddOnServicesBooking(CommonModel model, string btnPrevNext)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (btnPrevNext.ToLower() == "prev")
                {
                    return RedirectToAction("TrackWorkshopBooking", "Home");
                }

                if (btnPrevNext.ToLower() == "next")
                {
                    return RedirectToAction("WorkflowActivity", "Home");
                }
            }
            catch
            {
            }

            return RedirectToAction("TrackWorkshopBooking", "Home");
        }

        [ValidateAntiForgeryToken]
        public ActionResult LoadAddOnServicesInfo()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlBookingDetailAddOnServicesViewFront objreturndbmlBookingDetailAddOnServicesViewFront = new returndbmlBookingDetailAddOnServicesViewFront();
            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    objreturndbmlBookingDetailAddOnServicesViewFront = objServiceClient.BookingDetailAddOnServicesViewFrontGetByBookingId(objdbmlBooking.BookingId);
                    if (objreturndbmlBookingDetailAddOnServicesViewFront.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Success";
                    }
                    else
                    {
                        strStatus = objreturndbmlBookingDetailAddOnServicesViewFront.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, LoadAddOnServicesList = objreturndbmlBookingDetailAddOnServicesViewFront.objdbmlBookingDetailAddOnServicesViewFront }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult AddOnServicesSave(dbmlBookingDetailAddOnServicesViewFront model)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlBookingDetailAddOnServicesViewFront objreturndbmlBookingDetailAddOnServicesViewFront = new returndbmlBookingDetailAddOnServicesViewFront();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    model.BookingId = objdbmlBooking.BookingId;
                    model.BPId = Convert.ToInt32(HardCodeValues.ServiceBPIdAddOn);
                    //if (objdbmlBooking.BPId == 46 || objdbmlBooking.BPId == 91)
                    //{
                    //    model.ServiceDate = objdbmlBooking.BookingDate.AddDays(Convert.ToInt32(model.BookingDay) - 1);
                    //}
                    //else
                    //{
                    model.ServiceDate = objClassUserFunctions.ToDateTimeNotNull(model.ZZServiceDate);
                    // }
                    model.CreateId = Convert.ToInt32(Session["UserId"]);
                    model.CreateDate = DateTime.Now;
                    model.UpdateId = Convert.ToInt32(Session["UserId"]);
                    model.UpdateDate = DateTime.Now;

                    returndbmlBookingDetailAddOnServicesViewFront objreturndbmlBookingDetailAddOnServicesViewFrontTemp = new returndbmlBookingDetailAddOnServicesViewFront();
                    ObservableCollection<dbmlBookingDetailAddOnServicesViewFront> objdbmlBookingDetailAddOnServicesViewFront = new ObservableCollection<dbmlBookingDetailAddOnServicesViewFront>();
                    objdbmlBookingDetailAddOnServicesViewFront.Add(model);
                    objreturndbmlBookingDetailAddOnServicesViewFrontTemp.objdbmlBookingDetailAddOnServicesViewFront = objdbmlBookingDetailAddOnServicesViewFront;

                    objreturndbmlBookingDetailAddOnServicesViewFront = objServiceClient.BookingDetailAddOnServicesInsertFront(objreturndbmlBookingDetailAddOnServicesViewFrontTemp);

                    if (objreturndbmlBookingDetailAddOnServicesViewFront != null && objreturndbmlBookingDetailAddOnServicesViewFront.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Addon Service Saved Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlBookingDetailAddOnServicesViewFront.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, LoadAddOnServicesList = objreturndbmlBookingDetailAddOnServicesViewFront.objdbmlBookingDetailAddOnServicesViewFront }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult AddOnServicesInsert(dbmlBookingDetailAddOnServicesViewFront model, dbmlGeneral modelDate)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlBookingDetailAddOnServicesViewFront objreturndbmlBookingDetailAddOnServicesViewFront = new returndbmlBookingDetailAddOnServicesViewFront();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    model.BookingId = objdbmlBooking.BookingId;
                    model.BPId = Convert.ToInt32(HardCodeValues.ServiceBPIdAddOn);
                    //if (objdbmlBooking.BPId == 46 || objdbmlBooking.BPId == 91)
                    //{
                    //    model.ServiceDate = objdbmlBooking.BookingDate.AddDays(Convert.ToInt32(model.BookingDay) - 1);
                    //}
                    //else
                    //{
                    model.ServiceDate = objClassUserFunctions.ToDateTimeNotNull(model.ZZServiceDate);
                    // }
                    model.CreateId = Convert.ToInt32(Session["UserId"]);
                    model.CreateDate = DateTime.Now;
                    model.UpdateId = Convert.ToInt32(Session["UserId"]);
                    model.UpdateDate = DateTime.Now;

                    modelDate.dtFromDate = objClassUserFunctions.ToDateTimeNotNull(modelDate.StrOne);
                    modelDate.dtToDate = objClassUserFunctions.ToDateTimeNotNull(modelDate.StrTwo);

                    returndbmlBookingDetailAddOnServicesViewFront objreturndbmlBookingDetailAddOnServicesViewFrontTemp = new returndbmlBookingDetailAddOnServicesViewFront();
                    ObservableCollection<dbmlBookingDetailAddOnServicesViewFront> objdbmlBookingDetailAddOnServicesViewFront = new ObservableCollection<dbmlBookingDetailAddOnServicesViewFront>();
                    objdbmlBookingDetailAddOnServicesViewFront.Add(model);
                    objreturndbmlBookingDetailAddOnServicesViewFrontTemp.objdbmlBookingDetailAddOnServicesViewFront = objdbmlBookingDetailAddOnServicesViewFront;
                    objreturndbmlBookingDetailAddOnServicesViewFrontTemp.objdbmlGeneral = modelDate; // irfan
                    objreturndbmlBookingDetailAddOnServicesViewFront = objServiceClient.BookingDetailAddOnServicesInsertFrontRepeatRowNew(objreturndbmlBookingDetailAddOnServicesViewFrontTemp);

                    if (objreturndbmlBookingDetailAddOnServicesViewFront != null && objreturndbmlBookingDetailAddOnServicesViewFront.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Addon Service Saved Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlBookingDetailAddOnServicesViewFront.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, LoadAddOnServicesList = objreturndbmlBookingDetailAddOnServicesViewFront.objdbmlBookingDetailAddOnServicesViewFront }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult DailyServiceCount(dbmlGeneral model)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlDailyServiceCount objdbmlDailyServiceCount = new returndbmlDailyServiceCount();
            dbmlGeneral objGeneral = new dbmlGeneral();
            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    objGeneral.dtFromDate = model.dtFromDate;
                    objGeneral.IntOne = Convert.ToInt32(model.IntOne);
                    objGeneral.IntTwo = model.IntTwo;
                    objdbmlDailyServiceCount = objServiceClient.DailyServiceCount(objGeneral);
                    if (objdbmlDailyServiceCount != null && objdbmlDailyServiceCount.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Daily Service Count";
                    }
                    else
                    {
                        strStatus = objdbmlDailyServiceCount.objdbmlStatus.Status;
                        intStatusId = Convert.ToInt32(objdbmlDailyServiceCount.objdbmlStatus.StatusId);
                    }
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, DailyServiceCountList = objdbmlDailyServiceCount.objdbmlDailyServiceCountList }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult BookingDetailAddOnServicesInsertFrontRepeatRow(dbmlGeneral model)
        {

            //string strBookingId, string strTrackBookingTimeDetailId, string strFromDate, string strToDate
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";
            //int intTrackGroupId = Convert.ToInt32(Session["TrackGroupId"]);
            model.dtFromDate = Convert.ToDateTime(DateTime.ParseExact(model.StrOne, "dd-MM-yyyy", CultureInfo.InvariantCulture)).Add(DateTime.Now.TimeOfDay);
            model.dtToDate = Convert.ToDateTime(DateTime.ParseExact(model.StrTwo, "dd-MM-yyyy", CultureInfo.InvariantCulture)).Add(DateTime.Now.TimeOfDay);

            returndbmlBookingDetailAddOnServicesViewFront objreturndbmlBookingDetailAddOnServicesViewFront = new returndbmlBookingDetailAddOnServicesViewFront();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    objreturndbmlBookingDetailAddOnServicesViewFront = objServiceClient.BookingDetailAddOnServicesInsertFrontRepeatRow(model);
                    if (objreturndbmlBookingDetailAddOnServicesViewFront.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Data Saved Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlBookingDetailAddOnServicesViewFront.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, LoadAddOnServicesList = objreturndbmlBookingDetailAddOnServicesViewFront.objdbmlBookingDetailAddOnServicesViewFront }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult AddOnServicesDelete(int intBookingAddOnServiceId)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlBookingDetailAddOnServicesViewFront objreturndbmlBookingDetailAddOnServicesViewFront = new returndbmlBookingDetailAddOnServicesViewFront();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    objreturndbmlBookingDetailAddOnServicesViewFront = objServiceClient.BookingDetailAddOnServicesDelete(objdbmlBooking.BookingId, intBookingAddOnServiceId);

                    if (objreturndbmlBookingDetailAddOnServicesViewFront != null && objreturndbmlBookingDetailAddOnServicesViewFront.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "AddOn Services Deleted Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlBookingDetailAddOnServicesViewFront.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, LoadAddOnServicesList = objreturndbmlBookingDetailAddOnServicesViewFront.objdbmlBookingDetailAddOnServicesViewFront }, JsonRequestBehavior.AllowGet);
        }

        public List<SelectListItem> GetAddOnServiveLookup(int intBPId)
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            try
            {
                ObservableCollection<dbmlServicesView> objdbmlServicesView = new ObservableCollection<dbmlServicesView>();
                if (Session["AddOnServices"] != null)
                {
                    GeneralColl<dbmlServicesView>.CopyCollection(Session["AddOnServices"] as ObservableCollection<dbmlServicesView>, objdbmlServicesView);
                }
                else
                {
                    returndbmlServicesView objreturndbmlServicesView = objServiceClient.ServicesGetByBPId(intBPId);
                    if (objreturndbmlServicesView.objdbmlStatus.StatusId == 1 && objreturndbmlServicesView.objdbmlServicesView.Count > 0)
                    {
                        Session["AddOnServices"] = objreturndbmlServicesView.objdbmlServicesView;
                        objdbmlServicesView = objreturndbmlServicesView.objdbmlServicesView;
                    }
                }

                if (objdbmlServicesView != null && objdbmlServicesView.Count > 0)
                {
                    if (objdbmlServicesView[0].ServiceName == "Food")
                        ViewBag.MinInDay = objdbmlServicesView[0].MinInDay;
                    ObservableCollection<dbmlServicesView> objdbmlServicesViewList = new ObservableCollection<dbmlServicesView>(objdbmlServicesView.Where(itm => itm.ServiceId > 0).OrderBy(itm => itm.SrNo));
                    foreach (var itm in objdbmlServicesViewList)
                    {
                        if (Items.FirstOrDefault(itmTrack => Convert.ToInt32(itmTrack.Value) == itm.ServiceId) == null)
                        {
                            Items.Add(new SelectListItem { Text = itm.ServiceName + " " + itm.ServiceSpecification, Value = itm.ServiceId.ToString(), Selected = false });
                        }
                    }
                }
            }
            catch
            {

            }
            return Items;
        }

        [ValidateAntiForgeryToken]
        public ActionResult LabBookingDetailInsertFrontRepeatRow(dbmlGeneral model)
        {

            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";
            model.dtFromDate = Convert.ToDateTime(DateTime.ParseExact(model.StrOne, "dd-MM-yyyy", CultureInfo.InvariantCulture)).Add(DateTime.Now.TimeOfDay);
            model.dtToDate = Convert.ToDateTime(DateTime.ParseExact(model.StrTwo, "dd-MM-yyyy", CultureInfo.InvariantCulture)).Add(DateTime.Now.TimeOfDay);

            returndbmlLabBookingDetailViewFront objreturndbmlLabBookingDetailViewFront = new returndbmlLabBookingDetailViewFront();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    objreturndbmlLabBookingDetailViewFront = objServiceClient.LabBookingDetailInsertFrontRepeatRow(model);
                    if (objreturndbmlLabBookingDetailViewFront.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Data Saved Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlLabBookingDetailViewFront.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, WorkshopBookingDetailList = objreturndbmlLabBookingDetailViewFront.objdbmlLabBookingDetailViewFront }, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region Lab Booking Detail

        public ActionResult MainLabBooking()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.TrackGroup = "Track Booking";
                model.ViewTitle = "Track Booking";
                model.StateId = Convert.ToInt32(Session["StateId"]);

                returndbmlServicesView objreturndbmlServicesView = objServiceClient.ServicesGetByBPId(Convert.ToInt32(HardCodeValues.ServiceBPIdLab));
                if (objreturndbmlServicesView.objdbmlStatus.StatusId == 1 && objreturndbmlServicesView.objdbmlServicesView.Count > 0)
                {
                    Session["LabServices"] = objreturndbmlServicesView.objdbmlServicesView;
                }

                ViewBag.ServiveLookup = GetLabServiveLookup(Convert.ToInt32(HardCodeValues.ServiceBPIdAddOn));

                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);
                    model.DocDate = objdbmlBooking.ZZBookingDate;
                    model.DocNo = objdbmlBooking.BookingNo;
                    model.DocType = objdbmlBooking.ZZBookingType;
                    model.WorkFlowId = Convert.ToInt32(objdbmlBooking.ZZWorkFlowId);
                    model.WorkFlowStatusId = objdbmlBooking.ZZStatusWorkflowId;
                    model.StatusPropId = Convert.ToInt32(objdbmlBooking.StatusPropId);
                    model.BPId = Convert.ToInt32(Session["BPId"]);
                    model.ReportURL = strRptURL;
                    model.DocId = objdbmlBooking.BookingId;
                    model.POURL = strPOURL + objdbmlBooking.PODocPath;
                    model.SupportDocPathURL = strbookingsupportdoc + objdbmlBooking.ZZSupportDocPath;
                    //if (Convert.ToInt32(objdbmlBooking.TabStatusId) + 10 < 40)
                    //{
                    //    return RedirectToActionByStatusId(Convert.ToInt32(objdbmlBooking.TabStatusId));
                    //}
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), objdbmlBooking.BookingId);
                    model.RFQId = Convert.ToInt32(objdbmlBooking.RFQId);
                    model.RFQBPId = Convert.ToInt32(objdbmlBooking.ZZRFQBPId);
                    model.RFQBookingNo = objdbmlBooking.ZZRFQBookingNo;
                }
                else
                {
                    return RedirectToAction("Basic", "Home");
                }

            }
            catch
            {
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MainLabBooking(CommonModel model, string btnPrevNext)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (btnPrevNext.ToLower() == "prev")
                {
                    return RedirectToAction("Component", "Home");
                }

                if (btnPrevNext.ToLower() == "next")
                {
                    return RedirectToAction("TrackWorkshopBooking", "Home");
                }
            }
            catch
            {
            }

            return RedirectToAction("TrackWorkshopBooking", "Home");
        }

        [ValidateAntiForgeryToken]
        public ActionResult LoadLabServicesInfo()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlLabBookingDetailViewFront objreturndbmlLabBookingDetailViewFront = new returndbmlLabBookingDetailViewFront();
            ObservableCollection<dbmlServicesView> objdbmlServicesView = new ObservableCollection<dbmlServicesView>();
            ObservableCollection<dbmlLablinkVorC> objdbmlLablinkVorC = new ObservableCollection<dbmlLablinkVorC>();
            returndbmlListOfVehicleComponent objreturndbmlListOfVehicleComponent = new returndbmlListOfVehicleComponent();

            ObservableCollection<dbmlBookingView> objdbmlBookingList = new ObservableCollection<dbmlBookingView>();
            try
            {
                if (Session["LabServices"] != null)
                {
                    GeneralColl<dbmlServicesView>.CopyCollection(Session["LabServices"] as ObservableCollection<dbmlServicesView>, objdbmlServicesView);
                }

                if (Session["LablinkVorC"] != null)
                {
                    GeneralColl<dbmlLablinkVorC>.CopyCollection(Session["LablinkVorC"] as ObservableCollection<dbmlLablinkVorC>, objdbmlLablinkVorC);
                }

                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    objdbmlBookingList.Add(objdbmlBooking);

                    objreturndbmlLabBookingDetailViewFront = objServiceClient.LabBookingDetailViewFrontGetByBookingId(objdbmlBooking.BookingId);
                    if (objreturndbmlLabBookingDetailViewFront.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Success";
                    }
                    else
                    {
                        strStatus = objreturndbmlLabBookingDetailViewFront.objdbmlStatus.Status;
                    }

                    objreturndbmlListOfVehicleComponent = objServiceClient.ListOfVehicleComponentGetByDocId(objdbmlBooking.BookingId);

                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, BookingList = objdbmlBookingList, ServiceList = objdbmlServicesView, LablinkVorCList = objdbmlLablinkVorC, VehicleCompList = objreturndbmlListOfVehicleComponent.objdbmlListOfVehicleComponent, LabServicesList = objreturndbmlLabBookingDetailViewFront.objdbmlLabBookingDetailViewFront }, JsonRequestBehavior.AllowGet);
        }

        //[ValidateAntiForgeryToken]
        public ActionResult LabServicesSave(HttpPostedFileBase[] files, string data)
        {
            dbmlLabBookingDetailViewFront model = JsonConvert.DeserializeObject<dbmlLabBookingDetailViewFront>(data);
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            ObservableCollection<dbmlLabServiceDoc> objddbmlLabServiceDoc = new ObservableCollection<dbmlLabServiceDoc>();

            int intStatusId = 99;
            string strStatus = "Invalid";
            int uploadedFileCount = 0;

            returndbmlLabBookingDetailViewFront objreturndbmlLabBookingDetailViewFront = new returndbmlLabBookingDetailViewFront();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    for (int i = 0; i < files.Length; i++)
                    {
                        dbmlLabServiceDoc serviceDoc = UploadDoc(files[i], objdbmlBooking.BookingId, model.ServiceId, i + 1);
                        uploadedFileCount = uploadedFileCount + 1;
                        if (string.IsNullOrEmpty(serviceDoc.DocumentName))
                        {
                            throw new Exception("not able to Upload Document on Azure.");
                        }

                        objddbmlLabServiceDoc.Add(serviceDoc);
                    }

                    model.BookingId = objdbmlBooking.BookingId;
                    model.RefServiceBPId = Convert.ToInt32(HardCodeValues.ServiceBPIdLab);
                    //if (objdbmlBooking.BPId == 46 || objdbmlBooking.BPId == 91)
                    //{
                    //    model.UsageDate = objdbmlBooking.BookingDate.AddDays(Convert.ToInt32(model.BookingDay) - 1);
                    //}
                    //else
                    //{
                    model.UsageDate = objClassUserFunctions.ToDateTimeNotNull(model.ZZUsageDate);
                    //}

                    model.CreateId = Convert.ToInt32(Session["UserId"]);
                    model.CreateDate = DateTime.Now;
                    model.UpdateId = Convert.ToInt32(Session["UserId"]);
                    model.UpdateDate = DateTime.Now;

                    returndbmlLabBookingDetailViewFront objreturndbmlLabBookingDetailViewFrontTemp = new returndbmlLabBookingDetailViewFront();
                    ObservableCollection<dbmlLabBookingDetailViewFront> objdbmlLabBookingDetailViewFront = new ObservableCollection<dbmlLabBookingDetailViewFront>();

                    objdbmlLabBookingDetailViewFront.Add(model);
                    objreturndbmlLabBookingDetailViewFrontTemp.objdbmlLabBookingDetailViewFront = objdbmlLabBookingDetailViewFront;
                    objreturndbmlLabBookingDetailViewFrontTemp.objdbmlLabServiceDoc = objddbmlLabServiceDoc;

                    objreturndbmlLabBookingDetailViewFront = objServiceClient.LabBookingDetailInsertFront(objreturndbmlLabBookingDetailViewFrontTemp);

                    if (objreturndbmlLabBookingDetailViewFront != null && objreturndbmlLabBookingDetailViewFront.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Lab Service Saved Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlLabBookingDetailViewFront.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, LabServicesList = objreturndbmlLabBookingDetailViewFront.objdbmlLabBookingDetailViewFront, fileCount = uploadedFileCount }, JsonRequestBehavior.AllowGet);
        }

        public dbmlLabServiceDoc UploadDoc(HttpPostedFileBase file, int bookingId, int? serviceId, int i)
        {
            dbmlLabServiceDoc docModel = new dbmlLabServiceDoc();
            if (file.InputStream.Length > 0)
            {
                string strFileExtention = Path.GetExtension(file.FileName);


                strFileExtention = strFileExtention.Substring(strFileExtention.IndexOf('/') + 1);
                byte[] byteImage = objClassUserFunctions.ConvertToBytes(file.InputStream);
                bool blnStatus = false;

                string strBlobAccount = System.Configuration.ConfigurationManager.AppSettings["strBlobAccount"];
                string strAccountKey = System.Configuration.ConfigurationManager.AppSettings["strAccountKey"];
                string strFileStorage = System.Configuration.ConfigurationManager.AppSettings["strFileStorage"];

                // string strImageName = $"LabDoc_{bookingId}_{serviceId}_{i}.{strFileExtention}";
                int id = serviceId != null ? Convert.ToInt32(serviceId) : 0;
                string strImageName = GetFileExtension(file, id, i, bookingId);
                string strImageContainerName = "bookingsupportdocument";

                /////////////////////// For Azure Blob ///////////////////////////////////////////////////                            
                string strImageURL = objClassUserFunctions.UploadFileStreamToAzureBlob(strBlobAccount, strAccountKey, strImageContainerName, strImageName, byteImage);
                if (strImageURL != "")
                    blnStatus = true;

                if (blnStatus)
                {
                    docModel.DocumentName = strImageName;
                    docModel.DocumentPath = $"{strBlobAccount}/{strImageContainerName}/{strImageName}";
                    docModel.DocumentType = strFileExtention;
                    docModel.ServiceId = serviceId != null ? Convert.ToInt32(serviceId) : 0;
                }
            }
            return docModel;
        }

        public string GetFileExtension(HttpPostedFileBase imageToUpload, int ServiceId, int count, int bookingId)
        {
            string imageName = "";
            if (Path.GetExtension(imageToUpload.FileName) != ".pdf")
            {
                imageName = $"LabDoc_{bookingId}_{ServiceId}_{count}.pdf";
                if (Path.GetExtension(imageToUpload.FileName) == ".xls" || Path.GetExtension(imageToUpload.FileName) == ".xlsx")
                {
                    imageName = $"LabDoc_{bookingId}_{ServiceId}_{count}.xls";
                }
                else if (Path.GetExtension(imageToUpload.FileName) == ".doc" || Path.GetExtension(imageToUpload.FileName) == ".docx")
                {
                    imageName = $"LabDoc_{bookingId}_{ServiceId}_{count}.docx";
                }
            }
            else
            {
                imageName = $"LabDoc_{bookingId}_{ServiceId}_{count}{Path.GetExtension(imageToUpload.FileName)}";
            }

            return imageName;
        }
        [ValidateAntiForgeryToken]
        public ActionResult LabServicesDelete(int intLabBookingDetailId)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlLabBookingDetailViewFront objreturndbmlLabBookingDetailViewFront = new returndbmlLabBookingDetailViewFront();

            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    objreturndbmlLabBookingDetailViewFront = objServiceClient.LabBookingDetailDelete(objdbmlBooking.BookingId, intLabBookingDetailId);

                    if (objreturndbmlLabBookingDetailViewFront != null && objreturndbmlLabBookingDetailViewFront.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Lab Service Deleted Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlLabBookingDetailViewFront.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, LabServicesList = objreturndbmlLabBookingDetailViewFront.objdbmlLabBookingDetailViewFront }, JsonRequestBehavior.AllowGet);
        }

        public List<SelectListItem> GetLabServiveLookup(int intBPId)
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            try
            {
                ObservableCollection<dbmlServicesView> objdbmlServicesView = new ObservableCollection<dbmlServicesView>();
                if (Session["LabServices"] != null)
                {
                    GeneralColl<dbmlServicesView>.CopyCollection(Session["LabServices"] as ObservableCollection<dbmlServicesView>, objdbmlServicesView);
                }
                else
                {
                    returndbmlServicesView objreturndbmlServicesView = objServiceClient.ServicesGetByBPId(intBPId);
                    if (objreturndbmlServicesView.objdbmlStatus.StatusId == 1 && objreturndbmlServicesView.objdbmlServicesView.Count > 0)
                    {
                        Session["LabServices"] = objreturndbmlServicesView.objdbmlServicesView;
                        objdbmlServicesView = objreturndbmlServicesView.objdbmlServicesView;
                    }
                }

                if (objdbmlServicesView != null && objdbmlServicesView.Count > 0)
                {
                    ObservableCollection<dbmlServicesView> objdbmlServicesViewList = new ObservableCollection<dbmlServicesView>(objdbmlServicesView.Where(itm => itm.ServiceId > 0).OrderBy(itm => itm.SrNo));
                    foreach (var itm in objdbmlServicesViewList)
                    {
                        if (Items.FirstOrDefault(itmTrack => itmTrack.Value == itm.ServiceName) == null)
                        {
                            Items.Add(new SelectListItem { Text = itm.ServiceName, Value = itm.ServiceName, Selected = false });
                        }
                    }
                }
            }
            catch
            {

            }
            return Items;
        }


        #endregion

        #region Workflow Activity Track
        public ActionResult WorkflowActivity()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.StatusPropId = 0;
                model.StateId = Convert.ToInt32(Session["StateId"]);
                ViewBag.VehicleType = GetVehicleType();

                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);
                    model.DocDate = objdbmlBooking.ZZBookingDate;
                    model.DocNo = objdbmlBooking.BookingNo;
                    model.DocType = objdbmlBooking.ZZBookingType;
                    model.WorkFlowId = Convert.ToInt32(objdbmlBooking.ZZWorkFlowId);
                    model.WorkFlowStatusId = objdbmlBooking.ZZStatusWorkflowId;
                    model.StatusPropId = Convert.ToInt32(objdbmlBooking.StatusPropId);
                    model.BPId = Convert.ToInt32(Session["BPId"]);
                    model.ReportURL = strRptURL;
                    model.DocId = objdbmlBooking.BookingId;
                    model.POURL = strPOURL + objdbmlBooking.PODocPath;

                    //if (Convert.ToInt32(objdbmlBooking.TabStatusId) + 10 < 10)
                    //{
                    //    return RedirectToActionByStatusId(Convert.ToInt32(objdbmlBooking.TabStatusId));
                    //}
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), objdbmlBooking.BookingId);
                    model.RFQId = Convert.ToInt32(objdbmlBooking.RFQId);
                    model.RFQBPId = Convert.ToInt32(objdbmlBooking.ZZRFQBPId);
                    model.RFQBookingNo = objdbmlBooking.ZZRFQBookingNo;
                }
                else
                {
                    return RedirectToAction("Basic", "Home");
                }
            }
            catch
            {
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult WorkflowActivity(CommonModel model, string btnPrevNext)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (btnPrevNext.ToLower() == "prev")
                {
                    return RedirectToAction("TrackAddOnServicesBooking", "Home");
                }

            }
            catch
            {
            }

            return RedirectToAction("Vehicle", "Home");
        }

        [ValidateAntiForgeryToken]
        public ActionResult LoadWorkflowActivityInfo()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlWorkFlowActivityTrackView objreturndbmlWorkFlowActivityTrackView = new returndbmlWorkFlowActivityTrackView();
            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    objreturndbmlWorkFlowActivityTrackView = objServiceClient.WorkFlowActivityTrackGetByBPIdDocId(objdbmlBooking.BPId, objdbmlBooking.BookingId);
                    if (objreturndbmlWorkFlowActivityTrackView.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Success";
                    }
                    else
                    {
                        strStatus = objreturndbmlWorkFlowActivityTrackView.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, WorkFlowActivityList = objreturndbmlWorkFlowActivityTrackView.objdbmlWorkFlowActivityTrackView }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #endregion

        #region Change Password
        public ActionResult ChangePassword()
        {
            CommonModel model = new CommonModel();
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public ActionResult UserPaswordReset(dbmlUserView model)
        {
            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlUser objreturndbmlUser = new returndbmlUser();
            try
            {
                objreturndbmlUser = objServiceClient.UserPaswordReset(model.UserId, model.PassWord);
                if (objreturndbmlUser != null && objreturndbmlUser.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = objreturndbmlUser.objdbmlStatus.Status;
                }
                else
                {
                    strStatus = objreturndbmlUser.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UserGetByLoginId(dbmlUserView model)
        {
            int intStatusId = 99;
            string strStatus = "Invalid";
            returndbmlUser objreturndbmlUser = new returndbmlUser();
            try
            {
                objreturndbmlUser = objServiceClient.UserGetByLoginId(model.LoginId, model.PassWord);
                if (objreturndbmlUser != null && objreturndbmlUser.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = objreturndbmlUser.objdbmlStatus.Status;
                }
                else
                {
                    strStatus = objreturndbmlUser.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Company/Department  
        public ActionResult VerifyeMail(string xyz, string abc, string lmn)
        {
            CommonModel model = new CommonModel();
            model.StatusPropId = -1;
            try
            {
                model.POURL = strPOURL;
                int intUserId = Convert.ToInt32(abc);
                returndbmlUser objreturndbmlUser = objServiceClient.UsereMailIdVerification(intUserId);
                if (objreturndbmlUser != null && objreturndbmlUser.objdbmlStatus.StatusId == 1 && objreturndbmlUser.objdbmlUserView.Count > 0)
                {
                    Session["UserView"] = objreturndbmlUser.objdbmlUserView.FirstOrDefault();
                    Session["UserIdTemp"] = objreturndbmlUser.objdbmlUserView.FirstOrDefault().UserId;
                    model.UserName = objreturndbmlUser.objdbmlUserView.FirstOrDefault().UserName;
                    model.LoginId = objreturndbmlUser.objdbmlUserView.FirstOrDefault().LoginId;
                    model.PWDenterdStatusId = 0;
                    if (objreturndbmlUser.objdbmlUserView.FirstOrDefault().PassWord != null && objreturndbmlUser.objdbmlUserView.FirstOrDefault().PassWord != "")
                    {
                        model.PWDenterdStatusId = 1;
                    }
                    model.Message = objreturndbmlUser.objdbmlStatus.Status;
                    model.StatusPropId = 1;
                    model.DocId = objreturndbmlUser.objdbmlUserView.FirstOrDefault().EmailVerify == true ? 1 : 0;
                }
                else
                {
                    model.Message = objreturndbmlUser.objdbmlStatus.Status;
                    if (objreturndbmlUser.objdbmlStatus.StatusId == -2)
                    {
                        model.StatusPropId = -2;
                    }

                }
            }
            catch
            {
                model.Message = "Un-Authorized Access";
            }

            return View(model);
        }

        [ValidateAntiForgeryToken]
        public ActionResult CreatePassword(dbmlUserView model)
        {
            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlUser objreturndbmlUser = new returndbmlUser();
            try
            {
                string strPSW = DecryptStringAES(model.PassWord);

                objreturndbmlUser = objServiceClient.UserPaswordReset(Convert.ToInt32(Session["UserIdTemp"]), strPSW);
                if (objreturndbmlUser != null && objreturndbmlUser.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = objreturndbmlUser.objdbmlStatus.Status;
                }
                else
                {
                    strStatus = objreturndbmlUser.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult CreatePasswordVerifyMailPage(dbmlUserView model)
        {
            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlUser objreturndbmlUser = new returndbmlUser();
            try
            {
                string strPSW = DecryptStringAES(model.PassWord);

                objreturndbmlUser = objServiceClient.UserPaswordReset(Convert.ToInt32(Session["UserIdTemp"]), strPSW);
                if (objreturndbmlUser != null && objreturndbmlUser.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = objreturndbmlUser.objdbmlStatus.Status;
                    // strBody = "Dear NATRAX Admin,";
                    string strTo = string.Empty, strBcc = string.Empty, strSubject = string.Empty, strBody = string.Empty, strAttachDocPath = string.Empty;
                    string[] strCc = { };
                    string strLink = "https://pgmsbackoffice.azurewebsites.net/";
                    strSubject = "Company Verification Pending.";
                    strBody += "<br /><br /> Document uploaded by " + "<b>" + Convert.ToString(objreturndbmlUser.objdbmlUserView.FirstOrDefault().ZZCompanyName) + "</b>" + " for registration process.";
                    strBody += "<br /><br /> Please login to <a href='" + strLink + "'>PGMS</a> for futher process.";
                    strBody += "<br /><br />Regards";
                    strBody += "<br /><span style='font-weight:bold;font-family:Trebuchet MS;font-style:italic'>NATRAX</span>";
                    SendMailMessageAsync(strFromEmail, strFromPwd, strRegistrationUploadDocument, strBcc, strCc, strSubject, strBody, "");
                }
                else
                {
                    strStatus = objreturndbmlUser.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult UserVerifyStatusUpdate(dbmlUserView model)
        {
            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlUser objreturndbmlUser = new returndbmlUser();
            try
            {

                objreturndbmlUser = objServiceClient.UserVerifyStatusUpdate(Convert.ToInt32(Session["UserIdTemp"]));
                if (objreturndbmlUser != null && objreturndbmlUser.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = objreturndbmlUser.objdbmlStatus.Status;
                    //strBody = "Dear NATRAX Admin,";
                    string strTo = string.Empty, strBcc = string.Empty, strSubject = string.Empty, strBody = string.Empty, strAttachDocPath = string.Empty;
                    string[] strCc = { };
                    string strLink = "https://pgmsbackoffice.azurewebsites.net/";
                    strSubject = "Company document verification request.";
                    strBody += "<br /><br /> Document uploaded by " + "<b>" + Convert.ToString(objreturndbmlUser.objdbmlUserView.FirstOrDefault().ZZCompanyName) + "</b>" + " for registration process.";
                    strBody += "<br /><br /> Please login to <a href='" + strLink + "'>PGMS</a> for futher process.";
                    strBody += "<br /><br />Regards";
                    strBody += "<br /><span style='font-weight:bold;font-family:Trebuchet MS;font-style:italic'>NATRAX</span>";
                    SendMailMessageAsync(strFromEmail, strFromPwd, strRegistrationUploadDocument, strBcc, strCc, strSubject, strBody, "");
                }
                else
                {
                    strStatus = objreturndbmlUser.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult LoadVerifyeMailInfo()
        {
            if (Session["UserIdTemp"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlCustomerMasterPhoto objreturndbmlCustomerMasterPhoto = new returndbmlCustomerMasterPhoto();
            try
            {
                if (Session["UserView"] != null)
                {
                    dbmlUserView objdbmlUserView = new dbmlUserView();
                    GeneralColl<dbmlUserView>.CopyObject(Session["UserView"] as dbmlUserView, objdbmlUserView);

                    objreturndbmlCustomerMasterPhoto = objServiceClient.CustomerMasterPhotoGetByCustomerMasterId(Convert.ToInt32(objdbmlUserView.CustomerMasterId));
                    if (objreturndbmlCustomerMasterPhoto.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Success";
                    }
                    else
                    {
                        strStatus = objreturndbmlCustomerMasterPhoto.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, DocList = objreturndbmlCustomerMasterPhoto.objdbmlCustomerMasterPhoto }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RegDocUpload()
        {
            if (Session["UserIdTemp"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }
            ObservableCollection<dbmlCustomerMasterPhoto> objdbmlCustomerMasterPhotoRet = new ObservableCollection<dbmlCustomerMasterPhoto>();
            int intStatusId = 99;
            string strStatus = "Invalid";

            try
            {
                if (Session["UserView"] != null)
                {
                    dbmlUserView objdbmlUserView = new dbmlUserView();
                    GeneralColl<dbmlUserView>.CopyObject(Session["UserView"] as dbmlUserView, objdbmlUserView);

                    HttpPostedFileBase file = Request.Files["ImageData"];
                    int intDocType = Convert.ToInt32(Request.Form["DocType"]);

                    if (file.InputStream.Length > 0 && intDocType > 0)
                    {
                        string strFileExtention = file.ContentType;
                        if (strFileExtention.ToLower() == "image/jpeg" || strFileExtention.ToLower() == "application/pdf")
                        {
                            if ((file.ContentLength / 1024) > 0 && (file.ContentLength / 1024) <= 2048)
                            {
                                strFileExtention = strFileExtention.Substring(strFileExtention.IndexOf('/') + 1);
                                byte[] byteImage = objClassUserFunctions.ConvertToBytes(file.InputStream);
                                bool blnStatus = false;
                                string strFTPUserName = System.Configuration.ConfigurationManager.AppSettings["strFTPUserName"];
                                string strFTPUserPSW = System.Configuration.ConfigurationManager.AppSettings["strFTPUserPassword"];
                                string strFTPUrl = System.Configuration.ConfigurationManager.AppSettings["strFTPServer"];
                                string strFTPRoot = System.Configuration.ConfigurationManager.AppSettings["strFTPRoot"];

                                string strBlobAccount = System.Configuration.ConfigurationManager.AppSettings["strBlobAccount"];
                                string strAccountKey = System.Configuration.ConfigurationManager.AppSettings["strAccountKey"];
                                string strFileStorage = System.Configuration.ConfigurationManager.AppSettings["strFileStorage"];

                                string strImageName = "CompReg_" + Convert.ToString(intDocType) + "_" + Convert.ToString(objdbmlUserView.CustomerMasterId) + "." + strFileExtention;
                                string strImageContainerName = "booking";
                                string strImageURL = "";
                                string strFTPFilePath = "";
                                if (strFileStorage == "FTP")
                                {
                                    ////////////////////// For FTP /////////////////////////////////////////////                       
                                    strFTPFilePath = strFTPRoot + strImageName;

                                    blnStatus = objClassUserFunctions.UploadImageToFTPFromWEBCLIENT(strFTPUrl, strFTPUserName, strFTPUserPSW, byteImage, strFTPFilePath);
                                }
                                else if (strFileStorage == "AzureBlob")
                                {
                                    /////////////////////// For Azure Blob ///////////////////////////////////////////////////                            
                                    strImageURL = objClassUserFunctions.UploadFileStreamToAzureBlob(strBlobAccount, strAccountKey, strImageContainerName, strImageName, byteImage);
                                    if (strImageURL != "")
                                        blnStatus = true;
                                }
                                if (blnStatus)
                                {
                                    dbmlCustomerMasterPhoto objdbmlCustomerMasterPhoto = new dbmlCustomerMasterPhoto();
                                    objdbmlCustomerMasterPhoto.CustomerMasterId = (int)objdbmlUserView.CustomerMasterId;
                                    objdbmlCustomerMasterPhoto.ImageSerialNo = intDocType;
                                    objdbmlCustomerMasterPhoto.ImageName = strImageName;
                                    //objdbmlCustomerMasterPhoto.Remark = "";
                                    objdbmlCustomerMasterPhoto.VerifiedBy = 0;
                                    objdbmlCustomerMasterPhoto.CreateId = objdbmlUserView.UserId;
                                    objdbmlCustomerMasterPhoto.CreateDate = DateTime.Now;

                                    returndbmlCustomerMasterPhoto objreturndbmlCustomerMasterPhotoTemp = new returndbmlCustomerMasterPhoto();
                                    ObservableCollection<dbmlCustomerMasterPhoto> objdbmlCustomerMasterPhotoList = new ObservableCollection<dbmlCustomerMasterPhoto>();
                                    objdbmlCustomerMasterPhotoList.Add(objdbmlCustomerMasterPhoto);
                                    objreturndbmlCustomerMasterPhotoTemp.objdbmlCustomerMasterPhoto = objdbmlCustomerMasterPhotoList;

                                    returndbmlCustomerMasterPhoto objreturndbmlCustomerMasterPhoto = objServiceClient.CustomerMasterPhotoInsert(objreturndbmlCustomerMasterPhotoTemp);
                                    if (objreturndbmlCustomerMasterPhoto != null && objreturndbmlCustomerMasterPhoto.objdbmlStatus.StatusId == 1)
                                    {
                                        objdbmlCustomerMasterPhotoRet = objreturndbmlCustomerMasterPhoto.objdbmlCustomerMasterPhoto;
                                        intStatusId = 1;
                                        strStatus = "Document Uploaded Successfully";
                                    }
                                    else
                                    {
                                        if (strFileStorage == "FTP")
                                        {
                                            bool delStatus = objClassUserFunctions.DeleteFileFromFTPFromWEBCLIENT(strFTPUrl, strFTPUserName, strFTPUserPSW, strFTPFilePath);
                                        }
                                        else if (strFileStorage == "AzureBlob")
                                        {
                                            bool delStatus = objClassUserFunctions.DeleteFileFromAzureBlob(strBlobAccount, strAccountKey, strImageContainerName, strImageName);
                                        }

                                        strStatus = "Document Uploading Process Failed!";
                                        //strStatus = objreturndbmlBooking.objdbmlStatus.Status;
                                    }
                                }
                                else
                                {
                                    strStatus = "Document Uploading Process Failed!";
                                }
                            }
                            else
                            {
                                strStatus = "File Size between 1KB to 2 MB can be accepted!";
                            }
                        }
                        else
                        {
                            strStatus = "Only (JPEG, PDF) format can be accepted!";
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }

            return Json(new { StatusId = intStatusId, Status = strStatus, DocList = objdbmlCustomerMasterPhotoRet }, JsonRequestBehavior.AllowGet);
        }

        public List<SelectListItem> LoadState()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            try
            {
                returndbmlState objreturndbmlState = objServiceClient.StateGetAll();
                if (objreturndbmlState != null && objreturndbmlState.objdbmlStatus.StatusId == 1)
                {
                    foreach (var itm in objreturndbmlState.objdbmlState)
                    {
                        Items.Add(new SelectListItem { Text = itm.State, Value = itm.StateId.ToString(), Selected = false });
                    }
                }
            }
            catch
            {

            }
            return Items;
        }

        [ValidateAntiForgeryToken]
        public ActionResult LoadDistictByStateId(int intStateId)
        {
            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlDistrict objreturndbmlDistrict = new returndbmlDistrict();
            try
            {
                objreturndbmlDistrict = objServiceClient.DistrictGetByStateId(intStateId);
                if (objreturndbmlDistrict != null && objreturndbmlDistrict.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                }
                else
                {
                    strStatus = objreturndbmlDistrict.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, DistrictList = objreturndbmlDistrict.objdbmlDistrict }, JsonRequestBehavior.AllowGet);
        }

        public List<SelectListItem> LoadCountry()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            try
            {
                returndbmlCountry objreturndbmlCountry = objServiceClient.CountryGetAll();
                if (objreturndbmlCountry != null && objreturndbmlCountry.objdbmlStatus.StatusId == 1)
                {
                    foreach (var itm in objreturndbmlCountry.objdbmlCountry)
                    {
                        Items.Add(new SelectListItem { Text = itm.Country, Value = itm.CountryId.ToString(), Selected = false });
                    }
                }
            }
            catch
            {

            }
            return Items;
        }

        public ActionResult Department()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                if (Convert.ToInt32(Session["UserTypePropId"]) == 168)
                {
                    return RedirectToAction("Dashboard", "Home");
                }

                ViewBag.Country = LoadCountry();
                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.StateId = Convert.ToInt32(Session["StateId"]);
            }
            catch
            {
            }

            return View(model);
        }

        [ValidateAntiForgeryToken]
        public ActionResult LoadDepartmentInfo()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlCompanyDepartment objreturndbmlCompanyDepartment = new returndbmlCompanyDepartment();
            try
            {
                objreturndbmlCompanyDepartment = objServiceClient.CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
                if (objreturndbmlCompanyDepartment.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objreturndbmlCompanyDepartment.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, CompanyDepartmentList = objreturndbmlCompanyDepartment.objdbmlCompanyDepartment }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult DepartmentSave(dbmlCompanyDepartment model)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlCompanyDepartment objreturndbmlCompanyDepartment = new returndbmlCompanyDepartment();

            try
            {
                model.CustomerMasterId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.CreateId = Convert.ToInt32(Session["UserId"]);
                model.CreateDate = DateTime.Now;
                model.UpdateId = Convert.ToInt32(Session["UserId"]);
                model.UpdateDate = DateTime.Now;

                returndbmlCompanyDepartment objreturndbmlCompanyDepartmentTemp = new returndbmlCompanyDepartment();
                ObservableCollection<dbmlCompanyDepartment> objdbmlCompanyDepartment = new ObservableCollection<dbmlCompanyDepartment>();
                objdbmlCompanyDepartment.Add(model);
                objreturndbmlCompanyDepartmentTemp.objdbmlCompanyDepartment = objdbmlCompanyDepartment;
                if (model.CompanyDepartmentId <= 0)
                {
                    objreturndbmlCompanyDepartment = objServiceClient.CompanyDepartmentInsert(objreturndbmlCompanyDepartmentTemp);
                }
                else
                {
                    objreturndbmlCompanyDepartment = objServiceClient.CompanyDepartmentUpdate(objreturndbmlCompanyDepartmentTemp);
                }
                if (objreturndbmlCompanyDepartment != null && objreturndbmlCompanyDepartment.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Data Saved Successfully";
                }
                else
                {
                    strStatus = objreturndbmlCompanyDepartment.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, CompanyDepartmentList = objreturndbmlCompanyDepartment.objdbmlCompanyDepartment }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Users()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (Convert.ToInt32(Session["UserTypePropId"]) == 168)
                {
                    return RedirectToAction("Dashboard", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.StateId = Convert.ToInt32(Session["StateId"]);
                if (model.UserTypePropId == 167)
                {
                    ViewBag.Department = LoadDepartmentWithFilter();
                }
                else
                {
                    ViewBag.Department = LoadDepartment();
                }
            }
            catch
            {
            }

            return View(model);
        }

        public List<SelectListItem> LoadDepartment()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            try
            {
                returndbmlCompanyDepartment objreturndbmlCompanyDepartment = objServiceClient.CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
                if (objreturndbmlCompanyDepartment != null && objreturndbmlCompanyDepartment.objdbmlStatus.StatusId == 1)
                {
                    foreach (var itm in objreturndbmlCompanyDepartment.objdbmlCompanyDepartment)
                    {
                        Items.Add(new SelectListItem { Text = itm.Department, Value = itm.CompanyDepartmentId.ToString(), Selected = false });
                    }
                }
            }
            catch
            {

            }
            return Items;
        }

        public List<SelectListItem> LoadDepartmentWithFilter()
        {
            List<SelectListItem> Items = new List<SelectListItem>();
            try
            {
                returndbmlCompanyDepartment objreturndbmlCompanyDepartment = objServiceClient.CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
                if (objreturndbmlCompanyDepartment != null && objreturndbmlCompanyDepartment.objdbmlStatus.StatusId == 1)
                {
                    foreach (var itm in objreturndbmlCompanyDepartment.objdbmlCompanyDepartment)
                    {
                        if (itm.CompanyDepartmentId == Convert.ToInt32(Session["CompanyDepartmentId"]))
                            Items.Add(new SelectListItem { Text = itm.Department, Value = itm.CompanyDepartmentId.ToString(), Selected = false });
                    }
                }
            }
            catch
            {

            }
            return Items;
        }

        [ValidateAntiForgeryToken]
        public ActionResult LoadUserInfo()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlUser objreturndbmlUser = new returndbmlUser();
            try
            {
                objreturndbmlUser = objServiceClient.UserViewFrontGetByCompanyId(Convert.ToInt32(Session["ZZCompanyId"]));
                if (objreturndbmlUser.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objreturndbmlUser.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, UserList = objreturndbmlUser.objdbmlUserView }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult UserSave(dbmlUserView model)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlUser objreturndbmlUser = new returndbmlUser();

            try
            {
                model.CustomerMasterId = Convert.ToInt32(Session["ZZCompanyId"]);
                // model.UserTypePropId = 81;
                model.CreateId = Convert.ToInt32(Session["UserId"]);
                model.CreateDate = DateTime.Now;
                model.UpdateId = Convert.ToInt32(Session["UserId"]);
                model.UpdateDate = DateTime.Now;

                returndbmlUser objreturndbmlUserTemp = new returndbmlUser();
                ObservableCollection<dbmlUserView> objdbmlUserView = new ObservableCollection<dbmlUserView>();
                objdbmlUserView.Add(model);
                objreturndbmlUserTemp.objdbmlUserView = objdbmlUserView;
                if (model.UserId <= 0)
                {
                    objreturndbmlUser = objServiceClient.UserInsert(objreturndbmlUserTemp);
                }
                else
                {
                    objreturndbmlUser = objServiceClient.UserUpdate(objreturndbmlUserTemp);
                }
                if (objreturndbmlUser != null && objreturndbmlUser.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Data Saved Successfully";
                }
                else
                {
                    strStatus = objreturndbmlUser.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, UserList = objreturndbmlUser.objdbmlUserView }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region PO Upload
        [ValidateAntiForgeryToken]
        public ActionResult LoadGridData()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }
            int intStatusId = 99;
            string strStatus = "Invalid";
            returnBookingQuotationPIDdetailView objreturndbmlBookingQuotationPIDdetail = new returnBookingQuotationPIDdetailView();
            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);
                    objreturndbmlBookingQuotationPIDdetail = objServiceClient.BookingQuotationPIDetailGetByBookingId(objdbmlBooking.BookingId);
                    if (objreturndbmlBookingQuotationPIDdetail.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Success";
                    }
                    else
                    {
                        strStatus = objreturndbmlBookingQuotationPIDdetail.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, VehicleList = objreturndbmlBookingQuotationPIDdetail.objdbmlBookingQuotationPIDdetailView }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult POSave(dbmlBookingQuotationPIDetailView model)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }
            int intStatusId = 99;
            string strStatus = "Invalid";
            returnBookingQuotationPIDdetailView objreturndbmlBookingQuotationPIDdetail = new returnBookingQuotationPIDdetailView();
            try
            {
                model.UpdateId = Convert.ToInt32(Session["UserId"]);
                model.PODate = Convert.ToDateTime(DateTime.ParseExact(model.ZZPODate, "dd-MM-yyyy", CultureInfo.InvariantCulture)).Add(DateTime.Now.TimeOfDay);
                model.POValidityDate = Convert.ToDateTime(DateTime.ParseExact(model.ZZPOValidityDate, "dd-MM-yyyy", CultureInfo.InvariantCulture)).Add(DateTime.Now.TimeOfDay);
                objreturndbmlBookingQuotationPIDdetail = objServiceClient.BookingQuotationPIDetailUpdate(model);

                if (objreturndbmlBookingQuotationPIDdetail != null && objreturndbmlBookingQuotationPIDdetail.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Data Saved Successfully";
                }
                else
                {
                    strStatus = objreturndbmlBookingQuotationPIDdetail.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, VehicleList = objreturndbmlBookingQuotationPIDdetail.objdbmlBookingQuotationPIDdetailView }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Quick Estimate
        public ActionResult BasicEstimate()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.StateId = Convert.ToInt32(Session["StateId"]);
                model.BPId = Convert.ToInt32(Session["BPId"]);
                model.ReportURL = strRptURL;

                ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));

                if (Session["objdbmlEstimate"] != null)
                {
                    dbmlEstimateView objdbmlEstimate = new dbmlEstimateView();
                    GeneralColl<dbmlEstimateView>.CopyObject(Session["objdbmlEstimate"] as dbmlEstimateView, objdbmlEstimate);
                    model.DocDate = objdbmlEstimate.ZZDocDate;
                    model.DocNo = Convert.ToString(objdbmlEstimate.DocNo);
                    model.ZZDocNo = objdbmlEstimate.ZZDocNo;
                    model.DocType = "Quick Estimate - Track";
                    model.WorkFlowId = Convert.ToInt32(objdbmlEstimate.ZZWorkFlowId);
                    model.WorkFlowStatusId = objdbmlEstimate.ZZStatusWorkflowId;
                    model.StatusPropId = Convert.ToInt32(objdbmlEstimate.StatusId);
                    model.DocId = objdbmlEstimate.EstimateId;
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), objdbmlEstimate.EstimateId);
                    // model.POURL = strPOURL + objdbmlEstimate.PODocPath;
                    // model.RFQId = Convert.ToInt32(objdbmlEstimate.RFQId);
                    // model.RFQBPId = Convert.ToInt32(objdbmlEstimate.ZZRFQBPId);
                    // model.RFQBookingNo = objdbmlEstimate.ZZRFQBookingNo;
                    // ViewBag.WorkflowRemark = objdbmlEstimate.ZZWorkflowRemark;
                    switch (Convert.ToInt32(Session["BPId"]))
                    {
                        case 192:
                            model.DocType = "Quick Estimate - Track";
                            Session["SessBookingType"] = "Track";
                            break;
                        case 193:
                            model.DocType = "Quick Estimate - Lab";
                            Session["SessBookingType"] = "Lab";
                            break;

                    }
                }
                else
                {
                    model.DocDate = "To be allotted";
                    model.DocNo = "To be allotted";
                    switch (Convert.ToInt32(Session["BPId"]))
                    {
                        case 192:
                            model.WorkFlowId = Convert.ToInt32(HardCodeValues.EstimateCreateWFId);
                            model.DocType = "Quick Estimate - Track";
                            Session["SessBookingType"] = "Track";
                            break;
                        case 193:
                            model.WorkFlowId = Convert.ToInt32(HardCodeValues.EstimateLabCreateWFId);
                            model.DocType = "Quick Estimate - Lab";
                            Session["SessBookingType"] = "Lab";
                            break;
                    }
                    model.StatusPropId = Convert.ToInt32(HardCodeValues.OpenStatusId);
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), 0);
                    model.RFQId = 0;
                    model.RFQBPId = 0;
                    model.RFQBookingNo = "";
                }
            }
            catch (Exception ex)
            {

            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BasicEstimate(CommonModel model, string btnPrevNext)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (btnPrevNext.ToLower() == "prev")
                {
                    if (Convert.ToInt32(Session["BPId"]) == 193)
                    {
                        return RedirectToAction("LabEstimate", "Home");
                    }
                    else
                    {
                        return RedirectToAction("TrackEstimate", "Home");
                    }

                }

                if (btnPrevNext.ToLower() == "next")
                {
                    if (Convert.ToInt32(Session["BPId"]) == 193)
                    {
                        return RedirectToAction("LabEstimate", "Home");
                    }
                    else
                    {
                        return RedirectToAction("TrackEstimate", "Home");
                    }
                }
            }
            catch
            {
            }

            return RedirectToAction("BasicEstimate", "Home");
        }

        public ActionResult Estimate()
        {
            CommonModel model = new CommonModel();
            try
            {
                Session["SessBookingType"] = "Track";

                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["objdbmlEstimate"] = null;

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);

                ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
            }
            catch
            {
            }

            return View(model);
        }

        public ActionResult NewEstimate(int intBPId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                Session["BPId"] = intBPId;
            }
            catch
            {

            }

            return RedirectToAction("BasicEstimate", "Home");
        }

        [ValidateAntiForgeryToken]
        public ActionResult BasicEstimateInsertUpdate(dbmlEstimateView model)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";
            returndbmlEstimate objreturndbmlEstimate = new returndbmlEstimate();
            try
            {
                model.DocDate = DateTime.Now;//objClassUserFunctions.ToDateTimeNotNull(model.ZZBookingDate);
                model.BPId = Convert.ToInt32(Session["BPId"]);
                model.CustomerMasterId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.CreateId = Convert.ToInt32(Session["UserId"]);
                model.CreateDate = DateTime.Now;
                model.UpdateId = Convert.ToInt32(Session["UserId"]);
                model.UpdateDate = DateTime.Now;
                // model.Prefix = "";
                model.StatusId = Convert.ToInt32(HardCodeValues.OpenStatusId);
                //model.StatusPropId = Convert.ToInt32(HardCodeValues.OpenStatusId);

                returndbmlEstimate objreturndbmlEstimateTemp = new returndbmlEstimate();
                ObservableCollection<dbmlEstimateView> objdbmlEstimateList = new ObservableCollection<dbmlEstimateView>();
                objdbmlEstimateList.Add(model);
                objreturndbmlEstimateTemp.objdbmlEstimateList = objdbmlEstimateList;

                if (model.EstimateId <= 0)
                {
                    objreturndbmlEstimate = objServiceClient.EstimateInsert(objreturndbmlEstimateTemp);
                }
                else
                {
                    objreturndbmlEstimate = objServiceClient.EstimateUpdate(objreturndbmlEstimateTemp);
                }

                if (objreturndbmlEstimate != null && objreturndbmlEstimate.objdbmlStatus.StatusId == 1)
                {
                    Session["objdbmlEstimate"] = objreturndbmlEstimate.objdbmlEstimateList.FirstOrDefault();
                    intStatusId = 1;
                    strStatus = "Data Saved Successfully";
                }
                else
                {
                    strStatus = objreturndbmlEstimate.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, EstimateList = objreturndbmlEstimate.objdbmlEstimateList }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TrackEstimate()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.TrackGroup = "Track Booking";
                model.ViewTitle = "Track Booking";
                model.StateId = Convert.ToInt32(Session["StateId"]);

                returndbmlServicesView objreturndbmlServicesView = objServiceClient.ServicesGetByBPId(Convert.ToInt32(HardCodeValues.ServiceBPIdTrack));
                if (objreturndbmlServicesView.objdbmlStatus.StatusId == 1 && objreturndbmlServicesView.objdbmlServicesView.Count > 0)
                {
                    Session["Services"] = objreturndbmlServicesView.objdbmlServicesView;
                }

                //Session["TrackGroupId"] = model.TrackGroupId;
                //Session["TrackGroup"] = model.TrackGroup;
                ViewBag.ServiveLookup = GetServiveLookup(Convert.ToInt32(HardCodeValues.ServiceBPIdTrack));
                ViewBag.TimeSlot = GetTimeSlot();
                ViewBag.ServiveCategory = GetServiveCategory(model.TrackGroupId);

                if (Session["objdbmlEstimate"] != null)
                {
                    dbmlEstimateView objdbmlEstimate = new dbmlEstimateView();
                    GeneralColl<dbmlEstimateView>.CopyObject(Session["objdbmlEstimate"] as dbmlEstimateView, objdbmlEstimate);
                    model.DocDate = objdbmlEstimate.ZZDocDate;
                    model.DocNo = Convert.ToString(objdbmlEstimate.DocNo);
                    model.ZZDocNo = objdbmlEstimate.ZZDocNo;
                    model.DocType = "Quick Estimate RFQ";
                    model.WorkFlowId = Convert.ToInt32(objdbmlEstimate.ZZWorkFlowId);
                    model.WorkFlowStatusId = objdbmlEstimate.ZZStatusWorkflowId;
                    model.StatusPropId = Convert.ToInt32(objdbmlEstimate.StatusId);
                    model.DocId = objdbmlEstimate.EstimateId;
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), objdbmlEstimate.EstimateId);
                    // model.POURL = strPOURL + objdbmlEstimate.PODocPath;
                    // model.RFQId = Convert.ToInt32(objdbmlEstimate.RFQId);
                    // model.RFQBPId = Convert.ToInt32(objdbmlEstimate.ZZRFQBPId);
                    // model.RFQBookingNo = objdbmlEstimate.ZZRFQBookingNo;
                    // ViewBag.WorkflowRemark = objdbmlEstimate.ZZWorkflowRemark;
                }
                else
                {
                    return RedirectToAction("BasicEstimate", "Home");
                }

            }
            catch
            {
            }

            return View("TrackEstimate", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TrackEstimate(CommonModel model, string btnPrevNext)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (btnPrevNext.ToLower() == "prev")
                {
                    return RedirectToAction("BasicEstimate", "Home");
                }
                else if (btnPrevNext.ToLower() == "next")
                {
                    return RedirectToAction("TrackWorkshopEstimate", "Home");
                }
            }
            catch
            {
            }

            return RedirectToAction("TrackEstimate", "Home");
        }

        public ActionResult TrackWorkshopEstimate()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.TrackGroup = "Track Booking";
                model.ViewTitle = "Track Booking";
                model.StateId = Convert.ToInt32(Session["StateId"]);

                returndbmlServicesView objreturndbmlServicesView = objServiceClient.ServicesGetByBPId(Convert.ToInt32(HardCodeValues.ServiceBPIdWorkShop));
                if (objreturndbmlServicesView.objdbmlStatus.StatusId == 1 && objreturndbmlServicesView.objdbmlServicesView.Count > 0)
                {
                    Session["WorkShopServices"] = objreturndbmlServicesView.objdbmlServicesView;
                }

                ViewBag.ServiveLookup = GetWorkShopServiveLookup(Convert.ToInt32(HardCodeValues.ServiceBPIdWorkShop));

                if (Session["objdbmlEstimate"] != null)
                {
                    dbmlEstimateView objdbmlEstimate = new dbmlEstimateView();
                    GeneralColl<dbmlEstimateView>.CopyObject(Session["objdbmlEstimate"] as dbmlEstimateView, objdbmlEstimate);
                    model.DocDate = objdbmlEstimate.ZZDocDate;
                    model.DocNo = Convert.ToString(objdbmlEstimate.DocNo);
                    model.ZZDocNo = objdbmlEstimate.ZZDocNo;
                    model.DocType = "Quick Estimate RFQ";
                    model.WorkFlowId = Convert.ToInt32(objdbmlEstimate.ZZWorkFlowId);
                    model.WorkFlowStatusId = objdbmlEstimate.ZZStatusWorkflowId;
                    model.StatusPropId = Convert.ToInt32(objdbmlEstimate.StatusId);
                    model.DocId = objdbmlEstimate.EstimateId;
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), objdbmlEstimate.EstimateId);
                    // model.POURL = strPOURL + objdbmlEstimate.PODocPath;
                    // model.RFQId = Convert.ToInt32(objdbmlEstimate.RFQId);
                    // model.RFQBPId = Convert.ToInt32(objdbmlEstimate.ZZRFQBPId);
                    // model.RFQBookingNo = objdbmlEstimate.ZZRFQBookingNo;
                    // ViewBag.WorkflowRemark = objdbmlEstimate.ZZWorkflowRemark;
                }
                else
                {
                    return RedirectToAction("BasicEstimate", "Home");
                }

            }
            catch
            {
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TrackWorkshopEstimate(CommonModel model, string btnPrevNext)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (btnPrevNext.ToLower() == "prev")
                {
                    if (Convert.ToInt32(Session["BPId"]) == 193)
                    {
                        return RedirectToAction("LabEstimate", "Home");
                    }
                    else
                    {
                        return RedirectToAction("TrackEstimate", "Home");
                    }

                }

                if (btnPrevNext.ToLower() == "next")
                {
                    return RedirectToAction("TrackAddOnServicesEstimate", "Home");
                }
            }
            catch
            {
            }

            return RedirectToAction("TrackWorkshopEstimate", "Home");
        }

        public ActionResult TrackAddOnServicesEstimate()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.TrackGroup = "Track Booking";
                model.ViewTitle = "Track Booking";
                model.StateId = Convert.ToInt32(Session["StateId"]);

                returndbmlServicesView objreturndbmlServicesView = objServiceClient.ServicesGetByBPId(Convert.ToInt32(HardCodeValues.ServiceBPIdAddOn));
                if (objreturndbmlServicesView.objdbmlStatus.StatusId == 1 && objreturndbmlServicesView.objdbmlServicesView.Count > 0)
                {
                    Session["AddOnServices"] = objreturndbmlServicesView.objdbmlServicesView;
                }

                ViewBag.ServiveLookup = GetAddOnServiveLookup(Convert.ToInt32(HardCodeValues.ServiceBPIdAddOn));

                if (Session["objdbmlEstimate"] != null)
                {
                    dbmlEstimateView objdbmlEstimate = new dbmlEstimateView();
                    GeneralColl<dbmlEstimateView>.CopyObject(Session["objdbmlEstimate"] as dbmlEstimateView, objdbmlEstimate);
                    model.DocDate = objdbmlEstimate.ZZDocDate;
                    model.DocNo = Convert.ToString(objdbmlEstimate.DocNo);
                    model.ZZDocNo = objdbmlEstimate.ZZDocNo;
                    model.DocType = "Quick Estimate RFQ";
                    model.WorkFlowId = Convert.ToInt32(objdbmlEstimate.ZZWorkFlowId);
                    model.WorkFlowStatusId = objdbmlEstimate.ZZStatusWorkflowId;
                    model.StatusPropId = Convert.ToInt32(objdbmlEstimate.StatusId);
                    model.DocId = objdbmlEstimate.EstimateId;
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), objdbmlEstimate.EstimateId);
                    // model.POURL = strPOURL + objdbmlEstimate.PODocPath;
                    // model.RFQId = Convert.ToInt32(objdbmlEstimate.RFQId);
                    // model.RFQBPId = Convert.ToInt32(objdbmlEstimate.ZZRFQBPId);
                    // model.RFQBookingNo = objdbmlEstimate.ZZRFQBookingNo;
                    // ViewBag.WorkflowRemark = objdbmlEstimate.ZZWorkflowRemark;
                }
                else
                {
                    return RedirectToAction("BasicEstimate", "Home");
                }

            }
            catch
            {
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TrackAddOnServicesEstimate(CommonModel model, string btnPrevNext)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (btnPrevNext.ToLower() == "prev")
                {
                    return RedirectToAction("TrackWorkshopEstimate", "Home");
                }
                //if (btnPrevNext.ToLower() == "next")
                //{
                //    return RedirectToAction("WorkflowActivity", "Home");
                //}
            }
            catch
            {
            }

            return RedirectToAction("TrackWorkshopEstimate", "Home");
        }

        [ValidateAntiForgeryToken]
        public ActionResult EstimateSearchViewFrontGetByCompanyIdFromDateToDate(int intDepartmentId, int intBookingTypeId, int intStatusPropId)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlEstimateSearchViewFront objreturndbmlEstimateSearchViewFront = new returndbmlEstimateSearchViewFront();

            try
            {
                objreturndbmlEstimateSearchViewFront = objServiceClient.EstimateSearchViewFrontGetByCompanyIdFromDateToDate(intDepartmentId, DateTime.Now.Date, DateTime.Now.Date, intBookingTypeId, intStatusPropId);

                if (objreturndbmlEstimateSearchViewFront != null && objreturndbmlEstimateSearchViewFront.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objreturndbmlEstimateSearchViewFront.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, BookingList = objreturndbmlEstimateSearchViewFront.objdbmlEstimateSearchViewFrontList }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EstimateViewGetByEstimateId(int intEstimateId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            returndbmlEstimate objreturndbmlEstimate = new returndbmlEstimate();
            try
            {
                objreturndbmlEstimate = objServiceClient.EstimateViewGetByEstimateId(intEstimateId);

                if (objreturndbmlEstimate != null && objreturndbmlEstimate.objdbmlStatus.StatusId == 1)
                {
                    Session["objdbmlEstimate"] = objreturndbmlEstimate.objdbmlEstimateList.FirstOrDefault();
                    Session["BPId"] = objreturndbmlEstimate.objdbmlEstimateList.FirstOrDefault().BPId;
                }
            }
            catch
            {

            }

            return RedirectToAction("BasicEstimate", "Home");
        }

        [ValidateAntiForgeryToken]
        public ActionResult LoadQuickBasicInfo()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";
            ObservableCollection<dbmlEstimateView> objdbmlEstimateList = new ObservableCollection<dbmlEstimateView>();
            try
            {
                if (Session["objdbmlEstimate"] != null)
                {
                    dbmlEstimateView objdbmlEstimate = new dbmlEstimateView();
                    GeneralColl<dbmlEstimateView>.CopyObject(Session["objdbmlEstimate"] as dbmlEstimateView, objdbmlEstimate);

                    objdbmlEstimateList.Add(objdbmlEstimate);

                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = "Estimate Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, BookingList = objdbmlEstimateList }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult LoadEstimateInfo()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlEstimate objreturndbmlEstimate = new returndbmlEstimate();
            try
            {
                if (Session["objdbmlEstimate"] != null)
                {
                    dbmlEstimateView objdblmEstimate = new dbmlEstimateView();
                    GeneralColl<dbmlEstimateView>.CopyObject(Session["objdbmlEstimate"] as dbmlEstimateView, objdblmEstimate);

                    objreturndbmlEstimate = objServiceClient.EstimateDetailViewGetByEstimateId(objdblmEstimate.EstimateId);
                    if (objreturndbmlEstimate.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Success";
                    }
                    else
                    {
                        strStatus = objreturndbmlEstimate.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Estimate Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, BookingList = objreturndbmlEstimate.objdbmlEstimateDetailViewList }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult EstimateDetailSave(dbmlEstimateDetailView model)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlEstimate objreturndbmlEstimate = new returndbmlEstimate();

            try
            {
                if (Session["objdbmlEstimate"] != null)
                {
                    dbmlEstimateView objdblmEstimate = new dbmlEstimateView();
                    GeneralColl<dbmlEstimateView>.CopyObject(Session["objdbmlEstimate"] as dbmlEstimateView, objdblmEstimate);

                    model.EstimateId = objdblmEstimate.EstimateId;
                    model.CreateId = Convert.ToInt32(Session["UserId"]);
                    model.CreateDate = DateTime.Now;
                    model.UpdateId = Convert.ToInt32(Session["UserId"]);
                    model.UpdateDate = DateTime.Now;

                    returndbmlEstimate objreturndbmlEstimateTemp = new returndbmlEstimate();
                    ObservableCollection<dbmlEstimateDetailView> objdbmlEstimateDetailView = new ObservableCollection<dbmlEstimateDetailView>();
                    objdbmlEstimateDetailView.Add(model);
                    objreturndbmlEstimateTemp.objdbmlEstimateDetailViewList = objdbmlEstimateDetailView;
                    if (model.EstimateDetailId <= 0)
                        objreturndbmlEstimate = objServiceClient.EstimateDetailInsert(objreturndbmlEstimateTemp);
                    else
                        objreturndbmlEstimate = objServiceClient.EstimateDetailUpdate(objreturndbmlEstimateTemp);

                    if (objreturndbmlEstimate != null && objreturndbmlEstimate.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Detail Saved Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlEstimate.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, BookingList = objreturndbmlEstimate.objdbmlEstimateDetailViewList }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult EstimateDetailDelete(int intEstimateDetailId)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlEstimate objreturndbmlEstimate = new returndbmlEstimate();

            try
            {
                if (Session["objdbmlEstimate"] != null)
                {
                    dbmlEstimateView objdblmEstimate = new dbmlEstimateView();
                    GeneralColl<dbmlEstimateView>.CopyObject(Session["objdbmlEstimate"] as dbmlEstimateView, objdblmEstimate);

                    objreturndbmlEstimate = objServiceClient.EstimateDetailDelete(intEstimateDetailId, objdblmEstimate.EstimateId);

                    if (objreturndbmlEstimate != null && objreturndbmlEstimate.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Details Deleted Successfully";
                    }
                    else
                    {
                        strStatus = objreturndbmlEstimate.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, BookingList = objreturndbmlEstimate.objdbmlEstimateDetailViewList }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult BookingStatusGetByServiceIdTimeSlotPropIdWEFDateEstimate(int[] intlstServiceId, int intTimeSlotId, string strWED)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlBookingStatusTimeSlotView objreturndbmlBookingStatusTimeSlotView = new returndbmlBookingStatusTimeSlotView();

            try
            {
                dbmlEstimateView objdblmEstimate = new dbmlEstimateView();
                GeneralColl<dbmlEstimateView>.CopyObject(Session["objdbmlEstimate"] as dbmlEstimateView, objdblmEstimate);

                DateTime dtWED = DateTime.Now;
                dtWED = objClassUserFunctions.ToDateTimeNotNull(strWED);

                objreturndbmlBookingStatusTimeSlotView = objServiceClient.BookingStatusGetByServiceIdTimeSlotPropIdWEFDate(intlstServiceId, intTimeSlotId, dtWED);

                if (objreturndbmlBookingStatusTimeSlotView != null && objreturndbmlBookingStatusTimeSlotView.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objreturndbmlBookingStatusTimeSlotView.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, BookingStatusList = objreturndbmlBookingStatusTimeSlotView.objdbmlBookingStatusTimeSlotView }, JsonRequestBehavior.AllowGet);
        }


        [ValidateAntiForgeryToken]
        public ActionResult SubmitEstimateDoc(int intQuotFlag)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";
            returndbmlEstimate objreturndbmlBooking = new returndbmlEstimate();
            try
            {

                if (Session["objdbmlEstimate"] != null)
                {
                    dbmlEstimateView objdbmlBooking = new dbmlEstimateView();
                    GeneralColl<dbmlEstimateView>.CopyObject(Session["objdbmlEstimate"] as dbmlEstimateView, objdbmlBooking);
                    if (intQuotFlag == 1)
                    {
                        objreturndbmlBooking = objServiceClient.EstimateViewGetByEstimateId(objdbmlBooking.EstimateId);
                        if (objreturndbmlBooking.objdbmlStatus.StatusId == 1)
                        {
                            if (objreturndbmlBooking.objdbmlEstimateDetailViewList != null &&
                                objreturndbmlBooking.objdbmlEstimateDetailViewList.Count > 0)
                            {
                                objreturndbmlBooking = objServiceClient.WorkFlowActivityInsertEstimate(objdbmlBooking.EstimateId, Convert.ToInt32(Session["BPId"]), Convert.ToInt32(objdbmlBooking.ZZWorkFlowId), Convert.ToInt32(HardCodeValues.SubmitStatusId), "", Convert.ToInt32(Session["UserId"]));
                                if (objreturndbmlBooking != null && objreturndbmlBooking.objdbmlStatus.StatusId == 1)
                                {
                                    Session["objdbmlEstimate"] = objreturndbmlBooking.objdbmlEstimateList.FirstOrDefault();
                                    intStatusId = 1;
                                    strStatus = "Data Saved Successfully";
                                }
                                else
                                {
                                    strStatus = objreturndbmlBooking.objdbmlStatus.Status;
                                }
                            }
                            else
                            {
                                strStatus = "Track detail not found. Please enter track details";
                            }
                        }
                        else
                        {
                            strStatus = objreturndbmlBooking.objdbmlStatus.Status;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, BookingList = objreturndbmlBooking.objdbmlEstimateList }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Quick Estimate Lab
        public ActionResult EstimateLab()
        {
            CommonModel model = new CommonModel();
            try
            {
                Session["SessBookingType"] = "Lab";

                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                Session["objdbmlEstimate"] = null;

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);

                ViewBag.CompanyDepartment = CompanyDepartmentGetByCustomerMasterId(Convert.ToInt32(Session["ZZCompanyId"]));
            }
            catch
            {
            }

            return View(model);
        }

        public ActionResult LabEstimate()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.TrackGroup = "Quick Estimate Lab";
                model.ViewTitle = "Quick Estimate Lab";
                model.StateId = Convert.ToInt32(Session["StateId"]);

                returndbmlServicesView objreturndbmlServicesView = objServiceClient.ServicesGetByBPId(Convert.ToInt32(HardCodeValues.ServiceBPIdLab));
                if (objreturndbmlServicesView.objdbmlStatus.StatusId == 1 && objreturndbmlServicesView.objdbmlServicesView.Count > 0)
                {
                    Session["LabServices"] = objreturndbmlServicesView.objdbmlServicesView;
                }

                ViewBag.ServiveLookup = GetLabServiveLookup(Convert.ToInt32(HardCodeValues.ServiceBPIdAddOn));

                if (Session["objdbmlEstimate"] != null)
                {
                    dbmlEstimateView objdbmlEstimate = new dbmlEstimateView();
                    GeneralColl<dbmlEstimateView>.CopyObject(Session["objdbmlEstimate"] as dbmlEstimateView, objdbmlEstimate);
                    model.DocDate = objdbmlEstimate.ZZDocDate;
                    model.DocNo = Convert.ToString(objdbmlEstimate.DocNo);
                    model.DocType = "Quick Estimate RFQ";
                    model.WorkFlowId = Convert.ToInt32(objdbmlEstimate.ZZWorkFlowId);
                    model.WorkFlowStatusId = objdbmlEstimate.ZZStatusWorkflowId;
                    model.StatusPropId = Convert.ToInt32(objdbmlEstimate.StatusId);
                    model.DocId = objdbmlEstimate.EstimateId;
                    model.WorkFlowView = WorkFlowViewGetByBPId(Convert.ToInt32(Session["BPId"]), objdbmlEstimate.EstimateId);
                    // model.POURL = strPOURL + objdbmlEstimate.PODocPath;
                    // model.RFQId = Convert.ToInt32(objdbmlEstimate.RFQId);
                    // model.RFQBPId = Convert.ToInt32(objdbmlEstimate.ZZRFQBPId);
                    // model.RFQBookingNo = objdbmlEstimate.ZZRFQBookingNo;
                    // ViewBag.WorkflowRemark = objdbmlEstimate.ZZWorkflowRemark;
                }
                else
                {
                    return RedirectToAction("BasicEstimate", "Home");
                }

            }
            catch
            {
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LabEstimate(CommonModel model, string btnPrevNext)
        {
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (btnPrevNext.ToLower() == "prev")
                {
                    return RedirectToAction("BasicEstimate", "Home");
                }

                if (btnPrevNext.ToLower() == "next")
                {
                    return RedirectToAction("TrackWorkshopEstimate", "Home");
                }
            }
            catch
            {
            }

            return RedirectToAction("TrackWorkshopEstimate", "Home");
        }
        #endregion

        #region Vehicle wise summary
        [ValidateAntiForgeryToken]
        public ActionResult TrackBookingDetailViewFrontGetByBookingIdTrackGroupIdPopup()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";
            int intTrackGroupId = 0;

            returndbmlTrackBookingDetail objreturndbmlTrackBookingDetail = new returndbmlTrackBookingDetail();
            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingView objdbmlBooking = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);

                    objreturndbmlTrackBookingDetail = objServiceClient.TrackBookingDetailViewFrontGetByBookingIdTrackGroupIdPopup(objdbmlBooking.BookingId, intTrackGroupId);
                    if (objreturndbmlTrackBookingDetail.objdbmlStatus.StatusId == 1)
                    {
                        intStatusId = 1;
                        strStatus = "Success";
                    }
                    else
                    {
                        strStatus = objreturndbmlTrackBookingDetail.objdbmlStatus.Status;
                    }
                }
                else
                {
                    strStatus = "Booking Details Not Found";
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, TrackBookingDetailList = objreturndbmlTrackBookingDetail.objdbmlTrackBookingDetail, TrackBookingTimeDetailList = objreturndbmlTrackBookingDetail.objdbmlTrackBookingTimeDetail, TrackBookingTimeSummaryList = objreturndbmlTrackBookingDetail.objdbmlTrackBookingTimeSummary }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Upload AIS-007
        public ActionResult UploadAIS()
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }
            returndbmlBooking objreturndbmlBooking = new returndbmlBooking();
            int intStatusId = 99;
            string strStatus = "Invalid";
            try
            {
                if (Session["objdbmlBooking"] != null)
                {
                    dbmlBookingSupportDocument objdbmlBooking = new dbmlBookingSupportDocument();
                    dbmlBookingView objdbmlBookingView = new dbmlBookingView();
                    GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBookingView);
                    HttpPostedFileBase file = Request.Files["ImageData"];
                    if (file.InputStream.Length > 0)
                    {
                        string strFileExtention = file.ContentType;
                        if (strFileExtention.ToLower() == "image/jpeg" || strFileExtention.ToLower() == "application/pdf")
                        {
                            if ((file.ContentLength / 1024) > 0 && (file.ContentLength / 1024) <= 5120)
                            {
                                strFileExtention = strFileExtention.Substring(strFileExtention.IndexOf('/') + 1);
                                byte[] byteImage = objClassUserFunctions.ConvertToBytes(file.InputStream);
                                bool blnStatus = false;
                                string strFTPUserName = System.Configuration.ConfigurationManager.AppSettings["strFTPUserName"];
                                string strFTPUserPSW = System.Configuration.ConfigurationManager.AppSettings["strFTPUserPassword"];
                                string strFTPUrl = System.Configuration.ConfigurationManager.AppSettings["strFTPServer"];
                                string strFTPRoot = System.Configuration.ConfigurationManager.AppSettings["strFTPRoot"];

                                string strBlobAccount = System.Configuration.ConfigurationManager.AppSettings["strBlobAccount"];
                                string strAccountKey = System.Configuration.ConfigurationManager.AppSettings["strAccountKey"];
                                string strFileStorage = System.Configuration.ConfigurationManager.AppSettings["strFileStorage"];

                                string strImageName = "AIS-007_" + Convert.ToString(objdbmlBookingView.BookingId) + "." + strFileExtention;
                                string strImageContainerName = "bookingsupportdocument";
                                string strImageURL = "";
                                string strFTPFilePath = "";
                                if (strFileStorage == "FTP")
                                {
                                    ////////////////////// For FTP /////////////////////////////////////////////                       
                                    strFTPFilePath = strFTPRoot + strImageName;

                                    blnStatus = objClassUserFunctions.UploadImageToFTPFromWEBCLIENT(strFTPUrl, strFTPUserName, strFTPUserPSW, byteImage, strFTPFilePath);
                                }
                                else if (strFileStorage == "AzureBlob")
                                {
                                    /////////////////////// For Azure Blob ///////////////////////////////////////////////////                            
                                    strImageURL = objClassUserFunctions.UploadFileStreamToAzureBlob(strBlobAccount, strAccountKey, strImageContainerName, strImageName, byteImage);
                                    if (strImageURL != "")
                                        blnStatus = true;
                                }
                                if (blnStatus)
                                {
                                    objdbmlBooking.DocPath = strImageName;
                                    objdbmlBooking.UpdateId = Convert.ToInt32(Session["UserId"]);
                                    objdbmlBooking.updatedate = DateTime.Now;
                                    objdbmlBooking.DocId = objdbmlBookingView.BookingId;
                                    objdbmlBooking.BPId = objdbmlBookingView.BPId;

                                    returndbmlBooking objreturndbmlBookingTemp = new returndbmlBooking();
                                    ObservableCollection<dbmlBookingSupportDocument> objdbmlBookingViewList = new ObservableCollection<dbmlBookingSupportDocument>();
                                    objdbmlBookingViewList.Add(objdbmlBooking);
                                    objreturndbmlBookingTemp.objdbmlBookingSupportDocument = objdbmlBookingViewList;

                                    objreturndbmlBooking = objServiceClient.BookingSupportDocumentInsertUpdate(objreturndbmlBookingTemp);
                                    if (objreturndbmlBooking != null && objreturndbmlBooking.objdbmlStatus.StatusId == 1)
                                    {
                                        Session["objdbmlBooking"] = objreturndbmlBooking.objdbmlBookingList.FirstOrDefault();
                                        intStatusId = 1;
                                        strStatus = "AIS-007 Uploaded Successfully";
                                    }
                                    else
                                    {
                                        if (strFileStorage == "FTP")
                                        {
                                            bool delStatus = objClassUserFunctions.DeleteFileFromFTPFromWEBCLIENT(strFTPUrl, strFTPUserName, strFTPUserPSW, strFTPFilePath);
                                        }
                                        else if (strFileStorage == "AzureBlob")
                                        {
                                            bool delStatus = objClassUserFunctions.DeleteFileFromAzureBlob(strBlobAccount, strAccountKey, strImageContainerName, strImageName);
                                        }

                                        strStatus = "AIS-007 Uploading Process Failed!";
                                        //strStatus = objreturndbmlBooking.objdbmlStatus.Status;
                                    }
                                }
                                else
                                {
                                    strStatus = "AIS-007 Uploading Process Failed!";
                                }
                            }
                            else
                            {
                                strStatus = "AIS-007 File Size between 1KB to 5MB can be accepted!";
                            }
                        }
                        else
                        {
                            strStatus = "Only (JPEG, PDF) format can be accepted!";
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }

            return Json(new { StatusId = intStatusId, Status = strStatus, BookingList = objreturndbmlBooking.objdbmlBookingList }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region
        [ValidateAntiForgeryToken]
        public ActionResult PropertiesGetByPropertyTypeId(int intPropertyTypeId)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";

            returndbmlProperty objreturndbmlProperty = new returndbmlProperty();
            try
            {
                objreturndbmlProperty = objServiceClient.PropertiesGetByPropertyTypeId(intPropertyTypeId);
                if (objreturndbmlProperty.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objreturndbmlProperty.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, PropertyList = objreturndbmlProperty.objdbmlProperty }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Common
        public ActionResult RedirectToActionByStatusId(int intTabStatusId)
        {
            int intBPId = Convert.ToInt32(Session["BPId"]);
            switch (intTabStatusId)
            {
                case 0: return RedirectToAction("Basic", "Home");
                case 10: return RedirectToAction("Vehicle", "Home");
                case 20: return RedirectToAction("Driver", "Home");
                case 30: return RedirectToAction("Attenee", "Home");
                case 40: return RedirectToAction("MainTrackBookingNew", "Home");

                default: return RedirectToAction("Basic", "Home");
            }
        }
        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }
        #endregion

        #region Reports
        public ActionResult VehicleLogs()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (Convert.ToInt32(Session["UserTypePropId"]) == 167)
            {
                ViewBag.Department = LoadDepartmentWithFilter();
            }
            else
            {
                ViewBag.Department = LoadDepartment();
            }
            return View();
        }

        [ValidateAntiForgeryToken]
        public ActionResult VehicleLogsGetByCompanyIdDepartmentIdDate_Reports(dbmlGeneral model)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";
            model.IntOne = Convert.ToInt32(Session["ZZCompanyId"]);
            model.dtFromDate = objClassUserFunctions.ToDateTimeNotNull(model.StrOne);

            returndbmlVehicleLogs objreturndbmlVehicleLogs = new returndbmlVehicleLogs();
            try
            {
                objreturndbmlVehicleLogs = objServiceClient.VehicleLogsGetByCompanyIdDepartmentIdDate_Reports(model);
                if (objreturndbmlVehicleLogs.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objreturndbmlVehicleLogs.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, VehicleLogsList = objreturndbmlVehicleLogs.objdbmlVehicleLogsList }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Print Food Coupon
        public ActionResult PrintFoodCoupon()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (Convert.ToInt32(Session["UserTypePropId"]) == 168)
                {
                    return RedirectToAction("Dashboard", "Home");
                }
                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.BPId = Convert.ToInt32(Session["BPId"]);
                model.ReportURL = strRptURL;
            }
            catch
            {
            }

            return View(model);
        }

        [ValidateAntiForgeryToken]
        public ActionResult BookinglookupforCouponPrintGetByComapnyIdBPId(dbmlGeneral objdbmlGeneral)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";
            returndbmlBookinglookupforCouponPrint objreturndbmlBookinglookupforCouponPrint = new returndbmlBookinglookupforCouponPrint();
            try
            {
                objreturndbmlBookinglookupforCouponPrint = objServiceClient.BookinglookupforCouponPrintGetByComapnyIdBPId(objdbmlGeneral);
                if (objreturndbmlBookinglookupforCouponPrint.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objreturndbmlBookinglookupforCouponPrint.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, Result = objreturndbmlBookinglookupforCouponPrint.objdbmlBookinglookupforCouponPrint }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult BookedCouponDetailGetByBookingId(dbmlGeneral objdbmlGeneral)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            int intStatusId = 99;
            string strStatus = "Invalid";
            returndbmlBookedCouponDetail objReturn = new returndbmlBookedCouponDetail();
            try
            {
                objReturn = objServiceClient.BookedCouponDetailGetByBookingId(objdbmlGeneral);
                if (objReturn.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objReturn.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, Result = objReturn.objdbmlBookedCouponDetail }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CancelFoodCoupon()
        {
            CommonModel model = new CommonModel();
            try
            {
                if (Session["UserId"] == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (Convert.ToInt32(Session["UserTypePropId"]) == 168)
                {
                    return RedirectToAction("Dashboard", "Home");
                }
                model.UserId = Convert.ToInt32(Session["UserId"]);
                model.UserTypePropId = Convert.ToInt32(Session["UserTypePropId"]);
                model.ZZCompanyId = Convert.ToInt32(Session["ZZCompanyId"]);
                model.UserName = Convert.ToString(Session["UserName"]);
                model.EmailId = Convert.ToString(Session["EmailId"]);
                model.LoginId = Convert.ToString(Session["LoginId"]);
                model.ZZUserType = Convert.ToString(Session["ZZUserType"]);
                model.UserCode = Convert.ToString(Session["UserCode"]);
                model.BPId = Convert.ToInt32(Session["BPId"]);
                model.ReportURL = strRptURL;
            }
            catch
            {
            }

            return View(model);
        }

        [ValidateAntiForgeryToken]
        public ActionResult BookinglookupforCouponCancelGetByComapnyIdBPIdDate(dbmlGeneral objdbmlGeneral)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";
            returndbmlBookinglookupforCouponPrint objreturndbmlBookinglookupforCouponPrint = new returndbmlBookinglookupforCouponPrint();
            try
            {
                objreturndbmlBookinglookupforCouponPrint = objServiceClient.BookinglookupforCouponCancelGetByComapnyIdBPIdDate(objdbmlGeneral);
                if (objreturndbmlBookinglookupforCouponPrint.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objreturndbmlBookinglookupforCouponPrint.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, Result = objreturndbmlBookinglookupforCouponPrint.objdbmlBookinglookupforCouponPrint }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Testing
        public ActionResult Test()
        {
            return View();
        }
        #endregion

        [ValidateAntiForgeryToken]
        public ActionResult BookingGetByCompanyIdBPIdPONo(dbmlGeneral model)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";
            string returnDocPath = "";
            dbmlGeneral objdbmlGeneral = new dbmlGeneral();
            returndbmlBooking objreturndbmlBooking = new returndbmlBooking();
            try
            {
                objdbmlGeneral = objServiceClient.BookingGetByCompanyIdBPIdPONo(model);
                if (objdbmlGeneral.StrOne == "PO Not Found.")
                {
                    intStatusId = 99;
                    strStatus = "PO Not Found.";
                }
                else
                {
                    if (Session["objdbmlBooking"] != null)
                    {
                        dbmlBookingView objdbmlBooking = new dbmlBookingView();
                        GeneralColl<dbmlBookingView>.CopyObject(Session["objdbmlBooking"] as dbmlBookingView, objdbmlBooking);
                        string oldPONo = objdbmlGeneral.StrThree;
                        IEnumerable<IListBlobItem> objIListBlobItem = null;
                        string strFileStorage = System.Configuration.ConfigurationManager.AppSettings["strFileStorage"];
                        string strBlobAccount = System.Configuration.ConfigurationManager.AppSettings["strBlobAccount"];
                        string strBlobAccountKey = System.Configuration.ConfigurationManager.AppSettings["strAccountKey"];
                        string strImageContainerName = "booking";
                        if (strFileStorage == "AzureBlob")
                        {
                            objIListBlobItem = GetBlobList(strBlobAccount, strBlobAccountKey, strImageContainerName, oldPONo);
                            if (objIListBlobItem.Count() > 0)
                            {
                                if (oldPONo.Contains('.'))
                                {
                                    string[] Split = oldPONo.Split('.');
                                    byte[] byteData = DownloadBlobFileToStream(strBlobAccount, strBlobAccountKey, strImageContainerName, oldPONo);
                                    string strImageName = "PO_" + Convert.ToString(model.StrTwo) + "." + Split[1];
                                    returnDocPath = UploadFileStreamToAzureBlob(strBlobAccount, strBlobAccountKey, strImageContainerName, strImageName, byteData);
                                    if (returnDocPath != null)
                                    {
                                        ////////// Update ///////////
                                        objdbmlBooking.PODocPath = strImageName;
                                        objdbmlBooking.UpdateId = Convert.ToInt32(Session["UserId"]);
                                        objdbmlBooking.UpdateDate = DateTime.Now;

                                        returndbmlBooking objreturndbmlBookingTemp = new returndbmlBooking();
                                        ObservableCollection<dbmlBookingView> objdbmlBookingViewList = new ObservableCollection<dbmlBookingView>();
                                        objdbmlBookingViewList.Add(objdbmlBooking);
                                        objreturndbmlBookingTemp.objdbmlBookingList = objdbmlBookingViewList;

                                        objreturndbmlBooking = objServiceClient.BookingUpdate(objreturndbmlBookingTemp);
                                        if (objreturndbmlBooking != null && objreturndbmlBooking.objdbmlStatus.StatusId == 1)
                                        {
                                            Session["objdbmlBooking"] = objreturndbmlBooking.objdbmlBookingList.FirstOrDefault();
                                            intStatusId = 1;
                                            strStatus = "PO Uploaded Successfully";
                                        }
                                        else
                                        {
                                            intStatusId = 2;
                                            bool delStatus = objClassUserFunctions.DeleteFileFromAzureBlob(strBlobAccount, strBlobAccountKey, strImageContainerName, strImageName);
                                            strStatus = "PO Uploading Process Failed!";
                                        }
                                        ////////// Update ///////////
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, Result = objdbmlGeneral }, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        public ActionResult PONoGetAll(dbmlGeneral model)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";
            returndbmlPODetailCheck objreturndbmlPODetailCheck = new returndbmlPODetailCheck();
            try
            {
                objreturndbmlPODetailCheck = objServiceClient.PONOGetAll(model);

                if (objreturndbmlPODetailCheck != null && objreturndbmlPODetailCheck.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objreturndbmlPODetailCheck.objdbmlStatus.Status;
                }
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return Json(new { Status = strStatus, StatusId = intStatusId, List = objreturndbmlPODetailCheck.objdbmlPODetailUpload }, JsonRequestBehavior.AllowGet);
        }


        public string UploadFileStreamToAzureBlob(string strAccountName, string strKey, string strContainerName, string strTargetFileName, byte[] byteArraySourceFile)
        {
            string strStatus = string.Empty;
            try
            {

                StorageCredentials sc = new StorageCredentials(strAccountName, strKey);

                CloudStorageAccount storageAccount = new CloudStorageAccount(sc, true);

                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                CloudBlobContainer container = blobClient.GetContainerReference(strContainerName);

                CloudBlockBlob blockBlob = container.GetBlockBlobReference(strTargetFileName);

                MemoryStream ms = new MemoryStream(byteArraySourceFile);

                if (ms != null)
                {
                    blockBlob.UploadFromStream(ms);
                }

                strStatus = blockBlob.Uri.ToString();
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return strStatus;
        }

        public byte[] DownloadBlobFileToStream(string strAccountName, string strKey, string strContainerName, string strFileName)
        {
            Byte[] byteArrayFile;
            try
            {

                StorageCredentials sc = new StorageCredentials(strAccountName, strKey);

                CloudStorageAccount storageAccount = new CloudStorageAccount(sc, true);

                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                CloudBlobContainer container = blobClient.GetContainerReference(strContainerName);

                CloudBlockBlob blockBlob = container.GetBlockBlobReference(strFileName);

                MemoryStream ms = new MemoryStream();
                blockBlob.DownloadToStream(ms);
                byteArrayFile = ms.ToArray();
            }
            catch (Exception ex)
            {
                return null;
            }
            return byteArrayFile;
        }


        public IEnumerable<IListBlobItem> GetBlobList(string strAccountName, string strKey, string strContainerName, string strFileName)
        {
            IEnumerable<IListBlobItem> BlobList;
            try
            {
                StorageCredentials sc = new StorageCredentials(strAccountName, strKey);

                CloudStorageAccount storageAccount = new CloudStorageAccount(sc, true);

                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                CloudBlobContainer container = blobClient.GetContainerReference(strContainerName);

                BlobList = container.ListBlobs(null, false);
            }
            catch (Exception ex)
            {
                return null;
            }
            return BlobList;
        }

 public ActionResult GetFacilities(string serviceName)
        {
            int intStatusId = 99;
            string strStatus = "Invalid";

            ObservableCollection<dbmlServicesView> objdbmlServicesView = new ObservableCollection<dbmlServicesView>();
            IEnumerable<dbmlServicesView> facilities = null;

            returndbmlServicesView objreturndbmlServicesView = objServiceClient.ServicesGetByBPId(Convert.ToInt32(HardCodeValues.ServiceBPIdLab));
            if (objreturndbmlServicesView.objdbmlStatus.StatusId == 1 && objreturndbmlServicesView.objdbmlServicesView.Count > 0)
            {
                facilities = objreturndbmlServicesView.objdbmlServicesView.AsEnumerable().Where(x => x.ServiceName == serviceName);
                if (facilities != null && facilities.Count() > 0)
                {
                    GeneralColl<dbmlServicesView>.CopyCollection(facilities as ObservableCollection<dbmlServicesView>, objdbmlServicesView);

                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = "Facilities Not Found";
                }
            }

            return Json(new { Status = strStatus, StatusId = intStatusId, FacilityList = facilities }, JsonRequestBehavior.AllowGet);
        }



        public ActionResult LabBookingDoc(string serviceName)
        {
            LabModel labModel = new LabModel();
            labModel.labdocs = new List<LabDocModel>();
          
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            int intStatusId = 99;
            string strStatus = "Invalid";
			returndbmlLabServiceDoc objreturndbmlLabServiceDocs = new returndbmlLabServiceDoc();
            
            try
            {
                objreturndbmlLabServiceDocs = objServiceClient.LabBookingRequiredDoc();

                foreach (var item in objreturndbmlLabServiceDocs.objdbmlLabServiceDocs.Where(x => x.ServiceName == serviceName ))
                {
                    LabDocModel model = new LabDocModel();
                    model.ServiceId = item.ServiceId;
                    model.ServiceName = item.ServiceName;
                    model.ServiceSpecification = item.ServiceSpecification;
                    model.DocumentDescription = item.DocumentDescription;
                    model.DocumentName = item.DocumentName;

                    labModel.labdocs.Add(model);
                }


                if (objreturndbmlLabServiceDocs != null && objreturndbmlLabServiceDocs.objdbmlStatus.StatusId == 1)
                {
                    intStatusId = 1;
                    strStatus = "Success";
                }
                else
                {
                    strStatus = objreturndbmlLabServiceDocs.objdbmlStatus.Status;
                }
     
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }

            return PartialView("_LabBookingDoc", labModel.labdocs);
        }



        public ActionResult LabBookingRequiredDoc()
        {
            LabModel labModel = new LabModel();
            if (Session["UserId"] == null)
            {
                return Json(new { Status = "Session Timed Out", StatusId = -99 }, JsonRequestBehavior.AllowGet);
            }

            returndbmlServicesView objreturndbmlServicesView = objServiceClient.ServicesGetByBPId(Convert.ToInt32(HardCodeValues.ServiceBPIdLab));

            int intStatusId = 99;
            string strStatus = "Invalid";
            try
            {
                //ViewBag.ServiveLookup = GetServiveLookup(Convert.ToInt32(HardCodeValues.ServiceBPIdTrack));
                ViewBag.Services = new SelectList(objreturndbmlServicesView.objdbmlServicesView.AsEnumerable().DistinctBy(x => x.ServiceName), "ServiceId", "ServiceName");
                //ViewBag.Facilites = new SelectList(objreturndbmlServicesView.objdbmlServicesView.AsEnumerable().DistinctBy(x => x.ServiceSpecification), "ServiceId", "ServiceSpecification");
            }
            catch (Exception ex)
            {
                strStatus = ex.Message;
            }
            return View(labModel);
        }

        public ActionResult DownloadLabForm(string DocumentName)
        {
            string strFileStorage = System.Configuration.ConfigurationManager.AppSettings["strFileStorage"];
            string strBlobAccount = System.Configuration.ConfigurationManager.AppSettings["strBlobAccount"];
            string strBlobAccountKey = System.Configuration.ConfigurationManager.AppSettings["strAccountKey"];
            string strImageContainerName = "natraximage";
            byte[] bytes = null;

            StorageCredentials sc = new StorageCredentials(strBlobAccount, strBlobAccountKey);

            CloudStorageAccount storageAccount = new CloudStorageAccount(sc, true);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(strImageContainerName);

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(DocumentName);

            using (MemoryStream ms = new MemoryStream())
            {
                blockBlob.DownloadToStream(ms);
                bytes = ms.ToArray();
            }
            //XLWorkbook wb = new XLWorkbook();
            //DocumentName = DocumentName.ToLower();
            //if (DocumentName.Contains(".pdf") || DocumentName.Contains(".jpg") || DocumentName.Contains(".jpeg") || DocumentName.Contains(".png"))
            //{
            //    using (MemoryStream ms = new MemoryStream())
            //    {
            //        blockBlob.DownloadToStream(ms);
            //        bytes = ms.ToArray();
            //    }
            //    return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, DocumentName);
            //}
            //else
            //{
            //    Response.Clear();
            //    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //    Response.AddHeader("content-disposition", String.Format(@"attachment;filename={0}.xlsx", DocumentName));

            //    using (MemoryStream memoryStream = new MemoryStream())
            //    {
            //        wb.SaveAs(memoryStream);
            //        memoryStream.WriteTo(Response.OutputStream);
            //        memoryStream.Close();
            //    }
            //    Response.End();
            //    return File("", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", DocumentName);
            //}

            return File(bytes, "application/octet-stream", DocumentName);
        }
    }
}
