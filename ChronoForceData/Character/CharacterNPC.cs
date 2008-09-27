#region File Description
//-----------------------------------------------------------------------------
// CharacterNPC.cs
//
// Created by David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using ChronoForceData.Graphics;
using ChronoForceData.Actions;
using ChronoForceData.Base;
#endregion

namespace ChronoForceData.Character
{
    /// <summary>
    /// Holds the basic characteristics of an NPC.  It inherits from the character
    /// base, but does not have any ability to fight, so it simplifies the class since it
    /// doesn't need any stats other than scripted events, movement, and drawing the sprite.
    /// </summary>
    public class CharacterNPC : CharacterBase, ICloneable
    {
        #region Fields

        // True if the NPC doesn't move on the map
        bool npcMoves = false;
        // Max radius for the NPC to move around
        int maxRadius = 0;
        // Starting position of the NPC
        Point startingPos;
        // List of strings that the character can speak
        List<DialogData> dialogText;
        // Timer for moving the NPC.  This makes each NPC have their own timer so
        // they don't all move at the same time (unless we want them to)
        int npcTimer = 0;
        // List of points that are restricted and the NPC can't move onto
        List<Point> restrictedPos;
        // True if this NPC is a merchant
        bool isMerchant = false;

        #endregion

        #region Events

        /// <summary>
        /// Signals when an NPC is ready to move
        /// </summary>
        [ContentSerializerIgnore]
        public EventHandler<NumberEventArgs> Ready;

        /// <summary>
        /// Method for raising the Ready event.
        /// </summary>
        protected internal void OnReady()
        {
            if (Ready != null)
                Ready(this, new NumberEventArgs(this.ID));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Determines whether or not the NPC moves around the map
        /// </summary>
        public bool Moves
        {
            get { return npcMoves; }
            set { npcMoves = value; }
        }

        /// <summary>
        /// Max radius for the NPC to move around the map
        /// </summary>
        public int MaxRadius
        {
            get { return maxRadius; }
            set { maxRadius = value; }
        }

        /// <summary>
        /// Starting point on the map of the NPC
        /// </summary>
        /// <remarks>Note that this will copy the MapPosition from the XML as
        /// it is the same as the starting position.  This is needed to keep
        /// track of how far the NPC moves</remarks>
        [ContentSerializerIgnore]
        public Point StartingPosition
        {
            get { return startingPos; }
            set { startingPos = value; }
        }

        /// <summary>
        /// List of spoken dialog by the NPC
        /// </summary>
        public List<DialogData> DialogText
        {
            get { return dialogText; }
            set { dialogText = value; }
        }

        /// <summary>
        /// A list of restricted positions the NPC can't move to
        /// </summary>
        public List<Point> RestrictedPositions
        {
            get { return restrictedPos; }
            set { restrictedPos = value; }
        }

        /// <summary>
        /// Determines whether or not the NPC is a merchant
        /// </summary>
        public bool IsMerchant
        {
            get { return isMerchant; }
            set { isMerchant = value; }
        }

        /// <summary>
        /// Timer for when the NPC can act/move again
        /// </summary>
        [ContentSerializerIgnore]
        public int Timer
        {
            get { return npcTimer; }
            set { npcTimer = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Default contructor to generate a blank object
        /// </summary>
        public CharacterNPC()
        {
            // Does nothing
        }

        /// <summary>
        /// Copy contructor for the CharacterNPC
        /// </summary>
        /// <param name="source">CharacterNPC to copy</param>
        public CharacterNPC(CharacterNPC source)
        {
            Name = source.Name;
            Position = source.Position;
            Sprite = new AnimatingSprite(source.Sprite);

            npcMoves = source.Moves;
            maxRadius = source.MaxRadius;
            startingPos = source.StartingPosition;
            dialogText = source.DialogText;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks to see if the new position is somewhere the NPC can go.
        /// </summary>
        /// <param name="position">Position where the NPC will move to</param>
        /// <returns>True if the NPC can't move to the position</returns>
        public bool IsRestricted(Point position)
        {
            // Check to see if the position is a restricted position
            if (restrictedPos != null)
            {
                Point checkPos = restrictedPos.Find(
                    delegate(Point testPoint)
                    {
                        return (position == testPoint);
                    });

                if (checkPos != null)
                    return true;
            }

            // Check to see if it's walking out of range
            int distance = Math.Abs(position.X - startingPos.X) + Math.Abs(position.Y - startingPos.Y);
            if (distance > maxRadius)
                return true;

            // Otherwise, the NPC can move to the new position
            return false;
        }

        #endregion

        #region Updating

        /// <summary>
        /// Updates the timer and sprite for the NPC
        /// </summary>
        /// <param name="elapsed">Time since last update in milliseconds</param>
        public override void Update(int elapsed)
        {
            if (npcTimer > 0)
                npcTimer -= elapsed;

            // If the timer has reached zero, then signal that the NPC is ready to go again
            if (npcTimer <= 0)
            {
                OnReady();
            }

            // Update the base class
            base.Update(elapsed);
        }

        #endregion

        #region Cloning

        /// <summary>
        /// Clone implementation for NPCs
        /// </summary>
        /// <remarks>Used mostly for testing.</remarks>
        /// <returns>A duplicate NPC of the source</returns>
        public object Clone()
        {
            // Create a new NPC
            CharacterNPC npc = new CharacterNPC();

            npc.Name = Name;
            npc.Position = Position;
            npc.Sprite = new AnimatingSprite(Sprite);

            npc.Moves = Moves;
            npc.MaxRadius = MaxRadius;
            npc.StartingPosition = StartingPosition;
            npc.DialogText = DialogText;
            npc.RestrictedPositions = RestrictedPositions;
            npc.IsMerchant = IsMerchant;

            return npc;
        }

        #endregion

        #region Content Type Reader

        /// <summary>
        /// Reads a character npc from the pipeline
        /// </summary>
        public class CharacterNPCReader : ContentTypeReader<CharacterNPC>
        {
            /// <summary>
            /// Reads a CharacterBase object from the content pipeline.
            /// </summary>
            protected override CharacterNPC Read(ContentReader input,
                CharacterNPC existingInstance)
            {
                CharacterNPC npc = existingInstance;
                if (existingInstance == null)
                {
                    npc = new CharacterNPC();
                }

                // Basic character information
                input.ReadRawObject<CharacterBase>(npc as CharacterBase);

                // Store the starting position
                npc.StartingPosition = npc.MapPosition;

                // NPC specific information
                npc.Moves = input.ReadBoolean();
                npc.MaxRadius = input.ReadInt32();
                npc.DialogText = input.ReadObject<List<DialogData>>();
                npc.RestrictedPositions = input.ReadObject<List<Point>>();
                npc.IsMerchant = input.ReadBoolean();

                // Look through the dialog text and locate any empty speakers
                // Fill these with the NPC name
                foreach (DialogData data in npc.DialogText)
                {
                    if (data.Speaker == String.Empty)
                        data.Speaker = npc.Name;
                }

                return npc;
            }
        }

        #endregion
    }
}
