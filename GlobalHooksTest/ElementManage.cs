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

        private List<Process> proceses = null;
        private Process mainProcess = null;
        private List<IntPtr> handleList = null;
        
        //private AutomationEventHandler selectionHandler = null;
        //private AutomationEventHandler openHandler = null;
        //private static Process process = null;
        //private AutomationElement comboBoxElem;
        AutomationFocusChangedEventHandler focusHandler = null;

        public delegate bool CallBack(IntPtr hwnd, int lParam);

        AutomationElement ElementSubscribeButton;
        AutomationEventHandler UIAeventHandler;
        AutomationEventHandler menuOpenedHandler;
        AutomationEventHandler menuClosedHandler;

        private static AutomationElement targetApp;

        [DllImport("user32.dll")]
        public static extern int EnumWindows(CallBack lpfn, int lParam);
        //public static CallBack callBackEnumWindows = new CallBack(WindowProcess);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern uint RealGetWindowClass(IntPtr hWnd, StringBuilder pszType, uint cchType);
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle rect);

        public ElementManage()
        {
            //fetchRequest = new CacheRequest();
            elementList = new List<AutomationElement>();
            handleList = new List<IntPtr>();
            //childElemList = new List<AutomationElement>();
            proceses = new List<Process>();
            log = new StringBuilder();
            rect = new System.Windows.Rect();
            
            UIAeventHandler = new AutomationEventHandler(OnUIAutomationEvent);
            menuClosedHandler = new AutomationEventHandler(OnMenuClosed);
            menuOpenedHandler = new AutomationEventHandler(OnMenuOpened);
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
            if (hasWindowsProcess(handle))
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
            return null;
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
            mainProcess = Process.Start(strPath);
            
            //AutomationElement aeDeskTop = AutomationElement.RootElement;
            Thread.Sleep(500);
            //AutomationElement aeForm = null;
            
            //int times = 0;

            mainProcess.WaitForInputIdle();
            //while (mainProcess.MainWindowHandle == null || mainProcess.MainWindowHandle == IntPtr.Zero)
            //{
            //    Thread.Sleep(200);
            //    if (times > 5 && mainProcess.Handle != IntPtr.Zero)
            //    {
            //        break;
            //    }
            //    times++;

            //}
            //if (mainProcess.MainWindowHandle == IntPtr.Zero)
            //{
            //    EnumWindows(callBackEnumWindows, mainProcess.Id);
            //}
            //else
            //{
            //    elementList.Add(AutomationElement.FromHandle(mainProcess.MainWindowHandle));
            //}

            //Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            //Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
            //Condition condition = new PropertyCondition(AutomationElement.ProcessIdProperty, mainProcess.Id);
            ////AutomationElement aeForm = AutomationElement.FromHandle(process.MainWindowHandle);
            
            //using (fetchRequest.Activate())
            //{
            //    foreach (AutomationElement aeForm in elementList)
            //    {
            //        AutomationElementCollection tElemList = aeForm.FindAll(TreeScope.Subtree, new AndCondition(condition, condition1, condition2));
            //        //elements = aeForm.FindAll(TreeScope.Subtree, new AndCondition(condition1, condition2, condition));
            //        ///elements.CopyTo(temp, temp.Count);
            //        for (int i = 0; i < tElemList.Count; i++)
            //        {
            //            childElemList.Add(tElemList[i]);
            //        }
            //    }
            //    //WalkEnabledElements(aeDeskTop);

            //}
            
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
            
            StringBuilder str = new StringBuilder("\"");
           
            str.Append(GetElementName(element)).Append("\"");
            //string type = "";

            //ControlType controlType = GetElementType(element);
            string type = element.Current.LocalizedControlType;
            //if (controlType == ControlType.TreeItem)
            //{
            //    type = "tree item";
            //}
            //if (controlType == ControlType.Menu)
            //{
            //    type = "menu item";
            //}
            
            str.Append(type).Append("\"");

            string autoId = GetElementAutomationId(element);
            if (autoId=="")
            {
                TreeWalker walker = TreeWalker.ControlViewWalker;
                
                AutomationElement parent = walker.GetParent(element);
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
                        string pt = parent.Current.LocalizedControlType;
                        if (ptype == ControlType.List)
                        {
                            pt = "list";
                        }
                        
                        str.Append(pt).Append("\"");
                    }
                    
                }
            }
            else
            {
                str.Append(autoId).Append("\"");
            }
            
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
                    
                }
            }
            catch (System.Exception ex)
            {
                string m = ex.Message;
            }
            return false;
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
            AutomationElement autoElem ;
            string str = "";
            try
            {
                
                Thread thread = new Thread(() =>
                {
                    autoElem = AutomationElement.FromPoint(wpt);
                    if (IsOwnProcess(autoElem))
                    {
                        str = GetCurrentElementInfo(autoElem);
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

        public void SubscribeToMenuClosed()
        {
            
            //Automation.AddAutomationFocusChangedEventHandler(focusHandler);
            Automation.AddAutomationEventHandler(AutomationElement.MenuClosedEvent,
                    AutomationElement.RootElement,
                    TreeScope.Descendants,
                    UIAeventHandler);
            
        }

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
            }
            
        }

        private void OnMenuClosed(object src, AutomationEventArgs e)
        {
            AutomationElement selectionitemElement = src as AutomationElement;
            //Feedback("MenuClosed event: ");
            AddText("MenuClosed ");
        }

        private void OnMenuOpened(object src, AutomationEventArgs e)
        {
            AutomationElement element = src as AutomationElement;
            string message = "";
            //Thread thread = new Thread(() =>
            //{
            //message = GetCurrentElementInfo(element);
            message += "\"" + GetElementName(element) + "\"menu item\"\"";
           // });
            
            //Feedback("MenuOpened event: ");
            AddText("MenuOpened|"+message);
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
            try
            {
                Thread thread = new Thread(() =>
                {
                    
                    AutomationElement element = GetElementFromHandle(handler);
                    targetApp = element;
                   
                });
                thread.Start();
                thread.Join();

                
            }
            catch (System.Exception ex)
            {
                return;
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

        public bool hasWindowsProcess(IntPtr hwnd)
        {
            uint processId = 0;
            //bool flag = true;
            GetWindowThreadProcessId(hwnd, ref processId);
            foreach (Process process in proceses)
            {
                if (process.Id==processId)
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

        public StringBuilder AnalysisStr()
        {
            string buf = log.ToString();
            using (StreamWriter sw = File.CreateText("E:\\GitHub\\AutoUITest\\Log2.txt"))
            {
                sw.Write(buf);
            }

            bool first = true;
            //bool dFlag = true;
            string cmd = "";
            string elem = "";
            string time = "";
            //string temp = "";
            string downTime = "";
            int length = 0;
            bool dcFlag = true;

            StringBuilder builder = new StringBuilder();

            string[] strLines = buf.Split(new char[] { '\n' });
            for (int i = 0; i < strLines.Length; i++)
            {
                string[] args = strLines[i].Split(new char[] { '|' });
                if (args[0] == "LeftMouseDown")
                {
                    if (first)
                    {
                        first = false;
                        string message = string.Format("LeftMouseDown {0}\n", args[1]);
                        builder.Append(message);
                        length = message.Length;
                        downTime = args[2];
                    }
                    else
                    {
                        int t = Int32.Parse(args[2]) - Int32.Parse(downTime);
                        if (t < 500 && elem == args[1] && cmd == "LeftMouseUp")
                        {
                            builder.Remove(builder.Length - length, length);
                            string message = string.Format("DoubleClick {0}\n", args[1]);
                            builder.Append(message);
                            length = message.Length;
                            dcFlag = false;

                        }
                        else
                        {
                            string message = string.Format("LeftMouseDown {0}\n", args[1]);
                            builder.Append(message);
                            length = message.Length;
                            downTime = args[2];
                            dcFlag = true;
                        }

                    }

                }
                else if (args[0] == "LeftMouseUp")
                {
                    if (dcFlag && time != "")
                    {
                        int t = Int32.Parse(args[2]) - Int32.Parse(time);
                        if (t < 200 && elem == args[1] && cmd == "LeftMouseDown")
                        {
                            builder.Remove(builder.Length - length, length);
                            string message = string.Format("Click {0}\n", args[1]);
                            builder.Append(message);
                            length = message.Length;
                        }
                        else
                        {
                            string message = string.Format("LeftMouseUp {0}\n", args[1]);
                            builder.Append(message);
                            length = message.Length;
                        }
                    }
                    else
                    {
                        dcFlag = false;
                    }

                }
                else if (args[0] == "RightMouseDown")
                {
                    if (first)
                    {
                        first = false;
                        string message = string.Format("RightMouseDown {0}\n", args[1]);
                        builder.Append(message);
                        length = message.Length;
                    }
                    else
                    {
                        string message = string.Format("RightMouseDown {0}\n", args[1]);
                        builder.Append(message);
                        length = message.Length;
                    }
                }
                else if (args[0] == "RightMouseUp")
                {
                    int t = Int32.Parse(args[2]) - Int32.Parse(time);
                    if (t < 200 && elem == args[1] && cmd == "RightMouseDown")
                    {
                        builder.Remove(builder.Length - length, length);
                        string message = string.Format("RightClick {0}\n", args[1]);
                        builder.Append(message);
                        length = message.Length;
                    }
                    else
                    {
                        string message = string.Format("RightMouseUp {0}\n", args[1]);
                        builder.Append(message);
                        length = message.Length;
                    }
                }
                else if (args[0] == "MouseMove")
                {
                    string message = string.Format("MouseMove {0}\n", args[1]);
                    builder.Append(message);
                    length = message.Length;
                }
                else if (args[0] == "KeyDown")
                {

                    string message = string.Format("KeyDown \"{0}\"\n", args[1]);
                    builder.Append(message);
                    length = message.Length;
                    #region MyRegion
                    //if (cmd == "KeyUp")
                    //{
                    //    builder.Remove(builder.Length - 1, 1);
                    //    //string tmp = args[1].Substring(1, 6);
                    //    if (args[1].Length == 7 && args[1].Substring(0, 6) == "NumPad")
                    //    {
                    //        //builder.Remove(builder.Length - 2, 2);
                    //        args[1] = args[1].Substring(6, 1) + "\"";
                    //        builder.Append(args[1] + "\n");

                    //    }
                    //    else if (args[1].Length == 2 && args[1].Substring(0, 1) == "D")
                    //    {
                    //        //builder.Remove(builder.Length - 2, 2);
                    //        args[1] = args[1].Substring(1, 1) + "\"";
                    //        builder.Append(args[1] + "\n");
                    //    }
                    //    else
                    //    {
                    //        //builder.Remove(builder.Length - 1, 1);
                    //        builder.Append(args[1] + "\"\n");

                    //    }

                    //}
                    //else
                    //{
                    //    //string tmp = args[1].Substring(1, 6);
                    //    if (args[1].Length == 7 && args[1].Substring(0, 6) == "NumPad")
                    //    {
                    //        args[1] = args[1].Substring(6, 1) ;

                    //    }
                    //    else if (args[1].Length == 2 && args[1].Substring(0, 1) == "D")
                    //    {
                    //        args[1] = args[1].Substring(1, 1);

                    //    }

                    //    builder.Append("SendKeys \"" + args[1] + "\"\n");
                    //} 
                    #endregion
                }
                else if (args[0] == "KeyUp")
                {
                    if (cmd == "KeyDown" && args[1] == elem)
                    {
                        builder.Remove(builder.Length - length, length);
                        string message = string.Format("SendKey \"{0}\"\n", args[1]);
                        builder.Append(message);
                        length = message.Length;
                    }
                    else
                    {
                        string message = string.Format("KeyUp \"{0}\"\n", args[1]);
                        builder.Append(message);
                        length = message.Length;
                    }
                    #region MyRegion
                    //if (cmd == "KeyDown")
                    //{
                    //    builder.Remove(builder.Length - 1, 1);
                    //    //string tmp = args[1].Substring(1, 6);
                    //    if (args[1].Length == 7 && args[1].Substring(0, 6) == "NumPad")
                    //    {
                    //        //builder.Remove(builder.Length - 2, 2);
                    //        args[1] = args[1].Substring(6, 1) + "\"";
                    //        builder.Append(args[1] + "\n");

                    //    }
                    //    else if (args[1].Length == 2 && args[1].Substring(0, 1) == "D")
                    //    {
                    //        //builder.Remove(builder.Length - 2, 2);
                    //        args[1] = args[1].Substring(1, 1) + "\"";
                    //        builder.Append(args[1] + "\n");
                    //    }
                    //    else
                    //    {
                    //        //builder.Remove(builder.Length - 1, 1);
                    //        builder.Append(args[1] + "\"\n");

                    //    }

                    //}
                    //else
                    //{
                    //    //string tmp = args[1].Substring(1, 6);
                    //    if (args[1].Length == 7 && args[1].Substring(0, 6) == "NumPad")
                    //    {
                    //        args[1] = args[1].Substring(6, 1);

                    //    }
                    //    else if (args[1].Length == 2 && args[1].Substring(0, 1) == "D")
                    //    {
                    //        args[1] = args[1].Substring(1, 1);

                    //    }

                    //    builder.Append("SendKeys \"" + args[1] + "\"\n");
                    //} 
                    #endregion
                }
                else if (args[0] == "Activate")
                {
                    string message = string.Format("Activate {0}\n", args[1]);
                    builder.Append(message);
                    length = message.Length;
                }
                else if (args[0] == "SetFocus")
                {
                    string message = string.Format("SetFocus {0}\n", args[1]);
                    builder.Append(message);
                    length = message.Length;
                }
                else if (args[0] == "MenuOpened")
                {
                    if (cmd == "LeftMouseDown")
                    {
                        continue;
                    }
                    else if (cmd == "LeftMouseUp")
                    {
                        string arg = elem.Substring(0, args[1].Length-1)+"\"";
                        if (arg != args[1])
                        {
                            string message = string.Format("MenuOpened {0}\n", args[1]);
                            builder.Append(message);
                            length = message.Length;
                        }
                    }
                    else
                    {
                        string message = string.Format("MenuOpened {0}\n", args[1]);
                        builder.Append(message);
                        length = message.Length;
                    }
                                        
                    
                }
                else if (args[0] == "Start")
                {
                    string message = string.Format("Start {0}\n", args[1]);
                    builder.Append(message);
                    length = message.Length;
                }
                else if (args[0] == "WindowCreate")
                {
                    string message = string.Format("WindowCreate {0}\n", args[1]);
                    builder.Append(message);
                    length = message.Length;
                }
                else if (args[0] == "Stop")
                {
                    builder.Append("Stop");
                }
                if (args.Length == 3)
                {
                    cmd = args[0];
                    elem = args[1];
                    time = args[2];
                }
                else if (args.Length == 2)
                {
                    cmd = args[0];
                    elem = args[1];
                }

            }

            return builder;
        }

        public void AddText(string str)
        {
            if (!str.EndsWith("\n"))
            {
                str += "\n";
            }
            log.Append(str); 
        }

        public void SubscribeToInvoke(AutomationElement elementButton)
        {
            if (elementButton != null)
            {
                Automation.AddAutomationEventHandler(InvokePattern.InvokedEvent,
                     elementButton, TreeScope.Subtree,
                     UIAeventHandler);
                ElementSubscribeButton = elementButton;
            }
        }

        private void OnUIAutomationEvent(object src, AutomationEventArgs e)
        {
            // Make sure the element still exists. Elements such as tooltips
            // can disappear before the event is processed.
            AutomationElement sourceElement;
            try
            {
                sourceElement = src as AutomationElement;
            }
            catch (ElementNotAvailableException)
            {
                return;
            }
            if (e.EventId == InvokePattern.InvokedEvent)
            {
                // TODO Add handling code.
            }
            else
            {
                // TODO Handle any other events that have been subscribed to.
            }
        }

        private void ShutdownUIA()
        {
            if (UIAeventHandler != null)
            {
                Automation.RemoveAutomationEventHandler(InvokePattern.InvokedEvent,
                    ElementSubscribeButton, UIAeventHandler);
            }
            //if (menuClosedHandler != null)
            //{
            //    Automation.RemoveAutomationEventHandler(AutomationElement.MenuClosedEvent,targetApp)
            //}
        }
    }
}
