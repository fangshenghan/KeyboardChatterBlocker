using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyboardChatterBlocker.Languages
{
    public class Chinese : Language
    {
        public Chinese()
        {
            this.language = "简体中文";
            this.chatter_threshold_label = "按键间隔 (毫秒)";
        }
        
    }
}
