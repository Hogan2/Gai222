using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GMap.NET.WindowsPresentation
{
    /// <summary>
    /// Circle.xaml 的交互逻辑
    /// </summary>
    public partial class Circle : UserControl
    {
        /// <summary>
        /// 
        /// </summary>
        public Circle(GMapMarker gMapMarker)
        {
            InitializeComponent();
            //<!--航点：&#x0a;纬度：&#x0a;经度：&#x0a;高度：&#x0a;空速：-->
            //wayPointIndex.SetBinding(TextBlock.TextProperty, new Binding("WPNumber") { Source = wayPointMarker });
            //TagContent.SetBinding(TextBlock.TextProperty, new Binding("TagText") { Source = wayPointMarker });
        }
    }
}
