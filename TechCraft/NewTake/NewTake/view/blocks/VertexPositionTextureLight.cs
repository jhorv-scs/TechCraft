using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NewTake.view.blocks
{
    [Serializable]
    public struct VertexPositionTextureLight : IVertexType
    {
        Vector3 _position;
        Vector2 _textureCoordinate1;
        float _sunLight;
        Vector3 _localLight;

        public static readonly VertexElement[] VertexElements = new VertexElement[]
        { 
            new VertexElement(0,VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float)*3,VertexElementFormat.Vector2,  VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float)*5,VertexElementFormat.Single, VertexElementUsage.Color,0),
            new VertexElement(sizeof(float)*6,VertexElementFormat.Vector3, VertexElementUsage.Color,1)   
             };

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(VertexElements);
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

        public VertexPositionTextureLight(Vector3 position, Vector2 textureCoordinate1, float sunLight, Vector3 localLight)
        {
            _position = position;
            _textureCoordinate1 = textureCoordinate1;
            _sunLight = sunLight;
            _localLight = localLight;
        }


        public override String ToString()
        {
            return "(" + _position + "),(" + _textureCoordinate1 + "),(" + _textureCoordinate1 + ")";
        }

        public Vector3 Position { get { return _position; } set { _position = value; } }
        public Vector2 TextureCoordinate1 { get { return _textureCoordinate1; } set { _textureCoordinate1 = value; } }
        public Vector3 LocalLight { get { return _localLight; } set { _localLight = value; } }
        public float SunLight { get { return _sunLight; } set { _sunLight = value; } }
        public static int SizeInBytes { get { return sizeof(float) * 8; } }
    }
}
