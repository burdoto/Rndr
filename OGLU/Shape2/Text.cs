﻿using System.Drawing;
using SharpGL;

namespace OGLU.Shape2
{
    public class Text : AbstractRenderObject
    {
        public Text(IGameObject gameObject, string text = "") : base(gameObject)
        {
            Content = text;
        }

        public Text(IGameObject gameObject, ITransform transform, string text = "") : base(gameObject, transform)
        {
            Content = text;
        }

        public string Content { get; set; }
        public Color FontColor { get; set; } = Color.Red;
        public string FontName { get; set; } = "Courier New";
        public float FontSize { get; set; } = 12;

        public override void Draw(OpenGL gl, ITransform camera)
        {
            gl.DrawText((int)Position.X, (int)Position.Y, FontColor.R, FontColor.G, FontColor.B, FontName, FontSize,
                Content);
        }
    }
}