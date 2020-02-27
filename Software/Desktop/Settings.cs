﻿using System.Configuration;
using System.Windows.Media;
using static Rubinator3000.CubeScan.ReadUtility;

namespace Rubinator3000 {
    public static class Settings {

        // CubeView Settings
        public static readonly int CameraFov = int.Parse(ConfigurationManager.AppSettings["cameraFov"]);
        public static readonly int CameraDistance = int.Parse(ConfigurationManager.AppSettings["cameraDistance"]);
        public static readonly int ViewerDelay = int.Parse(ConfigurationManager.AppSettings["viewerDelay"]);
        public static readonly float MouseSensitivity = float.Parse(ConfigurationManager.AppSettings["mouseSensitivity"]);
        public static readonly float ScrollSensitivity = float.Parse(ConfigurationManager.AppSettings["scrollSensitivity"]);

        // Arduino
        public static readonly int ArduinoTimeout = int.Parse(ConfigurationManager.AppSettings["arduinoTimeout"]);
        public static readonly int MoveAnimatedTime = int.Parse(ConfigurationManager.AppSettings["moveAnimationTime"]);
        public static bool UseMultiTurn = true;
        public static int StepDelay = 300;

        // ReadPositionDialog
        public static int OffsetToTile = 30;

        // CubeScanner
        public static ReadoutRequested ReadoutRequested = ReadoutRequested.DISABLED;
        public static int TicksPerSecond = 10;
        public static int MaxWebCamControlCount = 4;

        // CameraPreview
        public static int CircleRadius = 3;
        public static Color HightlightColor = Color.FromArgb(255, 0, 255, 255);
        public static int HightlightThickness = 3;
        public static string HighlightName = "TileHighlight";
        public static int PositionRadius = 5;

        // ColorIdentification
        public static int CannyThresh = 120;
        public static int CannyThreshLinking = 20;
        public static int MinimalContourArea = 1500;
        public static int MinimalContourLength = 200;
        public static int MaximalContourLength = 350;
    }
}
