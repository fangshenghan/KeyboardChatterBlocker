using SkyHook;
using System.Collections.Generic;

namespace KeyboardChatterBlocker
{
    public static class Utils
    {

        public static readonly ISet<AsyncKeyCode> ALWAYS_BOUND_ASYNC_KEYS = new HashSet<AsyncKeyCode> {
            new AsyncKeyCode(0, KeyLabel.MouseLeft),
            new AsyncKeyCode(1, KeyLabel.MouseRight),
            new AsyncKeyCode(2, KeyLabel.MouseMiddle),
            new AsyncKeyCode(3, KeyLabel.MouseX1),
            new AsyncKeyCode(4, KeyLabel.MouseX2),
        };

        public static List<AsyncKeyCode> GetKeysDownThisFrame()
        {
            List<AsyncKeyCode> list = new List<AsyncKeyCode>();
            foreach (AsyncKeyCode asyncKeyCode in AsyncInputManager.frameDependentKeyDownMask)
            {
                list.Add(asyncKeyCode);
            }
            return list;
        }

    }
}
