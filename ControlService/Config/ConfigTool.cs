using log4net;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlService.Config
{
    public class ConfigTool<T>
    {
        ILog logger = LogManager.GetLogger(typeof(ConfigTool<T>));
       

        public T ReadFile(string FilePath)
        {

            try
            {
       
                string t = File.ReadAllText(FilePath, Encoding.UTF8);
                T r = JsonSerializer.Deserialize<T>(File.ReadAllText(FilePath, Encoding.UTF8));
                return r;
            }
            catch (Exception ex)
            {
                logger.Error("ReadFile:" + ex.Message + "\n" + ex.StackTrace);
            }

            return default(T);
        }

        public void WriteFile(string FilePath, T Obj)
        {
            try
            {
                
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                File.WriteAllText(FilePath, JsonSerializer.Serialize<T>(Obj, options), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                logger.Error("WriteFile:" + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
