using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using static GMap.NET.WindowsPresentation.GMapMarker;

namespace GMap.NET.WindowsPresentation
{
    public class AirRoute
    {
        GMapControl GMap;
        List<PointLatLng> AirRoutePoints = new List<PointLatLng>();

        public AirRoute(GMapControl map)
        {
            GMap = map;
        }

        /// <summary>
        /// 绘制航线
        /// </summary>
        public void Drawing(int id)
        {
            var ssss = GMap.Markers.Where(marker => marker.ID == (int)GMapMarkers_ID.CourseBeacon+id
            || marker.ID >= 10000);
            Remove(id);
            AirRoutePoints.Clear();
            for (int i = 0; i < ssss.Count(); i++)
            {
                AirRoutePoints.Add(ssss.ElementAt(i).Position);
            }
            GMapRoute airRoute = new GMapRoute((int)GMapMarkers_ID.AirRoute+id,
                (int)GMapMarkers_ZIndex.AirRoute, AirRoutePoints, Colors.Yellow, 4, GMap);
        }

        /// <summary>
        /// 删除航线
        /// </summary>
        public void Remove(int id)
        {
            GMap.RemoveMarkerAtID(GMap, (int)GMapMarkers_ID.AirRoute + id);
        }

        public void RemoveAll()
        {
            GMap.RemoveMarkerAtZindex(GMap, (int)GMapMarkers_ZIndex.AirRoute);
        }

        /// <summary>
        /// 更新航线
        /// </summary>
        /// <param name="points"></param>
        public void Update(int id)
        {
            Remove(id);
            Drawing(id);
        }
    }
}
