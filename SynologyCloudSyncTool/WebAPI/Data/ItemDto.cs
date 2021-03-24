namespace WebAPI.Data
{
    public enum ItemType
    {
        Directory,
        File
    }

    public class ItemDto
    {
        public ItemType Type { get; set; }
        public string Path { get; set; }
    }
}