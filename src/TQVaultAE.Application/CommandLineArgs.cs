//-----------------------------------------------------------------------
// <copyright file="CommandLineArgs.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Globalization;

namespace TQVaultAE.Application;

/// <summary>
/// Class for parsing command line arguments
/// </summary>
public class CommandLineArgs
{
	/// <summary>
	/// Initializes a new instance of the CommandLineArgs class.
	/// </summary>
	/// <param name="args">The command line as <see cref="Environment.GetCommandLineArgs"/> gives it: the program first.</param>
	public CommandLineArgs(string[] args)
	{
		// Modified by VillageIdiot
		// to accept mapname from the command line
		if (args.Length == 2)
		{
			if (IsModArgument(args[1]))
				this.MapName = args[1].Trim().Substring(5);
			else
				this.Player = args[1];
		}
		else if (args.Length == 3)
		{
			if (IsModArgument(args[2]))
			{
				this.Player = args[1];
				this.MapName = args[2].Trim().Substring(5);
			}
			else if (IsModArgument(args[1]))
			{
				// Check to see if the arguments are swapped.
				this.Player = args[2];
				this.MapName = args[1].Trim().Substring(5);
			}
			else
			{
				// Neither one started with /mod: so we fail.
				this.Error = UsageMessage("Unknown arguments");
			}
		}
		else if (args.Length > 3)
		{
			this.Error = UsageMessage("Too many arguments");
		}
	}

	/// <summary>
	/// Gets the name of the player specified on the command line.
	/// </summary>
	public string? Player { get; }

	/// <summary>
	/// Gets the name of the custom map specified on the command line.
	/// </summary>
	public string? MapName { get; }

	/// <summary>
	/// Gets the usage message when the arguments could not be parsed, else null.
	/// </summary>
	public string? Error { get; }

	/// <summary>
	/// Gets a value indicating whether to bypass the starts screen.
	/// </summary>
	public bool IsAutomatic => this.Player is not null || this.MapName is not null;

	/// <summary>
	/// Gets a value indicating whether a custom map has been specified.
	/// </summary>
	public bool HasMapName => this.MapName is not null;

	/// <summary>
	/// Throws the usage message if the command line arguments are incorrect.
	/// </summary>
	/// <exception cref="ArgumentException">The arguments could not be parsed.</exception>
	public void ThrowIfInvalid()
	{
		if (this.Error is not null)
			throw new ArgumentException(this.Error);
	}

	private static bool IsModArgument(string arg)
		=> arg.Trim().StartsWith("/MOD:", StringComparison.OrdinalIgnoreCase);

	private static string UsageMessage(string error)
		=> string.Format(CultureInfo.InvariantCulture, "Error parsing command-line arguments: {0}\n\nUsage: TQVault [playername] [/mod:<mod name>]", error);
}
