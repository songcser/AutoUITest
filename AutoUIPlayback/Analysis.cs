using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Windows;
using System.Diagnostics;
using System.Windows.Forms;

namespace AutoUIPlayback
{
   
    class Analysis
    {
        public struct POINTAPI
        {
            public uint x;
            public uint y;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }
        public struct My_lParam
        {
            public int i;
            public string s;
        }
        private static Process process;
        private List<int> pid;

        [DllImport("user32.dll")]
        extern static bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        extern static void mouse_event(int mouseEventFlag, int incrementX, int intincrementY, uint data, UIntPtr extraInfo);
        [DllImport("user32.dll")]
        static extern void keybd_event(uint bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        [DllImport("user32.dll")]
        static extern byte MapVirtualKey(uint uCode, uint uMap);
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern IntPtr ChildWindowFromPoint(IntPtr parent, POINTAPI p);
        [DllImport("user32.dll")]
        public static extern uint ScreenToClient(IntPtr hwnd,ref POINTAPI p );
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd,int Msg,int wParam,int lParam);
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SendMessage(IntPtr hwnd,int msg,IntPtr wParam,String lParam);
        #region MyRegion
        //[DllImport("User32.dll", EntryPoint = "SendMessage")]
        //public static extern int SendMessage(
        //    IntPtr hWnd,        // 信息发往的窗口的句柄
        //    int Msg,            // 消息ID
        //    int wParam,         // 参数1
        //    ref  COPYDATASTRUCT lParam  //参数2
        //);
        //[DllImport("User32.dll", EntryPoint = "PostMessage")]
        //public static extern int PostMessage(
        //    IntPtr hWnd,        // 信息发往的窗口的句柄
        //    int Msg,            // 消息ID
        //    int wParam,         // 参数1
        //    int lParam            // 参数2
        //);
        //[DllImport("User32.dll", EntryPoint = "PostMessage")]
        //public static extern int PostMessage(
        //    IntPtr hWnd,        // 信息发往的窗口的句柄
        //    int Msg,            // 消息ID
        //    int wParam,         // 参数1
        //    ref My_lParam lParam //参数2
        //); 
        #endregion
        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref int lpdwProcessId);
        [DllImport("user32.dll", EntryPoint = "GetDoubleClickTime")]
        public static extern int GetDoubleClickTime();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumWindows(EnumThreadWindowsCallback callback, IntPtr extraData);
        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern bool EnumChildWindows(HandleRef hwndParent, EnumChildrenCallback lpEnumFunc, HandleRef lParam);
        private delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);
        private delegate bool EnumChildrenCallback(IntPtr hwnd, IntPtr lParam);

        const int MOUSEEVENTF_MOVE = 0x0001;

        const int MOUSEEVENTF_LEFTDOWN = 0x0002;

        const int MOUSEEVENTF_LEFTUP = 0x0004;

        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;

        const int MOUSEEVENTF_RIGHTUP = 0x0010;

        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;

        const int MOUSEEVENTF_MIDDLEUP = 0x0040;

        const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        const int LB_ADDSTRING = 0x0180;

        #region MyRegion
        ////按下鼠标左键
        //public static int WM_LBUTTONDOWN = 0x201;
        ////释放鼠标左键
        //public static int WM_LBUTTONUP = 0x202;
        ////双击鼠标左键
        //public static int WM_LBUTTONDBLCLK = 0x203;
        ////按下鼠标右键
        //public static int WM_RBUTTONDOWN = 0x204;
        ////释放鼠标右键
        //public static int WM_RBUTTONUP = 0x205;
        ////双击鼠标右键
        //public static int WM_RBUTTONDBLCLK = 0x206; 
        #endregion

        private static AutomationElement mainWindow = null;
        private static IntPtr mainHandle = IntPtr.Zero;
        private static bool isClear = false;
 //       private static AutomationElement valueElement = null;
        private static List<IntPtr> windowHandle;
        private static CacheRequest cacheRequest = null;
        AutomationElementCollection elementCollection;
        List<AutomationElementCollection> collectionList;

        private static bool SyncFlag = true;
        private int dcTime = 0;
        private string tname;
        private string ttype;
        private bool tenable;
        private string tautoId;
        private string tpname;
        private string tptype;
        private AutomationElement targetElement;
        private bool anyFlag = false;
        //public AutomationElement MainWindow
        //{
        //    get { return mainWindow; }
        //    set { mainWindow = value; }
        //}
        public Analysis()
        {
            pid = new List<int>();
            windowHandle = new List<IntPtr>();

            cacheRequest = new CacheRequest();
            cacheRequest.Add(AutomationElement.NameProperty);
            cacheRequest.Add(AutomationElement.IsEnabledProperty);
            cacheRequest.Add(AutomationElement.ControlTypeProperty);
            cacheRequest.Add(AutomationElement.AutomationIdProperty);
            cacheRequest.Add(AutomationElement.IsControlElementProperty);
            cacheRequest.Add(AutomationElement.LocalizedControlTypeProperty);
//             cacheRequest.Add(SelectionItemPattern.Pattern);
//             cacheRequest.Add(InvokePattern.Pattern);
//             cacheRequest.Add(ExpandCollapsePattern.Pattern);
//             cacheRequest.Add(ValuePattern.Pattern);
//             cacheRequest.Add(TogglePattern.Pattern);

            collectionList = new List<AutomationElementCollection>();
            //if (dcTime>300)
            //{
            //    dcTime = GetDoubleClickTime() - 300;
            //}
            //else
            //{
            //    dcTime = GetDoubleClickTime();
            //}
            dcTime = 50;
        }

        public bool StartAnalysis(string s)
        {
            string[] str = GetString(s);
            if (str==null)
            {
                return false;
            }
            List<string> strList = new List<string>(str.ToArray());
            string temp = "";
            for (int i = 0; i < strList.Count; i++)
            {
                if (!strList[i].Equals("") && !strList[i].Equals(" "))
                {
                    temp += strList[i] + "---";

                }
            }
             Console.WriteLine(temp);
            if ("Start " == strList[0])
            {
                Start(strList[1]);
            }
            else if ("Click " == strList[0])
            {
                //Thread.Sleep(500);
                #region MyRegion
                if (strList.Count == 8)
                {
                    ClickLeftMouse(strList[1], strList[2],bool.Parse(strList[3]), strList[4], Int32.Parse(strList[5]), Int32.Parse(strList[6]));
                }
                else if (strList.Count == 9)
                {
                    ClickLeftMouse(strList[1], strList[2], bool.Parse(strList[3]), strList[4], strList[5], Int32.Parse(strList[6]), Int32.Parse(strList[7]));
                }
                else
                {
                    return false;
                } 
                #endregion
                
            }
            else if ("RightClick " == strList[0])
            {
                //Thread.Sleep(500);
                #region MyRegion
                if (strList.Count == 8)
                {
                    ClickRightMouse(strList[1], strList[2], bool.Parse(strList[3]), strList[4], Int32.Parse(strList[5]), Int32.Parse(strList[6]));
                }
                else if (strList.Count == 9)
                {
                    ClickRightMouse(strList[1], strList[2], bool.Parse(strList[3]), strList[4], strList[5], Int32.Parse(strList[6]), Int32.Parse(strList[7]));
                }
                else
                {
                    return false;
                } 
                #endregion
            }
            else if ("DoubleClick " == strList[0])
            {
                //Thread.Sleep(500);
                #region MyRegion
                if (strList.Count == 8)
                {
                    DoubleMouseDown(strList[1], strList[2], bool.Parse(strList[3]), strList[4], Int32.Parse(strList[5]), Int32.Parse(strList[6]));
                }
                else if (strList.Count == 9)
                {
                    DoubleMouseDown(strList[1], strList[2], bool.Parse(strList[3]), strList[4], strList[5], Int32.Parse(strList[6]), Int32.Parse(strList[7]));
                }
                else
                {
                    return false;
                }
                
                #endregion
            }
            else if ("Stop" == strList[0])
            {
                //Stop();
                return false;
            }
            else if ("SetFocus "==strList[0])
            {
                if (strList.Count == 8)
                {
                    SetFocus(strList[1], strList[2], bool.Parse(strList[3]), strList[4]);
                }
                else if (strList.Count == 9)
                {
                    SetFocus(strList[1], strList[2], bool.Parse(strList[3]), strList[4], strList[5]);
                }
            }
            else if ("SendKey " == strList[0])
            {
                Thread.Sleep(10);
                SendKey(strList[1]);
            }
            else if ("KeyDown " == strList[0])
            {
                Thread.Sleep(10);
                KeyDown(strList[1]);
            }
            else if ("KeyUp " == strList[0])
            {
                Thread.Sleep(10);
                KeyUp(strList[1]);
            }
            else if ("OpenMenu " == strList[0])
            {
                #region MyRegion

                ClickLeftMouse(strList[1], strList[2],true, strList[3], 0, 0);
               
                #endregion
                //ClickLeftMouse(strList[1], strList[2], strList[3]);
            }
            else if ("WindowCreate " == strList[0])
            {
                WaitWindow(strList[1]);
            }
            else if ("Activate " == strList[0])
            {
                if (strList.Count == 9)
                {
                    ActivateWindow(strList[1], strList[2], bool.Parse(strList[3]), strList[4], strList[5]);
                }
                else
                {
                    ActivateWindow(strList[1], strList[2]);
                }
                
            }
            else if ("MouseDown " == strList[0])
            {
                #region MyRegion
                
                if (strList.Count == 8)
                {
                    ClickLeftMouse(strList[1], strList[2], bool.Parse(strList[3]), strList[4], Int32.Parse(strList[5]), Int32.Parse(strList[6]));
                }
                else if (strList.Count == 9)
                {
                    ClickLeftMouse(strList[1], strList[2], bool.Parse(strList[3]), strList[4], strList[5], Int32.Parse(strList[6]), Int32.Parse(strList[7]));
                }
                else
                {
                    return false;
                } 
                #endregion
            }
            else if ("MouseUp" == strList[0])
            {
                isClear = false;

            }
            else if ("Wait " == strList[0])
            {
                if (strList.Count == 9)
                {
                    WaitControl(strList[1], strList[2], bool.Parse(strList[3]), strList[4], strList[5]);
                }
                else if (strList.Count == 10)
                {
                    WaitControl(strList[1], strList[2], bool.Parse(strList[3]), strList[4], strList[5], strList[7]);
                }
                else
                {
                    return false;
                }
            }
            else if ("Move " == strList[0])
            {
                if (strList.Count == 10)
                {
                    Move(strList[1], strList[2], bool.Parse(strList[3]), strList[4], Int32.Parse(strList[5]), Int32.Parse(strList[6]), Int32.Parse(strList[7]), Int32.Parse(strList[8]));
                }
                else if (strList.Count == 11)
                {
                    Move(strList[1], strList[2], bool.Parse(strList[3]), strList[4], strList[5], Int32.Parse(strList[6]), Int32.Parse(strList[7]), Int32.Parse(strList[8]), Int32.Parse(strList[9]));
                }
                else
                {
                    return false;
                }
            }
            else if ("SetValue " == strList[0])
            {
                if (strList.Count == 8)
                {
                    SetValue(strList[1], strList[2], bool.Parse(strList[3]), strList[4], strList[7]);
                }
                else if (strList.Count == 9)
                {
                    SetValue(strList[1], strList[2], bool.Parse(strList[3]), strList[4], strList[5], strList[8]);
                }
            }
            return true;
        }

        public void UpdateCache()
        {
            Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
            int i = 0;
            for (; i < collectionList.Count;i++ )
            {
                if (collectionList[i]==elementCollection)
                {
                    break;
                }
            }

            using (cacheRequest.Activate())
            {
                elementCollection = mainWindow.FindAll(TreeScope.Subtree, new AndCondition(condition1, condition2));
            }

            collectionList[i] = elementCollection;
        }

        private void StartCache(IntPtr handle)
        {
            bool flag = true;
            for (int i = 0; i < windowHandle.Count; i++)
            {
                if (windowHandle[i] == handle)
                {
                    elementCollection = null;
                    elementCollection = collectionList[i];
                    flag = false;
                }
            }
            if (flag)
            {

                Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
                Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
                //Condition condition3 = new NotCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Image));
                SyncFlag = false;
                Thread thread = new Thread(() =>
                {
                    using (cacheRequest.Activate())
                    {
                        try
                        {
                            elementCollection = mainWindow.FindAll(TreeScope.Descendants, new AndCondition(condition1, condition2));
                        }
                        catch (System.Exception ex)
                        {

                        }

                    }
                    windowHandle.Add(handle);
                    collectionList.Add(elementCollection);
                    SyncFlag = true;
                });
                thread.Start();
                //thread.Join();
            }
        }
        
        public void ActivateWindow(string name, string type)
        {
            //Thread.Sleep(200);
            
            IntPtr handle = FindWindow(null, name);
            int count = 0;
            while (handle == IntPtr.Zero || handle == null)
            {
                handle = FindWindow(null, name);
                Thread.Sleep(50);
                if (count>1200*2)
                {
                    return;
                }
                count++;
            }

            mainHandle = handle;
            mainWindow = AutomationElement.FromHandle(handle);

            StartCache(handle);
            #region MyRegion
            //Condition cond1 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
            //Condition cond2 = new PropertyCondition(AutomationElement.NameProperty, name);
            //Condition cond3 = new PropertyCondition(AutomationElement.LocalizedControlTypeProperty,type);
            ////int count = 0;
            ////while (count<10)
            ////{
            ////    AutomationElement targetElem = AutomationElement.RootElement.FindFirst(TreeScope.Subtree, new AndCondition(cond1, cond2, cond3));
            ////    if (targetElem != null)
            ////    {
            ////        mainWindow = targetElem;
            ////        return;
            ////    }
            ////    Thread.Sleep(200);
            ////    count++;
            ////}
            ////if (name == "Material Library Manger")
            ////{
            ////    AutomationElement elent = AutomationElement.FocusedElement;
            ////    string tmp = elent.Current.Name;
            ////}
            //Thread.Sleep(200);

            //AutomationElement focusElement = AutomationElement.FocusedElement;

            //AutomationElement node = focusElement;
            //TreeWalker walker = TreeWalker.ControlViewWalker;
            //AutomationElement elementParent;

            //if (focusElement == AutomationElement.RootElement) return ;

            ////AutomationElement elementParent = focusElement.FindFirst(TreeScope.Ancestors,new AndCondition(cond1,cond2,cond3));
            ////if (elementParent != null)
            ////{
            ////    mainWindow = elementParent;
            ////}
            //do
            //{
            //    elementParent = walker.GetParent(node);
            //    object controlTypeNoDefault = elementParent.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty, true);
            //    if (controlTypeNoDefault == AutomationElement.NotSupported)
            //    {
            //        return;
            //    }
            //    ControlType controlType = controlTypeNoDefault as ControlType;

            //    if (type == controlType.LocalizedControlType&&elementParent.Current.Name==name|| elementParent == AutomationElement.RootElement)
            //    {
            //        mainWindow = elementParent;
            //        //if (name == "Material Library Manger")
            //        //{
            //        //    string tmp = mainWindow.Current.Name;
            //        //}
            //        break;
            //    }

            //    node = elementParent;
            //}
            //while (true);
            
            #endregion
            
        }

        public void ActivateWindow(string name, string type,bool flag, string pname, string ptype)
        {
            if (name == "")
            {
                AutomationElement element = FindElement(name, type,flag, pname, ptype);
                if (element != null)
                {
                    mainWindow = element;
                   
                }
                return;
            }
            ActivateWindow(name, type);
        }

        public void WaitWindow(string name)
        {
#region MyRegion
		//             Condition cond1 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
//             Condition cond2 = new PropertyCondition(AutomationElement.NameProperty, name);
//             Condition cond3 = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window);
// 
//             
//             while (true)
//             {
//                 AutomationElement targetElem = AutomationElement.RootElement.FindFirst(TreeScope.Subtree, new AndCondition(cond1,cond2,cond3));
//                 if (targetElem!=null)
//                 {
//                     mainWindow = targetElem;
//                     break;
//                 }
//             }
 
	#endregion
            IntPtr handle = FindWindow(null, name);

            while (handle == IntPtr.Zero || handle == null)
            {
                handle = FindWindow(null, name);
                Thread.Sleep(50);
            }

            int processid = 0;
            GetWindowThreadProcessId(handle, ref processid);
            if (processid != process.Id)
            {
                bool pflag = true;
                for (int i = 0; i < pid.Count;i++ )
                {
                    if (pid[i] == processid)
                    {
                        pflag = false;
                    }
                }
                if (pflag)
                {
                    pid.Add(processid);
                }
            }

            mainHandle = handle;
            mainWindow = AutomationElement.FromHandle(handle);

            StartCache(handle);
        }

        public void WaitControl(string name, string type, bool flag, string autoId,string message)
        {
           
            while (true)
            {
                try
                {
                    bool enable = false;
                    if (message == "")
                    {
                        AutomationElement element = FindCurrentElement(name, type, autoId, enable);

                        if (element != null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        AutomationElement element = FindElement(name, type,flag, autoId);
                        ControlType t = element.Current.ControlType;
                        if (t == ControlType.Edit)
                        {
                            ValuePattern currentPattern = GetValuePattern(element);
                            string value = currentPattern.Current.Value;
                            if (value == message)
                            {
                                break;
                            }
                        }
                    }
                    
                }
                catch (System.Exception ex)
                {
                	
                }
                
            }
        }

        public void WaitControl(string name, string type, bool flag, string pname, string ptype,string message)
        {

            while (true)
            {
                try
                {
                    bool enable = false;
                    if (message == "")
                    {
                        AutomationElement element = FindCurrentElement(name, type,flag, pname,ptype);

                        if (element != null)
                        {
                            break;
                        }
                    }
                    else
                    {
                        AutomationElement element = FindElement(name, type, flag, pname,ptype);
                        ControlType t = element.Current.ControlType;
                        if (t == ControlType.Edit)
                        {
                            ValuePattern currentPattern = GetValuePattern(element);
                            string value = currentPattern.Current.Value;
                            if (value == message)
                            {
                                break;
                            }
                        }
                    }

                }
                catch (System.Exception ex)
                {

                }
            }
        }

        private void LeftMouseDown(string name, string type, bool flag, string autoId, int offsetX, int offsetY)
        {
            AutomationElement element = FindElement(name, type,flag, autoId);
            if (element == null)
            {
                return;
                throw new NullReferenceException(string.Format("Element with AutomationId '{0}' and Name '{1}' can not be find.",

                    element.Current.AutomationId, element.Current.Name));
            }
            if (type=="edit")
            {
                if (isClear)
                {
                    element.SetFocus();
                    ValuePattern currentPattern = GetValuePattern(element);
                    //currentPattern.SetValue("");
                }
            }
            Rect rect = element.Current.BoundingRectangle;

            int IncrementX = (int)(rect.Left + offsetX);

            int IncrementY = (int)(rect.Top + offsetY);
            if (offsetX == 0 && offsetY == 0)
            {
                Point p = element.GetClickablePoint();
                IncrementX = (int)p.X;
                IncrementY = (int)p.Y;
            }
            //Point p = element.GetClickablePoint();
            //Point p = new Point(IncrementX, IncrementY);
            SetCursorPos(IncrementX, IncrementY);

            mouse_event(MOUSEEVENTF_LEFTDOWN, IncrementX, IncrementX, 0, UIntPtr.Zero);

            //mouse_event(MOUSEEVENTF_RIGHTUP, (int)p.X, (int)p.Y, 0, UIntPtr.Zero);
            
        }

        private void LeftMouseDown(string name, string type,bool flag, string pname, string ptype, int offsetX, int offsetY)
        {
            AutomationElement element = FindElement(name, type,flag, pname,ptype);
            if (element == null)
            {
                throw new NullReferenceException(string.Format("Element with AutomationId '{0}' and Name '{1}' can not be find.",

                    element.Current.AutomationId, element.Current.Name));
            }
            
            Rect rect = element.Current.BoundingRectangle;

            int IncrementX = (int)(rect.Left + offsetX);

            int IncrementY = (int)(rect.Top + offsetY);
            if (offsetX == 0 && offsetY == 0)
            {
                Point p = element.GetClickablePoint();
                IncrementX = (int)p.X;
                IncrementY = (int)p.Y;
            }
            //Point p = element.GetClickablePoint();
            //Point p = new Point(IncrementX, IncrementY);
            SetCursorPos(IncrementX, IncrementY);

            mouse_event(MOUSEEVENTF_LEFTDOWN, IncrementX, IncrementX, 0, UIntPtr.Zero);

            //mouse_event(MOUSEEVENTF_LEFTDOWN, (int)p.X, (int)p.Y, 0, UIntPtr.Zero);

            //mouse_event(MOUSEEVENTF_RIGHTUP, (int)p.X, (int)p.Y, 0, UIntPtr.Zero);

        }

        private void LeftMouseUp(string name, string type, bool flag, string autoId, int offsetX, int offsetY)
        {
            AutomationElement element = FindElement(name, type,flag, autoId);
            if (element == null)
            {
                return;
                throw new NullReferenceException(string.Format("Element with AutomationId '{0}' and Name '{1}' can not be find.",

                    element.Current.AutomationId, element.Current.Name));
            }
            System.Windows.Rect rect = element.Current.BoundingRectangle;
            int x = (int)rect.X + offsetX;
            int y = (int)rect.Y + offsetY;
            if (offsetX == 0 && offsetY == 0)
            {
                Point point = element.GetClickablePoint();
                x = (int)point.X;
                y = (int)point.Y;
            }
            SetCursorPos(x, y);

            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, UIntPtr.Zero);

            //mouse_event(MOUSEEVENTF_LEFTDOWN, (int)p.X, (int)p.Y, 0, UIntPtr.Zero);

           // mouse_event(MOUSEEVENTF_RIGHTUP, (int)p.X, (int)p.Y, 0, UIntPtr.Zero);

        }

        private void LeftMouseUp(string name, string type, bool flag, string pname, string ptype, int offsetX, int offsetY)
        {
            AutomationElement element = FindElement(name, type,flag, pname, ptype);
            if (element == null)
            {
                return;
                throw new NullReferenceException(string.Format("Element with AutomationId '{0}' and Name '{1}' can not be find.",

                    element.Current.AutomationId, element.Current.Name));
            }
            Rect rect = element.Current.BoundingRectangle;

            int IncrementX = (int)(rect.Left + offsetX);

            int IncrementY = (int)(rect.Top + offsetY);
            if (offsetX == 0 && offsetY == 0)
            {
                Point p = element.GetClickablePoint();
                IncrementX = (int)p.X;
                IncrementY = (int)p.Y;
            }
            //Point p = element.GetClickablePoint();
            //Point p = new Point(IncrementX, IncrementY);
            SetCursorPos(IncrementX, IncrementY);

            //mouse_event(MOUSEEVENTF_LEFTDOWN, (int)p.X, (int)p.Y, 0, UIntPtr.Zero);

            mouse_event(MOUSEEVENTF_LEFTUP, IncrementX, IncrementY, 0, UIntPtr.Zero);

        }

        public void ClickLeftMouse(string name, string type, bool flag, string autoId, int offsetX, int offsetY)
        {
            AutomationElement element = FindElement(name, type, flag,autoId);
            
            if (element == null)
            {
                return; 
            }
            
            try
            {
                AnalysisType(element,offsetX,offsetY);
            }
            catch (System.Exception ex)
            {
                string str = ex.Message;
                //AutomationElement elem = FindCurrentElement(name, type, autoId,true);
                if (element != null)
                {
                    System.Windows.Rect rect = element.Current.BoundingRectangle;
                    int x = (int)rect.X + offsetX;
                    int y = (int)rect.Y + offsetY;
                    if (offsetX==0&&offsetY==0)
                    {
                        Point point = element.GetClickablePoint();
                        x = (int)point.X;
                        y = (int)point.Y;
                    }
                    SetCursorPos(x, y);

                    mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, UIntPtr.Zero);
                    //Thread.Sleep(50);
                    mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, UIntPtr.Zero);
                }
            }

            #region MyRegion
            //InvokePattern invokePattern = null;

            //try
            //{
            //    invokePattern = element.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
            //}
            //catch (ElementNotEnabledException)
            //{
            //    // Object is not enabled
            //    return;
            //}
            //catch (InvalidOperationException)
            //{
            //    // object doesn't support the InvokePattern control pattern
            //    return;
            //}

            //invokePattern.Invoke();

            //Rect rect = element.Current.BoundingRectangle;

            //int IncrementX = (int)(rect.Left + rect.Width / 2);

            //int IncrementY = (int)(rect.Top + rect.Height / 2);

            ////Make the cursor position to the element.

            //SetCursorPos(IncrementX, IncrementY);

            ////Make the left mouse down and up.

            //mouse_event(MOUSEEVENTF_LEFTDOWN, IncrementX, IncrementY, 0, UIntPtr.Zero);

            //mouse_event(MOUSEEVENTF_LEFTUP, IncrementX, IncrementY, 0, UIntPtr.Zero);
            
            #endregion

        }

        public void ClickLeftMouse(string name, string type, bool flag, string pname, string ptype, int offsetX, int offsetY)
        {
            AutomationElement element = FindElement(name, type,flag, pname,ptype);
            
            if (element == null)
            {
                return;    
            }
            
            try
            {
                AnalysisType(element,offsetX,offsetY);
                
            }
            catch (System.Exception ex)
            {
                string str = ex.Message;
                
                AutomationElement elem = FindCurrentElement(name, type,true, pname,ptype);
                if (elem != null)
                {

                    System.Windows.Rect rect = elem.Current.BoundingRectangle;
                    int x = (int)rect.X + offsetX;
                    int y = (int)rect.Y + offsetY;
                    if (offsetX == 0 && offsetY == 0)
                    {
                        Point point = elem.GetClickablePoint();
                        x = (int)point.X;
                        y = (int)point.Y;
                    }
                    SetCursorPos(x, y);

                    mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, UIntPtr.Zero);

                    mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, UIntPtr.Zero);
                }
            }
        }

        public void ClickRightMouse(string name, string type, bool flag, string autoId, int offsetX, int offsetY)
        {

            AutomationElement element = FindElement( name, type,flag,autoId);

            if (element == null)
            {

                throw new NullReferenceException(string.Format("Element with AutomationId '{0}' and Name '{1}' can not be find.",

                    element.Current.AutomationId, element.Current.Name));

            }
            Rect rect = element.Current.BoundingRectangle;

            int IncrementX = (int)(rect.Left + offsetX);

            int IncrementY = (int)(rect.Top + offsetY);
            if (offsetX==0&&offsetY==0)
            {
                Point p = element.GetClickablePoint();
                IncrementX = (int)p.X;
                IncrementY = (int)p.Y;
            }
            
            SetCursorPos(IncrementX, IncrementY);

            #region MyRegion
            //Rect rect = element.Current.BoundingRectangle;

            //int IncrementX = (int)(rect.Left + rect.Width / 2);

            //int IncrementY = (int)(rect.Top + rect.Height / 2);

            //element.SetFocus();
            //Win32Api api = new Win32Api();
            //Win32Api.MouseRightKeyDown(IncrementX, IncrementY);
            //Win32Api.MouseRightKeyUp(IncrementX, IncrementY);

            //POINTAPI point = new POINTAPI();
            //point.x = (uint)p.X;
            //point.y = (uint)p.Y;
            //ScreenToClient(mainHandle, ref point);
            //IntPtr handle = ChildWindowFromPoint(mainHandle, point);
            //             //handle = (IntPtr)element.Current.NativeWindowHandle;
            //             AutomationElement elem = AutomationElement.FromHandle(handle);
            //            //Make the cursor position to the element.

            //var value = ((int)p.X) << 16 + (int)p.Y;
            //SendMessage(mainHandle, WM_RBUTTONDOWN, 0, value);
            //SendMessage(mainHandle, WM_RBUTTONUP, 0, value);
            //Thread.Sleep(200);
            //var processId = element.GetCurrentPropertyValue(AutomationElement.ProcessIdProperty);
            //var window = AutomationElement.RootElement.FindFirst(
            //    TreeScope.Children,
            //    new PropertyCondition(AutomationElement.ProcessIdProperty,
            //                          processId));
            //var handle = window.Current.NativeWindowHandle;
            //SetCursorPos((int)p.X,(int)p.Y);
            //SetCursorPos(IncrementX, IncrementY);
            ////             SendMessage(mainHandle, MOUSEEVENTF_RIGHTDOWN, IntPtr.Zero, (IntPtr)(IncrementY * 65536 + IncrementX));
            ////             SendMessage(mainHandle, MOUSEEVENTF_RIGHTUP, IntPtr.Zero, (IntPtr)(IncrementY * 65536 + IncrementX));
            //SendMessage(mainHandle, WM_RBUTTONDOWN, IntPtr.Zero, (IntPtr)(value));
            //SendMessage(mainHandle, WM_RBUTTONUP, IntPtr.Zero, (IntPtr)(value));

            //             SendMessage(handle, WM_RBUTTONDOWN, (IntPtr)IncrementX, (IntPtr)IncrementY);
            //             SendMessage(handle, WM_RBUTTONUP, (IntPtr)IncrementX, (IntPtr)IncrementY);
            //Make the left mouse down and up. 
            #endregion

            mouse_event(MOUSEEVENTF_RIGHTDOWN, IncrementX, IncrementY, 0, UIntPtr.Zero);

            mouse_event(MOUSEEVENTF_RIGHTUP, IncrementX, IncrementY, 0, UIntPtr.Zero);

        }

        public void ClickRightMouse(string name, string type, bool flag, string pname, string ptype, int offsetX, int offsetY)
        {
            //ClickRightMouse(name, type, "");
            AutomationElement element = FindElement(name, type,flag, pname, ptype);

            if (element == null)
            {
                return;
                throw new NullReferenceException(string.Format("Element with AutomationId '{0}' and Name '{1}' can not be find.",

                    element.Current.AutomationId, element.Current.Name));
            }

            Rect rect = element.Current.BoundingRectangle;

            int IncrementX = (int)(rect.Left + offsetX);

            int IncrementY = (int)(rect.Top + offsetY);
            if (offsetX == 0 && offsetY == 0)
            {
                Point p = element.GetClickablePoint();
                IncrementX = (int)p.X;
                IncrementY = (int)p.Y;
            }
            
            SetCursorPos(IncrementX, IncrementY);

            mouse_event(MOUSEEVENTF_RIGHTDOWN, IncrementX, IncrementY, 0, UIntPtr.Zero);

            mouse_event(MOUSEEVENTF_RIGHTUP, IncrementX, IncrementY, 0, UIntPtr.Zero);
        }

        public void DoubleMouseDown(string name, string type, bool flag, string autoId, int offsetX, int offsetY)
        {
            AutomationElement element = FindElement( name, type,flag,autoId);

            if (element == null)
            {
                return;
                //throw new NullReferenceException(string.Format("Element with AutomationId '{0}' and Name '{1}' can not be find.",

                //    element.Current.AutomationId, element.Current.Name));

            }

            #region MyRegion
            //InvokePattern invokePattern = null;

            //try
            //{
            //    invokePattern = element.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
            //}
            //catch (ElementNotEnabledException)
            //{
            //    // Object is not enabled
            //    return;
            //}
            //catch (InvalidOperationException)
            //{
            //    // object doesn't support the InvokePattern control pattern
            //    return;
            //}

            //invokePattern.Invoke();
            //invokePattern.Invoke(); 
            #endregion

            Rect rect = element.Current.BoundingRectangle;

            //int IncrementX = (int)(rect.Left + rect.Width / 2);

            //int IncrementY = (int)(rect.Top + rect.Height / 2);

            //Make the cursor position to the element.
            //Point p = element.GetClickablePoint();
            int IncrementX = (int)rect.X+offsetX;
            int IncrementY = (int)rect.Y+offsetY;
            if (offsetX == 0 && offsetY == 0)
            {
                Point p = element.GetClickablePoint();
                IncrementX = (int)p.X;
                IncrementY = (int)p.Y;
            }
            SetCursorPos(IncrementX, IncrementY);

            //Make the left mouse down and up.

            mouse_event(MOUSEEVENTF_LEFTDOWN, IncrementX, IncrementY, 0, UIntPtr.Zero);

            mouse_event(MOUSEEVENTF_LEFTUP, IncrementX, IncrementY, 0, UIntPtr.Zero);
            Thread.Sleep(dcTime);
            mouse_event(MOUSEEVENTF_LEFTDOWN, IncrementX, IncrementY, 0, UIntPtr.Zero);

            mouse_event(MOUSEEVENTF_LEFTUP, IncrementX, IncrementY, 0, UIntPtr.Zero);
        }

        public void DoubleMouseDown(string name, string type, bool flag, string pname, string ptype, int offsetX, int offsetY)
        {
            AutomationElement element = FindElement(name, type, flag,pname, ptype);

            if (element == null)
            {
                return;
                throw new NullReferenceException(string.Format("Element with AutomationId '{0}' and Name '{1}' can not be find.",

                    element.Current.AutomationId, element.Current.Name));
            }
            Rect rect = element.Current.BoundingRectangle;

            //int IncrementX = (int)(rect.Left + rect.Width / 2);

            //int IncrementY = (int)(rect.Top + rect.Height / 2);

            //Make the cursor position to the element.
            //Point p = element.GetClickablePoint();
            int IncrementX = (int)rect.X + offsetX;
            int IncrementY = (int)rect.Y + offsetY;
            if (offsetX == 0 && offsetY == 0)
            {
                Point p = element.GetClickablePoint();
                IncrementX = (int)p.X;
                IncrementY = (int)p.Y;
            }
            SetCursorPos(IncrementX, IncrementY);
            
            //SetCursorPos((int)p.X, ()p.Y);
            //Make the left mouse down and up.

            
            mouse_event(MOUSEEVENTF_LEFTDOWN, IncrementX, IncrementY, 0, UIntPtr.Zero);

            mouse_event(MOUSEEVENTF_LEFTUP, IncrementX, IncrementY, 0, UIntPtr.Zero);
            Thread.Sleep(dcTime);
            mouse_event(MOUSEEVENTF_LEFTDOWN, IncrementX, IncrementY, 0, UIntPtr.Zero);

            mouse_event(MOUSEEVENTF_LEFTUP, IncrementX, IncrementY, 0, UIntPtr.Zero);
        }

        public void Move(string name, string type, bool flag, string autoId, int offsetX, int offsetY, int moveX, int moveY)
        {
            AutomationElement element = FindElement(name, type,flag, autoId);
            if (element == null)
            {
                return;
            }
            Rect rect = element.Current.BoundingRectangle;
            int IncrementX = (int)rect.X + offsetX;
            int IncrementY = (int)rect.Y + offsetY;
            SetCursorPos(IncrementX, IncrementY);
            mouse_event(MOUSEEVENTF_LEFTDOWN, IncrementX, IncrementY, 0, UIntPtr.Zero);
            mouse_event(MOUSEEVENTF_MOVE, IncrementX + moveX, IncrementY + moveY, 0, UIntPtr.Zero);
            mouse_event(MOUSEEVENTF_LEFTUP, IncrementX, IncrementY, 0, UIntPtr.Zero);
        }

        public void Move(string name, string type, bool flag, string pname, string ptype, int offsetX, int offsetY, int moveX, int moveY)
        {
            AutomationElement element = FindElement(name, type,flag, pname,ptype);
            if (element == null)
            {
                return;
            }
            Rect rect = element.Current.BoundingRectangle;
            int IncrementX = (int)rect.X + offsetX;
            int IncrementY = (int)rect.Y + offsetY;
            SetCursorPos(IncrementX, IncrementY);
            mouse_event(MOUSEEVENTF_LEFTDOWN, IncrementX, IncrementY, 0, UIntPtr.Zero);
            mouse_event(MOUSEEVENTF_MOVE, IncrementX + moveX, IncrementY + moveY, 0, UIntPtr.Zero);
            //SetCursorPos(IncrementX + moveX, IncrementY + moveY);
            mouse_event(MOUSEEVENTF_LEFTUP, IncrementX, IncrementY, 0, UIntPtr.Zero);
        }

        public void SetValue(string name, string type, bool flag, string autoId, string value)
        {
            AutomationElement element = FindElement(name, type,flag, autoId);
            if (element==null)
            {
                return;
            }
            int handler = element.Current.NativeWindowHandle;

            string[] val = value.Split('|');
            for (int i = 0; i < val.Length-1;i++ )
            {
                String v = val[i];
                
                SendMessage((IntPtr)handler, LB_ADDSTRING,IntPtr.Zero,v);
            }
        }

        public void SetValue(string name, string type, bool flag, string pname, string ptype, string value)
        {
            AutomationElement element = FindElement(name, type,flag, pname, ptype);
            if (element == null)
            {
                return;
            }
            ControlType ctype = element.Current.ControlType;
            if (ctype == ControlType.List)
            {
                int handler = element.Current.NativeWindowHandle;
                string[] val = value.Split('|');
                for (int i = 0; i < val.Length - 1; i++)
                {
                    String v = val[i];

                    SendMessage((IntPtr)handler, LB_ADDSTRING, IntPtr.Zero, v);
                }
            }
            
        }

        public void AnalysisType(AutomationElement element,int offsetX,int offsetY)
        {
            ControlType type = element.Current.ControlType;
            
            if (type == ControlType.RadioButton||type == ControlType.ListItem)
            {
                SelectionItemPattern selectionItemPattern = GetSelectionItemPattern(element);
                selectionItemPattern.Select();
            }
            else if (type == ControlType.TabItem)
            {
                SelectionItemPattern selectionItemPattern = GetSelectionItemPattern(element);
                selectionItemPattern.Select();
            }
            else if (type == ControlType.MenuBar)
            {
                InvokePattern invokePattern = element.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                invokePattern.Invoke();
            }
            else if (type == ControlType.ComboBox)
            {
                ExpandCollapsePattern currentPattern = GetExpandCollapsePattern(element);
                currentPattern.Expand();
            }
            else if (type == ControlType.MenuItem)
            {
                ExpandCollapsePattern pattern = element.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
                pattern.Expand();
               
            }
            else if (type == ControlType.TreeItem)
            {
                ExpandCollapsePattern currentPattern = GetExpandCollapsePattern(element);
                if (currentPattern == null)
                {
                    return;
                }
                if (currentPattern.Current.ExpandCollapseState == ExpandCollapseState.LeafNode)
                {
                    SelectionItemPattern selectionItemPattern = GetSelectionItemPattern(element);
                    selectionItemPattern.Select();
                }
                else if (currentPattern.Current.ExpandCollapseState == ExpandCollapseState.Expanded)
                {
                    currentPattern.Collapse();
                }
                else if (currentPattern.Current.ExpandCollapseState == ExpandCollapseState.Collapsed)
                {
                    currentPattern.Expand();
                }

                //= ExpandCollapseState.LeafNode;
            }
//            else if (type == ControlType.Edit)
//            {
//                //element.SetFocus();
//                //if (isClear)
//                //{
////                     ValuePattern currentPattern = GetValuePattern(element);
////                     currentPattern.SetValue("");
//                //}
//                //Thread.Sleep(100);
//            }
            else if (type == ControlType.CheckBox)
            {
                TogglePattern togglePattern = GetTogglePattern(element);
                togglePattern.Toggle();
            }
            else
            {

                System.Windows.Rect rect = element.Current.BoundingRectangle;
                int x = (int)rect.X + offsetX;
                int y = (int)rect.Y + offsetY;
                if (offsetX == 0 && offsetY == 0)
                {
                    Point point = element.GetClickablePoint();
                    x = (int)point.X;
                    y = (int)point.Y;
                }
                SetCursorPos(x, y);

                mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, UIntPtr.Zero);

                mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, UIntPtr.Zero);
                
            }
        }

        public static TogglePattern GetTogglePattern(AutomationElement element)
        {
            object currentPattern;
            if (!element.TryGetCachedPattern(TogglePattern.Pattern, out currentPattern))
            {
                if (!element.TryGetCurrentPattern(TogglePattern.Pattern,out currentPattern))
                {
                    throw new Exception(string.Format("Element with AutomationId '{0}' and Name '{1}' does not support the TogglePattern.",
                    element.Current.AutomationId, element.Current.Name));
                }
                
            }
            return currentPattern as TogglePattern;
        }

        public static ValuePattern GetValuePattern(AutomationElement element)
        {
            object currentPattern;
            if (!element.TryGetCachedPattern(ValuePattern.Pattern, out currentPattern))
            {
                if (!element.TryGetCurrentPattern(ValuePattern.Pattern,out currentPattern))
                {
                    throw new Exception(string.Format("Element with AutomationId '{0}' and Name '{1}' does not support the ValuePattern.",
                    element.Current.AutomationId, element.Current.Name));
                }
                
            }
            return currentPattern as ValuePattern;
        }

        public static ExpandCollapsePattern GetExpandCollapsePattern(AutomationElement element)
        {
            object currentPattern;
            if (!element.TryGetCachedPattern(ExpandCollapsePattern.Pattern, out currentPattern))
            {
                if (!element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern,out currentPattern))
                {
                    throw new Exception(string.Format("Element with AutomationId '{0}' and Name '{1}' does not support the ExpandCollapsePattern.",
                   element.Current.AutomationId, element.Current.Name));
                }
               
            }
            return currentPattern as ExpandCollapsePattern;
        }

        public static SelectionItemPattern GetSelectionItemPattern(AutomationElement element)
        {
            object currentPattern;
            if (!element.TryGetCachedPattern(SelectionItemPattern.Pattern, out currentPattern))
            {
                if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern,out currentPattern))
                {
                    throw new Exception(string.Format("Element with AutomationId '{0}' and Name '{1}' does not support the SelectionItemPattern.",
                    element.Current.AutomationId, element.Current.Name));
                }
                
            }
            return currentPattern as SelectionItemPattern;
        }

        public void SendKey(string key)
        {
            Keys k = (Keys)Enum.Parse(typeof(Keys), key, true);

            Win32Api.KeyDown(k);
            Win32Api.KeyUp(k);
        }

        public void KeyDown(string key)
        {
            Keys k = (Keys)Enum.Parse(typeof(Keys), key, true);

            Win32Api.KeyDown(k);
            //object valuePattern = null;
            //AutomationElement element = AutomationElement.FocusedElement;
            //if (element.TryGetCurrentPattern(ValuePattern.Pattern, out valuePattern))
            //{
                
            //    ((ValuePattern)valuePattern).SetValue(key);
            //}
        }

        public void KeyUp(string key)
        {
            Keys k = (Keys)Enum.Parse(typeof(Keys), key, true);

            Win32Api.KeyUp(k);
        }

        public void SetFocus(string name, string type,bool flag,string autoId)
        {
            AutomationElement element = FindElement(name, type,flag,autoId);

            if (element == null)
            {

                throw new NullReferenceException(string.Format("Element with AutomationId '{0}' and Name '{1}' can not be find.",

                    element.Current.AutomationId, element.Current.Name));

            }

            element.SetFocus();
        }

        public void SetFocus(string name, string type,bool flag, string pname,string ptype)
        {
            AutomationElement element = FindElement(name, type, flag, pname, ptype);

            if (element == null)
            {

                throw new NullReferenceException(string.Format("Element with AutomationId '{0}' and Name '{1}' can not be find.",

                    element.Current.AutomationId, element.Current.Name));

            }

            element.SetFocus();
        }

        public AutomationElement FindWindowByProcessId(int processId)
        {

            AutomationElement targetWindow = null;

            int count = 0;

            try
            {

                Process p = Process.GetProcessById(processId);

                targetWindow = AutomationElement.FromHandle(p.MainWindowHandle);

                return targetWindow;

            }

            catch (Exception ex)
            {

                count++;

                StringBuilder sb = new StringBuilder();

                string message = sb.AppendLine(string.Format("Target window is not existing.try #{0}", count)).ToString();

                if (count > 5)
                {

                    throw new InvalidProgramException(message, ex);

                }

                else
                {

                    return FindWindowByProcessId(processId);

                }

            }

        }

        public AutomationElement FindElement(string name, string type, bool flag, string autoId)
        {
            
            int count = 0;
            while (!SyncFlag)
            {
                Thread.Sleep(500);
                if (count > 10)
                {
                    SyncFlag = true;
                    AutomationElement elem = WalkEnabledElements(mainWindow, name, type, autoId, flag);
//                     AutomationElement elem = FindCurrentElement(name, type, autoId);
                    if (elem != null)
                    {
                        return elem;
                    }
                }
                count++;
            }
            
            AutomationElement element = FindCachedElement(name, type,flag, autoId);
            if (element!=null)
            {
                return element;
            }
            
            UpdateCache();
            element = FindCachedElement(name, type,flag, autoId);
            
            if (element != null)
            {
                return element;
            }

            //element = FindElementFromDesktop(name, type, autoId, true);
            tname = name;
            ttype = type;
            tautoId = autoId;
            tenable = flag;
            FindElementFromHandle();
            
            element = targetElement;
            //System.Windows.Rect rect = element.Current.BoundingRectangle;
            //Console.WriteLine(rect.X + " " + rect.Y + " " + rect.Bottom + " " + rect.Right);
            targetElement = null;
            if (element!=null)
            {
                return element;
            }
            return null;
            #region MyRegion
            ////AutomationElement aeForm = FindWindowByProcessId(processId);

            //Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            //Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
            ////Condition idcondition = new PropertyCondition(AutomationElement.ProcessIdProperty, process.Id);
            //Condition nameCondition = new PropertyCondition(AutomationElement.NameProperty, name);

            //Condition conditions = new AndCondition(nameCondition, condition2, condition1);
            //if (type != "")
            //{
            //    Condition typeCondition = new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, type);
            //    conditions = new AndCondition(conditions,typeCondition);
            //}
            //if (autoId != "")
            //{
            //    Condition autoIdCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, autoId);
            //    conditions = new AndCondition(conditions, autoIdCondition);
            //}


            //if (conditions==null)
            //{
            //    return null;
            //}
            //int count = 0;

            //while (true)
            //{
            //    //AutomationElement targetElem = mainWindow.FindFirst(TreeScope.Subtree, conditions);
            //    //if (targetElem != null)
            //    //{
            //    //    return targetElem;
            //    //}
            //    for (int i = 0; i < elementCollection.Count; i++)
            //    {
            //        AutomationElement targetElem = elementCollection[i];

            //        if (targetElem != null && name == targetElem.Cached.Name && type == targetElem.Cached.LocalizedControlType && autoId == targetElem.Cached.AutomationId)
            //        {
            //            return targetElem;
            //        }
            //    }


            //    if (count > 10)
            //    {
            //        break;
            //    }
            //    Thread.Sleep(200);
            //    count++;
            //}



            //return null; 
            #endregion
        }
        
        public AutomationElement FindCachedElement(string name, string type,bool flag, string autoId)
        {
            int count = 0;
            while (true)
            {
                for (int i = 0; i < elementCollection.Count; i++)
                {
                    try
                    {
                        AutomationElement targetElem = elementCollection[i];

                        if (targetElem != null && name == targetElem.Current.Name && type == targetElem.Current.LocalizedControlType
                            && autoId == targetElem.Current.AutomationId && flag == targetElem.Current.IsEnabled)
                        {
                            return targetElem;
                        }

                    }
                    catch (System.Exception ex)
                    {

                    }
                    
                }
                int id = -1;
                Int32.TryParse(autoId,out id);
                if (id!=-1)
                {
                    for (int i = 0; i < elementCollection.Count; i++)
                    {
                        try
                        {
                            AutomationElement targetElem = elementCollection[i];

                            if (targetElem != null && name == targetElem.Current.Name && type == targetElem.Current.LocalizedControlType && flag == targetElem.Current.IsEnabled)
                            {
                                return targetElem;
                            }

                        }
                        catch (System.Exception ex)
                        {

                        }

                    }
                }

                #region MyRegion
                //for (int i = 0; i < collectionList.Count;i++ )
                //{
                //    AutomationElementCollection collection = collectionList[i];
                //    for (int j = 0; j < collection.Count; j++)
                //    {
                //        try
                //        {
                //            AutomationElement targetElem = elementCollection[j];

                //            if (targetElem != null && name == targetElem.Cached.Name && type == targetElem.Cached.LocalizedControlType && autoId == targetElem.Cached.AutomationId)
                //            {
                //                return targetElem;
                //            }

                //        }
                //        catch (System.Exception ex)
                //        {

                //        }
                //    }
                //} 
                #endregion
                if (count > 10)
                {
                    break;
                }
                Thread.Sleep(200);
                count++;
                
            }
            return null;
        }

        public AutomationElement FindCurrentElement(string name, string type, string autoId,bool flag)
        {
            Condition conditions = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            
            
            //Condition idcondition = new PropertyCondition(AutomationElement.ProcessIdProperty, process.Id);
            Condition nameCondition = new PropertyCondition(AutomationElement.NameProperty, name);

            conditions = new AndCondition(nameCondition, conditions);
            
            Condition enableCondition = new PropertyCondition(AutomationElement.IsEnabledProperty, flag);
            conditions = new AndCondition(conditions, enableCondition);
            
            if (type != "")
            {
                Condition typeCondition = new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, type);
                conditions = new AndCondition(conditions, typeCondition);
            }
            if (autoId != "")
            {
                Condition autoIdCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, autoId);
                conditions = new AndCondition(conditions, autoIdCondition);
            }


            if (conditions == null)
            {
                return null;
            }
            int count = 0;

            while (true)
            {
                AutomationElement targetElem = mainWindow.FindFirst(TreeScope.Subtree, conditions);
                if (targetElem != null)
                {
                    return targetElem;
                }
                
                if (count > 10)
                {
                    break;
                }
                Thread.Sleep(200);
                count++;
            }



            return null;
        }

        public AutomationElement FindElementFromDesktop(string name, string type, string autoId, bool flag)
        {
            Condition conditions = new PropertyCondition(AutomationElement.IsControlElementProperty, true);


            //Condition idcondition = new PropertyCondition(AutomationElement.ProcessIdProperty, process.Id);
            Condition nameCondition = new PropertyCondition(AutomationElement.NameProperty, name);

            conditions = new AndCondition(nameCondition, conditions);

            Condition enableCondition = new PropertyCondition(AutomationElement.IsEnabledProperty, flag);
            conditions = new AndCondition(conditions, enableCondition);

            if (type != "")
            {
                Condition typeCondition = new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, type);
                conditions = new AndCondition(conditions, typeCondition);
            }
            if (autoId != "")
            {
                Condition autoIdCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, autoId);
                conditions = new AndCondition(conditions, autoIdCondition);
            }


            if (conditions == null)
            {
                return null;
            }
            int count = 0;

            while (true)
            {
                AutomationElement targetElem = AutomationElement.RootElement.FindFirst(TreeScope.Subtree, conditions);
                if (targetElem != null)
                {
                    return targetElem;
                }

                if (count > 10)
                {
                    break;
                }
                Thread.Sleep(200);
                count++;
            }
            return null;
        }

        private AutomationElement WalkEnabledElements(AutomationElement rootElement,string name, string type,string autoId,bool flag)
        {
            Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, flag);
            //Condition condition3 = new NotCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Image));
            //Condition condition4 = new NotCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text));
            //Condition condition5 = new NotCondition(new PropertyCondition(AutomationElement.NameProperty, "chart1"));
            TreeWalker walker = new TreeWalker(new AndCondition(condition1, condition2));
            AutomationElement elementNode = walker.GetFirstChild(rootElement);
            int count = 0;
            while (elementNode != null)
            {
                if (elementNode.Current.Name == name&&elementNode.Current.LocalizedControlType==type&&elementNode.Current.AutomationId==autoId)
                {
                    return elementNode;
                }
                if (elementNode.Current.LocalizedControlType != "image")
                {
                    AutomationElement elem = WalkEnabledElements(elementNode,name,type,autoId,flag);
                    if (elem!=null)
                    {
                        return elem;
                    }
                }
                //
                elementNode = walker.GetNextSibling(elementNode);
                count++;
            }
            return null;
        }
        
        private AutomationElement WalkEnabledElements(AutomationElement rootElement, string name, string type,bool flag, string pname,string ptype)
        {
            Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, flag);
            //Condition condition3 = new NotCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Image));
            //Condition condition4 = new NotCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text));
            //Condition condition5 = new NotCondition(new PropertyCondition(AutomationElement.NameProperty, "chart1"));
            TreeWalker walker = new TreeWalker(new AndCondition(condition1, condition2));
            AutomationElement elementNode = walker.GetFirstChild(rootElement);
            int count = 0;
            while (elementNode != null)
            {
                if (elementNode.Current.Name == name && elementNode.Current.LocalizedControlType == type && rootElement.Current.Name == pname&&rootElement.Current.LocalizedControlType == ptype)
                {
                    return elementNode;
                }
                if (elementNode.Current.LocalizedControlType != "image")
                {
                    AutomationElement elem = WalkEnabledElements(elementNode, name, type,flag, pname,ptype);
                    if (elem != null)
                    {
                        return elem;
                    }
                }
                //
                elementNode = walker.GetNextSibling(elementNode);
                count++;
            }
            return null;
        }
        
        public AutomationElement FindElement(string name, string type,bool flag, string pname, string ptype)
        {
            int count = 0;
            while (!SyncFlag)
            {
                Thread.Sleep(500);
                if (count > 10)
                {
                    SyncFlag = true;
                    AutomationElement elem = WalkEnabledElements(mainWindow, name, type, flag, pname, ptype);
                    //                     AutomationElement elem = FindCurrentElement(name, type, autoId);
                    if (elem != null)
                    {
                        return elem;
                    }
                }
                count++;
            }
            #region MyRegion
            //TreeWalker walker = TreeWalker.ControlViewWalker;
            //for (int i = 0; i < elementCollection.Count; i++)
            //{
            //    AutomationElement element = elementCollection[i];
            //    if (element != null)
            //    {
            //        try
            //        {
            //            if (name == element.Cached.Name && type == element.Cached.LocalizedControlType)
            //            {
            //                AutomationElement parent = walker.GetParent(element);
            //                //AutomationElement parent = element.CachedParent;
            //                if (parent != null)
            //                {
            //                    if (pname == parent.Current.Name && ptype == parent.Current.LocalizedControlType)
            //                    {
            //                        return element;
            //                    }
            //                }
            //            }
            //        }
            //        catch (System.Exception ex)
            //        {

            //        }

            //    }
            //} 
            #endregion
            
            AutomationElement element = FindCachedElement(name, type, flag,pname, ptype);
            
            if (element!=null)
            {
                return element;
            }
            
            UpdateCache();
            element = FindCachedElement(name, type, flag, pname, ptype);
            
            if (element != null)
            {
                return element;
            }
//          
            #region MyRegion
            //AutomationElementCollection collection = FindElements(name, type);
            //if (collection == null)
            //{
            //    return null;
            //}
            //if (collection.Count == 1)
            //{
            //    return collection[0];
            //}
            //else if (collection.Count == 0)
            //{
            //    AutomationElement element = FindElement(name, type, "");
            //    if (element != null)
            //    {
            //        return element;
            //    }
            //    else
            //    {
            //        return null;
            //    }
            //}
            //TreeWalker walker = TreeWalker.ControlViewWalker;
            //for (int i = 0; i < collection.Count; i++)
            //{
            //    AutomationElement element = collection[i];

            //    AutomationElement parent = walker.GetParent(element);
            //    if (parent != null)
            //    {
            //        string n = parent.Current.Name;
            //        string t = parent.Current.LocalizedControlType;
            //        if (n == pname && t == ptype)
            //        {
            //            return element;
            //        }
            //    }
            //}
            #endregion
            tname = name;
            ttype = type;
            tenable = flag;
            tpname = pname;
            tptype = ptype;
            FindElementFromHandleEx();
//          
            element = targetElement;
            
            targetElement = null;
            if (element != null)
            {
                return element;
            }

            #region MyRegion
            //AutomationElement pelem = AutomationElement.FromHandle(mainHandle);
            //if (pelem != null)
            //{
            //    element = FindChildElement(pelem, name, type,  true,pname,ptype);
            //    if (element != null)
            //    {
            //        return element;
            //    }
            //}


            //for (int i = 0; i < windowHandle.Count; i++)
            //{
            //    try
            //    {
            //        pelem = AutomationElement.FromHandle(windowHandle[i]);
            //        if (pelem != null)
            //        {
            //            element = FindChildElement(pelem, name, type, true, pname, ptype);
            //            if (element != null)
            //            {
            //                return element;
            //            }
            //        }
            //    }
            //    catch (System.Exception ex)
            //    {

            //    }

            //} 
            #endregion
            return null;
        }
        
        public AutomationElement FindCachedElement(string name, string type,bool flag, string pname, string ptype)
        {
            TreeWalker walker = TreeWalker.ControlViewWalker;
            for (int i = 0; i < elementCollection.Count; i++)
            {
                AutomationElement element = elementCollection[i];
                if (element != null)
                {
                    try
                    {
                        if (name == element.Current.Name && type == element.Current.LocalizedControlType&&flag == element.Current.IsEnabled)
                        {
                            AutomationElement parent = walker.GetParent(element);
                            //AutomationElement parent = element.CachedParent;
                            if (parent != null)
                            {
                                if (pname == parent.Current.Name && ptype == parent.Current.LocalizedControlType)
                                {
                                    return element;
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {

                    }

                }
            }
            #region MyRegion
            //for (int i = 0; i < collectionList.Count; i++)
            //{
            //    AutomationElementCollection collection = collectionList[i];
            //    for (int j = 0; j < elementCollection.Count; j++)
            //    {
            //        AutomationElement element = elementCollection[j];
            //        if (element != null)
            //        {
            //            try
            //            {
            //                if (name == element.Current.Name && type == element.Current.LocalizedControlType)
            //                {
            //                    AutomationElement parent = walker.GetParent(element);
            //                    //AutomationElement parent = element.CachedParent;
            //                    if (parent != null)
            //                    {
            //                        if (pname == parent.Current.Name && ptype == parent.Current.LocalizedControlType)
            //                        {
            //                            return element;
            //                        }
            //                    }
            //                }
            //            }
            //            catch (System.Exception ex)
            //            {

            //            }

            //        }
            //    }
            //} 
            #endregion
            return null;
        }

        //public AutomationElement FindCurrentElement(string name, string type, bool flag,string pname, string ptype)
        //{
        //    AutomationElementCollection collection = FindElements(name, type,flag);
        //    if (collection == null)
        //    {
        //        return null;
        //    }
        //    if (collection.Count == 1)
        //    {
        //        return collection[0];
        //    }
        //    else if (collection.Count == 0)
        //    {
        //        AutomationElement element = FindElement(name, type, "");
        //        if (element != null)
        //        {
        //            return element;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //    TreeWalker walker = TreeWalker.ControlViewWalker;
        //    for (int i = 0; i < collection.Count; i++)
        //    {
        //        AutomationElement element = collection[i];

        //        AutomationElement parent = walker.GetParent(element);
        //        if (parent != null)
        //        {
        //            string n = parent.Current.Name;
        //            string t = parent.Current.LocalizedControlType;
        //            if (n == pname && t == ptype)
        //            {
        //                return element;
        //            }
        //        }
        //    }

        //    return null;
        //}

        public AutomationElement FindCurrentElement(string name, string type, bool flag, string pname, string ptype)
        {
            return WalkEnabledElements(mainWindow, name, type, flag, pname, ptype);
        }

        public AutomationElement FindElementFromDesktop(string name, string type, bool flag, string pname, string ptype)
        {
            return WalkEnabledElements(AutomationElement.RootElement, name, type, flag, pname, ptype);
        }

        public AutomationElementCollection FindElements(string name, string type,bool flag)
        {
            Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, flag);
            //Condition idcondition = new PropertyCondition(AutomationElement.ProcessIdProperty, process.Id);
            Condition nameCondition = new PropertyCondition(AutomationElement.NameProperty, name);

            Condition conditions = new AndCondition(nameCondition, condition2, condition1);
            if (type != "")
            {
                Condition typeCondition = new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, type);
                conditions = new AndCondition(conditions, typeCondition);
            }
            
            if (conditions == null)
            {
                return null;
            }
            int count = 0;
            
            while (true)
            {
                AutomationElementCollection targetElem = mainWindow.FindAll(TreeScope.Descendants, conditions);
                if (targetElem != null)
                {
                    return targetElem;
                }
                if (count > 10)
                {
                    break;
                }
                Thread.Sleep(200);
                count++;
            }
            return null;
        }
        
        public AutomationElementCollection FindElementsFromDesktop(string name, string type, bool flag)
        {
            Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, flag);
            //Condition idcondition = new PropertyCondition(AutomationElement.ProcessIdProperty, process.Id);
            Condition nameCondition = new PropertyCondition(AutomationElement.NameProperty, name);

            Condition conditions = new AndCondition(nameCondition, condition2, condition1);
            if (type != "")
            {
                Condition typeCondition = new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, type);
                conditions = new AndCondition(conditions, typeCondition);
            }

            if (conditions == null)
            {
                return null;
            }
            int count = 0;

            while (true)
            {
                AutomationElementCollection targetElem = AutomationElement.RootElement.FindAll(TreeScope.Descendants, conditions);
                if (targetElem != null)
                {
                    return targetElem;
                }
                if (count > 10)
                {
                    break;
                }
                Thread.Sleep(200);
                count++;
            }
            return null;
        }

        public AutomationElement FindElementById(int processId, string automationId)
        {

            AutomationElement aeForm = FindWindowByProcessId(processId);

            AutomationElement tarFindElement = aeForm.FindFirst(TreeScope.Descendants,

            new PropertyCondition(AutomationElement.AutomationIdProperty, automationId));

            return tarFindElement;

        }

        public void FindElementFromHandle()
        {
            EnumThreadWindowsCallback callback1 = new EnumThreadWindowsCallback(EnumWindowsCallback);
            EnumWindows(callback1, IntPtr.Zero);
        }

        public void FindElementFromHandleEx()
        {
            EnumThreadWindowsCallback callback1 = new EnumThreadWindowsCallback(EnumWindowsCallbackEx);
            EnumWindows(callback1, IntPtr.Zero);
            
        }

        private bool EnumWindowsCallback(IntPtr handle, IntPtr extraParameter)
        {
            bool flag = false;
            int processid = 0;
            GetWindowThreadProcessId(handle, ref processid);
            if (processid != process.Id)
            {
                for (int i = 0; i < pid.Count;i++ )
                {
                    if (processid == pid[i])
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    return true;
                }
            }
            
            AutomationElement element = AutomationElement.FromHandle(handle);
            string name = element.Current.Name;
            string type = element.Current.LocalizedControlType;
            Console.WriteLine("name:" + name + " type:" + type);
            //AutomationElement tempElement = mainWindow;
            //mainWindow = element;
            Thread thread = new Thread(() =>
            {
                try
                {
                    targetElement = FindChildElement(element, tname, ttype, tautoId, tenable);
                }
                catch (System.Exception ex)
                {
                   
                }


            });
            thread.Start();
            thread.Join();

            if (targetElement!=null)
            {
                return false;
            }
            //mainWindow = tempElement;
            return true;
        }

        private bool EnumWindowsCallbackEx(IntPtr handle, IntPtr extraParameter)
        {
            bool flag = false;
            int processid = 0;
            GetWindowThreadProcessId(handle, ref processid);
            if (processid != process.Id)
            {
                for (int i = 0; i < pid.Count; i++)
                {
                    if (processid == pid[i])
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    return true;
                }
            }
            
            AutomationElement element = AutomationElement.FromHandle(handle);
//             string name = element.Current.Name;
//             string type = element.Current.LocalizedControlType;
//             Console.WriteLine("name:" + name + " type:" + type);
            //AutomationElement tempElement = mainWindow;
            //mainWindow = element;
            Thread thread = new Thread(() =>
            {
                try
                {
                    targetElement = FindChildElement(element, tname, ttype,tenable,  tpname,tptype);
                }
                catch (System.Exception ex)
                {

                }


            });
            thread.Start();
            thread.Join();

            if (targetElement != null)
            {
                return false;
            }
            //mainWindow = tempElement;
            return true;
        }

        private AutomationElement FindChildElement(AutomationElement rootElement, string name, string type, string autoId, bool flag)
        {
            #region MyRegion
            //             Condition conditions = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            // 
            // 
            //             //Condition idcondition = new PropertyCondition(AutomationElement.ProcessIdProperty, process.Id);
            //             Condition nameCondition = new PropertyCondition(AutomationElement.NameProperty, name);
            // 
            //             conditions = new AndCondition(nameCondition, conditions);
            // 
            //             Condition enableCondition = new PropertyCondition(AutomationElement.IsEnabledProperty, flag);
            //             conditions = new AndCondition(conditions, enableCondition);
            // 
            //             if (type != "")
            //             {
            //                 Condition typeCondition = new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, type);
            //                 conditions = new AndCondition(conditions, typeCondition);
            //             }
            //             if (autoId != "")
            //             {
            //                 Condition autoIdCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, autoId);
            //                 conditions = new AndCondition(conditions, autoIdCondition);
            //             }
            // 
            // 
            //             if (conditions == null)
            //             {
            //                 return null;
            //             }
            //             int count = 0;
            // 
            //             while (true)
            //             {
            //                 AutomationElement targetElem = element.FindFirst(TreeScope.Descendants, conditions);
            //                 string n = targetElem.Current.Name;
            //                 string t = targetElem.Current.LocalizedControlType;
            //                 
            //                 if (targetElem != null)
            //                 {
            //                     return targetElem;
            //                 }
            // 
            //                 if (count > 10)
            //                 {
            //                     break;
            //                 }
            //                 Thread.Sleep(200);
            //                 count++;
            //             }
            
            #endregion            
            Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            //Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, flag);
            //Condition condition3 = new NotCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Image));
            //Condition condition4 = new NotCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text));
            //Condition condition5 = new NotCondition(new PropertyCondition(AutomationElement.NameProperty, "chart1"));
            TreeWalker walker = new TreeWalker(condition1);
            AutomationElement elementNode = walker.GetFirstChild(rootElement);
            int count = 0;
            while (elementNode != null)
            {
                if (elementNode.Current.Name == name && elementNode.Current.LocalizedControlType == type && elementNode.Current.AutomationId == autoId)
                {
                    return elementNode;
                }
                AutomationElement element = WalkEnabledElements(elementNode, name, type,autoId, flag);
                if (element != null)
                {
                    return element;
                }
                //
                elementNode = walker.GetNextSibling(elementNode);
                count++;
            }
            return null;
        }

        private AutomationElement FindChildElement(AutomationElement rootElement, string name, string type, bool flag, string pname, string ptype)
        {
            Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, flag);
            //Condition condition3 = new NotCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Image));
            //Condition condition4 = new NotCondition(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text));
            //Condition condition5 = new NotCondition(new PropertyCondition(AutomationElement.NameProperty, "chart1"));
            TreeWalker walker = new TreeWalker(new AndCondition(condition1, condition2));
            AutomationElement elementNode = walker.GetFirstChild(rootElement);
            int count = 0;
            while (elementNode != null)
            {
                if (elementNode.Current.Name == name && elementNode.Current.LocalizedControlType == type && rootElement.Current.Name == pname&&rootElement.Current.LocalizedControlType == ptype)
                {
                    return elementNode;
                }
                AutomationElement element = WalkEnabledElements(elementNode, name, type, flag, pname, ptype);
                if (element!=null)
                {
                    return element;
                }
                //
                elementNode = walker.GetNextSibling(elementNode);
                count++;
            }
            return null;
        
        }

        public string[] GetString(string str)
        {
            //string delimstr = " []";
            //char[] delimiter = delimstr.ToCharArray();

            //string[] split = str.Split(new char[]{' '});

            //int index = str.IndexOf(" ");
            //if (index<0)
            //{
            //    return new string[]{ str };
            //}
            //string cmd = str.Substring(0, index);
            //string element = str.Substring(index+1);

            string[] strList = str.Split('\"');
            return strList;

            #region MyRegion
            //int startIndex = str.IndexOf("\"", index + 1);
            //int endIndex = str.IndexOf("\"", startIndex + 1);
            //if (startIndex==-1||endIndex==-1)
            //{
            //    return new string[] { cmd };
            //}
            //string arg1 = str.Substring(startIndex+1, endIndex - startIndex-1);

            //startIndex = str.IndexOf("\"",endIndex+1);
            //endIndex = str.IndexOf("\"", startIndex + 1);
            //if (startIndex==-1||endIndex==-1)
            //{
            //    return new string[] { cmd, arg1 };

            //}
            //string arg2 = str.Substring(startIndex+1, endIndex - startIndex-1);

            //startIndex = str.IndexOf("\"", endIndex + 1);
            //endIndex = str.IndexOf("\"", startIndex + 1);
            //if (startIndex == -1 || endIndex == -1)
            //{
            //    return new string[] { cmd, arg1,arg2 };

            //}

            //string arg3 = str.Substring(startIndex + 1, endIndex - startIndex - 1);

            //string[] split = new string[] { cmd, arg1, arg2 ,arg3};
            //return split; 
            #endregion
            //return "";
        }

        public void Start(string srcPath)
        {
            process = Process.Start(srcPath);

            process.WaitForInputIdle();
            int times = 0;
            while (process.MainWindowHandle == null || process.MainWindowHandle == IntPtr.Zero)
            {
                Thread.Sleep(1000);
                if (times > 5 && process.Handle != IntPtr.Zero)
                {
                    break;
                }
                times++;

            }
        }

        public void Stop()
        {
            //process.CloseMainWindow();
            //process.Close();
            for (int i = 0; i < pid.Count;i++ )
            {
                Process p = Process.GetProcessById((int)pid[i]);
                p.Kill();
            }
            process.Kill();
        }

        #region MyRegion SendCtrlKey
        //public static void SendCtrlKey(bool isKeyDown)
        //{

        //    if (!isKeyDown)
        //    {

        //        keybd_event(17, MapVirtualKey(17, 0), 0x2, 0);//Up CTRL key

        //    }

        //    else
        //    {

        //        keybd_event(17, MapVirtualKey(17, 0), 0, 0); //Down CTRL key

        //    }

        //} 
        #endregion

        public void KeyDown(VirtualKeys key)
        {
            keybd_event(Convert.ToUInt32(key), MapVirtualKey((uint)key, 0), 0, 0);
        }

        public void KeyUp(VirtualKeys key)
        {
            keybd_event(Convert.ToUInt32(key), MapVirtualKey((uint)key, 0), 0, 0);
        }
    }
}
