namespace wah.Util
{
	internal static class DirectoryInfoUtil
	{
		public static FileInfo AppendPath(this DirectoryInfo info, ReadOnlySpan<char> path)
		{
			return new FileInfo(Path.Combine(info.FullName, path.ToString()));
		}
	}
}
