using System;
using System.Windows.Media.Media3D;

namespace ZusiKlassenLib2.Graphic
{
    public static class Zusi3D
    {
        private static Vector3D _v3Left = new(-1, 0, 0);
        private static Vector3D _v3Right = new(1, 0, 0);
        private static Vector3D _v3Down = new(0, -1, 0);
        private static Vector3D _v3Up = new(0, 1, 0);
        private static Vector3D _v3Back = new(0, 0, -1);
        private static Vector3D _v3Forward = new(0, 0, 1);

        public static Vector3D V3Left => _v3Left;
        public static Vector3D V3Right => _v3Right;
        public static Vector3D V3Down => _v3Down;
        public static Vector3D V3Up => _v3Up;
        public static Vector3D V3Back => _v3Back;
        public static Vector3D V3Forward => _v3Forward;

        public enum Axis
        {
            X,
            Y,
            Z
        }

        // arc in radians
        public static System.Windows.Media.Media3D.Point3D Rotate(this System.Windows.Media.Media3D.Point3D p, Axis axis, double phi)
        {
            // Bei Zusi wurde die Drehrichtung der X- und Y-Achse umgekehrt
            if (axis != Axis.Z)
            {
                phi = -phi;
            }

            double s = Math.Sin(phi);
            double c = Math.Cos(phi);

            /* x-axis
             * X  1  0  0 = X
             * Y  0  c -s = cY - sZ
             * Z  0  s  c = sY + cZ
             *
             * y-axis
             * X  c  0  s = cX + sZ
             * Y  0  1  0 = Y
             * Z -s  0  c = cZ - sX
             * 
             * z-axis
             * X  c -s  0 = cX - sY
             * Y  s  c  0 = sX + cY
             * Z  0  0  1 = Z
             */

            return axis switch
            {
                Axis.X => new System.Windows.Media.Media3D.Point3D(p.X, c * p.Y - s * p.Z, s * p.Y + c * p.Z),
                Axis.Y => new System.Windows.Media.Media3D.Point3D(c * p.X + s * p.Z, p.Y, c * p.Z - s * p.X),
                Axis.Z => new System.Windows.Media.Media3D.Point3D(c * p.X - s * p.Y, s * p.X + c * p.Y, p.Z),
                _ => throw new ArgumentException("Can't rotate point, due to invalid axis"),
            };
        }

        public static bool IsNullPoint(this System.Windows.Media.Media3D.Point3D p)
        {
            return p.X == 0 && p.Y == 0 && p.Z == 0;
        }

        public static bool IsNullVector(this Vector3D v)
        {
            return v.X == 0 && v.Y == 0 && v.Z == 0;
        }

        public static Matrix3D RotationMatrix(Axis axis, double phi)
        {
            // Bei Zusi wurde die Drehrichtung der X- und Y-Achse umgekehrt
            if (axis != Axis.Z)
            {
                phi = -phi;
            }

            double c = Math.Cos(phi);
            double s = Math.Sin(phi);

            return axis switch
            {
                Axis.X => new Matrix3D(1, 0, 0, 0,
                                       0, c, -s, 0,
                                       0, s, c, 0,
                                       0, 0, 0, 1),
                Axis.Y => new Matrix3D(c, 0, s, 0,
                                       0, 1, 0, 0,
                                       -s, 0, c, 0,
                                       0, 0, 0, 1),
                Axis.Z => new Matrix3D(c, -s, 0, 0,
                                       s, c, 0, 0,
                                       0, 0, 1, 0,
                                       0, 0, 0, 1),
                _ => Matrix3D.Identity,
            };
        }

        public static Quaternion Create(Vector3D axis, double phi)
        {
            double phi_2 = phi * 0.5;
            double sin = Math.Sin(phi_2);
            return new Quaternion(sin * axis.X, sin * axis.Y, sin * axis.Z, Math.Cos(phi_2));
        }

        public static Quaternion CreateFromYawPitchRoll(Point3D phi)
        {
            Quaternion qYaw = Create(V3Forward, phi.Z);
            Quaternion qPitch = Create(V3Down, phi.Y);
            Quaternion qRoll = Create(V3Right, phi.X);
            return Quaternion.Multiply(Quaternion.Multiply(qYaw, qPitch), qRoll);
        }
    }
}
