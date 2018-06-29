
namespace GMap.NET.WindowsPresentation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Windows.Shapes;
    /// <summary>
    /// 
    /// </summary>
    public interface IShapable
    {
        /// <summary>
        /// 
        /// </summary>
        List<PointLatLng> Points
        {
            get; set;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="addBlurEffect"></param>
        /// <returns></returns>
        Path CreatePath(List<System.Windows.Point> localPath, bool addBlurEffect);
    }
    /// <summary>
    /// /
    /// </summary>
    public class GMapRoute : GMapMarker, IShapable
    {
        //Path Path1;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        public GMapRoute(int markers_ID, int markers_ZIndex, IEnumerable<PointLatLng> points, Color color, int strokeThickness, GMapControl map)
        {
            ZIndex =markers_ZIndex;
            ID = markers_ID;
            Map = map;
            Points = new List<PointLatLng>(points);
            Path1 = new Path() { Stroke = new SolidColorBrush(color), StrokeThickness = strokeThickness };
            Shape = Path1;
            if (Points.Count() > 1) Map.Markers.Add(this);
        }

        public void IsVisable(int id, bool isVisable)
        {
            var clear = Map.Markers.Where(marker => marker.ID == id);
            if (clear.Count() > 0)
            {
                if(isVisable) clear.ElementAt(0).Path1.Visibility = Visibility.Visible;
                else clear.ElementAt(0).Path1.Visibility = Visibility.Hidden;
            }            
        }
        /// <summary>
        /// 
        /// </summary>
        public List<PointLatLng> Points
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            Points.Clear();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="addBlurEffect"></param>
        /// <returns></returns>
        public virtual Path CreatePath(List<System.Windows.Point> localPath, bool addBlurEffect)
        {
            // Create a StreamGeometry to use to specify myPath.
            StreamGeometry geometry = new StreamGeometry();

            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(localPath[0], false, false);

                // Draw a line to the next specified point.
                ctx.PolyLineTo(localPath, true, true);
            }

            // Freeze the geometry (make it unmodifiable)
            // for additional performance benefits.
            geometry.Freeze();

            // Create a path to draw a geometry with.
            Path myPath = new Path();
            {
                // Specify the shape of the Path using the StreamGeometry.
                myPath.Data = geometry;

                if (addBlurEffect)
                {
                    BlurEffect ef = new BlurEffect();
                    {
                        ef.KernelType = KernelType.Gaussian;
                        ef.Radius = 3.0;
                        ef.RenderingBias = RenderingBias.Performance;
                    }

                    myPath.Effect = ef;
                }

                myPath.Stroke = Brushes.Navy;
                myPath.StrokeThickness = 5;
                myPath.StrokeLineJoin = PenLineJoin.Round;
                myPath.StrokeStartLineCap = PenLineCap.Triangle;
                myPath.StrokeEndLineCap = PenLineCap.Square;

                myPath.Opacity = 0.6;
                myPath.IsHitTestVisible = false;
            }
            return myPath;
        }
    }
}
