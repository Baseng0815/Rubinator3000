﻿#pragma checksum "..\..\MainWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "D84FB13BF77F296AE9AE75C43A58BCD1AA59A131B8B0DD53C4C65D757F657921"
//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

using Rubinator3000;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Rubinator3000 {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 120 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Forms.Integration.WindowsFormsHost winFormsHost;
        
        #line default
        #line hidden
        
        
        #line 125 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox textBoxLog;
        
        #line default
        #line hidden
        
        
        #line 162 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox allowPosEdit;
        
        #line default
        #line hidden
        
        
        #line 186 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image cameraPreview0;
        
        #line default
        #line hidden
        
        
        #line 196 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas cameraCanvas0;
        
        #line default
        #line hidden
        
        
        #line 203 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image cameraPreview1;
        
        #line default
        #line hidden
        
        
        #line 211 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas cameraCanvas1;
        
        #line default
        #line hidden
        
        
        #line 218 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image cameraPreview2;
        
        #line default
        #line hidden
        
        
        #line 226 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas cameraCanvas2;
        
        #line default
        #line hidden
        
        
        #line 233 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image cameraPreview3;
        
        #line default
        #line hidden
        
        
        #line 241 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas cameraCanvas3;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Rubinator3000;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 10 "..\..\MainWindow.xaml"
            ((Rubinator3000.MainWindow)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.Window_Closing);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 44 "..\..\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.CameraPreviewMenuItem_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 90 "..\..\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItemResetCube_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 91 "..\..\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItemSuffle_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 92 "..\..\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItemSolveCube_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 96 "..\..\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItemClose_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 101 "..\..\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItemChangeView_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 105 "..\..\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItemChangeView_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.winFormsHost = ((System.Windows.Forms.Integration.WindowsFormsHost)(target));
            
            #line 120 "..\..\MainWindow.xaml"
            this.winFormsHost.Initialized += new System.EventHandler(this.WinFormsHost_Initialized);
            
            #line default
            #line hidden
            return;
            case 10:
            this.textBoxLog = ((System.Windows.Controls.TextBox)(target));
            return;
            case 11:
            this.allowPosEdit = ((System.Windows.Controls.CheckBox)(target));
            
            #line 163 "..\..\MainWindow.xaml"
            this.allowPosEdit.Click += new System.Windows.RoutedEventHandler(this.AllowPosEdit_Click);
            
            #line default
            #line hidden
            return;
            case 12:
            this.cameraPreview0 = ((System.Windows.Controls.Image)(target));
            
            #line 186 "..\..\MainWindow.xaml"
            this.cameraPreview0.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.CameraPreview_MouseDown);
            
            #line default
            #line hidden
            return;
            case 13:
            this.cameraCanvas0 = ((System.Windows.Controls.Canvas)(target));
            return;
            case 14:
            this.cameraPreview1 = ((System.Windows.Controls.Image)(target));
            
            #line 203 "..\..\MainWindow.xaml"
            this.cameraPreview1.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.CameraPreview_MouseDown);
            
            #line default
            #line hidden
            return;
            case 15:
            this.cameraCanvas1 = ((System.Windows.Controls.Canvas)(target));
            return;
            case 16:
            this.cameraPreview2 = ((System.Windows.Controls.Image)(target));
            
            #line 218 "..\..\MainWindow.xaml"
            this.cameraPreview2.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.CameraPreview_MouseDown);
            
            #line default
            #line hidden
            return;
            case 17:
            this.cameraCanvas2 = ((System.Windows.Controls.Canvas)(target));
            return;
            case 18:
            this.cameraPreview3 = ((System.Windows.Controls.Image)(target));
            
            #line 233 "..\..\MainWindow.xaml"
            this.cameraPreview3.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.CameraPreview_MouseDown);
            
            #line default
            #line hidden
            return;
            case 19:
            this.cameraCanvas3 = ((System.Windows.Controls.Canvas)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

