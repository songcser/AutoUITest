using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Threading;
using System.Windows.Automation;

namespace GlobalHooksTest
{
    public partial class CompareForm : Form
    {
        [DllImport("user32")]
        private static extern bool ReleaseCapture();
        [DllImport("user32")]
        private static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        
        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursorFromFile(string fileName);
        [DllImport("user32.dll")]
        public static extern IntPtr SetCursor(IntPtr cursorHandle);

        [DllImport("user32.dll")]
        public static extern uint DestroyCursor(IntPtr cursorHandle);
        [DllImport("User32.dll")]
        public extern static System.IntPtr GetDC(System.IntPtr hWnd);

        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MOVE = 0Xf010;
        private const int HTCAPTION = 0x0002;

        //private Cursor myCursor;
        private IntPtr cursorHandle;
        private Thread workerThread;
        private ElementManage elemManage;
        private AutomationElement parentElem;
        private int offsetX;
        private int offsetY;
        
        private string elemInfo;
        private string autoId;
        //private bool threadFlag;
        //Graphics g;
        Rectangle rectTemp;

        public CompareForm()
        {
            InitializeComponent();
            
            elemManage = new ElementManage();
            //form1 = new Form1();
            //callbackMsg = new CallBackMessage(form1._GetMessageFrom);
            //System.IntPtr DesktopHandle = GetDC(System.IntPtr.Zero);
            //g = Graphics.FromHdc(DesktopHandle);

            //threadFlag = false;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                cursorHandle = Cursor.Current.Handle;
                Cursor myCursor = new Cursor(cursorHandle);
                IntPtr colorCursorHandle = LoadCursorFromFile("cursor1.cur");
                myCursor.GetType().InvokeMember("handle", BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.SetField, null, myCursor,
                    new object[] { colorCursorHandle });
                this.Cursor = myCursor;
                this.pictureBox1.Hide();
                //threadFlag = true;
                ThreadStart threadDelegate = new ThreadStart(StartWorkerThread);
                workerThread = new Thread(threadDelegate);
                workerThread.Start();
                
                //this.pictureBox2.Show();
            }
            
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
//             if (ismousedown)
//             {
//                 int left = pictureBox1.Left + e.X - zhizheng.X;
//                 int Right = pictureBox1.Top + e.Y - zhizheng.Y;
//                 this.pictureBox1.Location = new Point(left, Right);
//             }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Cursor myCursor = new Cursor(cursorHandle);
                this.Cursor = myCursor;
                this.pictureBox1.Show();
                //threadFlag = false;
                //this.pictureBox2.Hide();
                workerThread.Abort();
                //g.Dispose();
                ControlPaint.DrawReversibleFrame(rectTemp, Color.Red, FrameStyle.Thick);
            }
        }

        private void StartWorkerThread()
        {
            //int count = 0;
            AutomationElement autoElem = null;
            while (true)
            {
                int x = Control.MousePosition.X;
                int y = Control.MousePosition.Y;
                System.Windows.Point wpt = new System.Windows.Point(x, y);
                autoElem = AutomationElement.FromPoint(wpt);
                string name = autoElem.Current.Name;
                System.Windows.Rect rect = autoElem.Current.BoundingRectangle;
                Rectangle r = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
                if (rectTemp!=null)
                {
                    ControlPaint.DrawReversibleFrame(rectTemp, Color.Red, FrameStyle.Thick);
                }

                ControlPaint.DrawReversibleFrame(r, Color.Red, FrameStyle.Thick);
                rectTemp = new Rectangle(r.Location, r.Size);
                this.control.Text = name;
                try
                {
                    ValuePattern value = elemManage.GetValuePattern(autoElem);
                    this.expectedvalue.Text = value.Current.Value;
                }
                catch (System.Exception ex)
                {
                    this.expectedvalue.Text = name;
                }
                this.type.Text = autoElem.Current.LocalizedControlType;
                autoId = autoElem.Current.AutomationId;
                this.automationId.Text = autoId;
                parentElem = elemManage.GetParentElement(autoElem);
                System.Windows.Rect pr = parentElem.Current.BoundingRectangle;
                offsetX = (int)(rect.X - pr.X);
                offsetY = (int)(rect.Y - pr.Y);
                this.offset.Text = offsetX + "," + offsetY;
                elemInfo = "\"" + autoId + "\"";
                Thread.Sleep(200);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string expected = this.expectedvalue.Text;
            string tolerance = this.tolerance.Text;
            elemInfo = "Compare " + elemInfo + expected + "\"" + tolerance + "\"";
            //callbackMsg("Compare " + elemInfo + "\"" + expected + "\"" + tolerance + "\"");
            //SendMessageBack("Compare "+elemInfo+"\""+expected+"\""+tolerance+"\"");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public string GetCompareInfo()
        {
            return elemInfo;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            elemInfo = "\"" + autoId + "\"";
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            elemInfo = elemManage.GetCurrentElementInfo(parentElem)+offsetX+"\""+offsetY+"\"";
        }
    }
}
