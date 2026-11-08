using AwesomeAssertions;
using TQVaultAE.Application;

namespace TQVaultAE.Tests.Application;

public class CommandLineArgsTests
{
	private const string Exe = "TQVaultAE.exe";

	[Fact]
	public void NoArguments_IsNotAutomatic()
	{
		var args = new CommandLineArgs([Exe]);

		args.IsAutomatic.Should().BeFalse();
		args.HasMapName.Should().BeFalse();
		args.Error.Should().BeNull();
	}

	[Fact]
	public void PlayerOnly_IsAutomaticWithoutMap()
	{
		var args = new CommandLineArgs([Exe, "Hero"]);

		args.Player.Should().Be("Hero");
		args.MapName.Should().BeNull();
		args.IsAutomatic.Should().BeTrue();
	}

	[Fact]
	public void ModOnly_TakesTheMapNameWhateverTheCase()
	{
		var args = new CommandLineArgs([Exe, " /Mod:Atlantis "]);

		args.MapName.Should().Be("Atlantis");
		args.Player.Should().BeNull();
		args.HasMapName.Should().BeTrue();
		args.IsAutomatic.Should().BeTrue();
	}

	[Theory]
	[InlineData("Hero", "/mod:Atlantis")]
	[InlineData("/mod:Atlantis", "Hero")]
	public void PlayerAndMod_InAnyOrder(string first, string second)
	{
		var args = new CommandLineArgs([Exe, first, second]);

		args.Player.Should().Be("Hero");
		args.MapName.Should().Be("Atlantis");
		args.Error.Should().BeNull();
	}

	[Fact]
	public void TwoArgumentsWithoutMod_IsAnError()
	{
		var args = new CommandLineArgs([Exe, "Hero", "Villain"]);

		args.Error.Should().Contain("Unknown arguments");
		var act = args.ThrowIfInvalid;
		act.Should().Throw<ArgumentException>().WithMessage("*Unknown arguments*");
	}

	[Fact]
	public void TooManyArguments_IsAnError()
	{
		var args = new CommandLineArgs([Exe, "a", "b", "c"]);

		args.Error.Should().Contain("Too many arguments");
	}

	[Fact]
	public void ValidArguments_DoNotThrow()
	{
		var act = new CommandLineArgs([Exe, "Hero"]).ThrowIfInvalid;

		act.Should().NotThrow();
	}
}
