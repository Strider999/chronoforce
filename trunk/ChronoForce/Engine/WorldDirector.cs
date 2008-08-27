#region File Description
//-----------------------------------------------------------------------------
// WorldDirector.cs
//
// Copyright (C) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statments
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ChronoForce.Base;
using ChronoForceData.Base;
using ChronoForceData.Character;
using ChronoForceData.Actions;
#endregion

namespace ChronoForce.Engine
{
    /// <summary>
    /// Handles moving objects around in the world, including where to move characters when
    /// they walk, NPC movements, and camera views.  Used for either regular movement or
    /// in-game cinematics.  This contains a separate slot array for moving multiple
    /// characters asynchronously.
    /// </summary>
    class WorldDirector : Director
    {
        #region Constants

        /// <summary>
        /// Maximum number of concurrent actions to run
        /// </summary>
        const int cMaxSlots = 10;

        const int cXAmount = 32;
        const int cYAmount = 32;

        // Constant vectors of movement directions
        readonly Vector2 cMoveUp = new Vector2(0, -cYAmount);
        readonly Vector2 cMoveDown = new Vector2(0, cYAmount);
        readonly Vector2 cMoveRight = new Vector2(cXAmount, 0);
        readonly Vector2 cMoveLeft = new Vector2(-cXAmount, 0);

        // Movement speed
        const int cMoveSpeed = 7;

        #endregion

        #region Fields

        // Array of action slots for rendering single actions, usually for moving
        // characters around the screen.  These will always execute and update the
        // moment it is added, allowing for multiple characters to move at the same
        // time or at intersecting times.
        List<ActionSlot> currentSlots = new List<ActionSlot>(cMaxSlots);

        // Local action slot that will be reused to move the main part around the place
        ActionSlot moveSlot = new ActionSlot();

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles removing the action slots from the list when it's done
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e">Contains the array number in the list</param>
        void SlotHandler(object o, EventArgs e)
        {
            // Loops through the list of slots and see which one finished and remove it
            for (int i = 0; i < currentSlots.Count; i++)
            {
                if (currentSlots[i].IsFinished)
                {
                    currentSlots.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// When the party is done moving, stop the animation
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        void PartyMoveHandler(object o, EventArgs e)
        {
            // TODO:  Maybe a better way of doing this?  Too many accessors
            moveSlot.Actor.Sprite.Motion = "Face";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if the party is moving.  If so, don't do any more movements until
        /// it finishes.
        /// </summary>
        public bool PartyMoving
        {
            get { return !moveSlot.IsFinished; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new single action to the list of actions.  If this exceeds the maximum
        /// number of concurrent actions, don't add it and throw a warning.
        /// </summary>
        /// <param name="slot">Single action to be added</param>
        public void AddActionSlot(ActionSlot slot)
        {
            if (currentSlots.Count < cMaxSlots)
            {
                currentSlots.Add(slot);
                slot.Complete += SlotHandler;
            }
            else
            {
                Console.WriteLine("[WorldDirector WARNING]: Cannot add another action slot.  Currently full.");
            }
        }

        /// <summary>
        /// Adds a single action to the list that moves the party in a given direction.
        /// </summary>
        /// <param name="direction">Direction to move the party</param>
        public void MoveParty(CharacterBase actor, MapDirection direction)
        {
            moveSlot.Action = ActionCommand.MoveTo;
            moveSlot.Actor = actor;
            moveSlot.IsAbsolute = false;
            moveSlot.Speed = cMoveSpeed;

            switch (direction)
            {
                case MapDirection.Up:
                    moveSlot.EndPosition = cMoveUp;
                    break;
                case MapDirection.Down:
                    moveSlot.EndPosition = cMoveDown;
                    break;
                case MapDirection.Left:
                    moveSlot.EndPosition = cMoveLeft;
                    break;
                case MapDirection.Right:
                    moveSlot.EndPosition = cMoveRight;
                    break;
            }

            // Mark that the moveSlot is active again
            moveSlot.Reset();
            moveSlot.Complete += PartyMoveHandler;
        }

        #endregion

        #region Updating

        /// <summary>
        /// Updates all the action slots as well as any other actions
        /// </summary>
        /// <param name="elapsed">Time since last update</param>
        public override void Update(int elapsed)
        {
            // Loops through all the current actions and update them
            for (int i = 0; i < currentSlots.Count; i++)
                currentSlots[i].Update(elapsed);

            // Update the party movement if any
            moveSlot.Update(elapsed);

            // Update any other scripts
            base.Update(elapsed);
        }

        #endregion
    }
}
