using Rubinator3000.CubeScan.ColorIdentification;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Rubinator3000.XmlHandling {

    static class XmlHandler {

        public void SaveXDocument(string path, XDocument xDocument) {

            if (File.Exists(path)) {

                File.Delete(path);
            }
            xDocument.Save(path);
        }

        public static void SaveReadPositions(List<ReadPosition> readPositions) {


        }
    }
}
