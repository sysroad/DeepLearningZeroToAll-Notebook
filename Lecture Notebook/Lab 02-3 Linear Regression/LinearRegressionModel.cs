using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using System.Linq;
using System.Windows.Media;
using System.Collections.ObjectModel;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Data.Model;
using Tensorflow;
using static Tensorflow.Binding;

namespace Lab_02_3_Linear_Regression
{
    public class LinearRegressionModel : BindableObject
    {
        ObservableCollection<IRenderableSeriesViewModel> renderSeries;
        public ObservableCollection<IRenderableSeriesViewModel> RenderSeries
        {
            get { return renderSeries; }
            set
            {
                renderSeries = value;
                OnPropertyChanged();
            }
        }

        XyDataSeries<double, double> renderData;

        Dispatcher dispatcher;

        public LinearRegressionModel()
        {
            renderSeries = new ObservableCollection<IRenderableSeriesViewModel>();

            renderData = new XyDataSeries<double, double>();
            RenderSeries.Add(new XyScatterRenderableSeriesViewModel
            {
                DataSeries = renderData,
                Stroke = Colors.Yellow,
                StrokeThickness = 2,
                PointMarker = new SciChart.Charting.Visuals.PointMarkers.EllipsePointMarker(),
            });

            dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void Run()
        {
            dispatcher.Invoke(() =>
            {
                var X = tf.constant(new[] { 1.0, 2.0, 3.0 }, dtype: tf.float32);
                var Y = tf.constant(new[] { 1.0, 2.0, 3.0 }, dtype: tf.float32);

                using (renderData.SuspendUpdates())
                {
                    for (int i = -30; i < 50; ++i)
                    {
                        var cost = tf.reduce_mean(
                            tf.square(tf.subtract(tf.multiply(X, i * 0.1), Y))
                        );

                        renderData.Append(i * 0.1, (double)cost.numpy());
                    }
                    
                    
                    renderData.ParentSurface.ZoomExtents();
                }
            });
        }

        new void OnPropertyChanged([CallerMemberName] string property_name = "")
        {
            base.OnPropertyChanged(property_name);
        }
    }
}
