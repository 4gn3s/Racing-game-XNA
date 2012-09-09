using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Race
{
    public class BillboardCross
    {
        // Vertex buffer and index buffer, particle
        // and index arrays
        VertexBuffer verts;
        IndexBuffer ints;
        VertexPositionTexture[] particles;
        int[] indices;

        // Billboard settings
        int nBillboards;
        Vector2 billboardSize;
        Texture2D texture;

        // GraphicsDevice and Effect
        GraphicsDevice graphicsDevice;
        Effect effect;

        public bool EnsureOcclusion = true;

        public BillboardCross(GraphicsDevice graphicsDevice, 
            ContentManager content, Texture2D texture,
            Vector2 billboardSize, Vector3[] particlePositions)
        {
            this.nBillboards = particlePositions.Length;
            this.billboardSize = billboardSize;
            this.graphicsDevice = graphicsDevice;
            this.texture = texture;

            effect = content.Load<Effect>("BillboardCrossEffect");

            generateParticles(particlePositions);
        }

        void generateParticles(Vector3[] particlePositions)
        {
            // Create vertex and index arrays
            particles = new VertexPositionTexture[nBillboards * 8];
            indices = new int[nBillboards * 12];

            int x = 0;

            // For each billboard...
            for (int i = 0; i < nBillboards * 8; i += 8)
            {
                Vector3 pos = particlePositions[i / 8];

                Vector3 offsetX = new Vector3(billboardSize.X / 2.0f, billboardSize.Y / 2.0f, 0);
                Vector3 offsetZ = new Vector3(0, offsetX.Y, offsetX.X);

                // Add 4 vertices per rectangle
                particles[i + 0] = new VertexPositionTexture(pos + new Vector3(-1, 1, 0) * 
                    offsetX, new Vector2(0, 0));
                particles[i + 1] = new VertexPositionTexture(pos + new Vector3(-1, -1, 0) * 
                    offsetX, new Vector2(0, 1));
                particles[i + 2] = new VertexPositionTexture(pos + new Vector3(1, -1, 0) * 
                    offsetX, new Vector2(1, 1));
                particles[i + 3] = new VertexPositionTexture(pos + new Vector3(1, 1, 0) * 
                    offsetX, new Vector2(1, 0));

                particles[i + 4] = new VertexPositionTexture(pos + new Vector3(0, 1, -1) * 
                    offsetZ, new Vector2(0, 0));
                particles[i + 5] = new VertexPositionTexture(pos + new Vector3(0, -1, -1) * 
                    offsetZ, new Vector2(0, 1));
                particles[i + 6] = new VertexPositionTexture(pos + new Vector3(0, -1, 1) * 
                    offsetZ, new Vector2(1, 1));
                particles[i + 7] = new VertexPositionTexture(pos + new Vector3(0, 1, 1) * 
                    offsetZ, new Vector2(1, 0));

                // Add 6 indices per rectangle to form four triangles
                indices[x++] = i + 0;
                indices[x++] = i + 3;
                indices[x++] = i + 2;
                indices[x++] = i + 2;
                indices[x++] = i + 1;
                indices[x++] = i + 0;

                indices[x++] = i + 0 + 4;
                indices[x++] = i + 3 + 4;
                indices[x++] = i + 2 + 4;
                indices[x++] = i + 2 + 4;
                indices[x++] = i + 1 + 4;
                indices[x++] = i + 0 + 4;
            }

            // Create and set the vertex buffer
            verts = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture), 
                nBillboards * 8, BufferUsage.WriteOnly);
            verts.SetData<VertexPositionTexture>(particles);

            // Create and set the index buffer
            ints = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, 
                nBillboards * 12, BufferUsage.WriteOnly);
            ints.SetData<int>(indices);
        }

        void setEffectParameters(Matrix View, Matrix Projection)
        {
            effect.Parameters["ParticleTexture"].SetValue(texture);
            effect.Parameters["View"].SetValue(View);
            effect.Parameters["Projection"].SetValue(Projection);
        }

        public void Draw(Matrix View, Matrix Projection)
        {
            // Set the vertex and index buffer to the graphics card
            graphicsDevice.SetVertexBuffer(verts);
            graphicsDevice.Indices = ints;

            graphicsDevice.BlendState = BlendState.AlphaBlend;

            graphicsDevice.RasterizerState = RasterizerState.CullNone;

            setEffectParameters(View, Projection);

            if (EnsureOcclusion)
            {
                drawOpaquePixels();
                drawTransparentPixels();
            }
            else
            {
                graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                effect.Parameters["AlphaTest"].SetValue(false);
                drawBillboards();
            }

            // Reset render states
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Un-set the vertex and index buffer
            graphicsDevice.SetVertexBuffer(null);
            graphicsDevice.Indices = null;
        }

        void drawOpaquePixels()
        {
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            effect.Parameters["AlphaTest"].SetValue(true);
            effect.Parameters["AlphaTestGreater"].SetValue(true);

            drawBillboards();
        }

        void drawTransparentPixels()
        {
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            effect.Parameters["AlphaTest"].SetValue(true);
            effect.Parameters["AlphaTestGreater"].SetValue(false);

            drawBillboards();
        }

        void drawBillboards()
        {
            effect.CurrentTechnique.Passes[0].Apply();

            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                nBillboards * 8, 0, nBillboards * 4);
        }
    }
}
