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
            this.ignored_async_keys = "Ignored Keys(Async)";
            this.ignored_keys = "Ignored Keys";
            this.listening_keys = "Listening Keys...";
            this.change_keys = "Change Keys";
            this.clear_keys = "Clear Keys";
            this.enable_key_limiter = "Enable Key Limiter";
            this.allowed_async_keys = "Allowed Keys (Async)";
            this.allowed_keys = "Allowed Keys";
        }
        
    }
}
