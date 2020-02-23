using RubinatorCore;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.DataVisualization;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;

namespace Rubinator3000.CubeScan {

    class PieChart : Chart {

        private List<PieSlice> Slices { get; set; }

        private Dictionary<string, double> PieSource { get; set; }

        public PieSeries PieSeries { get; set; }

        public ResourceDictionaryCollection DataPointStyles { get; set; }

        public ReadPosition2 ReadPosition { get; set; }

        public PieChart(ReadPosition2 readPosition) {

            ReadPosition = readPosition;
            Slices = new List<PieSlice>();
            PieSource = new Dictionary<string, double>();

            PieSeries = new PieSeries {
                IndependentValueBinding = new Binding("Key"),
                DependentValueBinding = new Binding("Value")
            };
            Series.Add(PieSeries);
        }

        public void AddSlice(PieSlice slice) {

            Slices.Add(slice);
            Invalidate();
        }

        public void RemoveSlice(PieSlice slice) {

            Slices.Remove(slice);
            Invalidate();
        }

        public void Invalidate() {

            PieSeries.ItemsSource = null;

            UpdateValues();

            PieSource.Clear();

            PieSeries.Palette = new ResourceDictionaryCollection();

            for (int i = 0; i < Slices.Count; i++) {

                PieSource.Add(Slices[i].Name, Slices[i].Value);

                ResourceDictionary dataPointStyle = new ResourceDictionary();
                Style sliceStyle = new Style(typeof(PieDataPoint));
                sliceStyle.Setters.Add(new Setter(BackgroundProperty, Slices[i].ColorBrush));
                dataPointStyle.Add("DataPointStyle", sliceStyle);
                PieSeries.Palette.Add(dataPointStyle);
            }

            PieSeries.ItemsSource = PieSource;
        }

        private void UpdateValues() {

            Slices.Clear();

            for (int i = 0; i < 6; i++) {

                CubeColor cubeColor = (CubeColor)i;
                Slices.Add(new PieSlice(cubeColor.ToString(), ReadPosition.Percentages[i], ReadUtility.ColorBrush(cubeColor)));
            }
        }
    }
}
