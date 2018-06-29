using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using static GMap.NET.WindowsPresentation.GMapMarker;

namespace GMap.NET.WindowsPresentation
{
    public class Tag_Line
    {
        GMapControl GMap;
        GMapRoute TagLine;

        List<PointLatLng> TagLinePoints = new List<PointLatLng>();
        public Tag_Line(GMapControl map)
        {
            GMap = map;
        }

        public void IsVisable(int id, bool isvisable)
        {
            TagLine.IsVisable((int)GMapMarkers_ID.Tag_Line + id, isvisable);
        }

        public void Drawing(int id)
        {
            var ssss = GMap.Markers.Where(marker => marker.ID == (int)GMapMarkers_ID.MovingTarget + id);
            var ssss1 = GMap.Markers.Where(marker => marker.ID == (int)GMapMarkers_ID.MT_Tag + id);

            TagLinePoints.Clear();
            if (ssss.Count() > 0 && ssss1.Count() > 0)
            {
                TagLinePoints.Add(ssss.ElementAt(0).Position);
                TagLinePoints.Add(ssss1.ElementAt(0).Position);
            }
            TagLine = new GMapRoute((int)GMapMarkers_ID.Tag_Line + id,
                (int)GMapMarkers_ZIndex.Tag_Line, TagLinePoints, Colors.Black, 1, GMap);
        }

        public void Remove(int id)
        {
            GMap.RemoveMarkerAtID(GMap, (int)GMapMarkers_ID.Tag_Line + id);
        }

        public void Update(int id, bool isvisable)
        {
            Remove(id);
            Drawing(id);
            IsVisable(id, isvisable);
        }
    }
}
