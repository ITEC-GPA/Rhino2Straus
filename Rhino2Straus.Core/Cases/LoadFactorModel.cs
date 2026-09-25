namespace Rhino2Straus.Core.Cases
{
    public class LoadFactorModel
    {
        public LoadCaseModel LoadCase { get; set; }

        public double Factor { get; set; }

        public LoadFactorModel(LoadCaseModel loadCase, double factor)
        {
            LoadCase = loadCase;
            Factor = factor;
        }

        public LoadFactorModel()
        {
        }

        public LoadFactorModel(LoadFactorModel loadFactorModel)
        {
            LoadCase = new LoadCaseModel( loadFactorModel.LoadCase);
            Factor = loadFactorModel.Factor;
        }
    }
}
