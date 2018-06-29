using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using static GMap.NET.WindowsPresentation.GMapMarker;

namespace GMap.NET.WindowsPresentation
{
    public class DisLine
    {
        GMapControl GMap;
        GMapRoute Dis_Line;
        DisTag disTag;

        public DisLine(GMapControl map)
        {
            GMap = map;
            disTag = new DisTag(new PointLatLng(0,0),0,map);
        }

        public void Drawing(List<PointLatLng> pos)
        {
            if (pos.Count == 2)
            {
                Dis_Line = new GMapRoute((int)GMapMarkers_ID.DisLine,
                (int)GMapMarkers_ZIndex.DisLine, pos, Colors.Yellow, 1, GMap);
                var clear = GMap.Markers.Where(marker => marker.ID == (int)GMapMarkers_ID.DisMarker);
                if (clear.Count() > 1)
                {
                    disTag.Update(pos[1], GMap.MapProvider.Projection.GetDistance(pos[0], pos[1]));
                    disTag.Add();
                }
            }
        }

        public void Remove()
        {
            GMap.disPoints.Clear();
            GMap.RemoveMarkerAtZindex(GMap, (int)GMapMarkers_ZIndex.DisLine);
            disTag.Remove();
        }
    }
}
