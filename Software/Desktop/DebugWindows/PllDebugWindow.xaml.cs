using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Rubinator3000.DebugWindows {
    using Rubinator3000;
    using Rubinator3000.Solving;

    /// <summary>
    /// Interaction logic for PllDebugWindow.xaml
    /// </summary>
    public partial class PllDebugWindow : Window {
        private Cube cube;
        private (PllPattern pattern, MoveCollection algorithm)[] PllPatterns => LLSolver.PllPatterns;
        private static readonly SolidColorBrush[] colors = { Brushes.Orange, Brushes.Green, Brushes.Red, Brushes.Blue };
        private static readonly CubeColor[] middleLayerColors = { CubeColor.ORANGE, CubeColor.GREEN, CubeColor.RED, CubeColor.BLUE };

        public PllDebugWindow() {
            InitializeComponent();

            cube = new Cube();

            listBoxAlgorithms.ItemsSource = from pattern in PllPatterns
                                            select new ListBoxItem() {
                                                Content = "Pll " + pattern.pattern.Number,
                                                Tag = pattern.pattern.Number
                                            };

            for (int i = 0; i < 12; i++) {
                gridDisplayPattern.ColumnDefinitions.Add(new ColumnDefinition() {
                    Width = new GridLength(1, GridUnitType.Star)
                });

                gridDisplayCube.ColumnDefinitions.Add(new ColumnDefinition() {
                    Width = new GridLength(1, GridUnitType.Star)
                });
            }

            listBoxOffset.ItemsSource = new int[] { 0, 1, 2, 3 };
        }

        private void ButtonDoMoves_Click(object sender, RoutedEventArgs e) {
            if (listBoxAlgorithms.SelectedItem != null) {
                var moves = PllPatterns[listBoxAlgorithms.SelectedIndex].algorithm;

                cube.DoMoves(moves);
                DisplayCube();
            }
        }

        private void ListBoxAlgorithms_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var pattern = PllPatterns[listBoxAlgorithms.SelectedIndex];

            textBoxMoves.Clear();
            textBoxMoves.Text = pattern.algorithm.ToString();

            DisplayPattern(pattern.pattern);
        }

        private void DisplayPattern(PllPattern pattern) {            
            gridDisplayPattern.Children.Clear();

            for(int f = 0; f < 4; f++) {
                int delta1 = pattern.PatternData[f][0], delta2 = pattern.PatternData[f][1], delta3 = pattern.PatternData[f][2];


            }
        }

        private void DisplayCube() {
            gridDisplayCube.Children.Clear();

            for(int f = 0; f < 4; f++) {
                CubeFace face = CubeSolver.MiddleLayerFaces[f];

                for(int t = 6; t < 9; t++) {
                    int cubeColor = Array.IndexOf(middleLayerColors, cube.At(face, t));

                    Canvas c = new Canvas() {
                        Background = colors[cubeColor],
                        Margin = new Thickness(0)
                    };

                    Grid.SetColumn(c, f * 3 + t - 6);

                    gridDisplayCube.Children.Add(c);
                }
            }
        }
    }
}
