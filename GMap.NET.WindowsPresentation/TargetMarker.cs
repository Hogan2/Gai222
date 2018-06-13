using System.Windows;
using System.Windows.Input;

namespace GMap.NET.WindowsPresentation
{
    public class TargetMarker : GMapMarker
    {
        readonly FixedWing MyTarget;
        bool IsTagVisible = false;
        public TargetMarker(PointLatLng pos, float bearing, GMapControl map)
        {
            ID = (int)Markers_ZIndex.TargetMarker;
            ZIndex = (int)Markers_ZIndex.TargetMarker;
            Map = map;
            MyTarget = new FixedWing();
            Shape = MyTarget;
            Offset = new Point(-32, -106);
            UpdateTargetProperty(this, pos, bearing >= 0.0f && bearing <= 360.0f ? bearing : 0.0f);
            Map.Markers.Add(this);

            MyTarget.MouseEnter += new MouseEventHandler(MyTarget_MouseEnter);
            MyTarget.MouseLeave += new MouseEventHandler(MyTarget_MouseLeave);
            MyTarget.MouseDown += new MouseButtonEventHandler(MyTarget_MouseDown);
        }

        private void MyTarget_MouseLeave(object sender, MouseEventArgs e)
        {
            Map.IsWPMarkerCanAdd = true;
        }

        private void MyTarget_MouseEnter(object sender, MouseEventArgs e)
        {
            Map.IsWPMarkerCanAdd = false;
        }

        private void MyTarget_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!IsTagVisible) MyTarget.MyTag.Visibility = Visibility.Visible;
                else MyTarget.MyTag.Visibility = Visibility.Hidden;
                IsTagVisible = !IsTagVisible;
            }
        }
    }
}
