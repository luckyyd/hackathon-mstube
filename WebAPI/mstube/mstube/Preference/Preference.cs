using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mstube.Preference
{
    public class Preference
    {
        public long user_id { get; set; }

        public long item_id { get; set; }

        public int score { get; set; }

        public long timestamp { get; set; }
    }
}