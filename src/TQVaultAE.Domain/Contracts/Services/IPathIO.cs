using System.Collections.Generic;

namespace TQVaultAE.Domain.Contracts.Services;

public interface IPathIO
{
	string Combine(string path1, string path2);
	string Combine(string path1, string path2, string path3);
	string Combine(params string[] paths);
	string GetDirectoryName(string path);
	string GetFileName(string path);
	string GetFileNameWithoutExtension(string path);
	string GetExtension(string path);
	string ChangeExtension(string path, string extension);
	char[] GetInvalidPathChars();
	char[] GetInvalidFileNameChars();
}