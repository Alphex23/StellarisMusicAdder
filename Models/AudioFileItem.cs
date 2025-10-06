namespace StellarisMusicAdder.Models
{
    public class AudioFileItem
    {
        public string FilePath { get; set; } = "";
        public string FileName => System.IO.Path.GetFileName(FilePath);
    }
}
