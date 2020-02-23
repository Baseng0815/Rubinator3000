namespace Rubinator3000.CubeScan.CameraControl {
    public class Resolution {

        public int Width { get; set; }

        public int Height { get; set; }

        public Resolution(int width, int height) {
            Width = width;
            Height = height;
        }

        public int PixelCount() {

            return Width * Height;
        }

        public override string ToString() {
            return string.Format("{0}x{1}", Width, Height);
        }
    }
}
