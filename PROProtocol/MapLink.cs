namespace PROProtocol
{
    public class MapLink
    {
        public string DestinationMap;
        public int DestinationX;
        public int DestinationY;

        public MapLink(string map, int x, int y)
        {
            DestinationMap = map;
            DestinationX = x;
            DestinationY = y;
        }
    }
}