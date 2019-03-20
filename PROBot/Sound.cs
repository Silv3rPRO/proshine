namespace PROBot
{
    public class Sound
    {
        public string Name { get; private set; }
        public bool ShouldPlay { get; set; }
        public string File { get; private set; }

        public Sound(string _name, bool _shouldPlay, string _file)
        {
            Name = _name;
            ShouldPlay = _shouldPlay;
            File = _file;
        }
    }
}