using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ControlService.Config;
using ControlService.Engine;
using ControlService.Management;


namespace ControlService.TaksFlow
{
    class Sanwa_Sorter : ITaskFlow
    {
        ILog logger = LogManager.GetLogger(typeof(Sanwa_Sorter));
        public bool Excute(TaskFlowManagement.CurrentProcessTask TaskJob, ITaskFlowReport TaskReport)
        {
            logger.Debug("ITaskFlow:" + TaskJob.TaskName.ToString() + " Index:" + TaskJob.CurrentIndex.ToString());
            string Message = "";
            Node Target = null;
            Node Position = null;
            Node.ActionRequest newAct = null;
            string AlignerName = "";
            if (TaskJob.Params != null)
            {
                foreach (KeyValuePair<string, string> item in TaskJob.Params)
                {
                    switch (item.Key)
                    {
                        case "@Target":
                            Target = NodeManagement.Get(item.Value);
                            break;
                        case "@Position":
                            Position = NodeManagement.Get(item.Value);
                            break;
                    }
                }
            }
            try
            {
                if (CheckEMO(TaskJob, TaskReport))
                {
                    switch (TaskJob.TaskName)
                    {
                        case TaskFlowManagement.Command.FFU_START:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FFUType.Start, "")));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FFUType.GetStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.FFU_STOP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FFUType.End, "")));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FFUType.GetStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.FFU_ALARM_BYPASS:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FFUType.GetStatus, "")));
                                    break;
                                case 1:
                                    if (Target.HasAlarm)
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FFUType.AlarmBypass, "")));
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.FFU_SET_SPEED:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FFUType.SetSpeed, TaskJob.Params["@Value"])));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FFUType.GetStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.STOP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    foreach (Node nd in NodeManagement.GetList())
                                    {
                                        if (nd.Enable)
                                        {
                                            switch (nd.Type.ToUpper())
                                            {
                                                case "ROBOT":
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(nd.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Servo, "0")));
                                                    break;
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.SET_ALL_SPEED:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    foreach (Node nd in NodeManagement.GetList())
                                    {
                                        if (nd.Enable)
                                        {
                                            switch (nd.Type.ToUpper())
                                            {
                                                case "ROBOT":
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(nd.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Speed, nd.Name.ToUpper().Equals("ROBOT01") ? Recipe.Get(SystemConfig.Get().CurrentRecipe).robot1_speed : Recipe.Get(SystemConfig.Get().CurrentRecipe).robot2_speed)));
                                                    break;
                                                case "ALIGNER":
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(nd.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Speed, nd.Name.ToUpper().Equals("ALIGNER01") ? Recipe.Get(SystemConfig.Get().CurrentRecipe).aligner1_speed : Recipe.Get(SystemConfig.Get().CurrentRecipe).aligner2_speed)));
                                                    break;
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALL_INIT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:

                                    if (NodeManagement.Get("ROBOT01").RArmActive)
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.SetSV, "1", "", "", "1")));
                                    }

                                    break;
                                case 1:
                                    if (NodeManagement.Get("ROBOT01").LArmActive)
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.SetSV, "1", "", "", "2")));
                                    }
                                    break;
                                case 2:
                                    if (NodeManagement.Get("ROBOT01").RArmActive)
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "4")));
                                    }
                                    break;
                                case 3:
                                    if (!NodeManagement.Get("ROBOT01").R_Hold_Status.Equals("1") && NodeManagement.Get("ROBOT01").RArmActive)
                                    {
                                        if (TaskJob.RepeatCount < 10)
                                        {
                                            TaskJob.RepeatCount++;
                                            TaskJob.CurrentIndex = TaskJob.CurrentIndex - 2;//repeat
                                            SpinWait.SpinUntil(() => false, 200);
                                        }
                                        else
                                        {
                                            TaskJob.RepeatCount = 0;
                                        }
                                    }
                                    else
                                    {
                                        TaskJob.RepeatCount = 0;
                                    }
                                    break;
                                case 4:
                                    if (NodeManagement.Get("ROBOT01").LArmActive)
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "5")));
                                    }
                                    break;
                                case 5:
                                    if (!NodeManagement.Get("ROBOT01").L_Hold_Status.Equals("1") && NodeManagement.Get("ROBOT01").LArmActive)
                                    {
                                        if (TaskJob.RepeatCount < 10)
                                        {
                                            TaskJob.RepeatCount++;
                                            TaskJob.CurrentIndex = TaskJob.CurrentIndex - 2;//repeat
                                            SpinWait.SpinUntil(() => false, 200);
                                        }
                                        else
                                        {
                                            TaskJob.RepeatCount = 0;
                                        }
                                    }
                                    else
                                    {
                                        TaskJob.RepeatCount = 0;
                                    }
                                    break;
                                case 6:
                                    if (NodeManagement.Get("ROBOT01").RArmActive && !NodeManagement.Get("ROBOT01").R_Hold_Status.Equals("1"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.SetSV, "0", "", "", "1")));
                                    }
                                    break;
                                case 7:
                                    if (NodeManagement.Get("ROBOT01").LArmActive && !NodeManagement.Get("ROBOT01").L_Hold_Status.Equals("1"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.SetSV, "0", "", "", "2")));
                                    }
                                    break;
                                case 8:
                                    foreach (Node nd in NodeManagement.GetList())
                                    {
                                        if (nd.Enable)
                                        {
                                            switch (nd.Type.ToUpper())
                                            {
                                                case "ROBOT":
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(nd.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetStatus, "")));
                                                    break;
                                                case "ALIGNER":
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(nd.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetRIO, "8")));
                                                    break;
                                                case "LOADPORT":
                                                    //online mode
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(nd.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Mode, "0")));
                                                    break;
                                            }
                                        }
                                    }
                                    break;
                                case 9:
                                    foreach (Node nd in NodeManagement.GetList())
                                    {
                                        if (nd.Enable && !SystemConfig.Get().SaftyCheckByPass)
                                        {
                                            switch (nd.Type.ToUpper())
                                            {
                                                case "ROBOT":
                                                    if (nd.R_Hold_Status.Equals("1") && nd.RArmActive)
                                                    {
                                                        AbortTask( TaskReport,TaskJob, nd.Name, "ABS", "S0300007");//Arm1 上已經有 wafer
                                                        return false;
                                                    }
                                                    else if (nd.L_Hold_Status.Equals("1") && nd.LArmActive)
                                                    {
                                                        AbortTask( TaskReport,TaskJob, nd.Name, "ABS", "S0300009");//Arm2 上已經有 wafer
                                                        return false;
                                                    }
                                                    break;
                                                case "ALIGNER":
                                                    if (nd.R_Presence)
                                                    {
                                                        AbortTask( TaskReport,TaskJob, nd.Name, "ABS", "S0300004");//Aligner 上已經有 wafer
                                                        return false;
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                    break;
                                case 10:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    foreach (Node port in NodeManagement.GetLoadPortList())
                                    {
                                        if (port.Enable)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(port.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Reset, "")));
                                        }
                                    }
                                    foreach (Node al in NodeManagement.GetAlignerList())
                                    {
                                        if (al.Enable)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(al.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Reset, "")));
                                        }
                                    }
                                    foreach (Node rb in NodeManagement.GetEnableRobotList())
                                    {
                                        if (rb.Enable)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(rb.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Reset, "")));
                                        }
                                    }
                                    break;
                                case 11:
                                    foreach (Node port in NodeManagement.GetLoadPortList())
                                    {
                                        if (port.Enable)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(port.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetOpAccess, "0")));
                                        }
                                    }
                                    foreach (Node al in NodeManagement.GetAlignerList())
                                    {
                                        if (al.Enable)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(al.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Servo, "1")));
                                        }
                                    }
                                    foreach (Node rb in NodeManagement.GetEnableRobotList())
                                    {
                                        if (rb.Enable)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(rb.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Servo, "1")));
                                        }
                                    }
                                    break;
                                case 12:
                                    foreach (Node port in NodeManagement.GetLoadPortList())
                                    {
                                        if (port.Enable)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(port.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetLoad, "0")));
                                        }
                                    }
                                    foreach (Node al in NodeManagement.GetAlignerList())
                                    {
                                        if (al.Enable)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(al.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Mode, "0")));
                                        }
                                    }
                                    foreach (Node rb in NodeManagement.GetEnableRobotList())
                                    {
                                        if (rb.Enable)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(rb.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Mode, "0")));
                                        }
                                    }
                                    break;
                                case 13:
                                    foreach (Node port in NodeManagement.GetLoadPortList())
                                    {
                                        if (port.Enable)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(port.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetUnLoad, "0")));
                                        }
                                    }
                                    foreach (Node al in NodeManagement.GetAlignerList())
                                    {
                                        if (al.Enable)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(al.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Speed, al.Name.ToUpper().Equals("ALIGNER01") ? Recipe.Get(SystemConfig.Get().CurrentRecipe).aligner1_speed : Recipe.Get(SystemConfig.Get().CurrentRecipe).aligner2_speed)));
                                        }
                                    }
                                    foreach (Node rb in NodeManagement.GetEnableRobotList())
                                    {
                                        if (rb.Enable)
                                        {

                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(rb.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Speed, rb.Name.ToUpper().Equals("ROBOT01") ? Recipe.Get(SystemConfig.Get().CurrentRecipe).robot1_speed : Recipe.Get(SystemConfig.Get().CurrentRecipe).robot2_speed)));
                                        }
                                    }
                                    break;
                                case 14:
                                    foreach (Node port in NodeManagement.GetLoadPortList())
                                    {
                                        if (port.Enable)
                                        {//manual mode
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(port.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Mode, "1")));
                                        }
                                    }
                                    foreach (Node rb in NodeManagement.GetEnableRobotList())
                                    {
                                        if (rb.Enable)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(rb.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Home, "")));
                                        }
                                    }
                                    break;
                                case 15:
                                    foreach (Node port in NodeManagement.GetLoadPortList())
                                    {
                                        if (port.Enable)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(port.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.EQASP, port.CarrierType.ToUpper().Equals("FOUP") ? "0" : "1")));
                                        }
                                    }
                                    break;
                                case 16:
                                    foreach (Node port in NodeManagement.GetLoadPortList())
                                    {
                                        if (port.Enable)
                                        {//online mode
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(port.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Mode, "0")));
                                        }
                                    }
                                    break;
                                case 17:
                                    foreach (Node port in NodeManagement.GetLoadPortList())
                                    {
                                        if (port.Enable)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(port.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ForceInitialPos, "0")));
                                        }
                                    }
                                    foreach (Node al in NodeManagement.GetAlignerList())
                                    {
                                        if (al.Enable)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(al.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.OrginSearch, "")));
                                        }
                                    }
                                    break;
                                case 18:
                                    foreach (Node port in NodeManagement.GetLoadPortList())
                                    {
                                        if (port.Enable && port.CarrierType.ToUpper().Equals("ADAPT"))
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(port.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Load, "")));
                                        }
                                    }
                                    foreach (Node al in NodeManagement.GetAlignerList())
                                    {
                                        if (al.Enable)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(al.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Home, "")));
                                        }
                                    }
                                    break;
                                case 19:
                                    foreach (Node port in NodeManagement.GetLoadPortList())
                                    {
                                        if (port.Enable && port.CarrierType.ToUpper().Equals("ADAPT"))
                                        {
                                            //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(port.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Dock, "")));
                                        }
                                    }
                                    break;
                                case 20:
                                    foreach (Node port in NodeManagement.GetLoadPortList())
                                    {
                                        if (port.Enable)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(port.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                        }
                                    }
                                    foreach (Node nd in NodeManagement.GetList())
                                    {
                                        if (nd.Enable)
                                        {
                                            nd.InitialComplete = true;
                                            nd.OrgSearchComplete = true;
                                        }
                                    }
                                    break;
                                case 21:
                                    foreach (Node port in NodeManagement.GetLoadPortList())
                                    {
                                        if (port.Enable && port.Foup_Placement)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(port.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetOpAccess, "2")));
                                        }
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ROBOT_RESET:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Reset, "")));
                                    break;
                                case 1:
                                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Servo, "1")));
                                    break;
                                default:
                                    Target.Busy = false;
                                    Target.HasAlarm = false;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ROBOT_INIT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetStatus, "")));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetSpeed, "")));
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetMode, "")));
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetError, "00")));
                                    break;
                                case 4:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Servo, "1")));
                                    break;
                                default:
                                    Target.Busy = false;
                                    Target.InitialComplete = true;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ROBOT_ORGSH:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300015");
                                        return false;
                                    }
                                    else if (Target.Busy)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300010");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Home, "")));
                                    }
                                    break;
                                default:
                                    Target.OrgSearchComplete = true;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ROBOT_HOME:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300015");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300041");
                                        return false;
                                    }
                                    else if (Target.Busy)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300010");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Home, "")));
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ROBOT_SPEED:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Speed, TaskJob.Params["@Value"])));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetSpeed, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ROBOT_GETWAIT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300015");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300041");
                                        return false;
                                    }
                                    else if (Target.Busy)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300010");
                                        return false;
                                    }
                                    else
                                    {
                                        if (!SystemConfig.Get().SaftyCheckByPass)
                                        {
                                            switch (Position.Type.ToUpper())
                                            {
                                                case "LOADPORT":
                                                    if (!Position.IsMapping)
                                                    {
                                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300156");
                                                        return false;
                                                    }
                                                    break;
                                                case "LOADLOCK":

                                                    break;
                                                default:
                                                    logger.Error("Position.Type is not define.");
                                                    AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300018");
                                                    return false;
                                            }
                                        }
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetWait, "", TaskJob.Params["@Position"], TaskJob.Params["@Slot"], TaskJob.Params["@Arm"])));
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ROBOT_PUTWAIT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300015");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300041");
                                        return false;
                                    }
                                    else if (Target.Busy)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300010");
                                        return false;
                                    }
                                    else
                                    {
                                        if (!SystemConfig.Get().SaftyCheckByPass)
                                        {
                                            switch (Position.Type.ToUpper())
                                            {
                                                case "LOADPORT":
                                                    if (!Position.IsMapping)
                                                    {
                                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300156");
                                                        return false;
                                                    }
                                                    break;
                                                case "LOADLOCK":

                                                    break;
                                                default:
                                                    logger.Error("Position.Type is not define.");
                                                    AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300018");
                                                    return false;
                                            }
                                        }
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.PutWait, "", TaskJob.Params["@Position"], TaskJob.Params["@Slot"], TaskJob.Params["@Arm"])));
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ROBOT_GET:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300015");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300041");
                                        return false;
                                    }
                                    else if (Target.Busy)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300010");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "4")));
                                        if (Position.Type.ToUpper().Equals("ALIGNER"))
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Position.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetRIO, "8")));
                                        }
                                        else if (Position.Type.ToUpper().Equals("LOADPORT"))
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Position.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                        }
                                    }
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "5")));
                                    break;
                                case 2:
                                    if (!GetSafetyCheck(TaskJob, TaskReport))
                                    {
                                        return false;
                                    }
                                    else if (Position.Type.ToUpper().Equals("LOADPORT") && !Position.Z_Axis_Position.Equals("Down position") && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300174");
                                        return false;
                                    }
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Get, "", TaskJob.Params["@Position"], TaskJob.Params["@Slot"], TaskJob.Params["@Arm"])));
                                    break;
                                case 3:
                                    MoveWip(TaskJob.Params["@Position"], TaskJob.Params["@Slot"], TaskJob.Params["@Target"], TaskJob.Params["@Arm"]);
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ROBOT_PUT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300015");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300041");
                                        return false;
                                    }
                                    else if (Target.Busy)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300010");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "4")));
                                        if (Position.Type.ToUpper().Equals("ALIGNER"))
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Position.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetRIO, "8")));
                                        }
                                        else if (Position.Type.ToUpper().Equals("LOADPORT"))
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Position.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                        }
                                    }
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "5")));
                                    break;
                                case 2:
                                    if (!PutSafetyCheck(TaskJob, TaskReport))
                                    {
                                        return false;
                                    }
                                    else if (Position.Type.ToUpper().Equals("LOADPORT") && !Position.Z_Axis_Position.Equals("Down position") && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300174");
                                        return false;
                                    }
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Put, "", TaskJob.Params["@Position"], TaskJob.Params["@Slot"], TaskJob.Params["@Arm"])));
                                    break;
                                case 3:
                                    MoveWip(TaskJob.Params["@Target"], TaskJob.Params["@Arm"], TaskJob.Params["@Position"], TaskJob.Params["@Slot"]);
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ROBOT_RETRACT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300015");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300041");
                                        return false;
                                    }
                                    else if (Target.Busy)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300010");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.ArmReturn, "")));
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ROBOT_SERVO:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Servo, TaskJob.Params["@Value"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ROBOT_WAFER_HOLD:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.WaferHold, "", "", "", TaskJob.Params["@Arm"])));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetStatus, "")));
                                    break;
                                case 2:
                                    if (TaskJob.Params["@Arm"].Equals("1"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "4")));
                                    }
                                    break;
                                case 3:
                                    if (TaskJob.Params["@Arm"].Equals("1"))
                                    {
                                        if (!Target.R_Hold_Status.Equals("1"))
                                        {
                                            if (TaskJob.RepeatCount < 10)
                                            {
                                                TaskJob.RepeatCount++;
                                                TaskJob.CurrentIndex = TaskJob.CurrentIndex - 2;//repeat
                                                SpinWait.SpinUntil(() => false, 200);
                                            }
                                            else
                                            {
                                                TaskJob.RepeatCount = 0;
                                            }
                                        }
                                        else
                                        {
                                            TaskJob.RepeatCount = 0;
                                        }
                                    }
                                    break;
                                case 4:
                                    if (TaskJob.Params["@Arm"].Equals("2"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "5")));
                                    }
                                    break;
                                case 5:
                                    if (TaskJob.Params["@Arm"].Equals("2"))
                                    {
                                        if (!Target.L_Hold_Status.Equals("1"))
                                        {
                                            if (TaskJob.RepeatCount < 10)
                                            {
                                                TaskJob.RepeatCount++;
                                                TaskJob.CurrentIndex = TaskJob.CurrentIndex - 2;//repeat
                                                SpinWait.SpinUntil(() => false, 200);
                                            }
                                            else
                                            {
                                                TaskJob.RepeatCount = 0;
                                            }
                                        }
                                        else
                                        {
                                            TaskJob.RepeatCount = 0;
                                        }
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ROBOT_WAFER_RELEASE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.WaferRelease, "", "", "", TaskJob.Params["@Arm"])));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetStatus, "")));
                                    break;
                                case 2:
                                    if (TaskJob.Params["@Arm"].Equals("1"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "4")));
                                    }
                                    break;
                                case 3:
                                    if (TaskJob.Params["@Arm"].Equals("1"))
                                    {
                                        if (!Target.R_Hold_Status.Equals("0"))
                                        {
                                            if (TaskJob.RepeatCount < 10)
                                            {
                                                TaskJob.RepeatCount++;
                                                TaskJob.CurrentIndex = TaskJob.CurrentIndex - 2;//repeat
                                                SpinWait.SpinUntil(() => false, 200);
                                            }
                                            else
                                            {
                                                TaskJob.RepeatCount = 0;
                                            }
                                        }
                                        else
                                        {
                                            TaskJob.RepeatCount = 0;
                                        }
                                    }
                                    break;
                                case 4:
                                    if (TaskJob.Params["@Arm"].Equals("2"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "5")));
                                    }
                                    break;
                                case 5:
                                    if (TaskJob.Params["@Arm"].Equals("2"))
                                    {
                                        if (!Target.L_Hold_Status.Equals("0"))
                                        {
                                            if (TaskJob.RepeatCount < 10)
                                            {
                                                TaskJob.RepeatCount++;
                                                TaskJob.CurrentIndex = TaskJob.CurrentIndex - 2;//repeat
                                                SpinWait.SpinUntil(() => false, 200);
                                            }
                                            else
                                            {
                                                TaskJob.RepeatCount = 0;
                                            }
                                        }
                                        else
                                        {
                                            TaskJob.RepeatCount = 0;
                                        }
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ROBOT_MODE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Mode, TaskJob.Params["@Value"])));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetMode, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_RESET:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    //if (Target.Busy)
                                    //{
                                    //    AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300002");
                                    //    return false;
                                    //}
                                    //else
                                    //{
                                    TaskReport.On_Task_Ack(TaskJob);
                                    Target.Busy = true;
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Reset, "")));
                                    //}
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetError, "00")));
                                    break;
                                default:
                                    Target.Busy = false;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_SERVO:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (Target.Busy)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300002");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Servo, TaskJob.Params["@Value"])));
                                    }
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_SPEED:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Speed, TaskJob.Params["@Value"])));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetSpeed, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_WAFER_HOLD:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.WaferHold, "", "", TaskJob.Params["@Arm"])));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetSV, "1")));
                                    break;
                                case 2:
                                    if (!Target.R_Vacuum_Solenoid.Equals("1"))
                                    {
                                        if (TaskJob.RepeatCount < 10)
                                        {
                                            TaskJob.RepeatCount++;
                                            TaskJob.CurrentIndex = TaskJob.CurrentIndex - 2;//repeat
                                            SpinWait.SpinUntil(() => false, 200);
                                        }
                                        else
                                        {
                                            TaskJob.RepeatCount = 0;
                                        }
                                    }
                                    else
                                    {
                                        TaskJob.RepeatCount = 0;
                                    }
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetRIO, "8")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_WAFER_RELEASE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.WaferRelease, "", "", TaskJob.Params["@Arm"])));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetSV, "1")));
                                    break;
                                case 2:
                                    if (!Target.R_Vacuum_Solenoid.Equals("1"))
                                    {
                                        if (TaskJob.RepeatCount < 10)
                                        {
                                            TaskJob.RepeatCount++;
                                            TaskJob.CurrentIndex = TaskJob.CurrentIndex - 2;//repeat
                                            SpinWait.SpinUntil(() => false, 200);
                                        }
                                        else
                                        {
                                            TaskJob.RepeatCount = 0;
                                        }
                                    }
                                    else
                                    {
                                        TaskJob.RepeatCount = 0;
                                    }
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetRIO, "8")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_MODE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Mode, TaskJob.Params["@Value"])));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetMode, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_ALIGN:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300017");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300043");
                                        return false;
                                    }
                                    else if (Target.Busy)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300002");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Align, TaskJob.Params["@Value"])));
                                    }
                                    break;
                                default:
                                    Target.Home_Position = false;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_HOME:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300017");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300043");
                                        return false;
                                    }
                                    else if (Target.Busy)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300002");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Home, "")));
                                    }
                                    break;
                                default:
                                    Target.Home_Position = true;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_INIT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (Target.Busy)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300002");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetRIO, "8")));
                                    }
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetSpeed, "")));
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetMode, "")));
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetError, "00")));
                                    break;
                                case 4:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetSV, "1")));
                                    break;
                                case 5:
                                    if (Target.WaferSize.Equals("200"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.SetAlign, "100000")));
                                    }
                                    else if (Target.WaferSize.Equals("300"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.SetAlign, "150000")));
                                    }
                                    break;
                                case 6:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetStatus, "1")));
                                    break;
                                default:
                                    Target.Busy = false;
                                    Target.InitialComplete = true;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_ORGSH:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300017");
                                        return false;
                                    }
                                    else if (Target.Busy)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300002");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.OrginSearch, "")));
                                    }
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Home, "")));
                                    break;
                                default:
                                    Target.Home_Position = true;
                                    Target.OrgSearchComplete = true;
                                    Target.Busy = false;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_INIT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Mode, "1")));
                                    break;
                                case 2:
                                    if (Target.CarrierType.ToUpper().Equals("FOUP"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.EQASP, "0")));
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.EQASP, "1")));
                                    }
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Mode, "0")));
                                    break;
                                case 4:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetLED, "0")));
                                    break;
                                default:
                                    Target.InitialComplete = true;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_RESET:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Reset, "")));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_ORGSH:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else
                                    {
                                        if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                        {
                                            AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300166");
                                            return false;
                                        }
                                        else if (NodeManagement.Get(Target.Associated_Node).CurrentPosition.ToUpper().Equals(Target.Name.ToUpper()) || !NodeManagement.Get(Target.Associated_Node).OrgSearchComplete)
                                        {
                                            AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300041");
                                            return false;
                                        }
                                        else
                                        {
                                            TaskReport.On_Task_Ack(TaskJob);
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ForceInitialPos, "")));
                                        }
                                    }
                                    break;
                                case 1:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Load, "")));
                                    }
                                    break;
                                case 2:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Dock, "")));
                                    }
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    Target.OrgSearchComplete = true;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_CLAMP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else if (Target.Foup_Lock && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300142");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Clamp, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_UNCLAMP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Lock && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300167");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.UnClamp, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_OPEN:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                        {
                                            if (NodeManagement.Get(Target.Associated_Node).Busy && !SystemConfig.Get().SaftyCheckByPass)
                                            {
                                                AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300010");
                                                return false;
                                            }
                                            else if (!Target.Z_Axis_Position.Equals("Down position") && !SystemConfig.Get().SaftyCheckByPass)
                                            {
                                                AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300174");
                                                return false;
                                            }
                                            else if (Target.WaferProtrusionSensor && !SystemConfig.Get().SaftyCheckByPass)
                                            {
                                                AbortTask(TaskReport, TaskJob, Target.Name, "ABS", "S0300020");
                                                return false;
                                            }
                                            else
                                            {//Robot pre-mapping
                                                NodeManagement.Get(Target.Associated_Node).Busy = true;
                                                if (SystemConfig.Get().PreMapping)
                                                {
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.PreMapping, "", Target.Name)));
                                                }
                                            }
                                        }
                                        else
                                        {

                                        }
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetOpAccess, "0")));
                                    }
                                    break;
                                case 2:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        if (SystemConfig.Get().PreMapping)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetMapping, "")));

                                        }
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.MappingLoad, "")));
                                    }
                                    break;
                                case 3:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorDown, "")));
                                        if (SystemConfig.Get().PreMapping)
                                        {
                                            bool findWafer = false;
                                            string Mapping = Target.MappingResult;
                                            for (int i = 0; i < Mapping.Length; i++)
                                            {
                                                switch (Mapping[i])
                                                {
                                                    case '0':
                                                        break;
                                                    default:
                                                        findWafer = true;
                                                        break;
                                                }
                                                if (findWafer)
                                                {
                                                    break;
                                                }
                                            }
                                            if (findWafer)
                                            {
                                                AbortTask(TaskReport, TaskJob, Target.Name, "ABS", "S0300020");
                                                return false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetMapping, "")));
                                    }
                                    break;
                                case 4:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Mapping, "", Target.Name)));
                                    }
                                    break;
                                case 5:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetMapping, "")));
                                    }
                                    break;
                                case 6:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;

                                default:
                                    Target.IsLoad = true;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_OPEN_NOMAP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Load, "")));
                                        }
                                        else
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Load, "")));
                                        }
                                    }
                                    break;
                                case 2:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorDown, "")));
                                    }
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    Target.IsLoad = true;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_CLOSE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    //else if (NodeManagement.Get(Target.Associated_Node).CurrentPosition.ToUpper().Equals(Target.Name.ToUpper()) || !NodeManagement.Get(Target.Associated_Node).OrgSearchComplete)
                                    //{
                                    //    AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300041");
                                    //    return false;
                                    //}
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetPosition, "2")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else if (Convert.ToInt32(NodeManagement.Get(Target.Associated_Node).R_Position) > 500 || Convert.ToInt32(NodeManagement.Get(Target.Associated_Node).L_Position) > 500)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300019");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                        {
                                            if (!Target.Door_Position.Equals("Open position") && !Target.Z_Axis_Position.Equals("Down position") && !SystemConfig.Get().SaftyCheckByPass)
                                            {
                                                AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300174");
                                                return false;
                                            }
                                            else
                                            {//Robot mapping
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Mapping, "", Target.Name)));
                                            }
                                        }
                                        else
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.MappingUnload, "")));
                                        }
                                    }
                                    break;
                                case 2:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorUp, "")));
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetMapping, "")));
                                    }
                                    break;
                                case 3:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorClose, "")));
                                    }
                                    break;
                                case 4:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetMapping, "")));
                                    }
                                    break;
                                case 5:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    Target.IsLoad = false;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_CLOSE_NOMAP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    //else if (NodeManagement.Get(Target.Associated_Node).CurrentPosition.ToUpper().Equals(Target.Name.ToUpper()) || !NodeManagement.Get(Target.Associated_Node).OrgSearchComplete)
                                    //{
                                    //    AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300041");
                                    //    return false;
                                    //}
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetPosition, "2")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else if (Convert.ToInt32(NodeManagement.Get(Target.Associated_Node).R_Position) > 500 || Convert.ToInt32(NodeManagement.Get(Target.Associated_Node).L_Position) > 500)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300019");
                                        return false;
                                    }
                                    else
                                    {
                                        if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                        {
                                            if (!Target.Door_Position.Equals("Open position") && !Target.Z_Axis_Position.Equals("Down position") && !SystemConfig.Get().SaftyCheckByPass)
                                            {
                                                AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300174");
                                                return false;
                                            }
                                            else
                                            {
                                                TaskReport.On_Task_Ack(TaskJob);
                                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorUp, "")));
                                            }
                                        }
                                        else
                                        {
                                            TaskReport.On_Task_Ack(TaskJob);
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Unload, "")));
                                        }
                                    }
                                    break;
                                case 2:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorClose, "")));
                                        Target.IsMapping = false;
                                        Target.MappingResult = "";
                                        //刪除所有帳
                                        foreach (Job eachJob in Target.JobList.Values)
                                        {
                                            JobManagement.Remove(eachJob.Job_Id);
                                        }
                                        Target.JobList.Clear();
                                        Target.ReserveList.Clear();
                                        JobManagement.ClearAssignJobByPort(Target.Name);
                                        Target.FoupID = "";
                                    }
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    Target.IsLoad = false;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_DOCK:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Y_Axis_Position.Equals("Undock position") && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300173");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Dock, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_UNDOCK:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Y_Axis_Position.Equals("Dock position") && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300145");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.UnDock, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_DOOR_CLOSE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorClose, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_DOOR_DOWN:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorDown, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_DOOR_OPEN:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorOpen, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_DOOR_UP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else if (NodeManagement.Get(Target.Associated_Node).CurrentPosition.ToUpper().Equals(Target.Name.ToUpper()) || !NodeManagement.Get(Target.Associated_Node).OrgSearchComplete)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300041");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetPosition, "2")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else if (Convert.ToInt32(NodeManagement.Get(Target.Associated_Node).R_Position) > 500 || Convert.ToInt32(NodeManagement.Get(Target.Associated_Node).L_Position) > 500)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300019");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorUp, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_FORCE_ORGSH:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else if (NodeManagement.Get(Target.Associated_Node).CurrentPosition.ToUpper().Equals(Target.Name.ToUpper()) || !NodeManagement.Get(Target.Associated_Node).OrgSearchComplete)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300041");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetPosition, "2")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else if (Convert.ToInt32(NodeManagement.Get(Target.Associated_Node).R_Position) >500 || Convert.ToInt32(NodeManagement.Get(Target.Associated_Node).L_Position) > 500)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300019");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ForceInitialPos, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_GET_MAPDT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else if (!Target.IsMapping && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300156");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetMapping, "")));
                                        }
                                        else
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetMapping, "")));
                                        }
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_LATCH:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else if (!Target.Latch_Open && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300144");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.LatchDoor, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_UNLATCH:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else if (Target.Latch_Open && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300143");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.UnLatchDoor, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_RE_MAPPING:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else if (NodeManagement.Get(Target.Associated_Node).CurrentPosition.ToUpper().Equals(Target.Name.ToUpper()) || !NodeManagement.Get(Target.Associated_Node).OrgSearchComplete)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300041");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetPosition, "2")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else if (!Target.Door_Position.Equals("Open position") && !Target.Z_Axis_Position.Equals("Down position") && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300174");
                                        return false;
                                    }
                                    else if (Convert.ToInt32(NodeManagement.Get(Target.Associated_Node).R_Position) > 500 || Convert.ToInt32(NodeManagement.Get(Target.Associated_Node).L_Position) > 500)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300019");
                                        return false;
                                    }
                                    else
                                    {
                                        if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                        {
                                            if (NodeManagement.Get(Target.Associated_Node).Busy && !SystemConfig.Get().SaftyCheckByPass)
                                            {
                                                AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300010");
                                                return false;
                                            }
                                            else
                                            {//Robot mapping
                                                TaskReport.On_Task_Ack(TaskJob);
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Mapping, "", Target.Name)));
                                            }
                                        }
                                        else
                                        {
                                            TaskReport.On_Task_Ack(TaskJob);
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorUp, "")));
                                        }
                                    }
                                    break;
                                case 2:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetMapping, "")));
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorUp, "")));
                                    }
                                    break;
                                case 3:
                                    if (!Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorOpen, "")));
                                    }
                                    break;
                                case 4:
                                    if (!Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.MapperStartPosition, "")));
                                    }
                                    break;
                                case 5:
                                    if (!Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.MapperArmStretch, "")));
                                    }
                                    break;
                                case 6:
                                    if (!Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.MappingDown, "")));
                                    }
                                    break;
                                case 7:
                                    if (!Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.MapperArmRetracted, "")));
                                    }
                                    break;
                                case 8:
                                    if (!Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorDown, "")));
                                    }
                                    break;
                                case 9:
                                    if (!Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetMapping, "")));
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_READ_LED:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetLED, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_READ_STATUS:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_READ_VERSION:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadVersion, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_READYTOLOAD:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetUnLoad, "0")));
                                    break;
                                case 1:
                                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetLoad, "0")));
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetOpAccess, "2")));

                                    break;
                                case 3:
                                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetLED, "0")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_UNLOADCOMPLETE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetOpAccess, "0")));
                                    break;
                                case 1:
                                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetLoad, "0")));
                                    break;
                                case 2:
                                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetUnLoad, "0")));

                                    break;
                                case 3:
                                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetLED, "0")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.DISABLE_OPACCESS:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    foreach (Node port in NodeManagement.GetLoadPortList())
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(port.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetOpAccess, "0")));
                                    }
                                    break;
                                case 1:
                                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetLoad, "0")));
                                    break;
                                case 2:
                                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetUnLoad, "0")));

                                    break;
                                case 3:
                                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetLED, "0")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_VAC_ON:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.VacuumON, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_VAC_OFF:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.VacuumOFF, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_ALIGNER_ALIGN:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Align, Target.Name.ToUpper().Equals("ALIGNER01") ? Recipe.Get(SystemConfig.Get().CurrentRecipe).aligner1_angle : Recipe.Get(SystemConfig.Get().CurrentRecipe).aligner2_angle)));
                                    break;
                                case 1:
                                    //newAct = new Node.ActionRequest();
                                    //newAct.TaskName = (TaskFlowManagement.Command)Enum.Parse(typeof(TaskFlowManagement.Command), "TRANSFER_GETW_" + Target.Name);
                                    //newAct.Position = Target.Name;
                                    //NodeManagement.Get(TaskJob.Params["@ULDRobot"]).RequestQueue.Add(newAct.TaskName, newAct);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.WaferRelease, "")));
                                    break;
                                case 2:
                                    newAct = new Node.ActionRequest();
                                    newAct.TaskName = TaskFlowManagement.Command.TRANSFER_GET_LOADPORT;
                                    newAct.Position = TaskJob.Params["@Loadport"];
                                    if (!NodeManagement.Get(TaskJob.Params["@ULDRobot"]).RequestQueue.ContainsKey(newAct.TaskName))
                                    {
                                        NodeManagement.Get(TaskJob.Params["@ULDRobot"]).RequestQueue.Add(newAct.TaskName, newAct);
                                    }
                                    newAct = new Node.ActionRequest();
                                    newAct.TaskName = (TaskFlowManagement.Command)Enum.Parse(typeof(TaskFlowManagement.Command), "TRANSFER_GET_" + Target.Name);
                                    newAct.Position = Target.Name;
                                    if (!NodeManagement.Get(TaskJob.Params["@ULDRobot"]).RequestQueue.ContainsKey(newAct.TaskName))
                                    {
                                        NodeManagement.Get(TaskJob.Params["@ULDRobot"]).RequestQueue.Add(newAct.TaskName, newAct);
                                    }
                                    Target.ReadyForGet = true;
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_ALIGNER_WHLD:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.WaferHold, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_ALIGNER_HOME:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Home, "")));
                                    break;
                                default:
                                    Target.ReadyForPut = true;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_ALIGNER_WRLS:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.WaferRelease, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_GETW_ALIGNER01:
                        case TaskFlowManagement.Command.TRANSFER_GETW_ALIGNER02:
                            AlignerName = TaskJob.TaskName == TaskFlowManagement.Command.TRANSFER_GETW_ALIGNER01 ? "ALIGNER01" : "ALIGNER02";
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetWait, "", AlignerName, "1", TaskJob.Params["@Arm"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_GET_ALIGNER01:
                        case TaskFlowManagement.Command.TRANSFER_GET_ALIGNER02:
                            AlignerName = TaskJob.TaskName == TaskFlowManagement.Command.TRANSFER_GET_ALIGNER01 ? "ALIGNER01" : "ALIGNER02";
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "4")));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "5")));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(AlignerName, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetRIO, "8")));
                                    break;
                                case 2:
                                    if (!GetSafetyCheck(TaskJob, TaskReport))
                                    {
                                        return false;
                                    }
                                    TaskReport.On_Task_Ack(TaskJob);
                                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.WaitBeforeGet, "", AlignerName, "1", TaskJob.Params["@Arm"])));
                                    break;
                                case 3:
                                    newAct = new Node.ActionRequest();
                                    newAct.TaskName = (TaskFlowManagement.Command)Enum.Parse(typeof(TaskFlowManagement.Command), "TRANSFER_GET_" + AlignerName + "_1");
                                    newAct.Position = AlignerName;
                                    newAct.Slot = "1";
                                    newAct.Arm = TaskJob.Params["@Arm"];
                                    if (!Target.RequestQueue.ContainsKey(newAct.TaskName))
                                    {
                                        Target.RequestQueue.Add(newAct.TaskName, newAct);
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_GET_ALIGNER01_1:
                        case TaskFlowManagement.Command.TRANSFER_GET_ALIGNER02_1:
                            AlignerName = TaskJob.TaskName == TaskFlowManagement.Command.TRANSFER_GET_ALIGNER01_1 ? "ALIGNER01" : "ALIGNER02";
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetWithoutBack, "", AlignerName, "1", TaskJob.Params["@Arm"])));
                                    break;
                                case 1:
                                    newAct = new Node.ActionRequest();
                                    newAct.TaskName = (TaskFlowManagement.Command)Enum.Parse(typeof(TaskFlowManagement.Command), "TRANSFER_GET_" + AlignerName + "_2");
                                    newAct.Position = AlignerName;
                                    newAct.Slot = "1";
                                    newAct.Arm = TaskJob.Params["@Arm"];
                                    if (!Target.RequestQueue.ContainsKey(newAct.TaskName))
                                    {
                                        Target.RequestQueue.Add(newAct.TaskName, newAct);
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_GET_ALIGNER01_2:
                        case TaskFlowManagement.Command.TRANSFER_GET_ALIGNER02_2:
                            AlignerName = TaskJob.TaskName == TaskFlowManagement.Command.TRANSFER_GET_ALIGNER01_2 ? "ALIGNER01" : "ALIGNER02";
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    MoveWip(AlignerName, "1", Target.Name, TaskJob.Params["@Arm"]);
                                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetAfterWait, "", AlignerName, "1", TaskJob.Params["@Arm"])));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Get, "", AlignerName, "1", TaskJob.Params["@Arm"])));
                                    break;
                                case 1:
                                    newAct = new Node.ActionRequest();
                                    newAct.TaskName = TaskFlowManagement.Command.TRANSFER_PUT_UNLOADPORT;
                                    if (!Target.RequestQueue.ContainsKey(newAct.TaskName))
                                    {
                                        Target.RequestQueue.Add(newAct.TaskName, newAct);
                                    }
                                    newAct = new Node.ActionRequest();
                                    newAct.TaskName = TaskFlowManagement.Command.TRANSFER_ALIGNER_HOME;
                                    if (!NodeManagement.Get(AlignerName).RequestQueue.ContainsKey(newAct.TaskName))
                                    {
                                        NodeManagement.Get(AlignerName).RequestQueue.Add(newAct.TaskName, newAct);
                                    }

                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_GET_LOADPORT:
                        case TaskFlowManagement.Command.TRANSFER_GET_LOADPORT_2ARM:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "4")));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "5")));
                                    break;
                                case 2:

                                    if (!Position.IsMapping && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "ABS", "S0300162");
                                        return false;
                                    }
                                    if (!GetSafetyCheck(TaskJob, TaskReport))
                                    {
                                        return false;
                                    }
                                    TaskReport.On_Task_Ack(TaskJob);
                                    if (TaskJob.Params["@Arm"].Equals("3"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.DoubleGet, "", TaskJob.Params["@Position"], TaskJob.Params["@Slot"], TaskJob.Params["@Arm"])));
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Get, "", TaskJob.Params["@Position"], TaskJob.Params["@Slot"], TaskJob.Params["@Arm"])));
                                    }
                                    break;
                                case 3:

                                    if (TaskJob.TaskName == TaskFlowManagement.Command.TRANSFER_GET_LOADPORT_2ARM)
                                    {
                                        MoveWip(TaskJob.Params["@Position"], TaskJob.Params["@Slot"], Target.Name, "1");
                                        MoveWip(TaskJob.Params["@Position"], (Convert.ToInt16(TaskJob.Params["@Slot"]) - 1).ToString(), Target.Name, "2");
                                    }
                                    else
                                    {
                                        MoveWip(TaskJob.Params["@Position"], TaskJob.Params["@Slot"], Target.Name, TaskJob.Params["@Arm"]);
                                    }
                                    newAct = new Node.ActionRequest();
                                    newAct.TaskName = TaskFlowManagement.Command.TRANSFER_GET_LOADPORT;
                                    newAct.Position = TaskJob.Params["@Position"];
                                    if (!Target.RequestQueue.ContainsKey(newAct.TaskName))
                                    {
                                        Target.RequestQueue.Add(newAct.TaskName, newAct);
                                        Target.ForcePutToUnload = true;
                                    }
                                    if (NodeManagement.GetAlignerList().Count == 0)
                                    {
                                        newAct = new Node.ActionRequest();
                                        newAct.TaskName = TaskFlowManagement.Command.TRANSFER_PUT_UNLOADPORT;
                                        if (!Target.RequestQueue.ContainsKey(newAct.TaskName))
                                        {
                                            Target.RequestQueue.Add(newAct.TaskName, newAct);
                                        }
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_PUTW_ALIGNER01:
                        case TaskFlowManagement.Command.TRANSFER_PUTW_ALIGNER02:
                            AlignerName = TaskJob.TaskName == TaskFlowManagement.Command.TRANSFER_PUTW_ALIGNER01 ? "ALIGNER01" : "ALIGNER02";
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    if (NodeManagement.Get(AlignerName).Enable)
                                    {
                                        Target.ForcePutToUnload = false;
                                        //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.PutWait, "", AlignerName, "1", TaskJob.Params["@Arm"])));
                                    }
                                    else
                                    {//直送Unloader
                                        Target.ForcePutToUnload = true;
                                        Target.LockOn = "";
                                        newAct = new Node.ActionRequest();
                                        newAct.TaskName = TaskFlowManagement.Command.TRANSFER_PUT_UNLOADPORT;
                                        if (!Target.RequestQueue.ContainsKey(newAct.TaskName))
                                        {
                                            Target.RequestQueue.Add(newAct.TaskName, newAct);
                                        }
                                    }
                                    break;
                                case 1:
                                    if (NodeManagement.Get(AlignerName).Enable)
                                    {
                                        newAct = new Node.ActionRequest();
                                        newAct.TaskName = (TaskFlowManagement.Command)Enum.Parse(typeof(TaskFlowManagement.Command), "TRANSFER_PUT_" + AlignerName);
                                        newAct.Position = AlignerName;
                                        newAct.Slot = "1";
                                        newAct.Arm = TaskJob.Params["@Arm"];
                                        if (!NodeManagement.Get(TaskJob.Params["@LDRobot"]).RequestQueue.ContainsKey(newAct.TaskName))
                                        {
                                            NodeManagement.Get(TaskJob.Params["@LDRobot"]).RequestQueue.Add(newAct.TaskName, newAct);
                                        }
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_PUT_ALIGNER01:
                        case TaskFlowManagement.Command.TRANSFER_PUT_ALIGNER02:
                            AlignerName = TaskJob.TaskName == TaskFlowManagement.Command.TRANSFER_PUT_ALIGNER01 ? "ALIGNER01" : "ALIGNER02";
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "4")));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "5")));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(AlignerName, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetRIO, "8")));
                                    break;
                                case 2:
                                    if (!PutSafetyCheck(TaskJob, TaskReport))
                                    {
                                        return false;
                                    }
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.WaitBeforePut, "", AlignerName, "1", TaskJob.Params["@Arm"])));
                                    break;
                                case 3:
                                    MoveWip(Target.Name, TaskJob.Params["@Arm"], AlignerName, "1");
                                    newAct = new Node.ActionRequest();
                                    newAct.TaskName = TaskFlowManagement.Command.TRANSFER_ALIGNER_WHLD;
                                    if (!NodeManagement.Get(AlignerName).RequestQueue.ContainsKey(newAct.TaskName))
                                    {
                                        NodeManagement.Get(AlignerName).RequestQueue.Add(newAct.TaskName, newAct);
                                    }
                                    newAct = new Node.ActionRequest();
                                    newAct.TaskName = (TaskFlowManagement.Command)Enum.Parse(typeof(TaskFlowManagement.Command), "TRANSFER_PUT_" + AlignerName + "_1");
                                    newAct.Position = AlignerName;
                                    newAct.Slot = "1";
                                    newAct.Arm = TaskJob.Params["@Arm"];
                                    if (!Target.RequestQueue.ContainsKey(newAct.TaskName))
                                    {
                                        Target.RequestQueue.Add(newAct.TaskName, newAct);
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_PUT_ALIGNER01_1:
                        case TaskFlowManagement.Command.TRANSFER_PUT_ALIGNER02_1:
                            AlignerName = TaskJob.TaskName == TaskFlowManagement.Command.TRANSFER_PUT_ALIGNER01_1 ? "ALIGNER01" : "ALIGNER02";
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.PutWithoutBack, "", AlignerName, "1", TaskJob.Params["@Arm"])));
                                    break;
                                case 1:
                                    newAct = new Node.ActionRequest();
                                    newAct.TaskName = (TaskFlowManagement.Command)Enum.Parse(typeof(TaskFlowManagement.Command), "TRANSFER_PUT_" + AlignerName + "_2");
                                    newAct.Position = AlignerName;
                                    newAct.Slot = "1";
                                    newAct.Arm = TaskJob.Params["@Arm"];
                                    if (!NodeManagement.Get(TaskJob.Params["@LDRobot"]).RequestQueue.ContainsKey(newAct.TaskName))
                                    {
                                        NodeManagement.Get(TaskJob.Params["@LDRobot"]).RequestQueue.Add(newAct.TaskName, newAct);
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_PUT_ALIGNER01_2:
                        case TaskFlowManagement.Command.TRANSFER_PUT_ALIGNER02_2:
                            AlignerName = TaskJob.TaskName == TaskFlowManagement.Command.TRANSFER_PUT_ALIGNER01_2 ? "ALIGNER01" : "ALIGNER02";
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.PutBack, "", AlignerName, "1", TaskJob.Params["@Arm"])));
                                    break;
                                case 1:
                                    if (Target.JobList.Count == 0)
                                    {
                                        newAct = new Node.ActionRequest();
                                        newAct.TaskName = TaskFlowManagement.Command.TRANSFER_GET_LOADPORT;
                                        newAct.Position = TaskJob.Params["@Loadport"];
                                        if (!Target.RequestQueue.ContainsKey(newAct.TaskName))
                                        {
                                            Target.RequestQueue.Add(newAct.TaskName, newAct);
                                        }
                                    }
                                    newAct = new Node.ActionRequest();
                                    newAct.TaskName = TaskFlowManagement.Command.TRANSFER_ALIGNER_ALIGN;
                                    if (!NodeManagement.Get(AlignerName).RequestQueue.ContainsKey(newAct.TaskName))
                                    {
                                        NodeManagement.Get(AlignerName).RequestQueue.Add(newAct.TaskName, newAct);
                                    }
                                    newAct = new Node.ActionRequest();
                                    newAct.TaskName = TaskFlowManagement.Command.TRANSFER_PUT_UNLOADPORT;
                                    if (!Target.RequestQueue.ContainsKey(newAct.TaskName))
                                    {
                                        Target.RequestQueue.Add(newAct.TaskName, newAct);
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_PUT_UNLOADPORT:
                        case TaskFlowManagement.Command.TRANSFER_PUT_UNLOADPORT_2ARM:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "4")));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.GetRIO, "5")));
                                    break;
                                case 2:
                                    if (!Position.IsMapping && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        AbortTask( TaskReport,TaskJob, Target.Name, "ABS", "S0300162");
                                        return false;
                                    }
                                    if (!PutSafetyCheck(TaskJob, TaskReport))
                                    {
                                        return false;
                                    }
                                    TaskReport.On_Task_Ack(TaskJob);


                                    if (Target.JobList.Count != 0)
                                    {
                                        if (TaskJob.TaskName == TaskFlowManagement.Command.TRANSFER_PUT_UNLOADPORT_2ARM)
                                        {
                                            MoveWip(Target.Name, "1", TaskJob.Params["@Position"], TaskJob.Params["@Slot"]);
                                            MoveWip(Target.Name, "2", TaskJob.Params["@Position"], (Convert.ToInt16(TaskJob.Params["@Slot"]) - 1).ToString());
                                        }
                                        else
                                        {
                                            MoveWip(Target.Name, TaskJob.Params["@Arm"], TaskJob.Params["@Position"], TaskJob.Params["@Slot"]);
                                        }
                                        if (TaskJob.Params["@Arm"].Equals("3"))
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.DoublePut, "", TaskJob.Params["@Position"], TaskJob.Params["@Slot"], TaskJob.Params["@Arm"])));
                                        }
                                        else
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Put, "", TaskJob.Params["@Position"], TaskJob.Params["@Slot"], TaskJob.Params["@Arm"])));
                                        }
                                    }
                                    break;
                                case 3:
                                    newAct = new Node.ActionRequest();
                                    newAct.TaskName = TaskFlowManagement.Command.TRANSFER_PUT_UNLOADPORT;
                                    newAct.Position = TaskJob.Params["@Position"];
                                    newAct.Slot = TaskJob.Params["@Slot"];
                                    newAct.Arm = "1";
                                    if (!Target.RequestQueue.ContainsKey(newAct.TaskName))
                                    {
                                        Target.RequestQueue.Add(newAct.TaskName, newAct);
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    if (TaskJob.CheckList.Count != 0)
                    {
                        foreach (TaskFlowManagement.ExcutedCmd eachCmd in TaskJob.CheckList)
                        {
                            if (NodeManagement.Get(eachCmd.NodeName).Enable)
                            {
                                NodeManagement.Get(eachCmd.NodeName).SendCommand(eachCmd.Txn, out Message);
                                if (eachCmd.Txn.Method == Transaction.Command.LoadPortType.GetMappingDummy)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                eachCmd.Finished = true;
                            }
                        }
                    }
                    else
                    {//recursive
                        TaskJob.CurrentIndex++;
                        if (!this.Excute(TaskJob, TaskReport))
                        {
                            return false;
                        }
                    }
                }


            }
            catch (Exception e)
            {
                logger.Error("Excute fail Task Name:" + TaskJob.TaskName.ToString() + " exception: " + e.StackTrace);
                AbortTask( TaskReport,TaskJob, "SYSTEM", "ABS", e.StackTrace);
                return false;
            }
            return true;
        }
        public void SetRequestQueue(string TaskName, string Value, string Position = "", string Arm = "", string Slot = "", string Position2 = "", string Arm2 = "", string Slot2 = "")
        {

        }
        private bool CheckEMO(TaskFlowManagement.CurrentProcessTask TaskJob, ITaskFlowReport TaskReport)
        {
            bool result = true;
            if (!SystemConfig.Get().SaftyCheckByPass)
            {
                if (RouteControl.Instance.DIO.GetIO("DIN", "SAFETYRELAY").ToUpper().Equals("TRUE"))
                {
                    TaskFlowManagement.TaskRemove(TaskJob.Id);
                    AbortTask( TaskReport,TaskJob, "SYSTEM", "CAN", "S0300170");
                    result = false;
                }
            }
            return result;
        }
        private bool GetSafetyCheck(TaskFlowManagement.CurrentProcessTask TaskJob, ITaskFlowReport TaskReport)
        {
            if (!SystemConfig.Get().SaftyCheckByPass)
            {
                Node Target = NodeManagement.Get(TaskJob.Params["@Target"]);
                Node Position = NodeManagement.Get(TaskJob.Params["@Position"]);
                string Arm = TaskJob.Params["@Arm"];
                string Slot = TaskJob.Params["@Slot"];
                if ((Arm.Equals("1") || Arm.Equals("3")) && Target.R_Hold_Status.Equals("HLD"))
                {
                    AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300007");//Arm1 上已經有 wafer
                    return false;
                }
                if ((Arm.Equals("2") || Arm.Equals("3")) && Target.R_Hold_Status.Equals("HLD"))
                {
                    AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300009");//Arm2 上已經有 wafer
                    return false;
                }
                switch (Position.Type.ToUpper())
                {
                    case "LOADPORT":
                        if (!Position.IsMapping)
                        {
                            AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300156");
                            return false;
                        }
                        switch (Arm)
                        {
                            case "1"://R Arm  
                            case "2"://L Arm
                                if (!Position.JobList[Slot].MapFlag || Position.JobList[Slot].ErrPosition)
                                {
                                    AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300162");//取片來源地點無Wafer資料
                                    return false;
                                }
                                break;
                            case "3"://Double Arm
                                if (!Position.JobList[Slot].MapFlag || Position.JobList[Slot].ErrPosition)
                                {
                                    AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300162");//取片來源地點無Wafer資料
                                    return false;
                                }
                                if (!Position.JobList[(Convert.ToInt16(Slot) - 1).ToString()].MapFlag || Position.JobList[(Convert.ToInt16(Slot) - 1).ToString()].ErrPosition)
                                {
                                    AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300162");//取片來源地點無Wafer資料
                                    return false;
                                }
                                break;
                        }
                        break;
                    case "ALIGNER":
                        if (!Position.R_Presence)
                        {
                            AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300003");//Aligner上沒有 wafer
                            return false;
                        }
                        break;
                    case "LOADLOCK":

                        break;
                    default:
                        logger.Error("Position.Type is not define.");
                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300018");
                        return false;
                }
            }
            return true;
        }
        private bool PutSafetyCheck(TaskFlowManagement.CurrentProcessTask TaskJob, ITaskFlowReport TaskReport)
        {
            if (!SystemConfig.Get().SaftyCheckByPass)
            {
                Node Target = NodeManagement.Get(TaskJob.Params["@Target"]);
                Node Position = NodeManagement.Get(TaskJob.Params["@Position"]);
                string Arm = TaskJob.Params["@Arm"];
                string Slot = TaskJob.Params["@Slot"];
                if ((Arm.Equals("1") || Arm.Equals("3")) && Target.R_Hold_Status.Equals("REL"))
                {
                    AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300006");//Arm1 上無 wafer
                    return false;
                }
                if ((Arm.Equals("2") || Arm.Equals("3")) && Target.R_Hold_Status.Equals("REL"))
                {
                    AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300008");//Arm2 上無 wafer
                    return false;
                }
                switch (Position.Type.ToUpper())
                {
                    case "LOADPORT":
                        if (!Position.IsMapping)
                        {
                            AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300156");//Load Port 尚未執行過 mapping
                            return false;
                        }
                        switch (Arm)
                        {
                            case "1"://R Arm  
                            case "2"://L Arm
                                if (Position.JobList[Slot].MapFlag || Position.JobList[Slot].ErrPosition)
                                {
                                    AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300171");//放片目的地已有Wafer
                                    return false;
                                }
                                break;
                            case "3"://Double Arm
                                if (Position.JobList[Slot].MapFlag || Position.JobList[Slot].ErrPosition)
                                {
                                    AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300171");//放片目的地已有Wafer
                                    return false;
                                }
                                if (Position.JobList[(Convert.ToInt16(Slot) - 1).ToString()].MapFlag || Position.JobList[(Convert.ToInt16(Slot) - 1).ToString()].ErrPosition)
                                {
                                    AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300171");//放片目的地已有Wafer
                                    return false;
                                }
                                break;
                        }
                        break;
                    case "ALIGNER":
                        if (Position.R_Presence)
                        {
                            AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300004");//Aligner 上已經有 wafer
                            return false;
                        }
                        break;
                    case "LOADLOCK":

                        break;
                    default:
                        logger.Error("Position.Type is not define.");
                        AbortTask( TaskReport,TaskJob, Target.Name, "CAN", "S0300018");
                        return false;
                }
            }
            return true;
        }
        private void MoveWip(string FromPosition, string FromSlot, string ToPosition, string ToSlot)
        {
            try
            {
                Node FNode = NodeManagement.Get(FromPosition);
                if (!FNode.Enable)
                {
                    return;
                }
                Node TNode = NodeManagement.Get(ToPosition);
                if (!TNode.Enable)
                {
                    return;
                }
                Job J;
                Job tmp;
                if (!FNode.JobList.TryRemove(FromSlot, out J))
                {
                    J = RouteControl.CreateJob();//當沒有帳時強制建帳
                    J.Job_Id = JobManagement.GetNewID();
                    J.Host_Job_Id = J.Job_Id;
                    J.Position = FNode.Name;
                    J.Slot = FromSlot;
                    J.MapFlag = true;
                    J.MappingValue = "0";
                    JobManagement.Add(J.Job_Id, J);
                }
                if (FNode.Type.ToUpper().Equals("LOADPORT"))
                {
                    //LOADPORT空的Slot要塞空資料                                       
                    tmp = RouteControl.CreateJob();
                    tmp.Job_Id = "No wafer";
                    tmp.Host_Job_Id = "No wafer";
                    tmp.Slot = FromSlot;
                    tmp.Position = FNode.Name;
                    tmp.MappingValue = "0";
                    FNode.JobList.TryAdd(tmp.Slot, tmp);
                    //從LOADPORT取出，處理開始
                    J.InProcess = true;
                    J.StartTime = DateTime.Now;
                    J.FromPort = FNode.Name;
                    J.FromPortSlot = FromSlot;
                    J.FromFoupID = FNode.FoupID;
                }
                if (TNode.Type.ToUpper().Equals("LOADPORT"))
                {
                    //放回UNLOADPORT，處理結束
                    J.InProcess = false;
                    J.NeedProcess = false;
                    J.EndTime = DateTime.Now;
                    J.ToFoupID = TNode.FoupID;
                    J.ToPort = TNode.Name;
                    J.ToPortSlot = ToSlot;
                }
                if (TNode.Type.ToUpper().Equals("ALIGNER"))
                {
                    J.AlignerFlag = true;
                }
                J.LastNode = J.Position;
                J.LastSlot = J.Slot;

                TNode.JobList.TryRemove(ToSlot, out tmp);
                if (TNode.JobList.TryAdd(ToSlot, J))
                {
                    //更新WAFER位置
                    J.Position = TNode.Name;
                    J.Slot = ToSlot;
                    J.PositionChangeReport();
                }
                else
                {
                    logger.Error("Move wip error(Add): From=" + FromPosition + " Slot=" + FromSlot + " To=" + ToPosition + " Slot=" + ToSlot);
                }
                FNode.RefreshMap();
                TNode.RefreshMap();
            }
            catch (Exception e)
            {
                logger.Error("Move wip fail:" + e.Message + " exception: " + e.StackTrace);
            }
        }
        private void AbortTask(ITaskFlowReport TaskReport, TaskFlowManagement.CurrentProcessTask TaskJob, string Location, string ReportType, string Message)
        {
            
            TaskReport.On_Task_Abort(TaskJob, Location, ReportType, Message);
            TaskJob.HasError = true;
        }
    }
}
