using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.Management;

namespace GlobalHooksTest
{
    class ElementManage
    {
        private static List<AutomationElement> elementList = null;
        //private static List<AutomationElement> childElemList = null;
        //private static AutomationElementCollection elements = null;

        //private CacheRequest fetchRequest;
        private bool isFocusable = false;
        public bool IsFocusable
        {
            get { return isFocusable; }
            set { isFocusable = value; }
        }
        private static StringBuilder log;
        private static System.Windows.Rect rect;

        private const int LB_GETTEXT = 0x0189; 

        private List<Process> proceses = null;
        private Process mainProcess = null;
        private List<IntPtr> handleList = null;
        
        //private AutomationEventHandler selectionHandler = null;
        //private AutomationEventHandler openHandler = null;
        //private static Process process = null;
        //private AutomationElement comboBoxElem;
        AutomationFocusChangedEventHandler focusHandler = null;

        public delegate bool CallBack(IntPtr hwnd, int lParam);

        //AutomationElement ElementSubscribeButton;
        //AutomationEventHandler UIAeventHandler;
        private System.Timers.Timer timer;
        //private Thread workerThread;
        AutomationEventHandler menuOpenedHandler;
        AutomationEventHandler menuClosedHandler;

        public delegate void CallBackMessage(string message);
        public event CallBackMessage SendMessageBack;
        private static AutomationElement targetApp;
        private AutomationElement currentElement;
        private AutomationElement preElement;
        private AutomationElement openedMenuElement;
        private AutomationElement focusElement;
        //private static AutomationElement drawElement;
        private static string elementInfo;
        private static string preInfo;
        private static int clickCount = 0;
        private static bool clickFlag = false;
        private static bool mouseDownFlag = false;
        private static bool mouseUpFlag = false;
        private static bool mouseMoveFlag = false;
        private static int preX = 0;
        private static int preY = 0;
        private string menuInfo;
        private string windowName;
        private bool activateFlag = false;
        private bool hookFlag = false;

        private Keys keyDown;
        private bool keyDownFlag = false;

