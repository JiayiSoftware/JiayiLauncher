using System;
using System.Runtime.InteropServices;

namespace JiayiLauncher.Utils;

public static partial class Imports
{
	// constants
	public const int PROCESS_CREATE_THREAD = 0x0002;
	public const int PROCESS_QUERY_INFORMATION = 0x0400;
	public const int PROCESS_VM_OPERATION = 0x0008;
	public const int PROCESS_VM_WRITE = 0x0020;
	public const int PROCESS_VM_READ = 0x0010;

	public const uint MEM_COMMIT = 0x00001000;
	public const uint MEM_RESERVE = 0x00002000;
	public const uint PAGE_READWRITE = 4;
	
	public const uint WM_COPYDATA = 0x004A;

	// structs
	[StructLayout(LayoutKind.Sequential)]
	public struct CopyData
	{
		public nint dwData;
		public uint cbData;
		public nint lpData;
	}
	
	// functions
	[LibraryImport("kernel32.dll")]
	public static partial nint OpenProcess(int dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

	[LibraryImport("kernel32.dll")]
	public static partial nint GetModuleHandleW([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

	[LibraryImport("kernel32", SetLastError = true)]
	public static partial nint GetProcAddress(nint hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

	[LibraryImport("kernel32.dll", SetLastError = true)]
	public static partial nint VirtualAllocEx(nint hProcess, nint lpAddress, uint dwSize,
		uint flAllocationType, uint flProtect);
	
	[LibraryImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static partial bool WriteProcessMemory(nint hProcess, nint lpBaseAddress, byte[] lpBuffer, uint nSize,
		out nuint lpNumberOfBytesWritten);

	[LibraryImport("kernel32.dll")]
	public static partial nint CreateRemoteThread(nint hProcess, nint lpThreadAttributes, uint dwStackSize,
		nint lpStartAddress, nint lpParameter, uint dwCreationFlags, nint lpThreadId);
	
	// because LibraryImport just dies when it comes to this function
	[DllImport("user32.dll")]
	public static extern int SendMessage(nint hWnd, int Msg, int wParam, long lParam);
	
	[LibraryImport("user32.dll")]
	public static partial nint FindWindowW([MarshalAs(UnmanagedType.LPWStr)] string? lpClassName, [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName);
	
	[LibraryImport("ole32.dll")]
	public static partial int CoCreateInstance(ref Guid rclsid, nint pUnkOuter, uint dwClsContext, ref Guid riid, out nint ppv);
	
	[LibraryImport("user32.dll")]
	public static partial IntPtr SetWindowLongPtrA(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
	
	[LibraryImport("user32.dll")]
	public static partial int CallWindowProcA(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, UIntPtr wParam, IntPtr lParam);
}