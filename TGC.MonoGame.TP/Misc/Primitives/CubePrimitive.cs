#region File Description

//-----------------------------------------------------------------------------
// CubePrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion File Description
#region Using Statements

using BepuUtilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;


#endregion Using Statements

namespace TGC.MonoGame.TP.Misc.Primitives
{
    /// <summary>
    ///     Geometric primitive class for drawing cubes.
    /// </summary>
    public class CubePrimitive : GeometricPrimitive
    {
        public CubePrimitive(GraphicsDevice graphicsDevice) : this(graphicsDevice, 1)
        {
        }

        /// <summary>
        ///     Constructs a new cube primitive, with the specified size.
        /// </summary>
        public CubePrimitive(GraphicsDevice graphicsDevice, float size)
        {
            // A cube has six faces, each one pointing in a different direction.
            Vector3[] normals =
            {
                Vector3.UnitZ, // front normal
                -Vector3.UnitZ, // back normal
                Vector3.UnitX, // right normal
                -Vector3.UnitX, // left normal
                Vector3.UnitY, // top normal
                -Vector3.UnitY // bottom normal
            };

            // Create each face in turn.
            foreach (var normal in normals)
                AddVertexPerFace(normal, Vector3.One * size);

            InitializePrimitive(graphicsDevice);
        }

        /// <summary>
        ///     Constructs a new cube primitive, with the specified height, width and depth.
        /// </summary>
        public CubePrimitive(GraphicsDevice graphicsDevice, Vector3 size)
        {
            AddVertexPerFace(Vector3.UnitZ, size); // front
            AddVertexPerFace(-Vector3.UnitZ, size); // back

            AddVertexPerFace(Vector3.UnitX, size); // right
            AddVertexPerFace(-Vector3.UnitX, size); // left

            AddVertexPerFace(Vector3.UnitY, size); // top
            AddVertexPerFace(-Vector3.UnitY, size); // bottom

            InitializePrimitive(graphicsDevice);
        }

        private void AddVertexPerFace(Vector3 normal, Vector3 size)
        {
            // Get two vectors perpendicular to the face normal and to each other.
            Vector3 side1 = new Vector3(normal.Y, normal.Z, normal.X);
            Vector3 side2 = Vector3.Cross(normal, side1);

            Vector3 vertex1 = normal - side1 - side2;
            Vector3 vertex2 = normal - side1 + side2;
            Vector3 vertex3 = normal + side1 + side2;
            Vector3 vertex4 = normal + side1 - side2;

            // Six indices (two triangles) per face.
            AddIndex(CurrentVertex + 0);
            AddIndex(CurrentVertex + 1);
            AddIndex(CurrentVertex + 2);

            AddIndex(CurrentVertex + 0);
            AddIndex(CurrentVertex + 2);
            AddIndex(CurrentVertex + 3);

            // Four vertices per face.
            AddVertex(vertex1 * size / 2f, normal, Vector2.UnitX);
            AddVertex(vertex2 * size / 2f, normal, Vector2.Zero);
            AddVertex(vertex3 * size / 2f, normal, Vector2.UnitY);
            AddVertex(vertex4 * size / 2f, normal, Vector2.One);
        }
    }
}