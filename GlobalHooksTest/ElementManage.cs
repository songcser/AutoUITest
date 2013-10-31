using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading;
using System.Diagnostics;

namespace GlobalHooksTest
{
    class ElementManage
    {
        private static List<AutomationElement> elementList = null;
        private static List<AutomationElement> childElemList = null;
        private static AutomationElementCollection elements = null;

        private CacheRequest fetchRequest;

        private static Process process = null;

        public delegate bool CallBack(IntPtr hwnd, int lParam);

        

        [DllImport("user32.dll")]
        public static extern int EnumWindows(CallBack lpfn, int lParam);
        public static CallBack callBackEnumWindows = new CallBack(WindowProcess);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint lpdwProcessId);

        public ElementManage()
        {
            elementList = new List<AutomationElement>();
            childElemList = new List<AutomationElement>();
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
            process = Process.Start(strPath);
            //AutomationElement aeDeskTop = AutomationElement.RootElement;
            Thread.Sleep(500);
            //AutomationElement aeForm = null;
            int times = 0;
            process.WaitForInputIdle();
            while (process.MainWindowHandle == null || process.MainWindowHandle == IntPtr.Zero)
            {
                Thread.Sleep(1000);
                if (times > 5 && process.Handle != IntPtr.Zero)
                {
                    break;
                }
                times++;

            }
            if (process.MainWindowHandle == IntPtr.Zero)
            {
                EnumWindows(callBackEnumWindows, 0);
            }
            else
            {
                elementList.Add(AutomationElement.FromHandle(process.MainWindowHandle));
            }

            Condition condition1 = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
            Condition condition2 = new PropertyCondition(AutomationElement.IsEnabledProperty, true);
            Condition condition = new PropertyCondition(AutomationElement.ProcessIdProperty, process.Id);
            //AutomationElement aeForm = AutomationElement.FromHandle(process.MainWindowHandle);
            fetchRequest = new CacheRequest();
            fetchRequest.Add(AutomationElement.NameProperty);
            fetchRequest.Add(AutomationElement.AutomationIdProperty);
            fetchRequest.Add(AutomationElement.ControlTypeProperty);
            fetchRequest.Add(AutomationElement.BoundingRectangleProperty);

            using (fetchRequest.Activate())
            {
                foreach (AutomationElement aeForm in elementList)
                {
                    AutomationElementCollection tElemList = aeForm.FindAll(TreeScope.Subtree, new AndCondition(condition, condition1, condition2));
                    //elements = aeForm.FindAll(TreeScope.Subtree, new AndCondition(condition1, condition2, condition));
                    ///elements.CopyTo(temp, temp.Count);
                    for (int i = 0; i < tElemList.Count; i++)
                    {
                        childElemList.Add(tElemList[i]);
                    }
                }
                //WalkEnabledElements(aeDeskTop);

            }
        }

        public string GetElementInfo(Point point)
        {
            System.Windows.Point wpt = new System.Windows.Point(point.X, point.Y);
            try
            {
                AutomationElement autoElem = AutomationElement.FromPoint(wpt);
                //autoElem.SetFocus();
                //AutomationElement autoElem = AutomationElement.FocusedElement;
                if (autoElem == null)
                {
                    //AddText("Automation null");
                    return "Automation null";
                }
                string name = "[" + autoElem.Current.Name;
                object controlTypeNoDefault = autoElem.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty, true);
                if (controlTypeNoDefault != AutomationElement.NotSupported)
                {
                    ControlType controlType = controlTypeNoDefault as ControlType;
                    name += "] [" + controlType.LocalizedControlType + "]";
                }
                //string name = "null";
                //string msg = string.Format("Mouse event: {0}-->{1}: ({2},{3}).,{4}", mEvent.ToString(), name, point.X, point.Y, hwnd);
                return name;
            }
            catch (System.Exception ex)
            {
                AutomationElement targetElem = null;

                targetElem = GetElementsByPoint(point);
                if (targetElem == null)
                {
                    //AddText("get element error");
                    return "get element error";
                }
                string name = "[" + targetElem.Cached.Name;
                object controlTypeNoDefault = targetElem.GetCachedPropertyValue(AutomationElement.ControlTypeProperty, true);
                if (controlTypeNoDefault != AutomationElement.NotSupported)
                {
                    ControlType controlType = controlTypeNoDefault as ControlType;
                    name += "] [" + controlType.LocalizedControlType + "]";
                }
                //string msg = string.Format("Mouse event: {0}-->{1}: ({2},{3}).,{4}", mEvent.ToString(), name, point.X, point.Y, hwnd);
                return name;
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
            string name = process.MainWindowTitle;
            GetWindowThreadProcessId(hwnd, ref processId);
            if (processId == process.Id && title.Length > 0)
            {
                AutomationElement mainElement = AutomationElement.FromHandle(hwnd);
                if (mainElement != null)
                {
                    elementList.Add(mainElement);
                }

                //elementList.Add(mainElement);
            }
            if (hwnd == process.MainWindowHandle || hwnd == process.Handle)
            {
                //mainElement = AutomationElement.FromHandle(hwnd);

            }
            return true;
        }
    }
}
