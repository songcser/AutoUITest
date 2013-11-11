using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace GlobalHooksTest
{
    class ElementManage
    {
        private static List<AutomationElement> elementList = null;
        private static List<AutomationElement> childElemList = null;
        private static AutomationElementCollection elements = null;

        private CacheRequest fetchRequest;
        private bool isFocusable = false;
        public bool IsFocusable
        {
            get { return isFocusable; }
            set { isFocusable = value; }
        }

        private List<Process> proceses = null;
        private Process mainProcess = null;
        private bool isLock = false;
        //private AutomationEventHandler selectionHandler = null;
        //private AutomationEventHandler openHandler = null;
        //private static Process process = null;
        //private AutomationElement comboBoxElem;
        AutomationFocusChangedEventHandler focusHandler = null;

        public delegate bool CallBack(IntPtr hwnd, int lParam);

        

        [DllImport("user32.dll")]
        public static extern int EnumWindows(CallBack lpfn, int lParam);
        public static CallBack callBackEnumWindows = new CallBack(WindowProcess);

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
            fetchRequest = new CacheRequest();
            elementList = new List<AutomationElement>();
            childElemList = new List<AutomationElement>();
            proceses = new List<Process>();

            fetchRequest.Add(AutomationElement.NameProperty);
            fetchRequest.Add(AutomationElement.AutomationIdProperty);
            fetchRequest.Add(AutomationElement.ControlTypeProperty);
            fetchRequest.Add(AutomationElement.BoundingRectangleProperty);

            
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

        public void UpdateCache()
        {
            for (int i = 0; i < childElemList.Count;i++ )
            {
                childElemList[i] = childElemList[i].GetUpdatedCache(fetchRequest);
            }
        }

        public void StartProcess(string strPath)
        {
            mainProcess = Process.Start(strPath);
            
            //AutomationElement aeDeskTop = AutomationElement.RootElement;
            Thread.Sleep(500);
            //AutomationElement aeForm = null;
            
            int times = 0;

            mainProcess.WaitForInputIdle();
            while (mainProcess.MainWindowHandle == null || mainProcess.MainWindowHandle == IntPtr.Zero)
            {
                Thread.Sleep(1000);
                if (times > 5 && mainProcess.Handle != IntPtr.Zero)
                {
                    break;
                }
                times++;

            }
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

        public void addChildElement(AutomationElement element)
        {
            Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
            //Condition condition = new PropertyCondition(AutomationElement.ProcessIdProperty, pId);
            //AutomationElement element = AutomationElement.FromHandle(handle);
            if (element!=null)
            {
                using (fetchRequest.Activate())
                {
                    AutomationElementCollection tElemList = element.FindAll(TreeScope.Subtree, new AndCondition( condition1, condition2));
                    //elements = aeForm.FindAll(TreeScope.Subtree, new AndCondition(condition1, condition2, condition));
                    ///elements.CopyTo(temp, temp.Count);
                    for (int i = 0; i < tElemList.Count; i++)
                    {
                        childElemList.Add(tElemList[i]);
                    }
                }
            }
            
        }

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
            if (element == null)
            {
                //AddText("Automation null");
                return null;
            }
            string str = "\"";
            try
            {
                int processId;
                object processIdentifierNoDefault =
                    element.GetCurrentPropertyValue(AutomationElement.ProcessIdProperty, true);
                if (processIdentifierNoDefault == AutomationElement.NotSupported)
                {
                    // TODO Handle the case where you do not wish to proceed using the default value.
                    return null;
                }
                else
                {
                    processId = (int)processIdentifierNoDefault;
                    if (!isProcessId(processId))
                    {
                        return null;
                    }
                    
                }
                //string name = element.Current.Name;
                string name = element.GetCurrentPropertyValue(AutomationElement.NameProperty, true) as string;
                
                str += name + "\"";
                string type = "";

                object controlTypeNoDefault = element.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty, true);
                if (controlTypeNoDefault == AutomationElement.NotSupported)
                {
                    return null;
                }
                ControlType controlType = controlTypeNoDefault as ControlType;
                str += AnalyseType(controlType, element);
            }
            catch (System.Exception ex)
            {
                string m = ex.Message;
            }
            

            return str;
        }
        
        public bool isProcessId(int processId)
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
            string type = "";
            
            object controlTypeNoDefault = element.GetCachedPropertyValue(AutomationElement.ControlTypeProperty, true);
            if (controlTypeNoDefault == AutomationElement.NotSupported)
            {
                return null;
            }
            ControlType controlType = controlTypeNoDefault as ControlType;
            str += AnalyseType(controlType, element);
            
            //str += " \"" + element.Cached.ControlType.ToString() + "\"";

            return str;
        }

        private string AnalyseType(ControlType controlType,AutomationElement element)
        {
            string str = "";
            string  type = controlType.LocalizedControlType;
            if (type == "tree view" && !isFocusable)
            {

                UpdateCache();
                Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
                Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
                //Condition condition = new PropertyCondition(AutomationElement.ProcessIdProperty, process.Id);
                using (fetchRequest.Activate())
                {
                    AutomationElementCollection tElemList = element.FindAll(TreeScope.Subtree, new AndCondition(condition1, condition2));
                    for (int i = 0; i < tElemList.Count; i++)
                    {
                        childElemList.Add(tElemList[i]);
                    }
                }

            }
            else
            {
                str += "\"" + type + "\"";
            }


            if (controlType == ControlType.MenuItem)
            {

            }
            else if (controlType == ControlType.ComboBox)
            {
                return null;
            }

            return str;
        }

        private void SelectionHandler(object sender, AutomationEventArgs e)
        {
            AutomationElement element = sender as AutomationElement;
            // TODO: event handling
        }

        private void OnStructureChanged(object sender, StructureChangedEventArgs e)
        {
            AutomationElement element = sender as AutomationElement;

            if (e.StructureChangeType == StructureChangeType.ChildAdded)
            {
                using (fetchRequest.Activate())
                {
                    childElemList.Add(element);
                }
                addChildElement(element);
            }
        }

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
                        str = GetCurrentElementInfo(autoElem);
                        
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
                    isFocusable = true;

                }
                else
                {
                    return element;
                }
            }
            catch (System.Exception ex)
            {
                isFocusable = true;
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
                    using (fetchRequest.Activate())
                    {
                        AutomationElementCollection tElemList = elementParent.FindAll(TreeScope.Subtree, new AndCondition(condition1, condition2));
                        for (int i = 0; i < tElemList.Count; i++)
                        {
                            childElemList.Add(tElemList[i]);
                        }
                    }
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

        public void SubscribeToFocusChange()
        {
            focusHandler = new AutomationFocusChangedEventHandler(OnFocusChange);
            Automation.AddAutomationFocusChangedEventHandler(focusHandler);
        }

        private void OnFocusChange(object src, AutomationFocusChangedEventArgs e)
        {
            // TODO Add event handling code.
            // The arguments tell you which elements have lost and received focus.
        }

        public void UnsubscribeFocusChange()
        {
            if (focusHandler != null)
            {
                Automation.RemoveAutomationFocusChangedEventHandler(focusHandler);
            }
        }

        public string GetNameFromHandle(IntPtr handle)
        {
            
            string str = "";
            try
            {
                Thread thread = new Thread(() =>
                {
                    AutomationElement element = GetElementFromHandle(handle);
                    str = GetCurrentElementInfo(element);
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

        private AutomationElement GetElementsByPoint(Point point)
        {
            AutomationElement targetElem = null;
           
            for (int i = 0; i < childElemList.Count; i++)
            {
                try
                {
                    AutomationElement element = childElemList[i];
                    System.Windows.Rect boundingRect = (System.Windows.Rect)element.GetCachedPropertyValue(AutomationElement.BoundingRectangleProperty);
                    if (point.X > boundingRect.Left && point.X < boundingRect.Right && point.Y < boundingRect.Bottom && point.Y > boundingRect.Top)
                    {
                        targetElem = element;
                        //return element;
                    }
                }
                catch (System.Exception ex)
                {
                    string exception = ex.Message;
                }

            }
            return targetElem;
        }
        
        public static bool WindowProcess(IntPtr hwnd, int lParam)
        {
            //EnumChildWindows(hwnd, callBackEnumChildWindows, 0);
            StringBuilder title = new StringBuilder(200);
            int len;
            len = GetWindowText(hwnd, title, 200);
            //count++;
            uint processId = 0;
            //string name = process.MainWindowTitle;
            GetWindowThreadProcessId(hwnd, ref processId);
            if (processId == lParam && title.Length > 0)
            {
                AutomationElement mainElement = AutomationElement.FromHandle(hwnd);
                if (mainElement != null)
                {
                    elementList.Add(mainElement);
                }

                //elementList.Add(mainElement);
            }
           
            return true;
        }

        public bool hasWindowsProcess(IntPtr hwnd)
        {
            uint processId = 0;
            bool flag = true;
            GetWindowThreadProcessId(hwnd, ref processId);
            foreach (Process process in proceses)
            {
                if (process.Id==processId)
                {
                    flag = false;
                }
            }
            if (flag&&processId!=mainProcess.Id)
            {
                int count = proceses.Count;
                Process process = Process.GetProcessById((int)processId);
                
                Process parent = GetPrentProcessName(process);
                if (parent.Id==mainProcess.Id)
                {
                    proceses.Add(process);
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (proceses[i].Id == parent.Id)
                        {
                            proceses.Add(process);
                        }
                    }
                }
            }
            return true;
        }
        
        private void waitProcess(IntPtr  hwnd)
        {
            
            if (hwnd == IntPtr.Zero)
            {
                EnumWindows(callBackEnumWindows, 0);
            }
            else
            {
                try
                {
                    AutomationElement element = AutomationElement.FromHandle(hwnd);
                    Automation.AddStructureChangedEventHandler(element, TreeScope.Children,new StructureChangedEventHandler(OnStructureChanged));
                    elementList.Add(element);
                }
                catch (System.Exception ex)
                {
                    string data = ex.Message;
                }

            }
            childElemList.Clear();
            //foreach (AutomationElement aeForm in elementList)
            //{
            //    addChildElement(aeForm);
            //}
            for (int i = 0; i < elementList.Count;i++ )
            {
                addChildElement(elementList[i]);
            }
        }

        private Process GetPrentProcessName(Process p)
        {

            PerformanceCounter performanceCounter = new PerformanceCounter("Process", "Creating Process ID", p.ProcessName);

            Process parent = Process.GetProcessById((int)performanceCounter.NextValue());

            return parent;
        }

    }
}
