namespace JiayiLauncher.State.Mods.Actions;

public class CreateModCollectionAction
{
	public string ModCollectionPath { get; }
	
	public CreateModCollectionAction(string modCollectionPath)
	{
		ModCollectionPath = modCollectionPath;
	}
}