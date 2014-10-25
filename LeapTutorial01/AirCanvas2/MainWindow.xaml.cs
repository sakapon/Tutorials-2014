using Leap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AirCanvas2
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        const int ScreenWidth = 1920;
        const int ScreenHeight = 1080;
        const int MappingScale = 3;

        Controller controller;
        Dictionary<int, Stroke> strokes = new Dictionary<int, Stroke>();

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            controller = new Controller();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            controller.Dispose();
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            var frame = controller.Frame();

            var positions = frame.Pointables
                .Where(p => p.IsValid)
                .Where(p => p.StabilizedTipPosition.IsValid())
                .Where(p => p.StabilizedTipPosition.z < 0)
                .ToDictionary(p => p.Id, p => ToStylusPoint(p.StabilizedTipPosition));

            var idsToRemove = strokes.Keys.Except(positions.Keys).ToArray();
            foreach (var id in idsToRemove)
            {
                strokes.Remove(id);
            }

            foreach (var item in positions)
            {
                if (strokes.ContainsKey(item.Key))
                {
                    strokes[item.Key].StylusPoints.Add(item.Value);
                }
                else
                {
                    var stroke = new Stroke(new StylusPointCollection(new[] { item.Value }));
                    strokes[item.Key] = stroke;

                    TheCanvas.Strokes.Add(stroke);
                }
            }
        }

        static StylusPoint ToStylusPoint(Leap.Vector v)
        {
            return new StylusPoint(ScreenWidth / 2 + MappingScale * v.x, ScreenHeight - MappingScale * v.y);
        }
    }
}
