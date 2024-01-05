using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using JiayiLauncher.Utils;

namespace JiayiLauncher.Features.Game;

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("f27c3930-8029-4ad1-94e3-3dba417810c1")]
public partial interface IPackageDebugSettings
{
	void EnableDebugging(string packageFullName, string? debuggerCommandLine, string? environment);
	void DisableDebugging(string packageFullName);
}

[GeneratedComClass]
public partial class PackageDebugSettings : IPackageDebugSettings
{
	private readonly IPackageDebugSettings _interface;

	public PackageDebugSettings()
	{
		ComWrappers wrappers = new StrategyBasedComWrappers();
		
		var classId = new Guid("b1aec16f-2383-4852-b0e9-8f0b1dc66b4d");
		var interfaceId = new Guid("f27c3930-8029-4ad1-94e3-3dba417810c1");
		var hr = Imports.CoCreateInstance(
			ref classId,
			nint.Zero, 
			1, // CLSCTX_INPROC_SERVER
			ref interfaceId,
			out var obj);
		
		if (hr != 0) Marshal.ThrowExceptionForHR(hr);
		
		_interface = (IPackageDebugSettings)wrappers.GetOrCreateObjectForComInstance(obj, CreateObjectFlags.None);
	}
	
	public void EnableDebugging(string packageFullName, string? debuggerCommandLine, string? environment)
	{
		_interface.EnableDebugging(packageFullName, debuggerCommandLine, environment);
	}
	
	public void DisableDebugging(string packageFullName)
	{
		_interface.DisableDebugging(packageFullName);
	}
}