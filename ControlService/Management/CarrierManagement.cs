using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlService.Management
{
    public class CarrierManagement
    {
        private static List<Carrier> CarrierList = new List<Carrier>();

        public static void Initial()
        {
            CarrierList.Clear();
        }

        public static int GetNewIndex()
        {
            int currentIdx = 1;

            lock (CarrierList)
            {
                while (true)
                {
                    var find = from each in CarrierList
                               where each.CarrierIndex == currentIdx
                               select each;
                    if (find.Count() == 0)
                    {
                        return currentIdx;
                    }
                    currentIdx++;
                }
            }

        }

        public static List<Carrier> GetCarrierList()
        {

            List<Carrier> result = new List<Carrier>();
            lock (CarrierList)
            {
                lock (CarrierList)
                {
                    result = CarrierList;
                }
            }
            return result;
        }

        public static Carrier FindByLocation(string PortName)
        {
            Carrier result = null;
            lock (CarrierList)
            {
                var find = from each in CarrierList
                           where each.LocationID.ToUpper().Equals(PortName.ToUpper())
                           select each;
                if (find.Count() != 0)
                {
                    result = find.First();
                }
            }
            return result;
        }

        public static Carrier Get(string CarrierId)
        {
            Carrier result = null;

            lock (CarrierList)
            {
                var find = from each in CarrierList
                           where each.CarrierID.ToUpper().Equals(CarrierId.ToUpper())
                           select each;
                if (find.Count() != 0)
                {
                    result = find.First();
                }
            }

            return result;
        }

        public static Carrier Add()
        {
            Carrier Carrier = new Carrier();
            lock (CarrierList)
            {
                CarrierList.Add(Carrier);
            }
            return Carrier;
        }

        public static bool Remove(Carrier Carrier)
        {
            if (Carrier == null)
            {
                return true;
            }
            bool result = false;
            lock (CarrierList)
            {
                Node orgNode = NodeManagement.Get(Carrier.LocationID);
                if (orgNode != null)
                {
                    orgNode.Carrier = null;
                }
                result = CarrierList.Remove(Carrier);

            }
            return result;
        }

    }
}
