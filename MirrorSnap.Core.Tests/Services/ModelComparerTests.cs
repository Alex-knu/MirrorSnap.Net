using MirrorSnap.Core.Models;
using MirrorSnap.Core.Services;
using MirrorSnap.Core.Tests.Models.TestModels;

namespace MirrorSnap.Core.Tests.Services
{
    public class ModelComparerTests
    {
        private readonly ModelComparer _comparer = new();

        [Fact]
        public void Compare_PrimitivesEqual_NoErrors()
        {
            PrimitiveModel actual = new()
            {
                IntValue = 1,
                StringValue = "foo",
                BoolValue = true,
                DecimalValue = 3.14m
            };

            PrimitiveModel expected = new()
            {
                IntValue = 1,
                StringValue = "foo",
                BoolValue = true,
                DecimalValue = 3.14m
            };

            var errors = _comparer.CompareModels(actual, expected, new SnapSettings { IgnoreProperties = Enumerable.Empty<string>() });
            Assert.Empty(errors);
        }

        [Fact]
        public void Compare_PrimitivesDifferent_ReturnsErrors()
        {
            PrimitiveModel actual = new()
            {
                IntValue = 1,
                StringValue = "foo",
                BoolValue = true,
                DecimalValue = 3.14m
            };
            PrimitiveModel expected = new()
            {
                IntValue = 2,
                StringValue = "bar",
                BoolValue = false,
                DecimalValue = 1.23m
            };

            List<ErrorMessage> errors = _comparer.CompareModels(actual, expected, new SnapSettings { IgnoreProperties = Enumerable.Empty<string>() }).ToList();

            Assert.Equal(4, errors.Count);
            Assert.Contains(errors, e => e.Message.Contains("IntValue"));
            Assert.Contains(errors, e => e.Message.Contains("StringValue"));
            Assert.Contains(errors, e => e.Message.Contains("BoolValue"));
            Assert.Contains(errors, e => e.Message.Contains("DecimalValue"));
        }

        [Fact]
        public void Compare_NestedObjectsEqual_NoErrors()
        {
            NestedModel actual = new()
            {
                Name = "outer",
                Inner = new PrimitiveModel
                {
                    IntValue = 5,
                    StringValue = "bar",
                    BoolValue = false,
                    DecimalValue = 0.1m
                }
            };
            NestedModel expected = new()
            {
                Name = "outer",
                Inner = new PrimitiveModel
                {
                    IntValue = 5,
                    StringValue = "bar",
                    BoolValue = false,
                    DecimalValue = 0.1m
                }
            };

            var errors = _comparer.CompareModels(actual, expected, new SnapSettings { IgnoreProperties = Enumerable.Empty<string>() });
            Assert.Empty(errors);
        }

        [Fact]
        public void Compare_NestedObjectsDifferent_ReturnsErrorWithPath()
        {
            NestedModel actual = new()
            {
                Name = "outer",
                Inner = new PrimitiveModel
                {
                    IntValue = 5,
                    StringValue = "foo",
                    BoolValue = true,
                    DecimalValue = 9.9m
                }
            };
            NestedModel expected = new()
            {
                Name = "outer",
                Inner = new PrimitiveModel
                {
                    IntValue = 5,
                    StringValue = "bar",
                    BoolValue = true,
                    DecimalValue = 9.9m
                }
            };

            List<ErrorMessage> errors = _comparer.CompareModels(actual, expected, new SnapSettings { IgnoreProperties = Enumerable.Empty<string>() }).ToList();
            Assert.Single(errors);
            Assert.Contains(".Inner.StringValue", errors[0].Message);
        }

        [Fact]
        public void Compare_IgnorePattern_IgnoresMatchingProperty()
        {
            NestedModel actual = new()
            {
                Name = "outer",
                Inner = new PrimitiveModel
                {
                    IntValue = 1,
                    StringValue = "foo",
                    BoolValue = true,
                    DecimalValue = 1.0m
                }
            };
            NestedModel expected = new()
            {
                Name = "outer",
                Inner = new PrimitiveModel
                {
                    IntValue = 2,
                    StringValue = "bar",
                    BoolValue = false,
                    DecimalValue = 2.0m
                }
            };

            // ignore everything under Inner.StringValue and IntValue using regex
            SnapSettings settings = new()
            {
                IgnoreProperties = new[] { @"\.Inner\.StringValue", @"\.Inner\.IntValue" }
            };

            List<ErrorMessage> errors = _comparer.CompareModels(actual, expected, settings).ToList();
            // only BoolValue and DecimalValue should be reported
            Assert.Equal(2, errors.Count);
            Assert.DoesNotContain(errors, e => e.Message.Contains("StringValue"));
            Assert.DoesNotContain(errors, e => e.Message.Contains("IntValue"));
        }

