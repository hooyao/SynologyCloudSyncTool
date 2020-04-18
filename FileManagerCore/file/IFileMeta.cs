namespace FileManagerCore.file
{
    public struct FileVersion
    {
        public readonly int Major;
        public readonly int Minor;

        public FileVersion(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }
    }
    public interface IFileMeta
    {
        FileVersion MinVer { get; }
        FileVersion MaxVer { get; }
    }
}