using System;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace GSDev.UserLabel.Sample
{
    public static class UserLabelGetters
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

        public static void Fetch()
        {
            var manager = new UserLabelManager();
            manager.FetchLabels(typeof(UserLabelGetters).GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
        }
    }
}