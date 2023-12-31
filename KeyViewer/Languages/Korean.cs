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
            this.ignored_async_keys = "무시된 키(Async)";
            this.ignored_keys = "무시된 키";
            this.listening_keys = "청취 키...";
            this.change_keys = "키 변경";
            this.clear_keys = "모든 키 지우기";
        }
        
    }
}
