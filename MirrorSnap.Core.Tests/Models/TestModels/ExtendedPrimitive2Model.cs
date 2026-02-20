namespace MirrorSnap.Core.Tests.Models.TestModels
{
    public class ExtendedPrimitive2Model
    {
        public Guid GuidValue { get; set; }
        public DateTime DateTimeValue { get; set; }
        public DateTimeOffset DateTimeOffsetValue { get; set; }
        public TimeSpan TimeSpanValue { get; set; }
        public DateOnly DateOnlyValue { get; set; }
        public TimeOnly TimeOnlyValue { get; set; }
        public ExampleEnum EnumValue { get; set; }
    }
}