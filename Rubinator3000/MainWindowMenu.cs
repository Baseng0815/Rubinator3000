using System;
using System.Windows;
using System.Windows.Controls;

namespace Rubinator3000 {
    partial class MainWindow {
        private void MenuItemChangeView_Click(object sender, RoutedEventArgs e) {
            if(sender is MenuItem menuItem){
                switch (menuItem.Tag as string) {
                    case "2D":
                        _3dView = false;
                        break;
                    case "3D":
                        _3dView = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private void MenuItemClose_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }
    }
}