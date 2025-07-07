using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.Utility
{
    public class Vector3Ref
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public Vector3 Value { get; private set; }

        public static Vector3Ref Parse(string content)
        {
            var vector3Ref = new Vector3Ref();
            if (string.IsNullOrWhiteSpace(content))
                return new Vector3Ref();

            var arr = content.Split(':');
            vector3Ref.X = arr.Length >= 1 ? arr[0].ToFloat() : 0;
            vector3Ref.Y = arr.Length >= 2 ? arr[1].ToFloat() : 0;
            vector3Ref.Z = arr.Length >= 3 ? arr[2].ToFloat() : 0;
            vector3Ref.Value = new Vector3(vector3Ref.X, vector3Ref.Y, vector3Ref.Z);
            return vector3Ref;
        }
    }
}