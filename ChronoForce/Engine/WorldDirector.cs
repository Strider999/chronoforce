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

        const int cXAmount = 40;
        const int cYAmount = 40;

        // Constant vectors of movement directions
        readonly Vector2 cMoveUp = new Vector2(0, -cYAmount);
        readonly Vector2 cMoveDown = new Vector2(0, cYAmount);
        readonly Vector2 cMoveRight = new Vector2(cXAmount, 0);
        readonly Vector2 cMoveLeft = new Vector2(-cXAmount, 0);

        // Movement speed
        const int cDefaultMoveFrame = 7;

        #endregion

        #region Fields

        // Array of action slots for rendering single actions, usually for moving
        // characters around the screen.  These will always execute and update the
        // moment it is added, allowing for multiple characters to move at the same
        // time or at intersecting times.
        List<ActionSlot> currentSlots = new List<ActionSlot>(cMaxSlots);

        // List of action slots for moving NPCs around the world
        List<ActionSlot> npcSlots = new List<ActionSlot>(cMaxSlots);

        // Local action slot that will be reused to move the main party around the place
        ActionSlot moveSlot = new ActionSlot();

        #endregion

        #region Event Handlers

        /// <summary>
        /// Signals when it finished an action slot.
        /// </summary>
        public EventHandler<EventArgs> SlotDone;

        /// <summary>
        /// Internal function for signalling the event
        /// </summary>
        protected internal void OnSlotDone()
        {
            if (SlotDone != null)
                SlotDone(this, EventArgs.Empty);
        }

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
                    OnSlotDone();
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
            // For some reason, it's better to move a character and reset the animation
            // instead of doing it by action as this causes no stuttering/glitching while
            // the action does for continuous movement for some reason
            CharacterBase player = (CharacterBase)moveSlot.Actor;
            player.Sprite.Motion = "Face";
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

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public WorldDirector()
        {
            // Loads the npcSlots with blank action slots
            for (int i = 0; i < cMaxSlots; i++)
                npcSlots.Add(new ActionSlot());
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
                Console.WriteLine("[WorldDirector INFO]: Adding " + slot.Action.ToString() + " to list.");
                slot.Complete += SlotHandler;
            }
            else
            {
                Console.WriteLine("[WorldDirector WARNING]: Cannot add another action slot.  Currently full.");
            }
        }

        /// <summary>
        /// Adds a single action to the list that moves the party in a given direction using the default speed.
        /// </summary>
        /// <param name="actor">Actor to move on the field</param>
        /// <param name="direction">Direction to move the party</param>
        public void MoveNPC(CharacterBase actor, MapDirection direction)
        {
            npcSlots[actor.ID].Action = ActionCommand.WalkTo;
            npcSlots[actor.ID].Actor = actor;
            npcSlots[actor.ID].IsAbsolute = false;
            npcSlots[actor.ID].Frames = 30;

            switch (direction)
            {
                case MapDirection.Up:
                    npcSlots[actor.ID].EndPosition = cMoveUp;
                    break;
                case MapDirection.Down:
                    npcSlots[actor.ID].EndPosition = cMoveDown;
                    break;
                case MapDirection.Left:
                    npcSlots[actor.ID].EndPosition = cMoveLeft;
                    break;
                case MapDirection.Right:
                    npcSlots[actor.ID].EndPosition = cMoveRight;
                    break;
            }

            // Mark that the npcSlot is active again
            npcSlots[actor.ID].Reset();
        }

        /// <summary>
        /// Adds a single action to the list that moves the party in a given direction using the default speed.
        /// </summary>
        /// <param name="actor">Actor to move on the field</param>
        /// <param name="direction">Direction to move the party</param>
        public void MoveParty(CharacterBase actor, MapDirection direction)
        {
            actor.Sprite.Motion = "Walk";

            moveSlot.Action = ActionCommand.MoveTo;
            moveSlot.Actor = actor;
            moveSlot.IsAbsolute = false;
            moveSlot.Frames = cDefaultMoveFrame;

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

        /// <summary>
        /// Adds a single action to the list that moves the party in a given direction using the specified speed.
        /// </summary>
        /// <param name="actor">Actor to move on the field</param>
        /// <param name="direction">Direction to move the party</param>
        /// <param name="speed">Number of frames to move the actor</param>
        public void MoveParty(CharacterBase actor, MapDirection direction, int frames)
        {
            moveSlot.Action = ActionCommand.WalkTo;
            moveSlot.Actor = actor;
            moveSlot.IsAbsolute = false;
            moveSlot.Frames = frames;

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
            //moveSlot.Complete += PartyMoveHandler;
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

            // Loops through all the npc actions and update them
            for (int i = 0; i < npcSlots.Count; i++)
                npcSlots[i].Update(elapsed);

            // Update the party movement if any
            moveSlot.Update(elapsed);

            // Update any other scripts
            base.Update(elapsed);
        }

        #endregion
    }
}