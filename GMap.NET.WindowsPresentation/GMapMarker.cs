
namespace GMap.NET.WindowsPresentation
{
    using GMap.NET;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Shapes;

    /// <summary>
    /// GMap.NET marker
    /// </summary>
    public class GMapMarker : DependencyObject, INotifyPropertyChanged
    {
        /// <summary>
        /// /
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        protected void OnPropertyChanged(PropertyChangedEventArgs name)
        {
            PropertyChanged?.Invoke(this, name);
        }

        UIElement shape;
        static readonly PropertyChangedEventArgs Shape_PropertyChangedEventArgs = new PropertyChangedEventArgs("Shape");

        /// <summary>
        /// marker visual
        /// </summary>
        public UIElement Shape
        {
            get
            {
                return shape;
            }
            set
            {
                if (shape != value)
                {
                    shape = value;
                    OnPropertyChanged(Shape_PropertyChangedEventArgs);

                    UpdateLocalPosition();
                }
            }
        }

        private PointLatLng position;

        /// <summary>
        /// coordinate of marker
        /// </summary>
        public PointLatLng Position
        {
            get
            {
                return position;
            }
            set
            {
                if (position != value)
                {
                    position = value;
                    UpdateLocalPosition();
                }
            }
        }

        GMapControl map;

        /// <summary>
        /// the map of this marker
        /// </summary>
        public GMapControl Map
        {
            get
            {
                if (Shape != null && map == null)
                {
                    DependencyObject visual = Shape;
                    while (visual != null && !(visual is GMapControl))
                    {
                        visual = VisualTreeHelper.GetParent(visual);
                    }

                    map = visual as GMapControl;
                }

                return map;
            }
            internal set
            {
                map = value;
            }
        }

        /// <summary>
        /// custom object
        /// </summary>
        public object Tag;

        /// <summary>
        /// marker ID
        /// </summary>

        Point offset;
        /// <summary>
        /// offset of marker
        /// </summary>
        public Point Offset
        {
            get
            {
                return offset;
            }
            set
            {
                if (offset != value)
                {
                    offset = value;
                    UpdateLocalPosition();
                }
            }
        }

        int localPositionX;
        static readonly PropertyChangedEventArgs LocalPositionX_PropertyChangedEventArgs = new PropertyChangedEventArgs("LocalPositionX");

        /// <summary>
        /// local X position of marker
        /// </summary>
        public int LocalPositionX
        {
            get
            {
                return localPositionX;
            }
            internal set
            {
                if (localPositionX != value)
                {
                    localPositionX = value;
                    OnPropertyChanged(LocalPositionX_PropertyChangedEventArgs);
                }
            }
        }

        int localPositionY;
        static readonly PropertyChangedEventArgs LocalPositionY_PropertyChangedEventArgs = new PropertyChangedEventArgs("LocalPositionY");

        /// <summary>
        /// local Y position of marker
        /// </summary>
        public int LocalPositionY
        {
            get
            {
                return localPositionY;
            }
            internal set
            {
                if (localPositionY != value)
                {
                    localPositionY = value;
                    OnPropertyChanged(LocalPositionY_PropertyChangedEventArgs);
                }
            }
        }

        int zIndex;
        static readonly PropertyChangedEventArgs ZIndex_PropertyChangedEventArgs = new PropertyChangedEventArgs("ZIndex");

