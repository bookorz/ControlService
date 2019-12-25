using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlService.Management
{
    public class Job
    {
        ILog logger = LogManager.GetLogger(typeof(Job));
        public string Slot { get; set; }
        public string MappingValue { get; set; }
        public string Job_Id { get; set; }
        public string Host_Job_Id { get; set; }
        public string Host_Lot_Id { get; set; }
        public List<OCRInfo> OcrCodeList { get; set; }
        public bool InProcess { get; set; }
        public bool NeedProcess { get; set; }
        public bool AbortProcess { get; set; }
        public bool IsReversed { get; set; }
        public bool ProcessFlag { get; set; }
        public bool AlignerFlag { get; set; }
        public bool OCRFlag { get; set; }
        public string OCRImgPath { get; set; }
        public string OCRScore { get; set; }
        public string OCRResult { get; set; }
        public bool OCRPass { get; set; }
        public string OCR_M12_ImgPath { get; set; }
        public string OCR_M12_Score { get; set; }
        public string OCR_M12_Result { get; set; }
        public bool OCR_M12_Pass { get; set; }
        public string OCR_T7_ImgPath { get; set; }
        public string OCR_T7_Score { get; set; }
        public string OCR_T7_Result { get; set; }
        public bool OCR_T7_Pass { get; set; }
        public string Position { get; set; }
        public string FromFoupID { get; set; }
        public string ToFoupID { get; set; }
        public string FromPort { get; set; }
        public string ToPort { get; set; }
        public string FromPortSlot { get; set; }
        public string ToPortSlot { get; set; }
        public string Destination { get; private set; }
        public string DisplayDestination { get; private set; }
        public string DestinationSlot { get; private set; }
        public string ReservePort { get; set; }
        public string ReserveSlot { get; set; }
        public string LastNode { get; set; }
        public string LastSlot { get; set; }
        public string WaitToDo { get; set; }
        public string RecipeID { get; set; }
        public bool ErrPosition { get; set; }
        public bool MapFlag { get; set; }
        public int Offset { get; set; }
        public bool IsAssigned { get; set; }
        public bool Locked { get; set; }
        public DateTime AssignTime { get; private set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string WaferSize { get; set; }
        IJobReport _Report = null;

        public Job(IJobReport Report)
        {
            _Report = Report;
            Job_Id = "";
            Host_Lot_Id = "";
            WaitToDo = "";
            Destination = "";
            DestinationSlot = "";
            OCRImgPath = "";
            OCR_M12_ImgPath = "";
            OCR_T7_ImgPath = "";
            RecipeID = "";
            ProcessFlag = false;
            MapFlag = false;
            ReservePort = "";
            ReserveSlot = "";
            FromFoupID = "";
            ToFoupID = "";
            AlignerFlag = false;
            NeedProcess = false;
            AbortProcess = false;
            OCRFlag = false;
            InProcess = false;
            IsAssigned = false;
            Locked = false;
            OcrCodeList = new List<OCRInfo>();
            ToPort = "";
            ToPortSlot = "";
            IsReversed = false;
            WaferSize = "";
            MappingValue = "";
        }

        public void PositionChangeReport()
        {
            if(this.Position.ToUpper().Equals(this.Destination.ToUpper()) && this.Slot.Equals(this.DestinationSlot))
            {
                this.Destination = "";
                this.DestinationSlot = "";
            }
            if (_Report != null)
            {
                _Report.On_Job_Position_Changed(this);
            }
        }

        public bool AssignPort(string Position, string Slot)
        {
            Node targetPort = NodeManagement.Get(Position);
            if (targetPort == null)
            {
                return false;
            }
            Job targetSlot;
            if (targetPort.JobList.TryGetValue(Slot, out targetSlot))
            {
                this.FromPort = this.Position;
                this.FromPortSlot = this.Slot;
                this.Destination = Position;
                this.DisplayDestination = Position.Replace("Load", "");
                this.DestinationSlot = Slot;
                this.AssignTime = DateTime.Now;
                this.NeedProcess = true;
                this.AbortProcess = false;
                this.AlignerFlag = false;
                //設定UnloadPort的補償角度
                string ULDRobot = NodeManagement.Get(this.Destination).Associated_Node;
               
                RobotPoint point = PointManagement.GetPoint(ULDRobot, Position);
                if (point != null)
                {
                    this.Offset = point.Offset;
                }
                else
                {
                    logger.Error("Job AssignPort: point not exist. Node:" + ULDRobot + " Position:" + Position);
                }
                targetSlot.ReservePort = this.Position;
                targetSlot.ReserveSlot = this.Slot;
                targetSlot.IsAssigned = true;
            }
            else
            {
                return false;
            }
            return true;
        }

        public void UnAssignPort()
        {
            Node targetPort = NodeManagement.Get(this.Destination);
            if (targetPort != null)
            {
                Job targetSlot;
                if (targetPort.JobList.TryGetValue(this.DestinationSlot, out targetSlot))
                {
                    targetSlot.ReservePort = "";
                    targetSlot.ReserveSlot = "";
                    targetSlot.IsAssigned = false;
                }
            }
            this.Destination = "";
            this.DisplayDestination = "";
            this.DestinationSlot = "";
            this.NeedProcess = false;
            this.IsReversed = false;
        }

        public class State
        {
            public const string WAIT_PUT = "WAIT_PUT";
            public const string WAIT_WHLD = "WAIT_WHLD";
            public const string WAIT_ALIGN = "WAIT_ALIGN";
            public const string WAIT_OCR = "WAIT_OCR";
            public const string WAIT_WRLS = "WAIT_WRLS";
            public const string WAIT_GET = "WAIT_GET";
            public const string WAIT_RET = "WAIT_RET";
            public const string WAIT_UNLOAD = "WAIT_UNLOAD";
        }
    }
}
