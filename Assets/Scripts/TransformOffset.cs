using System;
using UnityEngine;

namespace PeakTrials.Scripts.Common
{
    [Serializable]
    public class TransformOffset
    {
        public Vector3 position;
        public Vector3 rotation;


        public Quaternion Rotation => Quaternion.Euler(rotation);
    }
}