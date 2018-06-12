using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
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
        int flag = 1;
        TargetMarker TargetMarker1;
        public MapTestView()
        {
            InitializeComponent();
            IniMap();
            TargetMarker1 = new TargetMarker(GMapCtrl.Position, 30.0, GMapCtrl);
            MouseDoubleClick += new MouseButtonEventHandler(MapTest_MouseDoubleClick);
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
        //List<PointLatLng> points = new List<PointLatLng>();
        private void MapTest_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(GMapCtrl);
            PointLatLng newWayPoint = GMapCtrl.FromLocalToLatLng((int)p.X, (int)p.Y);

            //添加航点
            if (e.LeftButton == MouseButtonState.Pressed && flag == 1 && GMapCtrl.IsWPMarkerCanAdd)
            {
                WayPointMarker WayPointMarker = new WayPointMarker(newWayPoint, GMapCtrl);
                GMapCtrl.UpdateRouteMarker(GMapCtrl);
            }

            //删除航点
            if (e.LeftButton == MouseButtonState.Pressed && flag == 2)
            {
                GMapCtrl.RemoveSelectedMarker(GMapCtrl);
                GMapCtrl.UpdateRouteMarker(GMapCtrl);
            }
        }

        private void Button111_Click_(object sender, RoutedEventArgs e)
        {
            flag = 1;
            GMapCtrl.IsWPMarkerCanGrag = true;
        }

        private void Button222_Click(object sender, RoutedEventArgs e)
        {
            flag = 2;
            GMapCtrl.IsWPMarkerCanGrag = false;
        }
    }
}
