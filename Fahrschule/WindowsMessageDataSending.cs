using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ZusiCLIProject.Utils
{
	/// <summary>Represents a Windows-Message-System controled Window</summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
	[System.Diagnostics.DebuggerDisplay("MSWindow, Name = {Name}, Process = {Process.ProcessName}")]
	public struct WindowsMessageDataSending
    {
		[System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindow", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
		private static extern System.IntPtr GetWindow(System.IntPtr hwnd, int wCmd);
		[System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetDesktopWindow", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
		private static extern System.IntPtr GetDesktopWindow();
		[System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowTextA", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
		private static extern int GetWindowText(System.IntPtr hwnd, byte[] lpString, int cch);
		[System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
		private static extern System.IntPtr GetWindowThreadProcessId(System.IntPtr hwnd, ref System.IntPtr lpdwProcessId);

		/// <summary>Searchs for all Top-Level-Windows with the following owner. Faster then filtering all Top-Level-Windows by process.</summary>
		public static WindowsMessageDataSending[] GetDesktopWindowHandlesByProcess(System.Diagnostics.Process Process)
		{
			return GetWindowHandlesFiltered(GetDesktopWindow(), Process);
		}
		private static WindowsMessageDataSending[] GetWindowHandlesFiltered(System.IntPtr ip, System.Diagnostics.Process Process)
		{
			System.IntPtr pidcompare = System.IntPtr.Zero;
			if (Process != null)
				pidcompare = new System.IntPtr(Process.Id);
			ip = GetWindow(ip, 5);
			var l = new System.Collections.Generic.List<WindowsMessageDataSending>();
			byte[] b = new byte[256];
			for (; ip != System.IntPtr.Zero; ip = GetWindow(ip, 2))
			{
				//Allgemein
				var cur = new WindowsMessageDataSending();
				cur.Handle = ip;

				//Prozess
				System.IntPtr thread = System.IntPtr.Zero;
				System.IntPtr processPtr = System.IntPtr.Zero;
				thread = GetWindowThreadProcessId(cur.Handle, ref processPtr);
				if (Process == null)
				{
					try
					{
						cur.Process = System.Diagnostics.Process.GetProcessById(processPtr.ToInt32());
					}
					catch (System.ArgumentException)
					{
						cur.Process = null;
					}
				}
				else
				{
					if (processPtr != pidcompare)
						continue;
					cur.Process = Process;
				}

				//Name
				int ln = b.Length - 1;
				ln = GetWindowText(ip, b, ln);
				cur.Name = System.Text.Encoding.Default.GetString(b, 0, ln);

				//Zur Liste hinzufügen
				l.Add(cur);
			}
			return l.ToArray();
		}


		/// <summary>The Handle of the window.</summary>
		public System.IntPtr Handle { get; set; }
		/// <summary>The title or another simmilar name of the Window.</summary>
		public string Name { get; set; }
		/// <summary>The Process owning the window.</summary>
		public System.Diagnostics.Process Process { get; set; }

		[System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "PostMessage", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = false)]
		private static extern bool PostMessage(System.IntPtr hWnd, uint Msg, System.IntPtr wParam, System.IntPtr lParam); //ToDo: Geht nicht mit ExactSpelling.
		/// <summary>Simulates pressing or releasing a key.</summary>
		public bool SendKeyState(System.Windows.Forms.Keys key, bool DirectionUp)
		{
			uint msgparam;
			System.IntPtr wparam;
			System.IntPtr lparam;
			if (DirectionUp)
			{
				msgparam = 0x101;
				lparam = new System.IntPtr(0x40190001 + int.MinValue); //0xC0190001 //0xC0000001
			}
			else
			{
				msgparam = 0x100;
				lparam = new System.IntPtr(0x190001); //0x1
			}
			wparam = new System.IntPtr((int)key);

			return PostMessage(Handle, msgparam, wparam, lparam);
		}
	}
}
