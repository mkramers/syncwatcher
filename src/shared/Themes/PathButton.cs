using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Themes
{
    public class PathButton : Button
    {
        public static DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(Geometry), typeof(PathButton), new FrameworkPropertyMetadata(Data_Changed));

        public static DependencyProperty FillProperty = DependencyProperty.Register("Fill", typeof(SolidColorBrush), typeof(PathButton), new FrameworkPropertyMetadata(Fill_Changed));

        //public Path Path
        //{
        //    get { return (Path)GetValue(PathProperty); }
        //    set { SetValue(PathProperty, value); }
        //}

        public Geometry Data
        {
            get => (Geometry) GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public SolidColorBrush Fill
        {
            get => (SolidColorBrush) GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        private void SetData()
        {
            RefreshPath();
        }

        private void SetFill()
        {
            RefreshPath();
        }

        private void RefreshPath()
        {
            Viewbox viewBox = new Viewbox
            {
                Width = Width,
                Height = Height,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Canvas canvas = new Canvas
            {
                Width = 24,
                Height = 24
            };

            Path path = new Path
            {
                Data = Data,
                Stroke = Fill,
                StrokeThickness = 1,
                Fill = Fill
            };

            viewBox.Child = canvas;
            canvas.Children.Add(path);

            Content = viewBox;
        }

        private static void Data_Changed(DependencyObject _o, DependencyPropertyChangedEventArgs _args)
        {
            PathButton thisClass = (PathButton) _o;
            thisClass.SetData();
        }

        private static void Fill_Changed(DependencyObject _o, DependencyPropertyChangedEventArgs _e)
        {
            PathButton thisClass = (PathButton) _o;
            thisClass.SetFill();
        }
    }
}
