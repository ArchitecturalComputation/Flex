using System;
using System.Linq;
using System.Collections.Generic;
using Rhino.Geometry;
using Grasshopper;
using KangarooSolver;
using KangarooSolver.Goals;

namespace FlexComputation
{

    // Step 0. Create a HEXAGON MESH on the deformed inpout mesh. - Remesher/DanielPiker https://github.com/Dan-Piker/MeshMachine/blob/master/src/remesher.cs
    // Step 1. Make CURVATURE ANALYSIS for the inpout mesh, using the midpoint of each hexagon. Calculate the product of the principal curvatures at each point.
    // Step 2. Approximately UNROLL the input mesh to a flat mesh. - Unroller(Brep) or Unroller(Surface)
    // Step 3. REMAP the curvature analysis values. These numbers will be used for the thickness (0.5 to 1.5) of the outpout geometry and the offset of the inner hexagon mesh.
    // Step 4. Create an INNER HEXAGON MESH on the flat mesh. The offset distance is calculated according to the remapped values. - Remesher/DanielPiker https://github.com/Dan-Piker/MeshMachine/blob/master/src/remesher.cs
    // Step 5. Copy this mesh and MOVE the MESH POINTS upwards according to the remaped values from the curvature analysis. This will give different thickness along the geometry.
    // Step 6. Create a closed SOLID GEOMETRY suitable for 3d printing, using the hexagon meshes.

    class FlexComputation
    {
        public IEnumerable<GeometryBase> OutGeometry { get; set; }

        public FlexComputation(Mesh inputMesh, double targetLength)
        {
            var remesh = Remesher.ReMesh(inputMesh, targetLength);
            var duals = DualGraph(remesh);

            OutGeometry = duals.Select(d=>d.ToPolylineCurve());
            //OutGeometry = null;
        }

        List<Polyline> DualGraph(Mesh mesh)
        {
            mesh.TopologyVertices.SortEdges();
            var duals = new List<Polyline>();

            for (int i = 0; i < mesh.TopologyVertices.Count; i++)
            {
                var edges = mesh.TopologyVertices.ConnectedEdges(i);

                var dual = new Polyline();
                foreach (var edge in edges)
                {
                    int[] faces = mesh.TopologyEdges.GetConnectedFaces(edge, out bool[] orientation);
                    if (faces.Length == 1)
                    {
                        dual = null;
                        break;
                    }

                    var pair = mesh.TopologyEdges.GetTopologyVertices(edge);
                    int faceEdge = pair.I == i ? 0 : 1;
                    if (!orientation[0]) faceEdge = 1 - faceEdge;
                    int face = faces[faceEdge];
                    Point3d center = mesh.Faces.GetFaceCenter(face);
                    dual.Add(center);
                }

                if (dual != null)
                {
                    dual.Add(dual[0]);
                    duals.Add(dual);
                }
            }

            return duals;
        }
    }
}

