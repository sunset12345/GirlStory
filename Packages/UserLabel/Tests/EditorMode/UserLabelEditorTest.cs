using System.Reflection;
using NUnit.Framework;
using GSDev.UserLabel;

public class UserLabelEditorTest
{
    private static class LabelGetter
    {
        [UserLabelGetter("string_field")]
        private static string StringLabelGetter()
        {
            return "string_value";
        }
        
        [UserLabelGetter("int_field")]
        private static int IntLabelGetter()
        {
            return 12;
        }

        [UserLabelGetter("string_public")]
        public static string PublicStringLabelGetter()
        {
            return "public";
        }

        [UserLabelGetter("float_field")]
        public static float FloatLabelGetter()
        {
            return 0.5f;
        }
    }

    [Test]
    public void FetchLabels()
    {
        var manager = new UserLabelManager();
        manager.FetchLabels(typeof(LabelGetter).GetMethods(
            BindingFlags.Static | 
            BindingFlags.NonPublic | 
            BindingFlags.Public));

        Assert.True(manager.GetFieldValue<int>("int_field") == 12);
        Assert.True(manager.GetFieldValue<string>("string_field") == "string_value");
        Assert.True(manager.GetFieldValue<string>("string_public") == "public");
        Assert.True(manager.GetFieldValue<float>("float_field") == 0.5f);
    }
}