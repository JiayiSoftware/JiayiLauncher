namespace JiayiLauncher.Utils;

public static class Singletons
{
	private static readonly List<object> _singletons = new();
	
	public static T Add<T>(T singleton) where T : class
	{
		if (_singletons.Any(x => x is T))
			throw new Exception($"Singleton of type {typeof(T)} already exists.");
		
		_singletons.Add(singleton);
		return singleton;
	}
	
	public static T Add<T>(params object[] args) where T : class
	{
		if (_singletons.Any(x => x is T))
			throw new Exception($"Singleton of type {typeof(T)} already exists.");
		
		var singleton = (T?)Activator.CreateInstance(typeof(T), args);
		if (singleton == null)
			throw new Exception($"Failed to create singleton of type {typeof(T)}.");
		
		_singletons.Add(singleton);
		return singleton;
	}
	
	public static T Get<T>() where T : class
	{
		var singleton = _singletons.OfType<T>().FirstOrDefault();
		if (singleton == null)
			throw new Exception($"Singleton of type {typeof(T)} not found.");

		return singleton;
	}
}