        [DllImport("user32.dll")]
        public static extern int EnumWindows(CallBack lpfn, int lParam);
        //public static CallBack callBackEnumWindows = new CallBack(WindowProcess)
        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint lpdwProcessId);
        [DllImport("user32.dll")]
        private static extern uint RealGetWindowClass(IntPtr hWnd, StringBuilder pszType, uint cchType);
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle rect);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", EntryPoint = "GetDoubleClickTime")]
        public static extern int GetDoubleClickTime();
        [DllImport("User32.dll")]
        public extern static System.IntPtr GetDC(System.IntPtr hWnd);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SendMessage(int hwnd, int msg, int wParam, StringBuilder lParam);

        public ElementManage()
        {
            //fetchRequest = new CacheRequest();
            elementList = new List<AutomationElement>();
            handleList = new List<IntPtr>();
            //childElemList = new List<AutomationElement>();
            proceses = new List<Process>();
            log = new StringBuilder();
            rect = new System.Windows.Rect();

            timer = new System.Timers.Timer();
            timer.Interval = GetDoubleClickTime();
            timer.AutoReset = false;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnSetTimer);

            //UIAeventHandler = new AutomationEventHandler(OnUIAutomationEvent);
            menuClosedHandler = new AutomationEventHandler(OnMenuClosed);
            menuOpenedHandler = new AutomationEventHandler(OnMenuOpened);
        }

        public void SetHookFlag(bool flag)
        {
            hookFlag = flag;
        }

        public bool isTopWindow()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid = 0;
            GetWindowThreadProcessId(hwnd, ref pid);
            if (pid == mainProcess.Id)
            {
                return true;
            }
            for (int i = 0; i < proceses.Count;i++ )
            {
                if (pid == proceses[i].Id)
                {
                    return true;
                }
            }
            return false;
        }

        public bool addHandler(IntPtr handler)
        {
            
            for (int i = 0; i < handleList.Count; i++)
            {
                if (handleList[i] == handler)
                {
                    return false;
                }
            }
            handleList.Add(handler);
            return true;
        }

        public string AddMainElementFromHandle(IntPtr handle)
        {
            
            string str = "";
            try
            {
                Thread thread = new Thread(() =>
                {
                    AutomationElement element = GetElementFromHandle(handle);
                    if (IsOwnProcess(element))
                    {
                        targetApp = element;

                        str = GetCurrentElementInfo(element);
                    }
                        

                });
                thread.Start();
                thread.Join();

                return str;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        public void OnWindowOpenOrClose(object src, AutomationEventArgs e)
        {
            if (e.EventId == WindowPattern.WindowOpenedEvent)
            {
                try
                {
                    AutomationElement sourceElement = src as AutomationElement;
                }
                catch (System.Exception ex)
                {
                	
                }
            }
        }

        public string GetSelectValue(IntPtr hwnd)
        {
            AutomationElement elem = AutomationElement.FromHandle(hwnd);
            if (elem==null)
            {
                return "";
            }
            SelectionPattern selectPattern = (SelectionPattern)elem.GetCurrentPattern(SelectionPattern.Pattern);
            //selectPattern.Select();
            AutomationElement[] elems = selectPattern.Current.GetSelection();
            string value = "";
            for (int i = 0; i < elems.Length;i++ )
            {
                value = elems[i].Current.Name;
            }
            string str = string.Format("SelectComboBox: \"{0}\" \"{1}\"",elem.Current.Name,value);
            return str;
        }

        public string GetWindowName(IntPtr Hwnd)
        {
            // This function gets the name of a window from its handle
            StringBuilder Title = new StringBuilder(256);
            GetWindowText(Hwnd, Title, 256);

            return Title.ToString().Trim();
        }

        public string GetWindowClass(IntPtr Hwnd)
        {
            // This function gets the name of a window class from a window handle
            StringBuilder Title = new StringBuilder(256);
            RealGetWindowClass(Hwnd, Title, 256);

            return Title.ToString().Trim();
        }

        //public void UpdateCache()
        //{
        //    for (int i = 0; i < childElemList.Count;i++ )
        //    {
        //        childElemList[i] = childElemList[i].GetUpdatedCache(fetchRequest);
        //    }
        //}

        public void StartProcess(string strPath)
        {
            if (strPath==""||strPath==null)
            {
                MessageBox.Show("please input application path");
                throw new Exception("");
                
            }
            mainProcess = Process.Start(strPath);
            
//             Thread.Sleep(500);
//             
//             mainProcess.WaitForInputIdle();
            
            
        }

        //public void addChildElement(AutomationElement element)
        //{
        //    Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
        //    Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
        //    //Condition condition = new PropertyCondition(AutomationElement.ProcessIdProperty, pId);
        //    //AutomationElement element = AutomationElement.FromHandle(handle);
        //    if (element!=null)
        //    {
        //        using (fetchRequest.Activate())
        //        {
        //            AutomationElementCollection tElemList = element.FindAll(TreeScope.Subtree, new AndCondition( condition1, condition2));
        //            //elements = aeForm.FindAll(TreeScope.Subtree, new AndCondition(condition1, condition2, condition));
        //            ///elements.CopyTo(temp, temp.Count);
        //            for (int i = 0; i < tElemList.Count; i++)
        //            {
        //                childElemList.Add(tElemList[i]);
        //            }
        //        }
        //    }
            
        //}

        public bool isEnable(IntPtr hwnd)
        {
            uint processId = 0;
            
            GetWindowThreadProcessId(hwnd, ref processId);

            if (processId == mainProcess.Id)
            {
                return true;
            }
            foreach (Process process in proceses)
            {
                if (process.Id == processId)
                {
                    return true;
                }
            }
            return false;
        }

        public string GetCurrentElementInfo(AutomationElement element)
        {

            //ControlType controlType = GetElementType(element);
            AutomationElement parent = GetParentElement(element);
            ControlType pt = GetElementType(parent);
            if (pt == ControlType.ComboBox)
            {
                return GetCurrentElementInfo(parent);
            }
            

            StringBuilder str = new StringBuilder("\"");
           
            str.Append(GetElementName(element)).Append("\"");
           
            string type = element.Current.LocalizedControlType;
           
            str.Append(type).Append("\"");

            bool enable = element.Current.IsEnabled;
            if (enable)
            {
                str.Append("true\"");
            }
            else
            {
                str.Append("false\"");
            }

            string autoId = GetElementAutomationId(element);

            if (autoId=="")
            {
                //TreeWalker walker = TreeWalker.ControlViewWalker;
                
                //AutomationElement parent = walker.GetParent(element);
                if (parent!=null)
                {
                    if (parent == AutomationElement.RootElement)
                    {
                        str.Append("\"");
                    }
                    else
                    {
                        str.Append(GetElementName(parent)).Append("\"");
                        ControlType ptype = GetElementType(parent);
                        string plct = parent.Current.LocalizedControlType;
                        if (ptype == ControlType.List)
                        {
                            plct = "list";
                        }

                        str.Append(plct).Append("\"");
                    }
                }
            }
            else
            {
                str.Append(autoId).Append("\"");
            }
            
            System.Windows.Rect rect = element.Current.BoundingRectangle;
            int offsetX = preX - (int)rect.Left;
            int offsetY = preY - (int)rect.Top;
            str.Append(offsetX).Append("\"").Append(offsetY).Append("\"");
            
            //AnalyseType(controlType, element);
            return str.ToString();
        }

        public string GetElementAutomationId(AutomationElement element)
        {
            string autoIdString = "";
            try
            {
                object autoIdNoDefault = element.GetCurrentPropertyValue(AutomationElement.AutomationIdProperty, true);
                if (autoIdNoDefault == AutomationElement.NotSupported)
                {
                    // TODO Handle the case where you do not wish to proceed using the default value.
                    return "";
                }
                else
                {
                    autoIdString = autoIdNoDefault as string;
                }
            }
            catch (System.Exception ex)
            {
                return "";
            }
            
            return autoIdString;
        }
        
        public bool IsEnablePoint(int x, int y)
        {
            if (isFocusable)
            {
                if (x<rect.Left||x>rect.Right||y<rect.Top||y>rect.Bottom)
                {
                    return true;
                }
            }
            return false;
        }

        public string GetMenuElementByPoint(int x, int y)
        {
            
            System.Windows.Point wpt = new System.Windows.Point(x,y);
            AutomationElement autoElem;
            string str = "";
            try
            {

                Thread thread = new Thread(() =>
                {
                    autoElem = AutomationElement.FromPoint(wpt);
                    ControlType controlType = GetElementType(autoElem);
                    if (controlType == ControlType.MenuItem)
                    {
                        string name = GetElementName(autoElem);
                        str = "\"" + name + "\"\"menu item\"";
                        rect = (System.Windows.Rect)autoElem.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
                    }


                });
                thread.Start();
                thread.Join();

                return str;

            }
            catch (System.Exception ex)
            {
                //AutomationElement targetElem = null;

                string m = ex.Message;

                return null;
            }
        }

        private bool IsOwnProcess(AutomationElement element)
        {
            if (element == null)
            {
                //AddText("Automation null");
                return false;
            }
            
            try
            {
                int processId;
                object processIdentifierNoDefault = element.GetCurrentPropertyValue(AutomationElement.ProcessIdProperty, true);
                if (processIdentifierNoDefault == AutomationElement.NotSupported)
                {
                    // TODO Handle the case where you do not wish to proceed using the default value.
                    return false;
                }
                else
                {
                    processId = (int)processIdentifierNoDefault;
                    if (isProcessId(processId))
                    {
                        return true;
                    }
                    else
                    {
                        return isChildElement(element, processId);
                    }
                    
                }
            }
            catch (System.Exception ex)
            {
                string m = ex.Message;
            }
            return false;
        }

        private bool isChildElement(AutomationElement element,int processId)
        {
            TreeWalker walker = TreeWalker.ControlViewWalker;
            AutomationElement elementParent;
            AutomationElement node = element;
            if (node == AutomationElement.RootElement) return false;
            do
            {
                elementParent = walker.GetParent(node);
                if (elementParent == AutomationElement.RootElement) 
                    return false;
                if (elementParent == targetApp)
                {
                    Process process = Process.GetProcessById((int)processId);
                    proceses.Add(process);
                    return true;
                }
                node = elementParent;
            }
            while (true);
            
        }

        private bool isProcessId(int processId)
        {
            bool flag = false;
            
            if (processId != mainProcess.Id)
            {
                for (int i = 0; i < proceses.Count; i++)
                {
                    if (processId == proceses[i].Id)
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    return false;
                }
            }
            return true;
        }

        private string GetElementName(AutomationElement element)
        {

            try
            {
                string name = element.GetCurrentPropertyValue(AutomationElement.NameProperty, true) as string;
                return name;
            }
            catch (System.Exception ex)
            {
                string m = ex.Message;
            }
            return null;
        }

        private ControlType GetElementType(AutomationElement element)
        {
            try
            {
                object controlTypeNoDefault = element.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty, true);
                if (controlTypeNoDefault == AutomationElement.NotSupported)
                {
                    return null;
                }
                return controlTypeNoDefault as ControlType;
            }
            catch (System.Exception ex)
            {
                string m = ex.Message;
            }
            return null;
        }

        public string GetCachedElementInfo(AutomationElement element)
        {
            if (element == null)
            {
                //AddText("Automation null");
                return null;
            }
            string str = "\"";
            string name = "";
            if (element.Cached.Name != null)
            {
                name = element.Cached.Name;
            }
            
            else return null;

            str += name + "\"";
            //string type = "";
            
            object controlTypeNoDefault = element.GetCachedPropertyValue(AutomationElement.ControlTypeProperty, true);
            if (controlTypeNoDefault == AutomationElement.NotSupported)
            {
                return null;
            }
            ControlType controlType = controlTypeNoDefault as ControlType;
            //str += AnalyseType(controlType, element);
            
            str += " \"" + controlType.LocalizedControlType + "\"";

            return str;
        }

        private void AnalyseType(ControlType controlType,AutomationElement element)
        {
            
            if (controlType == ControlType.MenuItem)
            {
                isFocusable = true;
                rect = (System.Windows.Rect)element.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
                //SubscribeToInvoke(element);
                //object currentPattern;
                //if (!element.TryGetCurrentPattern(InvokePattern.Pattern, out currentPattern))
                //{
                //    throw new Exception(string.Format("Element with AutomationId '{0}' and Name '{1}' does not support the InvokePattern.",
                //    element.Current.AutomationId, element.Current.Name));
                //}
                //InvokePattern curPattern = currentPattern as InvokePattern;
                //curPattern.Invoke();
            }
            else
            {
                isFocusable = false;
            }
            
            
        }

        private void SelectionHandler(object sender, AutomationEventArgs e)
        {
            AutomationElement element = sender as AutomationElement;
            // TODO: event handling
        }

        //private void OnStructureChanged(object sender, StructureChangedEventArgs e)
        //{
        //    AutomationElement element = sender as AutomationElement;

        //    if (e.StructureChangeType == StructureChangeType.ChildAdded)
        //    {
        //        using (fetchRequest.Activate())
        //        {
        //            childElemList.Add(element);
        //        }
        //        addChildElement(element);
        //    }
        //}

        public string GetElementFromPoint(Point point)
        {
            System.Windows.Point wpt = new System.Windows.Point(point.X, point.Y);
            
            string str = "";
            try
            {
                
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        AutomationElement autoElem = AutomationElement.FromPoint(wpt);
                        if (IsOwnProcess(autoElem))
                        {
                            str = GetCurrentElementInfo(autoElem);

                            currentElement = autoElem;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        str = null;
                    }
                    
                    
                });
                thread.Start();
                thread.Join();

                return str;
                
            }
            catch (System.Exception ex)
            {
                //AutomationElement targetElem = null;

                string m = ex.Message;

                //targetElem = GetElementsByPoint(point);
                
                //return GetCachedElementInfo(targetElem);
                return null;
            }
        }

        public AutomationElement GetElementFromHandle(IntPtr handle)
        {
            uint processId = 0;
            bool flag = false;
            GetWindowThreadProcessId(handle, ref processId);
            if (processId != mainProcess.Id)
            {
                for (int i = 0; i < proceses.Count;i++ )
                {
                    if (proceses[i].Id == processId)
                    {
                        flag = true;
                    }
                }
            }
            else
            {
                flag = true;
            }
            if (!flag)
            {
                return null;
            }
            try
            {
                AutomationElement element = AutomationElement.FromHandle(handle);
                if (element == null)
                {
                    
                    return null;
                }
                else
                {
                    return element;
                }
            }
            catch (System.Exception ex)
            {
                
            }
            
            return null;
        }
        
        public AutomationElement GetElementFromFocus()
        {
            AutomationElement element = AutomationElement.FocusedElement;
            if (element == null)
            {
                return null;
            }
            TreeWalker walker = TreeWalker.ControlViewWalker;
            AutomationElement elementParent;
            AutomationElement node = element;
            if (node == AutomationElement.RootElement) return node;

            do
            {
                elementParent = walker.GetParent(node);
                object controlTypeNoDefault = elementParent.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty, true);
                if (controlTypeNoDefault == AutomationElement.NotSupported)
                {
                    return null;
                }
                ControlType controlType = controlTypeNoDefault as ControlType;
                //if (elementParent.Current.AutomationId == "MaterialLibraryMan")
                //{
                //    Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
                //    Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
                //    AutomationElementCollection tElemList = element.FindAll(TreeScope.Subtree, new AndCondition(condition2, condition1));
                //    break;
                //}
                if (controlType == ControlType.Window || elementParent == AutomationElement.RootElement)
                {
                    Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
                    Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
                    //Condition condition = new PropertyCondition(AutomationElement.ProcessIdProperty, process.Id);
                    //using (fetchRequest.Activate())
                    //{
                    //    AutomationElementCollection tElemList = elementParent.FindAll(TreeScope.Subtree, new AndCondition(condition1, condition2));
                    //    for (int i = 0; i < tElemList.Count; i++)
                    //    {
                    //        childElemList.Add(tElemList[i]);
                    //    }
                    //}
                    //Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
                    //Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
                    //AutomationElementCollection tElemList = element.FindAll(TreeScope.Subtree, new AndCondition(condition2, condition1));
                    break;
                }
                //if (elementParent.Current.Name == "Material Library Manager")
                //{
                //    break;
                //}
                node = elementParent;
            }
            while (true);
            
            return elementParent;
        }

        //public void SubscribeToMenuClosed()
        //{
            
        //    //Automation.AddAutomationFocusChangedEventHandler(focusHandler);
        //    Automation.AddAutomationEventHandler(AutomationElement.MenuClosedEvent,
        //            AutomationElement.RootElement,
        //            TreeScope.Descendants,
        //            UIAeventHandler);
            
        //}

        public void RegisterForEvents()
        {
            if (targetApp != null)
            {
                Automation.AddAutomationEventHandler(AutomationElement.MenuOpenedEvent,
                    targetApp,
                    TreeScope.Descendants,
                    menuOpenedHandler);

                Automation.AddAutomationEventHandler(AutomationElement.MenuClosedEvent,
                    targetApp,
                    TreeScope.Descendants,
                    menuClosedHandler);

                //Automation.AddStructureChangedEventHandler(
                //    targetApp,
                //    TreeScope.Descendants,
                //    OnStructureChangedHandler);
                //AutomationPropertyChangedEventHandler handler = new AutomationPropertyChangedEventHandler(OnPropertyChangedHandler);
                //Automation.AddAutomationPropertyChangedEventHandler(
                //    targetApp,
                //    TreeScope.Descendants,
                //    handler,AutomationElement.IsEnabledProperty);
            }
            
        }

        private void OnPropertyChangedHandler(object src, AutomationEventArgs e)
        {
            AutomationElement element = src as AutomationElement;

            try
            {
                string name = GetElementName(element);
                string type = element.Current.LocalizedControlType;
                if (name != "" && type != "")
                {
                    //AddText("StructureChange|" + name + "|" + type);
                    SendMessageBack("PropertyChange|\"" + name + "\"" + type + "\"\"");
                }
            }
            catch (System.Exception ex)
            {

            }
        }

        private void OnStructureChangedHandler(object src, AutomationEventArgs e)
        {
            AutomationElement element = src as AutomationElement;
            try
            {
                string name = GetElementName(element);
                string type = element.Current.LocalizedControlType;
                if (name != "" && type != "")
                {
                    //AddText("StructureChange|" + name + "|" + type);
                    //GetMessage("StructureChange|\"" + name + "\"" + type+"\"\"");
                }
            }
            catch (System.Exception ex)
            {
            	
            }
        }

        private void OnMenuClosed(object src, AutomationEventArgs e)
        {
            //AutomationElement element = src as AutomationElement;
            //Feedback("MenuClosed event: ");
            //AddText("MenuClosed ");
            if (openedMenuElement!=null)
            {
                openedMenuElement = null;
            }
//             string message = "";
//             string name = GetElementName(element);
//             if (name == "")
//             {
//                 return;
//             }
//             message += "\"" + GetElementName(element) + "\"menu item\"\"";
//             SendMessage("CloseMenu|" + message);
        }

        private void OnMenuOpened(object src, AutomationEventArgs e)
        {
            if (mouseDownFlag||!hookFlag)
            {
                return;
            }
            AutomationElement element = src as AutomationElement;
            menuInfo = "";
            //Thread thread = new Thread(() =>
            //{
            //message = GetCurrentElementInfo(element);
            //string time = GetTime();
            if (openedMenuElement == null)
            {
                openedMenuElement = element;
            }
            else if (openedMenuElement == element)
            {
                return;
            }
            
            string name = GetElementName(element);
            if (name=="")
            {
                return;
            }
//             string autoId = element.Current.AutomationId;
//             if (autoId!=null||autoId!="")
//             {
//             }
            menuInfo = "\"" + GetElementName(element) + "\"menu item\"\"";
           // });
            //menuInfo = message;
            //Feedback("MenuOpened event: ");
            //AddText("OpenMenu|" + message);
            openedMenuElement = element;
            SendMessageBack("OpenMenu " + menuInfo);
        }

        private void OnFocusChange(object src, AutomationFocusChangedEventArgs e)
        {
            // TODO Add event handling code.
            // The arguments tell you which elements have lost and received focus.
            string message = "";
            Thread thread = new Thread(() =>
            {
                AutomationElement element = src as AutomationElement;

                if (IsOwnProcess(element))
                {
                    message = GetCurrentElementInfo(element);
                }
                

            });
            thread.Start();
            thread.Join();
            AddText("SetFocus|" + message);
        }

        public void UnsubscribeFocusChange()
        {
            if (focusHandler != null)
            {
                Automation.RemoveAutomationFocusChangedEventHandler(focusHandler);
                
            }
            //if (UIAeventHandler != null)
            //{
            //    Automation.RemoveAutomationEventHandler(UIAeventHandler);
            //}
        }

        public string GetNameFromHandle(IntPtr handle)
        {
            
            string str = "";
            try
            {
                Thread thread = new Thread(() =>
                {
                    #region MyRegion
                    //CacheRequest cacheRequest = new CacheRequest();
                    //cacheRequest.Add(AutomationElement.NameProperty);
                    //cacheRequest.Add(AutomationElement.IsEnabledProperty);
                    //cacheRequest.Add(AutomationElement.ControlTypeProperty);
                    ////cacheRequest.Add(SelectionItemPattern.SelectionContainerProperty);
                    //AutomationElement element ;
                    //using (cacheRequest.Activate())
                    //{
                    //    Condition cond = new PropertyCondition(AutomationElement.IsSelectionItemPatternAvailableProperty, true);
                    //    element = GetElementFromHandle(handle);
                    //}
                    ////str = GetCurrentElementInfo(element);
                    //str = GetCachedElementInfo(element); 
                    #endregion
                    AutomationElement element = GetElementFromHandle(handle);
                    //targetApp = element;
                    if (IsOwnProcess(element))
                    {
                        str = GetCurrentElementInfo(element);
                    }
                    
                });
                thread.Start();
                thread.Join();
                
                return str;
            }
            catch (System.Exception ex)
            {
                return null;
            }
            
        }
        
        public void ActivateWnd(IntPtr handler)
        {
            focusElement = null;
            string name = AddMainElementFromHandle(handler);
            if (name == null || name == "\"" || name == " " || name == "")
            {
                return;
            }
            if (mouseDownFlag)
            {
                windowName = name;
                activateFlag = true;
            }
            else
            {
                SendMessageBack("Activate " + name);
            }
        }

        //private AutomationElement GetElementsByPoint(Point point)
        //{
        //    AutomationElement targetElem = null;
           
        //    for (int i = 0; i < childElemList.Count; i++)
        //    {
        //        try
        //        {
        //            AutomationElement element = childElemList[i];
        //            System.Windows.Rect boundingRect = (System.Windows.Rect)element.GetCachedPropertyValue(AutomationElement.BoundingRectangleProperty);
        //            if (point.X > boundingRect.Left && point.X < boundingRect.Right && point.Y < boundingRect.Bottom && point.Y > boundingRect.Top)
        //            {
        //                targetElem = element;
        //                //return element;
        //            }
        //        }
        //        catch (System.Exception ex)
        //        {
        //            string exception = ex.Message;
        //        }

        //    }
        //    return targetElem;
        //}
        
        //public static bool WindowProcess(IntPtr hwnd, int lParam)
        //{
        //    //EnumChildWindows(hwnd, callBackEnumChildWindows, 0);
        //    StringBuilder title = new StringBuilder(200);
        //    int len;
        //    len = GetWindowText(hwnd, title, 200);
        //    //count++;
        //    uint processId = 0;
        //    //string name = process.MainWindowTitle;
        //    GetWindowThreadProcessId(hwnd, ref processId);
        //    if (processId == lParam && title.Length > 0)
        //    {
        //        AutomationElement mainElement = AutomationElement.FromHandle(hwnd);
        //        if (mainElement != null)
        //        {
        //            elementList.Add(mainElement);
        //        }

        //        //elementList.Add(mainElement);
        //    }
           
        //    return true;
        //}

        private bool hasProcessId(int processId)
        {
            for (int i = 0; i < proceses.Count;i++ )
            {
                Process process = proceses[i];
                if (process.Id == processId)
                {
                    //flag = false;
                    return true;
                }
            }
            if (processId != mainProcess.Id)
            {
                int count = proceses.Count;
                Process process = Process.GetProcessById((int)processId);

                Process parent = GetPrentProcessName(process);
                if (parent == null)
                {
                    return false;
                }
                if (parent.Id == mainProcess.Id)
                {
                    proceses.Add(process);
                    return true;
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (proceses[i].Id == parent.Id)
                        {
                            proceses.Add(process);
                            return true;
                        }
                    }
                }
            }
            else
            {
                return true;
            }
            return false;
        }

        public bool hasWindowsProcess(IntPtr hwnd)
        {
            uint processId = 0;
            //bool flag = true;
            GetWindowThreadProcessId(hwnd, ref processId);

            return hasProcessId((int)processId);
        }
        
        //private void waitProcess(IntPtr  hwnd)
        //{
            
        //    if (hwnd == IntPtr.Zero)
        //    {
        //        EnumWindows(callBackEnumWindows, 0);
        //    }
        //    else
        //    {
        //        try
        //        {
        //            AutomationElement element = AutomationElement.FromHandle(hwnd);
        //            Automation.AddStructureChangedEventHandler(element, TreeScope.Children,new StructureChangedEventHandler(OnStructureChanged));
        //            elementList.Add(element);
        //        }
        //        catch (System.Exception ex)
        //        {
        //            string data = ex.Message;
        //        }

        //    }
        //    childElemList.Clear();
        //    //foreach (AutomationElement aeForm in elementList)
        //    //{
        //    //    addChildElement(aeForm);
        //    //}
        //    for (int i = 0; i < elementList.Count;i++ )
        //    {
        //        addChildElement(elementList[i]);
        //    }
        //}

        private Process GetPrentProcessName(Process p)
        {

            try
            {
                PerformanceCounter performanceCounter = new PerformanceCounter("Process", "Creating Process ID", p.ProcessName);

                Process parent = Process.GetProcessById((int)performanceCounter.NextValue());

                return parent;
            }
            catch (System.Exception ex)
            {
                return null;
            }
            
        }

        //public StringBuilder AnalysisStr()
        //{
        //    string buf = log.ToString();
        //    using (StreamWriter sw = File.CreateText("E:\\Test\\FunAutoTester\\Log2.txt"))
        //    {
        //        sw.Write(buf);
        //    }

        //    bool first = true;
        //    //bool dFlag = true;
        //    string cmd = "";
        //    string elem = "";
        //    string time = "";
        //    //string temp = "";
        //    string downTime = "";
        //    int length = 0;
        //    bool dcFlag = true;

        //    StringBuilder builder = new StringBuilder();

        //    string[] strLines = buf.Split(new char[] { '\n' });
        //    for (int i = 0; i < strLines.Length; i++)
        //    {
        //        //int t = 0;
        //        string[] args = strLines[i].Split(new char[] { '|' });
        //        //if (time!=""&&args.Length>1)
        //        //{
        //        //    t = Int32.Parse(args[2]) - Int32.Parse(time);
        //        //    if (t>1000*60*3)
        //        //    {
        //        //        string message = string.Format("Wait {0}\n", t);
        //        //        builder.Append(message);
        //        //    }
        //        //}
                
        //        if (args[0] == "LeftMouseDown")
        //        {
        //            if (first)
        //            {
        //                first = false;
        //                string message = string.Format("LeftMouseDown {0}\n", args[1]);
        //                builder.Append(message);
        //                length = message.Length;
        //                downTime = args[2];
        //            }
        //            else
        //            {
        //                int t = Int32.Parse(args[2]) - Int32.Parse(time);
        //                if (t < 500 && elem == args[1] && cmd == "LeftMouseUp"&&dcFlag)
        //                {
        //                    builder.Remove(builder.Length - length, length);
        //                    string message = string.Format("DoubleClick {0}\n", args[1]);
        //                    builder.Append(message);
        //                    length = message.Length;
        //                    dcFlag = false;

        //                }
        //                else
        //                {
        //                    string message = string.Format("LeftMouseDown {0}\n", args[1]);
        //                    builder.Append(message);
        //                    length = message.Length;
        //                    downTime = args[2];
        //                    dcFlag = true;
                            
        //                }

        //            }

        //        }
        //        else if (args[0] == "LeftMouseUp")
        //        {
        //            if (dcFlag && time != "")
        //            {
        //                int t = Int32.Parse(args[2]) - Int32.Parse(time);
        //                if (t < 200 && elem == args[1] && cmd == "LeftMouseDown")
        //                {
        //                    builder.Remove(builder.Length - length, length);
        //                    string message = string.Format("Click {0}\n", args[1]);
        //                    builder.Append(message);
        //                    length = message.Length;
        //                }
        //                else
        //                {
        //                    string message = string.Format("LeftMouseUp {0}\n", args[1]);
        //                    builder.Append(message);
        //                    length = message.Length;
        //                }
        //            }
        //            else
        //            {
        //                dcFlag = false;
                        
        //            }

        //        }
        //        else if (args[0] == "RightMouseDown")
        //        {
        //            if (first)
        //            {
        //                first = false;
        //                string message = string.Format("RightMouseDown {0}\n", args[1]);
        //                builder.Append(message);
        //                length = message.Length;
        //            }
        //            else
        //            {
        //                string message = string.Format("RightMouseDown {0}\n", args[1]);
        //                builder.Append(message);
        //                length = message.Length;
        //            }
        //        }
        //        else if (args[0] == "RightMouseUp")
        //        {
        //            int t = Int32.Parse(args[2]) - Int32.Parse(time);
        //            if (t < 200 && elem == args[1] && cmd == "RightMouseDown")
        //            {
        //                builder.Remove(builder.Length - length, length);
        //                string message = string.Format("RightClick {0}\n", args[1]);
        //                builder.Append(message);
        //                length = message.Length;
        //            }
        //            else
        //            {
        //                string message = string.Format("RightMouseUp {0}\n", args[1]);
        //                builder.Append(message);
        //                length = message.Length;
        //            }
        //        }
        //        else if (args[0] == "MouseMove")
        //        {
        //            string message = string.Format("MouseMove {0}\n", args[1]);
        //            builder.Append(message);
        //            length = message.Length;
        //        }
        //        else if (args[0] == "KeyDown")
        //        {

        //            string message = string.Format("KeyDown \"{0}\"\n", args[1]);
        //            builder.Append(message);
        //            length = message.Length;
        //            #region MyRegion
        //            //if (cmd == "KeyUp")
        //            //{
        //            //    builder.Remove(builder.Length - 1, 1);
        //            //    //string tmp = args[1].Substring(1, 6);
        //            //    if (args[1].Length == 7 && args[1].Substring(0, 6) == "NumPad")
        //            //    {
        //            //        //builder.Remove(builder.Length - 2, 2);
        //            //        args[1] = args[1].Substring(6, 1) + "\"";
        //            //        builder.Append(args[1] + "\n");

        //            //    }
        //            //    else if (args[1].Length == 2 && args[1].Substring(0, 1) == "D")
        //            //    {
        //            //        //builder.Remove(builder.Length - 2, 2);
        //            //        args[1] = args[1].Substring(1, 1) + "\"";
        //            //        builder.Append(args[1] + "\n");
        //            //    }
        //            //    else
        //            //    {
        //            //        //builder.Remove(builder.Length - 1, 1);
        //            //        builder.Append(args[1] + "\"\n");

        //            //    }

        //            //}
        //            //else
        //            //{
        //            //    //string tmp = args[1].Substring(1, 6);
        //            //    if (args[1].Length == 7 && args[1].Substring(0, 6) == "NumPad")
        //            //    {
        //            //        args[1] = args[1].Substring(6, 1) ;

        //            //    }
        //            //    else if (args[1].Length == 2 && args[1].Substring(0, 1) == "D")
        //            //    {
        //            //        args[1] = args[1].Substring(1, 1);

        //            //    }

        //            //    builder.Append("SendKeys \"" + args[1] + "\"\n");
        //            //} 
        //            #endregion
        //        }
        //        else if (args[0] == "KeyUp")
        //        {
        //            if (cmd == "KeyDown" && args[1] == elem)
        //            {
        //                builder.Remove(builder.Length - length, length);
        //                string message = string.Format("SendKey \"{0}\"\n", args[1]);
        //                builder.Append(message);
        //                length = message.Length;
        //            }
        //            else
        //            {
        //                string message = string.Format("KeyUp \"{0}\"\n", args[1]);
        //                builder.Append(message);
        //                length = message.Length;
        //            }
        //            #region MyRegion
        //            //if (cmd == "KeyDown")
        //            //{
        //            //    builder.Remove(builder.Length - 1, 1);
        //            //    //string tmp = args[1].Substring(1, 6);
        //            //    if (args[1].Length == 7 && args[1].Substring(0, 6) == "NumPad")
        //            //    {
        //            //        //builder.Remove(builder.Length - 2, 2);
        //            //        args[1] = args[1].Substring(6, 1) + "\"";
        //            //        builder.Append(args[1] + "\n");

        //            //    }
        //            //    else if (args[1].Length == 2 && args[1].Substring(0, 1) == "D")
        //            //    {
        //            //        //builder.Remove(builder.Length - 2, 2);
        //            //        args[1] = args[1].Substring(1, 1) + "\"";
        //            //        builder.Append(args[1] + "\n");
        //            //    }
        //            //    else
        //            //    {
        //            //        //builder.Remove(builder.Length - 1, 1);
        //            //        builder.Append(args[1] + "\"\n");

        //            //    }

        //            //}
        //            //else
        //            //{
        //            //    //string tmp = args[1].Substring(1, 6);
        //            //    if (args[1].Length == 7 && args[1].Substring(0, 6) == "NumPad")
        //            //    {
        //            //        args[1] = args[1].Substring(6, 1);

        //            //    }
        //            //    else if (args[1].Length == 2 && args[1].Substring(0, 1) == "D")
        //            //    {
        //            //        args[1] = args[1].Substring(1, 1);

        //            //    }

        //            //    builder.Append("SendKeys \"" + args[1] + "\"\n");
        //            //} 
        //            #endregion
        //        }
        //        else if (args[0] == "Activate")
        //        {
        //            string message = string.Format("Activate {0}\n", args[1]);
        //            builder.Append(message);
        //            length = message.Length;
        //        }
        //        else if (args[0] == "SetFocus")
        //        {
        //            string message = string.Format("SetFocus {0}\n", args[1]);
        //            builder.Append(message);
        //            length = message.Length;
        //        }
        //        else if (args[0] == "OpenMenu")
        //        {
        //            if (cmd == "LeftMouseDown")
        //            {
        //                continue;
        //            }
        //            else if (cmd == "LeftMouseUp")
        //            {
                        
        //                //string arg = elem.Substring(0, args[1].Length - 1) + "\"";
        //                //if (arg != args[1])
        //                //{
        //                //    string message = string.Format("MenuOpened {0}\n", args[1]);
        //                //    builder.Append(message);
        //                //    length = message.Length;
        //                //}
        //            }
        //            else if (cmd == "OpenMenu"&&args[1] == elem)
        //            {
        //                continue;
        //            }
        //            else
        //            {
        //                string message = string.Format("OpenMenu {0}\n", args[1]);
        //                builder.Append(message);
        //                length = message.Length;
        //            }
                                        
                    
        //        }
        //        else if (args[0] == "Start")
        //        {
        //            string message = string.Format("Start {0}\n", args[1]);
        //            builder.Append(message);
        //            length = message.Length;
        //        }
        //        else if (args[0] == "WindowCreate")
        //        {
        //            string message = string.Format("WindowCreate {0}\n", args[1]);
        //            builder.Append(message);
        //            length = message.Length;
        //        }
        //        else if (args[0] == "Stop")
        //        {
        //            builder.Append("Stop");
        //        }
        //        else if(args[0] == "Wait")
        //        {
        //            string message = string.Format("Wait {0}\n", args[1]);
        //            builder.Append(message);
        //            length = message.Length;
        //        }
        //        if (args.Length == 3)
        //        {
        //            cmd = args[0];
        //            elem = args[1];
        //            time = args[2];
        //        }
        //        else if (args.Length == 2)
        //        {
        //            cmd = args[0];
        //            elem = args[1];
        //        }

        //    }

        //    return builder;
        //}

        public void AddText(string str)
        {
            if (!str.EndsWith("\n"))
            {
                str += "\n";
            }
            log.Append(str); 
        }

        //public void SubscribeToInvoke(AutomationElement elementButton)
        //{
        //    if (elementButton != null)
        //    {
        //        Automation.AddAutomationEventHandler(InvokePattern.InvokedEvent,
        //             elementButton, TreeScope.Subtree,
        //             UIAeventHandler);
        //        ElementSubscribeButton = elementButton;
        //    }
        //}

        //private void OnUIAutomationEvent(object src, AutomationEventArgs e)
        //{
        //    // Make sure the element still exists. Elements such as tooltips
        //    // can disappear before the event is processed.
        //    AutomationElement sourceElement;
        //    try
        //    {
        //        sourceElement = src as AutomationElement;
        //    }
        //    catch (ElementNotAvailableException)
        //    {
        //        return;
        //    }
        //    if (e.EventId == InvokePattern.InvokedEvent)
        //    {
        //        // TODO Add handling code.
        //    }
        //    else
        //    {
        //        // TODO Handle any other events that have been subscribed to.
        //    }
        //}

        //private void ShutdownUIA()
        //{
        //    if (UIAeventHandler != null)
        //    {
        //        Automation.RemoveAutomationEventHandler(InvokePattern.InvokedEvent,
        //            ElementSubscribeButton, UIAeventHandler);
        //    }
        //    //if (menuClosedHandler != null)
        //    //{
        //    //    Automation.RemoveAutomationEventHandler(AutomationElement.MenuClosedEvent,targetApp)
        //    //}
        //}

        public string GetUserPath()
        {
            RegistryKey folders;
            folders = OpenRegistryPath(Registry.CurrentUser, @"/software/microsoft/windows/currentversion/explorer/shell folders");
            string desktop = folders.GetValue("Desktop").ToString();
            return desktop + "\\Untitled.aui";
        }

        private RegistryKey OpenRegistryPath(RegistryKey root, string s)
        {
            s = s.Remove(0, 1) + @"/";
            while (s.IndexOf(@"/") != -1)
            {
                root = root.OpenSubKey(s.Substring(0, s.IndexOf(@"/")));
                s = s.Remove(0, s.IndexOf(@"/") + 1);
            }
            return root;
        }

        public string GetTime()
        {
            //System.Management.ObjectQuery MyQuery = new System.Management.ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            ////System.Management.ObjectQuery MyQuery = new System.Management.ObjectQuery("SELECT * FROM Win32_ComputerSystem");
            //System.Management.ManagementScope MyScope = new System.Management.ManagementScope();
            //ManagementObjectSearcher MySearch = new ManagementObjectSearcher(MyScope, MyQuery);
            //ManagementObjectCollection MyCollection = MySearch.Get();
            //string StrInfo = "";
            //foreach (ManagementObject MyObject in MyCollection)
            //{
            //    //显示系统基本信息
            //    StrInfo = MyObject.GetText(TextFormat.Mof);
            //    //string[] MyString = { "" };
            //    //重新启动计算机
            //    //MyObject.InvokeMethod("Reboot",MyString);								
            //    //关闭计算机
            //    //MyObject.InvokeMethod("Shutdown",MyString);				
            //}
            ////string InstallDate = StrInfo.Substring(StrInfo.LastIndexOf("InstallDate") + 15, 14);
            //string LastBootUpTime = StrInfo.Substring(StrInfo.LastIndexOf("LastBootUpTime") + 18, 14);
            
            //int time = (int)DateTime.Now.Ticks / 1000;
            //DateTime d1 = DateTime.Now;
            //DateTime d2 = new DateTime(1970, 1, 1);
            //TimeSpan ts = d1 - d2;
            //double d = ts.TotalMilliseconds;
            //DateTime t1 = DateTime.Now;
            //TimeSpan t22 = new TimeSpan(t1.Ticks);
            //int time2 = Convert.ToInt32(t22.TotalSeconds);
            //return time2+"";
            return "";
        }

        public void DrawElement(int x, int y)
        {
            System.Windows.Point wpt = new System.Windows.Point(x, y);
            Thread thread = new Thread(() =>
            {
                try
                {
                    
//                     AutomationElement delem = AutomationElement.FromPoint(wpt);
// 
//                     if (IsOwnProcess(delem))
//                     {
//                         //drawElement = delem;
//                         System.Windows.Rect rect = delem.Current.BoundingRectangle;
//                         Rectangle r = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
//                         ControlPaint.DrawReversibleFrame(r, Color.Red, FrameStyle.Thick);
//                         //System.IntPtr DesktopHandle = GetDC(System.IntPtr.Zero); 
//                         //Graphics g = Graphics.FromHdc(DesktopHandle);
//                         //g.DrawRectangle(new Pen(Color.Red), r);
//                             
//                        //g.Dispose();
//                        
//                         
//                     }
                }
                catch (System.Exception ex)
                {
                   
                }
            });
            thread.Start();
            //sthread.Join();
        }

        public void SetWaitListener()
        {
            string message = "Wait ";
            Point point = Control.MousePosition;
            System.Windows.Point wpt = new System.Windows.Point(point.X, point.Y);
            Thread thread = new Thread(() =>
            {
                try
                {

                    AutomationElement element = AutomationElement.FromPoint(wpt);

                    if (IsOwnProcess(element))
                    {
                        ControlType type = GetElementType(element);
                        if (type==ControlType.Edit)
                        {
                            ValuePattern currentPattern = GetValuePattern(element);
                            string value = currentPattern.Current.Value;
                            message += GetCurrentElementInfo(element) +value+ "\"";
                        }
                        else
                        {
                            //bool enable = element.Current.IsEnabled;
                            //if (enable)
                            //{
                            //    message += GetCurrentElementInfo(element) + "true\"";
                            //}
                            //else
                            //{
                            //    message += GetCurrentElementInfo(element) + "false\"";
                            //}
                            message += GetCurrentElementInfo(element)+"\"";
                        }
                        

                    }
                }
                catch (System.Exception ex)
                {

                }


            });
            thread.Start();
            thread.Join();
            SendMessageBack(message);
           
        }

        public void SetValueListener()
        {
            string message = "";
            Point point = Control.MousePosition;
            System.Windows.Point wpt = new System.Windows.Point(point.X, point.Y);
//             Thread thread = new Thread(() =>
//             {
                try
                {
                    AutomationElement element = AutomationElement.FromPoint(wpt);

                    if (IsOwnProcess(element))
                    {
                        //message += GetCurrentElementInfo(element);
                        message = GetElementValue(element);
                        
                    }
                }
                catch (System.Exception ex)
                {

                }
//             });
//             thread.Start();
//             thread.Join();
                SendMessageBack(message);
        }

        public static ValuePattern GetValuePattern(AutomationElement element)
        {
            object currentPattern;
            if (!element.TryGetCurrentPattern(ValuePattern.Pattern, out currentPattern))
            {
                throw new Exception(string.Format("Element with AutomationId '{0}' and Name '{1}' does not support the ValuePattern.",
                    element.Current.AutomationId, element.Current.Name));
            }
            return currentPattern as ValuePattern;
        }

        private string GetElementValue(AutomationElement element)
        {
            StringBuilder value = new StringBuilder("SetValue ");
            ControlType type = GetElementType(element);
            if (type == ControlType.ListItem)
            {
                AutomationElement parent = GetParentElement(element);
                
                ControlType ptype = GetElementType(parent);
                if (ptype == ControlType.List)
                {

                    value.Append(GetCurrentElementInfo(parent));
                    //AutomationElementCollection childCollection = parent.FindAll(TreeScope.Children,
                    //    new AndCondition(new PropertyCondition(AutomationElement.IsControlElementProperty, true)));
                    TreeWalker walker = new TreeWalker(new PropertyCondition(AutomationElement.IsControlElementProperty, true));
                    AutomationElement elementNode = walker.GetFirstChild(parent);
                    int handler = parent.Current.NativeWindowHandle;
                    int i =0;
                    while (elementNode != null)
                    {
                        if (GetElementType(elementNode)==ControlType.ListItem)
                        {
                            //string name = elementNode.Current.Name;
                            StringBuilder text = new StringBuilder("",100);
                            SendMessage(handler, LB_GETTEXT, i, text);
                            value.Append(text).Append("|");
                            i++;
                        }
                        
                        elementNode = walker.GetNextSibling(elementNode);
                    }
                }
            }
            else if (type == ControlType.List)
            {
                value.Append(GetCurrentElementInfo(element));
                //AutomationElementCollection childCollection = parent.FindAll(TreeScope.Children,
                //    new AndCondition(new PropertyCondition(AutomationElement.IsControlElementProperty, true)));
                TreeWalker walker = new TreeWalker(new PropertyCondition(AutomationElement.IsControlElementProperty, true));
                AutomationElement elementNode = walker.GetFirstChild(element);
                while (elementNode != null)
                {
                    //string name = GetElementName(elementNode);
                    string name = elementNode.Current.Name;
                    value.Append("Add ").Append(name).Append("|");
                    elementNode = walker.GetNextSibling(elementNode);
                }
            }
            else if (type == ControlType.Edit)
            {

            }
            value.Append("\"");
            return value.ToString();
        }

        public AutomationElement GetParentElement(AutomationElement element)
        {
            TreeWalker walker = TreeWalker.ControlViewWalker;
            return walker.GetParent(element);
        }

        public void LeftMouseDownAction(int x, int y)
        {
            focusElement = null;
            preX = x;
            preY = y;
            elementInfo = GetElementFromPoint(new Point(x, y));
            
            if (elementInfo == null || elementInfo == "")
            {
                return;
            }
            if (clickCount == 0)
            {
                preElement = currentElement;
                preInfo = elementInfo;
            }
            
            clickCount++;
            if (!mouseDownFlag)
            {
                mouseDownFlag = true;
                timer.Enabled = true;
                timer.Start();
            }
        }

        public void LeftMouseUpAction(int x, int y)
        {
            if (mouseMoveFlag)
            {
                if (elementInfo != null && elementInfo != "")
                {
                    int offsetX = x - preX;
                    int offsetY = y - preY;
                    if (Math.Abs(offsetX) < 2 && Math.Abs(offsetY) < 2)
                    {
                        SendMessageBack("Click " + elementInfo);
                    }
                    else
                    {
                        clickFlag = false;
                        clickCount = 0;
                        mouseDownFlag = false;
                        timer.Stop();
                        SendMessageBack("Move " + elementInfo + offsetX + "\"" + offsetY + "\"");
                    }
                    mouseMoveFlag = false;
                }

            }
            else
            {
                //elementInfo = GetElementFromPoint(new Point(x, y));

                if (elementInfo == null || elementInfo == "")
                {
                    return ;
                }
                if (preElement == currentElement)
                {
                    if (mouseUpFlag)
                    {
                        mouseUpFlag = false;
                        SendMessageBack("Click " + elementInfo);
                        clickFlag = false;
                    }
                    else
                    {
                        clickFlag = true;
                    }
                }
                else
                {
                    if (mouseUpFlag)
                    {
                        mouseUpFlag = false;
                        clickFlag = false;
                        SendMessageBack("MouseDown " + preInfo);
                        SendMessageBack("MouseUp " + elementInfo);
                    }
                    else
                    {
                        clickFlag = true;
                    }
                    
                }
            }
            if (activateFlag)
            {
                activateFlag = false;
                SendMessageBack("Activate " + windowName);
                windowName = "";
            }
        }

        public void RightMouseDownAction(int x, int y)
        {
            focusElement = null;
            preX = x;
            preY = y;
            elementInfo = GetElementFromPoint(new Point(x, y));

            if (elementInfo == null || elementInfo == "")
            {
                return ;
            }
            preElement = currentElement;
        }

        public void RightMouseUpAction(int x, int y)
        {
            elementInfo = GetElementFromPoint(new Point(x, y));

            if (elementInfo == null || elementInfo == "")
            {
                return;
            }
            if (preElement == currentElement)
            {
                SendMessageBack("RightClick " + elementInfo);
            }

            if (activateFlag)
            {
                activateFlag = false;
                SendMessageBack("Activate " + windowName);
                windowName = "";
            }
        }

        public void MouseMoveAction(int x, int y)
        {

            mouseMoveFlag = true;
            
            
        }
        
        //public string GetMouseAction(string action,MouseEventArgs e)
        //{
            
        //    if (action == "LeftMouseDown")
        //    {
        //        preX = e.X;
        //        preY = e.Y;
        //        elementInfo = GetElementFromPoint(new Point(e.X, e.Y));
        //        //                 if (openedMenuElement != null && currentElement.Current.LocalizedControlType == "menu item" && currentElement.Current.Name == openedMenuElement.Current.Name)
        //        //                 {
        //        //                     return "";
        //        //                 }
        //        if (elementInfo == null || elementInfo == "")
        //        {
        //            return "";
        //        }

        //        preElement = currentElement;
        //        clickCount++;
        //        if (!mouseDownFlag)
        //        {
        //            mouseDownFlag = true;
        //            timer.Enabled = true;
        //            timer.Start();
        //        }
                
        //    }
        //    else if (action == "LeftMouseUp")
        //    {
                
        //        if (mouseMoveFlag)
        //        {
        //            if (elementInfo != null || elementInfo != "")
        //            {
        //                int offsetX = e.X - preX;
        //                int offsetY = e.Y - preY;
        //                SendMessage("Move " + elementInfo + offsetX + "\"" + offsetY + "\"");
        //                mouseMoveFlag = false;
        //            }
                    
        //        }
        //        else
        //        {
        //            elementInfo = GetElementFromPoint(new Point(e.X, e.Y));

        //            if (elementInfo == null || elementInfo == "")
        //            {
        //                return "";
        //            }
        //            if (preElement == currentElement)
        //            {
        //                if (mouseUpFlag)
        //                {
        //                    mouseUpFlag = false;
        //                    SendMessage("Click " + elementInfo);
        //                }
        //                else
        //                {
        //                    clickFlag = true;
        //                }
        //            }
        //        }
                
        //    }
        //    else if (action == "RightMouseDown")
        //    {
        //        preX = e.X;
        //        preY = e.Y;
        //        elementInfo = GetElementFromPoint(new Point(e.X, e.Y));

        //        if (elementInfo == null || elementInfo == "")
        //        {
        //            return "";
        //        }
        //        preElement = currentElement;
        //    }
        //    else if (action == "RightMouseUp")
        //    {
                
        //        elementInfo = GetElementFromPoint(new Point(e.X, e.Y));

        //        if (elementInfo == null || elementInfo == "")
        //        {
        //            return "";
        //        }
        //        if (preElement == currentElement)
        //        {
        //            SendMessage("RightClick " + elementInfo);
        //        }
        //    }
        //    else if (action == "MouseMove")
        //    {
        //        mouseMoveFlag = true;
        //        clickFlag = false;
        //        clickCount = 0;
        //        mouseDownFlag = false;
        //        timer.Stop();
        //    }
        //    return "";
        //}

        public void KeyDownAction(Keys key)
        {
            if (focusElement==null)
            {
                string str = "";
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        focusElement = AutomationElement.FocusedElement;
                        if (IsOwnProcess(focusElement))
                        {
                            str = GetCurrentElementInfo(focusElement);

                            currentElement = focusElement;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        str = null;
                    }


                });
                thread.Start();
                thread.Join();

                SendMessageBack("SetFocus " + str);
            }
            if (keyDownFlag)
            {
                SendMessageBack("KeyDown \"" + keyDown + "\"");
            }
            keyDown = key;
            keyDownFlag = true;
            
        }

        public void KeyUpAction(Keys key)
        {
            if (keyDownFlag && key == keyDown)
            {
                SendMessageBack("SendKey \"" + keyDown + "\"");
            }
            else
            {
                SendMessageBack("KeyUp \"" + key + "\"");
            }
            keyDownFlag = false;
        }

        private void OnSetTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (mouseMoveFlag)
            {
            }
            if (clickCount==1)
            {
                if (clickFlag)
                {
                    SendMessageBack("Click " + preInfo);
                }
                else
                {
                    mouseUpFlag = true;
                }
            }
            else if (clickCount ==2)
            {
                if (clickFlag)
                {
                    if (preElement == currentElement)
                    {
                        SendMessageBack("DoubleClick " + preInfo);
                    }
                    else
                    {
                        SendMessageBack("Click " + preInfo);
                        SendMessageBack("Click " + elementInfo);
                    }
                }
            }
            if (activateFlag)
            {
                activateFlag = false;
                SendMessageBack("Activate " + windowName);
                windowName = "";
            }
            clickFlag = false;
            clickCount = 0;
            mouseDownFlag = false;
            timer.Stop();
        }

        public void WriteToFile(string path)
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.Write(log.ToString());
            }
        }

        
    }
}
