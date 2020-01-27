using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

namespace RubinatorTabletView
{
    class Settings
    {
        public static readonly int CameraFov = 90;
        public static readonly int CameraDistance = 10;
        //public static readonly int ViewerDelay = int.Parse(ConfigurationManager.AppSettings["viewerDelay"]);
        //public static readonly float MouseSensitivity = float.Parse(ConfigurationManager.AppSettings["mouseSensitivity"]);
        //public static readonly float ScrollSensitivity = float.Parse(ConfigurationManager.AppSettings["scrollSensitivity"]);

        public static readonly int MoveAnimatedTime = 500;

        public static bool UseMultiTurn = false;
        public static bool CalibrateColors = false;
        public static bool PositionEditingAllowed = false;
        public static bool UseReferenceColors = false;
    }
}
