using log4net;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ControlService.Comm;
using ControlService.Config;

namespace ControlService.Management
{
    public class PointManagement
    {
        public static Dictionary<string, Dictionary<string, RobotPoint>> PointList;
        static ILog logger = LogManager.GetLogger(typeof(PointManagement));

        public static void LoadConfig()
        {
            PointList = new Dictionary<string, Dictionary<string, RobotPoint>>();
            //Dictionary<string, object> keyValues = new Dictionary<string, object>();
            //string Sql = @"SELECT t.node_name AS NodeName,t.position AS POSITION,t.position_type AS PositionType,t.point as Point, t.mapping_point as MappingPoint, t.pre_mapping_point as PreMappingPoint, t.`offset` as Offset 
            //                FROM config_point t 
            //                WHERE t.equipment_model_id = @equipment_model_id";
            //keyValues.Add("@equipment_model_id", SystemConfig.Get().SystemMode);
            //DataTable dt = dBUtil.GetDataTable(Sql, keyValues);
            //string str_json = JsonConvert.SerializeObject(dt, Formatting.Indented);
            //List<RobotPoint> Points = JsonConvert.DeserializeObject<List<RobotPoint>>(str_json);
            List<RobotPoint> RobotPointList = new ConfigTool<List<RobotPoint>>().ReadFile("config/Point.json");

            foreach (RobotPoint each in RobotPointList)
            {
                Dictionary<string, RobotPoint> tmp;
                if (PointList.TryGetValue(each.NodeName,out tmp))
                {
                    if (tmp.ContainsKey(each.Position))
                    {
                        tmp.Remove(each.Position);
                    }
                    tmp.Add(each.Position,each);
                }
                else
                {
                    tmp = new Dictionary<string, RobotPoint>();
                    tmp.Add(each.Position, each);
                    PointList.Add(each.NodeName,tmp);
                }
            }
        }      

        public static RobotPoint GetPoint(string NodeName, string Position)
        {
            RobotPoint result = null;
            Dictionary<string, RobotPoint> tmp;
            Node targetPosition = NodeManagement.Get(Position);
            if (!targetPosition.CarrierType.Equals(""))
            {
                Position += "_" + targetPosition.CarrierType;
            }
            if (!targetPosition.WaferSize.Equals(""))
            {
                Position += "_" + targetPosition.WaferSize;
            }

            if (PointList.TryGetValue(NodeName, out tmp))
            {
                tmp.TryGetValue(Position, out result);

            }
            return result;
        }

        //public static RobotPoint GetMapPoint(string RobotName,string Position, string RecipeID)
        //{
        //    RobotPoint result = null;
        //    List<RobotPoint> tmp;
        //    if (PointList.TryGetValue(RecipeID, out tmp))
        //    {
        //        var findPoint = from point in tmp
        //                        where point.Position.ToUpper().Equals(Position.ToUpper()) && point.NodeName.ToUpper().Equals(RobotName)// && point.PositionType.Equals("MAPPER")
        //                        select point;
        //        if (findPoint.Count() != 0)
        //        {
        //            result = findPoint.First();
        //        }

        //    }
        //    return result;
        //}
    }
}
