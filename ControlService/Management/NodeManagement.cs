
using log4net;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ControlService.Comm;
using ControlService.Config;

namespace ControlService.Management
{
    public static class NodeManagement
    {
        private static ConcurrentDictionary<string, Node> NodeList;
        private static ConcurrentDictionary<string, Node> NodeListByCtrl;
        static ILog logger = LogManager.GetLogger(typeof(NodeManagement));



        public static void LoadConfig()
        {
            NodeList = new ConcurrentDictionary<string, Node>();
            NodeListByCtrl = new ConcurrentDictionary<string, Node>();
            //dictionary<string, object> keyvalues = new dictionary<string, object>();
            //string sql = @"select 
            //             upper(t.node_id) as name, upper(t.controller_id) as controller,
            //             t.conn_address as adrno, upper(t.node_type) as type, 
            //             upper(t.vendor) as brand,t.bypass,t.enable_flg as enable,                        
            //             t.wafer_size as wafersize,
            //                t.double_arm as doublearmactive,
            //                t.notch_angle as notchangle,
            //                t.r_flip_degree as r_flip_degree,
            //                t.associated_node as associated_node,
            //                t.r_arm as rarmactive,
            //                t.l_arm as larmactive,
            //                t.carrier_type as carriertype,
            //                t.mode as mode,
            //                t.ack_timeout as acktimeout,
            //                t.motion_timeout as motiontimeout,
            //                t.pooltask as pooltask
            //            from config_node t
            //            where t.equipment_model_id = @equipment_model_id";
            //keyValues.Add("@equipment_model_id", SystemConfig.Get().SystemMode);
            //DataTable dt = dBUtil.GetDataTable(Sql, keyValues);
            //string str_json = JsonConvert.SerializeObject(dt, Formatting.Indented);
            ////str_json = str_json.Replace("\"[", "[").Replace("]\"", "]").Replace("\\\"", "\"");
            //List<Node> nodeList = JsonConvert.DeserializeObject<List<Node>>(str_json);
            List<Node> nodeList = new ConfigTool<List<Node>>().ReadFile("config/Node.json");

            foreach (Node each in nodeList)
            {
                //if (each.Enable)
                //{
                    each.InitialObject();
                    NodeList.TryAdd(each.Name, each);
                    NodeListByCtrl.TryAdd(each.Controller + each.AdrNo, each);
                //}
            }

        }



        public static List<Node> GetLoadPortList()
        {
            List<Node> result = new List<Node>();

            var findPort = from port in NodeList.Values.ToList()
                           where port.Type.Equals("LOADPORT")
                           select port;

            if (findPort.Count() != 0)
            {
                result = findPort.ToList();
                result.Sort((x, y) => { return x.Name.CompareTo(y.Name); });
            }

            return result;
        }

        public static List<Node> GetAlignerList()
        {
            List<Node> result = new List<Node>();

            var findA = from A in NodeList.Values.ToList()
                           where A.Type.Equals("ALIGNER")
                           select A;

            if (findA.Count() != 0)
            {
                result = findA.ToList();
                result.Sort((x, y) => { return x.Name.CompareTo(y.Name); });
            }

            return result;
        }

        public static List<Node> GetLoadPortList(string Mode)
        {
            List<Node> result = new List<Node>();

            var findPort = from port in NodeList.Values.ToList()
                           where port.Type.Equals("LOADPORT") && port.Mode.Equals(Mode)
                           select port;

            if (findPort.Count() != 0)
            {
                result = findPort.ToList();
                result.Sort((x, y) => { return x.Name.CompareTo(y.Name); });
            }

            return result;
        }

        public static Node GetLoadPortByFoup(string FoupId)
        {
            Node result = null;
            var findPort = from port in NodeList.Values.ToList()
                           where port.Type.Equals("LOADPORT") && port.Enable && port.Carrier.CarrierID.ToUpper().Equals(FoupId.ToUpper())
                           select port;
            if (findPort.Count() != 0)
            {
                result = findPort.First();
            }
            return result;
        }

