using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GMap.NET.WindowsPresentation
{
    public class PolygonMarker : GMapMarker
    {
        PolygonVertex PolygonVertex1;
        PolygonPath PolygonPath1;
        public PolygonMarker(int id, PointLatLng polygonpoint, GMapControl map)
        {
            Map = map;
            Position = polygonpoint;
            ZIndex = (int)GMapMarkers_ZIndex.PolygonMarker;
            Offset = new Point(-5, -5);
            ID = (int)GMapMarkers_ID.PolygonMarker + id;

            PolygonVertex1 = new PolygonVertex();
            Shape = PolygonVertex1;
            PolygonPath1 = new PolygonPath(Map);

            PolygonVertex1.MouseEnter += new MouseEventHandler(PolygonVertex1_MouseEnter);
            PolygonVertex1.MouseLeave += new MouseEventHandler(PolygonVertex1_MouseLeave);

            PolygonVertex1.MouseMove += new MouseEventHandler(PolygonVertex1_MouseMove);
            PolygonVertex1.MouseLeftButtonUp += new MouseButtonEventHandler(PolygonVertex1_MouseLeftButtonUp);
            PolygonVertex1.MouseLeftButtonDown += new MouseButtonEventHandler(PolygonVertex1_MouseLeftButtonDown);
            PolygonVertex1.MouseDoubleClick += new MouseButtonEventHandler(PolygonVertex1_MouseDoubleClick);
        }

        private void PolygonVertex1_MouseLeave(object sender, MouseEventArgs e)
        {
            ID -= 30000;

        }

        private void PolygonVertex1_MouseEnter(object sender, MouseEventArgs e)
        {
            ID += 30000;

        }

        private void PolygonVertex1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                RemoveSelected();
            }
        }

        private void PolygonVertex1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && PolygonVertex1.IsMouseCaptured && Map.IsPolygonMarkerCanDrag)
            {
                Point p = e.GetPosition(Map);
                PointLatLng point = Map.FromLocalToLatLng((int)p.X, (int)p.Y);
                Update(point);
            }
        }

        private void PolygonVertex1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (PolygonVertex1.IsMouseCaptured)
            {
                Mouse.Capture(null);
            }
        }

        private void PolygonVertex1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!PolygonVertex1.IsMouseCaptured)
            {
                Mouse.Capture(PolygonVertex1);

            }
        }

        public void Add()
        {
            Map.Markers.Add(this);

            var clear = Map.Markers.Where(marker => marker.ID == ID);
            if (clear.Count() > 2)
            {
                Map.polyPoints.Clear();

                for (int i = 0; i < clear.Count(); i++)
                {
                    Map.polyPoints.Add(clear.ElementAt(i).Position);
                }
                PolygonPath1.Update(ID - (int)GMapMarkers_ID.PolygonMarker, Map.polyPoints);
            }

        }

        public void Remove(int id)
        {
            Map.RemoveMarkerAtID(Map, (int)GMapMarkers_ID.PolygonMarker + id);
        }

        public void RemoveAll()
        {
            Map.RemoveMarkerAtZindex(Map, (int)GMapMarkers_ZIndex.PolygonMarker);
        }

        public void RemoveSelected()
        {
            int id = 0;
            var clear = Map.Markers.Where(marker => marker.ID >= 30000 && marker.ID < 40000);
            if (clear.Count() > 0)
            {
                id = clear.ElementAt(0).ID - 30000;
                Map.Markers.Remove(clear.ElementAt(0));
            }
            var clear1 = Map.Markers.Where(marker => marker.ID == id);
            if (clear1.Count() > 0)
            {
                Map.polyPoints.Clear();

                for (int i = 0; i < clear1.Count(); i++)
                {
                    Map.polyPoints.Add(clear1.ElementAt(i).Position);
                }
                PolygonPath1.Update(id - (int)GMapMarkers_ID.PolygonMarker, Map.polyPoints);
            }
        }

        public void Update(PointLatLng point)
        {
            Position = point;
            int id = 0;
            var clear = Map.Markers.Where(marker => marker.ID >= 30000 && marker.ID < 40000);
            if (clear.Count() > 0)
            {
                id = clear.ElementAt(0).ID - 30000;
            }
            var clear1 = Map.Markers.Where(marker => marker.ID == id||(marker.ID >= 30000 && marker.ID < 40000));
            if (clear1.Count() > 2)
            {
                Map.polyPoints.Clear();

                for (int i = 0; i < clear1.Count(); i++)
                {
                    Map.polyPoints.Add(clear1.ElementAt(i).Position);
                }
                PolygonPath1.Update(id - (int)GMapMarkers_ID.PolygonMarker, Map.polyPoints);
            }
        }

        public void OnSave()
        {
            string sss = "";
            var ssss = Map.Markers.Where(marker => marker.ZIndex == (int)GMapMarkers_ZIndex.PolygonMarker);
            if (ssss.Count() > 0)
            {
                for (int i = 0; i < ssss.Count(); i++)
                {
                    string ss = (ssss.ElementAt(i).ID - (int)GMapMarkers_ID.PolygonMarker).ToString() + " "
                        + ssss.ElementAt(i).Position.Lat.ToString() + " " + ssss.ElementAt(i).Position.Lng.ToString() + "\r\n";
                    sss += ss;
                }
            }

            var dlg = new SaveFileDialog()
            {
                Title = "保存多边形标记",
                DefaultExt = "txt",
                Filter = "poly files (*.poly)|*.poly|All files|*.*",
            };
            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText(dlg.FileName, sss);
            }
        }

        public void Read()
        {
            Map.ReadPolygonMarkers(Map);
        }

        public void OnSavePolyText()
        {
            string sss = "";
            var ssss = Map.Markers.Where(marker => marker.ZIndex == (int)GMapMarkers_ZIndex.PolygonMarker);
            if (ssss.Count() > 0)
            {
                for (int i = 0; i < ssss.Count(); i++)
                {
                    string ss = "poly "+(ssss.ElementAt(i).ID - (int)GMapMarkers_ID.PolygonMarker).ToString() + " "
                        + ssss.ElementAt(i).Position.Lat.ToString() + " " + ssss.ElementAt(i).Position.Lng.ToString() + "\r\n";
                    sss += ss;
                }
            }
            var ssss1 = Map.Markers.Where(marker => marker.ZIndex == (int)GMapMarkers_ZIndex.TextMarker);
            if (ssss1.Count() > 0)
            {
                for (int i = 0; i < ssss1.Count(); i++)
                {
                    string ss = "text "+ssss1.ElementAt(i).Position.Lat.ToString() + " " + ssss1.ElementAt(i).Position.Lng.ToString() + " "
                        + ssss1.ElementAt(i).TextMarkerContent + "\r\n";
                    sss += ss;
                }
            }
            var dlg = new SaveFileDialog()
            {
                Title = "保存标记",
                DefaultExt = "txt",
                Filter = "mark files (*.mark)|*.mark|All files|*.*",
            };
            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText(dlg.FileName, sss);
            }
        }
        public void OnReadPolyText()
        {
            Map.ReadPTMarkers(Map);
        }
    }
}
