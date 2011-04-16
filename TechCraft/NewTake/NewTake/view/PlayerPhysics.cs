using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NewTake.model;
using Microsoft.Xna.Framework.Input;

namespace NewTake.view
{
   public class PlayerPhysics
    {
       private readonly Player player;
       private readonly FirstPersonCamera camera;
       //private MouseState previousMouseState;       

       //TODO set constants 
       const float PLAYERJUMPVELOCITY = 6f;
       const float PLAYERGRAVITY = -15f;
       const float PLAYERMOVESPEED = 3.5f;

       public PlayerPhysics(PlayerRenderer playerRenderer)
       {
           this.player = playerRenderer.player;
           this.camera = playerRenderer.camera;
       }

       public void move(GameTime gameTime)
       {

           MouseState mouseState = Mouse.GetState();

           if (mouseState.MiddleButton==ButtonState.Pressed )
           {
               Vector3 footPosition = player.position + new Vector3(0f, -1.5f, 0f);
               //XXX fly mode
               if (player.world.BlockAt(footPosition).Solid &&  player.velocity.Y == 0)
               {
               player.velocity.Y = PLAYERJUMPVELOCITY;
               float amountBelowSurface = ((ushort)footPosition.Y) + 1 - footPosition.Y;
               player.position.Y += amountBelowSurface + 0.01f;
               }
           }
           UpdatePosition(gameTime);

           float headbobOffset = (float)Math.Sin(player.headBob) * 0.06f;
           camera.Position =  player.position + new Vector3(0, 0.15f + headbobOffset, 0);

       }
                
        private void UpdatePosition(GameTime gameTime)
        {
            player.velocity.Y += PLAYERGRAVITY * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 footPosition = player.position + new Vector3(0f, -1.5f, 0f);
            Vector3 headPosition = player.position + new Vector3(0f, 0.1f, 0f);
            
            //TODO _isAboveSnowline = headPosition.Y > WorldSettings.SNOWLINE;
            
            if (player.world.BlockAt(footPosition).Solid || player.world.BlockAt(headPosition).Solid)
            {
                BlockType standingOnBlock = player.world.BlockAt(footPosition).Type;
                BlockType hittingHeadOnBlock = player.world.BlockAt(headPosition).Type;

                // If we"re hitting the ground with a high velocity, die!
                //if (standingOnBlock != BlockType.None && _P.playerVelocity.Y < 0)
                //{
                //    float fallDamage = Math.Abs(_P.playerVelocity.Y) / DIEVELOCITY;
                //    if (fallDamage >= 1)
                //    {
                //        _P.PlaySoundForEveryone(InfiniminerSound.GroundHit, _P.playerPosition);
                //        _P.KillPlayer(Defines.deathByFall);//"WAS KILLED BY GRAVITY!");
                //        return;
                //    }
                //    else if (fallDamage > 0.5)
                //    {
                //        // Fall damage of 0.5 maps to a screenEffectCounter value of 2, meaning that the effect doesn't appear.
                //        // Fall damage of 1.0 maps to a screenEffectCounter value of 0, making the effect very strong.
                //        if (standingOnBlock != BlockType.Jump)
                //        {
                //            _P.screenEffect = ScreenEffect.Fall;
                //            _P.screenEffectCounter = 2 - (fallDamage - 0.5) * 4;
                //            _P.PlaySoundForEveryone(InfiniminerSound.GroundHit, _P.playerPosition);
                //        }
                //    }
                //}

                // If the player has their head stuck in a block, push them down.
                if (player.world.BlockAt(headPosition).Solid)
                {
                    int blockIn = (int)(headPosition.Y);
                    player.position.Y = (float)(blockIn - 0.15f);
                }

                // If the player is stuck in the ground, bring them out.
                // This happens because we're standing on a block at -1.5, but stuck in it at -1.4, so -1.45 is the sweet spot.
                if (player.world.BlockAt(footPosition).Solid)
                {
                    int blockOn = (int)(footPosition.Y);
                    player.position.Y = (float)(blockOn + 1 + 1.45);
                }

                player.velocity.Y = 0;

                // Logic for standing on a block.
                // switch (standingOnBlock)
                //  {
                //case BlockType.Jump:
                //    _P.playerVelocity.Y = 2.5f * JUMPVELOCITY;
                //    _P.PlaySoundForEveryone(InfiniminerSound.Jumpblock, _P.playerPosition);
                //    break;

                //case BlockType.Road:
                //    movingOnRoad = true;
                //    break;

                //case BlockType.Lava:
                //    _P.KillPlayer(Defines.deathByLava);
                //    return;
                //  }

                // Logic for bumping your head on a block.
                // switch (hittingHeadOnBlock)
                // {
                //case BlockType.Shock:
                //    _P.KillPlayer(Defines.deathByElec);
                //    return;

                //case BlockType.Lava:
                //    _P.KillPlayer(Defines.deathByLava);
                //    return;
                //}
            }

            // Death by falling off the map.
            //if (_P.playerPosition.Y < -30)
            //{
            //    _P.KillPlayer(Defines.deathByMiss);
            //    return;
            //}

            player.position += player.velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState kstate = Keyboard.GetState();

            Vector3 moveVector = new Vector3();

            if (kstate.IsKeyDown(Keys.Up))
            {
                moveVector += Vector3.Forward * 2;
            }
            if (kstate.IsKeyDown(Keys.Down))
            {
                moveVector += Vector3.Backward * 2;
            }
            if (kstate.IsKeyDown(Keys.Left))
            {
                moveVector += Vector3.Left * 2;
            }
            if (kstate.IsKeyDown(Keys.Right))
            {
                moveVector += Vector3.Right * 2;
            }

            //moveVector.Normalize();
            moveVector *= PLAYERMOVESPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 rotatedMoveVector = Vector3.Transform(moveVector, Matrix.CreateRotationY(camera.LeftRightRotation));

            // Attempt to move, doing collision stuff.
            if (TryToMoveTo(rotatedMoveVector, gameTime)) { }
            else if (!TryToMoveTo(new Vector3(0, 0, rotatedMoveVector.Z), gameTime)) { }
            else if (!TryToMoveTo(new Vector3(rotatedMoveVector.X, 0, 0), gameTime)) { }
        }