        [Fact]
        public void Compare_DeepNestedObjects_ReturnsDeepPathError()
        {
            DeepModel actual = new()
            {
                Level1 = new NestedModel
                {
                    Name = "level1",
                    Inner = new PrimitiveModel
                    {
                        IntValue = 1,
                        StringValue = "foo",
                        BoolValue = true,
                        DecimalValue = 0m
                    }
                },
                Extra = new PrimitiveModel
                {
                    IntValue = 9,
                    StringValue = "extra",
                    BoolValue = false,
                    DecimalValue = 5m
                }
            };
            DeepModel expected = new()
            {
                Level1 = new NestedModel
                {
                    Name = "level1",
                    Inner = new PrimitiveModel
                    {
                        IntValue = 1,
                        StringValue = "bar",
                        BoolValue = true,
                        DecimalValue = 0m
                    }
                },
                Extra = new PrimitiveModel
                {
                    IntValue = 9,
                    StringValue = "extra",
                    BoolValue = false,
                    DecimalValue = 5m
                }
            };

            List<ErrorMessage> errors = _comparer.CompareModels(actual, expected, new SnapSettings { IgnoreProperties = Enumerable.Empty<string>() }).ToList();
            Assert.Single(errors);
            Assert.Contains(".Level1.Inner.StringValue", errors[0].Message);
        }

        [Fact]
        public void Compare_TypeMismatch_Throws()
        {
            // actual and expected types differ
            object a = new PrimitiveModel();
            object b = new NestedModel();
            Assert.Throws<Exception>(() => _comparer.CompareModels(a, b, new SnapSettings()));
        }

        [Fact]
        public void Compare_ExtendedPrimitivesEqual_NoErrors()
        {
            ExtendedPrimitiveModel actual = new()
            {
                ByteValue = 1,
                SByteValue = -1,
                CharValue = 'z',
                DoubleValue = 1.23,
                FloatValue = 4.56f,
                UIntValue = 123u,
                NIntValue = -42,
                NUIntValue = 42u,
                LongValue = -1000L,
                ULongValue = 1000UL,
                ShortValue = -10,
                UShortValue = 10
            };

            ExtendedPrimitiveModel expected = new()
            {
                ByteValue = 1,
                SByteValue = -1,
                CharValue = 'z',
                DoubleValue = 1.23,
                FloatValue = 4.56f,
                UIntValue = 123u,
                NIntValue = -42,
                NUIntValue = 42u,
                LongValue = -1000L,
                ULongValue = 1000UL,
                ShortValue = -10,
                UShortValue = 10
            };

            var errors = _comparer.CompareModels(actual, expected, new SnapSettings { IgnoreProperties = Enumerable.Empty<string>() });
            Assert.Empty(errors);
        }

        [Fact]
        public void Compare_ExtendedPrimitivesDifferent_ReturnsErrors()
        {
            ExtendedPrimitiveModel actual = new()
            {
                ByteValue = 1,
                SByteValue = -1,
                CharValue = 'z',
                DoubleValue = 1.23,
                FloatValue = 4.56f,
                UIntValue = 123u,
                NIntValue = -42,
                NUIntValue = 42u,
                LongValue = -1000L,
                ULongValue = 1000UL,
                ShortValue = -10,
                UShortValue = 10
            };

            ExtendedPrimitiveModel expected = new()
            {
                ByteValue = 2,
                SByteValue = 1,
                CharValue = 'a',
                DoubleValue = 2.34,
                FloatValue = 7.89f,
                UIntValue = 456u,
                NIntValue = 24,
                NUIntValue = 24u,
                LongValue = 2000L,
                ULongValue = 2000UL,
                ShortValue = 20,
                UShortValue = 20
            };

            List<ErrorMessage> errors = _comparer.CompareModels(actual, expected, new SnapSettings { IgnoreProperties = Enumerable.Empty<string>() }).ToList();

            // there should be one mismatch per property (12 total)
            Assert.Equal(12, errors.Count);
            Assert.Contains(errors, e => e.Message.Contains("ByteValue"));
            Assert.Contains(errors, e => e.Message.Contains("SByteValue"));
            Assert.Contains(errors, e => e.Message.Contains("CharValue"));
            Assert.Contains(errors, e => e.Message.Contains("DoubleValue"));
            Assert.Contains(errors, e => e.Message.Contains("FloatValue"));
            Assert.Contains(errors, e => e.Message.Contains("UIntValue"));
            Assert.Contains(errors, e => e.Message.Contains("NIntValue"));
            Assert.Contains(errors, e => e.Message.Contains("NUIntValue"));
            Assert.Contains(errors, e => e.Message.Contains("LongValue"));
            Assert.Contains(errors, e => e.Message.Contains("ULongValue"));
            Assert.Contains(errors, e => e.Message.Contains("ShortValue"));
            Assert.Contains(errors, e => e.Message.Contains("UShortValue"));
        }

