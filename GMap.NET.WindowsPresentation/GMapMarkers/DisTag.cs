using System.Windows;

namespace GMap.NET.WindowsPresentation
{
    public class DisTag : GMapMarker
    {
        DisText DisText1;
        public DisTag(PointLatLng dispoint, double distance, GMapControl map)
        {
            Map = map;
            Position = dispoint;
            ZIndex = (int)GMapMarkers_ZIndex.DisTag;
            Offset = new Point(15, -30);
            ID = (int)GMapMarkers_ID.DisTag;
            DisText1 = new DisText();
            Shape = DisText1;
            DistanceValue = distance.ToString("0.000") + " KM";
        }

        public void Add()
        {
            Map.Markers.Add(this);
        }

        public void Remove()
        {
            Map.RemoveMarkerAtZindex(Map, (int)GMapMarkers_ZIndex.DisTag);
        }

        public void Update(PointLatLng dispoint, double distance)
        {
            Position = dispoint;
            DistanceValue = distance.ToString("0.000") + " KM";
        }
    }
}
