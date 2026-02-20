namespace MirrorSnap.Core.Tests.Models.TestModels
{
    public class NestedModel
    {
        public string? Name { get; set; }
        public PrimitiveModel? Inner { get; set; }
    }
}