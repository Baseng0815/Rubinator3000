using DirectShowLib;

namespace Rubinator3000.CubeScan.CameraControl {
    class CameraDevice {

        public DsDevice DsDevice { get; set; }

        public Resolution Resolution { get; set; }

        public CameraDevice(DsDevice dsDevice, Resolution resolution) {

            DsDevice = dsDevice;
            Resolution = resolution;
        }

        public CameraDevice(DsDevice dsDevice, int width, int height) {
            DsDevice = dsDevice;
            Resolution = new Resolution(width, height);
        }
    }
}
