using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NewTake.view;
using NewTake.controllers;

namespace NewTake.model
{
    public class Player
    {
        public readonly World world;

        public Player(World world) {
            this.world = world;
        }

        Vector3 position;
        Vector3 velocity;

        Tool Left;
        Tool Right; //keep it stupid simple for now, left hand/mousebutton & right hand/mousebutton

      
    }
}
