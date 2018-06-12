using System.Windows;

namespace GMap.NET.WindowsPresentation
{
    public class TagMakrer : GMapMarker
    {
        readonly Callout MyCallout;
        public TagMakrer(PointLatLng pos, string tagcontent)
        {
            ZIndex = (int)Markers_ZIndex.TagMarker;
            ID = (int)Markers_ID.TagMarker;
            Position = pos;
            MyCallout = new Callout(tagcontent);
            Shape = MyCallout;
            Offset = new Point(0, -115);
            Map.Markers.Add(this);
        }
    }
}
