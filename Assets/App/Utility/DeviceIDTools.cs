using System;
using System.Runtime.InteropServices;
using GSDev.Singleton;
using UnityEngine;

namespace GSDev.DeviceID
{
    public class DeviceIDTools
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern IntPtr GetDeviceID();
#endif

        public static string GetDeviceUUID()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return Marshal.PtrToStringAuto(GetDeviceID());
#else
            return SystemInfo.deviceUniqueIdentifier;
#endif
        }
    }
}
