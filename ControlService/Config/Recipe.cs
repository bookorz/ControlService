using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlService.Comm;
using ControlService.Management;

namespace ControlService.Config
{
    public class Recipe
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Recipe));
        //id
        public string recipe_id { get; set; }
        public string recipe_name { get; set; }


        //port_type: Load, Unload, Both
        public string port1_type { get; set; }
        public string port2_type { get; set; }
        public string port3_type { get; set; }
        public string port4_type { get; set; }
        public string port5_type { get; set; }
        public string port6_type { get; set; }
        public string port7_type { get; set; }
        public string port8_type { get; set; }
        //port_priority: 1~8
        public int port1_priority { get; set; }
        public int port2_priority { get; set; }
        public int port3_priority { get; set; }
        public int port4_priority { get; set; }
        public int port5_priority { get; set; }
        public int port6_priority { get; set; }
        public int port7_priority { get; set; }
        public int port8_priority { get; set; }

        //port_carrier_type: FOUP, FOSB, 200_Adapter, 300_Adapter
        public string port1_carrier_type { get; set; }
        public string port2_carrier_type { get; set; }
        public string port3_carrier_type { get; set; }
        public string port4_carrier_type { get; set; }
        public string port5_carrier_type { get; set; }
        public string port6_carrier_type { get; set; }
        public string port7_carrier_type { get; set; }
        public string port8__carriertype { get; set; }

        //robot_speed: 1~100
        public string robot1_speed { get; set; }
        public string robot2_speed { get; set; }

        //aligner_speed: 1~100
        public string aligner1_speed { get; set; }
        public string aligner2_speed { get; set; }

        //aligner_speed: 1~100
        public string aligner1_angle { get; set; }
        public string aligner2_angle { get; set; }
        public string notch_angle { get; set; }

        public string ocr_ttl_config { get; set; }
        public Boolean is_use_ocr_ttl { get; set; }
        public string ocr_t7_config { get; set; }
        public Boolean is_use_ocr_t7 { get; set; }
        public string ocr_m12_config { get; set; }
        public Boolean is_use_ocr_m12 { get; set; }
        public Boolean is_use_aligner1 { get; set; }
        public Boolean is_use_aligner2 { get; set; }
        public Boolean is_use_exchange { get; set; }
        public Boolean is_use_burnin { get; set; }
        public string ocr_type { get; set; }
        public string ocr_check_Rule { get; set; }
        public string get_slot_order { get; set; }
        public string put_slot_order { get; set; }
        public string wafer_size { get; set; }
        ////ocr_config
        //public string ocr1_config { get; set; }
        //public string ocr2_config { get; set; }

        //uac_check_CIDRW
        public string uac_check_CIDRW { get; set; }

        //input_proc_fin:
        public string input_proc_fin { get; set; }

        //output_proc_fin
        public string output_proc_fin { get; set; }

        //auto_proc_fin
        public string auto_proc_fin { get; set; }

        //manual_proc_fin
        public string manual_proc_fin { get; set; }

        //auto_get_constrict
        public string auto_get_constrict { get; set; }

        //auto_put_constrict
        public string auto_put_constrict { get; set; }

        //manual_get_constrict
        public string manual_get_constrict { get; set; }

        //manual_put_constrict
        public string manual_put_constrict { get; set; }

        //auto_fin_unclamp
        public string auto_fin_unclamp { get; set; }

        //manual_fin_unclamp
        public string manual_fin_unclamp { get; set; }

        //log_path
        public string log_path { get; set; }

        //equip_id
        //public string equip_id { get; set; }//20190613 移到 sysconfig

        public string ffu_rpm_open { get; set; }
        public string ffu_rpm_close { get; set; }

        public Boolean is_use_double_arm { get; set; }
        public Boolean is_use_r_arm { get; set; }
        public Boolean is_use_l_arm { get; set; }

        static Dictionary<string, Recipe> tmp = new Dictionary<string, Recipe>();

        public void Reload()
        {
            Recipe Content;
            tmp.Remove(this.recipe_id);

            ConfigTool<Recipe> SysCfg = new ConfigTool<Recipe>();
            Content = SysCfg.ReadFile("recipe/" + this.recipe_id + ".json");
            if (Content != null)
            {
                //Content.is_use_burnin = false;
                tmp.Add(this.recipe_id, Content);
            }
        }
        public static Recipe Get(string fileName)
        {
            Recipe Content;
            if (!tmp.TryGetValue(fileName, out Content))
            {
                ConfigTool<Recipe> SysCfg = new ConfigTool<Recipe>();
                Content = SysCfg.ReadFile("recipe/" + fileName + ".json");
                if (Content != null)
                {
                    //Content.is_use_burnin = false;
                    tmp.Add(fileName, Content);
                }
            }
            return Content;
        }
        public static void Set(string fileName, Recipe recipe)
        {
            if (recipe != null)
            {
                ConfigTool<Recipe> SysCfg = new ConfigTool<Recipe>();
                SysCfg.WriteFile("recipe/" + fileName + ".json", recipe);
                tmp.Remove(fileName);
                tmp.Add(fileName, (Recipe)recipe.MemberwiseClone());
            }
        }
        public static Boolean Delete(string fileName)
        {
            try
            {
                string date = System.DateTime.Now.ToString("yyyyMMdd");
                string time = System.DateTime.Now.ToString("HHmmss");

                string oldName = "recipe/" + fileName + ".json";
                string newName = "recipe/" + fileName + "_" + date + "_" + time + ".del";
                System.IO.File.Move(oldName, newName);
                return true;
            }
            catch (Exception e)
            {
                logger.Error(e.Message + " " + e.StackTrace);
                return false;
            }
        }

        public static void updateDBConfig(string recipeName)
        {
            try
            {
                Recipe recipe = Recipe.Get(recipeName);
                //DBUtil dBUtil = new DBUtil();
                //Dictionary<string, object> keyValues = new Dictionary<string, object>();
                //string strSql = " UPDATE config_node SET carrier_type = CASE node_id WHEN 'LOADPORT01' THEN @ctype1 " +
                //                "                                                    WHEN 'LOADPORT02' THEN @ctype2 " +
                //                "                                                    WHEN 'LOADPORT03' THEN @ctype3 " +
                //                "                                                    WHEN 'LOADPORT04' THEN @ctype4 " +
                //                "                                                    ELSE carrier_type END, " +
                //                "                          mode = CASE node_id WHEN 'LOADPORT01' THEN @mode1 " +
                //                "                                              WHEN 'LOADPORT02' THEN @mode2 " +
                //                "                                              WHEN 'LOADPORT03' THEN @mode3 " +
                //                "                                              WHEN 'LOADPORT04' THEN @mode4 " +
                //                "                                              ELSE mode END," +
                //                "                          enable_flg = CASE node_id WHEN 'LOADPORT01' THEN @enable1 " +
                //                "                                                    WHEN 'LOADPORT02' THEN @enable2 " +
                //                "                                                    WHEN 'LOADPORT03' THEN @enable3 " +
                //                "                                                    WHEN 'LOADPORT04' THEN @enable4 " +
                //                "                                                    WHEN 'ALIGNER01' THEN @bypassA1 " +
                //                "                                                    WHEN 'ALIGNER02' THEN @bypassA2 " +
                //                "                                                    ELSE enable_flg END," +       
                //                "                          double_arm = CASE node_id WHEN 'ROBOT01' THEN @double_arm_r1 " +
                //                "                                                    ELSE double_arm END, " +
                //                "                          r_arm = CASE node_id WHEN 'ROBOT01' THEN @r_arm_r1 " +
                //                "                                                    ELSE r_arm END, " +
                //                "                          l_arm = CASE node_id WHEN 'ROBOT01' THEN @l_arm_r1 " +
                //                "                                                    ELSE l_arm END, " +
                //                "                          wafer_size = @wafer_size, " +
                //                "                          modify_user = @modify_user, modify_timestamp = NOW() " +
                //                " WHERE equipment_model_id = @equipment_model_id " +
                //                "   AND node_type IN ('LOADPORT','ROBOT','Aligner') ;";

                //keyValues.Add("@equipment_model_id", SystemConfig.Get().SystemMode);
                //keyValues.Add("@modify_user", Global.currentUser);
                //keyValues.Add("@ctype1", recipe.port1_carrier_type);
                //keyValues.Add("@ctype2", recipe.port2_carrier_type);
                //keyValues.Add("@ctype3", recipe.port3_carrier_type);
                //keyValues.Add("@ctype4", recipe.port4_carrier_type);
                //keyValues.Add("@mode1", getPortType(recipe.port1_type));
                //keyValues.Add("@mode2", getPortType(recipe.port2_type));
                //keyValues.Add("@mode3", getPortType(recipe.port3_type));
                //keyValues.Add("@mode4", getPortType(recipe.port4_type));
                //keyValues.Add("@enable1", getEnable(recipe.port1_type));
                //keyValues.Add("@enable2", getEnable(recipe.port2_type));
                //keyValues.Add("@enable3", getEnable(recipe.port3_type));
                //keyValues.Add("@enable4", getEnable(recipe.port4_type));
                //keyValues.Add("@bypassA1", recipe.is_use_aligner1 ? 1 : 0);
                //keyValues.Add("@bypassA2", recipe.is_use_aligner2 ? 1 : 0);
                //keyValues.Add("@double_arm_r1", recipe.is_use_double_arm ? 1 : 0);
                //keyValues.Add("@r_arm_r1", recipe.is_use_r_arm ? 1 : 0);
                //keyValues.Add("@l_arm_r1", recipe.is_use_l_arm ? 1 : 0);
                //keyValues.Add("@wafer_size", recipe.wafer_size);
                //dBUtil.ExecuteNonQuery(strSql, keyValues);
                try
                {
                    foreach (Node node in NodeManagement.GetList())
                    {
                        switch (node.Name.ToUpper())
                        {
                            case "LOADPORT01":
                                node.WaferSize = recipe.wafer_size;
                                node.CarrierType = recipe.port1_carrier_type;
                                node.Mode = getPortType(recipe.port1_type);
                                node.Enable = getEnable(recipe.port1_type) == 1 ? true : false;
                                node.OrgSearchComplete = false;
                                break;
                            case "LOADPORT02":
                                node.WaferSize = recipe.wafer_size;
                                node.CarrierType = recipe.port2_carrier_type;
                                node.Mode = getPortType(recipe.port2_type);
                                node.Enable = getEnable(recipe.port2_type) == 1 ? true : false;
                                node.OrgSearchComplete = false;
                                break;
                            case "LOADPORT03":
                                node.WaferSize = recipe.wafer_size;
                                node.CarrierType = recipe.port3_carrier_type;
                                node.Mode = getPortType(recipe.port3_type);
                                node.Enable = getEnable(recipe.port3_type) == 1 ? true : false;
                                node.OrgSearchComplete = false;
                                break;
                            case "LOADPORT04":
                                node.WaferSize = recipe.wafer_size;
                                node.CarrierType = recipe.port4_carrier_type;
                                node.Mode = getPortType(recipe.port4_type);
                                node.Enable = getEnable(recipe.port4_type) == 1 ? true : false;
                                node.OrgSearchComplete = false;
                                break;
                            case "ROBOT01":
                                node.WaferSize = recipe.wafer_size;
                                node.RArmActive = recipe.is_use_r_arm;
                                node.LArmActive = recipe.is_use_l_arm;
                                node.DoubleArmActive = recipe.is_use_double_arm;
                                break;
                            case "ALIGNER01":
                                node.WaferSize = recipe.wafer_size;
                                node.Enable = recipe.is_use_aligner1;
                                break;
                            case "ALIGNER02":
                                node.WaferSize = recipe.wafer_size;
                                node.Enable = recipe.is_use_aligner2;
                                break;
                        }
                    }
                    NodeManagement.Save();
                }
                catch (Exception ex)
                {
                    logger.Error("Update load port 資訊失敗! " + ex.StackTrace);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace);
            }
        }
        private static string getPortType(string port_type)
        {
            string result = "";
            switch (port_type)
            {
                case "L":
                    result = "LD";
                    break;
                case "U":
                    result = "ULD";
                    break;
                case "N":
                    result = "";
                    break;
            }
            return result;
        }
        private static int getEnable(string port_type)
        {
            int result = 0;
            switch (port_type)
            {
                case "L":
                case "U":
                    result = 1;
                    break;
                case "N":
                    result = 0;
                    break;
            }
            return result;
        }
    }
}
