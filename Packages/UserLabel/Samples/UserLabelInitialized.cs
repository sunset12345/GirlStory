// using System;
// using System.Linq;
// using System.Reflection;
//
// namespace GSDev.UserLabel.Sample
// {
//     public static class UserLabelInitialized
//     {
//         public static void Initialized(this UserLabelManager manager)
//         {
//             manager.LoadConfig();
//             
//             // var methodInfos = AppDomain.CurrentDomain.GetAssemblies()
//             //     .SelectMany(x => x.GetTypes())
//             //     .Where(t => t.IsClass)
//             //     .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
//             //     .Where(m => m.GetCustomAttributes(typeof(UserLabelGetterAttribute), false).FirstOrDefault() != null);
//
//             var methodInfos = typeof(UserLabelGetters).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
//             
//             manager.FetchLabels(methodInfos);
//             manager.RefreshMatchedRule();
//         }
//     }
// }