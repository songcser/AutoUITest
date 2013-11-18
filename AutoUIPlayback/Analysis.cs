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
        private static extern int SendMessage(IntPtr hWnd,int Msg,IntPtr wParam,IntPtr lParam);
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(
            IntPtr hWnd,        // 信息发往的窗口的句柄
            int Msg,            // 消息ID
            int wParam,         // 参数1
            ref  COPYDATASTRUCT lParam  //参数2
        );
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(
            IntPtr hWnd,        // 信息发往的窗口的句柄
            int Msg,            // 消息ID
            int wParam,         // 参数1
            int lParam            // 参数2
        );
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(
            IntPtr hWnd,        // 信息发往的窗口的句柄
            int Msg,            // 消息ID
            int wParam,         // 参数1
            ref My_lParam lParam //参数2
        );
        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref int lpdwProcessId);

        const int MOUSEEVENTF_MOVE = 0x0001;

        const int MOUSEEVENTF_LEFTDOWN = 0x0002;

        const int MOUSEEVENTF_LEFTUP = 0x0004;

        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;

        const int MOUSEEVENTF_RIGHTUP = 0x0010;

        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;

        const int MOUSEEVENTF_MIDDLEUP = 0x0040;

        const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        //按下鼠标左键
        public static int WM_LBUTTONDOWN = 0x201;
        //释放鼠标左键
        public static int WM_LBUTTONUP = 0x202;
        //双击鼠标左键
        public static int WM_LBUTTONDBLCLK = 0x203;
        //按下鼠标右键
        public static int WM_RBUTTONDOWN = 0x204;
        //释放鼠标右键
        public static int WM_RBUTTONUP = 0x205;
        //双击鼠标右键
        public static int WM_RBUTTONDBLCLK = 0x206;

        private static AutomationElement mainWindow = null;
        private static IntPtr mainHandle = IntPtr.Zero;
 //       private static AutomationElement valueElement = null;
        
        //public AutomationElement MainWindow
        //{
        //    get { return mainWindow; }
        //    set { mainWindow = value; }
        //}
        public Analysis()
        {
            pid = new List<int>();
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
                
                if (strList.Count==5)
                {
                    ClickLeftMouse(strList[1], strList[2],strList[3]);
                }
                else if (strList.Count == 6)
                {
                    ClickLeftMouse(strList[1],strList[2],strList[3],strList[4]);
                }
                else
                {
                    return false;
                }
                
            }
            else if ("RightClick " == strList[0])
            {
                if (strList.Count == 5)
                {
                    ClickRightMouse(strList[1], strList[2], strList[3]);
                }
                else if (strList.Count == 6)
                {
                    ClickRightMouse(strList[1], strList[2], strList[3], strList[4]);
                }
                else
                {
                    return false;
                }
            }
            else if ("DoubleClick " == strList[0])
            {
                if (strList.Count == 5)
                {
                    DoubleMouseDown(strList[1], strList[2], strList[3]);
                }
                else if (strList.Count == 6)
                {
                    DoubleMouseDown(strList[1], strList[2], strList[3], strList[4]);
                }
                else
                {
                    return false;
                }
                
            }
            else if ("Stop" == strList[0])
            {
                Stop();
                return false;
            }
            else if ("SetFocus "==strList[0])
            {
                SetFocus(strList[1], strList[2],strList[3]);
            }
            else if ("SendKey " == strList[0])
            {
                Thread.Sleep(500);
                SendKey(strList[1]);
            }
            else if ("KeyDown " == strList[0])
            {
                Thread.Sleep(500);
                KeyDown(strList[1]);
            }
            else if ("KeyUp " == strList[0])
            {
                Thread.Sleep(500);
                KeyUp(strList[1]);
            }
            else if ("MenuOpened " == strList[0])
            {
                if (strList.Count == 5)
                {
                    ClickLeftMouse(strList[1], strList[2], strList[3]);
                }
                else if (strList.Count == 6)
                {
                    ClickLeftMouse(strList[1], strList[2], strList[3], strList[4]);
                }
                else
                {
                    return false;
                }
                //ClickLeftMouse(strList[1], strList[2], strList[3]);
            }
            else if ("WindowCreate " == strList[0])
            {
                WaitWindow(strList[1]);
            }
            else if ("Activate " == strList[0])
            {
                ActivateWindow(strList[1], strList[2]);
            }
            else if ("LeftMouseDown " == strList[0])
            {
                if (strList.Count == 5)
                {
                    ClickLeftMouse(strList[1], strList[2], strList[3]);
                }
                else if (strList.Count == 6)
                {
                    ClickLeftMouse(strList[1], strList[2], strList[3], strList[4]);
                }
                else
                {
                    return false;
                }
            }
            
            return true;
        }

        public void ActivateWindow(string name, string type)
        {
            //Thread.Sleep(200);
            IntPtr handle = FindWindow(null, name);

            while (handle == IntPtr.Zero || handle == null)
            {
                handle = FindWindow(null, name);
                Thread.Sleep(200);
            }
            mainHandle = handle;
            mainWindow = AutomationElement.FromHandle(handle);
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
                Thread.Sleep(200);
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
        }

        public void ClickLeftMouse(string name, string type,string autoId)
        {
            AutomationElement element = FindElement(name, type,autoId);

            if (element == null)
            {
                return;
                throw new NullReferenceException(string.Format("Element with AutomationId '{0}' and Name '{1}' can not be find.",

                    element.Current.AutomationId, element.Current.Name));
            }
            try
            {
                AnalysisType(element);
            }
            catch (System.Exception ex)
            {
                string str = ex.Message;
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

        public void ClickLeftMouse(string name, string type, string pname, string ptype)
        {
            AutomationElement element = FindElement(name, type, pname,ptype);

            if (element == null)
            {
                return;    
            }
            
            try
            {
                AnalysisType(element);
            }
            catch (System.Exception ex)
            {
                string str = ex.Message;
            }
        }

        public void ClickRightMouse(string name,string type,string autoId)
        {

            AutomationElement element = FindElement( name, type,autoId);

            if (element == null)
            {

                throw new NullReferenceException(string.Format("Element with AutomationId '{0}' and Name '{1}' can not be find.",

                    element.Current.AutomationId, element.Current.Name));

            }

            Rect rect = element.Current.BoundingRectangle;

            int IncrementX = (int)(rect.Left + rect.Width / 2);
 
            int IncrementY = (int)(rect.Top + rect.Height / 2);

            //element.SetFocus();
            //Win32Api api = new Win32Api();
            //Win32Api.MouseRightKeyDown(IncrementX, IncrementY);
            //Win32Api.MouseRightKeyUp(IncrementX, IncrementY);
            //Point p = element.GetClickablePoint();
            //POINTAPI point = new POINTAPI();
            //point.x = (uint)p.X;
            //point.y = (uint)p.Y;
            //ScreenToClient(mainHandle, ref point);
            //IntPtr handle = ChildWindowFromPoint(mainHandle, point);
//             //handle = (IntPtr)element.Current.NativeWindowHandle;
//             AutomationElement elem = AutomationElement.FromHandle(handle);
//            //Make the cursor position to the element.
            
            //var value = ((int)p.X) << 16 + (int)p.Y;
            //PostMessage(handle, WM_RBUTTONDOWN, 0, value);
            //PostMessage(handle, WM_RBUTTONUP, 0, value);
            //Thread.Sleep(200);
            //var processId = element.GetCurrentPropertyValue(AutomationElement.ProcessIdProperty);
            //var window = AutomationElement.RootElement.FindFirst(
            //    TreeScope.Children,
            //    new PropertyCondition(AutomationElement.ProcessIdProperty,
            //                          processId));
            //var handle = window.Current.NativeWindowHandle;
            //SetCursorPos((int)p.X,(int)p.Y);
            SetCursorPos(IncrementX, IncrementY);
////             SendMessage(mainHandle, MOUSEEVENTF_RIGHTDOWN, IntPtr.Zero, (IntPtr)(IncrementY * 65536 + IncrementX));
////             SendMessage(mainHandle, MOUSEEVENTF_RIGHTUP, IntPtr.Zero, (IntPtr)(IncrementY * 65536 + IncrementX));
            //SendMessage(mainHandle, WM_RBUTTONDOWN, IntPtr.Zero, (IntPtr)(value));
            //SendMessage(mainHandle, WM_RBUTTONUP, IntPtr.Zero, (IntPtr)(value));

//             SendMessage(handle, WM_RBUTTONDOWN, (IntPtr)IncrementX, (IntPtr)IncrementY);
//             SendMessage(handle, WM_RBUTTONUP, (IntPtr)IncrementX, (IntPtr)IncrementY);
            //Make the left mouse down and up.

            mouse_event(MOUSEEVENTF_RIGHTDOWN, IncrementX, IncrementY, 0, UIntPtr.Zero);

            mouse_event(MOUSEEVENTF_RIGHTUP, IncrementX, IncrementY, 0, UIntPtr.Zero);

        }

        public void ClickRightMouse(string name, string type, string pname, string ptype)
        {
            //ClickRightMouse(name, type, "");
            AutomationElement element = FindElement(name, type, pname, ptype);

            if (element == null)
            {
                return;
                throw new NullReferenceException(string.Format("Element with AutomationId '{0}' and Name '{1}' can not be find.",

                    element.Current.AutomationId, element.Current.Name));
            }

            Point p = element.GetClickablePoint();
            SetCursorPos((int)p.X, (int)p.Y);

            mouse_event(MOUSEEVENTF_RIGHTDOWN, (int)p.X, (int)p.Y, 0, UIntPtr.Zero);

            mouse_event(MOUSEEVENTF_RIGHTUP, (int)p.X, (int)p.Y, 0, UIntPtr.Zero);
        }

        public void DoubleMouseDown(string name, string type,string autoId)
        {
            AutomationElement element = FindElement( name, type,autoId);

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

            int IncrementX = (int)(rect.Left + rect.Width / 2);

            int IncrementY = (int)(rect.Top + rect.Height / 2);

            //Make the cursor position to the element.

            SetCursorPos(IncrementX, IncrementY);

            //Make the left mouse down and up.

            mouse_event(MOUSEEVENTF_LEFTDOWN, IncrementX, IncrementY, 0, UIntPtr.Zero);

            mouse_event(MOUSEEVENTF_LEFTUP, IncrementX, IncrementY, 0, UIntPtr.Zero);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTDOWN, IncrementX, IncrementY, 0, UIntPtr.Zero);

            mouse_event(MOUSEEVENTF_LEFTUP, IncrementX, IncrementY, 0, UIntPtr.Zero);
        }

        public void DoubleMouseDown(string name, string type, string pname, string ptype)
        {
            AutomationElement element = FindElement(name, type, pname, ptype);

            if (element == null)
            {
                return;
                throw new NullReferenceException(string.Format("Element with AutomationId '{0}' and Name '{1}' can not be find.",

                    element.Current.AutomationId, element.Current.Name));
            }
            Rect rect = element.Current.BoundingRectangle;

            int IncrementX = (int)(rect.Left + rect.Width / 2);

            int IncrementY = (int)(rect.Top + rect.Height / 2);

            //Make the cursor position to the element.

            SetCursorPos(IncrementX, IncrementY);

            //Make the left mouse down and up.

            mouse_event(MOUSEEVENTF_LEFTDOWN, IncrementX, IncrementY, 0, UIntPtr.Zero);

            mouse_event(MOUSEEVENTF_LEFTUP, IncrementX, IncrementY, 0, UIntPtr.Zero);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTDOWN, IncrementX, IncrementY, 0, UIntPtr.Zero);

            mouse_event(MOUSEEVENTF_LEFTUP, IncrementX, IncrementY, 0, UIntPtr.Zero);
        }

        public void AnalysisType(AutomationElement element)
        {
            ControlType type = element.Current.ControlType;
            
            if (type == ControlType.RadioButton||type == ControlType.CheckBox||type == ControlType.ListItem)
            {
                SelectionItemPattern selectionItemPattern = GetSelectionItemPattern(element);
                selectionItemPattern.Select();
            }
            else if (type == ControlType.TabItem)
            {
                SelectionItemPattern selectionItemPattern = GetSelectionItemPattern(element);
                selectionItemPattern.Select();
            }
            else if (type == ControlType.Button || type == ControlType.MenuBar || type == ControlType.MenuItem)
            {
                InvokePattern invokePattern = element.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                invokePattern.Invoke();
            }
            else if (type == ControlType.ComboBox)
            {
                ExpandCollapsePattern currentPattern = GetExpandCollapsePattern(element);
                currentPattern.Expand();
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
            else if (type == ControlType.Edit)
            {
                element.SetFocus();
                Thread.Sleep(200);
            }
        }

        public static ExpandCollapsePattern GetExpandCollapsePattern(AutomationElement element)
        {
            object currentPattern;
            if (!element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out currentPattern))
            {
                throw new Exception(string.Format("Element with AutomationId '{0}' and Name '{1}' does not support the ExpandCollapsePattern.",
                    element.Current.AutomationId, element.Current.Name));
            }
            return currentPattern as ExpandCollapsePattern;
        }

        public static SelectionItemPattern GetSelectionItemPattern(AutomationElement element)
        {
            object currentPattern;
            if (!element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out currentPattern))
            {
                throw new Exception(string.Format("Element with AutomationId '{0}' and Name '{1}' does not support the SelectionItemPattern.",
                    element.Current.AutomationId, element.Current.Name));
            }
            return currentPattern as SelectionItemPattern;
        }

        public void SendKey(string key)
        {
            Keys k = (Keys)Enum.Parse(typeof(Keys), key, true);

            Win32Api.KeyDown(k);
            Win32Api.KeyDown(k);
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

        public void SetFocus(string name, string type,string autoId)
        {
            AutomationElement element = FindElement(name, type,autoId);

            if (element == null)
            {

                throw new NullReferenceException(string.Format("Element with AutomationId '{0}' and Name '{1}' can not be find.",

                    element.Current.AutomationId, element.Current.Name));

            }

            element.SetFocus();
        }

        public static AutomationElement FindWindowByProcessId(int processId)
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

        public static AutomationElement FindElement( string name, string type,string autoId)
        {

            //AutomationElement aeForm = FindWindowByProcessId(processId);

            Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
            //Condition idcondition = new PropertyCondition(AutomationElement.ProcessIdProperty, process.Id);
            Condition nameCondition = new PropertyCondition(AutomationElement.NameProperty, name);

            Condition conditions = new AndCondition(nameCondition, condition2, condition1);
            if (type != "")
            {
                Condition typeCondition = new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, type);
                conditions = new AndCondition(conditions,typeCondition);
            }
            if (autoId != "")
            {
                Condition autoIdCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, autoId);
                conditions = new AndCondition(conditions, autoIdCondition);
            }
            

            if (conditions==null)
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
                if (count>10)
                {
                    break;
                }
                Thread.Sleep(200);
                count++;
            }
            
            return null;
        }

        public AutomationElement FindElement(string name, string type, string pname, string ptype)
        {
            AutomationElementCollection collection = FindElements(name, type);
            if (collection == null)
            {
                return null;
            }
            if (collection.Count == 1)
            {
                return collection[0];
            }
            else if (collection.Count == 0)
            {
                AutomationElement element = FindElement(name, type, "");
                if (element != null)
                {
                    return element;
                }
                else
                {
                    return null;
                }
            }
            for (int i = 0; i < collection.Count; i++)
            {
                AutomationElement element = collection[i];
                TreeWalker walker = TreeWalker.ControlViewWalker;
                AutomationElement parent = walker.GetParent(element);
                if (parent != null)
                {
                    string n = parent.Current.Name;
                    string t = parent.Current.LocalizedControlType;
                    if (n == pname && t == ptype)
                    {
                        return element;
                    }
                }
            }
            return null;
        }

        public AutomationElementCollection FindElements(string name, string type)
        {
            Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
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
                    if (name == "5 Deg.")
                    {

                    }
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

        public static AutomationElement FindElementById(int processId, string automationId)
        {

            AutomationElement aeForm = FindWindowByProcessId(processId);

            AutomationElement tarFindElement = aeForm.FindFirst(TreeScope.Descendants,

            new PropertyCondition(AutomationElement.AutomationIdProperty, automationId));

            return tarFindElement;

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
