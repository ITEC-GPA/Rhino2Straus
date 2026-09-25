using Rhino2Straus.Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino2Straus.Grasshopper.Datatype;
using System;
using System.Collections.Generic;
using System.IO;

namespace Rhino2Straus.Grasshopper.Components.Export
{
    public class StrausR3ExportComponent : SafeComponent
    {
        protected string _status;
        protected bool _run;
        private bool previousRun;
        private bool succeeded;
        private string savedPath="";
        private List<string> exportWarnings = new(), exportErrors = new();

        /// <summary>
        /// Initializes a new instance of the StrausExportComponent class.
        /// </summary>
        public StrausR3ExportComponent()
          : base("Straus7 R3 Export", "ST7 R3", "Export to Straus7", Rhino2Straus.Core.Helper.Constants.CATEGORY_RHINO2STRAUS, Rhino2Straus.Core.Helper.Constants.SUBCATEGORY_EXPORT)
        {
            _status = "";
            _run = false;
        }

        public override void CreateAttributes()
        {
            GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes attr = new GrasshopperHelper.ComponentAttributes.ComponentOneButtonAttributes(this, "Save");
            attr.ButtonPressed += () =>
            {
                _run = true;
                ExpireSolution(true);
            };
            m_attributes = attr;
        }


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "The FEM model", GH_ParamAccess.item);
            pManager.AddTextParameter("Output Path", "O", "The full path of the file model to create", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Run", "Run", "Use a button or a false-to-true transition", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Overwrite", "Overwrite", "Allow replacement of an existing .st7 file", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Status", "S", "The status of the last export", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Success", "OK", "True only after a completed export", GH_ParamAccess.item);
            pManager.AddTextParameter("Warnings", "W", "Export warnings", GH_ParamAccess.list);
            pManager.AddTextParameter("Errors", "E", "Export errors", GH_ParamAccess.list);
            pManager.AddTextParameter("Model path","Path","Path of the last successful export",GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Solve(IGH_DataAccess DA)
        {
            bool inputRun = Item<bool>(DA, 2);
            bool fire = _run || (inputRun && !previousRun);
            previousRun = inputRun; _run = false;
            if (fire)
            {
                succeeded = false; savedPath=""; _status = "Failed";
                var model = Item<GH_Model>(DA, 0);
                string path = Item<string>(DA, 1);
                succeeded = model.Value.CreateModelStraus7R3(path, out _, out exportWarnings, out exportErrors, Item<bool>(DA, 3));
                savedPath=succeeded?Path.GetFullPath(path):"";
                _status = succeeded ? "Saved: " + Path.GetFullPath(path) : "Export failed";
            }
            foreach (var warning in exportWarnings) AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
            foreach (var error in exportErrors) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error);
            Message = succeeded ? "Saved" : (string.IsNullOrEmpty(_status) ? "Idle" : "Failed");
            DA.SetData(0, string.IsNullOrEmpty(_status) ? "Idle" : _status); DA.SetData(1, succeeded); DA.SetDataList(2, exportWarnings); DA.SetDataList(3, exportErrors);DA.SetData(4,savedPath);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("11ccef5e-40d7-4b18-b768-81111781b452");
    }
}
