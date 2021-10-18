﻿using System.Numerics;

namespace OpenGL_Util.Model
{
    public class Singularity : ITransform
    {
        public Singularity() : this(Vector3.Zero)
        {
        }

        public Singularity(Vector3 position) : this(position, Quaternion.Identity)
        {
        }

        public Singularity(Vector3 position, Quaternion rotation) : this(position, rotation, Vector3.One)
        {
        }

        public Singularity(Vector3 position, Vector3 scale) : this(position, Quaternion.Identity, scale)
        {
        }

        public Singularity(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
    }
}
