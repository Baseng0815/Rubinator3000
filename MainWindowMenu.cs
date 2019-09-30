﻿using Rubinator3000.Solving;
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

            MoveCollection moves = solver.GetMoves();

            Task moveTask = Task.Factory.StartNew(() => {
                cube.DoMoves(moves);
            });

            await moveTask;
        }        

        // View
        private void MenuItemChangeView_Click(object sender, RoutedEventArgs e) {
            if(sender is MenuItem menuItem){
                switch (menuItem.Tag as string) {
                    case "2D":
                        Renderer.DisplayMode = CubeDisplayMode.FLAT;
                        CubeViewer.Window.Invalidate();
                        CubeViewer.DetachInputEvents();
                        break;
                    case "3D":
                        Renderer.DisplayMode = CubeDisplayMode.CUBE;
                        CubeViewer.Window.Invalidate();
                        CubeViewer.AttachInputEvents();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}