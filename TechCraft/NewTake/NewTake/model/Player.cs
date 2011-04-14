using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NewTake.view;
using NewTake.controllers;
using NewTake.model.tools;
using NewTake.model.types;

namespace NewTake.model
{
    public class Player
    {
        public readonly World world;


        public Vector3 position;
        public Vector3 velocity;
  
        public PositionedBlock? currentSelection;

        public PositionedBlock? currentSelectedAdjacent; // = where a block would be added with the add tool


        public Tool LeftTool;
        public Tool RightTool;
        //keep it stupid simple for now, left hand/mousebutton & right hand/mousebutton

        public Player(World world)
        {
            this.world = world;
            LeftTool = new BlockRemover(this);
            //LeftTool = new PowerDrill(this);
            
            
            RightTool = new BlockAdder(this);
        }

    }
}
