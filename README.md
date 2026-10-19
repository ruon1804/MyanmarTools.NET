# Myanmar Tools for .NET

A .NET package of [Myanmar Tools](https://github.com/google/myanmar-tools/), based on its C# client.
It detects whether Myanmar text is Zawgyi or Unicode and converts between the two encodings.

## Installation

```
dotnet add package MyanmarTools.NET
```

Version 2.x targets .NET Standard 2.0, so it runs on .NET Core 2.0 or later, .NET 5 or later, and .NET Framework 4.6.2 or later.

## Usage

To detect Zawgyi, create an instance of `ZawgyiDetector` and call `GetZawgyiProbability` with your string.
The result is the probability that the string is Zawgyi, rounded to three decimal places.
It returns `double.NegativeInfinity` if the string contains no Myanmar characters.

```csharp
using MyanmarTools;

var detector = new ZawgyiDetector();
double score = detector.GetZawgyiProbability("မ္း");
// score is now 1 (very likely Zawgyi)
```

If you need the unrounded value, call `GetZawgyiProbabilityRaw` instead.

```csharp
double raw = detector.GetZawgyiProbabilityRaw("မ္း");
// raw is now 0.99975726752178...
```

To convert between Zawgyi and Unicode, use the class `ZawgyiConverter` as shown below.

```csharp
var converter = new ZawgyiConverter();

// Zawgyi to Unicode
string uniOutput = converter.ZawgyiToUnicode("မ္း");
// uniOutput is now "မ်း"

// Unicode to Zawgyi
string zawOutput = converter.UnicodeToZawgyi("မ်း");
// zawOutput is now "မ္း"
```

## Contributing

Bug reports and pull requests for this .NET package are welcome on GitHub at https://github.com/ruon1804/MyanmarTools.NET. This project is intended to be a safe, welcoming space for collaboration, and contributors are expected to adhere to the [Contributor Covenant](http://contributor-covenant.org) code of conduct.

## License

The package is available as open source under the terms of the [Apache-2.0 License](http://www.apache.org/licenses/LICENSE-2.0).
