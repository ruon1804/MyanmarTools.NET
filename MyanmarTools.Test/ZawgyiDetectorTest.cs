using System;
using System.Globalization;
using System.IO;
using System.Text;
using Xunit;

namespace MyanmarTools.Test
{
    public class ZawgyiDetectorTest
    {
        private readonly ZawgyiDetector detector;

        public ZawgyiDetectorTest()
        {
            detector = new ZawgyiDetector();
        }

        [Fact(DisplayName = "Sanity")]
        public void Sanity()
        {
            double actual = detector.GetZawgyiProbability("hello world");
            Assert.Equal(double.NegativeInfinity, actual);
        }

        [Fact(DisplayName = "testIgnoreNonMyanmarCodePoints")]
        public void TestIgnoreNonMyanmarCodePoints()
        {
            var allASCII = "blah blah blah blah blah";
            var mixedUnicode = "<span>blah blah ဒဂုန်ဦးစန်းငွေ </span> blah blah blah blah";
            string mixedZawgyi = "blah blah blah blah blah သို႔သြားပါ။ blah blah blah";

            //All ASCII
            var Expected = double.NegativeInfinity;
            var Actual = detector.GetZawgyiProbability(allASCII);
            Assert.True(Actual == Expected, $"Error:All ASCII; Expected:{Expected} == Actual:{Actual}");

            //Mixed Unicode
            Expected = 0.01;
            Actual = detector.GetZawgyiProbability(mixedUnicode);
            Assert.True(Actual < Expected, $"Error:Mixed Unicode; Expected:{Expected} < Actual:{Actual}");

            //Mixed Zawgyi
            Expected = 0.99;
            Actual = detector.GetZawgyiProbability(mixedZawgyi);
            Assert.True(Actual > Expected, $"Error:Mixed Zawgyi; Expected:{Expected} > Actual:{Actual}");
        }

        [Fact(DisplayName = "strongUnicodeReturnsLowScore")]
        public void StrongUnicodeReturnsLowScore()
        {
            var strongUnicode = "အပြည်ပြည်ဆိုင်ရာ လူ့အခွင့်အရေး ကြေညာစာတမ်း";

            var Expected = 0.001;
            var Actual = detector.GetZawgyiProbability(strongUnicode);
            Assert.True(Actual < Expected, $"Error:Strong Unicode; Expected:{Expected} < Actual:{Actual}");
        }

        [Fact(DisplayName = "strongZawgyiReturnsHighScore")]
        public void StrongZawgyiReturnsHighScore()
        {
            var strongZawgyi = "အျပည္ျပည္ဆိုင္ရာ လူ႔အခြင့္အေရး ေၾကညာစာတမ္း";
            var Expected = 0.999;
            var Actual = detector.GetZawgyiProbability(strongZawgyi);
            Assert.True(Actual > Expected, $"Error:Strong Zawgyi; Expected:{Expected} > Actual:{Actual}");
        }

        [Theory(DisplayName = "testDifficult")]
        // STRINGS IDENTICAL IN UNICODE/ZAWGYI
        // TODO: These should return scores in the middle range.
        [InlineData("အသံကို အစားထိုးလိုပါသလား။", 0.995)] // Truth: Identical in U/Z
        [InlineData("နမူနာ", 0.26)] // Truth: Identical in U/Z
        [InlineData(" ဦး", 0.35)] // Truth: Identical in U/Z
        // UNICODE STRINGS WITH HIGH ZAWGYI SCORES
        // TODO: Fix detection so that these get low Zawgyi scores.
        [InlineData("အစားထိုး အထူးအက္ခရာ", 0.995)] // Truth: Unicode
        [InlineData("ယခု မိုးရွာနေပါသလား။", 0.995)] // Truth: Unicode
        [InlineData("အခြား", 0.74)] // Truth: Unicode
        // DIFFICULT STRINGS THAT DETECT CORRECTLY
        // Changes to the detector should not significantly change these scores.
        [InlineData("ကာမစာအုပ္မ်ား(ေစာက္ပတ္စာအုပ္မ်ား)", 1.0)] // Truth: Zawgyi
        [InlineData("ညႇပ္စရာမလို", 0.82)] // Truth: Zawgyi
        public void TestDifficult(string Input, double Expected)
        {
            var Actual = detector.GetZawgyiProbability(Input);
            var ExpectedDiff = 0.005;
            var Epsilon = 1e-12;
            var ActualDiff = Math.Abs(Actual - Expected);
            Assert.True(ActualDiff <= ExpectedDiff + Epsilon, $"Error:{Input}; Expected:{ExpectedDiff} Different Actual:{ActualDiff}");
        }

        [Fact(DisplayName = "testIgnoreNumerals")]
        public void TestIgnoreNumerals()
        {
            // Digits/numerals are ignored and treated like non-Myanmar code points.
            Assert.Equal(double.NegativeInfinity, detector.GetZawgyiProbability("၉၆.၀ kHz"));
            Assert.Equal(detector.GetZawgyiProbability("၂၄၀၉ ဒဂုန်"), detector.GetZawgyiProbability("ဒဂုန်"));
        }

        [Fact(DisplayName = "GetZawgyiProbabilityRaw returns full precision")]
        public void GetZawgyiProbabilityRawReturnsFullPrecision()
        {
            var input = "လူခပ်သိမ်း၏";

            var rounded = detector.GetZawgyiProbability(input);
            var raw = detector.GetZawgyiProbabilityRaw(input);

            Assert.Equal(Math.Round(raw, 3), rounded);
            Assert.NotEqual(raw, rounded);
            Assert.Equal(double.NegativeInfinity, detector.GetZawgyiProbabilityRaw("hello world"));
        }

        /// <summary>
        /// Compares the unrounded probability against the reference values in compatibility.tsv,
        /// so that any drift in the model output is detected, not only drift beyond the rounding step.
        /// </summary>
        [Fact(DisplayName = "testCompatibility")]
        public void TestCompatibility()
        {
            using var stream = File.Open(Path.Combine("resources", "compatibility.tsv"), FileMode.Open, FileAccess.Read, FileShare.Read);
            using var streamReader = new StreamReader(stream, Encoding.UTF8, true, 1024);

            while (streamReader.Peek() != -1)
            {
                var line = streamReader.ReadLine()!;
                int tabIdx = line.IndexOf('\t');
                var strValue = line[..tabIdx];
                double Expected = double.NegativeInfinity;
                if (strValue != "-Infinity")
                {
                    Expected = double.Parse(strValue, CultureInfo.InvariantCulture);
                }

                var input = line[(tabIdx + 1)..];
                double Actual = detector.GetZawgyiProbabilityRaw(input);

                if (Expected == double.NegativeInfinity)
                {
                    Assert.True(Actual == Expected, $"Error:{input}; Expected:{strValue} Different Actual:{Actual}");
                }
                else
                {
                    var ActualDiff = Math.Abs(Actual - Expected);
                    Assert.True(ActualDiff <= 1e-6, $"Error:{input}; Expected:{strValue} Different Actual:{Actual}");
                }
            }
        }
    }
}
