using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GMap.NET.WindowsPresentation
{
    public class TextMarker : GMapMarker
    {
        Text_Marker _Marker;
        public TextMarker(PointLatLng textpoint, GMapControl map)
        {
            Map = map;
            Position = textpoint;
            ZIndex = (int)GMapMarkers_ZIndex.TextMarker;
            Offset = new Point(-30, -15);
            ID = (int)GMapMarkers_ID.TextMarker;
            _Marker = new Text_Marker();

            Shape = _Marker;

            _Marker.MouseMove += new MouseEventHandler(_Marker_MouseMove);
            _Marker.MouseLeftButtonUp += new MouseButtonEventHandler(_Marker_MouseLeftButtonUp);
            _Marker.MouseLeftButtonDown += new MouseButtonEventHandler(_Marker_MouseLeftButtonDown);
        }

        private void _Marker_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _Marker.IsMouseCaptured)
            {
                Point p = e.GetPosition(Map);
                PointLatLng point = Map.FromLocalToLatLng((int)p.X, (int)p.Y);
                Update(point);
            }
        }

        private void _Marker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_Marker.IsMouseCaptured)
            {
                Mouse.Capture(null);
            }
        }

        private void _Marker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_Marker.IsMouseCaptured)
            {
                Mouse.Capture(_Marker);
            }
        }

        public void Add()
        {
            Map.Markers.Add(this);
        }

        public void Remove()
        {
            Map.RemoveMarkerAtZindex(Map, (int)GMapMarkers_ZIndex.TextMarker);
        }

        public void Update(PointLatLng point)
        {
            Position = point;
        }

        public void RemoveSelected()
        {

        }

        public void IsTextMarkerFocusable(bool isfocusable)
        {
            var clear = Map.Markers.Where(marker => marker.ZIndex == (int)GMapMarkers_ZIndex.TextMarker);
            if (clear.Count() > 0)
            {
                for (int i = 0; i < clear.Count(); i++)
                {
                    clear.ElementAt(i).IsTextFocusable = isfocusable;
                }
            }
        }
    }
}
