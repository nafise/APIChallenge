using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoConsole
{
    public class AutoDataSet
    {
        public string datasetId { get; set; }
        public List<Dealer> dealers { get; set; }

    }
    public class SAutoDataSet
    {
        public List<Dealer> dealers { get; set; }
    }
}
