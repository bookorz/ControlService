using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlService.Management
{
    public class Carrier
    {
        //Create Data
        public string CarrierID;
        public int IDStatus;
        public int Capacity;
        public int SlotMapStatus;
        //2.3.1
        public int EqpBasedSlotMapVerify;   //0: Host based, 1: EqpBased
        public int ContentMapList;
        public int SlotMapCapcity;
        public int AccessingStatus;
        public string LocationID;
        public int SubstrateCount;
        public string Usage;
        public int CarrierRejectFlag;
        public int CJ_index;            // for clear specif CJ
        public int LocationNo;          // 1: LP1, 2:LP2, 
        public int used;
        //2.3.1
        public int Command;             // record Command for BIND, Notification
        public int PTN;                 // record Command for BIND, Notification
        public int CarrierIndex = 0;

        public string[] LotID;
        public string[] SubStID;
        public int[] SlotMap;

        //for Local Mode and Offline Mode
        public string[] ReadWaferID;

        public Carrier()
        {
            Command = 0;
            CarrierID = "";
            IDStatus = 0;
            Capacity = 50;
            ContentMapList = 0;
            SlotMapStatus = 0;
            EqpBasedSlotMapVerify = 0;
            SlotMapCapcity = 50;
            AccessingStatus = 0;
            LocationID = "";
            SubstrateCount = 0;
            Usage = "";
            LotID = new string[0];
            SubStID = new string[0];
            SlotMap = new int[0];
            LocationNo = 0;
            used = 0;
            PTN = CarrierManagement.GetNewIndex();

            //for Local Mode and Offline Mode
            ReadWaferID = new string[0];

            CarrierRejectFlag = 0;  // 1:Carrier need reject
            CJ_index = 0;
        }

        public void SetLocation(string LocationID)
        {
            Node TargetNode = NodeManagement.Get(LocationID);
            if (TargetNode != null) {
                if (TargetNode.Carrier != null)
                {
                    CarrierManagement.Remove(TargetNode.Carrier);
                }
                TargetNode.Carrier = this;
            }
            this.LocationID = LocationID;
        }
    }
}
