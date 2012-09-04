using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Race
{
    class Track
    {
        List<Vector2> controlPoints;

        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;

        public VertexPositionNormalTexture[] trackVertices;

        int numberVertices, numberIndices;

        GraphicsDevice graphicsDevice;
        BasicEffect effect;
        Texture2D texture;

        float trackLength; //total len of the track

        public Track(List<Vector2> points, int numberDivisions, float width, int textureRepeats, GraphicsDevice device, ContentManager manager)
        {
            this.graphicsDevice = device;
            this.controlPoints = SmoothPath(points, numberDivisions);

            effect = new BasicEffect(this.graphicsDevice);
            texture = manager.Load<Texture2D>("track");

            createBuffers(width, textureRepeats);

            System.Console.Out.WriteLine("n vert" + numberVertices);
            System.Console.Out.WriteLine("n ind" + numberIndices);
        }

        List<Vector2> SmoothPath(List<Vector2> additionalPoints, int numberDivisions)
        {
            List<Vector2> newControlPoints = new List<Vector2>();

            for (int i = 0; i < additionalPoints.Count - 1; i++)
            {
                newControlPoints.Add(additionalPoints[i]);
                for (int j = 0; j < numberDivisions; j++)
                {
                    float far = (float)(j + 1) / (float)(numberDivisions + 2);

                    Vector2 interpolated = catmullRom(
                        additionalPoints[MakeIndex(i-1, additionalPoints.Count-1)],
                        additionalPoints[i],
                        additionalPoints[MakeIndex(i+1, additionalPoints.Count -1)],
                        additionalPoints[MakeIndex(i+2, additionalPoints.Count-1)],
                        far
                        );

                    newControlPoints.Add(interpolated);
                }
            }

            return newControlPoints;
        }

        private int MakeIndex(int val, int maxVal)
        {
            val = Math.Abs( val % maxVal ); //check
            return val;
        }

        private Vector2 catmullRom(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4, float far)
        {
            return new Vector2(MathHelper.CatmullRom(v1.X, v2.X, v3.X, v4.X, far), MathHelper.CatmullRom(v1.Y, v2.Y, v3.Y, v4.Y, far));
        }

        void createBuffers(float trackWidth, int textureRepetitions)
        {
            VertexPositionNormalTexture[] vertices = createVertices(trackWidth,
                textureRepetitions);

            // Create vertex buffer and set data
            vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture),
                vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionNormalTexture>(vertices);

            trackVertices = vertices;

            int[] indices = createIndices();

            // Reach compatibility requires 16 bit indices (short instead of int)
            if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
            {
                short[] indices16 = new short[indices.Length];

                for (int i = 0; i < indices.Length; i++)
                    indices16[i] = (short)indices[i];

                // Create index buffer and set data
                indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits,
                    indices.Length, BufferUsage.WriteOnly);
                indexBuffer.SetData<short>(indices16);
            }
            else
            {
                // Create index buffer and set data
                indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits,
                    indices.Length, BufferUsage.WriteOnly);
                indexBuffer.SetData<int>(indices);
            }
        }

        VertexPositionNormalTexture[] createVertices(float trackWidth, int textureRepetitions)
        {
            // Create 2 vertices for each track point
            numberVertices = controlPoints.Count * 2;
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[numberVertices];

            int j = 0;
            trackLength = 0;

            for (int i = 0; i < controlPoints.Count; i++)
            {
                // Find the index of the next position
                int next = MakeIndex(i + 1, controlPoints.Count - 1);

                // Find the current and next positions on the path
                Vector3 position = new Vector3(controlPoints[i].X, 0, controlPoints[i].Y);
                Vector3 nextPosition = new Vector3(controlPoints[next].X, 0, controlPoints[next].Y);

                // Find the vector between the current and next position
                Vector3 forward = nextPosition - position;
                float length = forward.Length();
                forward.Normalize();

                // Find the side vector based on the forward and up vectors
                Vector3 side = -Vector3.Cross(forward, Vector3.Up) * trackWidth;

                // Create a vertex to the left and right of the current position
                vertices[j++] = new VertexPositionNormalTexture(position - side,
                    Vector3.Up, new Vector2(0, trackLength));
                vertices[j++] = new VertexPositionNormalTexture(position + side,
                    Vector3.Up, new Vector2(1, trackLength));

                trackLength += length;
            }

            // Attach the end vertices to the beginning to close the loop
            vertices[vertices.Length - 1].Position = vertices[1].Position;
            vertices[vertices.Length - 2].Position = vertices[0].Position;

            // For each vertex...
            for (int i = 0; i < vertices.Length; i++)
            {
                // Bring the UV's Y coordinate back to the [0, 1] range
                vertices[i].TextureCoordinate.Y /= trackLength;

                // Tile the texture along the track
                vertices[i].TextureCoordinate.Y *= textureRepetitions;
            }

            return vertices;
        }

        int[] createIndices()
        {
            // Create indices
            numberIndices = (controlPoints.Count - 1) * 6;
            int[] indices = new int[numberIndices];

            int j = 0;

            // Create two triangles between every position
            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                int i0 = i * 2;

                indices[j++] = i0;
                indices[j++] = i0 + 1;
                indices[j++] = i0 + 2;
                indices[j++] = i0 + 2;
                indices[j++] = i0 + 1;
                indices[j++] = i0 + 3;
            }

            return indices;
        }

        // Returns the position on the track the given distance from the start,
        // and the forward direction at that point
        public Vector2 TracePath(float distance, out Vector2 direction)
        {
            // Remove extra laps
            while (distance >trackLength)
                distance -= trackLength;

            int i = 0;

            while (true)
            {
                // Find the index of the next and last position
                int last = MakeIndex(i - 1, controlPoints.Count - 1);
                int next = MakeIndex(i + 1, controlPoints.Count - 1);

                // Find the distance between this position and the next
                direction = controlPoints[next] - controlPoints[i];
                float length = direction.Length();

                // If the length remaining is greater than the distance to
                // the next position, keep looping. Otherwise, the
                // final position is somewhere between the current and next
                // position in the list
                if (length < distance)
                {
                    distance -= length;
                    i++;
                    continue;
                }

                // Find the direction from the last position to the current position
                Vector2 lastDirection = controlPoints[i] - controlPoints[last];
                lastDirection.Normalize();
                direction.Normalize();

                // Determine how far the position is between the current and next
                // positions in the list
                float amt = distance / length;

                // Interpolate the last and current direction and current and 
                // next position to find final direction and position
                direction = Vector2.Lerp(lastDirection, direction, amt);
                return Vector2.Lerp(controlPoints[i], controlPoints[next], amt);
            }
        }

        //public Vector2 TracePath(float distance, float shift, out Vector2 direction)
        //{
        //    while (distance > trackLength)
        //        distance -= trackLength;

        //    int i = 0;

        //    while (true)
        //    {
        //        // Find the index of the next and last position
        //        int last = MakeIndex(i - 1, controlPoints.Count - 1);
        //        int next = MakeIndex(i + 1, controlPoints.Count - 1);

        //        // Find the distance between this position and the next
        //        direction = controlPoints[next] - controlPoints[i];
        //        float length = direction.Length();

        //        // If the length remaining is greater than the distance to
        //        // the next position, keep looping. Otherwise, the
        //        // final position is somewhere between the current and next
        //        // position in the list
        //        if (length < distance)
        //        {
        //            distance -= length;
        //            i++;
        //            continue;
        //        }

        //        // Find the direction from the last position to the current position
        //        Vector2 lastDirection = controlPoints[i] - controlPoints[last];
        //        lastDirection.Normalize();
        //        direction.Normalize();

        //        // Determine how far the position is between the current and next
        //        // positions in the list
        //        float amt = distance / length;

        //        // Interpolate the last and current direction and current and 
        //        // next position to find final direction and position
        //        direction = Vector2.Lerp(lastDirection, direction, amt);
        //        Vector2 middle= Vector2.Lerp(controlPoints[i], controlPoints[next], amt);
        //        Vector3 left = trackVertices[2*i].Position;
        //        Vector3 right = trackVertices[2 * i + 1].Position;
        //        Vector3 nextleft = trackVertices[MakeIndex(2 * i + 2, trackVertices.Length)].Position;
        //        Vector3 nextright = trackVertices[MakeIndex(2 * i + 3, trackVertices.Length)].Position;
        //        //Vector3 ansDirection = left - right;
        //        //ansDirection.Normalize();

        //        Vector2 answer ;
        //        if (shift < 0)
        //            answer = new Vector2(shift + middle.X, middle.Y) + new Vector2(middle.X, left.Z - middle.Y);
        //        else
        //            answer = new Vector2(shift + middle.X, middle.Y) + new Vector2(middle.X, right.Z - middle.Y);
        //        //Console.Out.WriteLine("middle" + middle);
        //        //Console.Out.WriteLine("left" + left);
        //        //Console.Out.WriteLine("right" + right);
        //        //Console.Out.WriteLine("shift" + shift);
        //        //
        //        //return answer;
        //        if (shift > 0)
        //            return Vector2.Lerp((new Vector2(right.X, right.Z) + controlPoints[i])/2, (new Vector2(nextright.X, nextright.Z)+controlPoints[i])/2, amt);
        //        else
        //            return Vector2.Lerp((new Vector2(left.X, left.Z) +controlPoints[next])/2, (new Vector2(nextleft.X, nextleft.Z) + controlPoints[next])/2, amt);
        //    }
        //}

        public bool IsOnTrack(Vector3 v) //TODO
        {
            return true;
        }

        public void Draw(Matrix View, Matrix Projection)
        {
            // Set effect parameters
            effect.World = Matrix.Identity;
            effect.View = View;
            effect.Projection = Projection;
            effect.Texture = texture;
            effect.TextureEnabled = true;

            // Set the vertex and index buffers to the graphics device
            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;

            // Apply the effect
            effect.CurrentTechnique.Passes[0].Apply();
            // Draw the list of triangles
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                0, 0, numberVertices, 0, numberIndices / 3);
        }

    }
}
