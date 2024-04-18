using Microsoft.AspNetCore.Components;

namespace JiayiLauncher.Shared;

public class JiayiPage : ComponentBase
{
	// lets us know if this page is doing something so we can cache the page and not lose state
	public bool TaskRunning { get; set; }
	
	protected virtual void OnPageShow() { }
	protected virtual void OnPageHide() { }
}