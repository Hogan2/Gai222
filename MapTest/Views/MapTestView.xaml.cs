using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MapTest.Views
{
    /// <summary>
    /// MapTestView.xaml 的交互逻辑
    /// </summary>
    public partial class MapTestView : UserControl
    {
        bool flag = true;
        TargetMarker TargetMarker1;

        double lat = 30.6898;
        double lng = 103.9468;
        float bear = 0.0f;


        public MapTestView()
        {
            InitializeComponent();
            IniMap();

            TargetMarker1 = new TargetMarker(GMapCtrl.Position, 45.0f, GMapCtrl);
            MouseDoubleClick += new MouseButtonEventHandler(MapTest_MouseDoubleClick);
        }

        private async void UpdateTime()
        {
            while (true)
            {
                await Task.Run(() => Thread.Sleep(50));
                GMapCtrl.UpdateTrackMarker(GMapCtrl, TargetMarker1, new PointLatLng(lat, lng), bear);
                //lat += 0.0001;
                lng += 0.0001;
                bear += 1.0f;
                //await Task.Delay(0);
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
        private void MapTest_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(GMapCtrl);
            PointLatLng newWayPoint = GMapCtrl.FromLocalToLatLng((int)p.X, (int)p.Y);

            //添加航点
            if (e.LeftButton == MouseButtonState.Pressed && flag && GMapCtrl.IsWPMarkerCanAdd)
            {
                WayPointMarker WayPointMarker = new WayPointMarker(newWayPoint, GMapCtrl);
                GMapCtrl.UpdateRouteMarker(GMapCtrl);
            }

            //删除航点
            if (e.LeftButton == MouseButtonState.Pressed && !flag)
            {
                GMapCtrl.RemoveSelectedMarker(GMapCtrl);
                GMapCtrl.UpdateRouteMarker(GMapCtrl);
            }
        }
        /// <summary>
        /// 模拟模式切换, 添加航点模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button111_Click_(object sender, RoutedEventArgs e)
        {
            flag = true;
            GMapCtrl.IsWPMarkerCanGrag = true;
        }
        /// <summary>
        /// 模拟模式切换, 删除航点模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button222_Click(object sender, RoutedEventArgs e)
        {
            flag = false;
            GMapCtrl.IsWPMarkerCanGrag = false;
        }

        /// <summary>
        /// 模拟起飞按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button333_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(UpdateTime), null);
        }
    }
}
