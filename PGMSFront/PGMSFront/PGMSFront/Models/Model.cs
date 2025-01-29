
using PGMSFront.WCFPGMSRef;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGMSFront.Models {
    public class LoginModel
    {
        public string LoginId { get; set; }
        public string Password { get; set; }
        public string Message { get; set; }

    }
    public class CommonModel
    {
        public string LoginId { get; set; }
        public int UserId { get; set; }
        public int UserTypePropId { get; set; }
        public int ZZCompanyId { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string ZZUserType { get; set; }
        public string UserCode { get; set; }
        public string Message { get; set; }
        public int StatusPropId { get; set; }
        public int TrackGroupId { get; set; }
        public string TrackGroup { get; set; }
        public string ViewTitle { get; set; }
        public string DocDate { get; set; }
        public string DocType { get; set; }
        public string DocNo { get; set; }
        public string ZZDocNo { get; set; }
        public int WorkFlowId { get; set; }
        public int? WorkFlowStatusId { get; set; }
        public int StateId { get; set; }
        public int DocId { get; set; }
        public int BPId { get; set; }
        public string ReportURL { get; set; }
        public string POURL { get; set; }
        public int RFQId { get; set; }
        public int RFQBPId{ get; set; }
        public string RFQBookingNo { get; set; }
        public int TrackBookingTimeDetailId { get; set; }
        public int WorkshopBookingDetailId { get; set; }
        public int BookingAddOnServiceId { get; set; }
        public int EstimateDetailId { get; set; }
        public int LabBookingDetailId { get; set; }
        public int PWDenterdStatusId { get; set; }
        public string SupportDocPathURL { get; set; }
        public int BookingId { get; set; }

        public ObservableCollection<dbmlWorkFlowView> WorkFlowView = new ObservableCollection<dbmlWorkFlowView>();

    }
}
