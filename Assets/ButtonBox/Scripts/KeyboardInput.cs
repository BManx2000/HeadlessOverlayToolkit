using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class KeyboardInput {

    [DllImport("user32.dll")]
    internal static extern uint SendInput(uint nInputs,
   [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs,
   int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT {
        internal InputType type;
        internal InputUnion U;
        internal static int Size {
            get { return Marshal.SizeOf(typeof(INPUT)); }
        }
    }

    internal enum InputType : uint {
        MOUSE = 0,
        KEYBOARD = 1,
        HARDWARE = 2
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct InputUnion {
        [FieldOffset(0)]
        internal MOUSEINPUT mi;
        [FieldOffset(0)]
        internal KEYBDINPUT ki;
        [FieldOffset(0)]
        internal HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSEINPUT {
        internal int dx;
        internal int dy;
        internal int mouseData;
        internal uint dwFlags;
        internal uint time;
        internal UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KEYBDINPUT {
        internal short wVk;
        internal short wScan;
        internal KEYEVENTF dwFlags;
        internal int time;
        internal UIntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HARDWAREINPUT {
        internal int uMsg;
        internal short wParamL;
        internal short wParamH;
    }

    [Flags]
    internal enum KEYEVENTF : uint {
        EXTENDEDKEY = 0x0001,
        KEYUP = 0x0002,
        SCANCODE = 0x0008,
        UNICODE = 0x0004
    }

    public static void Down(KeyCombo k) {
        if(k == null || k.keypresses.Count == 0) {
            return;
        }

        int numKeys = k.keypresses.Count;
        INPUT[] InputData = new INPUT[numKeys];

        for(int i=0; i<numKeys; i++) {
            InputData[i].type = InputType.KEYBOARD;
            InputData[i].U.ki.wScan = (short)k.keypresses[i].Scancode;
            InputData[i].U.ki.dwFlags = KEYEVENTF.SCANCODE | (k.keypresses[i].Extended ? KEYEVENTF.EXTENDEDKEY : 0);
        }

        // send keydown
        if (SendInput((uint)numKeys, InputData, Marshal.SizeOf(InputData[0])) == 0) {
            Debug.Log("SendInput failed with code: " +
            Marshal.GetLastWin32Error().ToString());
        }
    }

    public static void Up(KeyCombo k) {
        if (k == null || k.keypresses.Count == 0) {
            return;
        }

        int numKeys = k.keypresses.Count;
        INPUT[] InputData = new INPUT[numKeys];

        for(int i=0; i<numKeys; i++) {
            InputData[i].type = InputType.KEYBOARD;
            InputData[i].U.ki.wScan = (short)k.keypresses[numKeys-1-i].Scancode;
            InputData[i].U.ki.dwFlags = KEYEVENTF.SCANCODE | KEYEVENTF.KEYUP | (k.keypresses[numKeys - 1 - i].Extended ? KEYEVENTF.EXTENDEDKEY : 0);
        }
        
        // send keyup
        if (SendInput((uint)numKeys, InputData, Marshal.SizeOf(InputData[0])) == 0) {
            Debug.Log("SendInput failed with code: " +
            Marshal.GetLastWin32Error().ToString());
        }
    }
}
