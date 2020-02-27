using Rubinator3000.CubeScan.ColorIdentification;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using static Rubinator3000.XmlHandling.XmlDesignations;

namespace Rubinator3000.XmlHandling {

    static class XmlHandler {

        public static void SaveXDocument(string path, XDocument xDocument) {

            if (File.Exists(path)) {

                File.Delete(path);
            }
            xDocument.Save(path);
        }

        public static XDocument InitXDocument(string rootDesignation) {

            return new XDocument(new XElement(rootDesignation)) {
                Declaration = new XDeclaration("1.0", "UTF-8", null)
            };
        }

        public static XElement ReadPositionXElement(ReadPosition readPosition) {

            if (readPosition == null) {
                return null;
            }
            return new XElement(
                XReadPosition,
                new XAttribute(XFaceIndex, readPosition.FaceIndex),
                new XAttribute(XRowIndex, readPosition.RowIndex),
                new XAttribute(XColIndex, readPosition.ColIndex),
                new XAttribute(XCameraIndex, readPosition.CameraIndex),
                new XAttribute(XRelativeCenterX, readPosition.Contour.RelativeCenterX),
                new XAttribute(XRelativeCenterY, readPosition.Contour.RelativeCenterY)
            );
        }

        public static XAttribute[] ContourXElements(Contour contour) {

            return new XAttribute[2] {
                new XAttribute(nameof(contour.RelativeCenterX), contour.RelativeCenterX),
                new XAttribute(nameof(contour.RelativeCenterY), contour.RelativeCenterY)
            };
        }

        public static void SaveReadPositions(ReadPosition[,] readPositions) {

            XDocument xmlReadPositions = InitXDocument(XReadPositions);
            for (int i = 0; i < readPositions.GetLength(0); i++) {
                for (int j = 0; j < readPositions.GetLength(1); j++) {

                    xmlReadPositions.Root.Add(ReadPositionXElement(readPositions[i, j]));
                }
            }

            SaveXDocument(string.Format("{0}.xml", XReadPositions), xmlReadPositions);
        }
    }
}
