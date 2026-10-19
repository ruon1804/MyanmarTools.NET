using MyanmarTools.Markov;
using System.IO;
using System.Reflection;

namespace MyanmarTools
{
    /// <summary>
    /// Provides utilities to detect whether Myanmar text is more likely Zawgyi or Unicode.
    /// </summary>
    public class ZawgyiDetector
    {
        private readonly ZawgyiUnicodeMarkovModel Model;

        /// <summary>
        /// Initializes a new instance of the ZawgyiDetector.
        /// </summary>
        /// <exception cref="IOException">
        /// The embedded zawgyiUnicodeModel.dat resource is missing or is not a valid model file.
        /// </exception>
        public ZawgyiDetector()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MyanmarTools.Resources.zawgyiUnicodeModel.dat")
                ?? throw new IOException("Failed to load embedded model stream.");

            this.Model = new ZawgyiUnicodeMarkovModel(stream);
        }

        /// <summary>
        /// <para>Performs detection on the given string. Returns the probability that the string is Zawgyi given
        /// that it is either Unicode or Zawgyi. Values approaching 1 are strong Zawgyi; values approaching
        /// 0 are strong Unicode; and values close to 0.5 are toss-ups.</para>
        /// If the string does not contain any Myanmar range code points, <see cref="double.NegativeInfinity"/> is returned.
        /// <para>ပေးထားသော စာသားအား ယူနီကုတ် သို့မဟုတ် ဇော်ဂျီ ဖြစ်ကြောင်းစစ်ဆေးရန်။
        /// ဇော်ဂျီ စာသား ဖြစ်ပါက ၁ နှင့် အနီးပတ်ဝန်းကျင် ဂဏန်း ကိုရရှိပြီး ၊ ယူနီကုတ် စာသား ဖြစ်ပါက ၀ နှင့် အနီးပတ်ဝန်းကျင် ဂဏန်း ကိုရရှိမည် ဖြစ်ပါသည်။
        /// ၀.၅ အနီးပတ်ဝန်းကျင် ဂဏန်း ဖြစ်ပါက ယူနီကုတ်/ဇော်ဂျီ  နှစ်ခုစလုံး ဖြစ်နိုင်ပြီး လိုအပ်သလိုချိန်ဆ သုံးသွားနိုင်ပါသည်။
        /// ပေးထားသော စာသားတွင် ယူနီကုတ် နှင့် ဇော်ဂျီ  နှစ်ခုစလုံး မပါရှိပါက <see cref="double.NegativeInfinity"/> ကိုရရှိမည် ဖြစ်ပါသည်။</para>
        /// </summary>
        /// <param name="Input">
        /// <para>The string on which to run detection.</para>
        /// <para>ယူနီကုတ် သို့မဟုတ် ဇော်ဂျီ ဖြစ်ကြောင်းစစ်ဆေးရန် စာသား</para>
        /// </param>
        /// <returns>
        /// <para>The rounded probability (3 decimal places) that the string is Zawgyi (between 0 and 1), or
        /// <see cref="double.NegativeInfinity"/> if the string contains no Myanmar range code points.</para>
        /// <para>ဇော်ဂျီ စာသား ဖြစ်နိုင်ခြေ (၀ နှင့် ၁ ကြား) ၊ သို့မဟုတ် ယူနီကုတ် နှင့် ဇော်ဂျီ  နှစ်ခုစလုံး မပါရှိပါက <see cref="double.NegativeInfinity"/>.</para>
        /// </returns>
        public double GetZawgyiProbability(string Input)
        {
            return Model.Predict(Input);
        }

        /// <summary>
        /// <para>Performs detection on the given string and returns the unrounded Zawgyi probability.</para>
        /// <para>This method preserves full precision and is intended for advanced consumers.
        /// Use <see cref="GetZawgyiProbability(string)"/> for backward-compatible rounded behavior.</para>
        /// </summary>
        /// <param name="Input">The string on which to run detection.</param>
        /// <returns>
        /// The unrounded probability that the string is Zawgyi (between 0 and 1), or
        /// <see cref="double.NegativeInfinity"/> if no Myanmar range code points are present.
        /// </returns>
        public double GetZawgyiProbabilityRaw(string Input)
        {
            return Model.PredictRaw(Input);
        }
    }
}
