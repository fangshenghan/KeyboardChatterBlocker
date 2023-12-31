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
            this.ignored_async_keys = "忽略按键(异步)";
            this.ignored_keys = "忽略按键";
            this.change_keys = "改变按键";
            this.listening_keys = "等待按键...";
            this.clear_keys = "清空按键";
        }
        
    }
}
