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
    class WTS : ITaskFlow
    {
        ILog logger = LogManager.GetLogger(typeof(WTS));
        private TaskFlowManagement.Command WTS_LastCmd = 0;
        private TaskFlowManagement.Command STK_LastCmd = 0;
        private Dictionary<string, string> WTS_LastCmdParam = null;
        private Dictionary<string, string> STK_LastCmdParam = null;
        public bool Excute(TaskFlowManagement.CurrentProcessTask TaskJob, ITaskFlowReport TaskReport)
        {
            logger.Debug("ITaskFlow:" + TaskJob.TaskName.ToString() + " Index:" + TaskJob.CurrentIndex.ToString());
            if (!TransferControl.Instance.GetOnline() && TaskJob.TaskName != TaskFlowManagement.Command.CONTROL_MODE)
            {
                AbortTask(TaskReport, TaskJob, "SYSTEM", "ABS", "2012");
                return false;
            }
                string Message = "";
            Node Target = null;
            Node Position = null;
            //TaskFlowManagement.CurrentProcessTask WaitTask = null;

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
                        case TaskFlowManagement.Command.CLAMP_ELPT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.Clamp, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.UNCLAMP_ELPT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.UnClamp, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.MOVE_FOUP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    STK_LastCmd = TaskFlowManagement.Command.MOVE_FOUP;
                                    STK_LastCmdParam = TaskJob.Params;
                                    TaskReport.On_Task_Ack(TaskJob);

                                    if (TaskJob.Params["@FromPosition"].Contains("ELPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(TaskJob.Params["@FromPosition"], "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveIn, "")));
                                    }
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.PreparePick, "", TaskJob.Params["@FromPosition"], "", "", TaskJob.Params["@ToPosition"])));

                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Pick, "", TaskJob.Params["@FromPosition"], "", "", TaskJob.Params["@ToPosition"])));

                                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Transfer, "", TaskJob.Params["@FromPosition"], "", "", TaskJob.Params["@ToPosition"])));
                                    break;
                                case 2:
                                    if (TaskJob.Params["@FromPosition"].Contains("ELPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(TaskJob.Params["@FromPosition"], "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveOut, "")));
                                    }
                                    if (TaskJob.Params["@ToPosition"].Contains("ELPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.PreparePlace, "", TaskJob.Params["@ToPosition"], "", "", TaskJob.Params["@ToPosition"])));

                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(TaskJob.Params["@ToPosition"], "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveIn, "")));

                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Place, "", TaskJob.Params["@ToPosition"], "", "", TaskJob.Params["@ToPosition"])));

                                    }


                                    break;
                                case 3:
                                    if (TaskJob.Params["@ToPosition"].Contains("ELPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Place, "", TaskJob.Params["@ToPosition"], "", "", TaskJob.Params["@ToPosition"])));
                                    }
                                    break;
                                case 4:
                                    if (TaskJob.Params["@ToPosition"].Contains("ELPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(TaskJob.Params["@ToPosition"], "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveOut, "")));
                                    }
                                    break;
                                case 5:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("SHELF", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.Shelf.GetFOUPPresence, "")));
                                    break;
                                default:
                                    STK_LastCmd = 0;
                                    STK_LastCmdParam = null;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.STK_PRESENT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("SHELF", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.Shelf.GetFOUPPresence, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.FOUP_ID:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.ReadCID, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.OPEN_FOUP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.Load, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CLOSE_FOUP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.Unload, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_WTS:
                            WTS_LastCmd = TaskFlowManagement.Command.TRANSFER_WTS;
                            WTS_LastCmdParam = TaskJob.Params;
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    if (TaskJob.Params["@FromPosition"].Contains("ILPT"))
                                    {
                                        //WHR get ILPT
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Pick, TaskJob.Params["@Mode"], TaskJob.Params["@FromPosition"])));
                                        //CTU getwait
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Pick, TaskJob.Params["@Mode"], "WHR", "", "", "", "", "", "0")));

                                    }
                                    else if (TaskJob.Params["@FromPosition"].Contains("CTU"))
                                    {
                                        //CTU Putwait
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Place, TaskJob.Params["@Mode"], "WHR", "", "", "", "", "", "0")));
                                        //WHR Getwait
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.PreparePick, TaskJob.Params["@Mode"], "CTU")));

                                    }
                                    break;
                                case 1:
                                    if (TaskJob.Params["@FromPosition"].Contains("ILPT"))
                                    {
                                        //WHR putwait for CTU
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.PreparePlace, TaskJob.Params["@Mode"], "CTU")));

                                    }
                                    else if (TaskJob.Params["@FromPosition"].Contains("CTU"))
                                    {
                                        //WHR Extend Get
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Extend, TaskJob.Params["@Mode"], "", "3", "", "", "", "", "0")));
                                    }
                                    break;
                                case 2:
                                    if (TaskJob.Params["@FromPosition"].Contains("ILPT"))
                                    {
                                        //WHR extend Put
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Extend, TaskJob.Params["@Mode"], "", "3", "", "", "", "", "1")));

                                    }
                                    else if (TaskJob.Params["@FromPosition"].Contains("CTU"))
                                    {
                                        //WHR Up
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Up, "")));

                                    }
                                    break;
                                case 3:
                                    if (TaskJob.Params["@FromPosition"].Contains("ILPT"))
                                    {
                                        //CTU hold
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Hold, TaskJob.Params["@Mode"])));

                                    }
                                    else if (TaskJob.Params["@FromPosition"].Contains("CTU"))
                                    {
                                        //CTU Release
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Release, TaskJob.Params["@Mode"])));
                                    }
                                    break;
                                case 4:
                                    if (TaskJob.Params["@FromPosition"].Contains("ILPT"))
                                    {
                                        //WHR Down
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Down, "")));

                                    }
                                    else if (TaskJob.Params["@FromPosition"].Contains("CTU"))
                                    {
                                        //WHR Retract
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Retract, "")));
                                    }
                                    break;
                                case 5:
                                    if (TaskJob.Params["@FromPosition"].Contains("ILPT"))
                                    {
                                        //WHR Retract
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Retract, "")));

                                    }
                                    else if (TaskJob.Params["@FromPosition"].Contains("CTU"))
                                    {
                                        //CTU HOME
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Home, "")));
                                        //WHR Put ILPT
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Place, TaskJob.Params["@Mode"], TaskJob.Params["@ToPosition"])));
                                    }
                                    break;
                                case 6:
                                    if (TaskJob.Params["@FromPosition"].Contains("ILPT"))
                                    {
                                        //CTU HOME
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Home, "")));
                                        //WHR Home
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.SHome, "")));
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(TaskJob.Params["@FromPosition"], "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.RaiseClose, "")));
                                    }
                                    else if (TaskJob.Params["@FromPosition"].Contains("CTU"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(TaskJob.Params["@ToPosition"], "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.RaiseClose, "")));
                                    }

                                    break;
                                case 7:
                                    if (TaskJob.Params["@FromPosition"].Contains("ILPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(TaskJob.Params["@FromPosition"], "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.Load, "1")));
                                    }
                                    else if (TaskJob.Params["@FromPosition"].Contains("CTU"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(TaskJob.Params["@ToPosition"], "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.Load, "1")));
                                    }
                                        break;
                                default:
                                    WTS_LastCmd = 0;
                                    WTS_LastCmdParam = null;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_PTZ:
                            WTS_LastCmd = TaskFlowManagement.Command.TRANSFER_PTZ;
                            WTS_LastCmdParam = TaskJob.Params;
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("PTZ", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.PTZ.Transfer, TaskJob.Params["@Mode"], "", TaskJob.Params["@Station"], "", "", "", "", TaskJob.Params["@Direction"])));

                                    break;
                                case 1:

                                    //IN: put to ptz  OUT: get from ptz
                                    if (TaskJob.Params["@Way"].Equals("IN"))
                                    {
                                        //CTU Put to PTZ
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Place, TaskJob.Params["@Mode"], "PTZ", "", "", "", "", "", "1")));
                                    }
                                    else if (TaskJob.Params["@Way"].Equals("OUT"))
                                    {
                                        //CTU Get from PTZ
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Pick, TaskJob.Params["@Mode"], "PTZ", "", "", "", "", "", "1")));
                                    }
                                    break;
                                case 2:
                                    //PTZ Home
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("PTZ", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.PTZ.Home, "")));
                                    break;
                                default:
                                    WTS_LastCmd = 0;
                                    WTS_LastCmdParam = null;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.BLOCK_PTZ:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("PTZ", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.PTZ.SetPath, TaskJob.Params["@Path"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.RELEASE_PTZ:
                            switch (TaskJob.CurrentIndex)
                            {
                                default:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.BLOCK_ALIGNER:
                            switch (TaskJob.CurrentIndex)
                            {
                                default:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.RELEASE_ALIGNER:
                            switch (TaskJob.CurrentIndex)
                            {
                                default:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;

                        case TaskFlowManagement.Command.WHR_RETRACT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Retract, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_RESET:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Reset, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_SHOME:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.SHome, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_EXTEND:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Extend, TaskJob.Params["@Mode"], "", TaskJob.Params["@Station"], "", "", "", "", TaskJob.Params["@Value"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_PREPAREPICK:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.PreparePick, TaskJob.Params["@Mode"], TaskJob.Params["@Position"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_PICK:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Pick, TaskJob.Params["@Mode"], TaskJob.Params["@Position"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_PREPAREPLACE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.PreparePlace, TaskJob.Params["@Mode"], TaskJob.Params["@Position"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_PLACE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Place, TaskJob.Params["@Mode"], TaskJob.Params["@Position"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_DOWN:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Down, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_UP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Up, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_RESET:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Reset, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_HOME:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Home, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_INIT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.OrgSearch, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_PREPAREPICK:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Pick, TaskJob.Params["@Mode"], TaskJob.Params["@Position"], "", "", "", "", "", "0")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_PREPAREPLACE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Place, TaskJob.Params["@Mode"], TaskJob.Params["@Position"], "", "", "", "", "", "0")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_PLACE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Place, TaskJob.Params["@Mode"], TaskJob.Params["@Position"], "", "", "", "", "", "1")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_PICK:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Pick, TaskJob.Params["@Mode"], TaskJob.Params["@Position"], "", "", "", "", "", "1")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_HOLD:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Hold, TaskJob.Params["@Mode"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_RELEASE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Release, TaskJob.Params["@Mode"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.PTZ_TRANSFER:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("PTZ", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.PTZ.Transfer, TaskJob.Params["@Mode"], "", TaskJob.Params["@Station"], "", "", "", "", TaskJob.Params["@Direction"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.PTZ_HOME:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("PTZ", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.PTZ.Home, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WTSALIGNER_ALIGN:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    int deg = Convert.ToInt32(TaskJob.Params["@Value"]);
                                    if (deg > 180)
                                    {
                                        deg = deg - 360;
                                    }
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WTS_ALIGNER", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WTSAligner.Align, deg.ToString() + "000")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ELPT_READ_RFID:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.ReadCID, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ELPT_CLAMP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.Clamp, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ELPT_UNCLAMP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.UnClamp, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ELPT_OPEN_SHUTTER:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.OpenShutter, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ELPT_CLOSE_SHUTTER:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.CloseShutter, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ELPT_MOVE_IN:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveIn, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ELPT_MOVE_OUT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveOut, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ELPT_INIT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ELPT1", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.OrgSearch, "")));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ELPT2", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.OrgSearch, "")));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ELPT1", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveOut, "")));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ELPT2", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveOut, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ELPT_RESET:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.Reset, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ILPT_LOAD:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.Load, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ILPT_UNLOAD:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.Unload, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ILPT_INIT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.OrgSearch, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ILPT_RESET:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.Reset, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.FOUPROBOT_PREPARE_PICK:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.PreparePick, "", TaskJob.Params["@Position"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.FOUPROBOT_PICK:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Pick, "", TaskJob.Params["@Position"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.FOUPROBOT_PREPARE_PLACE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.PreparePlace, "", TaskJob.Params["@Position"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.FOUPROBOT_PLACE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Place, "", TaskJob.Params["@Position"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.FOUPROBOT_INIT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.SHome, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.FOUPROBOT_RESET:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Reset, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.FOUPROBOT_SET_SPEED:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.SetSpeed, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_SET_SPEED:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.SetSpeed, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.PTZ_SET_SPEED:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("PTZ", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.PTZ.SetSpeed, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ELPT_SET_SPEED:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.SetSpeed, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_SET_SPEED:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.SetSpeed, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WTSALIGNER_SET_SPEED:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WTS_ALIGNER", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WTSAligner.SetSpeed, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.PORT_ACCESS_MODE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    Target.Enable = TaskJob.Params["@Value"].Equals("OFFLINE") ? false : true;
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.STOP_STOCKER:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Pause, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.RESUME_STOCKER:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Continue, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ABORT_STOCKER:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Stop, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.RESET_STOCKER:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);

                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.LightCurtainReset, TaskJob.Params)));

                                    break;
                                case 1:

                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Reset, "")));
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ELPT1", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.UnClamp, "")));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ELPT2", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.UnClamp, "")));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Initial_IO, "")));

                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.SHome, "")));
                                    break;
                                case 4:

                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ELPT1", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.OrgSearch, "")));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ELPT2", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.OrgSearch, "")));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ILPT1", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.OrgSearch, TaskJob.Params)));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ILPT2", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.OrgSearch, TaskJob.Params)));
                                    break;
                                case 5:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ELPT1", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveOut, "")));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ELPT2", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveOut, "")));

                                    break;
                                case 6:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("SHELF", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.Shelf.GetFOUPPresence, "")));

                                    break;
                                case 7:
                                    if (STK_LastCmd== TaskFlowManagement.Command.MOVE_FOUP)
                                    {
                                        string destination = STK_LastCmdParam["@ToPosition"].Replace("_", "");
                                        if (NodeManagement.Get(destination) != null)
                                        {
                                            if (destination.Contains("ELPT"))
                                            {
                                                if (NodeManagement.Get("SHELF").Status[destination].Equals("1"))
                                                {
                                                    //目的地有FOUP
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(destination, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveIn, "")));
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.PreparePick, "", destination)));
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case 8:
                                    if (STK_LastCmd == TaskFlowManagement.Command.MOVE_FOUP)
                                    {

                                        string destination = STK_LastCmdParam["@ToPosition"].Replace("_", "");
                                        if (NodeManagement.Get(destination) != null)
                                        {
                                            if (NodeManagement.Get("SHELF").Status[destination].Equals("1"))
                                            {
                                                //目的地有FOUP
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Pick, "", destination)));
                                            }
                                        }
                                    }
                                    break;
                                case 9:
                                    if (STK_LastCmd == TaskFlowManagement.Command.MOVE_FOUP)
                                    {
                                        string Source = STK_LastCmdParam["@FromPosition"].Replace("_", "");
                                        string destination = STK_LastCmdParam["@ToPosition"].Replace("_", "");
                                        if (NodeManagement.Get(Source) != null && NodeManagement.Get(destination) != null)
                                        {
                                            if (NodeManagement.Get("SHELF").Status[destination].Equals("1"))
                                            {
                                                if (Source.Contains("ELPT"))
                                                {
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Source, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveIn, "")));
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.PreparePlace, "", Source)));

                                                }
                                            }
                                        }
                                    }
                                    break;
                                case 10:
                                    if (STK_LastCmd == TaskFlowManagement.Command.MOVE_FOUP)
                                    {
                                        string Source = STK_LastCmdParam["@FromPosition"].Replace("_", "");
                                        string destination = STK_LastCmdParam["@ToPosition"].Replace("_", "");
                                        if (NodeManagement.Get(Source) != null && NodeManagement.Get(destination) != null)
                                        {
                                            if (NodeManagement.Get("SHELF").Status[destination].Equals("1"))
                                            {
                                                //目的地有FOUP
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Place, "", Source)));
                                                if (destination.Contains("ELPT"))
                                                {
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(destination, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveOut, "")));
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case 11:
                                    if (STK_LastCmd == TaskFlowManagement.Command.MOVE_FOUP)
                                    {
                                        string Source = STK_LastCmdParam["@FromPosition"].Replace("_", "");
                                        string destination = STK_LastCmdParam["@ToPosition"].Replace("_", "");
                                        if (NodeManagement.Get(Source) != null && NodeManagement.Get(destination) != null)
                                        {
                                            if (NodeManagement.Get("SHELF").Status[destination].Equals("1"))
                                            {
                                                //目的地有FOUP
                                                if (Source.Contains("ELPT"))
                                                {
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Source, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveOut, "")));
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case 12:
                                    if (STK_LastCmd == TaskFlowManagement.Command.MOVE_FOUP)
                                    {
                                        string Source = STK_LastCmdParam["@FromPosition"].Replace("_", "");
                                        string destination = STK_LastCmdParam["@ToPosition"].Replace("_", "");
                                        if (NodeManagement.Get(Source) != null && NodeManagement.Get(destination) != null)
                                        {
                                            if (NodeManagement.Get("SHELF").Status["FOUP_ROBOT"].Equals("1"))
                                            {

                                                if (Source.Contains("ELPT"))
                                                {
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Source, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveIn, ""))); ;
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.PreparePlace, "", Source)));

                                                }
                                            }
                                        }
                                    }
                                    break;
                                case 13:
                                    if (STK_LastCmd == TaskFlowManagement.Command.MOVE_FOUP)
                                    {
                                        string Source = STK_LastCmdParam["@FromPosition"].Replace("_", "");
                                        string destination = STK_LastCmdParam["@ToPosition"].Replace("_", "");
                                        if (NodeManagement.Get(Source) != null && NodeManagement.Get(destination) != null)
                                        {
                                            if (NodeManagement.Get("SHELF").Status["FOUP_ROBOT"].Equals("1"))
                                            {
                                                //目的地有FOUP
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Place, "", Source)));

                                            }
                                        }
                                    }
                                    break;
                                case 14:
                                    if (STK_LastCmd == TaskFlowManagement.Command.MOVE_FOUP)
                                    {
                                        string Source = STK_LastCmdParam["@FromPosition"].Replace("_", "");
                                        string destination = STK_LastCmdParam["@ToPosition"].Replace("_", "");
                                        if (NodeManagement.Get(Source) != null && NodeManagement.Get(destination) != null)
                                        {
                                            if (NodeManagement.Get("SHELF").Status["FOUP_ROBOT"].Equals("1"))
                                            {

                                                if (Source.Contains("ELPT"))
                                                {
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Source, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveOut, ""))); ;
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case 15:
                                    break;
                                default:
                                    STK_LastCmd = 0;
                                    STK_LastCmdParam = null;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        
                        case TaskFlowManagement.Command.STOP_WTS:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Pause, TaskJob.Params)));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Pause, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.RESUME_WTS:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Continue, TaskJob.Params)));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Continue, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ABORT_WTS:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Stop, TaskJob.Params)));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Stop, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.RESET_WTS:

                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Reset, TaskJob.Params)));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Reset, TaskJob.Params)));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.SHome, TaskJob.Params)));
                                    
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.OrgSearch, TaskJob.Params)));
                                   
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Home, TaskJob.Params)));
                                    
                                    break;
                                case 4:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("PTZ", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.PTZ.Home, TaskJob.Params)));
                                    break;
                                case 5:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("PTZ", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.PTZ.Transfer, "1", "", "0", "", "", "", "", "3")));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("SHELF", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.Shelf.GetFOUPPresence, "")));
                                    break;
                                case 6:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("PTZ", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.PTZ.Home, TaskJob.Params)));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ILPT1", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.OrgSearch, TaskJob.Params)));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ILPT2", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.OrgSearch, TaskJob.Params)));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WTS_ALIGNER", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WTSAligner.Align, "0000")));
                                    break;
                                case 7:
                                    switch (WTS_LastCmd)
                                    {
                                        case TaskFlowManagement.Command.TRANSFER_WTS:
                                           
                                            break;
                                        case TaskFlowManagement.Command.TRANSFER_PTZ:
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("PTZ", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.PTZ.Transfer, WTS_LastCmdParam["@Mode"], "", WTS_LastCmdParam["@Station"], "", "", "", "", WTS_LastCmdParam["@Direction"])));


                                            break;
                                    }
                                    break;
                                case 8:
                                    switch (WTS_LastCmd)
                                    {
                                        case TaskFlowManagement.Command.TRANSFER_WTS:
                                            if (WTS_LastCmdParam["@FromPosition"].Contains("ILPT"))
                                            {
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(WTS_LastCmdParam["@FromPosition"], "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.Load, "1")));
                                            }
                                            if (WTS_LastCmdParam["@ToPosition"].Contains("ILPT"))
                                            {
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(WTS_LastCmdParam["@ToPosition"], "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.Load, "1")));
                                            }
                                            break;
                                        case TaskFlowManagement.Command.TRANSFER_PTZ:
                                            var odd = from job in NodeManagement.Get("PTZ").JobList.Values.ToList()
                                                      where job.MapFlag && Convert.ToInt32(job.Slot) % 2 != 0
                                                      select job;
                                            var even = from job in NodeManagement.Get("PTZ").JobList.Values.ToList()
                                                       where job.MapFlag && Convert.ToInt32(job.Slot) % 2 == 0
                                                       select job;

                                            //IN: put to ptz  OUT: get from ptz
                                            if (WTS_LastCmdParam["@Way"].Equals("OUT"))
                                            {
                                                
                                            }
                                            else if (WTS_LastCmdParam["@Way"].Equals("IN"))
                                            {
                                                if (WTS_LastCmdParam["@Station"].Equals("0") && odd.Count() != 0)
                                                {//ODD
                                                 //CTU Get from PTZ
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Pick, WTS_LastCmdParam["@Mode"], "PTZ", "", "", "", "", "", "1")));

                                                }
                                                else if (WTS_LastCmdParam["@Station"].Equals("1") && even.Count() != 0)
                                                {//EVEN
                                                    //CTU Get from PTZ
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Pick, WTS_LastCmdParam["@Mode"], "PTZ", "", "", "", "", "", "1")));

                                                }
                                            }
                                            break;
                                    }
                                    break;
                                case 9:
                                    switch (WTS_LastCmd)
                                    {
                                        case TaskFlowManagement.Command.TRANSFER_WTS:
                                            if (WTS_LastCmdParam["@FromPosition"].Contains("ILPT"))
                                            {
                                                if (NodeManagement.Get(WTS_LastCmdParam["@FromPosition"]).MappingResult.Equals("0000000000000000000000000"))
                                                {
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Place, WTS_LastCmdParam["@Mode"], WTS_LastCmdParam["@FromPosition"])));
                                                }
                                            }
                                            if (WTS_LastCmdParam["@FromPosition"].Contains("CTU"))
                                            {
                                                if (NodeManagement.Get(WTS_LastCmdParam["@ToPosition"]).MappingResult.Equals("0000000000000000000000000"))
                                                {
                                                    //WHR Putwait CTU
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.PreparePlace, WTS_LastCmdParam["@Mode"], WTS_LastCmdParam["@FromPosition"])));
                                                    //CTU Getwait for WHR
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Pick, WTS_LastCmdParam["@Mode"], "WHR", "", "", "", "", "", "0")));
                                                }
                                                else
                                                {
                                                    //WHR Get ILPT
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Pick, WTS_LastCmdParam["@Mode"], WTS_LastCmdParam["@ToPosition"])));
                                                    //CTU Getwait for WHR
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Pick, WTS_LastCmdParam["@Mode"], "WHR", "", "", "", "", "", "0")));
                                                }
                                            }
                                            break;
                                        case TaskFlowManagement.Command.TRANSFER_PTZ:
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("PTZ", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.PTZ.Home, "")));
                                            break;
                                    }
                                    break;
                                case 10:
                                    switch (WTS_LastCmd)
                                    {
                                        case TaskFlowManagement.Command.TRANSFER_WTS:
                                            if (WTS_LastCmdParam["@FromPosition"].Contains("ILPT"))
                                            {
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(WTS_LastCmdParam["@FromPosition"], "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.Unload, "")));
                                            }
                                            if (WTS_LastCmdParam["@FromPosition"].Contains("CTU"))
                                            {

                                                //WHR extend to CTU 
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Extend, WTS_LastCmdParam["@Mode"], "", "3", "", "", "", "", "1")));
                                            }
                                            break;
                                        case TaskFlowManagement.Command.TRANSFER_PTZ:

                                            break;
                                    }
                                    break;
                                case 11:
                                    switch (WTS_LastCmd)
                                    {
                                        case TaskFlowManagement.Command.TRANSFER_WTS:

                                            if (WTS_LastCmdParam["@FromPosition"].Contains("CTU"))
                                            {

                                                //CTU hold
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Hold, WTS_LastCmdParam["@Mode"])));
                                            }
                                            break;
                                        case TaskFlowManagement.Command.TRANSFER_PTZ:

                                            break;
                                    }
                                    break;
                                case 12:
                                    switch (WTS_LastCmd)
                                    {
                                        case TaskFlowManagement.Command.TRANSFER_WTS:

                                            if (WTS_LastCmdParam["@FromPosition"].Contains("CTU"))
                                            {

                                                //WHR Down
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Down, "")));
                                            }
                                            break;
                                        case TaskFlowManagement.Command.TRANSFER_PTZ:

                                            break;
                                    }
                                    break;
                                case 13:
                                    switch (WTS_LastCmd)
                                    {
                                        case TaskFlowManagement.Command.TRANSFER_WTS:

                                            if (WTS_LastCmdParam["@FromPosition"].Contains("CTU"))
                                            {

                                                //WHR Retract
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Retract, "")));
                                            }
                                            break;
                                        case TaskFlowManagement.Command.TRANSFER_PTZ:

                                            break;
                                    }
                                    break;
                                case 14:
                                    switch (WTS_LastCmd)
                                    {
                                        case TaskFlowManagement.Command.TRANSFER_WTS:

                                            if (WTS_LastCmdParam["@FromPosition"].Contains("CTU"))
                                            {

                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.SHome, TaskJob.Params)));
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Home, TaskJob.Params)));
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(WTS_LastCmdParam["@ToPosition"], "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.Unload, "")));
                                            }
                                            break;
                                        case TaskFlowManagement.Command.TRANSFER_PTZ:

                                            break;
                                    }
                                    break;
                                case 15:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Initial_IO, TaskJob.Params)));
                                    break;
                                default:
                                    WTS_LastCmd = 0;
                                    WTS_LastCmdParam = null;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;


                        case TaskFlowManagement.Command.RESET_E84:


                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.E84.Reset, TaskJob.Params)));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.E84.SetTP1, TaskJob.Params["@TP1"])));
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.E84.SetTP2, TaskJob.Params["@TP2"])));
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.E84.SetTP3, TaskJob.Params["@TP3"])));
                                    break;
                                case 4:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.E84.SetTP4, TaskJob.Params["@TP4"])));
                                    break;
                                case 5:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.E84.SetTP5, TaskJob.Params["@TP5"])));
                                    break;
                                case 6:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.E84.SetTP6, TaskJob.Params["@TP6"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.E84_MODE:


                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                   // TaskReport.On_Task_Ack(TaskJob);
                                    if (TaskJob.Params["@Value"].Equals("AUTO"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.E84.AutoMode, TaskJob.Params)));
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.E84.ManualMode, TaskJob.Params)));
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LIGHT_CURTAIN_ENABLED:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    //TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.LightCurtainReset, TaskJob.Params)));                
                                      break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.LightCurtainEnabled, TaskJob.Params)));

                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LIGHT_CURTAIN_RESET:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);

                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.LightCurtainReset, TaskJob.Params)));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.GET_IO:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);

                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Get_DIO, "1")));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Get_DIO, "1")));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Get_DIO, "1")));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Get_DIO, "0")));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Get_DIO, "0")));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Get_DIO, "0")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.SET_IO:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);

                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("FOUP_ROBOT", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Set_DIO, TaskJob.Params)));
                                    break;
                                
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CONTROL_MODE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    if (TaskJob.Params["@Value"].Equals("1"))
                                    {
                                        TransferControl.Instance.SetOnline(true);
                                    }
                                    else
                                    {
                                        TransferControl.Instance.SetOnline(false);
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
                                if(!NodeManagement.Get(eachCmd.NodeName).SendCommand(eachCmd.Txn, out Message))
                                {
                                    logger.Error("Excute fail NodeName:" + eachCmd.NodeName);
                                    AbortTask(TaskReport, TaskJob, eachCmd.NodeName, "ABS", "ConnectionError");
                                    return false;
                                }
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
                AbortTask(TaskReport, TaskJob, "SYSTEM", "ABS", e.Message);
                return false;
            }
            return true;
        }
        private bool CheckEMO(TaskFlowManagement.CurrentProcessTask TaskJob, ITaskFlowReport TaskReport)
        {
            bool result = true;
            if (!SystemConfig.Get().SaftyCheckByPass)
            {
               // if (TransferControl.Instance.DIO.GetIO("DIN", "SAFETYRELAY").ToUpper().Equals("TRUE"))
                //{
                    //TaskFlowManagement.TaskRemove(TaskJob.Id);
                    //AbortTask(TaskReport, TaskJob, "SYSTEM", "CAN", "S0300170");
                    //result = false;
                //}
            }
            return result;
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
                    J = TransferControl.CreateJob();//當沒有帳時強制建帳
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
                    tmp = TransferControl.CreateJob();
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
