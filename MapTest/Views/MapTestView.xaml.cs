using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static GMap.NET.WindowsPresentation.GMapMarker;

namespace MapTest.Views
{
    /// <summary>
    /// MapTestView.xaml 的交互逻辑
    /// </summary>
    public partial class MapTestView : UserControl
    {
        TooType type;
        bool IsStart = true;
        bool IsTextFocusable = false;
        bool isTrackVisable = true;
        MovingTarget movingTarget;
        DisMarker DisMarker1;
        PolygonMarker polygonMarker;
        TextMarker textMarker;
        double lng = 0.0;
        int WPLine = 1;
        List<PointLatLng> pont1 = new List<PointLatLng>();

        float bear = 0.0f;

        private DispatcherTimer _timer = new DispatcherTimer();
        CourseBeacon CourseBeacon1;
        public MapTestView()
        {
            InitializeComponent();
            IniMap();

            _timer.Tick += OnTick;
            _timer.Interval = TimeSpan.FromMilliseconds(50);
            //movingTarget = new MovingTarget(i, GMapCtrl.Position, 0.0f, GMapCtrl);
            //movingTarget.Add();
            //AddPlane();
            MouseDoubleClick += new MouseButtonEventHandler(MapTest_MouseDoubleClick);
        }

        /// <summary>
        /// 定时器处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTick(object sender, EventArgs e)
        {
            for (int j = 0; j < i; j++)
            {
                movingTarget.Update(j + 1, new PointLatLng(pont1[j].Lat, pont1[j].Lng + lng), bear);
            }
            //lat += 0.0001;
            lng += 0.0001;
            if (bear < 90.0) bear += 1.0f;
            if (!GMapCtrl.PointInViewArea(movingTarget.Position.Lng, movingTarget.Position.Lat))
            {
                GMapCtrl.Position = movingTarget.Position;
            }
        }

        void IniMap()
        {
            //GMapCtrl.CacheLocation = @"D:\LOG\ProgramFiles\MapDownloader\MapCache";
            GMapCtrl.MapProvider = GMapProviders.AMapSatelite;
            GMapCtrl.Manager.Mode = AccessMode.ServerAndCache;
            GMapCtrl.Position = new PointLatLng(30.6898, 103.9468);
            GMapCtrl.MaxZoom = 18;
            GMapCtrl.MinZoom = 5;
            GMapCtrl.Zoom = 16;
            GMapCtrl.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            GMapCtrl.ShowCenter = false;
            GMapCtrl.IgnoreMarkerOnMouseWheel = true;
            GMapCtrl.DragButton = MouseButton.Right;
            GMapCtrl.ShowTileGridLines = false;
        }

        int i = 0;
        PointLatLng newWayPoint1;
        private void MapTest_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(GMapCtrl);
            PointLatLng newWayPoint = GMapCtrl.FromLocalToLatLng((int)p.X, (int)p.Y);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                switch (type)
                {
                    case TooType.AddWP:
                        {
                            //鼠标双击添加航点时isfirstenter为true
                            CourseBeacon1 = new CourseBeacon(WPLine, newWayPoint,true, GMapCtrl);
                            CourseBeacon1.Add();
                        }
                        break;
                    case TooType.DeleteWP:
                        {
                            CourseBeacon1.RemoveSelected();
                        }
                        break;
                    case TooType.AddTarget:
                        {
                            i++;
                            pont1.Add(newWayPoint);
                            movingTarget = new MovingTarget(i, newWayPoint, 0.0f, GMapCtrl);
                            movingTarget.Add();
                        }
                        break;
                    case TooType.DeleteTarget:
                        {
                            movingTarget.RemoveSelected();
                        }
                        break;
                    case TooType.AddPolygonMarker:
                        {
                            polygonMarker = new PolygonMarker(1, newWayPoint, GMapCtrl);
                            polygonMarker.Add();
                        }
                        break;
                    case TooType.DePolygonMarker:
                        {
                            polygonMarker.RemoveSelected();
                        }
                        break;
                    case TooType.AddText:
                        {
                            textMarker = new TextMarker(newWayPoint, GMapCtrl);
                            textMarker.Add();
                        }
                        break;
                    case TooType.DeText:
                        {
                            if (textMarker != null) textMarker.IsTextMarkerFocusable(false);

                        }
                        break;
                    case TooType.Distance:
                        {
                            DisMarker1 = new DisMarker(newWayPoint, GMapCtrl);
                            DisMarker1.Add();
                        }
                        break;
                    default:
                        break;
                }
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {

            }
        }
        float delt = 0.0f;
        private void AddPlane()
        {
            for (int i = 0; i < 100; i++)
            {
                delt += 0.0005f;
                newWayPoint1 = new PointLatLng(GMapCtrl.Position.Lat + delt, GMapCtrl.Position.Lng);
                pont1.Add(newWayPoint1);
                movingTarget = new MovingTarget(i + 1, newWayPoint1, 0.0f, GMapCtrl);
                movingTarget.Add();
            }
        }

