
namespace GMap.NET.WindowsPresentation
{
    using System.Collections.Generic;
    using System.Linq;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        public GMapRoute(Markers_ID markers_ID, Markers_ZIndex markers_ZIndex, IEnumerable<PointLatLng> points, Color color, int strokeThickness, GMapControl map)
        {
            ZIndex = (int)markers_ZIndex;//(int)Markers_ZIndex.RouteMarker;
            ID = (int)markers_ID;//(int)Markers_ID.RouteMarker;
            Map = map;
            Points = new List<PointLatLng>(points);
            Shape = new Path() { Stroke = new SolidColorBrush(color), StrokeThickness = strokeThickness };
            Map.RemoveMarkerAtID(Map, ID);
            if (Points.Count() > 1) Map.Markers.Add(this);
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
