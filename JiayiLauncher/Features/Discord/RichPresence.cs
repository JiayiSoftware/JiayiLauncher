using System;
using System.Timers;
using DiscordRPC;
using JiayiLauncher.Features.Bridge;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Settings;

namespace JiayiLauncher.Features.Discord;

public static class RichPresence
{
	private static DiscordRpcClient? _client;
	private static DateTime? _startTime;
	
	// a timer so presence can be updated every so often
	private static readonly Timer _timer = new(5000);
	
	private static string FormatString(string f)
	{
		switch (true)
		{
			case true when f.Contains("%mod_name%"):
				switch (Minecraft.ModsLoaded.Count)
				{
					case 0:
						return f.Replace("%mod_name%", "no mods");
					case 1:
						return f.Replace("%mod_name%", Minecraft.ModsLoaded[0].Name);
					case > 1:
						return f.Replace("%mod_name%", $"{Minecraft.ModsLoaded.Count} mods");
				}
				break;
			case true when f.Contains("%game_version%"):
				return f.Replace("%game_version%", Minecraft.GetVersion().GetAwaiter().GetResult());
			case true when f.Contains("%mod_count%"):
				if (ModCollection.Current is null) return f.Replace("%mod_count%", "no mods");
				var plural = ModCollection.Current.Mods.Count == 1 ? "mod" : "mods";
				return f.Replace("%mod_count%", $"{ModCollection.Current.Mods.Count.ToString()} {plural}");
		}
		
		return f;
	}
	
	public static void Initialize()
	{
		if (!JiayiSettings.Instance!.RichPresence) return;
		
		_startTime = DateTime.UtcNow;
		
		_client = new DiscordRpcClient(
			JiayiSettings.Instance.DiscordAppId == string.Empty
			? "858033874264260658"
			: JiayiSettings.Instance.DiscordAppId);
		_client.Initialize();
		_client.SkipIdenticalPresence = true;
		
		Update();
		
		_timer.Elapsed += (_, _) => Update();
		_timer.Start();
	}

	public static void Update()
	{
		if (!JiayiSettings.Instance!.RichPresence)
		{
			if (_client is { IsInitialized: true })
			{
				_client.ClearPresence();
				_client.Dispose();
				_client = null;
			}

			return;
		}
		
		_client ??= new DiscordRpcClient(
			JiayiSettings.Instance.DiscordAppId == string.Empty
			? "858033874264260658"
			: JiayiSettings.Instance.DiscordAppId);
		
		if (!_client.IsInitialized) _client.Initialize();

		_client.SetPresence(new DiscordRPC.RichPresence
		{
			Details = FormatString(JiayiSettings.Instance.DiscordDetails),
			State = FormatString(JiayiSettings.Instance.DiscordState),
			
			Assets = new Assets
			{
				LargeImageKey = JiayiSettings.Instance.DiscordLargeImageKey == string.Empty ? "logonewdiscord" : JiayiSettings.Instance.DiscordLargeImageKey,
				LargeImageText = JiayiSettings.Instance.DiscordLargeImageText == string.Empty ? "Jiayi Launcher" : FormatString(JiayiSettings.Instance.DiscordLargeImageText),
				
				SmallImageKey = JiayiSettings.Instance.DiscordSmallImageKey == string.Empty ? "minecraft" : JiayiSettings.Instance.DiscordSmallImageKey,
				SmallImageText = JiayiSettings.Instance.DiscordSmallImageText == string.Empty ? "Minecraft for Windows" : FormatString(JiayiSettings.Instance.DiscordSmallImageText)
			},
			
			Timestamps = new Timestamps
			{
				Start = JiayiSettings.Instance.DiscordShowElapsedTime ? _startTime : null
			}
		});		
	}
}