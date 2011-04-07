﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace NewTake.model
{

    public class Chunk
    {
        public const byte CHUNK_XMAX = 16;
        public const byte CHUNK_YMAX = 128;
        public const byte CHUNK_ZMAX = 16;

        public static Vector3i SIZE = new Vector3i(CHUNK_XMAX, CHUNK_YMAX, CHUNK_ZMAX);

        public Block[, ,] Blocks;

        public readonly Vector3i Position;
        public readonly Vector3i Index;

        public bool dirty;
        public bool visible;
        public bool generated;

        private BoundingBox _boundingBox;

        public Chunk(Vector3i index)
        {
            dirty = true;
            Index = index;
            visible = true;
            generated = false;

            Position = new Vector3i(index.X * CHUNK_XMAX, index.Y * CHUNK_YMAX, index.Z * CHUNK_ZMAX);
            Blocks = new Block[CHUNK_XMAX, CHUNK_YMAX, CHUNK_ZMAX]; //TODO test 3d sparse impl performance and memory
            _boundingBox = new BoundingBox(new Vector3(Position.X, Position.Y, Position.Z), new Vector3(Position.X + CHUNK_XMAX, Position.Y + CHUNK_YMAX, Position.Z + CHUNK_ZMAX));

        }

        public BoundingBox BoundingBox
        {
            get { return _boundingBox; }
        }

        public bool outOfBounds(byte x, byte y, byte z)
        {
            return (x < 0 || x >= CHUNK_XMAX || y < 0 || y >= CHUNK_YMAX || z < 0 || z >= CHUNK_ZMAX);
        }

    }
}