using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Race
{
    public class Track
    {
        List<Vector2> controlPoints;

        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;

        Vector3[] trackVertices;
        int[] trackIndices;

        Vector3[] roadsideVertices;
        const float roadsideWidth = 50; //each side

        int numberVertices, numberIndices;

        GraphicsDevice graphicsDevice;
        BasicEffect effect;
        Texture2D texture;

        float trackLength; //total len 
        public float TrackLength
        {
            get { return trackLength; }
        }

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
            val = Math.Abs( val % maxVal ); 
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

            vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture),
                vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionNormalTexture>(vertices);

            trackVertices=new Vector3[vertices.Count()];
            for(int i=0; i<vertices.Count(); i++)
                trackVertices[i] = vertices[i].Position;

            int[] indices = createIndices();
            trackIndices = indices;

            //roadside
            VertexPositionNormalTexture[] v = createVertices(trackWidth + 2 * roadsideWidth, textureRepetitions);
            roadsideVertices = new Vector3[v.Count()];
            for (int i = 0; i < v.Count(); i++)
                roadsideVertices[i] = v[i].Position;

            indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits,
                indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData<int>(indices);
        }

        VertexPositionNormalTexture[] createVertices(float trackWidth, int textureRepetitions)
        {
            numberVertices = controlPoints.Count * 2;
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[numberVertices];

            int j = 0;
            trackLength = 0;

            //Vector3 currentNormal = Vector3.Up;//
            //float banking = 1.5f;//

            for (int i = 0; i < controlPoints.Count; i++)
            {
                int next = MakeIndex(i + 1, controlPoints.Count - 1);
                Vector3 position = new Vector3(controlPoints[i].X, 0, controlPoints[i].Y);
                Vector3 nextPosition = new Vector3(controlPoints[next].X, 0, controlPoints[next].Y);
                //int prev = MakeIndex(i - 1, controlPoints.Count - 1);//
                //Vector3 prevPosition = new Vector3(controlPoints[prev].X, 0, controlPoints[prev].Y);//
                Vector3 forward = nextPosition - position;
                //Vector3 backward = position - prevPosition;//
                //backward.Normalize();//
                float length = forward.Length();
                forward.Normalize();

                //Vector3 perpDir = Vector3.Cross(forward, backward);//
                //Vector3 centriDir = Vector3.Cross(forward, perpDir);//
                //currentNormal = currentNormal + Vector3.Up / banking + centriDir * banking;//
                //currentNormal.Normalize();
                //Vector3 sideDir = Vector3.Cross(currentNormal, forward);
                //sideDir.Normalize();
                //currentNormal = Vector3.Cross(forward, sideDir);

                Vector3 side = -Vector3.Cross(forward, Vector3.Up) * trackWidth;

                vertices[j++] = new VertexPositionNormalTexture(position - side,
                    Vector3.Up, new Vector2(0, trackLength));
                vertices[j++] = new VertexPositionNormalTexture(position + side,
                    Vector3.Up, new Vector2(1, trackLength));

                //vertices[j++] = new VertexPositionNormalTexture(position - sideDir * trackWidth,//
                //    currentNormal, new Vector2(0, trackLength));//
                //vertices[j++] = new VertexPositionNormalTexture(position + sideDir *trackWidth,//
                //    currentNormal, new Vector2(1, trackLength));//

                trackLength += length;
            }

            vertices[vertices.Length - 1].Position = vertices[1].Position; //end to the beginning stick - to close the loop
            vertices[vertices.Length - 2].Position = vertices[0].Position;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].TextureCoordinate.Y /= trackLength; // Y in [0,1]
                vertices[i].TextureCoordinate.Y *= textureRepetitions;
            }

            return vertices;
        }

        int[] createIndices()
        {
            numberIndices = (controlPoints.Count - 1) * 6;
            int[] indices = new int[numberIndices];
            int j = 0;

            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                int i0 = i * 2;
                //2 triangles between every position
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
            while (distance >trackLength)
                distance -= trackLength;//extra laps removing

            int i = 0;
            while (true)
            {
                int last = MakeIndex(i - 1, controlPoints.Count - 1);
                int next = MakeIndex(i + 1, controlPoints.Count - 1);

                direction = controlPoints[next] - controlPoints[i];
                float length = direction.Length();
                if (length < distance)
                {
                    distance -= length;
                    i++;
                    continue;
                }
                Vector2 lastDirection = controlPoints[i] - controlPoints[last];
                lastDirection.Normalize();
                direction.Normalize();

                float amt = distance / length;
                direction = Vector2.Lerp(lastDirection, direction, amt);
                Vector2 ans = Vector2.Lerp(controlPoints[i], controlPoints[next], amt);
                return ans;
            }
        }

        public bool IsOnTrack(Vector3 v) 
        {
            for (int i = 0; i < trackIndices.Length; i += 3)
            {
                Vector3 p1 = trackVertices[trackIndices[i]];
                Vector3 p2 = trackVertices[trackIndices[i+1]];
                Vector3 p3 = trackVertices[trackIndices[i+2]];
                if (PointInTriangle(ref p1, ref p2,ref p3,ref v))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsOnTrackOrRoadside(Vector3 v)
        {
            for (int i = 0; i < trackIndices.Length; i += 3)
            {
                Vector3 p1 = roadsideVertices[trackIndices[i]];
                Vector3 p2 = roadsideVertices[trackIndices[i + 1]];
                Vector3 p3 = roadsideVertices[trackIndices[i + 2]];
                if (PointInTriangle(ref p1, ref p2, ref p3, ref v))
                {
                    return true;
                }
            }
            return false;
        }

        // Determine whether a point P is inside the triangle ABC. 
        // assumes that P is coplanar with the triangle.
        public static bool PointInTriangle(ref Vector3 A, ref Vector3 B, ref Vector3 C, ref Vector3 P)
        {
            Vector3 u = B - A;
            Vector3 v = C - A;
            Vector3 w = P - A;
            Vector3 vCrossW = Vector3.Cross(v, w);
            Vector3 vCrossU = Vector3.Cross(v, u);

            if (Vector3.Dot(vCrossW, vCrossU) < 0)
                return false;

            Vector3 uCrossW = Vector3.Cross(u, w);
            Vector3 uCrossV = Vector3.Cross(u, v);

            if (Vector3.Dot(uCrossW, uCrossV) < 0)
                return false;

            float denom = uCrossV.Length();
            float r = vCrossW.Length() / denom;
            float t = uCrossW.Length() / denom;

            return (r <= 1 && t <= 1 && r + t <= 1);
        }

        public void Draw(Matrix View, Matrix Projection)
        {
            effect.World = Matrix.Identity;
            effect.View = View;
            effect.Projection = Projection;
            effect.Texture = texture;
            effect.TextureEnabled = true;

            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;

            effect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                0, 0, numberVertices, 0, numberIndices / 3);
        }

    }
}
