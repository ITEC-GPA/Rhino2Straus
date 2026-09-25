namespace Rhino2Straus.Core.Base
{
    public class LoadCaseBase : ModelObjectId
    {
        public string Description { get; set; }

        public LoadCaseBase(string name, string description = "")
            : base(name)
        {
            Description = description;
        }

        public LoadCaseBase()
            : base()
        {
        }
    }
}
