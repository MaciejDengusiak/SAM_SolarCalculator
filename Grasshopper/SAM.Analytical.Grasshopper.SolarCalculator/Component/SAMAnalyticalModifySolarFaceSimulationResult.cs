﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.SolarCalculator.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.SolarCalculator
{
    public class SAMAnalyticalModifySolarFaceSimulationResult : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("408b1bd3-a9a3-49de-9574-3eccedec2eb1");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_SolarCalculator;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalModifySolarFaceSimulationResult()
          : base("SAMAnalytical.ModifySolarFaceSimulationResult", "SAMAnalytical.ModifySolarFaceSimulationResult",
              "This node modify Shading data for Panel",
              "SAM WIP", "Solar")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "_panel", NickName = "_panel", Description = "SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Integer integer = null;

                integer = new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "_year", NickName = "_year", Description = "year", Access = GH_ParamAccess.item };
                integer.SetPersistentData(2018);
                result.Add(new GH_SAMParam(integer, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_hoursOfYear", NickName = "_hoursOfYear", Description = "Hours Of Year", Access = GH_ParamAccess.list };
                genericObject.SetPersistentData(Analytical.Query.DefaultHoursOfYear().ToArray());
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Vector vector = new global::Grasshopper.Kernel.Parameters.Param_Vector() { Name = "_sunVectors", NickName = "_sunVectors", Description = "Sun Vectors Directions", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(vector, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_angleTolerance_", NickName = "_angleTolerance_", Description = "Angle Tolerance", Access = GH_ParamAccess.item };
                number.SetPersistentData(Core.Tolerance.Angle);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Boolean boolean;

                boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_merge_", NickName = "_merge_", Description = "Merge SolarFace3DSimulationResults with the results existing in the given Analytical Model", Access = GH_ParamAccess.item };
                boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Binding));

                boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
                boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "analyticalModel", NickName = "analyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooResultParam() { Name = "solarFaceSimulationResults", NickName = "solarFaceSimulationResults", Description = "SAM Analytical Model", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "successful", NickName = "successful", Description = "Successful?", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            int index_Successful = Params.IndexOfOutputParam("successful");
            if (index_Successful != -1)
                dataAccess.SetData(index_Successful, false);

            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (!dataAccess.GetData(index, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!run)
                return;

            index = Params.IndexOfInputParam("_angleTolerance_");
            double tolerance_Angle = Core.Tolerance.Angle;
            if (index != -1)
            {
                double tolerance_Angle_Temp = tolerance_Angle;
                if(dataAccess.GetData(index, ref tolerance_Angle_Temp) && !double.IsNaN(tolerance_Angle_Temp))
                {
                    tolerance_Angle = tolerance_Angle_Temp;
                }
            }

            index = Params.IndexOfInputParam("_analyticalModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            index = Params.IndexOfInputParam("_hoursOfYear");
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null || objectWrappers.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_year");
            int year = -1;
            if (index == -1 || !dataAccess.GetData(index, ref year) || year == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<GH_Vector> vectors = new List<GH_Vector>();
            index = Params.IndexOfInputParam("_sunVectors"); 
            if (index == -1 || !dataAccess.GetDataList(index, vectors) || vectors == null || vectors.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<DateTime> dateTimes = new List<DateTime>();
            foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                object @object = (objectWrapper?.Value as dynamic).Value;
                if(@object == null)
                {
                    continue;
                }

                if(SAM.Core.Query.IsNumeric(@object))
                {
                    dateTimes.Add(new DateTime(year, 1, 1).AddHours(System.Convert.ToInt32(@object)));
                }
                else if (@object is DateTime)
                {
                    dateTimes.Add((DateTime)@object);
                }

                //hoursOfYear.ConvertAll(x => new DateTime(year, 1, 1).AddHours(x)
            }

            Dictionary<DateTime, Geometry.Spatial.Vector3D> directionDictionary = new Dictionary<DateTime, Geometry.Spatial.Vector3D>();
            for(int i=0; i < dateTimes.Count; i++)
            {
                DateTime dateTime = dateTimes[i];

                GH_Vector vector = null;
                if(vectors.Count > i)
                {
                    vector = vectors[i];
                }

                directionDictionary[dateTime] = Geometry.Grasshopper.Convert.ToSAM(vector);
            }


            Panel panel = null;
            index = Params.IndexOfInputParam("_panel");
            if (index == -1 || !dataAccess.GetData(index, ref panel) || panel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool merge = true;
            index = Params.IndexOfInputParam("_merge_");
            if(index != -1)
            {
                if(!dataAccess.GetData(index, ref merge))
                {
                    merge = true;
                }
            }

            analyticalModel = new AnalyticalModel(analyticalModel);
            List<Geometry.SolarCalculator.SolarFaceSimulationResult> solarFaceSimulationResults = Analytical.SolarCalculator.Modify.Simulate(analyticalModel, directionDictionary, merge, tolerance_Angle: tolerance_Angle);

            index = Params.IndexOfOutputParam("analyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }

            index = Params.IndexOfOutputParam("solarFaceSimulationResults");
            if (index != -1)
            {
                dataAccess.SetDataList(index, solarFaceSimulationResults?.ConvertAll(x => new GooResult(x)));
            }

            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, solarFaceSimulationResults != null);
            }
        }
    }
}