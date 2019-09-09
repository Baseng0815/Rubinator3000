using Rubinator3000.Solving;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Rubinator3000 {
    partial class MainWindow {
        // File


        private void MenuItemClose_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void MenuItemSuffle_Click(object sender, RoutedEventArgs e) {
            Random rnd = new Random();

            Cube.Shuffle(rnd.Next(5, 20));
        }

        private void MenuItemResetCube_Click(object sender, RoutedEventArgs e) {
            Cube = new Cube();
            CubeViewer.Window.Invalidate();
        }

        private async void MenuItemSolveCube_Click(object sender, RoutedEventArgs e) {
            CubeSolver solver = new CubeSolverFridrich(cube);

#if !DEBUG
            Task solvingTask = Task.Factory.StartNew(solver.CalcMoves);

            await solvingTask;

#else
            solver.CalcMoves();
#endif

            MoveCollection moves = solver.Moves;

            Task moveTask = Task.Factory.StartNew(() => {
                cube.DoMoves(moves);
            });

            await moveTask;
        }

        // Edit

        private void MenuItemUndo_Click(object sender, RoutedEventArgs e) {
            if(MoveHistory.Count > 0) {
                Move m = MoveHistory.Pop();

                undoMode = true;
                cube.DoMove(m.GetInverted());
            }

            undoMode = false;
        }

        private void MenuItemRedo_Click(object sender, RoutedEventArgs e) {
            if(undoneMoves.Count > 0) {
                Move m = undoneMoves.Pop();

                redoMode = true;
                cube.DoMove(m.GetInverted());
            }
            
            redoMode = false;
        }

        // View
        private void MenuItemChangeView_Click(object sender, RoutedEventArgs e) {
            if(sender is MenuItem menuItem){
                switch (menuItem.Tag as string) {
                    case "2D":
                        Renderer.DisplayMode = CubeDisplayMode.FLAT;
                        CubeViewer.Window.Invalidate();
                        CubeViewer.AttachInputEvents();
                        break;
                    case "3D":
                        Renderer.DisplayMode = CubeDisplayMode.CUBE;
                        CubeViewer.Window.Invalidate();
                        CubeViewer.DetachInputEvents();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}