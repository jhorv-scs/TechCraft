﻿using System;
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

        public Player(World world) {
            this.world = world;
        }

        public Vector3 position;
        public Vector3 velocity;

        
        //TODO ***** merge usetools + game1.checkSelectionBlock to have currentSelected and currentSelectedAdjacent

        public PositionedBlock? currentSelection;

        public PositionedBlock? currentSelectedAdjacent; // = where a block would be added with the add tool
        

        Tool Left = new BlockRemover();
        Tool Right = new BlockAdder(); 
        //keep it stupid simple for now, left hand/mousebutton & right hand/mousebutton

      
    }
}
