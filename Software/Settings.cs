#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

namespace Rubinator3000
{
    class Settings
    {
        public static readonly int CameraFov = int.Parse(ConfigurationManager.AppSettings["cameraFov"]);
        public static readonly int CameraDistance = int.Parse(ConfigurationManager.AppSettings["cameraDistance"]);
        public static readonly int ViewerDelay = int.Parse(ConfigurationManager.AppSettings["viewerDelay"]);
        public static readonly float MouseSensitivity = float.Parse(ConfigurationManager.AppSettings["mouseSensitivity"]);
        public static readonly float ScrollSensitivity = float.Parse(ConfigurationManager.AppSettings["scrollSensitivity"]);

        public static int MoveAnimatedTime = int.Parse(ConfigurationManager.AppSettings["moveAnimationTime"]);
    }
}
