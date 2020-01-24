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

        public static readonly int ArduinoTimeout = int.Parse(ConfigurationManager.AppSettings["arduinoTimeout"]);
        public static readonly int MoveAnimatedTime = int.Parse(ConfigurationManager.AppSettings["moveAnimationTime"]);

        public static bool UseMultiTurn = false;
        public static bool CalibrateColors = false;
        public static bool PositionEditingAllowed = false;
        public static bool UseReferenceColors = false;
    }
}
