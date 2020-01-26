using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RubinatorMobile.pages {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewPage : ContentPage {
        private IOpenGLViewSharedCodeService openGLCode;

        public ViewPage() {
            InitializeComponent();

            openGLCode = DependencyService.Get<IOpenGLViewSharedCodeService>();
            openGLCode.Init();            
        }

        protected override void OnAppearing() {
            base.OnAppearing();

            glView.OnDisplay = openGLCode.OnDisplay;
            glView.HasRenderLoop = true;
        }
    }
}