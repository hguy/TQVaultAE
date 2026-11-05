<#
.SYNOPSIS
	Regenerates RelicAndCharm.cs (relic and charm enum) from the game database.

.DESCRIPTION
	Reads every ItemRelic and ItemCharm record with tqdb (src/TQVaultAE.DbCli) and writes one enum member per record:
	  - name:       the record file name split on '_' and reversed (01_ACT1_AEGISOFATHENA -> AEGISOFATHENA_ACT1_01),
	                prefixed with HCDUNGEON_ for records\item\hcdungeon;
	  - summary:    the English display name; remarks: the item text, wrapped at 40 characters;
	  - [Description]: the record path, upper case;
	  - [GearType]: the gear the relic fits, from the record's slot flags (helmet, bodyArmor, armband, greaves, ring,
	                amulet, spear, staff, bow, sword, mace, axe, shield). "bow" also covers thrown weapons: relics have no
	                thrown flag, yet relics made for thrown weapons (Hou Yi's Determination) only set bow, spear and sword.
	Members are sorted by name, with Eternal Embers (XPack4) and Hardcore dungeon records in their own regions.

	Run it after a game update that adds or changes relics or charms, then build, run the tests and commit the result.

.PARAMETER Game
	Game install folder. Default: tqdb's detection (TQ_GAME_PATH, Steam, GOG).

.PARAMETER Tqdb
	Path to tqdb.exe. Default: the newest build under src/TQVaultAE.DbCli/bin, else 'dotnet run' of the project.

.PARAMETER Output
	File to write. Default: RelicAndCharm.cs next to this script.

.EXAMPLE
	pwsh src/TQVaultAE.Domain/Entities/RegenRelicEnum.ps1
#>
[CmdletBinding()]
param(
	[string] $Game,
	[string] $Tqdb,
	[string] $Output = (Join-Path $PSScriptRoot 'RelicAndCharm.cs')
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
$dbCliProject = Join-Path $repoRoot 'src\TQVaultAE.DbCli\TQVaultAE.DbCli.csproj'

# Slot flag of the relic record -> GearType member, in GearType declaration order.
$slotFlags = [ordered]@{
	helmet    = 'Head'
	bodyArmor = 'Torso'
	armband   = 'Arm'
	greaves   = 'Leg'
	ring      = 'Ring'
	amulet    = 'Amulet'
	spear     = 'Spear'
	staff     = 'Staff'
	bow       = 'Bow'
	sword     = 'Sword'
	mace      = 'Mace'
	axe       = 'Axe'
	shield    = 'Shield'
}
$gearOrder = @('Head', 'Torso', 'Arm', 'Leg', 'Ring', 'Amulet', 'Spear', 'Staff', 'Thrown', 'Bow', 'Sword', 'Mace', 'Axe', 'Shield')
$composites = [ordered]@{
	AllArmor   = @('Head', 'Torso', 'Arm', 'Leg')
	Jewellery  = @('Ring', 'Amulet')
	AllWeapons = @('Spear', 'Staff', 'Thrown', 'Bow', 'Sword', 'Mace', 'Axe')
}
$wrapWidth = 40

function Invoke-Tqdb([string[]] $arguments) {
	if ($Game) { $arguments += @('--game', $Game) }
	if ($Tqdb) {
		& $Tqdb @arguments
	}
	else {
		$built = Get-ChildItem (Join-Path $repoRoot 'src\TQVaultAE.DbCli\bin') -Recurse -Filter 'tqdb.exe' -ErrorAction SilentlyContinue |
			Sort-Object LastWriteTime -Descending | Select-Object -First 1
		if ($built) { & $built.FullName @arguments }
		else { & dotnet run --project $dbCliProject -c Release -- @arguments }
	}
	if ($LASTEXITCODE -ne 0) { throw "tqdb failed with exit code $LASTEXITCODE" }
}

function Get-GearType($fields) {
	$on = [System.Collections.Generic.HashSet[string]]::new()
	foreach ($flag in $slotFlags.Keys) {
		if ($fields.PSObject.Properties[$flag] -and $fields.$flag -eq $true) { [void]$on.Add($slotFlags[$flag]) }
	}
	if ($on.Contains('Bow')) { [void]$on.Add('Thrown') }

	$parts = @()
	foreach ($name in $composites.Keys) {
		$members = $composites[$name]
		if (@($members | Where-Object { -not $on.Contains($_) }).Count -eq 0) {
			$parts += $name
			foreach ($m in $members) { [void]$on.Remove($m) }
		}
	}
	$parts += $gearOrder | Where-Object { $on.Contains($_) }
	if ($parts.Count -eq 0) { return 'GearType.Undefined' }
	return ($parts | ForEach-Object { "GearType.$_" }) -join ' | '
}

function Get-Remarks([string] $text) {
	$lines = @()
	if (-not $text) { return $lines }
	foreach ($paragraph in ($text -split '\{\^n\}')) {
		$clean = (($paragraph -replace '\{\^\w\}', '') -replace '\s+', ' ').Trim()
		if (-not $clean) { continue }
		$current = ''
		foreach ($word in $clean.Split(' ')) {
			if ($current -and ($current.Length + 1 + $word.Length) -gt $wrapWidth) {
				$lines += $current
				$current = $word
			}
			elseif ($current) { $current = "$current $word" }
			else { $current = $word }
		}
		if ($current) { $lines += $current }
	}
	return $lines
}

function Format-Xml([string] $text) {
	return [System.Security.SecurityElement]::Escape($text)
}

# Read the records
$fieldList = (@('id', 'displayName', 'itemText') + @($slotFlags.Keys)) -join ','
$json = Invoke-Tqdb @('records', '--quiet', '--class', 'ItemRelic', '--class', 'ItemCharm', '--fields', $fieldList, '--resolve', '--lang', 'EN', '--format', 'json', '--compact')
$records = ($json -join "`n") | ConvertFrom-Json

$members = foreach ($r in $records) {
	$path = $r.id.ToUpperInvariant()
	$stem = [System.IO.Path]::GetFileNameWithoutExtension($path)
	$parts = $stem.Split('_')
	[array]::Reverse($parts)
	$name = $parts -join '_'
	$group = 'Main'
	if ($path -match '\\HCDUNGEON\\') { $name = "HCDUNGEON_$name"; $group = 'HCDUNGEON' }
	elseif ($path -match '\\XPACK4\\') { $group = 'Eternal Embers' }

	$itemText = $null
	if ($r.PSObject.Properties['resolved'] -and $r.resolved.PSObject.Properties['itemText']) { $itemText = $r.resolved.itemText }

	[pscustomobject]@{
		Name    = $name
		Group   = $group
		Path    = $path
		Summary = $r.displayName
		Remarks = @(Get-Remarks $itemText)
		Gear    = Get-GearType $r.fields
	}
}

$duplicates = $members | Group-Object Name | Where-Object Count -gt 1
if ($duplicates) { throw "Duplicate member names: $(($duplicates | ForEach-Object Name) -join ', ')" }

# Write the enum
$sb = [System.Text.StringBuilder]::new()
function Line([string] $text = '') { [void]$sb.Append($text).Append("`r`n") }

Line '// This file is generated by RegenRelicEnum.ps1 from the game database. Do not edit it by hand:'
Line '// after a game update, run  pwsh src/TQVaultAE.Domain/Entities/RegenRelicEnum.ps1  then build, test and commit.'
Line 'using System.ComponentModel;'
Line ''
Line 'namespace TQVaultAE.Domain.Entities;'
Line ''
Line '/// <summary>'
Line '/// Relics and charms of the game, with their record id (<see cref="DescriptionAttribute"/>) and the gear they fit.'
Line '/// </summary>'
Line 'public enum RelicAndCharm'
Line '{'
Line "`t/// <summary>"
Line "`t/// Unknown"
Line "`t/// </summary>"
Line "`t[Description(@""NORECORDS"")]"
Line "`t[GearType(GearType.Undefined)]"
Line "`tUnknown,"

foreach ($group in @('Main', 'Eternal Embers', 'HCDUNGEON')) {
	$inGroup = $members | Where-Object Group -eq $group | Sort-Object { $_.Name } -Culture ([cultureinfo]::InvariantCulture)
	if (-not $inGroup) { continue }
	if ($group -ne 'Main') { Line ''; Line "`t#region $group" }
	foreach ($m in $inGroup) {
		Line ''
		Line "`t/// <summary>"
		Line "`t/// $(Format-Xml $m.Summary)"
		Line "`t/// </summary>"
		if ($m.Remarks.Count -gt 0) {
			Line "`t/// <remarks>"
			foreach ($remark in $m.Remarks) { Line "`t/// $(Format-Xml $remark)" }
			Line "`t/// </remarks>"
		}
		Line "`t[Description(@""$($m.Path)"")]"
		Line "`t[GearType($($m.Gear))]"
		Line "`t$($m.Name),"
	}
	if ($group -ne 'Main') { Line ''; Line "`t#endregion" }
}
Line '}'

[System.IO.File]::WriteAllText($Output, $sb.ToString(), [System.Text.UTF8Encoding]::new($false))
Write-Host "Wrote $(@($members).Count) relics and charms to $Output"
