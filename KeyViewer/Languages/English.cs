using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardChatterBlocker.Languages
{
    public class English : Language
    {
        public English()
        {
            this.language = "English";
            this.chatter_threshold_label = "Chatter Threshold (ms)";
        }
        
    }
}
