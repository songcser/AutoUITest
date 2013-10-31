using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text;
using System.Runtime.InteropServices;

namespace GlobalHooksTest
{
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button BtnInitCbt;
		private System.Windows.Forms.Button BtnUninitCbt;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button BtnUninitShell;
		private System.Windows.Forms.Button BtnInitShell;
		private System.Windows.Forms.ListBox ListCbt;
		private System.Windows.Forms.ListBox ListShell;
		private System.Windows.Forms.Label LblMouse;
		private System.ComponentModel.Container components = null;
        private TextBox textBox1;
        private Button button1;

		// API calls to give us a bit more information about the data we get from the hook
		[DllImport("user32.dll")]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder title, int size);
		[DllImport("user32.dll")]
		private static extern uint RealGetWindowClass(IntPtr hWnd, StringBuilder pszType, uint cchType);

        private ElementManage elemManage;
		private GlobalHooks _GlobalHooks;
        private static string strPath;
        private static string filePath;
        private static StringBuilder log;

		public Form1()
		{
			InitializeComponent();

			// Instantiate our GlobalHooks object
			_GlobalHooks = new GlobalHooks(this.Handle);

			// Set the hook events
			_GlobalHooks.CBT.Activate += new GlobalHooksTest.GlobalHooks.WindowEventHandler(_GlobalHooks_CbtActivate);
			_GlobalHooks.CBT.CreateWindow += new GlobalHooksTest.GlobalHooks.WindowEventHandler(_GlobalHooks_CbtCreateWindow);
			_GlobalHooks.CBT.DestroyWindow += new GlobalHooksTest.GlobalHooks.WindowEventHandler(_GlobalHooks_CbtDestroyWindow);
            _GlobalHooks.CBT.MoveSize += new GlobalHooksTest.GlobalHooks.WindowEventHandler(_GlobalHooks_CbtMoveSize);
			_GlobalHooks.Shell.WindowActivated += new GlobalHooksTest.GlobalHooks.WindowEventHandler(_GlobalHooks_ShellWindowActivated);
			_GlobalHooks.Shell.WindowCreated += new GlobalHooksTest.GlobalHooks.WindowEventHandler(_GlobalHooks_ShellWindowCreated);
			_GlobalHooks.Shell.WindowDestroyed += new GlobalHooksTest.GlobalHooks.WindowEventHandler(_GlobalHooks_ShellWindowDestroyed);
			_GlobalHooks.Shell.Redraw += new GlobalHooksTest.GlobalHooks.WindowEventHandler(_GlobalHooks_ShellRedraw);
			_GlobalHooks.MouseLL.MouseMove += new MouseEventHandler(MouseLL_MouseMove);
            _GlobalHooks.MouseLL.MouseDown += new MouseEventHandler(MouseLL_MouseDown);
            _GlobalHooks.MouseLL.MouseUp += new MouseEventHandler(MouseLL_MouseUp);

            elemManage = new ElementManage();
            log = new StringBuilder();
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				// Make sure we stop hooking before quitting! Otherwise, the hook procedures in GlobalCbtHook.dll
				// will keep being called, even after our application quits, which will needlessly degrade system
				// performance. And it's just plain sloppy.
				_GlobalHooks.CBT.Stop();
				_GlobalHooks.Shell.Stop();
				_GlobalHooks.MouseLL.Stop();

				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ListCbt = new System.Windows.Forms.ListBox();
            this.BtnUninitCbt = new System.Windows.Forms.Button();
            this.BtnInitCbt = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ListShell = new System.Windows.Forms.ListBox();
            this.BtnUninitShell = new System.Windows.Forms.Button();
            this.BtnInitShell = new System.Windows.Forms.Button();
            this.LblMouse = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ListCbt);
            this.groupBox1.Controls.Add(this.BtnUninitCbt);
            this.groupBox1.Controls.Add(this.BtnInitCbt);
            this.groupBox1.Location = new System.Drawing.Point(4, 60);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(280, 280);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "CBT Hooks";
            // 
            // ListCbt
            // 
            this.ListCbt.Location = new System.Drawing.Point(8, 56);
            this.ListCbt.Name = "ListCbt";
            this.ListCbt.Size = new System.Drawing.Size(264, 212);
            this.ListCbt.TabIndex = 3;
            // 
            // BtnUninitCbt
            // 
            this.BtnUninitCbt.Location = new System.Drawing.Point(144, 16);
            this.BtnUninitCbt.Name = "BtnUninitCbt";
            this.BtnUninitCbt.Size = new System.Drawing.Size(128, 32);
            this.BtnUninitCbt.TabIndex = 2;
            this.BtnUninitCbt.Text = "Uninitialize CBT Hooks";
            this.BtnUninitCbt.Click += new System.EventHandler(this.BtnUninitCbt_Click);
            // 
            // BtnInitCbt
            // 
            this.BtnInitCbt.Location = new System.Drawing.Point(8, 16);
            this.BtnInitCbt.Name = "BtnInitCbt";
            this.BtnInitCbt.Size = new System.Drawing.Size(128, 32);
            this.BtnInitCbt.TabIndex = 1;
            this.BtnInitCbt.Text = "Initialize CBT Hooks";
            this.BtnInitCbt.Click += new System.EventHandler(this.BtnInitCbt_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ListShell);
            this.groupBox2.Controls.Add(this.BtnUninitShell);
            this.groupBox2.Controls.Add(this.BtnInitShell);
            this.groupBox2.Location = new System.Drawing.Point(284, 60);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(296, 280);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Shell Hooks";
            // 
            // ListShell
            // 
            this.ListShell.Location = new System.Drawing.Point(8, 56);
            this.ListShell.Name = "ListShell";
            this.ListShell.Size = new System.Drawing.Size(280, 212);
            this.ListShell.TabIndex = 3;
            // 
            // BtnUninitShell
            // 
            this.BtnUninitShell.Location = new System.Drawing.Point(152, 16);
            this.BtnUninitShell.Name = "BtnUninitShell";
            this.BtnUninitShell.Size = new System.Drawing.Size(136, 32);
            this.BtnUninitShell.TabIndex = 2;
            this.BtnUninitShell.Text = "Uninitialize Shell Hooks";
            this.BtnUninitShell.Click += new System.EventHandler(this.BtnUninitShell_Click);
            // 
            // BtnInitShell
            // 
            this.BtnInitShell.Location = new System.Drawing.Point(8, 16);
            this.BtnInitShell.Name = "BtnInitShell";
            this.BtnInitShell.Size = new System.Drawing.Size(136, 32);
            this.BtnInitShell.TabIndex = 1;
            this.BtnInitShell.Text = "Initialize Shell Hooks";
            this.BtnInitShell.Click += new System.EventHandler(this.BtnInitShell_Click);
            // 
            // LblMouse
            // 
            this.LblMouse.Location = new System.Drawing.Point(12, 348);
            this.LblMouse.Name = "LblMouse";
            this.LblMouse.Size = new System.Drawing.Size(272, 23);
            this.LblMouse.TabIndex = 4;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(30, 13);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(453, 20);
            this.textBox1.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(497, 10);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Open";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(592, 392);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.LblMouse);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Global Hooks Test";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void BtnInitCbt_Click(object sender, System.EventArgs e)
		{
            elemManage.StartProcess(strPath);

			_GlobalHooks.CBT.Start();
            AddText("CBT hook Adding");
            _GlobalHooks.MouseLL.Start();
            AddText("MouseLL hook Adding");
		}

		private void BtnUninitCbt_Click(object sender, System.EventArgs e)
		{
			_GlobalHooks.CBT.Stop();
            AddText("CBT hook Remove");
            _GlobalHooks.MouseLL.Stop();
            AddText("MouseLL hook Remove");
		}

		private void BtnInitShell_Click(object sender, System.EventArgs e)
		{
			_GlobalHooks.Shell.Start();
		}

		private void BtnUninitShell_Click(object sender, System.EventArgs e)
		{
			_GlobalHooks.Shell.Stop();
		}

		protected override void WndProc(ref Message m)
		{
			// Check to see if we've received any Windows messages telling us about our hooks
			if (_GlobalHooks != null)
				_GlobalHooks.ProcessWindowMessage(ref m);

			base.WndProc (ref m);
		}

		#region Windows API Helper Functions

		private string GetWindowName(IntPtr Hwnd)
		{
			// This function gets the name of a window from its handle
			StringBuilder Title = new StringBuilder(256);
			GetWindowText(Hwnd, Title, 256);

			return Title.ToString().Trim();
		}

		private string GetWindowClass(IntPtr Hwnd)
		{
			// This function gets the name of a window class from a window handle
			StringBuilder Title = new StringBuilder(256);
			RealGetWindowClass(Hwnd, Title, 256);

			return Title.ToString().Trim();
		}

		#endregion

		private void _GlobalHooks_CbtActivate(IntPtr Handle)
		{
		//	this.ListCbt.Items.Add("Activate: " + GetWindowName(Handle));
		}

		private void _GlobalHooks_CbtCreateWindow(IntPtr Handle)
		{
		//	this.ListCbt.Items.Add("Create: " + GetWindowClass(Handle));
		}

		private void _GlobalHooks_CbtDestroyWindow(IntPtr Handle)
		{
		//	this.ListCbt.Items.Add("Destroy: " + GetWindowName(Handle));
		}

        private void _GlobalHooks_CbtMoveSize(IntPtr Handle)
        {
            string name = GetWindowName(Handle);
            elemManage.UpdateCache();
            
            AddText("MoveSize:" + name);
        }

		private void _GlobalHooks_ShellWindowActivated(IntPtr Handle)
		{
			//this.ListShell.Items.Add("Activated: " + GetWindowName(Handle));
		}

		private void _GlobalHooks_ShellWindowCreated(IntPtr Handle)
		{
			this.ListShell.Items.Add("Created: " + GetWindowName(Handle));
		}

		private void _GlobalHooks_ShellWindowDestroyed(IntPtr Handle)
		{
			this.ListShell.Items.Add("Destroyed: " + GetWindowName(Handle));
		}

		private void _GlobalHooks_ShellRedraw(IntPtr Handle)
		{
			this.ListShell.Items.Add("Redraw: " + GetWindowName(Handle));
		}

		private void MouseLL_MouseMove(object sender, MouseEventArgs e)
		{
			this.LblMouse.Text = "Mouse at: " + e.X + ", " + e.Y;
		}

        private void MouseLL_MouseDown(object sender, MouseEventArgs e)
        {
            //this.ListCbt.Items.Add("MouseDown"+e.X+","+e.Y);
            string message = elemManage.GetElementInfo(new Point(e.X, e.Y));
           // string msg = string.Format("Mouse event: {0}-->{1}: ({2},{3}).,{4}", mEvent.ToString(), name, point.X, point.Y, hwnd)
            //this.ListCbt.Items.Add("MouseDown:"+ message + e.X + "," + e.Y);
            string msg = string.Format("{0}MouseDown: {1} ({2},{3})",e.Button.ToString(), message, e.X, e.Y);
            AddText(msg);
        }

        private void MouseLL_MouseUp(object sender, MouseEventArgs e)
        {
            string message = elemManage.GetElementInfo(new Point(e.X, e.Y));
            // string msg = string.Format("Mouse event: {0}-->{1}: ({2},{3}).,{4}", mEvent.ToString(), name, point.X, point.Y, hwnd)
            //this.ListCbt.Items.Add("MouseDown:"+ message + e.X + "," + e.Y);
            string msg = string.Format("{0}MouseUp: {1} ({2},{3})", e.Button.ToString(), message, e.X, e.Y);
            AddText(msg);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "可执行文件|*.exe*|所有文件|*.*";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = openFileDialog.FileName;
                strPath = openFileDialog.FileName;
            }
        }

        private void AddText(string message)
        {
            this.ListCbt.Items.Add(message);
            log.Append(message);
        }
	}
}
