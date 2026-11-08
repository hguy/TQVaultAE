using System.Media;
using TQVaultAE.Domain.Entities;

namespace ArzExplorer.Models;

internal sealed class NodeTag
{
	internal RecordId? Thread;
	internal string Text = string.Empty;
	internal string TextU => Text.ToUpperInvariant();

	internal int TokIdx;
	internal RecordId Key = RecordId.Empty;
	internal string DictionaryKey = string.Empty;
	internal required TreeNode thisNode; // The node that holds the tag.
	internal required TQFileInfo File;
	internal byte[]? RawData;
	internal List<string> RecordText = [];
	internal Bitmap? Bitmap;
	internal SoundPlayer? SoundPlayer;
}