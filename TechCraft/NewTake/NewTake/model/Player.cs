using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NewTake.view;
using NewTake.controllers;
using NewTake.model.tools;

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

        
        //TODO ***** merge usetools + game1.checkSelectionBlock to have currentSelected and currentSelectedAdjacent
        
        Block currentSelectedBlock;
        Vector3i currentSelectedBlockPos;
        
        Block currentSelectedAdjacent; // = where a block would be added with the add tool
        Vector3i currentSelectedAdjacentPos;

        Tool Left = new BlockRemover();
        Tool Right = new BlockAdder(); 
        //keep it stupid simple for now, left hand/mousebutton & right hand/mousebutton

      
    }
}
