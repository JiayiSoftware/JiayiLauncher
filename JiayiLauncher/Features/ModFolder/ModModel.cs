using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JiayiLauncher.Features.ModFolder;

public class ModModel
{
	[Required]
	[StringLength(20, ErrorMessage = "Name cannot be longer than 20 characters.")]
	public string? Name { get; set; }
	
	public string? Path { get; set; }
	public List<string> SupportedVersions { get; set; } = new() { "Any" };
}