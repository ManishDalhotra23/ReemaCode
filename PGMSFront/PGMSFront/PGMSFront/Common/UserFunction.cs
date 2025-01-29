using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace PGMSFront.Common
{
    #region Class UserFunctions
    public class ClassUserFunctions
    {
        #region Conversion Functions
        public decimal? ToDecimal(string Val)
        {
            decimal? rval = 0;
            if (Val == null || Val.Trim() == "" || Val.Trim().ToLower().Contains("&nbsp;"))
            {
                rval = null;
            }
            else
            {
                rval = Convert.ToDecimal(Val);
            }
            return rval;
        }

        public decimal? ToDecimal(double? Val)
        {
            decimal? rval = 0;
            if (Val == null)
            {
                rval = null;
            }
            else
            {
                rval = Convert.ToDecimal(Val);
            }
            return rval;
        }

        public decimal ToDecimal(decimal? Val)
        {
            decimal rval = Convert.ToDecimal(0.00);

            if (Val != null)
            {
                rval = Convert.ToDecimal(Val);
            }
            return rval;
        }

        public string ToString(decimal? Val)
        {
            string rval = null;
            if (Val == null)
            {
                rval = null;
            }
            else
            {
                rval = Convert.ToString(Val);
            }
            return rval;
        }

        public Int32? ToInt32(string Val)
        {
            Int32? rval = 0;
            if (Val == null || Val.Trim() == "" || Val.Trim().ToLower().Contains("&nbsp;") || Val.Trim().ToLower() == "select" || Val.Trim() == "0")
            {
                rval = null;
            }
            else
            {
                rval = Convert.ToInt32(Val);
            }
            return rval;
        }

        public string ToString(int? Val)
        {
            string rval = null;
            if (Val == null)
            {
                rval = null;
            }
            else
            {
                rval = Convert.ToString(Val);
            }
            return rval;
        }

        public string ToString(double? Val)
        {
            string rval = null;
            if (Val == null)
            {
                rval = null;
            }
            else
            {
                rval = Convert.ToString(Val);
            }
            return rval;
        }

        public string ToString(bool? Val)
        {
            string rval = null;
            if (Val == null)
            {
                rval = null;
            }
            else
            {
                rval = Convert.ToString(Val);
            }
            return rval;
        }

        public string getDate_DDMMYYYY(string strDate)
        {
            string cDate = null;
            if (strDate == null || strDate.Trim() == "" || strDate.Trim().ToLower().Contains("&nbsp;"))
            {
                cDate = null;
            }
            else
            {
                cDate = Convert.ToDateTime(strDate).ToString("dd/MM/yyyy");
            }
            return cDate;
        }

        public string getDate_DDMMYYYY(DateTime? dtDate)
        {
            string cDate = null;
            if (dtDate == null)
            {
                cDate = null;
            }
            else
            {
                cDate = Convert.ToDateTime(dtDate).ToString("dd/MM/yyyy");
            }
            return cDate;
        }

        public string getTimeHHMMTT(DateTime? dtDate)
        {
            string cTime = null;
            if (dtDate != null)
            {
                cTime = Convert.ToDateTime(dtDate).ToString("hh:mm tt");
            }
            return cTime;
        }

        public string getDate_MMDDYYYY(string strDate)
        {
            string cDate = null;
            if (strDate == null || strDate.Trim() == "" || strDate.Trim().ToLower().Contains("&nbsp;"))
            {
                cDate = null;
            }
            else
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
                DateTime dtProjectStartDate = Convert.ToDateTime(strDate);
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                cDate = (Convert.ToDateTime(dtProjectStartDate).ToString("MM/dd/yyyy"));
            }
            return cDate;
        }

        public DateTime? ToDateTime(string strDate)
        {
            DateTime? cDate = null;
            if (strDate == null || strDate.Trim() == "" || strDate.Trim().ToLower().Contains("&nbsp;"))
            {
                cDate = null;
            }
            else
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
                DateTime dtProjectStartDate = Convert.ToDateTime(strDate);
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                cDate = Convert.ToDateTime((Convert.ToDateTime(dtProjectStartDate).ToString("MM/dd/yyyy")));
            }
            return cDate;
        }

        public DateTime ToDateTimeNotNull(string strDate)
        {
            DateTime cDate;
            if (strDate == null || strDate.Trim() == "" || strDate.Trim().ToLower().Contains("&nbsp;"))
            {
                cDate = DateTime.Now.Date;
            }
            else
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
                DateTime dtProjectStartDate = Convert.ToDateTime(strDate);
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                cDate = Convert.ToDateTime((Convert.ToDateTime(dtProjectStartDate).ToString("MM/dd/yyyy")));
            }
            return cDate;
        }

        public DateTime? ToDateTime(string strDate, string strTime)
        {
            DateTime? cDate = null;
            if (strDate == null || strDate.Trim() == "" || strTime.Trim() == "" || strDate.Trim().ToLower().Contains("&nbsp;"))
            {
                cDate = null;
            }
            else
            {
                string[] strReportTime = strTime.Split(':', ' ');
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
                DateTime dtProjectStartDate = Convert.ToDateTime(strDate);
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                cDate = DateTime.Parse(string.Format("{0} {1}:{2}:{3} {4}", dtProjectStartDate.ToString("MM/dd/yyyy"), strReportTime[0], strReportTime[1], "00", strReportTime[2]));
            }
            return cDate;
        }

        public byte[] ConvertToBytes(Stream InputStream)
        {
            byte[] imageBytes = null;
            BinaryReader reader = new BinaryReader(InputStream);
            imageBytes = reader.ReadBytes((int)InputStream.Length);
            return imageBytes;
        }

        public T ToObjectFromJSON<T>(string jsonString)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            var memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(jsonString));
            var newObject = (T)serializer.ReadObject(memoryStream);
            memoryStream.Close();
            return newObject;
        }

        public bool ValidateInvoiceDate(string strInDateddMMyyyy)
        {
            bool blnStatus = true;
            if (strInDateddMMyyyy != null && strInDateddMMyyyy.Trim() != "")
            {
                string strDate = "01/07/2017";
                DateTime ValidDate = ToDateTimeNotNull(strDate);
                DateTime dtInDate = ToDateTimeNotNull(strInDateddMMyyyy);

                if (dtInDate < ValidDate)
                {
                    blnStatus = false;
                }
            }
            return blnStatus;
        }

        public bool ValidateDocumentDate(string strInDateddMMyyyy)
        {
            bool blnStatus = true;
            if (strInDateddMMyyyy != null && strInDateddMMyyyy.Trim() != "")
            {
                string strDate = "01/07/2017";
                DateTime ValidDate = ToDateTimeNotNull(strDate);
                DateTime dtInDate = ToDateTimeNotNull(strInDateddMMyyyy);

                if (dtInDate < ValidDate)
                {
                    blnStatus = false;
                }
            }
            return blnStatus;
        }

        #endregion

        #region FTP Functions

        private FtpWebRequest ftpRequest = null;
        private FtpWebResponse ftpResponse = null;
        private Stream ftpStream = null;
        private int bufferSize = 150000;

        public bool UploadFileToFTPFromWEBCLIENT(string ftpurl, string ftpusername, string ftppassword, byte[] inputStream, string strFTPFilePath)
        {
            bool upldStatus = false;

            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpurl + strFTPFilePath);
                ftpRequest.Credentials = new NetworkCredential(ftpusername, ftppassword);

                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = false;
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                // Stream straemIn = inputStream;
                byte[] buffer = inputStream;
                //straemIn.Read(buffer, 0, buffer.Length);
                //straemIn.Close();

                ftpStream = ftpRequest.GetRequestStream();
                ftpStream.Write(buffer, 0, buffer.Length);
                ftpStream.Close();

                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();

                //response.StatusDescription      

                response.Close();
                upldStatus = true;
                ftpRequest = null;
            }
            catch (Exception)
            {

            }

            return upldStatus;

        }

        public bool UploadImageToFTPFromWEBCLIENT(string ftpurl, string ftpusername, string ftppassword, byte[] inputStream, string strFTPFilePath)
        {
            bool upldStatus = false;

            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpurl + strFTPFilePath);
                ftpRequest.Credentials = new NetworkCredential(ftpusername, ftppassword);

                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = false;
                ftpRequest.KeepAlive = true;
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                //FileStream fs = File.OpenRead(strSorceFilePath);
                //byte[] buffer = new byte[fs.Length];
                //fs.Read(buffer, 0, buffer.Length);
                //fs.Close();

                // ftpStream = inputStream;
                byte[] buffer = inputStream;
                //ftpStream.Read(buffer, 0, buffer.Length);
                //ftpStream.Close();

                Stream ftpstream = ftpRequest.GetRequestStream();
                ftpstream.Write(buffer, 0, buffer.Length);
                ftpstream.Close();

                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

                //response.StatusDescription      

                ftpResponse.Close();
                upldStatus = true;
                ftpRequest = null;
            }
            catch (Exception ex)
            {
            }

            return upldStatus;

        }

        public byte[] DownloadImageFromFTPFromWEBCLIENT(string ftpurl, string ftpusername, string ftppassword, string strFTPFilePath)
        {

            long intFileSize = GetFileSizeFromFTPFromWEBCLIENT(ftpurl, ftpusername, ftppassword, strFTPFilePath);
            byte[] bytePhoto = new byte[intFileSize];

            if (intFileSize > 0)
            {
                bufferSize = (Int32)intFileSize;
            }

            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpurl + strFTPFilePath);
                ftpRequest.Credentials = new NetworkCredential(ftpusername, ftppassword);
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = false;
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                // Get the object used to communicate with the server.
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                ftpStream = ftpResponse.GetResponseStream();

                //int bytesRead = 0;
                byte[] buffer = new byte[bufferSize];

                //Stream streamFile = File.Create(@"c:\\aa.jpg");
                //while (true)
                //{

                //    bytesRead = ftpStream.Read(buffer, 0, bufferSize);
                //    streamFile.Write(buffer, 0, bytesRead);
                //    if (bytesRead == 0)
                //        break;

                //}

                int contentLength = ftpStream.Read(buffer, 0, bufferSize);

                while (contentLength != 0)
                {
                    contentLength = ftpStream.Read(buffer, 0, bufferSize);
                }

                bytePhoto = buffer;

                ftpStream.Close();
                ftpResponse.Close();
                ftpRequest = null;
            }
            catch (Exception)
            {

            }

            return bytePhoto;

        }

        public bool CreateDirectoryOnFTPFromWEBCLIENT(string ftpurl, string ftpusername, string ftppassword, string strNewDirectory)
        {

            bool Status = false;

            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpurl + strNewDirectory);
                ftpRequest.Credentials = new NetworkCredential(ftpusername, ftppassword);

                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = false;
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Resource Cleanup */

                ftpResponse.Close();
                ftpRequest = null;
                Status = true;
            }
            catch (Exception)
            {
            }

            return Status;
        }

        public bool DeleteFileFromFTPFromWEBCLIENT(string ftpurl, string ftpusername, string ftppassword, string strFTPFilePath)
        {
            bool Status = false;

            try
            {
                if (GetFileSizeFromFTPFromWEBCLIENT(ftpurl, ftpusername, ftppassword, strFTPFilePath) > 0)
                {
                    ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpurl + strFTPFilePath);
                    ftpRequest.Credentials = new NetworkCredential(ftpusername, ftppassword);

                    /* When in doubt, use these options */
                    ftpRequest.UseBinary = true;
                    ftpRequest.UsePassive = true;
                    ftpRequest.KeepAlive = false;
                    ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                    /* Establish Return Communication with the FTP Server */
                    ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

                    /* Resource Cleanup */
                    ftpResponse.Close();
                    ftpRequest = null;
                    Status = true;
                }
                else
                {
                    Status = true;
                }
            }
            catch (Exception)
            {

            }
            return Status;
        }

        public long GetFileSizeFromFTPFromWEBCLIENT(string ftpurl, string ftpusername, string ftppassword, string strFTPFilePath)
        {
            long size = 0;

            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ftpurl + strFTPFilePath);
                ftpRequest.Credentials = new NetworkCredential(ftpusername, ftppassword);

                /* When in doubt, use these options */
                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = false;
                /* Specify the Type of FTP Request */
                ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                /* Establish Return Communication with the FTP Server */
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */

                FtpWebResponse loginresponse = (FtpWebResponse)ftpRequest.GetResponse();
                FtpWebResponse respSize = (FtpWebResponse)ftpRequest.GetResponse();
                respSize = (FtpWebResponse)ftpRequest.GetResponse();
                size = respSize.ContentLength;

                ftpResponse.Close();
                loginresponse.Close();
                respSize.Close();
                ftpRequest = null;
                /* Return File Size */
            }
            catch (Exception)
            {

            }

            return size;

        }


        #endregion

        #region Blob Functions

        ////UploadFileStreamToAzureBlob("portalvhds2p3pblpxrbwgb", "+cNEOA8gjQmAPMZ/cc+MtSmG/XGoGWfqEvvVmshFNyrMQx/eJr3WeKOR2EQDCXXMkYurrnlp5novBs0yEap5kg==", "vhds", "SubhashIMG1.jpg", btArrayFile);
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

                if (strTargetFileName != String.Empty &&
                    (strTargetFileName.Contains(".jpeg") || strTargetFileName.Contains(".jpg") || strTargetFileName.Contains(".png")))
                {
                    blockBlob.Properties.ContentType = "image/jpg";
                }else if(strTargetFileName != String.Empty && strTargetFileName.Contains(".pdf"))
                {
                    blockBlob.Properties.ContentType = "application/pdf";
                }
                

                MemoryStream ms = new MemoryStream(byteArraySourceFile);

                if (ms != null)
                {
                    blockBlob.UploadFromStream(ms);
                }

                strStatus = blockBlob.Uri.ToString();
            }
            catch (Exception ex)
            {
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

        public bool DeleteFileFromAzureBlob(string strAccountName, string strKey, string strContainerName, string strFileName)
        {
            bool blnStatus = false;
            try
            {
                StorageCredentials sc = new StorageCredentials(strAccountName, strKey);

                CloudStorageAccount storageAccount = new CloudStorageAccount(sc, true);

                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                CloudBlobContainer container = blobClient.GetContainerReference(strContainerName);

                CloudBlockBlob blockBlob = container.GetBlockBlobReference(strFileName);

                blnStatus = blockBlob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);

            }
            catch (Exception ex)
            {

            }
            return blnStatus;
        }

        #endregion

        #region Send Mail
        public bool SendMailMessage(string strFrom, string strPSW, string strTo, string strReplyTo, string strBcc, string strCc, string strSubject, string strBody, Stream streamAtt, string strAttFileName)
        {
            bool blnMailSentStatus = false;

            try
            {
                MailMessage mMailMessage = new MailMessage();
                mMailMessage.From = new MailAddress(strFrom);
                mMailMessage.To.Add(new MailAddress(strTo));
                if (strReplyTo != "")
                {
                    mMailMessage.ReplyTo = new MailAddress(strReplyTo);
                }

                if ((strBcc != null) && (strBcc != string.Empty))
                {
                    mMailMessage.Bcc.Add(new MailAddress(strBcc));
                }

                if ((strCc != null) && (strCc != string.Empty))
                {
                    mMailMessage.CC.Add(new MailAddress(strCc));
                }
                mMailMessage.Subject = strSubject;
                mMailMessage.Body = strBody;
                mMailMessage.IsBodyHtml = true;
                mMailMessage.Priority = MailPriority.Normal;

                if (streamAtt != null && streamAtt.Length > 0)
                {
                    if (String.IsNullOrEmpty(strAttFileName))
                        strAttFileName = "Attachment1";
                    mMailMessage.Attachments.Add(new Attachment(streamAtt, strAttFileName));
                }

                SmtpClient mSmtpClient = new SmtpClient();

                // mSmtpClient.Timeout = 20000;
                mSmtpClient.EnableSsl = true;
                // mSmtpClient.UseDefaultCredentials = false;
                // mSmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                mSmtpClient.Credentials = new NetworkCredential(strFrom, strPSW);

                mSmtpClient.Host = "smtp.gmail.com";
                mSmtpClient.Port = 587;
                mSmtpClient.Send(mMailMessage);
                blnMailSentStatus = true;

            }
            catch (Exception ex)
            {
                string strEx = ex.Message;
            }
            return blnMailSentStatus;
        }
        #endregion

        #region Excel Import
        public DataTable GetDataTableFromSpreadsheet(Stream MyExcelStream, bool ReadOnly)
        {
            DataTable dt = new DataTable();
            using (SpreadsheetDocument sDoc = SpreadsheetDocument.Open(MyExcelStream, ReadOnly))
            {
                WorkbookPart workbookPart = sDoc.WorkbookPart;
                IEnumerable<Sheet> sheets = sDoc.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                string relationshipId = sheets.First().Id.Value;
                WorksheetPart worksheetPart = (WorksheetPart)sDoc.WorkbookPart.GetPartById(relationshipId);
                Worksheet workSheet = worksheetPart.Worksheet;
                SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                IEnumerable<Row> rows = sheetData.Descendants<Row>();

                foreach (Cell cell in rows.ElementAt(0))
                {
                    dt.Columns.Add(GetCellValue(sDoc, cell));
                }

                foreach (Row row in rows) //this will also include your header row...
                {
                    DataRow tempRow = dt.NewRow();
                    int columnIndex = 0;
                    foreach (Cell cell in row.Descendants<Cell>())
                    {
                        // Gets the column index of the cell with data
                        int cellColumnIndex = (int)GetColumnIndexFromName(GetColumnName(cell.CellReference));
                        cellColumnIndex--; //zero based index
                        if (columnIndex < cellColumnIndex)
                        {
                            do
                            {
                                tempRow[columnIndex] = ""; //Insert blank data here;
                                columnIndex++;
                            }
                            while (columnIndex < cellColumnIndex);
                        }
                        tempRow[columnIndex] = GetCellValue(sDoc, cell);

                        columnIndex++;
                    }
                    dt.Rows.Add(tempRow);
                }
            }
            dt.Rows.RemoveAt(0);
            return dt;
        }

        public static string GetColumnName(string cellReference)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellReference);
            return match.Value;
        }

        public static int? GetColumnIndexFromName(string columnName)
        {

            //return columnIndex;
            string name = columnName;
            int number = 0;
            int pow = 1;
            for (int i = name.Length - 1; i >= 0; i--)
            {
                number += (name[i] - 'A' + 1) * pow;
                pow *= 26;
            }
            return number;
        }

        public static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            if (cell.CellValue == null)
            {
                return "";
            }
            string value = cell.CellValue.InnerXml;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else
            {
                return value;
            }
        }

        public string ConvertDataTableToHTMLTable(DataTable dt)
        {
            string ret = "";
            ret = "<table id=" + (char)34 + "tblExcel" + (char)34 + ">";
            ret += "<tr>";
            foreach (DataColumn col in dt.Columns)
            {
                ret += "<td class=" + (char)34 + "tdColumnHeader" + (char)34 + ">" + col.ColumnName + "</td>";
            }
            ret += "</tr>";
            foreach (DataRow row in dt.Rows)
            {
                ret += "<tr>";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    ret += "<td class=" + (char)34 + "tdCellData" + (char)34 + ">" + row[i].ToString() + "</td>";
                }
                ret += "</tr>";
            }
            ret += "</table>";
            return ret;
        }
        #endregion

    }
    #endregion

    #region HardCodedFunctionsVariables
    public enum HardCodeValues
    {
        ServiceBPIdLab = 11,
        ServiceBPIdTrack = 12,
        ServiceBPIdWorkShop = 13,
        ServiceBPIdAddOn = 37,
        TimeSlotPropId = 9,

        BookingBPId = 21,
        RFQConfBPId = 98,
        RFQRegBPId = 46,
        BookingWFId = 6,
        RFQConfWFId = 57,
        RFQRegWFId = 29,

        LabBookingBPId = 90,      
        LabRFQRegBPId = 91,
        LabBookingWFId = 36,       
        LabRFQRegWFId = 38,

        VehicleGrpPropId = 99,
        VehicleTypePropId = 14,
        CompGrpPropId = 98,
        CompOptPropId = 97,

        OpenStatusId =40,  
        ApproveStatusId =38,
        UnApproveStatusId=39,
        SubmitStatusId=41,
        RevertStatusId=43,
        RejectStatusId=45,

        ServiceBPIdEstimateTrack = 12,
        ServiceBPIdEstimateWorkShop = 13,
        ServiceBPIdEstimateAddon = 37,
        ServiceBPIdEstimateLab = 11,

        EstimateCreateWFId = 216,
        EstimateLabCreateWFId = 219,
    };

    public static class HardCodeList
    {
        public static List<SelectListItem> GetMonth()
        {
            List<SelectListItem> objMonthList = new List<SelectListItem>();
            objMonthList.Add(new SelectListItem() { Value = "1", Text = "Jan", Selected = false });
            objMonthList.Add(new SelectListItem() { Value = "2", Text = "Feb", Selected = false });
            objMonthList.Add(new SelectListItem() { Value = "3", Text = "Mar", Selected = false });
            objMonthList.Add(new SelectListItem() { Value = "4", Text = "Apr", Selected = false });
            objMonthList.Add(new SelectListItem() { Value = "5", Text = "May", Selected = false });
            objMonthList.Add(new SelectListItem() { Value = "6", Text = "Jun", Selected = false });
            objMonthList.Add(new SelectListItem() { Value = "7", Text = "Jul", Selected = false });
            objMonthList.Add(new SelectListItem() { Value = "8", Text = "Aug", Selected = false });
            objMonthList.Add(new SelectListItem() { Value = "9", Text = "Sept", Selected = false });
            objMonthList.Add(new SelectListItem() { Value = "10", Text = "Oct", Selected = false });
            objMonthList.Add(new SelectListItem() { Value = "11", Text = "Nov", Selected = false });
            objMonthList.Add(new SelectListItem() { Value = "12", Text = "Dec", Selected = false });
            return objMonthList;
        }
    }
    #endregion

    #region General Coll
    public class GeneralColl<T> where T : new()
    {
        public static void CopyObject(T a, T b)
        {
            if (b == null)
                b = new T();
            foreach (PropertyInfo property in a.GetType().GetProperties())
            {
                object objValue = property.GetValue(a, null);
                if (objValue != null)
                    property.SetValue(b, objValue, null);
            }
        }

        public static void CopyCollection(ObservableCollection<T> objSourceList, ObservableCollection<T> objDestinationList)
        {
            if (objSourceList == null)
                return;
            if (objDestinationList != null)
                objDestinationList.Clear();
            foreach (T a in objSourceList)
            {
                T b = new T();
                CopyObject(a, b);
                objDestinationList.Add(b);
            }
        }

        public static void CopyList(List<T> objSourceList, List<T> objDestinationList)
        {
            if (objSourceList == null)
                return;
            if (objDestinationList != null)
                objDestinationList.Clear();
            foreach (T a in objSourceList)
            {
                T b = new T();
                CopyObject(a, b);
                objDestinationList.Add(b);
            }
        }

        public static DataSet ToDataSetFromListObject(ObservableCollection<T> objList)
        {
            Type ElementType = typeof(T);
            System.Data.DataSet ds = new System.Data.DataSet();
            System.Data.DataTable dt = new System.Data.DataTable();

            foreach (var profInfo in ElementType.GetProperties())
            {
                dt.Columns.Add(profInfo.Name);
            }

            foreach (var itm in objList)
            {
                System.Data.DataRow dr = dt.NewRow();
                foreach (var profInfo in ElementType.GetProperties())
                {
                    dr[profInfo.Name] = profInfo.GetValue(itm, null);
                }
                dt.Rows.Add(dr);
            }
            ds.Tables.Add(dt);
            return ds;
        }
    }

    #endregion
}