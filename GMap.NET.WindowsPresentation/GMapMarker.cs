
namespace GMap.NET.WindowsPresentation
{
    using GMap.NET;
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Media;

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
        public enum Markers_ID
        {
            RouteMarker = 1,
            PolygonMarker,
            WaypointMarker,
            TargetMarker,
            TagMarker
        }
        public enum Markers_ZIndex
        {
            RouteMarker = 1,
            PolygonMarker,
            WaypointMarker,
            TargetMarker,
            TagMarker
        }
        public int ID { get; set; }
        //private int wpNumber;

        //public int WPNumber
        //{
        //    get { return wpNumber; }
        //    set
        //    {
        //        wpNumber = value;
        //        if (this.PropertyChanged != null)
        //        {
        //            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("WPNumber"));
        //        }
        //    }
        //}

        public int WPNumber
        {
            get { return (int)GetValue(WPNumberProperty); }
            set { SetValue(WPNumberProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WPNumber.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WPNumberProperty =
            DependencyProperty.Register("WPNumber", typeof(int), typeof(GMapMarker), new PropertyMetadata(0));

        //private string tagText;
        //public string TagText
        //{
        //    get { return tagText; }
        //    set
        //    {
        //        tagText = value;
        //        if (this.PropertyChanged != null)
        //        {
        //            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("TagText"));
        //        }
        //    }
        //}


        public string TagText
        {
            get { return (string)GetValue(TagTextProperty); }
            set { SetValue(TagTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TagText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TagTextProperty =
            DependencyProperty.Register("TagText", typeof(string), typeof(GMapMarker), new PropertyMetadata(""));
        public string TagetText
        {
            get { return (string)GetValue(TagetTextProperty); }
            set { SetValue(TagetTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TagText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TagetTextProperty =
            DependencyProperty.Register("TagetText", typeof(string), typeof(GMapMarker), new PropertyMetadata(""));

        public double Bearing
        {
            get { return (double)GetValue(BearingProperty); }
            set { SetValue(BearingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bearing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BearingProperty =
            DependencyProperty.Register("Bearing", typeof(double), typeof(GMapMarker), new PropertyMetadata(0.0));


        #endregion
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}