﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Race
{
    public class BillboardSystem
    {
        VertexBuffer verts;
        IndexBuffer ints;
        VertexPositionTexture[] particles;
        int[] indices;

        int nBillboards;
        Vector2 billboardSize;
        Texture2D texture;

        GraphicsDevice graphicsDevice;
        Effect effect;

        public bool EnsureOcclusion = true;

        public enum BillboardMode { Cylindrical, Spherical };
        public BillboardMode Mode = BillboardMode.Spherical;

        public BillboardSystem(GraphicsDevice graphicsDevice, 
            ContentManager content, Texture2D texture,
            Vector2 billboardSize, Vector3[] particlePositions)
        {
            this.nBillboards = particlePositions.Length;
            this.billboardSize = billboardSize;
            this.graphicsDevice = graphicsDevice;
            this.texture = texture;

            effect = content.Load<Effect>("BillboardEffect");

            generateParticles(particlePositions);
        }

        void generateParticles(Vector3[] particlePositions)
        {
            particles = new VertexPositionTexture[nBillboards * 4];
            indices = new int[nBillboards * 6];

            int x = 0;

            for (int i = 0; i < nBillboards * 4; i += 4)
            {
                Vector3 pos = particlePositions[i / 4];

                particles[i + 0] = new VertexPositionTexture(pos, new Vector2(0, 0));
                particles[i + 1] = new VertexPositionTexture(pos, new Vector2(0, 1));
                particles[i + 2] = new VertexPositionTexture(pos, new Vector2(1, 1));
                particles[i + 3] = new VertexPositionTexture(pos, new Vector2(1, 0));

                indices[x++] = i + 0;
                indices[x++] = i + 3;
                indices[x++] = i + 2;
                indices[x++] = i + 2;
                indices[x++] = i + 1;
                indices[x++] = i + 0;
            }

            verts = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture), 
                nBillboards * 4, BufferUsage.WriteOnly);
            verts.SetData<VertexPositionTexture>(particles);

            ints = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, 
                nBillboards * 6, BufferUsage.WriteOnly);
            ints.SetData<int>(indices);
        }

        void setEffectParameters(Matrix View, Matrix Projection, Vector3 Up, Vector3 Right)
        {
            effect.Parameters["ParticleTexture"].SetValue(texture);
            effect.Parameters["View"].SetValue(View);
            effect.Parameters["Projection"].SetValue(Projection);
            effect.Parameters["Size"].SetValue(billboardSize / 2f);
            effect.Parameters["Up"].SetValue(Mode == BillboardMode.Spherical ? Up : Vector3.Up);
            effect.Parameters["Side"].SetValue(Right);
        }

        public void Draw(Matrix View, Matrix Projection, Vector3 Up, Vector3 Right)
        {
            graphicsDevice.SetVertexBuffer(verts);
            graphicsDevice.Indices = ints;

            graphicsDevice.BlendState = BlendState.AlphaBlend;

            setEffectParameters(View, Projection, Up, Right);

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

            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

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
                4 * nBillboards, 0, nBillboards * 2);
        }
    }
}
