using System;
using System.Numerics;

namespace SoftRenderingApp3D {

    public struct Rotation3D {
        public Quaternion rotation;

        // Constructor using Euler angles
        public Rotation3D(float pitch, float yaw, float roll) {
            rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
        }

        public Quaternion Quaternion { get { return rotation; } }
        
        // Constructor using a Quaternion
        public Rotation3D(Quaternion quaternion) {
            rotation = quaternion;
        }

        // Factory method to create Rotation3D from Euler angles
        public static Rotation3D FromEulerAngles(float pitch, float yaw, float roll) {
            return new Rotation3D(Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll));
        }

        // Convert Rotation3D to a unit vector representation
        public Vector3 ToVector() => Vector3.Transform(Vector3.UnitZ, rotation);
        
        // Apply rotation to a vector
        public Vector3 Rotate(Vector3 point) {
            return Vector3.Transform(point, rotation);
        }

        // Combine two rotations
        public static Rotation3D operator *(Rotation3D a, Rotation3D b) {
            return new Rotation3D(a.rotation * b.rotation);
        }

        // Inverse rotation
        public Rotation3D Inverse() {
            return new Rotation3D(Quaternion.Inverse(rotation));
        }

        // Interpolation between two rotations
        public static Rotation3D Slerp(Rotation3D a, Rotation3D b, float t) {
            return new Rotation3D(Quaternion.Slerp(a.rotation, b.rotation, t));
        }

        // Override ToString for debugging purposes
        public override string ToString() {
            return $"Rotation3D: {rotation}";
        }
    }
}
