using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubinatorCore {
    struct FaceRelation {
        public CubeFace departure;
        public CubeFace destination;

        public (int departure, int destination)[] relation;

        public FaceRelation(CubeFace departure, CubeFace destination, params (int, int)[] relation) {
            this.departure = departure;
            this.destination = destination;
            this.relation = relation;
        }
    }
}
