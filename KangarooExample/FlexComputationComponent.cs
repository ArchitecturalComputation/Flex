using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;


namespace FlexComputation

{
    public class FlexComputationComponent : GH_Component
    {
        public FlexComputationComponent() : base("FlexComputation", "Flex", "This component produces a flat flexible object, starting from a complex surface.", "AC", "Studio 2") { }
        protected override System.Drawing.Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("fd8536c5-cd18-49db-8ecf-ffb5de01318d");

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "This is my input deformed mesh.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Edge length", "L", "Target edge length for remeshing.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "This is the output flat 3D object.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            double targetLength = 0;
            DA.GetData(0, ref mesh);
            DA.GetData(1, ref targetLength);
            if (targetLength <= 0.0)
                throw new ArgumentOutOfRangeException(" Target length must be greater than zero.");

            var flex = new FlexComputation(mesh, targetLength);
 
            DA.SetDataList(0, flex.OutGeometry);
        }
    }
}
