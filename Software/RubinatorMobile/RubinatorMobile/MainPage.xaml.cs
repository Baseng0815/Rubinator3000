using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace RubinatorMobile {
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage {
        public int GLViewWidth => (int)glView.Width;
        public int GLViewHeight => (int)glView.Height;

        public MainPage() {
            InitializeComponent();

        }

        protected override void OnAppearing() {
            base.OnAppearing();
            
            var sharedCode = DependencyService.Get<IOpenGLViewSharedCodeService>();

            if (sharedCode != null) {
                glView.OnDisplay = sharedCode.OnDisplay;                    
                glView.HasRenderLoop = true;
            }

            glView.Display();
        }
        
    }    
}
