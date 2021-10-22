﻿using System;
using System.Diagnostics;
using System.Numerics;
using System.Timers;
using OpenGL_Util.Matrix;
using OpenGL_Util.Model;
using SharpGL;

namespace OpenGL_Util.Game
{
    public abstract class GameBase : Container, IDrawable
    {
        public const int TickTime = 20;

        protected GameBase() : this(new ShortLongMatrix())
        {
        }

        protected GameBase(GridMatrix gridMatrix) : this(new RenderMatrix(gridMatrix))
        {
        }

        protected GameBase(RenderMatrix renderMatrix)
        {
            RenderMatrix = renderMatrix;
            Main = this;
        }

        public static GameBase? Main { get; private set; }

        public static long TimeDelta { get; private set; }
        public bool Active { get; set; }
        public RenderMatrix RenderMatrix { get; }
        public GridMatrix Grid => RenderMatrix.Grid;
        public abstract ITransform Camera { get; }

        public void Draw(OpenGL gl, ITransform camera)
        {
            var cam = camera.Position;
            var look = cam + Vector3.Transform(-Vector3.UnitZ, camera.Rotation);
            gl.LookAt(cam.X, cam.Y, cam.Z,
                look.X, look.Y, look.Z,
                0, 1, 0);

            RenderMatrix.Draw(gl, Camera);
            /*
            foreach (var render in Children
                .SelectMany(it =>
                {
                    if (it is Container c)
                        return c.Children.Append(it);
                    return new [] { it };
                })
                .Select(it => {
                if (it is IDrawable draw) 
                    return draw;
                if (it is IGameObject go)
                    return go.RenderObject;
                return null;
            }).Where(it => it != null))
                render?.Draw(gl, camera);
                */
            gl.DrawText(5, 20, 255, 0, 0, "Courier New", 12, $"Tick: {TimeDelta}ms");
        }

        public override bool AddChild(IDisposable container)
        {
            if (container is IGameObject go)
                Grid[go.Position] = go;
            return base.AddChild(container);
        }

        public void Run()
        {
            try
            {
                if (!Load())
                    throw new Exception("Could not load game");
                if (!Enable())
                    throw new Exception("Could not enable game");
                Active = true;

                long start = 0;
                Timer timer = new Timer(TickTime);
                timer.Elapsed += (sender, args) =>
                {
                    try
                    {
                        Tick();
                        TimeDelta = DateTimeOffset.Now.ToUnixTimeMilliseconds() - start;
                        start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Exception occurred in tick, shutting down!\n" + e);
                        Active = false;
                    }

                    if (!Active)
                        timer.Dispose();
                };
                timer.Disposed += (sender, args) =>
                {
                    Disable();
                    Unload();
                };
                timer.Start();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Could not start Game tick timer\n" + e);
            }
        }
    }
}