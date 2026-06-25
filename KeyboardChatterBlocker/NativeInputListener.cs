using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace KeyboardChatterBlocker;

public static class NativeInputListener
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private static Thread _listenThread;
    private static bool _isRunning;

    public static readonly ConcurrentQueue<KeyCode> CapturedKeys = new ConcurrentQueue<KeyCode>();

    public static void StartListening()
    {
        if (_isRunning) return;

        while (CapturedKeys.TryDequeue(out _)) ;

        _isRunning = true;
        _listenThread = new Thread(LoopListen)
        {
            IsBackground = true,
            Name = "ChatterBlockerNativeInput"
        };
        _listenThread.Start();
    }

    public static void StopListening()
    {
        _isRunning = false;
        _listenThread = null;
    }

    private static void LoopListen()
    {
        bool[] keyWasPressed = new bool[256];

        while (_isRunning)
        {
            for (int vk = 0x07; vk < 256; vk++)
            {
                bool isDown = (GetAsyncKeyState(vk) & 0x8000) != 0;

                if (isDown && !keyWasPressed[vk])
                {
                    KeyCode mappedKey = ConvertVirtualKeyToUnityKey(vk);
                    if (mappedKey != KeyCode.None)
                    {
                        CapturedKeys.Enqueue(mappedKey);
                    }
                }
                keyWasPressed[vk] = isDown;
            }
            Thread.Sleep(2);
        }
    }

    private static KeyCode ConvertVirtualKeyToUnityKey(int vk)
    {
        if (vk >= 0x30 && vk <= 0x39) return (KeyCode)((int)KeyCode.Alpha0 + (vk - 0x30)); 
        if (vk >= 0x41 && vk <= 0x5A) return (KeyCode)((int)KeyCode.A + (vk - 0x41));    

        if (vk >= 0x70 && vk <= 0x7E) return (KeyCode)((int)KeyCode.F1 + (vk - 0x70));

        if (vk >= 0x60 && vk <= 0x69) return (KeyCode)((int)KeyCode.Keypad0 + (vk - 0x60));

        return vk switch
        {
            0x20 => KeyCode.Space,
            0x0D => KeyCode.Return,
            0x09 => KeyCode.Tab,
            0x1B => KeyCode.Escape,
            0x14 => KeyCode.CapsLock,
            //0x2B => KeyCode.Exclaim,
            0x2C => KeyCode.Print,
            0x2D => KeyCode.Insert,
            0x2E => KeyCode.Delete,
            0x2F => KeyCode.Help,

            0x25 => KeyCode.LeftArrow,
            0x26 => KeyCode.UpArrow,
            0x27 => KeyCode.RightArrow,
            0x28 => KeyCode.DownArrow,
            0x21 => KeyCode.PageUp,
            0x22 => KeyCode.PageDown,
            0x23 => KeyCode.End,
            0x24 => KeyCode.Home,
            0x08 => KeyCode.Backspace,

            0xA0 => KeyCode.LeftShift,
            0xA1 => KeyCode.RightShift,
            0xA2 => KeyCode.LeftControl,
            0xA3 => KeyCode.RightControl,
            0xA4 => KeyCode.LeftAlt,
            0xA5 => KeyCode.RightAlt,
            0x5B => KeyCode.LeftWindows,
            0x5C => KeyCode.RightWindows,


            0x6A => KeyCode.KeypadMultiply,
            0x6B => KeyCode.KeypadPlus,
            0x6C => KeyCode.KeypadEnter,
            0x6D => KeyCode.KeypadMinus,
            0x6E => KeyCode.KeypadPeriod,
            0x6F => KeyCode.KeypadDivide,
            0x90 => KeyCode.Numlock,
            0x91 => KeyCode.ScrollLock,

            0xBA => KeyCode.Semicolon,
            0xBB => KeyCode.Equals,
            0xBC => KeyCode.Comma,
            0xBD => KeyCode.Minus,
            0xBE => KeyCode.Period,
            0xBF => KeyCode.Slash,
            0xC0 => KeyCode.BackQuote,
            0xDB => KeyCode.LeftBracket,
            0xDC => KeyCode.Backslash,
            0xDD => KeyCode.RightBracket,
            0xDE => KeyCode.Quote,

            _ => KeyCode.None
        };
    }
}

