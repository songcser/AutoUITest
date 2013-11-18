using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GlobalHooksTest
{
	public class GlobalHooks
	{
		public delegate void HookReplacedEventHandler();
		public delegate void WindowEventHandler(IntPtr Handle);
		public delegate void SysCommandEventHandler(int SysCommand, int lParam);
		public delegate void ActivateShellWindowEventHandler();
		public delegate void TaskmanEventHandler();
		public delegate void BasicHookEventHandler(IntPtr Handle1, IntPtr Handle2);
		public delegate void WndProcEventHandler(IntPtr Handle, IntPtr Message, IntPtr wParam, IntPtr lParam);


		// Functions imported from our unmanaged DLL
		[DllImport("GlobalCbtHook.dll")]
		private static extern bool InitializeCbtHook(int threadID, IntPtr DestWindow);
		[DllImport("GlobalCbtHook.dll")]
		private static extern void UninitializeCbtHook();
		[DllImport("GlobalCbtHook.dll")]
		private static extern bool InitializeShellHook(int threadID, IntPtr DestWindow);
		[DllImport("GlobalCbtHook.dll")]
		private static extern void UninitializeShellHook();
		[DllImport("GlobalCbtHook.dll")]
		private static extern void InitializeKeyboardHook(int threadID, IntPtr DestWindow);
		[DllImport("GlobalCbtHook.dll")]
		private static extern void UninitializeKeyboardHook();
		[DllImport("GlobalCbtHook.dll")]
		private static extern void InitializeMouseHook(int threadID, IntPtr DestWindow);
		[DllImport("GlobalCbtHook.dll")]
		private static extern void UninitializeMouseHook();
		[DllImport("GlobalCbtHook.dll")]
		private static extern void InitializeKeyboardLLHook(int threadID, IntPtr DestWindow);
		[DllImport("GlobalCbtHook.dll")]
		private static extern void UninitializeKeyboardLLHook();
		[DllImport("GlobalCbtHook.dll")]
		private static extern void InitializeMouseLLHook(int threadID, IntPtr DestWindow);
		[DllImport("GlobalCbtHook.dll")]
		private static extern void UninitializeMouseLLHook();
		[DllImport("GlobalCbtHook.dll")]
		private static extern void InitializeCallWndProcHook(int threadID, IntPtr DestWindow);
		[DllImport("GlobalCbtHook.dll")]
		private static extern void UninitializeCallWndProcHook();
		[DllImport("GlobalCbtHook.dll")]
		private static extern void InitializeGetMsgHook(int threadID, IntPtr DestWindow);
		[DllImport("GlobalCbtHook.dll")]
		private static extern void UninitializeGetMsgHook();
        [DllImport("GlobalCbtHook.dll")]
        private static extern void InitializeSysMsgFilterHook(int threadID, IntPtr DestWindow);
        [DllImport("GlobalCbtHook.dll")]
        private static extern void UninitializeSysMsgFilterHook();

		// API call needed to retreive the value of the messages to intercept from the unmanaged DLL
		[DllImport("user32.dll")]
		private static extern int RegisterWindowMessage(string lpString);
		[DllImport("user32.dll")]
		private static extern IntPtr GetProp(IntPtr hWnd, string lpString);
		[DllImport("user32.dll")]
		private static extern IntPtr GetDesktopWindow();

		// Handle of the window intercepting messages
		private IntPtr _Handle;

		private CBTHook _CBT;
		private ShellHook _Shell;
		private KeyboardHook _Keyboard;
		private MouseHook _Mouse;
		private KeyboardLLHook _KeyboardLL;
		private MouseLLHook _MouseLL;
		private CallWndProcHook _CallWndProc;
		private GetMsgHook _GetMsg;
        private SysMsgFilterHook _SysMsgFilter;

        private static bool isCapsLockKey = false;
        private static bool isCtrlKey = false;
        private static bool isShiftKey = false;
        private static bool isAltKey = false;
        private static bool isTab = false;

		public GlobalHooks(IntPtr Handle)
		{
			_Handle = Handle;

			_CBT = new CBTHook(_Handle);
			_Shell = new ShellHook(_Handle);
			_Keyboard = new KeyboardHook(_Handle);
			_Mouse = new MouseHook(_Handle);
			_KeyboardLL = new KeyboardLLHook(_Handle);
			_MouseLL = new MouseLLHook(_Handle);
			_CallWndProc = new CallWndProcHook(_Handle);
			_GetMsg = new GetMsgHook(_Handle);
            _SysMsgFilter = new SysMsgFilterHook(_Handle);
		}

		~GlobalHooks()
		{
			_CBT.Stop();
			_Shell.Stop();
			_Keyboard.Stop();
			_Mouse.Stop();
			_KeyboardLL.Stop();
			_MouseLL.Stop();
			_CallWndProc.Stop();
			_GetMsg.Stop();
		}

		public void ProcessWindowMessage(ref System.Windows.Forms.Message m)
		{
			_CBT.ProcessWindowMessage(ref m);
			_Shell.ProcessWindowMessage(ref m);
			_Keyboard.ProcessWindowMessage(ref m);
			_Mouse.ProcessWindowMessage(ref m);
			_KeyboardLL.ProcessWindowMessage(ref m);
			_MouseLL.ProcessWindowMessage(ref m);
			_CallWndProc.ProcessWindowMessage(ref m);
			_GetMsg.ProcessWindowMessage(ref m);
            _SysMsgFilter.ProcessWindowMessage(ref m);
		}

		#region Accessors

		public CBTHook CBT
		{
			get { return _CBT; }
		}

		public ShellHook Shell
		{
			get { return _Shell; }
		}

		public KeyboardHook Keyboard
		{
			get { return _Keyboard; }
		}

		public MouseHook Mouse
		{
			get { return _Mouse; }
		}

		public KeyboardLLHook KeyboardLL
		{
			get { return _KeyboardLL; }
		}

		public MouseLLHook MouseLL
		{
			get  { return _MouseLL; }
		}

		public CallWndProcHook CallWndProc
		{
			get { return _CallWndProc; }
		}

		public GetMsgHook GetMsg
		{
			get { return _GetMsg; }
		}

        public SysMsgFilterHook SysMsgFilter
        {
            get { return _SysMsgFilter; }
        }

		#endregion

		public abstract class Hook
		{
			protected bool _IsActive = false;
			protected IntPtr _Handle;

			public Hook(IntPtr Handle)
			{
				_Handle = Handle;
			}

			public void Start()
			{
				if (!_IsActive)
				{
					_IsActive = true;
					OnStart();
				}
			}

			public void Stop()
			{
				if (_IsActive)
				{
					OnStop();
					_IsActive = false;
				}
			}

			~Hook()
			{
				Stop();
			}

			public bool IsActive
			{
				get { return _IsActive; }
			}

			protected abstract void OnStart();
			protected abstract void OnStop();
			public abstract void ProcessWindowMessage(ref System.Windows.Forms.Message m);
		}

		public class CBTHook : Hook
		{
			// Values retreived with RegisterWindowMessage
			private int _MsgID_CBT_HookReplaced;
			private int _MsgID_CBT_Activate;
			private int _MsgID_CBT_CreateWnd;
			private int _MsgID_CBT_DestroyWnd;
			private int _MsgID_CBT_MinMax;
			private int _MsgID_CBT_MoveSize;
			private int _MsgID_CBT_SetFocus;
			private int _MsgID_CBT_SysCommand;

			public event HookReplacedEventHandler HookReplaced;
			public event WindowEventHandler Activate;
			public event WindowEventHandler CreateWindow;
			public event WindowEventHandler DestroyWindow;
			public event WindowEventHandler MinMax;
			public event WindowEventHandler MoveSize;
			public event WindowEventHandler SetFocus;
			public event SysCommandEventHandler SysCommand;

			public CBTHook(IntPtr Handle) : base(Handle)
			{
			}

			protected override void OnStart()
			{
				// Retreive the message IDs that we'll look for in WndProc
				_MsgID_CBT_HookReplaced = RegisterWindowMessage("WILSON_HOOK_CBT_REPLACED");
				_MsgID_CBT_Activate = RegisterWindowMessage("WILSON_HOOK_HCBT_ACTIVATE");
				_MsgID_CBT_CreateWnd = RegisterWindowMessage("WILSON_HOOK_HCBT_CREATEWND");
				_MsgID_CBT_DestroyWnd = RegisterWindowMessage("WILSON_HOOK_HCBT_DESTROYWND");
				_MsgID_CBT_MinMax = RegisterWindowMessage("WILSON_HOOK_HCBT_MINMAX");
				_MsgID_CBT_MoveSize = RegisterWindowMessage("WILSON_HOOK_HCBT_MOVESIZE");
				_MsgID_CBT_SetFocus = RegisterWindowMessage("WILSON_HOOK_HCBT_SETFOCUS");
				_MsgID_CBT_SysCommand = RegisterWindowMessage("WILSON_HOOK_HCBT_SYSCOMMAND");

				// Start the hook
				InitializeCbtHook(0, _Handle);
			}

			protected override void OnStop()
			{
				UninitializeCbtHook();
			}

			public override void ProcessWindowMessage(ref System.Windows.Forms.Message m)
			{
				if (m.Msg == _MsgID_CBT_HookReplaced)
				{
					if (HookReplaced != null)
						HookReplaced();
				}
				else if (m.Msg == _MsgID_CBT_Activate)
				{
					if (Activate != null)
						Activate(m.WParam);
				}
				else if (m.Msg == _MsgID_CBT_CreateWnd)
				{
					if (CreateWindow != null)
						CreateWindow(m.WParam);
				}
				else if (m.Msg == _MsgID_CBT_DestroyWnd)
				{
					if (DestroyWindow != null)
						DestroyWindow(m.WParam);
				}
				else if (m.Msg == _MsgID_CBT_MinMax)
				{
					if (MinMax != null)
						MinMax(m.WParam);
				}
				else if (m.Msg == _MsgID_CBT_MoveSize)
				{
					if (MoveSize != null)
						MoveSize(m.WParam);
				}
				else if (m.Msg == _MsgID_CBT_SetFocus)
				{
					if (SetFocus != null)
						SetFocus(m.WParam);
                    
				}
				else if (m.Msg == _MsgID_CBT_SysCommand)
				{
					if (SysCommand != null)
						SysCommand(m.WParam.ToInt32(), m.LParam.ToInt32());
				}
			}
		}

		public class ShellHook : Hook
		{
			// Values retreived with RegisterWindowMessage
			private int _MsgID_Shell_ActivateShellWindow;
			private int _MsgID_Shell_GetMinRect;
			private int _MsgID_Shell_Language;
			private int _MsgID_Shell_Redraw;
			private int _MsgID_Shell_Taskman;
			private int _MsgID_Shell_HookReplaced;
			private int _MsgID_Shell_WindowActivated;
			private int _MsgID_Shell_WindowCreated;
			private int _MsgID_Shell_WindowDestroyed;

			public event HookReplacedEventHandler HookReplaced;
			public event ActivateShellWindowEventHandler ActivateShellWindow;
			public event WindowEventHandler GetMinRect;
			public event WindowEventHandler Language;
			public event WindowEventHandler Redraw;
			public event TaskmanEventHandler Taskman;
			public event WindowEventHandler WindowActivated;
			public event WindowEventHandler WindowCreated;
			public event WindowEventHandler WindowDestroyed;

			public ShellHook(IntPtr Handle) : base(Handle)
			{
			}

			protected override void OnStart()
			{
				// Retreive the message IDs that we'll look for in WndProc
				_MsgID_Shell_HookReplaced = RegisterWindowMessage("WILSON_HOOK_SHELL_REPLACED");
				_MsgID_Shell_ActivateShellWindow = RegisterWindowMessage("WILSON_HOOK_HSHELL_ACTIVATESHELLWINDOW");
				_MsgID_Shell_GetMinRect = RegisterWindowMessage("WILSON_HOOK_HSHELL_GETMINRECT");
				_MsgID_Shell_Language = RegisterWindowMessage("WILSON_HOOK_HSHELL_LANGUAGE");
				_MsgID_Shell_Redraw = RegisterWindowMessage("WILSON_HOOK_HSHELL_REDRAW");
				_MsgID_Shell_Taskman = RegisterWindowMessage("WILSON_HOOK_HSHELL_TASKMAN");
				_MsgID_Shell_WindowActivated = RegisterWindowMessage("WILSON_HOOK_HSHELL_WINDOWACTIVATED");
				_MsgID_Shell_WindowCreated = RegisterWindowMessage("WILSON_HOOK_HSHELL_WINDOWCREATED");
				_MsgID_Shell_WindowDestroyed = RegisterWindowMessage("WILSON_HOOK_HSHELL_WINDOWDESTROYED");

				// Start the hook
				InitializeShellHook(0, _Handle);
			}

			protected override void OnStop()
			{
				UninitializeShellHook();
			}

			public override void ProcessWindowMessage(ref System.Windows.Forms.Message m)
			{
				if (m.Msg == _MsgID_Shell_HookReplaced)
				{
					if (HookReplaced != null)
						HookReplaced();
				}
				else if (m.Msg == _MsgID_Shell_ActivateShellWindow)
				{
					if (ActivateShellWindow != null)
						ActivateShellWindow();
				}
				else if (m.Msg == _MsgID_Shell_GetMinRect)
				{
					if (GetMinRect != null)
						GetMinRect(m.WParam);
				}
				else if (m.Msg == _MsgID_Shell_Language)
				{
					if (Language != null)
						Language(m.WParam);
				}
				else if (m.Msg == _MsgID_Shell_Redraw)
				{
                    
					if (Redraw != null)
						Redraw(m.WParam);
				}
				else if (m.Msg == _MsgID_Shell_Taskman)
				{
					if (Taskman != null)
						Taskman();
				}
				else if (m.Msg == _MsgID_Shell_WindowActivated)
				{
					if (WindowActivated != null)
						WindowActivated(m.WParam);
				}
				else if (m.Msg == _MsgID_Shell_WindowCreated)
				{
					if (WindowCreated != null)
						WindowCreated(m.WParam);
				}
				else if (m.Msg == _MsgID_Shell_WindowDestroyed)
				{
					if (WindowDestroyed != null)
						WindowDestroyed(m.WParam);
				}
			}
		}

		public class KeyboardHook : Hook
		{
			// Values retreived with RegisterWindowMessage
			private int _MsgID_Keyboard;
			private int _MsgID_Keyboard_HookReplaced;

			public event HookReplacedEventHandler HookReplaced;
			public event BasicHookEventHandler KeyboardEvent;

			public KeyboardHook(IntPtr Handle) : base(Handle)
			{
			}

			protected override void OnStart()
			{
				// Retreive the message IDs that we'll look for in WndProc
				_MsgID_Keyboard = RegisterWindowMessage("WILSON_HOOK_KEYBOARD");
				_MsgID_Keyboard_HookReplaced = RegisterWindowMessage("WILSON_HOOK_KEYBOARD_REPLACED");

				// Start the hook
				InitializeKeyboardHook(0, _Handle);
			}
			protected override void OnStop()
			{
				UninitializeKeyboardHook();
			}

			public override void ProcessWindowMessage(ref System.Windows.Forms.Message m)
			{
				if (m.Msg == _MsgID_Keyboard)
				{
					if (KeyboardEvent != null)
						KeyboardEvent(m.WParam, m.LParam);
				}
				else if (m.Msg == _MsgID_Keyboard_HookReplaced)
				{
					if (HookReplaced != null)
						HookReplaced();
				}
			}
		}

		public class MouseHook : Hook
		{
			// Values retreived with RegisterWindowMessage
			private int _MsgID_Mouse;
			private int _MsgID_Mouse_HookReplaced;

			public event HookReplacedEventHandler HookReplaced;
			public event BasicHookEventHandler MouseEvent;

			public MouseHook(IntPtr Handle) : base(Handle)
			{
			}

			protected override void OnStart()
			{
				// Retreive the message IDs that we'll look for in WndProc
				_MsgID_Mouse = RegisterWindowMessage("WILSON_HOOK_MOUSE");
				_MsgID_Mouse_HookReplaced = RegisterWindowMessage("WILSON_HOOK_MOUSE_REPLACED");

				// Start the hook
				InitializeMouseHook(0, _Handle);
			}
			protected override void OnStop()
			{
				UninitializeMouseHook();
			}

			public override void ProcessWindowMessage(ref System.Windows.Forms.Message m)
			{
				if (m.Msg == _MsgID_Mouse)
				{
					if (MouseEvent != null)
						MouseEvent(m.WParam, m.LParam);
				}
				else if (m.Msg == _MsgID_Mouse_HookReplaced)
				{
					if (HookReplaced != null)
						HookReplaced();
				}
			}
		}

		public class KeyboardLLHook : Hook
		{
			// Values retreived with RegisterWindowMessage
			private int _MsgID_KeyboardLL;
			private int _MsgID_KeyboardLL_HookReplaced;

			public event HookReplacedEventHandler HookReplaced;
			public event BasicHookEventHandler KeyboardLLEvent;
            public event KeyEventHandler KeyDown;
            public event KeyEventHandler KeyUp;
            //public event KeyEventHandler SystemKeyDown;
            //public event KeyEventHandler SystemKeyUp;

            private const int WM_KEYDOWN        = 0x0100;
            private const int WM_KEYUP          = 0x0101;
            private const int WM_SYSTEMKEYDOWN  = 0x0104;
            private const int WM_SYSTEMKEYUP    = 0x0105;

			public KeyboardLLHook(IntPtr Handle) : base(Handle)
			{
			}
            struct KBDLLHOOKSTRUCT
            {
                public int vkCode;
                public IntPtr scanCode;
                public IntPtr flags;
                public IntPtr time;
                public IntPtr dwExtraInfo;
            }
			protected override void OnStart()
			{
				// Retreive the message IDs that we'll look for in WndProc
				_MsgID_KeyboardLL = RegisterWindowMessage("WILSON_HOOK_KEYBOARDLL");
				_MsgID_KeyboardLL_HookReplaced = RegisterWindowMessage("WILSON_HOOK_KEYBOARDLL_REPLACED");

				// Start the hook
				InitializeKeyboardLLHook(0, _Handle);
			}

			protected override void OnStop()
			{
				UninitializeKeyboardLLHook();
			}

			public override void ProcessWindowMessage(ref System.Windows.Forms.Message m)
			{
				if (m.Msg == _MsgID_KeyboardLL)
				{
					if (KeyboardLLEvent != null)
						KeyboardLLEvent(m.WParam, m.LParam);

                    KBDLLHOOKSTRUCT M = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(m.LParam, typeof(KBDLLHOOKSTRUCT));
                    int code = 0;
                    if (m.WParam.ToInt32() == WM_KEYDOWN)
                    {
                        if (KeyDown != null)
                        {
                            code = (int)M.vkCode;
                            //VirtualKeys vk = (VirtualKeys)M.vkCode;
                            //Keys key = ConvertKeyCode(vk);
                            Keys key = (Keys)M.vkCode;
                            if (key == Keys.LControlKey||key == Keys.RControlKey)
                            {
                                if (!isCtrlKey)
                                {
                                    KeyDown(this, new KeyEventArgs(key));
                                    isCtrlKey = true;
                                }
                                
                            }
                            else if (key == Keys.Alt)
                            {
                                if (!isAltKey)
                                {
                                    KeyDown(this,new KeyEventArgs(key));
                                    isAltKey = true;
                                }
                            }
                            else if (key == Keys.RShiftKey||key == Keys.LShiftKey)
                            {
                                if (!isShiftKey)
                                {
                                    KeyDown(this, new KeyEventArgs(key));
                                    isShiftKey = true;
                                }
                            }
                            else if (key == Keys.CapsLock)
                            {
                                if (!isCapsLockKey)
                                {
                                    KeyDown(this, new KeyEventArgs(key));
                                    isCapsLockKey = true;
                                }
                            }
                            else if (key == Keys.Tab)
                            {
                                if (!isTab)
                                {
                                    KeyDown(this, new KeyEventArgs(key));
                                    isTab = true;
                                }
                            }
                            else
                            {
                                KeyDown(this, new KeyEventArgs(key));
                            }
                            
                        }
                    }
                    else if (m.WParam.ToInt32() == WM_KEYUP)
                    {
                        if (KeyUp != null)
                        {
                            code = (int)M.vkCode;
                            VirtualKeys vk = (VirtualKeys)M.vkCode;
                            Keys key = ConvertKeyCode(vk);
                            //Keys key = (Keys)M.vkCode;
                            //KeyUp(this, new KeyEventArgs(key));
                            if (key == Keys.Control&&isCtrlKey)
                            {
                                isCtrlKey = false;
                            }
                            else if (key == Keys.Alt&&isAltKey)
                            {
                                isAltKey = false;
                            }
                            else if (key == Keys.Shift&&isShiftKey)
                            {
                                isShiftKey = false;
                            }
                            else if (key == Keys.CapsLock&&isCapsLockKey)
                            {
                                isCapsLockKey = false;
                            }
                            else if (key == Keys.Tab&&isTab)
                            {
                                isTab = false;
                            }
                            KeyUp(this, new KeyEventArgs(key));
                        }
                    }
                    else if (m.WParam.ToInt32() == WM_SYSTEMKEYDOWN)
                    {
                        if (KeyDown != null)
                        {
                            code = (int)M.vkCode;
                            VirtualKeys vk = (VirtualKeys)M.vkCode;
                            Keys key = ConvertKeyCode(vk);
                            //Keys key = (Keys)M.vkCode;
                            //KeyDown(this, new KeyEventArgs(key));
                            if (key == Keys.ControlKey)
                            {
                                if (!isCtrlKey)
                                {
                                    KeyDown(this, new KeyEventArgs(key));
                                    isCtrlKey = true;
                                }

                            }
                            else if (key == Keys.Alt)
                            {
                                if (!isAltKey)
                                {
                                    KeyDown(this, new KeyEventArgs(key));
                                    isAltKey = true;
                                }
                            }
                            else if (key == Keys.ShiftKey)
                            {
                                if (!isShiftKey)
                                {
                                    KeyDown(this, new KeyEventArgs(key));
                                    isShiftKey = true;
                                }
                            }
                            else if (key == Keys.CapsLock)
                            {
                                if (!isCapsLockKey)
                                {
                                    KeyDown(this, new KeyEventArgs(key));
                                    isCapsLockKey = true;
                                }
                            }
                            else if (key == Keys.Tab)
                            {
                                if (!isTab)
                                {
                                    KeyDown(this, new KeyEventArgs(key));
                                    isTab = true;
                                }
                            }
                            else
                            {
                                KeyDown(this, new KeyEventArgs(key));
                            }
                        }
                    }
                    else if (m.WParam.ToInt32() == WM_SYSTEMKEYUP)
                    {
                        if (KeyUp != null)
                        {
                            code = (int)M.vkCode;
                            VirtualKeys vk = (VirtualKeys)M.vkCode;
                            Keys key = ConvertKeyCode(vk);
                            //Keys key = (Keys)M.vkCode;
                            if (key == Keys.Control && isCtrlKey)
                            {
                                isCtrlKey = false;
                            }
                            else if (key == Keys.Alt && isAltKey)
                            {
                                isAltKey = false;
                            }
                            else if (key == Keys.Shift && isShiftKey)
                            {
                                isShiftKey = false;
                            }
                            else if (key == Keys.CapsLock && isCapsLockKey)
                            {
                                isCapsLockKey = false;
                            }
                            else if (key == Keys.Tab && isTab)
                            {
                                isTab = false;
                            }
                            KeyUp(this, new KeyEventArgs(key));
                        }
                    }
				}
				else if (m.Msg == _MsgID_KeyboardLL_HookReplaced)
				{
					if (HookReplaced != null)
						HookReplaced();
				}
			}
            private System.Windows.Forms.Keys ConvertKeyCode(VirtualKeys vk)
            {
                System.Windows.Forms.Keys key = System.Windows.Forms.Keys.Attn;

                switch (vk)
                {
                    case VirtualKeys.ShiftLeft:
                        key = System.Windows.Forms.Keys.Shift;
                        break;
                    case VirtualKeys.ShiftRight:
                        key = System.Windows.Forms.Keys.Shift;
                        break;
                    case VirtualKeys.ControlLeft:
                        key = System.Windows.Forms.Keys.Control;
                        break;
                    case VirtualKeys.ControlRight:
                        key = System.Windows.Forms.Keys.Control;
                        break;
                    case VirtualKeys.AltLeft:
                        key = System.Windows.Forms.Keys.Alt;
                        break;
                    case VirtualKeys.AltRight:
                        key = System.Windows.Forms.Keys.Alt;
                        break;
                    case VirtualKeys.Back:
                        key = System.Windows.Forms.Keys.Back;
                        break;
                    case VirtualKeys.Tab:
                        key = System.Windows.Forms.Keys.Tab;
                        break;
                    case VirtualKeys.Clear:
                        key = System.Windows.Forms.Keys.Clear;
                        break;
                    case VirtualKeys.Return:
                        key = System.Windows.Forms.Keys.Return;
                        break;
                    case VirtualKeys.Menu:
                        key = System.Windows.Forms.Keys.Menu;
                        break;
                    case VirtualKeys.Pause:
                        key = System.Windows.Forms.Keys.Pause;
                        break;
                    case VirtualKeys.Capital:
                        key = System.Windows.Forms.Keys.Capital;
                        break;
                    case VirtualKeys.Escape:
                        key = System.Windows.Forms.Keys.Escape;
                        break;
                    case VirtualKeys.Space:
                        key = System.Windows.Forms.Keys.Space;
                        break;
                    case VirtualKeys.Prior:
                        key = System.Windows.Forms.Keys.Prior;
                        break;
                    case VirtualKeys.Next:
                        key = System.Windows.Forms.Keys.Next;
                        break;
                    case VirtualKeys.End:
                        key = System.Windows.Forms.Keys.End;
                        break;
                    case VirtualKeys.Home:
                        key = System.Windows.Forms.Keys.Home;
                        break;
                    case VirtualKeys.Left:
                        key = System.Windows.Forms.Keys.Left;
                        break;
                    case VirtualKeys.Up:
                        key = System.Windows.Forms.Keys.Up;
                        break;
                    case VirtualKeys.Right:
                        key = System.Windows.Forms.Keys.Right;
                        break;
                    case VirtualKeys.Down:
                        key = System.Windows.Forms.Keys.Down;
                        break;
                    case VirtualKeys.Select:
                        key = System.Windows.Forms.Keys.Select;
                        break;
                    case VirtualKeys.Print:
                        key = System.Windows.Forms.Keys.Print;
                        break;
                    case VirtualKeys.Execute:
                        key = System.Windows.Forms.Keys.Execute;
                        break;
                    case VirtualKeys.Snapshot:
                        key = System.Windows.Forms.Keys.Snapshot;
                        break;
                    case VirtualKeys.Insert:
                        key = System.Windows.Forms.Keys.Insert;
                        break;
                    case VirtualKeys.Delete:
                        key = System.Windows.Forms.Keys.Delete;
                        break;
                    case VirtualKeys.Help:
                        key = System.Windows.Forms.Keys.Help;
                        break;
                    case VirtualKeys.D0:
                        key = System.Windows.Forms.Keys.D0;
                        break;
                    case VirtualKeys.D1:
                        key = System.Windows.Forms.Keys.D1;
                        break;
                    case VirtualKeys.D2:
                        key = System.Windows.Forms.Keys.D2;
                        break;
                    case VirtualKeys.D3:
                        key = System.Windows.Forms.Keys.D3;
                        break;
                    case VirtualKeys.D4:
                        key = System.Windows.Forms.Keys.D4;
                        break;
                    case VirtualKeys.D5:
                        key = System.Windows.Forms.Keys.D5;
                        break;
                    case VirtualKeys.D6:
                        key = System.Windows.Forms.Keys.D6;
                        break;
                    case VirtualKeys.D7:
                        key = System.Windows.Forms.Keys.D7;
                        break;
                    case VirtualKeys.D8:
                        key = System.Windows.Forms.Keys.D8;
                        break;
                    case VirtualKeys.D9:
                        key = System.Windows.Forms.Keys.D9;
                        break;
                    case VirtualKeys.A:
                        key = System.Windows.Forms.Keys.A;
                        break;
                    case VirtualKeys.B:
                        key = System.Windows.Forms.Keys.B;
                        break;
                    case VirtualKeys.C:
                        key = System.Windows.Forms.Keys.C;
                        break;
                    case VirtualKeys.D:
                        key = System.Windows.Forms.Keys.D;
                        break;
                    case VirtualKeys.E:
                        key = System.Windows.Forms.Keys.E;
                        break;
                    case VirtualKeys.F:
                        key = System.Windows.Forms.Keys.F;
                        break;
                    case VirtualKeys.G:
                        key = System.Windows.Forms.Keys.G;
                        break;
                    case VirtualKeys.H:
                        key = System.Windows.Forms.Keys.H;
                        break;
                    case VirtualKeys.I:
                        key = System.Windows.Forms.Keys.I;
                        break;
                    case VirtualKeys.J:
                        key = System.Windows.Forms.Keys.J;
                        break;
                    case VirtualKeys.K:
                        key = System.Windows.Forms.Keys.K;
                        break;
                    case VirtualKeys.L:
                        key = System.Windows.Forms.Keys.L;
                        break;
                    case VirtualKeys.M:
                        key = System.Windows.Forms.Keys.M;
                        break;
                    case VirtualKeys.N:
                        key = System.Windows.Forms.Keys.N;
                        break;
                    case VirtualKeys.O:
                        key = System.Windows.Forms.Keys.O;
                        break;
                    case VirtualKeys.P:
                        key = System.Windows.Forms.Keys.P;
                        break;
                    case VirtualKeys.Q:
                        key = System.Windows.Forms.Keys.Q;
                        break;
                    case VirtualKeys.R:
                        key = System.Windows.Forms.Keys.R;
                        break;
                    case VirtualKeys.S:
                        key = System.Windows.Forms.Keys.S;
                        break;
                    case VirtualKeys.T:
                        key = System.Windows.Forms.Keys.T;
                        break;
                    case VirtualKeys.U:
                        key = System.Windows.Forms.Keys.U;
                        break;
                    case VirtualKeys.V:
                        key = System.Windows.Forms.Keys.V;
                        break;
                    case VirtualKeys.W:
                        key = System.Windows.Forms.Keys.W;
                        break;
                    case VirtualKeys.X:
                        key = System.Windows.Forms.Keys.X;
                        break;
                    case VirtualKeys.Y:
                        key = System.Windows.Forms.Keys.Y;
                        break;
                    case VirtualKeys.Z:
                        key = System.Windows.Forms.Keys.Z;
                        break;
                    case VirtualKeys.LWindows:
                        key = System.Windows.Forms.Keys.LWin;
                        break;
                    case VirtualKeys.RWindows:
                        key = System.Windows.Forms.Keys.RWin;
                        break;
                    case VirtualKeys.Apps:
                        key = System.Windows.Forms.Keys.Apps;
                        break;
                    case VirtualKeys.NumPad0:
                        key = System.Windows.Forms.Keys.NumPad0;
                        break;
                    case VirtualKeys.NumPad1:
                        key = System.Windows.Forms.Keys.NumPad1;
                        break;
                    case VirtualKeys.NumPad2:
                        key = System.Windows.Forms.Keys.NumPad2;
                        break;
                    case VirtualKeys.NumPad3:
                        key = System.Windows.Forms.Keys.NumPad3;
                        break;
                    case VirtualKeys.NumPad4:
                        key = System.Windows.Forms.Keys.NumPad4;
                        break;
                    case VirtualKeys.NumPad5:
                        key = System.Windows.Forms.Keys.NumPad5;
                        break;
                    case VirtualKeys.NumPad6:
                        key = System.Windows.Forms.Keys.NumPad6;
                        break;
                    case VirtualKeys.NumPad7:
                        key = System.Windows.Forms.Keys.NumPad7;
                        break;
                    case VirtualKeys.NumPad8:
                        key = System.Windows.Forms.Keys.NumPad8;
                        break;
                    case VirtualKeys.NumPad9:
                        key = System.Windows.Forms.Keys.NumPad9;
                        break;
                    case VirtualKeys.Multiply:
                        key = System.Windows.Forms.Keys.Multiply;
                        break;
                    case VirtualKeys.Add:
                        key = System.Windows.Forms.Keys.Add;
                        break;
                    case VirtualKeys.Separator:
                        key = System.Windows.Forms.Keys.Separator;
                        break;
                    case VirtualKeys.Subtract:
                        key = System.Windows.Forms.Keys.Subtract;
                        break;
                    case VirtualKeys.Decimal:
                        key = System.Windows.Forms.Keys.Decimal;
                        break;
                    case VirtualKeys.Divide:
                        key = System.Windows.Forms.Keys.Divide;
                        break;
                    case VirtualKeys.F1:
                        key = System.Windows.Forms.Keys.F1;
                        break;
                    case VirtualKeys.F2:
                        key = System.Windows.Forms.Keys.F2;
                        break;
                    case VirtualKeys.F3:
                        key = System.Windows.Forms.Keys.F3;
                        break;
                    case VirtualKeys.F4:
                        key = System.Windows.Forms.Keys.F4;
                        break;
                    case VirtualKeys.F5:
                        key = System.Windows.Forms.Keys.F5;
                        break;
                    case VirtualKeys.F6:
                        key = System.Windows.Forms.Keys.F6;
                        break;
                    case VirtualKeys.F7:
                        key = System.Windows.Forms.Keys.F7;
                        break;
                    case VirtualKeys.F8:
                        key = System.Windows.Forms.Keys.F8;
                        break;
                    case VirtualKeys.F9:
                        key = System.Windows.Forms.Keys.F9;
                        break;
                    case VirtualKeys.F10:
                        key = System.Windows.Forms.Keys.F10;
                        break;
                    case VirtualKeys.F11:
                        key = System.Windows.Forms.Keys.F11;
                        break;
                    case VirtualKeys.F12:
                        key = System.Windows.Forms.Keys.F12;
                        break;
                    case VirtualKeys.F13:
                        key = System.Windows.Forms.Keys.F13;
                        break;
                    case VirtualKeys.F14:
                        key = System.Windows.Forms.Keys.F14;
                        break;
                    case VirtualKeys.F15:
                        key = System.Windows.Forms.Keys.F15;
                        break;
                    case VirtualKeys.F16:
                        key = System.Windows.Forms.Keys.F16;
                        break;
                    case VirtualKeys.F17:
                        key = System.Windows.Forms.Keys.F17;
                        break;
                    case VirtualKeys.F18:
                        key = System.Windows.Forms.Keys.F18;
                        break;
                    case VirtualKeys.F19:
                        key = System.Windows.Forms.Keys.F19;
                        break;
                    case VirtualKeys.F20:
                        key = System.Windows.Forms.Keys.F20;
                        break;
                    case VirtualKeys.F21:
                        key = System.Windows.Forms.Keys.F21;
                        break;
                    case VirtualKeys.F22:
                        key = System.Windows.Forms.Keys.F22;
                        break;
                    case VirtualKeys.F23:
                        key = System.Windows.Forms.Keys.F23;
                        break;
                    case VirtualKeys.F24:
                        key = System.Windows.Forms.Keys.F24;
                        break;
                    case VirtualKeys.NumLock:
                        key = System.Windows.Forms.Keys.NumLock;
                        break;
                    case VirtualKeys.Scroll:
                        key = System.Windows.Forms.Keys.Scroll;
                        break;
                }

                return key;
            }
		}

		public class MouseLLHook : Hook
		{
			// Values retreived with RegisterWindowMessage
			private int _MsgID_MouseLL;
			private int _MsgID_MouseLL_HookReplaced;

			public event HookReplacedEventHandler HookReplaced;
			public event BasicHookEventHandler MouseLLEvent;
			public event MouseEventHandler MouseDown;
			public event MouseEventHandler MouseMove;
			public event MouseEventHandler MouseUp;

			private const int WM_MOUSEMOVE                    = 0x0200;
			private const int WM_LBUTTONDOWN                  = 0x0201;
			private const int WM_LBUTTONUP                    = 0x0202;
			private const int WM_LBUTTONDBLCLK                = 0x0203;
			private const int WM_RBUTTONDOWN                  = 0x0204;
			private const int WM_RBUTTONUP                    = 0x0205;
			private const int WM_RBUTTONDBLCLK                = 0x0206;
			private const int WM_MBUTTONDOWN                  = 0x0207;
			private const int WM_MBUTTONUP                    = 0x0208;
			private const int WM_MBUTTONDBLCLK                = 0x0209;
			private const int WM_MOUSEWHEEL                   = 0x020A;

			struct MSLLHOOKSTRUCT 
			{
				public System.Drawing.Point pt;
				public int mouseData;
				public int flags;
				public int time;
				public IntPtr dwExtraInfo;
			};

			public MouseLLHook(IntPtr Handle) : base(Handle)
			{
			}

			protected override void OnStart()
			{
				// Retreive the message IDs that we'll look for in WndProc
				_MsgID_MouseLL = RegisterWindowMessage("WILSON_HOOK_MOUSELL");
				_MsgID_MouseLL_HookReplaced = RegisterWindowMessage("WILSON_HOOK_MOUSELL_REPLACED");

				// Start the hook
				InitializeMouseLLHook(0, _Handle);
			}

			protected override void OnStop()
			{
				UninitializeMouseLLHook();
			}

			public override void ProcessWindowMessage(ref System.Windows.Forms.Message m)

			{
				if (m.Msg == _MsgID_MouseLL)
				{
					if (MouseLLEvent != null)
						MouseLLEvent(m.WParam, m.LParam);

					MSLLHOOKSTRUCT M = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(m.LParam, typeof(MSLLHOOKSTRUCT));

					if (m.WParam.ToInt32() == WM_MOUSEMOVE)
					{
						if (MouseMove != null)
							MouseMove(this, new MouseEventArgs(MouseButtons.None, 0, M.pt.X, M.pt.Y, M.time));
					}
					else if (m.WParam.ToInt32() == WM_LBUTTONDOWN)
					{
						if (MouseDown != null)
                            MouseDown(this, new MouseEventArgs(MouseButtons.Left, m.HWnd.ToInt32(), M.pt.X, M.pt.Y, M.time));
					}
					else if (m.WParam.ToInt32() == WM_RBUTTONDOWN)
					{
						if (MouseDown != null)
							MouseDown(this, new MouseEventArgs(MouseButtons.Right, m.HWnd.ToInt32(), M.pt.X, M.pt.Y, M.time));
					}
					else if (m.WParam.ToInt32() == WM_LBUTTONUP)
					{
						if (MouseUp != null)
                            MouseUp(this, new MouseEventArgs(MouseButtons.Left, m.HWnd.ToInt32(), M.pt.X, M.pt.Y, M.time));
					}
					else if (m.WParam.ToInt32() == WM_RBUTTONUP)
					{
						if (MouseUp != null)
                            MouseUp(this, new MouseEventArgs(MouseButtons.Right, m.HWnd.ToInt32(), M.pt.X, M.pt.Y, M.time));
					}
				}
				else if (m.Msg == _MsgID_MouseLL_HookReplaced)
				{
					if (HookReplaced != null)
						HookReplaced();
				}
			}
		}
		
        public class CallWndProcHook : Hook
		{
			// Values retreived with RegisterWindowMessage
			private int _MsgID_CallWndProc;
			private int _MsgID_CallWndProc_Params;
			private int _MsgID_CallWndProc_HookReplaced;

			public event HookReplacedEventHandler HookReplaced;
			public event WndProcEventHandler CallWndProc;

			private IntPtr _CacheHandle;
			private IntPtr _CacheMessage;

			public CallWndProcHook(IntPtr Handle) : base(Handle)
			{
			}

			protected override void OnStart()
			{
				// Retreive the message IDs that we'll look for in WndProc
				_MsgID_CallWndProc_HookReplaced = RegisterWindowMessage("WILSON_HOOK_CALLWNDPROC_REPLACED");
				_MsgID_CallWndProc = RegisterWindowMessage("WILSON_HOOK_CALLWNDPROC");
				_MsgID_CallWndProc_Params = RegisterWindowMessage("WILSON_HOOK_CALLWNDPROC_PARAMS");

				// Start the hook
				InitializeCallWndProcHook(0, _Handle);
			}

			protected override void OnStop()
			{
				UninitializeCallWndProcHook();
			}

			public override void ProcessWindowMessage(ref System.Windows.Forms.Message m)
			{
				if (m.Msg == _MsgID_CallWndProc)
				{
					_CacheHandle = m.WParam;
					_CacheMessage = m.LParam;
				}
				else if (m.Msg == _MsgID_CallWndProc_Params)
				{
					if (CallWndProc != null && _CacheHandle != IntPtr.Zero && _CacheMessage != IntPtr.Zero)
						CallWndProc(_CacheHandle, _CacheMessage, m.WParam, m.LParam);
					_CacheHandle = IntPtr.Zero;
					_CacheMessage = IntPtr.Zero;
				}
				else if (m.Msg == _MsgID_CallWndProc_HookReplaced)
				{
					if (HookReplaced != null)
						HookReplaced();
				}
			}
		}
		
        public class GetMsgHook : Hook
		{
			// Values retreived with RegisterWindowMessage
			private int _MsgID_GetMsg;
			private int _MsgID_GetMsg_Params;
			private int _MsgID_GetMsg_HookReplaced;

			public event HookReplacedEventHandler HookReplaced;
			public event WndProcEventHandler GetMsg;

			private IntPtr _CacheHandle;
			private IntPtr _CacheMessage;

			public GetMsgHook(IntPtr Handle) : base(Handle)
			{
			}

			protected override void OnStart()
			{
				// Retreive the message IDs that we'll look for in WndProc
				_MsgID_GetMsg_HookReplaced = RegisterWindowMessage("WILSON_HOOK_GETMSG_REPLACED");
				_MsgID_GetMsg = RegisterWindowMessage("WILSON_HOOK_GETMSG");
				_MsgID_GetMsg_Params = RegisterWindowMessage("WILSON_HOOK_GETMSG_PARAMS");

				// Start the hook
				InitializeGetMsgHook(0, _Handle);
			}

			protected override void OnStop()
			{
				UninitializeGetMsgHook();
			}

			public override void ProcessWindowMessage(ref System.Windows.Forms.Message m)
			{
				if (m.Msg == _MsgID_GetMsg)
				{
					_CacheHandle = m.WParam;
					_CacheMessage = m.LParam;
				}
				else if (m.Msg == _MsgID_GetMsg_Params)
				{
					if (GetMsg != null && _CacheHandle != IntPtr.Zero && _CacheMessage != IntPtr.Zero)
						GetMsg(_CacheHandle, _CacheMessage, m.WParam, m.LParam);
					_CacheHandle = IntPtr.Zero;
					_CacheMessage = IntPtr.Zero;
				}
				else if (m.Msg == _MsgID_GetMsg_HookReplaced)
				{
					if (HookReplaced != null)
						HookReplaced();
				}
			}
		}

        public class SysMsgFilterHook : Hook
        {
            private int _MsgID_SysMsgFilter_HookReplaced;
            private int _MsgID_SysMsgFilter_DialogBox;
            private int _MsgID_SysMsgFilter_Menu;
            private int _MsgID_SysMsgFilter_Scrollbar;
            private int _MsgID_SysMsgFilter_NextWindow;

            public event HookReplacedEventHandler HookReplaced;
            public event WindowEventHandler DialogBox;
            public event WindowEventHandler Menu;
            public event WindowEventHandler Scrollbar;
            public event WindowEventHandler NextWindow;

            public SysMsgFilterHook(IntPtr Handle) : base(Handle)
            {

            }

            protected override void OnStart()
            {
                _MsgID_SysMsgFilter_HookReplaced = RegisterWindowMessage("WILSON_HOOK_SYSMSGFILTER_REPLACED");
                _MsgID_SysMsgFilter_DialogBox = RegisterWindowMessage("WILSON_HOOK_MSGF_DIALOGBOX");
                _MsgID_SysMsgFilter_Menu = RegisterWindowMessage("WILSON_HOOK_MSGF_MENU");
                _MsgID_SysMsgFilter_Scrollbar = RegisterWindowMessage("WILSON_HOOK_MSGF_SCROLLBAR");
                _MsgID_SysMsgFilter_NextWindow = RegisterWindowMessage("WILSON_HOOK_MSGF_NEXTWINDOW");

                InitializeSysMsgFilterHook(0, _Handle);
            }

            protected override void OnStop()
            {
                UninitializeSysMsgFilterHook();
            }

            public override void ProcessWindowMessage(ref System.Windows.Forms.Message m)
            {
                if (m.Msg == _MsgID_SysMsgFilter_HookReplaced)
                {
                    if (HookReplaced != null)
                        HookReplaced();
                }
                else if (m.Msg == _MsgID_SysMsgFilter_DialogBox)
                {
                    if (DialogBox!=null)
                    {
                        DialogBox(m.WParam);
                    }
                }
                else if (m.Msg == _MsgID_SysMsgFilter_Menu)
                {
                    if (Menu!=null)
                    {
                        Menu(m.WParam);
                    }
                }
                else if (m.Msg == _MsgID_SysMsgFilter_Scrollbar)
                {
                    if (Scrollbar!=null)
                    {
                        Scrollbar(m.WParam);
                    }
                }
                else if (m.Msg == _MsgID_SysMsgFilter_NextWindow)
                {
                    if (NextWindow!=null)
                    {
                        NextWindow(m.WParam);
                    }
                }
            }
        }
	}
}
