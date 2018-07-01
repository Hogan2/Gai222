using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GMap.NET.WindowsPresentation
{
    public class TextMarker : GMapMarker
    {
        Text_Marker _Marker;
        public TextMarker(PointLatLng textpoint, string textmarkercontent, GMapControl map)
        {
            Map = map;
            Position = textpoint;
            ZIndex = (int)GMapMarkers_ZIndex.TextMarker;
            Offset = new Point(-30, -15);
            ID = (int)GMapMarkers_ID.TextMarker;
            TextMarkerContent = textmarkercontent;
            _Marker = new Text_Marker();
            
            Shape = _Marker;

            _Marker.MouseEnter += new MouseEventHandler(_Marker_MouseEnter);
            _Marker.MouseLeave += new MouseEventHandler(_Marker_MouseLeave);

            _Marker.MouseMove += new MouseEventHandler(_Marker_MouseMove);
            _Marker.MouseLeftButtonUp += new MouseButtonEventHandler(_Marker_MouseLeftButtonUp);
            _Marker.MouseLeftButtonDown += new MouseButtonEventHandler(_Marker_MouseLeftButtonDown);
            _Marker.MouseDoubleClick += new MouseButtonEventHandler(_Marker_MouseDoubleClick);
        }

        private void _Marker_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                RemoveSelected();

            }
        }

        private void _Marker_MouseLeave(object sender, MouseEventArgs e)
        {
            ID -= 40000;

        }

        private void _Marker_MouseEnter(object sender, MouseEventArgs e)
        {
            ID += 40000;

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

        public void RemoveAll()
        {
            Map.RemoveMarkerAtZindex(Map, (int)GMapMarkers_ZIndex.TextMarker);
        }

        public void Update(PointLatLng point)
        {
            Position = point;
        }

        public void RemoveSelected()
        {
            var clear = Map.Markers.Where(marker => marker.ID >= 40000 && marker.ID < 50000);
            if (clear.Count() > 0)
            {
                Map.Markers.Remove(clear.ElementAt(0));
            }
        }

        public void OnSave()
        {
            string sss = "";
            var ssss = Map.Markers.Where(marker => marker.ZIndex == (int)GMapMarkers_ZIndex.TextMarker);
            if (ssss.Count() > 0)
            {
                for (int i = 0; i < ssss.Count(); i++)
                {
                    string ss = ssss.ElementAt(i).Position.Lat.ToString() + " " + ssss.ElementAt(i).Position.Lng.ToString() + " "
                        + ssss.ElementAt(i).TextMarkerContent + "\r\n";
                    sss += ss;
                }
            }

            var dlg = new SaveFileDialog()
            {
                Title = "保存文本标记",
                DefaultExt = "txt",
                Filter = "txt files (*.txt)|*.txt|All files|*.*",
            };
            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText(dlg.FileName, sss);
            }
        }

        public void OnRead()
        {
            Map.ReadTextMarkers(Map);
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
