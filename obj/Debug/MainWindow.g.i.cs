﻿#pragma checksum "..\..\MainWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "1E0EB416AADF768CE352E8BEA1B4BB4BC0E312393B9667880B49A8347B84CFAB"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
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
        
        
        #line 112 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Forms.Integration.WindowsFormsHost winFormsHost;
        
        #line default
        #line hidden
        
        
        #line 117 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox textBoxLog;
        
        #line default
        #line hidden
        
        
        #line 153 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox allowPosAdd;
        
        #line default
        #line hidden
        
        
        #line 175 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image cameraPreview0;
        
        #line default
        #line hidden
        
        
        #line 185 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas cameraCanvas0;
        
        #line default
        #line hidden
        
        
        #line 192 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image cameraPreview1;
        
        #line default
        #line hidden
        
        
        #line 200 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas cameraCanvas1;
        
        #line default
        #line hidden
        
        
        #line 207 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image cameraPreview2;
        
        #line default
        #line hidden
        
        
        #line 215 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas cameraCanvas2;
        
        #line default
        #line hidden
        
        
        #line 222 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image cameraPreview3;
        
        #line default
        #line hidden
        
        
        #line 230 "..\..\MainWindow.xaml"
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
            
            #line 11 "..\..\MainWindow.xaml"
            ((Rubinator3000.MainWindow)(target)).SizeChanged += new System.Windows.SizeChangedEventHandler(this.Window_SizeChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 83 "..\..\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItemResetCube_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 84 "..\..\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItemSuffle_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 85 "..\..\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItemSolveCube_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 89 "..\..\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItemClose_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 93 "..\..\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItemChangeView_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 97 "..\..\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItemChangeView_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.winFormsHost = ((System.Windows.Forms.Integration.WindowsFormsHost)(target));
            
            #line 112 "..\..\MainWindow.xaml"
            this.winFormsHost.Initialized += new System.EventHandler(this.WinFormsHost_Initialized);
            
            #line default
            #line hidden
            return;
            case 9:
            this.textBoxLog = ((System.Windows.Controls.TextBox)(target));
            return;
            case 10:
            this.allowPosAdd = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 11:
            this.cameraPreview0 = ((System.Windows.Controls.Image)(target));
            
            #line 175 "..\..\MainWindow.xaml"
            this.cameraPreview0.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.CameraPreview_MouseDown);
            
            #line default
            #line hidden
            return;
            case 12:
            this.cameraCanvas0 = ((System.Windows.Controls.Canvas)(target));
            return;
            case 13:
            this.cameraPreview1 = ((System.Windows.Controls.Image)(target));
            
            #line 192 "..\..\MainWindow.xaml"
            this.cameraPreview1.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.CameraPreview_MouseDown);
            
            #line default
            #line hidden
            return;
            case 14:
            this.cameraCanvas1 = ((System.Windows.Controls.Canvas)(target));
            return;
            case 15:
            this.cameraPreview2 = ((System.Windows.Controls.Image)(target));
            
            #line 207 "..\..\MainWindow.xaml"
            this.cameraPreview2.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.CameraPreview_MouseDown);
            
            #line default
            #line hidden
            return;
            case 16:
            this.cameraCanvas2 = ((System.Windows.Controls.Canvas)(target));
            return;
            case 17:
            this.cameraPreview3 = ((System.Windows.Controls.Image)(target));
            
            #line 222 "..\..\MainWindow.xaml"
            this.cameraPreview3.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.CameraPreview_MouseDown);
            
            #line default
            #line hidden
            return;
            case 18:
            this.cameraCanvas3 = ((System.Windows.Controls.Canvas)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
