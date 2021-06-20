namespace ppimgdiff
{
    internal class PathPair
    {
        public string Path1 { get; protected set; }
        public string Path2 { get; protected set; }

        public PathPair(string path1, string path2)
        {
            Path1 = path1;
            Path2 = path2;
        }
    }
}
