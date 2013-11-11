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

namespace AutoUIPlayback
{
   
    class Analysis
    {
        private static Process process;

        [DllImport("user32.dll")]
        extern static bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        extern static void mouse_event(int mouseEventFlag, int incrementX, int intincrementY, uint data, UIntPtr extraInfo);

        const int MOUSEEVENTF_MOVE = 0x0001;

        const int MOUSEEVENTF_LEFTDOWN = 0x0002;

        const int MOUSEEVENTF_LEFTUP = 0x0004;

        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;

        const int MOUSEEVENTF_RIGHTUP = 0x0010;

        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;

        const int MOUSEEVENTF_MIDDLEUP = 0x0040;

        const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        private static AutomationElement mainWindow = null;
 //       private static AutomationElement valueElement = null;
        
        //public AutomationElement MainWindow
        //{
        //    get { return mainWindow; }
        //    set { mainWindow = value; }
        //}

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
            if ("Start:" == strList[0])
            {
                Start(strList[1]);
            }
            else if ("LeftMouseDown:" == strList[0])
            {
                
                if (strList.Count<3)
                {
                    ClickLeftMouse(strList[1], "");
                }
                else
                {
                    ClickLeftMouse(strList[1], strList[2]);
                }
                
            }
            else if ("RightMouseDown:" == strList[0])
            {
                ClickRightMouse(strList[1], strList[2]);
            }
            else if ("DoubleMouseDown:" == strList[0])
            {
                DoubleMouseDown(strList[1], strList[2]);
            }
            else if ("Stop:" == strList[0])
            {
                return false;
            }
            else if ("SetFocus:"==strList[0])
            {

            }
            else if ("KeyDown:"==strList[0])
            {
                KeyDown(strList[1]);
            }
            else if ("ShellCreate:" == strList[0])
            {
                WaitWindow(strList[1]);
            }
            else if ("Activate:" == strList[0])
            {
                ActivateWindow(strList[1], strList[2]);
            }
            
            return true;
        }

        public void ActivateWindow(string name, string type)
        {
            Condition cond1 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
            Condition cond2 = new PropertyCondition(AutomationElement.NameProperty, name);
            Condition cond3 = new PropertyCondition(AutomationElement.LocalizedControlTypeProperty,type);
            //int count = 0;
            //while (count<10)
            //{
            //    AutomationElement targetElem = AutomationElement.RootElement.FindFirst(TreeScope.Subtree, new AndCondition(cond1, cond2, cond3));
            //    if (targetElem != null)
            //    {
            //        mainWindow = targetElem;
            //        return;
            //    }
            //    Thread.Sleep(200);
            //    count++;
            //}
            //if (name == "Material Library Manger")
            //{
            //    AutomationElement elent = AutomationElement.FocusedElement;
            //    string tmp = elent.Current.Name;
            //}
            Thread.Sleep(200);

            AutomationElement focusElement = AutomationElement.FocusedElement;
            
            AutomationElement node = focusElement;
            TreeWalker walker = TreeWalker.ControlViewWalker;
            AutomationElement elementParent;
            
            if (focusElement == AutomationElement.RootElement) return ;

            //AutomationElement elementParent = focusElement.FindFirst(TreeScope.Ancestors,new AndCondition(cond1,cond2,cond3));
            //if (elementParent != null)
            //{
            //    mainWindow = elementParent;
            //}
            do
            {
                elementParent = walker.GetParent(node);
                object controlTypeNoDefault = elementParent.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty, true);
                if (controlTypeNoDefault == AutomationElement.NotSupported)
                {
                    return;
                }
                ControlType controlType = controlTypeNoDefault as ControlType;

                if (type == controlType.LocalizedControlType&&elementParent.Current.Name==name|| elementParent == AutomationElement.RootElement)
                {
                    mainWindow = elementParent;
                    //if (name == "Material Library Manger")
                    //{
                    //    string tmp = mainWindow.Current.Name;
                    //}
                    break;
                }

                node = elementParent;
            }
            while (true);

            
        }

        public void WaitWindow(string name)
        {
            Condition cond1 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
            Condition cond2 = new PropertyCondition(AutomationElement.NameProperty, name);
            Condition cond3 = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window);

            
            while (true)
            {
                AutomationElement targetElem = AutomationElement.RootElement.FindFirst(TreeScope.Subtree, new AndCondition(cond1,cond2,cond3));
                if (targetElem!=null)
                {
                    mainWindow = targetElem;
                    break;
                }
            }
        }

        public void ClickLeftMouse(string name, string type)
        {

            AutomationElement element = FindElement(process.Id, name, type);

            
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
           

        }

        public void ClickRightMouse(string name,string type)
        {

            AutomationElement element = FindElement(process.Id, name, type);

            if (element == null)
            {

                throw new NullReferenceException(string.Format("Element with AutomationId '{0}' and Name '{1}' can not be find.",

                    element.Current.AutomationId, element.Current.Name));

            }

            Rect rect = element.Current.BoundingRectangle;

            int IncrementX = (int)(rect.Left + rect.Width / 2);

            int IncrementY = (int)(rect.Top + rect.Height / 2);

            //Make the cursor position to the element.

            SetCursorPos(IncrementX, IncrementY);

            //Make the left mouse down and up.

            mouse_event(MOUSEEVENTF_RIGHTDOWN, IncrementX, IncrementY, 0, UIntPtr.Zero);

            mouse_event(MOUSEEVENTF_RIGHTUP, IncrementX, IncrementY, 0, UIntPtr.Zero);

        }

        public void DoubleMouseDown(string name, string type)
        {
            AutomationElement element = FindElement(process.Id, name, type);

            if (element == null)
            {
                return;
                //throw new NullReferenceException(string.Format("Element with AutomationId '{0}' and Name '{1}' can not be find.",

                //    element.Current.AutomationId, element.Current.Name));

            }

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
            else if (type == ControlType.Button||type == ControlType.MenuBar||type == ControlType.MenuItem)
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

        public void KeyDown(string key)
        {
            object valuePattern = null;
            AutomationElement element = AutomationElement.FocusedElement;
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out valuePattern))
            {
                if ("NumPad" == key.Substring(0,6))
                {
                    key = key.Substring(6);
                }
                ((ValuePattern)valuePattern).SetValue(key);
            }
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

        public static AutomationElement FindElement(int processId, string name, string type)
        {

            //AutomationElement aeForm = FindWindowByProcessId(processId);

            Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
            Condition idcondition = new PropertyCondition(AutomationElement.ProcessIdProperty, process.Id);
            Condition nameCondition = new PropertyCondition(AutomationElement.NameProperty, name);
            Condition typeCondition = new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, type); 

            

            Condition conditions = new AndCondition(nameCondition,typeCondition,idcondition,condition2,condition1);

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

            int index = str.IndexOf(" ");
            if (index<0)
            {
                return null;
            }
            string cmd = str.Substring(0, index);
            int startIndex = str.IndexOf("\"", index + 1);
            int endIndex = str.IndexOf("\"", startIndex + 1);
            if (startIndex==-1||endIndex==-1)
            {
                return new string[] { cmd };
            }
            string arg1 = str.Substring(startIndex+1, endIndex - startIndex-1);

            startIndex = str.IndexOf("\"",endIndex+1);
            endIndex = str.IndexOf("\"", startIndex + 1);
            if (startIndex==-1||endIndex==-1)
            {
                return new string[] { cmd, arg1 };
               
            }
            string arg2 = str.Substring(startIndex+1, endIndex - startIndex-1);

            string[] split = new string[] { cmd, arg1, arg2 };
            return split;
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

    }
}
