using System;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
//using System.Windows.Forms;
namespace AutoUIPlayback
{
    public class Win32Api
    {
        public const int INPUT_MOUSE = 0;
        public const int INPUT_KEYBOARD = 1;
        public const int INPUT_HARDWARE = 2;
        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const uint KEYEVENTF_KEYUP = 0x0002;
        public const uint KEYEVENTF_UNICODE = 0x0004;
        public const uint KEYEVENTF_SCANCODE = 0x0008;
        public const uint XBUTTON1 = 0x0001;
        public const uint XBUTTON2 = 0x0002;
        public const uint MOUSEEVENTF_MOVE = 0x0001;
        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;
        public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        public const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        public const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        public const uint MOUSEEVENTF_XDOWN = 0x0080;
        public const uint MOUSEEVENTF_XUP = 0x0100;
        public const uint MOUSEEVENTF_WHEEL = 0x0800;
        public const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;
        public const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;
        [DllImport("User32.dll")]
        public static extern uint SendInput(uint numberOfInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] Input[] input, int structSize);
        [DllImport("user32.dll")]
        public static extern IntPtr GetMessageExtraInfo();
        [DllImport("kernel32.dll")]
        public static extern int GetTickCount();
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Point pt);
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);
        public static void KeyDown(Keys key)
        {
            Input[] inputs = new Input[1];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].keyboardInput.wVk = (short)key;
            inputs[0].keyboardInput.dwFlags = 0;
            inputs[0].keyboardInput.time = GetTickCount();
            inputs[0].keyboardInput.dwExtraInfo = GetMessageExtraInfo();
            SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }
        public static void KeyUp(Keys key)
        {
            Input[] inputs = new Input[1];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].keyboardInput.wVk = (short)key;
            inputs[0].keyboardInput.dwFlags = (int)KEYEVENTF_KEYUP;
            inputs[0].keyboardInput.time = GetTickCount();
            inputs[0].keyboardInput.dwExtraInfo = GetMessageExtraInfo();
            SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }
        public static void MouseLeftKeyDown()
        {
            Input[] inputs = new Input[1];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].mouseInput.dx = 0;
            inputs[0].mouseInput.dy = 0;
            inputs[0].mouseInput.mouseData = 0;
            inputs[0].mouseInput.dwFlags = (int)MOUSEEVENTF_LEFTDOWN;
            inputs[0].mouseInput.time = GetTickCount();
            inputs[0].mouseInput.dwExtraInfo = GetMessageExtraInfo();
            SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }
        public static void MouseLeftKeyUp()
        {
            Input[] inputs = new Input[1];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].mouseInput.dx = 0;
            inputs[0].mouseInput.dy = 0;
            inputs[0].mouseInput.mouseData = 0;
            inputs[0].mouseInput.dwFlags = (int)MOUSEEVENTF_LEFTUP;
            inputs[0].mouseInput.time = GetTickCount();
            inputs[0].mouseInput.dwExtraInfo = GetMessageExtraInfo();
            SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }
        public static void MouseRightKeyDown(int x, int y)
        {
            Input[] inputs = new Input[1];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].mouseInput.dx = x;
            inputs[0].mouseInput.dy = y;
            inputs[0].mouseInput.mouseData = 0;
            inputs[0].mouseInput.dwFlags = (int)MOUSEEVENTF_RIGHTDOWN;
            inputs[0].mouseInput.time = GetTickCount();
            inputs[0].mouseInput.dwExtraInfo = GetMessageExtraInfo();
            SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }
        public static void MouseRightKeyUp(int x, int y)
        {
            Input[] inputs = new Input[1];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].mouseInput.dx = x;
            inputs[0].mouseInput.dy = y;
            inputs[0].mouseInput.mouseData = 0;
            inputs[0].mouseInput.dwFlags = (int)MOUSEEVENTF_RIGHTUP;
            inputs[0].mouseInput.time = GetTickCount();
            inputs[0].mouseInput.dwExtraInfo = GetMessageExtraInfo();
            SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }
        public static void MouseMiddleKeyDown()
        {
            Input[] inputs = new Input[1];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].mouseInput.dx = 0;
            inputs[0].mouseInput.dy = 0;
            inputs[0].mouseInput.mouseData = 0;
            inputs[0].mouseInput.dwFlags = (short)MOUSEEVENTF_MIDDLEDOWN;
            inputs[0].mouseInput.time = GetTickCount();
            inputs[0].mouseInput.dwExtraInfo = GetMessageExtraInfo();
            SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }
        public static void MouseMiddleKeyUp()
        {
            Input[] inputs = new Input[1];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].mouseInput.dx = 0;
            inputs[0].mouseInput.dy = 0;
            inputs[0].mouseInput.mouseData = 0;
            inputs[0].mouseInput.dwFlags = (short)MOUSEEVENTF_MIDDLEUP;
            inputs[0].mouseInput.time = GetTickCount();
            inputs[0].mouseInput.dwExtraInfo = GetMessageExtraInfo();
            SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }
        public static void MouseMove(int cx, int cy)
        {
            Input[] inputs = new Input[1];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].mouseInput.dx = cy;
            inputs[0].mouseInput.dy = cy;
            inputs[0].mouseInput.mouseData = 0;
            inputs[0].mouseInput.dwFlags = (int)MOUSEEVENTF_MOVE;
            inputs[0].mouseInput.time = GetTickCount();
            inputs[0].mouseInput.dwExtraInfo = GetMessageExtraInfo();
            SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }
        public static void MouseMoveTo(int x, int y)
        {
            MouseMoveTo(x, y, 0, 0);
        }
        public static void MouseMoveTo(int x, int y, int maxMove, int interval)
        {
            Input[] inputs = new Input[1];
            Point p = new Point();
            int n;
            int perWidth = (0xFFFF / (GetSystemMetrics(SM_CXSCREEN) - 1));
            int perHeight = (0xFFFF / (GetSystemMetrics(SM_CYSCREEN) - 1));
            if (maxMove <= 0) { maxMove = 0x7FFFFFFF; }
            GetCursorPos(out p);
            while (p.X != x || p.Y != y)
            {
                n = x - p.X;
                if (Math.Abs(n) > maxMove)
                {
                    if (n > 0) { n = maxMove; }
                    else { n = -maxMove; }
                }
                p.X = p.X + n;
                n = y - p.Y;
                if (Math.Abs(n) > maxMove)
                {
                    if (n > 0) { n = maxMove; }
                    else { n = -maxMove; }
                }
                p.Y = p.Y + n;
                inputs[0].type = INPUT_MOUSE;
                inputs[0].mouseInput.dx = p.X * perWidth;
                inputs[0].mouseInput.dy = p.Y * perHeight;
                inputs[0].mouseInput.mouseData = 0;
                inputs[0].mouseInput.dwFlags = (int)(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE);
                inputs[0].mouseInput.time = GetTickCount();
                inputs[0].mouseInput.dwExtraInfo = GetMessageExtraInfo();
                SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));

                if (interval != 0) { System.Threading.Thread.Sleep(interval); }
            }
        }
        public static void MouseWheel(int cz)
        {
            Input[] inputs = new Input[1];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].mouseInput.dx = 0;
            inputs[0].mouseInput.dy = 0;
            inputs[0].mouseInput.mouseData = cz;
            inputs[0].mouseInput.dwFlags = (int)MOUSEEVENTF_WHEEL;
            inputs[0].mouseInput.time = GetTickCount();
            inputs[0].mouseInput.dwExtraInfo = GetMessageExtraInfo();
            SendInput(1, inputs, System.Runtime.InteropServices.Marshal.SizeOf(inputs[0]));
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct KeyBoardInput
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HardwareInput
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }
        [StructLayout(LayoutKind.Explicit)]
        public struct Input
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(4)]
            public MouseInput mouseInput;
            [FieldOffset(4)]
            public KeyBoardInput keyboardInput;
            [FieldOffset(4)]
            public HardwareInput hardwardInput;
        }
    }
}