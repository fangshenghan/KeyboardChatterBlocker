using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardChatterBlocker.Languages
{
    public class Korean : Language
    {
        public Korean()
        {
            this.language = "한국어";
            this.chatter_threshold_label = "채터 임계값 (ms)";
        }
        
    }
}
