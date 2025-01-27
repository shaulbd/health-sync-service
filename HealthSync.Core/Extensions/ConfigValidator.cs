using System.Text.RegularExpressions;

namespace HealthSync.Core.Extensions;

public static class ConfigValidator
{
	/// <summary>
	/// Validates if the given plugin ID is valid.
	/// A valid plugin ID does not contain spaces or special characters,
	/// but can include letters, numbers, hyphens, and periods.
	/// </summary>
	/// <param name="pluginId">The plugin ID to validate.</param>
	/// <returns>True if the plugin ID is valid; otherwise, false.</returns>
	public static bool IsValidPluginId(this string pluginId)
	{
		if (string.IsNullOrWhiteSpace(pluginId))
		{
			return false;
		}

		// Regular expression to match valid plugin IDs
		// Allows letters, numbers, hyphens, and periods
		var regex = new Regex(@"^[a-zA-Z0-9\-.]+$", RegexOptions.Compiled);

		return regex.IsMatch(pluginId);
	}
}
