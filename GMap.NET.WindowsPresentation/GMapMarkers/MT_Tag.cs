using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GMap.NET.WindowsPresentation
{
    public class MT_Tag : GMapMarker
    {
        Tag_Line Tag_Line1;
        int airplane_id = 0;
        
        //readonly Callout MyCallout;
        public MT_Tag(int id, PointLatLng pos, float bearing, GMapControl map)
        {
            Map = map;
            airplane_id = id;
            ID = (int)GMapMarkers_ID.MT_Tag + id;
            ZIndex = (int)GMapMarkers_ZIndex.MT_Tag;
            Position = pos;
            MyCallout = new Callout();
            Shape = MyCallout;
            Offset = new Point(-24, -110);
            TagetText = "航向：" + bearing.ToString("0.00") + "\n纬度：" + Math.Abs(Position.Lat).ToString("0.000000") +
            (Position.Lat >= 0 ? " N" : " S") + "\n经度：" + Math.Abs(Position.Lng).ToString("0.000000") +
            (Position.Lng >= 0 ? " E" : " W") + "\n高度：" + "100" + " m" + "\n空速：" + "30" + " m/s";

            MyCallout.MouseMove += new MouseEventHandler(MyCallout_MouseMove);
            MyCallout.MouseLeftButtonUp += new MouseButtonEventHandler(MyCallout_MouseLeftButtonUp);
            MyCallout.MouseLeftButtonDown += new MouseButtonEventHandler(MyCallout_MouseLeftButtonDown);
        }

        private void MyCallout_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!MyCallout.IsMouseCaptured)
            {
                Mouse.Capture(MyCallout);
                //ID += 10000;                
            }
        }

        private void MyCallout_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (MyCallout.IsMouseCaptured)
            {
                Mouse.Capture(null);
                //ID -= 10000;
            }
        }

        private void MyCallout_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && MyCallout.IsMouseCaptured)
            {
                Point p = e.GetPosition(Map);
                PointLatLng point = Map.FromLocalToLatLng((int)p.X, (int)p.Y);
                Update(ID - (int)GMapMarkers_ID.MT_Tag, point);
            }
        }

        public void IsVisable(int id, bool isvisable)
        {
            istagLineVisable = isvisable;
            Tag_Line1.IsVisable(id, isvisable);

            var clear = Map.Markers.Where(marker => marker.ID == (int)GMapMarkers_ID.MT_Tag + id);
            if (clear.Count() > 0)
            {
                if (isvisable)
                {
                    clear.ElementAt(0).MyCallout.Visibility = Visibility.Visible;                  
                }
                else
                {
                    clear.ElementAt(0).MyCallout.Visibility = Visibility.Hidden;
                }
            }
        }

        public void Add()
        {
            Map.Markers.Add(this);
            Tag_Line1 = new Tag_Line(Map);
            Tag_Line1.Drawing(airplane_id);
        }

        public void Remove(int id)
        {
            Map.RemoveMarkerAtID(Map, (int)GMapMarkers_ID.MT_Tag + id);
            Tag_Line1.Remove(id);
        }

        public void Update(int id, PointLatLng pos)
        {
            bool istaglinevisable = false;
            var clear = Map.Markers.Where(marker => marker.ID == (int)GMapMarkers_ID.MT_Tag + id);
            if (clear.Count() > 0)
            {
                clear.ElementAt(0).Position = pos;
                istaglinevisable = clear.ElementAt(0).istagLineVisable;
            }
            var clear1 = Map.Markers.Where(marker => marker.ID == (int)GMapMarkers_ID.MovingTarget + id);
            if (clear1.Count() > 0)
            {
                clear.ElementAt(0).TagetText = "航向：" + clear1.ElementAt(0).Bearing.ToString("0.00") + "\n纬度："
                        + Math.Abs(clear1.ElementAt(0).Position.Lat).ToString("0.000000")
                        + (clear1.ElementAt(0).Position.Lat >= 0 ? " N" : " S") + "\n经度："
                        + Math.Abs(clear1.ElementAt(0).Position.Lng).ToString("0.000000")
                        + (clear1.ElementAt(0).Position.Lng >= 0 ? " E" : " W") + "\n高度："
                        + "100" + " m" + "\n空速：" + "30" + " m/s";
            }
            Tag_Line1.Update(id, istaglinevisable);
        }
    }
}
