using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoConsole
{
    [Serializable()]
    public class Vehicle
    {
        public int vehicleId { get; set; }
        public int year { get; set; }
        public string make { get; set; }
        public string model { get; set; }

        [NonSerialized]
        private int _dealerId;
        public int dealerId
        {
            get { return _dealerId; }
            set { _dealerId = value; }
        }

    }


    public class SVehicle
    {
        public int vehicleId { get; set; }
        public int year { get; set; }
        public string make { get; set; }
        public string model { get; set; }
    }
}
