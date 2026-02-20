namespace MirrorSnap.Core.Tests.Models.TestModels
{
    public class DeepModel
    {
        public NestedModel? Level1 { get; set; }
        public PrimitiveModel? Extra { get; set; }
    }
}