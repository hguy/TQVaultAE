# PR #544 - Loss of Meaningful Comments Report

**Branch:** `optim`
**PR:** https://github.com/EtienneLamoureux/TQVaultAE/pull/544
**Date:** 2026-05-21

---

## Summary

This PR removes **~2600 non-trivial comment lines** across the codebase. While many are generated designer code or trivial XML doc, several categories represent meaningful loss of domain knowledge that aided understanding of data formats and game logic.

---

## CRITICAL LOSSES (Game File Format Documentation)

### 1. `src/TQVaultAE.Data/ArcFileProvider.cs` — ARC file format spec (ENTIRELY REMOVED)

The class previously contained a complete, commented specification of the Titan Quest ARC file binary format. This is reverse-engineered knowledge not documented elsewhere.

**Removed ARC file header documentation:**
```
// Format of an ARC file
// 0x08 - 4 bytes = # of files
// 0x0C - 4 bytes = # of parts
// 0x18 - 4 bytes = offset to directory structure
```

**Removed directory structure documentation:**
```
// Format of directory structure
// 4-byte int = offset in file where this part begins
// 4-byte int = size of compressed part
// 4-byte int = size of uncompressed part
// these triplets repeat for each part in the arc file
// After these triplets are a bunch of null-terminated strings
// which are the sub filenames.
```

**Removed subfile data documentation:**
```
// After the subfilenames comes the subfile data:
// 4-byte int = 3 == indicates start of subfile item  (maybe compressed flag??)
//          1 == maybe uncompressed flag??
// 4-byte int = offset in file where first part of this subfile begins
// 4-byte int = compressed size of this file
// 4-byte int = uncompressed size of this file
// 4-byte int = crap
// 4-byte int = crap
// 4-byte int = crap
// 4-byte int = numParts this file uses
// 4-byte int = part# of first part for this file (starting at 0).
// 4-byte int = length of filename string
// 4-byte int = offset in directory structure for filename
```

**Removed decompression logic comments:**
```
// Ignore the zlib compression method.
// Ignore the zlib compression flags.
// Create a deflate stream.
// storageType = 3 - compressed / 1- non compressed
```

### 2. `src/TQVaultAE.Data/ArzFileProvider.cs` — ARZ file header format (ENTIRELY REMOVED)

**Removed ARZ header documentation:**
```
// ARZ header file format
// 0x000000 int32
// 0x000004 int32 start of dbRecord table
// 0x000008 int32 size in bytes of dbRecord table
// 0x00000c int32 numEntries in dbRecord table
// 0x000010 int32 start of string table
// 0x000014 int32 size in bytes of string table
// 4 final int32's from file
// first int32 is numstrings in the stringtable
// second int32 is something ;)
// 3rd and 4th are crap (timestamps maybe?)
```

### 3. `src/TQVaultAE.Data/RecordInfoProvider.cs` — Record entry format (ENTIRELY REMOVED)

**Removed record entry documentation:**
```
// Record Entry Format
// 0x0000 int32 stringEntryID (dbr filename)
// 0x0004 int32 string length
// 0x0008 string (record type)
// 0x00?? int32 offset
// 0x00?? int32 length in bytes
// 0x00?? int32 timestamp?
// 0x00?? int32 timestamp?
// Compressed size
```

**Removed compression handling:**
```
// Ignore the zlib compression method.
// Ignore the zlib compression flags.
// Create a deflate stream.
// Create a memorystream to hold the decompressed data
```

Critical detail: `// 24 is the offset of where all record data begins` was also removed from `ArzFileProvider.cs`.

---

## HIGH LOSSES (Game Logic & Business Rules)

### 4. `src/TQVaultAE.Data/ItemProvider.cs` — 245 comment removals

This file lost many comments explaining item decoding and display logic:

**Loot/affix logic:**
```
// Always get Monster infrequent affixes          — on LootTableGearType check
// Always get loot table for uniques               — on IsTablesUnique check
```

**Artifact formula logic:**
```
// for artifacts we need to find the formulae that was used to create the artifact. sucks to be us
// The formulas seem to always be in the arcaneformulae subfolder with a _formula on the end
// Damn it, IL did not keep the filename consistent on Kingslayer (Sands of Kronos)
```

**Attribute grouping/sorting:**
```
// Now for the global params, we need to check to see if they are XOR or all.
// We do it by checking the effect just after the global param.
// Yes it is global and is also XOR
// flag our current attribute as XOR
// We do it by adding a second NULL entry to the list. Its a hack but it works
// it is a spurious globalChance entry. Let's add 2 null entries to signal it should be ignored
// To keep track of groups so they are not counted twice
// Filter Attribute chance and duration tags
// Filter base attributes
// Chance of effects are still messed up.
```

**Number formatting fixes:**
```
// Fix#246, double signed result on negative value Ex : string.Format("{0:+#0} d'intelligence", -10) by removing format sign.
// Fix "Dotted decimal mask" matching Ex : {0:#0.0} Health Regeneration per second
```

**Skill decoding:**
```
// Check to see if it's a Buff Skill
// Check to see if it really is a skill
// Check to see if item creates a pet
// There are upto 17 skills                          — on array size [17]
// Find the skill in the skill tree so that we can get the level
```

