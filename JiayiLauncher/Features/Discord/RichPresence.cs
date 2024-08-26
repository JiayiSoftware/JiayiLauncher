using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;
using DiscordRPC;
using JiayiLauncher.Features.Game;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Features.Profiles;
using JiayiLauncher.Features.Shaders;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;
using Button = DiscordRPC.Button;
using Timer = System.Timers.Timer;

namespace JiayiLauncher.Features.Discord;

public class RichPresence
{
	private DiscordRpcClient? _client;
	private DateTime? _startTime;
	
	// a timer so presence can be updated every so often
	private readonly Timer _timer = new(5000);
	
	private readonly Minecraft _minecraft = Singletons.Get<Minecraft>();
	private readonly PackageData _packageData = Singletons.Get<PackageData>();
	private readonly ShaderManager _shaderManager = Singletons.Get<ShaderManager>();
	
	public RichPresence()
	{
		if (!JiayiSettings.Instance!.RichPresence) return;
		
		var log = Singletons.Get<Log>();
		
		_startTime = DateTime.UtcNow;
		
		_client = new DiscordRpcClient("1138925544172441670");
		
		_client.SkipIdenticalPresence = true;
		_client.OnError += (_, e) => log.Write("Discord", e.Message, Log.LogLevel.Error);
		_client.OnReady += (_, e) => log.Write("Discord",
			JiayiSettings.Instance.AnonymizeLogs ? "Connected to Discord" : $"Connected to {e.User}");
		_client.OnPresenceUpdate += (_, e) 
			=> log.Write("Discord", $"Presence updated: {e.Presence.Details} - {e.Presence.State}");

		_client.Initialize();
		
		_timer.Elapsed += (_, _) => Update();
		_timer.Start();
	}
	
	private string FormatString(string f)
	{
		switch (true)
		{
			case true when f.Contains("%mod_name%"):
				switch (_minecraft.ModsLoaded.Count)
				{
					case 0:
						return f.Replace("%mod_name%", "no mods");
					case 1:
						return f.Replace("%mod_name%", _minecraft.ModsLoaded[0].Name);
					case > 1:
						return f.Replace("%mod_name%", $"{_minecraft.ModsLoaded.Count} mods");
				}
				break;
			
			case true when f.Contains("%game_version%"):
				return f.Replace("%game_version%", _packageData.GetVersion().Result);
			
			case true when f.Contains("%mod_count%"):
				if (ModCollection.Current is null) return f.Replace("%mod_count%", "no mods");
				var modsPlural = ModCollection.Current.Mods.Count == 1 ? "mod" : "mods";
				return f.Replace("%mod_count%", $"{ModCollection.Current.Mods.Count.ToString()} {modsPlural}");
			
			case true when f.Contains("%shader_name%"):
				var shaderName = _shaderManager.AppliedShader == string.Empty
					? "no shaders"
					: _shaderManager.AppliedShader;
				return f.Replace("%shader_name%", shaderName);
			
			case true when f.Contains("%profile_count%"):
				if (ProfileCollection.Current is null) return f.Replace("%profile_count%", "no profiles");
				var profilesPlural = ProfileCollection.Current.Profiles.Count == 1 ? "profile" : "profiles";
				return f.Replace("%profile_count%",
					$"{ProfileCollection.Current.Profiles.Count.ToString()} {profilesPlural}");
			
			case true when f.Contains("%launcher_version%"):
				var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);
				return f.Replace("%launcher_version%", $"v{version.Major}.{version.Minor}.{version.Build}");
		}
		
		return f;
	}

	private void Update()
	{
		if (!JiayiSettings.Instance.RichPresence)
		{
			if (_client is { IsInitialized: true })
			{
				_client.ClearPresence();
				_client.Dispose();
				_client = null;
			}

			return;
		}
		
		_client ??= new DiscordRpcClient("1138925544172441670");
		
		if (!_client.IsInitialized) _client.Initialize();

		var buttons = new List<Button>();
		
		if (JiayiSettings.Instance.DiscordShowDownloadButton)
		{
			buttons.Add(new Button { Label = "Download Jiayi", Url = "https://jiayisoftware.github.io/launcher" });
		}

		if (JiayiSettings.Instance.DiscordShareCurrentMod)
		{
			if (_minecraft.ModsLoaded.Count != 1) return;
			
			var mod = _minecraft.ModsLoaded.FirstOrDefault(x => x.FromInternet);
			if (mod == null) return;
			
			buttons.Add(new Button { Label = "Add this mod", Url = $"jiayi://addmod/{mod.Path}" });
		}

		_client.SetPresence(new DiscordRPC.RichPresence
		{
			Details = FormatString(JiayiSettings.Instance.DiscordDetails),
			State = FormatString(JiayiSettings.Instance.DiscordState),
			
			Assets = new Assets
			{
				LargeImageKey = JiayiSettings.Instance.DiscordLargeImageKey,
				LargeImageText = FormatString(JiayiSettings.Instance.DiscordLargeImageText),
				
				SmallImageKey = JiayiSettings.Instance.DiscordSmallImageKey,
				SmallImageText = FormatString(JiayiSettings.Instance.DiscordSmallImageText)
			},
			
			Timestamps = new Timestamps
			{
				Start = JiayiSettings.Instance.DiscordShowElapsedTime ? _startTime : null
			},
			
			Buttons = buttons.ToArray()
		});		
	}
}