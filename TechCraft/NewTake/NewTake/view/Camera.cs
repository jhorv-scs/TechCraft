using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace NewTake.view
{
    public abstract class Camera
    {

        #region inits

        private Matrix _view;
        private Matrix _projection;
        protected Vector3 _position = Vector3.Zero;

        private float _viewAngle = MathHelper.PiOver4;
        private float _nearPlane = 0.01f;
        private float _farPlane = WorldRenderer.FARPLANE;

        public readonly Viewport viewport;

        #endregion

        public Camera(Viewport viewport)
        {
            this.viewport = viewport;

        }

        public Matrix View
        {
            get { return _view; }
            protected set { _view = value; }
        }

        public Matrix Projection
        {
            get { return _projection; }
            protected set { _projection = value; }
        }

        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;

                CalculateView();
            }
        }

        protected virtual void CalculateProjection()
        {
            _projection = Matrix.CreatePerspectiveFieldOfView(_viewAngle, viewport.AspectRatio, _nearPlane, _farPlane);
        }

        protected virtual void CalculateView()
        {
        }

        public virtual void Initialize()
        {
            CalculateView();
            CalculateProjection();
        }

        public virtual void Update(GameTime gameTime)
        {
        }

    }
}
