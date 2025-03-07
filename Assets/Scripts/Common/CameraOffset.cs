using System;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// Структура для хранения смещения и поворота камеры.
    /// </summary>
    [Serializable]
    public struct CameraOffset
    {
        /// <summary>
        /// Поворот камеры вокруг оси X (тангаж).
        /// </summary>
        [Tooltip("Поворот камеры вокруг оси X (Наклон камеры)")]
        public float RotationX;

        /// <summary>
        /// Высота камеры по оси Y.
        /// </summary>
        [Tooltip("Высота камеры")]
        public float HeightY;

        /// <summary>
        /// Смещение камеры по оси Z.
        /// </summary>
        [Tooltip("Смещение по оси Z")]
        public float OffsetZ;

        /// <summary>
        /// Конструктор с параметрами для инициализации смещения камеры.
        /// </summary>
        /// <param name="rotationX">Поворот вокруг оси X.</param>
        /// <param name="heightY">Высота по оси Y.</param>
        /// <param name="offsetZ">Смещение по оси Z.</param>
        public CameraOffset(float rotationX, float heightY, float offsetZ)
        {
            RotationX = rotationX;
            HeightY = heightY;
            OffsetZ = offsetZ;
        }

        /// <summary>
        /// Позиция камеры на основе смещения.
        /// </summary>
        public Vector3 Position => new Vector3(0, HeightY, OffsetZ);

        /// <summary>
        /// Поворот камеры на основе угла RotationX.
        /// </summary>
        public Quaternion Rotation => Quaternion.Euler(RotationX, 0, 0);
    }
}