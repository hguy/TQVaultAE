using TQVaultAE.Domain.Contracts.Services;

namespace TQVaultAE.Services;

public class PathIO : IPathIO
{
	public virtual string Combine(string path1, string path2)
	{
		return System.IO.Path.Combine(path1, path2);
	}

	public virtual string Combine(string path1, string path2, string path3)
	{
		return System.IO.Path.Combine(path1, path2, path3);
	}

	public virtual string Combine(params string[] paths)
	{
		return System.IO.Path.Combine(paths);
	}

	public virtual string GetDirectoryName(string path)
	{
		return System.IO.Path.GetDirectoryName(path);
	}

	public virtual string GetFileName(string path)
	{
		return System.IO.Path.GetFileName(path);
	}

	public virtual string GetFileNameWithoutExtension(string path)
	{
		return System.IO.Path.GetFileNameWithoutExtension(path);
	}

	public virtual string GetExtension(string path)
	{
		return System.IO.Path.GetExtension(path);
	}

	public virtual string ChangeExtension(string path, string extension)
	{
		return System.IO.Path.ChangeExtension(path, extension);
	}

	public virtual char[] GetInvalidPathChars()
	{
		return System.IO.Path.GetInvalidPathChars();
	}
	public virtual char[] GetInvalidFileNameChars()
	{
		return System.IO.Path.GetInvalidFileNameChars();
	}
		
}