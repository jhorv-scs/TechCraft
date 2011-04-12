using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NewTake.view.blocks
{
    [Serializable]
    public struct VertexPositionDualTexture : IVertexType
    {
        Vector3 _position;
        Vector2 _textureCoordinate1;
        Vector2 _textureCoordinate2;
        float _aoWeight;

        public static readonly VertexElement[] VertexElements = new VertexElement[]
        { 
            new VertexElement(0,VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float)*3,VertexElementFormat.Vector2,  VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float)*5,VertexElementFormat.Vector2,  VertexElementUsage.TextureCoordinate, 1), 
            new VertexElement(sizeof(float)*7,VertexElementFormat.Single, VertexElementUsage.Color,0)   
             };

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(VertexElements);
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

        public VertexPositionDualTexture(Vector3 position, Vector2 textureCoordinate1, Vector2 textureCoordinate2, float aoWeight)
        {
            _position = position;
            _textureCoordinate1 = textureCoordinate1;
            _textureCoordinate2 = textureCoordinate2;
            _aoWeight = aoWeight;
        }


        public override String ToString()
        {
            return "(" + _position + "),(" + _textureCoordinate1 + "),(" + _textureCoordinate1 + ")";
        }

        public Vector3 Position { get { return _position; } set { _position = value; } }
        public Vector2 TextureCoordinate1 { get { return _textureCoordinate1; } set { _textureCoordinate1 = value; } }
        public Vector2 TextureCoordinate2 { get { return _textureCoordinate2; } set { _textureCoordinate2 = value; } }
        public float AOWeight { get { return _aoWeight; } set { _aoWeight = value; } }
        public static int SizeInBytes { get { return sizeof(float) * 8; } }
    }
}
