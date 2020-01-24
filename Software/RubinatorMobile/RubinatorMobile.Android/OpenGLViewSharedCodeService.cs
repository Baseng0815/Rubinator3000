using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using RubinatorMobile;
using Xamarin.Forms;

using OpenTK.Graphics.ES30;


[assembly: Dependency(typeof(RubinatorMobile.Droid.OpenGLViewSharedCodeService))]
namespace RubinatorMobile.Droid {
    class OpenGLViewSharedCodeService : IOpenGLViewSharedCodeService {        

        public void Init() {
            
        }

        public void OnDisplay(Rectangle r) {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);            

            DrawCube.Draw(CubeViewer.View);                                  
        }
    }
}