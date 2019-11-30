using CubeLibrary;
using CubeLibrary.Solving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using System.Xml;
using System.Xml.Linq;

namespace Rubinator3000.DebugWindows {
    /// <summary>
    /// Interaction logic for OllDebugWindow.xaml
    /// </summary>
    public partial class OllDebugWindow : Window {
        public MainWindow mainWindow;

        private Cube cube => mainWindow.Cube;
        private (OllPattern pattern, MoveCollection algorithm) selectedPattern = LLSolver.OllPatterns[0];
        private (int r, int c)[] sides = {
                (3, 0), (2, 0), (1, 0),
                (0, 1), (0, 2), (0, 3),
                (1, 4), (2, 4), (3, 4),
                (4, 3), (4, 2), (4, 1)

            };

        public OllDebugWindow() {
            InitializeComponent();

            IEnumerable<int> ollPatterns = GetPatterns();

            listView.ItemsSource = ollPatterns;
        }

        private IEnumerable<int> GetPatterns() {
            for (int i = 0; i < LLSolver.OllPatterns.Length; i++) {
                yield return i;
            }
        }

        private async void listView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (listView.SelectedItem != null) {
                int number = (int)listView.SelectedItem;

                image.ClearValue(Image.SourceProperty);
                algorithmTextBox.Clear();

                if (number == 0)
                    return;

                try {
                    HttpWebRequest request = WebRequest.CreateHttp("http://www.speedcube.de/images/OLL" + number + ".png");
                    var responseTask = request.GetResponseAsync();

                    selectedPattern = LLSolver.OllPatterns[number];
                    algorithmTextBox.Text = selectedPattern.algorithm.ToString();
                    SetCanvas();
                    if (selectedPattern.pattern.IsMatch(cube)) {
                        textBoxPatternMatches.Text = "Pattern erkannt!";
                    }
                    else {
                        textBoxPatternMatches.Clear();
                    }

                    HttpWebResponse response = (HttpWebResponse)await responseTask;

                    if (response.ContentType == "image/png") {

                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = response.GetResponseStream();
                        bitmap.EndInit();

                        image.Source = bitmap;
                    }
                }
                catch (WebException exp) {
                    MessageBox.Show(exp.Message + "\r\n" + exp.StackTrace);
                }
            }
        }

        private void SetCanvas() {
            canvasGridPattern.Children.Clear();

            for (int i = 0; i < 9; i++) {
                Canvas c = new Canvas() {
                    Margin = new Thickness(0),
                    Background = selectedPattern.pattern.Face[i] ? Brushes.Yellow : Brushes.Transparent

                };

                Grid.SetRow(c, i / 3 + 1);
                Grid.SetColumn(c, i % 3 + 1);

                canvasGridPattern.Children.Add(c);
            }

            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 3; j++) {
                    Canvas c = new Canvas() {
                        Margin = new Thickness(0),
                        Background = selectedPattern.pattern.Sides[i][j] ? Brushes.Yellow : Brushes.Transparent
                    };

                    Grid.SetRow(c, sides[i * 3 + j].r);
                    Grid.SetColumn(c, sides[i * 3 + j].c);

                    canvasGridPattern.Children.Add(c);
                }
            }
        }

        private void DisplayCube() {
            canvasGridCube.Children.Clear();

            for (int i = 0; i < 9; i++) {
                Canvas c = new Canvas() {
                    Margin = new Thickness(0),
                    Background = cube.At(CubeFace.DOWN, i) == CubeColor.YELLOW ? Brushes.Yellow : Brushes.Transparent

                };

                Grid.SetRow(c, i / 3 + 1);
                Grid.SetColumn(c, i % 3 + 1);

                canvasGridCube.Children.Add(c);
            }

            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 3; j++) {
                    Canvas c = new Canvas() {
                        Margin = new Thickness(0),
                        Background = cube.At(CubeSolver.MiddleLayerFaces[i], j + 6) == CubeColor.YELLOW ? Brushes.Yellow : Brushes.Transparent
                    };

                    Grid.SetRow(c, sides[i * 3 + j].r);
                    Grid.SetColumn(c, sides[i * 3 + j].c);

                    canvasGridCube.Children.Add(c);
                }
            }
        }


        private void buttonDoMoves_Click(object sender, RoutedEventArgs e) {
            mainWindow.Cube.DoMoves(selectedPattern.algorithm);
            

            if (selectedPattern.pattern.IsMatch(cube)) {
                textBoxPatternMatches.Text = "Pattern erkannt!";
            }
            else {
                textBoxPatternMatches.Clear();
            }
        }

        private void buttonSavePattern_Click(object sender, RoutedEventArgs e) {
            
        }

        private void buttonCorrectPattern_Click(object sender, RoutedEventArgs e) {
            MoveCollection moves = selectedPattern.algorithm;
            OllPattern pattern = selectedPattern.pattern;

            Func<Cube, bool> downSolved = c => {
                for (int i = 0; i < 9; i++) {
                    if (c.At(CubeFace.DOWN, i) != CubeColor.YELLOW)
                        return false;
                }

                return true;
            };

            do {
                cube.DoMoves(moves);
                DisplayCube();

                if (!downSolved(cube)) {
                    bool[] face = new bool[9];
                    for (int i = 0; i < 9; i++) {
                        face[i] = cube.At(CubeFace.DOWN, i) == CubeColor.YELLOW;
                    }

                    bool[][] sides = new bool[4][];
                    for (int i = 0; i < 4; i++) {
                        sides[i] = new bool[3];
                        var cubeFace = CubeSolver.MiddleLayerFaces[i];
                        for (int j = 0; j < 3; j++) {
                            sides[i][j] = cube.At(cubeFace, j + 6) == CubeColor.YELLOW;
                        }
                    }

                    pattern = new OllPattern(pattern.Number, face, sides);
                }
            } while (!downSolved(cube));

            // save pattern
            XDocument doc = XDocument.Load(@"..\..\Resources\ollSolving.xml");
            int patternNumber = 0;

            if (doc.Root.Elements("ollPattern").Any(element => XmlConvert.ToInt32(element.Attribute("number").Value) == selectedPattern.pattern.Number)) {
                XElement element = doc.Root.Elements("ollPattern").First(el => XmlConvert.ToInt32(el.Attribute("number").Value) == selectedPattern.pattern.Number);
                patternNumber = XmlConvert.ToInt32(element.Attribute("number").Value);

                int face = 0;
                for (int i = 0; i < 9; i++) {
                    face += (pattern.Face[i] ? 1 : 0) << i;
                }

                int[] sides = new int[4];
                for (int i = 0; i < 4; i++) {
                    var cubeFace = CubeSolver.MiddleLayerFaces[i];
                    for (int j = 0; j < 3; j++) {
                        sides[i] += (pattern.Sides[i][j] ? 1 : 0) << j;
                    }
                }

                element.SetAttributeValue("face", face);
                element.SetAttributeValue("side0", sides[0]);
                element.SetAttributeValue("side1", sides[1]);
                element.SetAttributeValue("side2", sides[2]);
                element.SetAttributeValue("side3", sides[3]);
            }

            doc.Save(@"..\..\Resources\ollSolving.xml");
            doc.Save(@".\Resources\ollSolving.xml");

            // update the gui and pattern data
            LLSolver.LoadOllPatterns();

            selectedPattern = LLSolver.OllPatterns[patternNumber];
            SetCanvas();
            if (selectedPattern.pattern.IsMatch(cube)) {
                textBoxPatternMatches.Text = "Pattern erkannt!";
            }
            else {
                textBoxPatternMatches.Clear();
            }
        }
    }
}