        [Fact]
        public void Compare_MorePrimitivesEqual_NoErrors()
        {
            ExtendedPrimitive2Model actual = new()
            {
                GuidValue = Guid.NewGuid(),
                DateTimeValue = DateTime.UtcNow,
                DateTimeOffsetValue = DateTimeOffset.Now,
                TimeSpanValue = TimeSpan.FromHours(1),
                DateOnlyValue = DateOnly.FromDateTime(DateTime.Today),
                TimeOnlyValue = TimeOnly.FromDateTime(DateTime.Now),
                EnumValue = ExampleEnum.First
            };

            // copy values exactly
            ExtendedPrimitive2Model expected = new()
            {
                GuidValue = actual.GuidValue,
                DateTimeValue = actual.DateTimeValue,
                DateTimeOffsetValue = actual.DateTimeOffsetValue,
                TimeSpanValue = actual.TimeSpanValue,
                DateOnlyValue = actual.DateOnlyValue,
                TimeOnlyValue = actual.TimeOnlyValue,
                EnumValue = actual.EnumValue
            };

            var errors = _comparer.CompareModels(actual, expected, new SnapSettings { IgnoreProperties = Enumerable.Empty<string>() });
            Assert.Empty(errors);
        }

        [Fact]
        public void Compare_MorePrimitivesDifferent_ReturnsErrors()
        {
            ExtendedPrimitive2Model actual = new()
            {
                GuidValue = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                DateTimeValue = new DateTime(2000, 1, 1),
                DateTimeOffsetValue = new DateTimeOffset(new DateTime(2001, 1, 1)),
                TimeSpanValue = TimeSpan.FromDays(1),
                DateOnlyValue = new DateOnly(2020, 1, 1),
                TimeOnlyValue = new TimeOnly(12, 0),
                EnumValue = ExampleEnum.First
            };

            ExtendedPrimitive2Model expected = new()
            {
                GuidValue = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                DateTimeValue = new DateTime(2010, 1, 1),
                DateTimeOffsetValue = new DateTimeOffset(new DateTime(2011, 1, 1)),
                TimeSpanValue = TimeSpan.FromDays(2),
                DateOnlyValue = new DateOnly(2021, 2, 2),
                TimeOnlyValue = new TimeOnly(13, 30),
                EnumValue = ExampleEnum.Second
            };

            List<ErrorMessage> errors = _comparer.CompareModels(actual, expected, new SnapSettings { IgnoreProperties = Enumerable.Empty<string>() }).ToList();

            // the comparer currently treats DateOnly/TimeOnly as non-primitive,
            // so mismatches for those properties may generate multiple errors.
            // we simply want to ensure each logical property is reported at least
            // once and that the total count is no less than the number of
            // expected properties.
            string[] expectedProperties =
            [
                "GuidValue", "DateTimeValue", "DateTimeOffsetValue",
                "TimeSpanValue", "DateOnlyValue", "TimeOnlyValue", "EnumValue"
            ];

            Assert.True(errors.Count == expectedProperties.Length,
                $"Expected exactly {expectedProperties.Length} errors but got {errors.Count}");

            foreach (var prop in expectedProperties)
            {
                Assert.Contains(errors, e => e.Message.Contains(prop));
            }
        }
    }
}