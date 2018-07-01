using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GMap.NET.WindowsPresentation
{
    public class CourseBeacon : GMapMarker
    {
        Circle WPMarker;
        AirRoute AirRoute1;

        bool isFirstEnter;
        int Line_id;

        public CourseBeacon(int id, PointLatLng waypoint, bool isfirstenter, GMapControl map)//航线编号/航点位置/鼠标是否第一次进入/父容器
        {
            isFirstEnter = isfirstenter;
            Map = map;
            Position = waypoint;
            ZIndex = (int)GMapMarkers_ZIndex.CourseBeacon;
            Line_id = id;
            WPNumber = Map.Markers.Where(marker => marker.ID == (int)GMapMarkers_ID.CourseBeacon + id
            || marker.ID >= 10000).Count() + 1;
            Offset = new Point(-24, -106);
            ID = (int)GMapMarkers_ID.CourseBeacon + id;

            TagText = "航点编号：" + Line_id + "-" + WPNumber + "\n纬度：" + Math.Abs(Position.Lat).ToString("0.000000") +
                (Position.Lat >= 0 ? " N" : " S") + "\n经度：" + Math.Abs(Position.Lng).ToString("0.000000") +
                (Position.Lng >= 0 ? " E" : " W") + "\n高度：" + "100" + " m" + "\n空速：" + "30" + " m/s";

            WPMarker = new Circle(this);
            Shape = WPMarker;
            AirRoute1 = new AirRoute(Map);

            SetFont();

            WPMarker.MouseMove += new MouseEventHandler(WPMarker_MouseMove);
            WPMarker.MouseLeftButtonUp += new MouseButtonEventHandler(WPMarker_MouseLeftButtonUp);
            WPMarker.MouseLeftButtonDown += new MouseButtonEventHandler(WPMarker_MouseLeftButtonDown);
            WPMarker.MouseEnter += new MouseEventHandler(WPMarker_MouseEnter);
            WPMarker.MouseLeave += new MouseEventHandler(WPMarker_MouseLeave);
            WPMarker.MouseDoubleClick += new MouseButtonEventHandler(WPMarker_MouseDoubleClick);
        }

        private void WPMarker_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed&& Map.IsWPMarkerCanRemove)
            {
                RemoveSelected();
            }

        }

        /// <summary>
        /// 设置航点编号图标
        /// </summary>
        private void SetFont()
        {
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
        }
        private void WPMarker_MouseLeave(object sender, MouseEventArgs e)
        {
            ID -= 10000;

            //ZIndex -= 10000;
            Map.IsWPMarkerCanAdd = true;
            isFirstEnter = false;
            WPMarker.MyTag.Visibility = Visibility.Hidden;
        }
        private void WPMarker_MouseEnter(object sender, MouseEventArgs e)
        {
            ID += 10000;

            //ZIndex += 10000;
            Map.IsWPMarkerCanAdd = false;
            if (!isFirstEnter) WPMarker.MyTag.Visibility = Visibility.Visible;
        }

        private void WPMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!WPMarker.IsMouseCaptured)
            {
                Mouse.Capture(WPMarker);
                //ID += 10000;

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
                //ID -= 10000;

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
                PointLatLng point = Map.FromLocalToLatLng((int)p.X, (int)p.Y);
                Update(point);
                AirRoute AirRoute1 = new AirRoute(Map);
                AirRoute1.Update(Line_id);
            }
        }

        /// <summary>
        /// 添加航点图标
        /// </summary>
        public void Add()
        {
            Map.Markers.Add(this);
            AirRoute1.Drawing(Line_id);

        }

        /// <summary>
        /// 删除选定的航点图标，并更新航点编号
        /// </summary>
        public void RemoveSelected()
        {
            var clear = Map.Markers.Where(marker => marker.ID >= 10000 && marker.ID < 20000);
            int id = 0;
            while (clear.Count() > 0)
            {
                id = clear.ElementAt(0).ID - 10000;
                Map.Markers.Remove(clear.ElementAt(0));
            };

            var wpmarker = Map.Markers.Where(marker => marker.ID == id);
            for (int i = 0; i < wpmarker.Count(); i++)
            {
                wpmarker.ElementAt(i).WPNumber = (i + 1);
                wpmarker.ElementAt(i).TagText = "航点编号：" + (id - (int)GMapMarkers_ID.CourseBeacon) + "-" + wpmarker.ElementAt(i).WPNumber + "\n纬度：" + Math.Abs(Position.Lat).ToString("0.000000") +
                (Position.Lat >= 0 ? " N" : " S") + "\n经度：" + Math.Abs(Position.Lng).ToString("0.000000") +
                (Position.Lng >= 0 ? " E" : " W") + "\n高度：" + "100" + " m" + "\n空速：" + "30" + " m/s";
            }
            AirRoute1.Update(id - (int)GMapMarkers_ID.CourseBeacon);

        }

        /// <summary>
        /// 删除所有航点图标
        /// </summary>
        public void RemoveAll()
        {
            Map.RemoveMarkerAtZindex(Map, (int)GMapMarkers_ZIndex.CourseBeacon);
            AirRoute1.RemoveAll();
        }
        /// <summary>
        /// 删除航线号为id的航点图标和航线
        /// </summary>
        /// <param name="id"></param>
        public void RemoveAtID(int id)
        {
            Map.RemoveMarkerAtID(Map, (int)GMapMarkers_ID.CourseBeacon + id);
            AirRoute1.Remove(id);
        }

        /// <summary>
        /// 更新航点图标位置、标签内容，并且更新航线
        /// </summary>
        /// <param name="point"></param>
        public void Update(PointLatLng point)
        {
            Position = point;
            TagText = "航点编号：" + Line_id + "-" + WPNumber + "\n纬度：" + Math.Abs(Position.Lat).ToString("0.000000") +
                (Position.Lat >= 0 ? " N" : " S") + "\n经度：" + Math.Abs(Position.Lng).ToString("0.000000") +
                (Position.Lng >= 0 ? " E" : " W") + "\n高度：" + "100" + " m" + "\n空速：" + "30" + " m/s";
        }

        public void OnSave()
        {
            string sss = "";
            var ssss = Map.Markers.Where(marker => marker.ZIndex == (int)GMapMarkers_ZIndex.CourseBeacon);
            if (ssss.Count() > 0)
            {
                for (int i = 0; i < ssss.Count(); i++)
                {
                    string ss = (ssss.ElementAt(i).ID - (int)GMapMarkers_ID.CourseBeacon).ToString() + " "
                        + ssss.ElementAt(i).Position.Lat.ToString() + " " + ssss.ElementAt(i).Position.Lng.ToString() + "\r\n";
                    sss += ss;
                }
            }

            var dlg = new SaveFileDialog()
            {
                Title = "保存航点",
                DefaultExt = "txt",
                Filter = "wpt files (*.wpt)|*.wpt|All files|*.*",
            };
            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText(dlg.FileName, sss);
            }
        }

        public void OnRead()
        {
            Map.ReadWPTMarkers(Map);
        }
    }
}
