using System;
using System.Windows;

namespace GMap.NET.WindowsPresentation
{
    public class TargetMarker : GMapMarker
    {
        readonly FixedWing MyTarget;

        //public double Bearing { get; set; }
        public TargetMarker(PointLatLng pos, double bearing, GMapControl map)
        {
            ID = (int)Markers_ZIndex.TargetMarker;
            ZIndex = (int)Markers_ZIndex.TargetMarker;
            Map = map;
            Position = pos;
            MyTarget = new FixedWing();
            Shape = MyTarget;
            Bearing = bearing;
            Offset = new Point(-32, -106);
            TagetText = "航向：" + Bearing.ToString("0.00") + "\n纬度：" + Math.Abs(Position.Lat).ToString("0.000000") +
                (Position.Lat >= 0 ? " N" : " S") + "\n经度：" + Math.Abs(Position.Lng).ToString("0.000000") +
                (Position.Lng >= 0 ? " E" : " W") + "\n高度：" + "100" + " m" + "\n空速：" + "30" + " m/s";
            //Shape.RenderTransform = new RotateTransform(Bearing, 32, 34);
            Map.Markers.Add(this);
        }
    }
}
