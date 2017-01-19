namespace PROBot.Modules
{
    public class OptionSlider
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }

        public OptionSlider(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
