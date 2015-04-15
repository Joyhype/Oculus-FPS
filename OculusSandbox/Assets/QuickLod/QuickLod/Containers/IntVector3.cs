// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IntVector3.cs" company="Jasper Ermatinger">
//   Copyright © 2014-2015 Jasper Ermatinger
//   Do not distribute or publish this code in any way without permission of the copyright owner.
// </copyright>
// <summary>
//   A structure to store vectors with integer precision
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QuickLod.Containers
{
    using System;

    using UnityEngine;

    /// <summary>
    /// A structure to store vectors with integer precision
    /// </summary>
    [Serializable]
    public struct IntVector3
    {
        #region Fields

        /// <summary>
        /// The x component of this vector
        /// </summary>
        public int X;

        /// <summary>
        /// The y component of this vector
        /// </summary>
        public int Y;

        /// <summary>
        /// The z component of this vector
        /// </summary>
        public int Z;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IntVector3"/> struct. 
        /// Initializes a new instance of the <see cref="IntVector3"/> class.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <param name="y">
        /// The y.
        /// </param>
        public IntVector3(int x, int y)
        {
            this.X = x;
            this.Y = y;
            this.Z = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntVector3"/> struct. 
        /// Initializes a new instance of the <see cref="IntVector3"/> class.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <param name="y">
        /// The y.
        /// </param>
        /// <param name="z">
        /// The z.
        /// </param>
        public IntVector3(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public IntVector3(Vector3 vector)
        {
            this.X = Mathf.RoundToInt(vector.x);
            this.Y = Mathf.RoundToInt(vector.y);
            this.Z = Mathf.RoundToInt(vector.z);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the length.
        /// </summary>
        public float Magnitude
        {
            get
            {
                return Mathf.Sqrt((this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z));
            }
        }

        /// <summary>
        /// Gets the square length.
        /// </summary>
        public float SqrMagnitude
        {
            get
            {
                return (this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z);
            }
        }

        #endregion

        #region Public Indexers

        /// <summary>
        /// The respective x, y, or z component
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <exception cref="IndexOutOfRangeException">
        /// Throws an exception when index is smaller than 0 or larger than 2
        /// </exception>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        {
                            return this.X;
                        }

                    case 1:
                        {
                            return this.Y;
                        }

                    case 2:
                        {
                            return this.Z;
                        }
                }

                throw new IndexOutOfRangeException("The index cannot be under 0 or over 2.");
            }

            set
            {
                switch (index)
                {
                    case 0:
                        {
                            this.X = value;
                            return;
                        }

                    case 1:
                        {
                            this.Y = value;
                            return;
                        }

                    case 2:
                        {
                            this.Z = value;
                            return;
                        }
                }

                throw new IndexOutOfRangeException("The index cannot be under 0 or over 2.");
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Adds two vectors
        /// </summary>
        /// <param name="vect1">
        /// The vect 1.
        /// </param>
        /// <param name="vect2">
        /// The vect 2.
        /// </param>
        /// <returns>
        /// </returns>
        public static IntVector3 operator +(IntVector3 vect1, IntVector3 vect2)
        {
            return new IntVector3(vect1.X + vect2.X, vect1.Y + vect2.Y, vect1.Z + vect2.Z);
        }

        /// <summary>
        /// Returns true when the two vectors are equal
        /// </summary>
        /// <param name="lhs">
        /// The lhs.
        /// </param>
        /// <param name="rhs">
        /// The rhs.
        /// </param>
        /// <returns>
        /// </returns>
        public static bool operator ==(IntVector3 lhs, IntVector3 rhs)
        {
            return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z;
        }

        /// <summary>
        /// Returns true if the vectors are different.
        /// </summary>
        /// <param name="lhs">
        /// The lhs.
        /// </param>
        /// <param name="rhs">
        /// The rhs.
        /// </param>
        /// <returns>
        /// </returns>
        public static bool operator !=(IntVector3 lhs, IntVector3 rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Multiplies a vector by a number.
        /// </summary>
        /// <param name="lhs">
        /// The lhs.
        /// </param>
        /// <param name="rhs">
        /// The rhs.
        /// </param>
        /// <returns>
        /// </returns>
        public static IntVector3 operator *(IntVector3 lhs, int rhs)
        {
            return new IntVector3(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs);
        }

        /// <summary>
        /// Multiplies a vector by a number.
        /// </summary>
        /// <param name="lhs">
        /// The lhs.
        /// </param>
        /// <param name="rhs">
        /// The rhs.
        /// </param>
        /// <returns>
        /// </returns>
        public static IntVector3 operator *(int lhs, IntVector3 rhs)
        {
            return new IntVector3(rhs.X * lhs, rhs.Y * lhs, rhs.Z * lhs);
        }

        /// <summary>
        /// Multiplies a vector by a number
        /// </summary>
        /// <param name="lhs">
        /// The lhs.
        /// </param>
        /// <param name="rhs">
        /// The rhs.
        /// </param>
        /// <returns>
        /// </returns>
        public static Vector3 operator *(IntVector3 lhs, float rhs)
        {
            return new Vector3(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs);
        }

        /// <summary>
        /// The *.
        /// </summary>
        /// <param name="lhs">
        /// The lhs.
        /// </param>
        /// <param name="rhs">
        /// The rhs.
        /// </param>
        /// <returns>
        /// </returns>
        public static Vector3 operator *(float lhs, IntVector3 rhs)
        {
            return new Vector3(rhs.X * lhs, rhs.Y * lhs, rhs.Z * lhs);
        }
        
        /// <summary>
        /// The -.
        /// </summary>
        /// <param name="vect1">
        /// The vect 1.
        /// </param>
        /// <param name="vect2">
        /// The vect 2.
        /// </param>
        /// <returns>
        /// </returns>
        public static IntVector3 operator -(IntVector3 vect1, IntVector3 vect2)
        {
            return new IntVector3(vect1.X - vect2.X, vect1.Y - vect2.Y, vect1.Z - vect2.Z);
        }

        /// <summary>
        /// Multiplies two <see cref="IntVector3"/> component-wise
        /// </summary>
        /// <param name="vector1">The first vector</param>
        /// <param name="vector2">The second vector</param>
        /// <returns>Returns the scaled vector</returns>
        public static IntVector3 Scale(IntVector3 vector1, IntVector3 vector2)
        {
            return new IntVector3(vector1.X * vector2.X, vector1.Y * vector2.Y, vector1.Z * vector2.Z);
        }

        /// <summary>
        /// Multiplies an <see cref="IntVector3"/> and a <see cref="Vector3"/> component-wise
        /// </summary>
        /// <param name="intVector">The <see cref="IntVector3"/></param>
        /// <param name="vector">The <see cref="Vector3"/></param>
        /// <returns>Returns the scaled vector</returns> 
        public static Vector3 Scale(IntVector3 intVector, Vector3 vector)
        {
            return new Vector3(intVector.X * vector.x, intVector.Y * vector.y, intVector.Z * vector.z);
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Equals(IntVector3 other)
        {
            return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is IntVector3 && this.Equals((IntVector3)obj);
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.X;
                hashCode = (hashCode * 397) ^ this.Y;
                hashCode = (hashCode * 397) ^ this.Z;
                return hashCode;
            }
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return this.X + ", " + this.Y + ", " + this.Z;
        }

        /// <summary>
        /// Converts this to a Vector3
        /// </summary>
        /// <returns>
        /// Returns a Vector3 equivalent of this one
        /// </returns>
        public Vector3 ToVector3()
        {
            return new Vector3(this.X, this.Y, this.Z);
        }

        #endregion
    }
}