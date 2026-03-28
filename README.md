# MirrorSnap.Net

MirrorSnap.Net is an early-stage .NET snapshot-style model comparison project. The current implementation is centered on a reflection-based comparer in `MirrorSnap.Core`, while the `MirrorSnap.xUnit` package is present only as a scaffold for future test-framework integration.

This repository does not yet implement a complete snapshot file workflow. What it does provide today is a tested object comparison engine that can walk object graphs, compare values, and ignore selected paths with regex patterns.

## Project layout

| Project | Target framework | Purpose |
| --- | --- | --- |
| `MirrorSnap.Core` | `netstandard2.0` | Core comparison engine and shared models |
| `MirrorSnap.xUnit` | `net8.0` | xUnit integration package shell with no runtime code yet |
| `MirrorSnap.Core.Tests` | `net8.0` | xUnit tests that define and verify current behavior |

## Current API

### `ModelComparer`

The main implemented entry point is `MirrorSnap.Core.Services.ModelComparer`:

```csharp
IEnumerable<ErrorMessage> CompareModels<TEntity>(
    TEntity actual,
    TEntity expected,
    SnapSettings settings)
```

`CompareModels` returns collected mismatch messages for value differences and throws exceptions for invalid or structurally incompatible inputs.

### `SnapSettings`

`MirrorSnap.Core.Models.SnapSettings` currently exposes:

```csharp
public class SnapSettings
{
    public IEnumerable<string> IgnoreProperties { get; set; }
}
```

`IgnoreProperties` is a sequence of regular expressions matched against generated property paths such as:

- `.Inner.StringValue`
- `.Level1.Inner.IntValue`
- `.Items[1].Name`

### Other present types

The repository also includes:

- `MirrorSnap.Core.Models.SnapSpec<TEntity>`
- `MirrorSnap.Core.MirrorSnapAttribute`

These types suggest a broader snapshot-oriented design, but the current repository does not yet include an end-to-end JSON load/save pipeline or implemented test framework helpers that use them.

## Current behavior

Based on the existing test suite, `MirrorSnap.Core` currently supports:

- Comparing primitive values such as `int`, `bool`, `string`, and `decimal`
- Comparing extended primitive-like values including `Guid`, `DateTime`, `DateTimeOffset`, `TimeSpan`, enums, numeric primitives, `IntPtr`, and `UIntPtr`
- Traversing nested objects recursively
- Comparing `List<T>` collections and arrays by index
- Returning accumulated `ErrorMessage` entries for value mismatches
- Throwing exceptions for null mismatches, type mismatches, and collection count mismatches

Behavior that is important to know up front:

- Property paths start at the root with a leading dot, for example `.Name` or `.Inner.StringValue`
- Collection element paths include indexes, for example `.Items[1].StringValue`
- Ignore rules are regex matches against those generated paths

## Examples

### Basic comparison

```csharp
using System;
using System.Linq;
using MirrorSnap.Core.Models;
using MirrorSnap.Core.Services;

var actual = new SampleModel
{
    Id = 1,
    Name = "actual"
};

var expected = new SampleModel
{
    Id = 1,
    Name = "expected"
};

var comparer = new ModelComparer();

var errors = comparer.CompareModels(
    actual,
    expected,
    new SnapSettings
    {
        IgnoreProperties = Array.Empty<string>()
    })
    .ToList();

foreach (var error in errors)
{
    Console.WriteLine(error.Message);
}

public class SampleModel
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

Possible output:

```text
Value mismatch at path '.Name'. Expected: expected, Actual: actual
```

### Ignoring selected paths

```csharp
using System.Linq;
using MirrorSnap.Core.Models;
using MirrorSnap.Core.Services;

var actual = new WrapperModel
{
    Inner = new SampleModel
    {
        Id = 1,
        Name = "left"
    }
};

var expected = new WrapperModel
{
    Inner = new SampleModel
    {
        Id = 2,
        Name = "right"
    }
};

var comparer = new ModelComparer();

var errors = comparer.CompareModels(
    actual,
    expected,
    new SnapSettings
    {
        IgnoreProperties = new[]
        {
            @"\.Inner\.Id",
            @"\.Inner\.Name"
        }
    })
    .ToList();

public class WrapperModel
{
    public SampleModel Inner { get; set; }
}

public class SampleModel
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

In this example, both mismatching properties are ignored because their generated paths match the provided regex patterns.

## Status and limitations

- No repository-level README existed before this one
- `MirrorSnap.xUnit` currently contains only project scaffolding and no implemented helpers, assertions, or extensions
- No JSON snapshot loading or writing flow is implemented today
- `MirrorSnapAttribute(string jsonPath)` exists, but there is no repository code yet that uses it to execute snapshot tests
- The current value proposition is the comparer and its tested traversal rules, not a finished snapshot-testing framework

## Development and verification

Requirements:

- .NET 8 SDK for running tests and working with `MirrorSnap.xUnit`

Main validation command:

```bash
dotnet test
```

This is the primary way to verify the documented behavior because the test suite in `MirrorSnap.Core.Tests` is the clearest executable specification of what the project supports today.

## Contributing

If you want to extend the project, the safest starting point is `MirrorSnap.Core.Tests`. New behavior should be described with tests first, then implemented in `MirrorSnap.Core`, and only after that exposed through future framework integrations such as `MirrorSnap.xUnit`.
