//-----------------------------------------------------------------------
// <copyright file="Factories.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// Entry points of the Core layer. Signatures are part of the contract; the bodies live in partial files:
//   ArzDatabaseFactory.Open(string path)                          -> ArzDatabase.cs
//   ArcArchiveFactory.Open(string path)                           -> ArcArchive.cs
//   TextDatabaseFactory.Load(string textArchivePath, string lang) -> TextDatabase.cs
//   GameLocator.Locate(string? explicitRoot = null)               -> GameLocator.cs
//   TexConverter.FrameCount / ToDds / ToPng                       -> TexConverter.cs
//   DisplayNames.NameTag / Resolve                                -> DisplayNames.cs

namespace TQVaultAE.DbCli.Core;

/// <summary>Opens database.arz files.</summary>
public static partial class ArzDatabaseFactory;

/// <summary>Opens .arc archives.</summary>
public static partial class ArcArchiveFactory;

/// <summary>Loads Text_XX.arc.</summary>
public static partial class TextDatabaseFactory;
