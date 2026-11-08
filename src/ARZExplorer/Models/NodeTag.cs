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
	internal TreeNode thisNode = null!; // Set with the node that holds the tag.
	internal TQFileInfo File = null!; // Set with the node that holds the tag.
	internal byte[]? RawData;
	internal List<string> RecordText = [];
	internal Bitmap? Bitmap;
	internal SoundPlayer? SoundPlayer;
}