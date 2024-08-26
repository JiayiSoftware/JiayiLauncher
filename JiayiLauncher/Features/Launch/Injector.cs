using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using JiayiLauncher.Features.Game;
using JiayiLauncher.Utils;
using static JiayiLauncher.Utils.Imports;

namespace JiayiLauncher.Features.Launch;

public class Injector
{
	private void ApplyPermissions(string file)
	{
		var fileInfo = new FileInfo(file);
		var fileSecurity = fileInfo.GetAccessControl();
		fileSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier("S-1-15-2-1"),
			FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.NoPropagateInherit,
			AccessControlType.Allow));
	
		fileInfo.SetAccessControl(fileSecurity);
	}

	public async Task<bool> Inject(string path)
	{
		var log = Singletons.Get<Log>();
		var minecraft = Singletons.Get<Minecraft>();
		
		ApplyPermissions(path);

		return await Task.Run(() =>
		{
			log.Write(nameof(Injector), $"Injecting {path}");

			var process = minecraft.Process;
			var processHandle = OpenProcess(
				PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION
				| PROCESS_VM_WRITE | PROCESS_VM_READ, false, process.Id);

			// ERROR CHECKING (i have never done this in any of the injectors i've made)
			if (processHandle == 0)
			{
				log.Write(nameof(Injector), "OpenProcess returned null", Log.LogLevel.Error);
				return false;
			}

			var loadLibraryAddress = GetProcAddress(GetModuleHandleW("kernel32.dll"), "LoadLibraryA");

			if (loadLibraryAddress == 0)
			{
				log.Write(nameof(Injector),
					"Couldn't get the address of LoadLibraryA, maybe the user's antivirus is hiding it from us",
					Log.LogLevel.Error);
				return false;
			}

			var allocatedMemory = VirtualAllocEx(processHandle, 0, (uint)((path.Length + 1) * Marshal.SizeOf<char>()),
				MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

			if (allocatedMemory == 0)
			{
				log.Write(nameof(Injector), "Failed to allocate memory needed for injection", Log.LogLevel.Error);
				return false;
			}

			var result = WriteProcessMemory(processHandle, allocatedMemory, Encoding.Default.GetBytes(path),
				(uint)((path.Length + 1) * Marshal.SizeOf<char>()), out _);

			if (!result)
			{
				log.Write(nameof(Injector), "Failed to write to allocated memory", Log.LogLevel.Error);
				return false;
			}

			var thread = CreateRemoteThread(processHandle, 0, 0, loadLibraryAddress, allocatedMemory, 0, 0);

			if (thread == 0)
			{
				log.Write(nameof(Injector), "Failed to create remote thread", Log.LogLevel.Error);
				return false;
			}
			
			// check if the game is open after injection because some antiviruses will close the game if they detect it
			if (minecraft.IsOpen)
			{
				// wait just a bit for the module to load
				Task.Delay(1000).Wait();
				if (!IsInjected(path))
				{
					log.Write(nameof(Injector), "Every native call succeeded, but the module wasn't loaded.",
						Log.LogLevel.Error);
					return false;
				}
				
				log.Write(nameof(Injector), $"Successfully injected {path}");
				return true;
			}

			log.Write(nameof(Injector), "The game was terminated by the user's antivirus", Log.LogLevel.Error);
			return false;
		});
	}
	
	public bool IsInjected(string path)
	{
		var process = Singletons.Get<Minecraft>().Process;
		process.Refresh();
		return process.Modules.Cast<ProcessModule>().Any(m => m.FileName == path);
	}
}