        /// <summary>
        /// the index of Z, render order
        /// </summary>
        public int ZIndex
        {
            get
            {
                return zIndex;
            }
            set
            {
                if (zIndex != value)
                {
                    zIndex = value;
                    OnPropertyChanged(ZIndex_PropertyChangedEventArgs);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        public GMapMarker(PointLatLng pos)
        {
            Position = pos;
        }

        internal GMapMarker()
        {
        }

        /// <summary>
        /// calls Dispose on shape if it implements IDisposable, sets shape to null and clears route
        /// </summary>
        public virtual void Clear()
        {
            var s = (Shape as IDisposable);
            if (s != null)
            {
                s.Dispose();
                s = null;
            }
            Shape = null;
        }

        /// <summary>
        /// updates marker position, internal access usualy
        /// </summary>
        void UpdateLocalPosition()
        {
            if (Map != null)
            {
                GPoint p = Map.FromLatLngToLocal(Position);
                p.Offset(-(long)Map.MapTranslateTransform.X, -(long)Map.MapTranslateTransform.Y);

                LocalPositionX = (int)(p.X + (long)(Offset.X));
                LocalPositionY = (int)(p.Y + (long)(Offset.Y));
            }
        }

        /// <summary>
        /// forces to update local marker  position
        /// dot not call it if you don't really need to ;}
        /// </summary>
        /// <param name="m"></param>
        internal void ForceUpdateLocalPosition(GMapControl m)
        {
            if (m != null)
            {
                map = m;
            }
            UpdateLocalPosition();
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region my codes

        public Callout MyCallout;
        public Path Path1;
        public Polyline mypolygon;
        public bool istagLineVisable = false;
        public List<PointLatLng> MTTrackPoints = new List<PointLatLng>();
        public enum GMapMarkers_ID
        {
            PolygonPath = 1,
            AirRoute=202,
            MT_Track=403,
            PolygonMarker = 604,
            TextMarker=805,
            CourseBeacon=1006,
            MovingTarget=1207,
            MT_Tag = 1408,
            Tag_Line = 1609,
            DisLine = 1810,
            DisMarker,
            DisTag
        }

        public enum GMapMarkers_ZIndex
        {
            PolygonPath = 1,
            AirRoute,
            MT_Track,
            PolygonMarker,
            TextMarker,
            CourseBeacon,
            MovingTarget,
            MT_Tag,
            Tag_Line,
            DisLine,
            DisMarker,
            DisTag
        }

        /// <summary>
        /// Marker ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 航点编号属性(依赖属性, 与航点编号Text绑定)
        /// </summary>
        public int WPNumber
        {
            get { return (int)GetValue(WPNumberProperty); }
            set { SetValue(WPNumberProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WPNumber.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WPNumberProperty =
            DependencyProperty.Register("WPNumber", typeof(int), typeof(GMapMarker), new PropertyMetadata(0));

        /// <summary>
        /// 航点标签属性(依赖属性, 与航点标签Text绑定)
        /// </summary>
        public string TagText
        {
            get { return (string)GetValue(TagTextProperty); }
            set { SetValue(TagTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TagText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TagTextProperty =
            DependencyProperty.Register("TagText", typeof(string), typeof(GMapMarker), new PropertyMetadata(""));

        /// <summary>
        /// 活动目标(飞机\坦克等)标签属性(依赖属性, 与目标标签Text绑定)
        /// </summary>
        public string TagetText
        {
            get { return (string)GetValue(TagetTextProperty); }
            set { SetValue(TagetTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TagText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TagetTextProperty =
            DependencyProperty.Register("TagetText", typeof(string), typeof(GMapMarker), new PropertyMetadata(""));

        /// <summary>
        /// 活动目标(飞机\坦克等)旋转方向属性(依赖属性, 与目标RotateTransform绑定)
        /// </summary>
        public float Bearing
        {
            get { return (float)GetValue(BearingProperty); }
            set { SetValue(BearingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bearing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BearingProperty =
            DependencyProperty.Register("Bearing", typeof(float), typeof(GMapMarker), new PropertyMetadata(0.0f));

        /// <summary>
        /// 地图测距文本显示
        /// </summary>
        public string DistanceValue
        {
            get { return (string)GetValue(DistanceValueProperty); }
            set { SetValue(DistanceValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DistanceValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DistanceValueProperty =
            DependencyProperty.Register("DistanceValue", typeof(string), typeof(GMapMarker), new PropertyMetadata(""));



        public bool IsTextFocusable
        {
            get { return (bool)GetValue(IsTextFocusableProperty); }
            set { SetValue(IsTextFocusableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTextFocusable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTextFocusableProperty =
            DependencyProperty.Register("IsTextFocusable", typeof(bool), typeof(GMapMarker), new PropertyMetadata(true));



        /// <summary>
        /// 更新活动目标(飞机\坦克等)属性
        /// </summary>
        /// <param name="targetMapMarker"></param>
        /// <param name="pos"></param>
        /// <param name="bearing"></param>
        public void UpdateTargetProperty(GMapMarker targetMapMarker, PointLatLng pos, float bearing)
        {
            targetMapMarker.Position = pos;
            targetMapMarker.Bearing = bearing >= 0.0f && bearing <= 360.0f ? bearing : 0.0f;
            targetMapMarker.TagetText = "航向：" + targetMapMarker.Bearing.ToString("0.00") + "\n纬度：" + Math.Abs(targetMapMarker.Position.Lat).ToString("0.000000") +
            (targetMapMarker.Position.Lat >= 0 ? " N" : " S") + "\n经度：" + Math.Abs(targetMapMarker.Position.Lng).ToString("0.000000") +
            (targetMapMarker.Position.Lng >= 0 ? " E" : " W") + "\n高度：" + "100" + " m" + "\n空速：" + "30" + " m/s";
        }
        #endregion
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}