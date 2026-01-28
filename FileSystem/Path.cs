namespace wah.FileSystem
{
	internal readonly struct Path
	{
		readonly PathBuffer buff;
		readonly ushort buffLength;
		readonly bool exists;
		readonly bool isDirectory;

		public PathBuffer Value => buff;
		public bool Exists => exists;
		public bool IsDirectory => isDirectory;

		public Path(ReadOnlySpan<char> path)
		{
			path.CopyTo(buff);
			buffLength = (ushort)path.Length;
			var str = path.ToString();
			exists = System.IO.Path.Exists(str);
			isDirectory = exists && (File.GetAttributes(str) & FileAttributes.Directory) != 0;
		}

		public static Path operator / (in Path path,ReadOnlySpan<char> pathToAppend)
		{
			var buff = path.buff;
			Span<char> c = buff;
			pathToAppend.CopyTo(c[path.buffLength..]);

			return new(buff);
		}
	}
}
