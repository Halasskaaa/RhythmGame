namespace wah.Util;

internal static class FileInfoUtil
{
	public static FileInfo AppendPath(this FileInfo info, ReadOnlySpan<char> path)
	{
		return new FileInfo(Path.Combine(info.FullName, path.ToString()));
	}
}