        enum TooType
        {
            Null,
            AddWP,
            DeleteWP,
            AddTarget,
            DeleteTarget,
            AddPolygonMarker,
            DePolygonMarker,
            AddPolygon,
            DePolygon,
            AddText,
            DeText,
            Distance
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            type = TooType.AddWP;
            GMapCtrl.IsWPMarkerCanGrag = true;
        }

        private void Button1_1_Click(object sender, RoutedEventArgs e)
        {
            type = TooType.DeleteWP;
            GMapCtrl.IsWPMarkerCanGrag = false;
        }

        private void Button1_2_Click(object sender, RoutedEventArgs e)
        {
            WPLine++;
        }

        private void Button1_3_Click(object sender, RoutedEventArgs e)
        {
            CourseBeacon1.OnSave();
        }

        private void Button1_4_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog()
            {
                Title = "打开航点文件",
                CheckPathExists = true,
                CheckFileExists = true,
                Filter = "wpt files (*.wpt)|*.wpt|All files|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            if (dlg.ShowDialog() == true)
            {
                if (CourseBeacon1 != null) CourseBeacon1.RemoveAll();
                Regex regex = new Regex("\r\n");
                Regex regex1 = new Regex(" ");

                string ss = File.ReadAllText(dlg.FileName);
                string[] dd = regex.Split(ss);
                foreach (string ff in dd)
                {
                    if (ff != "")
                    {
                        string[] gg = regex1.Split(ff);
                        //读取文件添加航点时isfirstenter为false
                        CourseBeacon1 = new CourseBeacon(Convert.ToInt16(gg[0]),
                            new PointLatLng(Convert.ToDouble(gg[1]), Convert.ToDouble(gg[2])),false, GMapCtrl);
                        CourseBeacon1.Add();
                    }

                }
            }
        }
        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            type = TooType.AddTarget;
        }

        private void Button2_1_Click(object sender, RoutedEventArgs e)
        {
            type = TooType.DeleteTarget;
        }
        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            type = TooType.AddPolygonMarker;
        }

        private void Button3_1_Click(object sender, RoutedEventArgs e)
        {
            type = TooType.DePolygonMarker;
        }

        private void Button3_2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            type = TooType.AddText;
        }

        private void Button4_1_Click(object sender, RoutedEventArgs e)
        {
            type = TooType.DeText;
        }

        private void Button4_2_Click(object sender, RoutedEventArgs e)
        {
            if (textMarker != null) textMarker.IsTextMarkerFocusable(IsTextFocusable);
            if (IsTextFocusable) focus.Content = "文本编辑";
            else focus.Content = "文本聚焦";
            IsTextFocusable = !IsTextFocusable;
        }
        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            type = TooType.Distance;
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            if (IsStart)
            {
                _timer.Start();
                start.Content = "停止";
            }
            else
            {
                _timer.Stop();
                start.Content = "开始";
            }
            IsStart = !IsStart;
        }

        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            if (movingTarget != null) movingTarget.RemoveAllMT_Tracks();
        }

        private void Button8_Click(object sender, RoutedEventArgs e)
        {
            isTrackVisable = !isTrackVisable;
            if(movingTarget!=null) movingTarget.IsAllTrackVisable(isTrackVisable);
            if (isTrackVisable) hide.Content = "隐藏航迹";
            else hide.Content = "显示航迹";
        }

    }
}