        public static Node GetLoadPortByPTN(int PTN)
        {
            Node result = null;
            var findPort = from port in NodeList.Values.ToList()
                           where port.Type.Equals("LOADPORT") && port.Enable && port.PTN.Equals(PTN)
                           select port;
            if (findPort.Count() != 0)
            {
                result = findPort.First();
            }
            return result;
        }

        public static List<Node> GetEnableRobotList()
        {
            List<Node> result = new List<Node>();

            var findRobot = from robot in NodeList.Values.ToList()
                            where robot.Type.Equals("ROBOT") && robot.Enable == true
                            select robot;

            if (findRobot.Count() != 0)
            {
                result = findRobot.ToList();
            }

            return result;
        }

        public static List<Node> GetList()
        {
            List<Node> result = NodeList.Values.ToList();

            result.Sort((x, y) => { return x.Name.CompareTo(y.Name); });

            return result;
        }
        public static void Save()
        {
            List<Node> result = NodeList.Values.ToList();

            result.Sort((x, y) => { return x.Name.CompareTo(y.Name); });

            new ConfigTool<List<Node>>().WriteFile("config/Node.json", result);
        }
        

        public static Node Get(string Name)
        {
            Node result = null;
            if (Name != null)
            {
                NodeList.TryGetValue(Name.ToUpper(), out result);
            }
            if (result == null)
            {
                logger.Error("Node not exist, Name:" + Name);
            }
            return result;
        }  
        public static Node GetByController(string DeviceName, string NodeAdr)
        {
            Node result = null;

            NodeListByCtrl.TryGetValue(DeviceName + NodeAdr, out result);

            return result;
        }
        public static Node GetFirstByController(string DeviceName)
        {
            Node result = null;

            
            var find = from node in NodeList.Values.ToList()
                           where node.Controller.Equals(DeviceName)
                           select node;
            if (find.Count() != 0)
            {
                result = find.First();
            }

            return result;
        }

        public static Node GetOCRByController(string DeviceName)
        {
            Node result = null;

            var node = from LD in NodeManagement.GetList()
                       where LD.Controller.Equals(DeviceName)
                       select LD;
            if (node.Count() != 0)
            {
                result = node.First();
                if (!result.Type.Equals("OCR"))
                {
                    result = null;
                }
            }
            return result;
        }

        //public static Node GetNextRobot(Node ProcessNode, Job Job)
        //{
        //    Node result = null;


        //    var findPoint = from point in PointManagement.GetPointList(ProcessNode.Name)
        //                    where point.Position.ToUpper().Equals(Job.Destination.ToUpper())
        //                    select point;
        //    if (findPoint.Count() != 0)
        //    {
        //        result = NodeManagement.Get(findPoint.First().NodeName);
        //    }
        //    return result;
        //}


        //public static Node GetOCRByAligner(Node Aligner)
        //{
        //    Node result = null;

        //    foreach (Node.Route eachRt in Aligner.RouteTable)
        //    {
        //        if (eachRt.NodeType.Equals("OCR"))
        //        {
        //            if (NodeList.TryGetValue(eachRt.NodeName, out result))
        //            {
        //                break;
        //            }
        //        }
        //    }
        //    return result;
        //}

        //public static Node GetAlignerByOCR(Node OCR)
        //{
        //    Node result = null;

        //    foreach (Node.Route eachRt in OCR.RouteTable)
        //    {
        //        if (eachRt.NodeType.Equals("ALIGNER"))
        //        {
        //            if (NodeList.TryGetValue(eachRt.NodeName, out result))
        //            {
        //                break;
        //            }
        //        }
        //    }
        //    return result;
        //}

        public static bool Add(string Name, Node Node)
        {
            bool result = false;


            if (!NodeList.ContainsKey(Name))
            {
                NodeList.TryAdd(Name, Node);
                NodeListByCtrl.TryAdd(Node.Controller + Node.AdrNo, Node);
                result = true;
            }



            return result;
        }

    }
}
