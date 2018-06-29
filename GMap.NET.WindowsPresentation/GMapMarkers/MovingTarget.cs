using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GMap.NET.WindowsPresentation
{
    public class MovingTarget : GMapMarker
    {
        AirPlane AirPlane1;
        bool IsTagVisible = true;
        MT_Tag MT_Tag1;
        MT_Track MT_Track1;
        bool isTrackVisable = true;
        //List<PointLatLng> MTTrackPoints = new List<PointLatLng>();

        int airplane_id = 0;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">范围1~200</param>
        /// <param name="pos"></param>
        /// <param name="bearing"></param>
        /// <param name="map"></param>
        public MovingTarget(int id, PointLatLng pos, float bearing, GMapControl map)
        {
            airplane_id = id;
            ID = (int)GMapMarkers_ID.MovingTarget + id;
            ZIndex = (int)GMapMarkers_ZIndex.MovingTarget;
            Map = map;
            AirPlane1 = new AirPlane();
            Shape = AirPlane1;
            Offset = new Point(-32, -34);

            Position = pos;
            Bearing = bearing >= 0.0f && bearing <= 360.0f ? bearing : 0.0f;
            //TagetText = "航向：" + Bearing.ToString("0.00") + "\n纬度：" + Math.Abs(Position.Lat).ToString("0.000000") +
            //(Position.Lat >= 0 ? " N" : " S") + "\n经度：" + Math.Abs(Position.Lng).ToString("0.000000") +
            //(Position.Lng >= 0 ? " E" : " W") + "\n高度：" + "100" + " m" + "\n空速：" + "30" + " m/s";

            AirPlane1.MouseEnter += new MouseEventHandler(MyTarget_MouseEnter);
            AirPlane1.MouseLeave += new MouseEventHandler(MyTarget_MouseLeave);
            AirPlane1.MouseLeftButtonUp += new MouseButtonEventHandler(AirPlane1_MouseLeftButtonUp);
            AirPlane1.MouseLeftButtonDown += new MouseButtonEventHandler(AirPlane1_MouseLeftButtonDown);
        }

        private void AirPlane1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!AirPlane1.IsMouseCaptured)
            {
                Mouse.Capture(AirPlane1);
                ID += 20000;
                if (IsTagVisible)
                {
                    MT_Tag1.IsVisable(airplane_id, true);
                }
                else
                {
                    MT_Tag1.IsVisable(airplane_id, false);
                }
                IsTagVisible = !IsTagVisible;
            }
        }

        private void AirPlane1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (AirPlane1.IsMouseCaptured)
            {
                Mouse.Capture(null);
                ID -= 20000;

            }
        }

        private void MyTarget_MouseLeave(object sender, MouseEventArgs e)
        {
            Map.IsWPMarkerCanAdd = true;
        }

        private void MyTarget_MouseEnter(object sender, MouseEventArgs e)
        {
            Map.IsWPMarkerCanAdd = false;
        }

        public void Add()
        {
            Map.Markers.Add(this);
            MT_Tag1 = new MT_Tag(airplane_id, Position, Bearing, Map);
            MT_Tag1.Add();
            MT_Tag1.IsVisable(airplane_id, false);
            MT_Track1 = new MT_Track(Map);
        }

        /// <summary>
        /// 删除选定id的活动目标
        /// </summary>
        /// <param name="id"></param>
        public void Remove(int id)
        {
            Map.RemoveMarkerAtID(Map, (int)GMapMarkers_ID.MovingTarget + id);
            MT_Tag1.Remove(id);
            MT_Track1.RemoveTrack(id);
        }

        public void RemoveSelected()
        {
            var clear = Map.Markers.Where(marker => marker.ID >= 20000 && marker.ID < 30000);////////////
            int id = 0;
            if(clear.Count() > 0)
            {
                id = clear.ElementAt(0).ID -20000- (int)GMapMarkers_ID.MovingTarget;
                MT_Tag1.Remove(id);
                MT_Track1.RemoveTrack(id);
                Map.Markers.Remove(clear.ElementAt(0));
            }
        }

        public void RemoveMT_Track(int id)
        {
            MT_Track1.RemoveTrack(id);
        }

        public void RemoveAllMT_Tracks()
        {
            MT_Track1.RemoveAllTracks();
        }

        public void IsAllTrackVisable(bool isvisable)
        {
            isTrackVisable = isvisable;
            var clear = Map.Markers.Where(marker => marker.ZIndex == (int)GMapMarkers_ZIndex.MovingTarget);
            if (clear.Count()>0)
            {
                for (int i=0;i<clear.Count();i++)
                {
                    MT_Track1.IsVisable(clear.ElementAt(i).ID - (int)GMapMarkers_ID.MovingTarget, isvisable);

                }
            }
        }
        /// <summary>
        /// 更新选定id号的活动目标
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pos"></param>
        /// <param name="bearing"></param>
        public void Update(int id, PointLatLng pos, float bearing)
        {
            SizeLatLng delta = new SizeLatLng();
            PointLatLng TrackMarkerPos_Old;
            PointLatLng TrackMarkerPos_New;

            var clear = Map.Markers.Where(marker => marker.ID == (int)GMapMarkers_ID.MovingTarget + id );
            var clear1 = Map.Markers.Where(marker => marker.ID == (int)GMapMarkers_ID.MT_Tag + id);

            if (clear.Count() > 0 && clear1.Count() > 0)
            {
                TrackMarkerPos_Old = clear.ElementAt(0).Position;

                delta = clear.ElementAt(0).Position - clear1.ElementAt(0).Position;
                clear.ElementAt(0).Position = pos;
                clear.ElementAt(0).Bearing = bearing >= 0.0f && bearing <= 360.0f ? bearing : 0.0f;
                MT_Tag1.Update(id, pos + delta);

                TrackMarkerPos_New = clear.ElementAt(0).Position;
                if (TrackMarkerPos_New != TrackMarkerPos_Old) clear.ElementAt(0).MTTrackPoints.Add(TrackMarkerPos_New);
                MT_Track1.Update(id, clear.ElementAt(0).MTTrackPoints, isTrackVisable);
            }
        }
    }
}
