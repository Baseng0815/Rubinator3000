using System.Collections.Generic;

namespace Rubinator3000.CubeScan.CameraControl {
    class CameraDevicesUpdate {

        public List<int> Arrived { get; set; }

        public List<int> Disconnected { get; set; }

        public CameraDevicesUpdate(List<int> arrived, List<int> disconnected) {
            Arrived = arrived;
            Disconnected = disconnected;
        }
    }
}
