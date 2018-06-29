using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using static GMap.NET.WindowsPresentation.GMapMarker;

namespace GMap.NET.WindowsPresentation
{
    public class MT_Track
    {
        GMapControl GMap;
        GMapRoute MTTrack;

        public MT_Track(GMapControl map)
        {
            GMap = map;
        }

        public void IsVisable(int id, bool isvisable)
        {
            if (MTTrack != null) MTTrack.IsVisable((int)GMapMarkers_ID.MT_Track + id, isvisable);
        }

        public void Drawing(int id, IEnumerable<PointLatLng> pos)
        {
            //MTTrackPoints.Add(pos);
            MTTrack = new GMapRoute((int)GMapMarkers_ID.MT_Track + id,
                (int)GMapMarkers_ZIndex.MT_Track, pos, Colors.Lime, 4, GMap);
        }

        public void RemoveTrack(int id)
        {
            GMap.RemoveMarkerAtID(GMap, (int)GMapMarkers_ID.MT_Track + id);
            var clear = GMap.Markers.Where(marker => marker.ID == (int)GMapMarkers_ID.MovingTarget + id);
            if (clear.Count() > 0)
            {
                clear.ElementAt(0).MTTrackPoints.Clear();
            }
        }

        public void RemoveAllTracks()
        {
            var clear = GMap.Markers.Where(marker => marker.ZIndex == (int)GMapMarkers_ZIndex.MovingTarget);
            if (clear.Count() > 0)
            {
                for (int i = 0; i < clear.Count(); i++)
                {
                    clear.ElementAt(i).MTTrackPoints.Clear();
                }
            }
            GMap.RemoveMarkerAtZindex(GMap, (int)GMapMarkers_ZIndex.MT_Track);
        }

        public void Remove(int id)
        {
            GMap.RemoveMarkerAtID(GMap, (int)GMapMarkers_ID.MT_Track + id);
        }

        public void Update(int id, IEnumerable<PointLatLng> pos, bool isvisable)
        {
            Remove(id);
            Drawing(id, pos);
            IsVisable(id, isvisable);
        }
    }
}
