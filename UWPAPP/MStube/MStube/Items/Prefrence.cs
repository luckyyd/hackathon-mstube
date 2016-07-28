using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MStube.Items
{
    public class Prefrence
    {
        public long user_id { get; set; }
        public long item_id { get; set; }
        public int score { get; set; }
        public long timestamp { get; set; }
    }
}
