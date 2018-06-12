using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GMap.NET.WindowsPresentation
{
    public class WayPointMarker : GMapMarker
    {
        Circle WPMarker;
        bool isFirstEnter = true;

        public WayPointMarker(PointLatLng waypoint, GMapControl map)
        {
            Map = map;
            Position = waypoint;
            ZIndex = (int)Markers_ZIndex.WaypointMarker;
            WPNumber = Map.Markers.Where(marker => marker.ID == (int)Markers_ID.WaypointMarker
            || marker.ID >= 10000).Count() + 1;
            Offset = new Point(-24, -106);
            ID = (int)Markers_ID.WaypointMarker;

            TagText = "航点：" + WPNumber + "\n纬度：" + Math.Abs(Position.Lat).ToString("0.000000") +
                (Position.Lat >= 0 ? " N" : " S") + "\n经度：" + Math.Abs(Position.Lng).ToString("0.000000") +
                (Position.Lng >= 0 ? " E" : " W") + "\n高度：" + "100" + " m" + "\n空速：" + "30" + " m/s";

            WPMarker = new Circle(this);
            Shape = WPMarker;

            #region set fontsize
            if (WPNumber < 10)
            {
                WPMarker.wayPointIndex.Width = 20;
                WPMarker.wayPointIndex.Height = 18;
                WPMarker.wayPointIndex.FontSize = 14;
                WPMarker.wayPointIndex.Margin = new Thickness(4, 5, 4, 5);
            }
            else if (WPNumber >= 10 && WPNumber < 100)
            {
                WPMarker.wayPointIndex.Width = 20;
                WPMarker.wayPointIndex.Height = 16;
                WPMarker.wayPointIndex.FontSize = 13;
                WPMarker.wayPointIndex.Margin = new Thickness(4, 6, 4, 6);
            }
            else
            {
                WPMarker.wayPointIndex.Width = 20;
                WPMarker.wayPointIndex.Height = 12;
                WPMarker.wayPointIndex.FontSize = 10;
                WPMarker.wayPointIndex.Margin = new Thickness(4, 8, 4, 8);
            }
            #endregion

            WPMarker.MouseMove += new MouseEventHandler(WPMarker_MouseMove);
            WPMarker.MouseLeftButtonUp += new MouseButtonEventHandler(WPMarker_MouseLeftButtonUp);
            WPMarker.MouseLeftButtonDown += new MouseButtonEventHandler(WPMarker_MouseLeftButtonDown);
            WPMarker.MouseEnter += new MouseEventHandler(WPMarker_MouseEnter);
            WPMarker.MouseLeave += new MouseEventHandler(WPMarker_MouseLeave);
            Map.Markers.Add(this);
        }

        private void WPMarker_MouseLeave(object sender, MouseEventArgs e)
        {
            ZIndex -= 10000;
            Map.IsWPMarkerCanAdd = true;
            isFirstEnter = false;
            WPMarker.MyTag.Visibility = Visibility.Hidden;
        }
        private void WPMarker_MouseEnter(object sender, MouseEventArgs e)
        {
            ZIndex += 10000;
            Map.IsWPMarkerCanAdd = false;
            if (!isFirstEnter) WPMarker.MyTag.Visibility = Visibility.Visible;
        }

        private void WPMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!WPMarker.IsMouseCaptured)
            {
                Mouse.Capture(WPMarker);
                ID += 10000;

                WPMarker.bigCircle.Fill = new SolidColorBrush
                {
                    Color = Color.FromRgb(0xfc, 0x63, 0x55)
                };
            }
        }

        private void WPMarker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (WPMarker.IsMouseCaptured)
            {
                Mouse.Capture(null);
                ID -= 10000;

                WPMarker.bigCircle.Fill = new SolidColorBrush
                {
                    Color = Color.FromRgb(0x7f, 0x7f, 0xf5)
                };
            }
        }

        private void WPMarker_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && WPMarker.IsMouseCaptured && Map.IsWPMarkerCanGrag)
            {
                Point p = e.GetPosition(Map);
                Position = Map.FromLocalToLatLng((int)p.X, (int)p.Y);
                Offset = new Point(-24, -106);
                TagText = "航点：" + WPNumber + "\n纬度：" + Math.Abs(Position.Lat).ToString("0.000000") +
                    (Position.Lat >= 0 ? " N" : " S") + "\n经度：" + Math.Abs(Position.Lng).ToString("0.000000") +
                    (Position.Lng >= 0 ? " E" : " W") + "\n高度：" + "100" + " m" + "\n空速：" + "30" + " m/s";
                Map.UpdateRouteMarker(Map);
            }
        }
    }
}
