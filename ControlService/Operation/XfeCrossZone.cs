using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ControlService.Config;
using ControlService.Engine;
using ControlService.Management;


namespace ControlService.Operation
{
    public class XfeCrossZone
    {
        private ILog logger = LogManager.GetLogger(typeof(XfeCrossZone));
        public static bool Running = false;

        public string LDRobot = "";
        public string LDRobot_Arm = "";
        public string ULDRobot = "";
        public string ULDRobot_Arm = "";
        public string LD = "";
        public List<string> ULD_List = new List<string>();
        public string tmpULD = "";
        public double ProcessTime = 0;
        public double ProcessCount = 0;
        public bool SingleAligner = false;
        bool IsTmpConfig = false;
        private string RunID = "";
        System.Diagnostics.Stopwatch watch;
        Recipe rcp = null;
        IXfeStateReport _Report;

        public XfeCrossZone(IXfeStateReport Report)
        {
            _Report = Report;
            RunID = Guid.NewGuid().ToString();

            //ThreadPool.QueueUserWorkItem(new WaitCallback(Engine), "ROBOT01");
            //ThreadPool.QueueUserWorkItem(new WaitCallback(Engine), "ROBOT02");
            //ThreadPool.QueueUserWorkItem(new WaitCallback(Engine), "ALIGNER01");
            //ThreadPool.QueueUserWorkItem(new WaitCallback(Engine), "ALIGNER02");
            foreach (Node node in NodeManagement.GetList())
            {
                if (node.Type.ToUpper().Equals("ROBOT") || node.Type.ToUpper().Equals("ALIGNER") || node.Type.ToUpper().Equals("LOADPORT"))
                {
                    if (node.Enable)
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(Engine), node.Name.ToUpper());
                    }
                }
            }

        }

        public void Initial()
        {
            logger.Debug("XfeCrossZone Initial");
            Running = false;

            LDRobot = "";
            LDRobot_Arm = "";
            ULDRobot = "";
            ULDRobot_Arm = "";
            LD = "";
            ULD_List = new List<string>();
            tmpULD = "";
            ProcessTime = 0;
            ProcessCount = 0;
            SingleAligner = false;
            RunID = "";
            rcp = Recipe.Get(SystemConfig.Get().CurrentRecipe);
        }
        private void RefreshConfig()
        {
            Dictionary<string, string> tmpParam = new Dictionary<string, string>();
            tmpParam.Add("@Alinger1Speed", Recipe.Get(SystemConfig.Get().CurrentRecipe).aligner1_speed);
            tmpParam.Add("@Robot1Speed", Recipe.Get(SystemConfig.Get().CurrentRecipe).robot1_speed);
            TaskFlowManagement.Excute(Guid.NewGuid().ToString(), TaskFlowManagement.Command.SET_ALL_SPEED, tmpParam);
            foreach (Node Al in NodeManagement.GetList())
            {

                switch (Al.Name.ToUpper())
                {
                    case "ALIGNER01":
                        Al.Enable = Recipe.Get(SystemConfig.Get().CurrentRecipe).is_use_aligner1;
                        break;
                    case "ALIGNER02":
                        Al.Enable = Recipe.Get(SystemConfig.Get().CurrentRecipe).is_use_aligner2;
                        break;
                }

            }
        }
        public bool Start(string LDPort, bool UseTmpConfig = false)
        {
            if (Running)
            {
                IsTmpConfig = false;
                Recipe.Get(SystemConfig.Get().CurrentRecipe).Reload();
                RefreshConfig();
                return false;
            }
            Initial();
            if (UseTmpConfig)
            {
                IsTmpConfig = true;
                RefreshConfig();
            }
            watch = System.Diagnostics.Stopwatch.StartNew();
            //開始前先重設
            foreach (Node each in NodeManagement.GetList())
            {
                each.RequestQueue.Clear();
                each.LockOn = "";
                each.ReadyForGet = true;
                each.ReadyForPut = true;
            }
            LDRobot = "";
            LDRobot_Arm = "";

            ULDRobot_Arm = "";
            LD = "";
            ULD_List.Clear();
            //找到LD
            Node nodeLD = NodeManagement.Get(LDPort);
            if (nodeLD == null)
            {
                logger.Error("XfeCrossZone Start fail:Node " + LDPort + " not found");
                IsTmpConfig = false;
                Recipe.Get(SystemConfig.Get().CurrentRecipe).Reload();
                RefreshConfig();
                return false;
            }
            _Report.On_LoadPort_Selected(nodeLD);
            Node LROB = NodeManagement.Get(nodeLD.Associated_Node);
            LDRobot = LROB.Name;

            LD = nodeLD.Name;



            var AvailableSlots = from eachSlot in nodeLD.JobList.Values.ToList()
                                 where eachSlot.NeedProcess && eachSlot.MapFlag && !eachSlot.ErrPosition && !eachSlot.AbortProcess
                                 select eachSlot;
            ProcessCount = AvailableSlots.Count();

            var UnloadPortSlots = from eachSlot in AvailableSlots
                                  group eachSlot by eachSlot.Destination into g
                                  select g.First();
            foreach (Job eachSlot in UnloadPortSlots)
            {
                Node port = NodeManagement.Get(eachSlot.Destination);
                if (port != null)
                {
                    _Report.On_UnLoadPort_Selected(port);
                }
            }
            //if (ProcessCount == 0)
            //{
            //    Running = false;
            //    _Report.On_Transfer_Complete(this);
            //    return true;
            //}
            //var crossRunSlots = from eachSlot in nodeLD.JobList.Values.ToList()
            //                    where !NodeManagement.Get(eachSlot.Destination).Equals(LDRobot)
            //                    select eachSlot;
            //if (crossRunSlots.Count() == 0)
            //{//只會用到第一支ROBOT
            //    if (usedList.ContainsKey(LDRobot) || usedList.ContainsKey(LROB.Associated_Node))
            //    {//資源取得失敗
            //        return false;
            //    }
            //    usedList.Add(LDRobot, RunID);
            //    usedList.Add(LROB.Associated_Node, RunID);
            //    ThreadPool.QueueUserWorkItem(new WaitCallback(Engine), LDRobot);
            //    ThreadPool.QueueUserWorkItem(new WaitCallback(Engine), LROB.Associated_Node);
            //}
            //else
            //{//兩隻ROBOT都會用到
            //   if(usedList.ContainsKey("ROBOT01") || usedList.ContainsKey("ROBOT02") || usedList.ContainsKey("ALIGNER01") || usedList.ContainsKey("ALIGNER02"))
            //    {//資源取得失敗
            //        return false;
            //    }

            //    usedList.Add("ROBOT01", RunID);
            //    usedList.Add("ROBOT02", RunID);
            //    usedList.Add("ALIGNER01", RunID);
            //    usedList.Add("ALIGNER02", RunID);
            //    ThreadPool.QueueUserWorkItem(new WaitCallback(Engine), "ROBOT01");
            //    ThreadPool.QueueUserWorkItem(new WaitCallback(Engine), "ROBOT02");
            //    ThreadPool.QueueUserWorkItem(new WaitCallback(Engine), "ALIGNER01");
            //    ThreadPool.QueueUserWorkItem(new WaitCallback(Engine), "ALIGNER02");
            //}



            Node.ActionRequest request = new Node.ActionRequest();
            request.TaskName = TaskFlowManagement.Command.TRANSFER_GET_LOADPORT;
            lock (LROB.RequestQueue)
            {
                if (!LROB.RequestQueue.ContainsKey(request.TaskName))
                {
                    LROB.RequestQueue.Add(request.TaskName, request);
                }
            }
            Running = true;
            logger.Debug("XfeCrossZone Start");
            //NodeStatusUpdate.UpdateCurrentState("RUN");
            return Running;
        }

        public static void Stop()
        {

            Running = false;
            TaskFlowManagement.Excute(Guid.NewGuid().ToString(), TaskFlowManagement.Command.STOP, null);
            //NodeStatusUpdate.UpdateCurrentState("IDLE");
        }

        private bool CheckQueue(Node Target)
        {
            //if (Target.Name.Equals("ROBOT02"))
            //{
            //    string ttt = "";
            //}
            //if (Target.Name.Equals("ROBOT01"))
            //{
            //    string ttt = "";
            //}
            if (Target.LockOn == null)
            {
                Target.LockOn = "";
            }
            //當沒有鎖定時，Queue裡有就做
            bool a = Target.RequestQueue.Count() != 0 && Target.LockOn.Equals("");
            bool b = false;
            lock (Target.RequestQueue)
            {
                //當有鎖定時，只能做鎖定的
                b = (from Request in Target.RequestQueue.Values.ToList()
                     where Request.Position.Equals(Target.LockOn)
                     select Request).Count() != 0 && !Target.LockOn.Equals("");
            }
            return a || b;
        }

        private bool CheckWIPForLoad(Node Robot)
        {
            bool result = false;
            bool a = false;
            bool b = false;
            //找到所有被取出FOUP的WAFER
            var ProcessList = from Job in JobManagement.GetJobList()
                              where Job.InProcess
                              select Job;

            foreach (Job each in ProcessList)
            {
                Node dest = NodeManagement.Get(each.Destination);
                //找尋到達目的地需要此ROBOT搬運的WAFER
                //同時也不在此ROBOT手上
                if (dest.Associated_Node.ToUpper().Equals(Robot.Name.ToUpper()) && !each.Position.ToUpper().Equals(Robot.Name.ToUpper()))
                {
                    a = true;
                    break;
                }
            }

            b = true;

            result = a && b;

            return result;
        }

        private bool CheckWIPForUnload(Node Robot)
        {
            bool result = false;
            bool a = false;
            bool b = false;
            bool c = false;
            if (Recipe.Get(SystemConfig.Get().CurrentRecipe).is_use_exchange)
            {//剛從Aligner上取一片，當令一片還沒處理，先放Aligner
                var ProcessList = from Job in Robot.JobList.Values
                                  where !Job.AlignerFlag
                                  select Job;
                if (ProcessList.Count() != 0)
                {
                    result = true;
                }
            }
            else
            {
                //找到所有被取出FOUP的WAFER
                var ProcessList = from Job in JobManagement.GetJobList()
                                  where Job.InProcess
                                  select Job;

                foreach (Job each in ProcessList)
                {
                    Node dest = NodeManagement.Get(each.Destination);
                    //找尋到達目的地需要此ROBOT搬運的WAFER
                    //同時也不在此ROBOT手上
                    if ((dest.Associated_Node.ToUpper().Equals(Robot.Name.ToUpper()) && !each.Position.ToUpper().Equals(Robot.Name.ToUpper())))
                    {
                        a = true;
                        break;
                    }
                }

                //if (Robot.JobList.Count != 2)
                //{
                //    b = true;
                //}

                ProcessList = from Job in JobManagement.GetJobList()
                              where Job.InProcess && Job.NeedProcess
                              select Job;

                if (ProcessList.Count() != 0)
                {
                    c = true;
                }

                result = (a || b || c)/* && Robot.JobList.Count != 2*/;
            }
            return result;
        }


        private void Engine(object NodeName)
        {

            Node Target = NodeManagement.Get(NodeName.ToString());
            if (Target == null)
            {
                return;
            }
            if (!Target.Enable)
            {
                return;
            }

            while (true)
            {
                try
                {
                    while (!CheckQueue(Target) && Running)
                    {
                        SpinWait.SpinUntil(() => CheckQueue(Target) || !Running, 5000);
                    }
                    if (Running)
                    {
                        string Message = "";
                        string id = Guid.NewGuid().ToString();
                        Node.ActionRequest req;
                        lock (Target.RequestQueue)
                        {
                            List<Node.ActionRequest> RequestQueue = Target.RequestQueue.Values.ToList();

                            if (!Target.LockOn.Equals(""))
                            {//當ROBOT正在存取某個，必須收回手臂才能對另一台動作
                                logger.Debug(NodeName + " LockOn:" + Target.LockOn);
                                var find = from Request in RequestQueue
                                           where Request.Position.Equals(Target.LockOn) && (!Request.TaskName.Equals("TRANSFER_GETW_ALIGNER01") && !Request.TaskName.Equals("TRANSFER_GETW_ALIGNER02"))
                                           select Request;
                                RequestQueue = find.ToList();
                            }

                            RequestQueue.Sort((x, y) => { return x.TimeStamp.CompareTo(y.TimeStamp); });
                            if (RequestQueue.Count == 0)
                            {
                                continue;
                            }
                            if (Target.LockOn.Equals(""))
                            {
                                //當混和模式時，放回ULD要在LD取片之前
                                var find = from Request in RequestQueue
                                           where Request.TaskName.Equals("TRANSFER_PUT_UNLOADPORT") || Request.TaskName.Equals("TRANSFER_PUT_UNLOADPORT_2ARM")
                                           select Request;

                                if (find.Count() != 0)
                                {
                                    RequestQueue = find.ToList();

                                }
                            }
                            req = RequestQueue.First();
                            Target.RequestQueue.Remove(req.TaskName);
                        }
                        logger.Debug(NodeName + " 開始執行:" + req.TaskName);
                        Node nodeLD;

                        switch (Target.Type)
                        {
                            case "ROBOT":
                                switch (req.TaskName)
                                {
                                    case TaskFlowManagement.Command.TRANSFER_GET_LOADPORT:
                                        if (CheckWIPForLoad(Target) && !Recipe.Get(SystemConfig.Get().CurrentRecipe).is_use_exchange)//混和模式 && 不在Exchange模式
                                        {
                                            //還有片要處理
                                            Target.LockOn = "";
                                            continue;
                                        }
                                        logger.Debug(NodeName + " 找到可用Loadport");
                                        nodeLD = NodeManagement.Get(LD);
                                        Target.LockOn = nodeLD.Name;//鎖定PORT
                                        req.Position = nodeLD.Name;

                                        var AvailableSlots = from eachSlot in nodeLD.JobList.Values.ToList()
                                                             where eachSlot.NeedProcess && eachSlot.MapFlag && !eachSlot.ErrPosition && !eachSlot.AbortProcess
                                                             select eachSlot;
                                        if (AvailableSlots.Count() != 0)
                                        {
                                            List<Job> AvailableSlotsList = AvailableSlots.ToList();
                                            if (Recipe.Get(SystemConfig.Get().CurrentRecipe).get_slot_order.Equals("BOTTOM_UP"))
                                            {
                                                AvailableSlotsList.Sort((x, y) => { return Convert.ToInt32(x.Slot).CompareTo(Convert.ToInt32(y.Slot)); });
                                            }
                                            else
                                            {
                                                AvailableSlotsList.Sort((x, y) => { return -Convert.ToInt32(x.Slot).CompareTo(Convert.ToInt32(y.Slot)); });
                                            }
                                            Job j;
                                            if (AvailableSlotsList.Count == 1)//Port剩下一片
                                            {
                                                j = AvailableSlotsList.First();
                                                req.Slot = j.Slot;
                                                if (!Target.JobList.ContainsKey("1") && Target.RArmActive)//R沒片且R為可用狀態
                                                {
                                                    req.Arm = "1";

                                                }
                                                else if (!Target.JobList.ContainsKey("2") && Target.LArmActive && !Recipe.Get(SystemConfig.Get().CurrentRecipe).is_use_exchange)//L沒片且L為可用狀態 && 不在Exchange模式
                                                {
                                                    req.Arm = "2";

                                                }
                                                else
                                                {
                                                    //無法再取片
                                                    if (Target.JobList.Count() != 0)
                                                    {//開始放片至Aligner
                                                        //Target.RequestQueue.Clear();
                                                        int Aidx = 1;
                                                        foreach (Job wafer in Target.JobList.Values)
                                                        {
                                                            foreach (Node Aligner in NodeManagement.GetAlignerList())
                                                            {
                                                                if (SingleAligner && !Aligner.Name.Equals(Target.Associated_Node))
                                                                {
                                                                    continue;
                                                                }

                                                                //佇列裡面沒有才加
                                                                Node.ActionRequest request = new Node.ActionRequest();
                                                                if (Recipe.Get(SystemConfig.Get().CurrentRecipe).is_use_exchange && Aligner.JobList.Count() != 0)
                                                                {
                                                                    request.TaskName = (TaskFlowManagement.Command)Enum.Parse(typeof(TaskFlowManagement.Command), "TRANSFER_GET_" + Aligner.Name);
                                                                }
                                                                else
                                                                {
                                                                    request.TaskName = (TaskFlowManagement.Command)Enum.Parse(typeof(TaskFlowManagement.Command), "TRANSFER_PUTW_" + Aligner.Name);
                                                                }
                                                                request.Position = Aligner.Name;
                                                                //request.Arm = wafer.Slot;
                                                                lock (Target.RequestQueue)
                                                                {
                                                                    //Target.RequestQueue.Clear();
                                                                    if (!Target.RequestQueue.ContainsKey(request.TaskName))
                                                                    {
                                                                        if (Aidx == 2)
                                                                        {
                                                                            //讓後面的ALIGNER有時間差，排序才會在上一台後面
                                                                            request.TimeStamp += Aidx;
                                                                        }
                                                                        Target.RequestQueue.Add(request.TaskName, request);
                                                                        Aidx++;
                                                                        break;
                                                                    }
                                                                }

                                                            }
                                                        }

                                                        Target.LockOn = "";//解除鎖定
                                                        continue;

                                                    }
                                                }
                                            }
                                            else//Port 有兩片以上
                                            {
                                                bool AllowDoubleArm = false;
                                                if (Math.Abs(Convert.ToInt32(AvailableSlotsList[1].Slot) - Convert.ToInt32(AvailableSlotsList[0].Slot)) == 1 && !Recipe.Get(SystemConfig.Get().CurrentRecipe).is_use_exchange)
                                                {
                                                    AllowDoubleArm = true;//連續Slot才能雙取 && 不在Exchange模式
                                                }

                                                if (!Target.JobList.ContainsKey("1") && !Target.JobList.ContainsKey("2") && Target.DoubleArmActive && Target.RArmActive && Target.LArmActive && AllowDoubleArm)//當可以雙取
                                                {//RL全為空 & RL都可用 & 雙取啟動 & 兩片為連續Slot
                                                 //雙取要用第二片的Slot
                                                    req.Slot = Convert.ToInt16(AvailableSlotsList[1].Slot) > Convert.ToInt16(AvailableSlotsList[0].Slot) ? AvailableSlotsList[1].Slot : AvailableSlotsList[0].Slot;
                                                    req.Slot2 = Convert.ToInt16(AvailableSlotsList[1].Slot) < Convert.ToInt16(AvailableSlotsList[0].Slot) ? AvailableSlotsList[1].Slot : AvailableSlotsList[0].Slot;
                                                    req.TaskName = TaskFlowManagement.Command.TRANSFER_GET_LOADPORT_2ARM;
                                                    req.Arm = "3";

                                                }
                                                else//只能單取
                                                {
                                                    //j = AvailableSlotsList.First();
                                                    //req.Slot = j.Slot;
                                                    //if (!Target.JobList.ContainsKey("1") && Target.RArmActive)//R沒片且R為可用狀態
                                                    //{
                                                    //    req.Arm = "1";
                                                    //}
                                                    //else if (!Target.JobList.ContainsKey("2") && Target.LArmActive)//L沒片且L為可用狀態
                                                    //{
                                                    //    req.Arm = "2";
                                                    //}
                                                    if (!Target.JobList.ContainsKey("1") && Target.RArmActive && !Target.JobList.ContainsKey("2") && Target.LArmActive && AvailableSlotsList.Count() >= 2 && Target.LArmActive && !Recipe.Get(SystemConfig.Get().CurrentRecipe).is_use_exchange)
                                                    {//R & L 都可用且有兩片能取


                                                        j = AvailableSlotsList.First();
                                                        req.Slot = j.Slot;
                                                        if (Convert.ToInt16(AvailableSlotsList[0].DestinationSlot) > Convert.ToInt16(AvailableSlotsList[1].DestinationSlot))
                                                        {//如果最下面那片的目的Slot比較高 用R取(R軸在上面L軸在下 以便雙放)
                                                            req.Arm = "1";
                                                        }
                                                        else
                                                        {
                                                            req.Arm = "2";
                                                        }
                                                    }
                                                    else if (!Target.JobList.ContainsKey("1") && Target.RArmActive)//R沒片且R為可用狀態
                                                    {
                                                        j = AvailableSlotsList.First();
                                                        req.Slot = j.Slot;
                                                        req.Arm = "1";
                                                    }
                                                    else if (!Target.JobList.ContainsKey("2") && Target.LArmActive && !Recipe.Get(SystemConfig.Get().CurrentRecipe).is_use_exchange)//L沒片且L為可用狀態 && 不在Exchange模式
                                                    {
                                                        j = AvailableSlotsList.First();
                                                        req.Slot = j.Slot;
                                                        req.Arm = "2";
                                                    }
                                                    else
                                                    {
                                                        //無法再取片
                                                        if (Target.JobList.Count() != 0)
                                                        {//開始放片至Aligner
                                                            //Target.RequestQueue.Clear();
                                                            int Aidx = 1;
                                                            foreach (Job wafer in Target.JobList.Values)
                                                            {
                                                                foreach (Node Aligner in NodeManagement.GetAlignerList())
                                                                {
                                                                    if (SingleAligner && !Aligner.Name.Equals(Target.Associated_Node))
                                                                    {
                                                                        continue;
                                                                    }

                                                                    //佇列裡面沒有才加
                                                                    Node.ActionRequest request = new Node.ActionRequest();
                                                                    if (Recipe.Get(SystemConfig.Get().CurrentRecipe).is_use_exchange && Aligner.JobList.Count() != 0)
                                                                    {
                                                                        request.TaskName = (TaskFlowManagement.Command)Enum.Parse(typeof(TaskFlowManagement.Command), "TRANSFER_GET_" + Aligner.Name);

                                                                    }
                                                                    else
                                                                    {
                                                                        request.TaskName = (TaskFlowManagement.Command)Enum.Parse(typeof(TaskFlowManagement.Command), "TRANSFER_PUTW_" + Aligner.Name);
                                                                    }
                                                                    request.Position = Aligner.Name;
                                                                    //request.Arm = wafer.Slot;
                                                                    lock (Target.RequestQueue)
                                                                    {
                                                                        //Target.RequestQueue.Clear();
                                                                        if (!Target.RequestQueue.ContainsKey(request.TaskName))
                                                                        {
                                                                            if (Aidx == 2)
                                                                            {
                                                                                //讓後面的ALIGNER有時間差，排序才會在上一台後面
                                                                                request.TimeStamp += Aidx;
                                                                            }
                                                                            Target.RequestQueue.Add(request.TaskName, request);
                                                                            Aidx++;
                                                                            break;
                                                                        }
                                                                    }

                                                                }
                                                            }

                                                            Target.LockOn = "";//解除鎖定
                                                            continue;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Target.JobList.Count != 0)
                                            {

                                                int Aidx = 1;
                                                //Target.RequestQueue.Clear();
                                                foreach (Job wafer in Target.JobList.Values)
                                                {
                                                    foreach (Node Aligner in NodeManagement.GetAlignerList())
                                                    {
                                                        if (SingleAligner && !Aligner.Name.Equals(Target.Associated_Node))
                                                        {
                                                            continue;
                                                        }



                                                        //佇列裡面沒有才加
                                                        Node.ActionRequest request = new Node.ActionRequest();
                                                        if (Recipe.Get(SystemConfig.Get().CurrentRecipe).is_use_exchange && Aligner.JobList.Count() != 0)
                                                        {
                                                            request.TaskName = (TaskFlowManagement.Command)Enum.Parse(typeof(TaskFlowManagement.Command), "TRANSFER_GET_" + Aligner.Name);
                                                        }
                                                        else
                                                        {
                                                            request.TaskName = (TaskFlowManagement.Command)Enum.Parse(typeof(TaskFlowManagement.Command), "TRANSFER_PUTW_" + Aligner.Name);
                                                        }
                                                        request.Position = Aligner.Name;
                                                        //request.Arm = wafer.Slot;
                                                        lock (Target.RequestQueue)
                                                        {
                                                            //Target.RequestQueue.Clear();
                                                            if (!Target.RequestQueue.ContainsKey(request.TaskName))
                                                            {
                                                                if (Aidx == 2)
                                                                {
                                                                    //讓後面的ALIGNER有時間差，排序才會在上一台後面
                                                                    request.TimeStamp += Aidx;
                                                                }
                                                                Target.RequestQueue.Add(request.TaskName, request);
                                                                Aidx++;
                                                                break;
                                                            }
                                                        }

                                                    }
                                                }

                                                Target.LockOn = "";//解除鎖定
                                                continue;
                                            }
                                            else
                                            {
                                                logger.Debug(NodeName + " Loadport沒有片可處理");

                                                nodeLD.Fetchable = false;
                                                //檢查是不是搬完了

                                                var Available = from each in JobManagement.GetJobList()
                                                                where NodeManagement.Get(each.Position).Type.Equals("ALIGNER")
                                                                select each;
                                                if (Available.Count() != 0)
                                                {
                                                    Node.ActionRequest request = new Node.ActionRequest();
                                                    request.TaskName = (TaskFlowManagement.Command)Enum.Parse(typeof(TaskFlowManagement.Command), "TRANSFER_GET_" + Available.First().Position);
                                                    request.Position = Available.First().Position;
                                                    lock (Target.RequestQueue)
                                                    {
                                                        if (!Target.RequestQueue.ContainsKey(request.TaskName))
                                                        {
                                                            Target.RequestQueue.Add(request.TaskName, request);

                                                        }
                                                    }
                                                    Target.LockOn = "";
                                                }
                                                else
                                                {
                                                    watch.Stop();
                                                    ProcessTime = watch.ElapsedMilliseconds;
                                                    logger.Debug("On_Transfer_Complete ProcessTime:" + ProcessTime.ToString());
                                                    logger.Debug("XfeCrossZone Stop");
                                                    Running = false;
                                                    if (IsTmpConfig)
                                                    {
                                                        IsTmpConfig = false;
                                                        Recipe.Get(SystemConfig.Get().CurrentRecipe).Reload();
                                                        RefreshConfig();
                                                    }
                                                    _Report.On_Transfer_Complete(this);
                                                }
                                                //    //結束工作
                                                //}

                                                continue;
                                            }
                                        }
                                        //}
                                        break;
                                    case TaskFlowManagement.Command.TRANSFER_PUT_ALIGNER01:
                                    case TaskFlowManagement.Command.TRANSFER_PUT_ALIGNER02:

                                        Node pos = NodeManagement.Get(req.Position);
                                        logger.Debug("Waiting for " + req.Position + " ready...");
                                        while (!pos.ReadyForPut && Running)
                                        {
                                            SpinWait.SpinUntil(() => pos.ReadyForPut || !Running, 99999999);
                                        }
                                        if (!Running)
                                        {
                                            continue;
                                        }
                                        logger.Debug(req.Position + " is ready now.");
                                        pos.ReadyForPut = false;
                                        pos.ReadyForGet = false;
                                        break;
                                    case TaskFlowManagement.Command.TRANSFER_PUT_ALIGNER01_2:
                                    case TaskFlowManagement.Command.TRANSFER_PUT_ALIGNER02_2:
                                        Target.LockOn = "";
                                        LDRobot_Arm = "";
                                        break;
                                    case TaskFlowManagement.Command.TRANSFER_PUTW_ALIGNER01:
                                    case TaskFlowManagement.Command.TRANSFER_PUTW_ALIGNER02:
                                        if (NodeManagement.GetAlignerList().Count == 0)
                                        {
                                            Node.ActionRequest newAct = new Node.ActionRequest();
                                            newAct.TaskName = TaskFlowManagement.Command.TRANSFER_PUT_UNLOADPORT;
                                            if (!Target.RequestQueue.ContainsKey(newAct.TaskName))
                                            {
                                                Target.RequestQueue.Add(newAct.TaskName, newAct);
                                            }
                                            continue;
                                        }
                                        if (Recipe.Get(SystemConfig.Get().CurrentRecipe).is_use_exchange)
                                        {
                                            Node alg = NodeManagement.Get(req.Position);
                                            if (alg.JobList.Count != 0)
                                            {
                                                continue;
                                            }
                                        }
                                        //決定要放R或L
                                        if (LDRobot_Arm.Equals(""))
                                        {
                                            var Available = from each in Target.JobList.Values
                                                            where each.NeedProcess
                                                            select each;

                                            if (Available.Count() == 0)
                                            {//當只有一台ALIGNER，被觸發放第二片但沒有時，略過此命令
                                                continue;
                                            }
                                            List<Job> tmpForSort = Available.ToList();
                                            tmpForSort.Sort((x, y) => { return -Convert.ToInt32(x.DestinationSlot).CompareTo(Convert.ToInt32(y.DestinationSlot)); });
                                            //先放目的地Slot位置比較高的，讓取片Robot先放在R軸，確保可以做雙ARM放片
                                            foreach (Job wafer in tmpForSort)
                                            {
                                                LDRobot_Arm = wafer.Slot;
                                                wafer.NeedProcess = false;

                                                break;
                                            }
                                        }
                                        req.Arm = LDRobot_Arm;
                                        Target.LockOn = req.Position;

                                        break;
                                    case TaskFlowManagement.Command.TRANSFER_GET_ALIGNER01:
                                    case TaskFlowManagement.Command.TRANSFER_GET_ALIGNER02:
                                        if (Target.RobotGetState == 1)
                                        {
                                            continue;
                                        }
                                        //決定要用R或L取
                                        if (ULDRobot_Arm.Equals(""))
                                        {
                                            if (!Target.JobList.ContainsKey("1") && Target.RArmActive)//R沒片且R為可用狀態
                                            {
                                                ULDRobot_Arm = "1";
                                            }
                                            else if (!Target.JobList.ContainsKey("2") && Target.LArmActive)//L沒片且L為可用狀態
                                            {
                                                ULDRobot_Arm = "2";
                                            }
                                        }
                                        req.Arm = ULDRobot_Arm;
                                        Target.LockOn = req.Position;
                                        pos = NodeManagement.Get(req.Position);
                                        logger.Debug("Waiting for " + req.Position + " ready...");
                                        while (!pos.ReadyForGet && Running)
                                        {
                                            SpinWait.SpinUntil(() => pos.ReadyForGet || !Running, 5000);
                                        }
                                        if (!Running)
                                        {
                                            continue;
                                        }
                                        logger.Debug(req.Position + " is ready now.");
                                        //pos.ReadyForGet = false;
                                        break;

                                    case TaskFlowManagement.Command.TRANSFER_GET_ALIGNER01_2:
                                    case TaskFlowManagement.Command.TRANSFER_GET_ALIGNER02_2:
                                        Target.LockOn = "";
                                        ULDRobot_Arm = "";

                                        if (NodeManagement.GetAlignerList().Count == 1)
                                        {
                                            //當只有一台ALIGNER使用邏輯
                                            //觸發放第二片                                   
                                            Node.ActionRequest request = new Node.ActionRequest();
                                            request.TaskName = (TaskFlowManagement.Command)Enum.Parse(typeof(TaskFlowManagement.Command), "TRANSFER_PUTW_" + req.Position);
                                            request.Position = req.Position;
                                            //request.Arm = wafer.Slot;
                                            Node LDRbt = NodeManagement.Get(LDRobot);
                                            lock (LDRbt.RequestQueue)
                                            {
                                                if (!LDRbt.RequestQueue.ContainsKey(request.TaskName))
                                                {
                                                    LDRbt.RequestQueue.Add(request.TaskName, request);
                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                    case TaskFlowManagement.Command.TRANSFER_GETW_ALIGNER01:
                                    case TaskFlowManagement.Command.TRANSFER_GETW_ALIGNER02:
                                        //決定要用R或L取
                                        Node al = NodeManagement.Get(req.Position);
                                        if (al.JobList.Count == 0)
                                        {
                                            continue;
                                        }
                                        if (ULDRobot_Arm.Equals(""))
                                        {
                                            if (!Target.JobList.ContainsKey("1") && Target.RArmActive)//R沒片且R為可用狀態
                                            {
                                                ULDRobot_Arm = "1";
                                            }
                                            else if (!Target.JobList.ContainsKey("2") && Target.LArmActive)//L沒片且L為可用狀態
                                            {
                                                ULDRobot_Arm = "2";
                                            }
                                        }
                                        req.Arm = ULDRobot_Arm;
                                        if (req.Arm.Equals(""))
                                        {
                                            continue;
                                        }
                                        Target.LockOn = req.Position;
                                        break;
                                    case TaskFlowManagement.Command.TRANSFER_PUT_UNLOADPORT:
                                        //檢查目前狀態是否要去放

                                        if (!Target.ForcePutToUnload)
                                        {
                                            if (CheckWIPForUnload(Target))
                                            {
                                                //還有片要處理




                                                Target.LockOn = "";
                                                continue;
                                            }
                                        }
                                        if (Target.DoubleArmActive && Target.RArmActive && Target.LArmActive && Target.JobList.Count == 2)
                                        {//支援雙放
                                            if (Target.JobList["1"].Destination.Equals(Target.JobList["2"].Destination) && Convert.ToInt32(Target.JobList["1"].DestinationSlot) - Convert.ToInt32(Target.JobList["2"].DestinationSlot) == 1)
                                            {//目的地Slot連續且順序正確
                                             //雙放要用R的Slot
                                                req.Arm = "3";
                                                req.Slot = Target.JobList["1"].DestinationSlot;
                                                req.Slot2 = Target.JobList["2"].DestinationSlot;
                                                req.TaskName = TaskFlowManagement.Command.TRANSFER_PUT_UNLOADPORT_2ARM;
                                                req.Position = Target.JobList["1"].Destination;
                                            }
                                            else
                                            {//目的地不同 OR Slot不連續，只能單放
                                             //看哪一片的Slot在上面就先放

                                                var ArmWafers = (from each in Target.JobList.Values
                                                                 select each).OrderByDescending(x => Convert.ToInt16(x.DestinationSlot));
                                                if (Recipe.Get(SystemConfig.Get().CurrentRecipe).put_slot_order.Equals("BOTTOM_UP"))
                                                {
                                                    ArmWafers = (from each in Target.JobList.Values
                                                                 select each).OrderBy(x => Convert.ToInt16(x.DestinationSlot));
                                                }
                                                req.Position = ArmWafers.First().Destination;
                                                req.Arm = ArmWafers.First().Slot;
                                                req.Slot = ArmWafers.First().DestinationSlot;
                                            }
                                        }
                                        else
                                        {//只能單放
                                            var ArmWafers = (from each in Target.JobList.Values
                                                             select each).OrderByDescending(x => Convert.ToInt16(x.DestinationSlot));

                                            if (Recipe.Get(SystemConfig.Get().CurrentRecipe).put_slot_order.Equals("BOTTOM_UP"))
                                            {
                                                ArmWafers = (from each in Target.JobList.Values
                                                             select each).OrderBy(x => Convert.ToInt16(x.DestinationSlot));
                                            }

                                            if (ArmWafers.Count() != 0)
                                            {
                                                req.Position = ArmWafers.First().Destination;
                                                req.Arm = ArmWafers.First().Slot;
                                                req.Slot = ArmWafers.First().DestinationSlot;
                                            }
                                            else
                                            {//沒東西放了


                                                Target.LockOn = "";
                                                if (NodeManagement.GetAlignerList().Count == 0)
                                                {
                                                    req = new Node.ActionRequest();
                                                    req.TaskName = TaskFlowManagement.Command.TRANSFER_GET_LOADPORT;
                                                    if (!Target.RequestQueue.ContainsKey(req.TaskName))
                                                    {
                                                        Target.RequestQueue.Add(req.TaskName, req);
                                                    }
                                                }
                                                continue;
                                            }

                                        }
                                        Target.LockOn = req.Position;

                                        var Match = from each in ULD_List
                                                    where each.Equals(req.Position)
                                                    select each;
                                        if (Match.Count() == 0)
                                        {
                                            ULD_List.Add(req.Position);
                                        }
                                        break;
                                }
                                break;
                            case "ALIGNER":
                                switch (req.TaskName)
                                {
                                    case TaskFlowManagement.Command.TRANSFER_ALIGNER_WHLD:
                                        //找到回送ULD的ROBOT
                                        ULDRobot = NodeManagement.Get(Target.JobList["1"].Destination).Associated_Node;

                                        break;
                                    case TaskFlowManagement.Command.TRANSFER_ALIGNER_WRLS:
                                        //找到回送ULD的ROBOT
                                        ULDRobot = NodeManagement.Get(Target.JobList["1"].Destination).Associated_Node;


                                        break;

                                    case TaskFlowManagement.Command.TRANSFER_ALIGNER_ALIGN:
                                        ULDRobot = NodeManagement.Get(Target.JobList["1"].Destination).Associated_Node;
                                        //放進UnloadPort補償角度
                                        RobotPoint point = PointManagement.GetPoint(ULDRobot, Target.Name);
                                        Target.JobList["1"].Offset += point.Offset;
                                        req.V3 = (Target.JobList["1"].Offset + Convert.ToInt32(rcp.notch_angle)).ToString();
                                        if (Target.Name.ToUpper().Equals("ALIGNER01"))
                                        {
                                            req.Value = rcp.aligner1_angle;
                                        }
                                        else if (Target.Name.ToUpper().Equals("ALIGNER02"))
                                        {
                                            req.Value = rcp.aligner2_angle;
                                        }

                                        req.V2 = Target.Associated_Node;
                                        break;
                                }
                                break;
                        }
                        Dictionary<string, string> param = new Dictionary<string, string>();

                        param.Add("@Target", NodeName.ToString());
                        param.Add("@Slot", req.Slot);
                        param.Add("@S2", req.Slot2);
                        param.Add("@Arm", req.Arm);
                        param.Add("@Value", req.Value);
                        param.Add("@V2", req.V2);
                        param.Add("@V3", req.V3);
                        param.Add("@V4", req.V4);
                        param.Add("@V5", req.V5);
                        param.Add("@V6", req.V6);
                        param.Add("@V7", req.V7);
                        param.Add("@V8", req.V8);
                        param.Add("@V9", req.V9);
                        param.Add("@Position", req.Position);
                        param.Add("@LDRobot", LDRobot);
                        param.Add("@ULDRobot", ULDRobot);
                        param.Add("@Loadport", LD);
                        TaskFlowManagement.CurrentProcessTask Task = TaskFlowManagement.Excute(id, req.TaskName, param);

                        //這邊要卡住直到Task完成
                        logger.Debug(NodeName + " 等待Task完成");
                        while (!Task.Finished && Running)
                        {
                            SpinWait.SpinUntil(() => Task.Finished || !Running, 5000);
                        }
                        if (Task.HasError)
                        {
                            Running = false;
                        }
                        if (Running)
                        {
                            logger.Debug(NodeName + " Task完成");
                            string TargetStr = Task.Params["@Target"];
                            string PositionStr = Task.Params["@Position"];
                            switch (Task.TaskName)
                            {

                                case TaskFlowManagement.Command.TRANSFER_PUT_UNLOADPORT_2ARM:
                                case TaskFlowManagement.Command.TRANSFER_PUT_UNLOADPORT:
                                    var LeftWafer = from each in JobManagement.GetJobList()
                                                    where each.Destination.ToUpper().Equals(PositionStr.ToUpper()) && !each.Position.ToUpper().Equals(PositionStr.ToUpper())
                                                    select each;
                                    if (LeftWafer.Count() == 0)
                                    {
                                        new Thread(() =>
                                        {
                                            Thread.CurrentThread.IsBackground = true;

                                            _Report.On_UnLoadPort_Complete(NodeManagement.Get(PositionStr));
                                        }).Start();
                                    }
                                    var NotFinished = from each in JobManagement.GetJobList()
                                                      where ((NodeManagement.Get(each.Position).Type.Equals("LOADPORT") && !each.Position.Equals(each.Destination) && !each.Destination.Equals("")) || NodeManagement.Get(each.Position).Type.Equals("ALIGNER") || NodeManagement.Get(each.Position).Type.Equals("ROBOT") || NodeManagement.Get(each.Position).Type.Equals("LOADLOCK"))
                                                      select each;
                                    if (NotFinished.Count() == 0)
                                    {
                                        watch.Stop();
                                        ProcessTime = watch.ElapsedMilliseconds;
                                        logger.Debug("On_Transfer_Complete ProcessTime:" + ProcessTime.ToString());
                                        logger.Debug("XfeCrossZone Stop");
                                        Running = false;
                                        if (IsTmpConfig)
                                        {
                                            IsTmpConfig = false;
                                            Recipe.Get(SystemConfig.Get().CurrentRecipe).Reload();
                                            RefreshConfig();
                                        }
                                        _Report.On_Transfer_Complete(this);
                                    }
                                    break;
                                case TaskFlowManagement.Command.TRANSFER_GET_LOADPORT:
                                case TaskFlowManagement.Command.TRANSFER_GET_LOADPORT_2ARM:
                                    var Available = from each in NodeManagement.Get(PositionStr).JobList.Values
                                                    where each.NeedProcess && !each.AbortProcess || (each.Destination.ToUpper().Equals(PositionStr.ToUpper()) && !each.Position.ToUpper().Equals(each.Destination.ToUpper()))
                                                    select each;
                                    if (Available.Count() == 0)
                                    {
                                        Available = from a in NodeManagement.Get(TargetStr).JobList.Values
                                                    where a.Destination.ToUpper().Equals(PositionStr.ToUpper())
                                                    select a;
                                        if (Available.Count() == 0)
                                        {
                                            new Thread(() =>
                                            {
                                                Thread.CurrentThread.IsBackground = true;
                                                _Report.On_LoadPort_Complete(NodeManagement.Get(PositionStr));
                                            }).Start();
                                        }
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            logger.Debug(NodeName + " 運作停止");
                            continue;
                        }
                    }
                    else
                    {
                        logger.Debug(NodeName + " 暫停監控RequestQueue");
                        while (!Running)
                        {
                            SpinWait.SpinUntil(() => Running, 99999999);
                        }
                        logger.Debug(NodeName + " 開始監控RequestQueue");
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.StackTrace);
                }
            }

        }
    }
}