        private bool TryToMoveTo(Vector3 moveVector, GameTime gameTime)
        {
            // Build a "test vector" that is a little longer than the move vector.
            float moveLength = moveVector.Length();
            Vector3 testVector = moveVector;
            testVector.Normalize();
            testVector = testVector * (moveLength + 0.3f);

            // Apply this test vector.
            Vector3 movePosition = player.position + testVector;
            Vector3 midBodyPoint = movePosition + new Vector3(0, -0.7f, 0);
            Vector3 lowerBodyPoint = movePosition + new Vector3(0, -1.4f, 0);

            if (!player.world.BlockAt(movePosition).Solid && !player.world.BlockAt(lowerBodyPoint).Solid && ! player.world.BlockAt(midBodyPoint).Solid)
            {
                player.position = player.position + moveVector;
                if (moveVector != Vector3.Zero)
                {
                    player.headBob += 0.2;
                }
                return true;
            }

            // It's solid there, so while we can't move we have officially collided with it.
            BlockType lowerBlock = player.world.BlockAt(lowerBodyPoint).Type;
            BlockType midBlock = player.world.BlockAt(midBodyPoint).Type;
            BlockType upperBlock = player.world.BlockAt(movePosition).Type;

            // It's solid there, so see if it's a lava block. If so, touching it will kill us!
            //if (upperBlock == BlockType.Lava || lowerBlock == BlockType.Lava || midBlock == BlockType.Lava)
            //{
            //    _P.KillPlayer(Defines.deathByLava);
            //    return true;
            //}

            // If it's a ladder, move up.
            //if (upperBlock == BlockType.Ladder || lowerBlock == BlockType.Ladder || midBlock == BlockType.Ladder)
            //{
            //    _P.playerVelocity.Y = CLIMBVELOCITY;
            //    Vector3 footPosition = _P.playerPosition + new Vector3(0f, -1.5f, 0f);
            //    if (_P.blockEngine.SolidAtPointForPlayer(footPosition))
            //        _P.playerPosition.Y += 0.1f;
            //    return true;
            //}

            return false;
        }
    }
}
