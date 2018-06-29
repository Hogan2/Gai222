using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GMap.NET.WindowsPresentation
{
    public class DisMarker : GMapMarker
    {
        readonly Ruler Ruler1;
        DisLine disLine;
        public DisMarker(PointLatLng dispoint, GMapControl map)
        {
            Map = map;
            Position = dispoint;
            ZIndex = (int)GMapMarkers_ZIndex.DisMarker;
            Offset = new Point(-16, -32);
            ID = (int)GMapMarkers_ID.DisMarker;

            Ruler1 = new Ruler();
            Shape = Ruler1;
            disLine = new DisLine(Map);
            Ruler1.MouseMove += new MouseEventHandler(Ruler1_MouseMove);
            Ruler1.MouseLeftButtonUp += new MouseButtonEventHandler(Ruler1_MouseLeftButtonUp);
            Ruler1.MouseLeftButtonDown += new MouseButtonEventHandler(Ruler1_MouseLeftButtonDown);
        }

        private void Ruler1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && Ruler1.IsMouseCaptured)
            {
                Point p = e.GetPosition(Map);
                PointLatLng point = Map.FromLocalToLatLng((int)p.X, (int)p.Y);
                Update(point);
                
            }
        }

        private void Ruler1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Ruler1.IsMouseCaptured)
            {
                Mouse.Capture(null);
            }
        }

        private void Ruler1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Ruler1.IsMouseCaptured)
            {
                Mouse.Capture(Ruler1);
            }
        }

        public void Add()
        {
            var clear = Map.Markers.Where(marker => marker.ID == (int)GMapMarkers_ID.DisMarker);
            if (clear.Count() >= 2)
            {                
                Remove();
                disLine.Remove();
            }
            Map.Markers.Add(this);
            Map.disPoints.Add(Position);
            disLine.Drawing(Map.disPoints);
        }   

        public void Remove()
        {
            Map.RemoveMarkerAtZindex(Map, (int)GMapMarkers_ZIndex.DisMarker);
        }

        public void Update(PointLatLng point)
        {
            Position = point;
            var clear = Map.Markers.Where(marker => marker.ID == (int)GMapMarkers_ID.DisMarker);
            if (clear.Count() == 2)
            {
                disLine.Remove();
                Map.disPoints.Add(clear.ElementAt(0).Position);
                Map.disPoints.Add(clear.ElementAt(1).Position);
                disLine.Drawing(Map.disPoints);
            }
        }
    }
}