**Skill type conversion (trigger types):**
```
// Convert TriggerType into text tag
// Activated on low health
// Activated on low energy
// Activated upon taking damage
// Activated upon taking melee damage
// Activated upon taking ranged damage
// Activated upon casting a buff
// Activated on attack
// Activated when equipped
```

**Requirement comparison bugfix:**
```
// Comparison was failing when level difference was too high
// (single digit vs multi-digit)
```

---

## MODERATE LOSSES (UI Logic & Architecture)

### 5. `src/TQVaultAE.GUI/Components/SackPanel.cs` — 826 comment removals

Lost comments explaining:
- Cell offset calculation for drag/drop: *"Identify all the cells and items under the drag item... drop into so we add 1/2 a cell to the drag location so that we pick the cells closest to the center of the item"*
- Bag offset logic: *"For the player panel we do not need an offset since the bags are already offset by 1"* and *"Internally to the sack panel they are still zero based"*
- Item move logic flow: copy, seed change check, modification marking
- Context menu operations (set item creation, relic extraction, bonus changes, suffix/prefix changes)

### 6. `src/TQVaultAE.GUI/MainForm.Panel.cs` — Item transfer routing logic

Lost the commented decision tree for item routing:
```
// This is a sack to sack move on the same panel.
// Special Case for moving to stash.
// Check if we are moving to the player's stash
// Equipment Panel is active so switch to the transfer stash.
// Check the transfer stash
// Check the relic vault stash
// We have nowhere to send the item so cancel the move.
// See if we have an open space to put the item.
```

### 7. `src/TQVaultAE.GUI/MainForm.Player.cs` — Player loading logic

Lost comments explaining Titan Quest vs Immortal Throne branching:
```
// Titan Quest original
// Only if it's IT, TQ doesn't have one
// Throw a message if the stash is not present.
```

Also the file watcher threading note: `// Called on FileSystemWatcher thread`

### 8. `src/TQVaultAE.Domain/Entities/SessionContext.cs` — 57 comment removals

Complete loss of XML documentation on all properties and methods.

### 9. `src/TQVaultAE.Services/VaultService.cs` — Caching logic

Lost the cache flow explanation:
```
// Check the cache
// We need to load vault.
// the file does not exist so create a new vault or convert old format to Json.
```

---

## MINOR LOSSES

- **`src/TQVaultAE.GUI/SearchDialogAdvanced.cs`** (101 removals) — Removed helpful comments about the search pipeline flow (AND/OR operator, filter application order, category display logic, threading with Invoke)
- **`src/TQVaultAE.Data/PlayerCollectionProvider.cs`** — Removed binary section logic comments (`// make binary section`, `// put pieces back together`, `// new item segment`, `// new equipment segment`)
- **`src/TQVaultAE.Data/TQDataService.cs`** — Removed key/value replacement offset comments (`// start content`, `// KeyLen`, `// ValueLen`, `// Value`, `// end content`)
- **`src/TQVaultAE.Services/GameFileService.cs`** — Removed git sync process documentation (standard error redirect, progress capture, junction creation for git repo)

---

## Files with Most Comment Removals

| File | Removals | Criticality |
|------|----------|-------------|
| `SackPanel.cs` | 826 | Low (UI mostly) |
| `VaultPanel.cs` | 265 | Low (UI mostly) |
| `ItemProvider.cs` | 245 | **HIGH** (game logic) |
| `SearchDialogAdvanced.Designer.cs` | 126 | Low (auto-generated) |
| `SearchDialogAdvanced.cs` | 101 | Medium |
| `MainForm.cs` | 81 | Medium |
| `ArcFileProvider.cs` | 70 | **CRITICAL** (file format) |
| `MainForm.Panel.cs` | 73 | Medium |
| `Services/GameFileService.cs` | 62 | Low |
| `ForgePanel.cs` | 59 | Low |
| `SessionContext.cs` | 57 | Low (XML doc) |
| `ARZExplorer/MainForm.cs` | 56 | Medium |
| `Program.cs` | 55 | Low |
| `RecordInfoProvider.cs` | 28 | **HIGH** (file format) |
| `ArzFileProvider.cs` | 25 | **CRITICAL** (file format) |
| `PlayerServiceTests.cs` | 36 | Low (tests) |

---

## Recommendations

1. **RESTORE** the ARC/ARZ/RecordInfo format documentation comments (`ArcFileProvider.cs`, `ArzFileProvider.cs`, `RecordInfoProvider.cs`). These document reverse-engineered binary file formats that have no external specification. Without them, anyone working on file I/O must re-discover the format from scratch.

2. **RESTORE or preserve in documentation** the critical game logic comments from `ItemProvider.cs`, especially:
   - XOR spurious global chance hack
   - Skill tree (17 skills) array sizing
   - Number formatting fixes (issue #246, dotted decimal mask)
   - Requirement comparison digit-length fix
   - Monster Infrequent / Unique loot table logic
   - Artifact formula filename inconsistencies

3. Consider moving format specs to a `docs/` file rather than removing them entirely.

4. The remaining removals (UI comment headers, designer files, trivial XML doc) are acceptable losses.
