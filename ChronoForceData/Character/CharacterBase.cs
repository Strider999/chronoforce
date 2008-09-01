#region File Description
//-----------------------------------------------------------------------------
// CharacterBase.cs
//
// Created by David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ChronoForceData.Base;
using ChronoForceData.Graphics;
#endregion

namespace ChronoForceData.Character
{
    /// <summary>
    /// Defines characters and monsters with customizable skills and spells based on
    /// a loaded file.  The player class will inherit from this with more abilities and
    /// the ability to level.
    /// </summary>
    public class CharacterBase
    {
        #region Constants
        protected const float cTimeScale = 0.25f;
        // Scales how fast to animate the sprites
        protected const float cSpeedScale = 0.07f;
        #endregion

        #region Fields
        Vector2 position = Vector2.Zero;  // Position to draw the character

        // Sprite class for the character
        AnimatingSprite sprite;
        // Character name
        string name;
        // Character ID number, used for Parties and scripts
        int id;

        #endregion

        #region Properties

        /// <summary>
        /// Name of the character
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Sets/Gets the player ID used for position in a party and scripts
        /// </summary>
        [ContentSerializerIgnore]
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// The position where the character is rendered
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// The sprite representing the character
        /// </summary>
        public AnimatingSprite Sprite
        {
            get { return sprite; }
            set { sprite = value; }
        }

        /// <summary>
        /// Changes which sprite to render with a string key
        /// </summary>
        [ContentSerializerIgnore]
        public ActionString SpriteAction
        {
            get { return Sprite.Action; }
            set { Sprite.Action = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor loads default values into the attributes
        /// </summary>
        public CharacterBase()
        {
            name = "Unknonwn";
        }

        /// <summary>
        /// Constructor with default name and loads the provided sprite
        /// </summary>
        /// <param name="sprite">Sprite that represents the character</param>
        public CharacterBase(AnimatingSprite sprite)
            : this()
        {
            this.sprite = sprite;
        }

        /// <summary>
        /// Constructor that loads the specific name
        /// </summary>
        /// <param name="nameArg">Name of the character</param>
        public CharacterBase(string nameArg)
        {
            name = nameArg;
            id = -1;
        }

        /// <summary>
        /// Constructor that loads a specific name and sprite
        /// </summary>
        /// <param name="nameArg">Name of the character</param>
        /// <param name="sprite">Sprite that represents the character</param>
        public CharacterBase(string nameArg, AnimatingSprite sprite)
        {
            name = nameArg;
            id = -1;
            this.sprite = sprite;
        }

        #endregion

        #region Updates

        /// <summary>
        /// Updates the timer for the character and any time based restoration.
        /// </summary>
        /// <param name="elapsed">Time passed since last update in milliseconds</param>
        public virtual void Update(int elapsed)
        {
            // Update the sprite, whether in battle or on the overworld map
            sprite.Update(elapsed);
        }

        #endregion

        #region Drawing and Rendering

        /// <summary>
        /// Draws the character on the screen
        /// </summary>
        /// <param name="batch">Batch to use for drawing</param>
        /// <param name="color">Color tint of the character.  White is normal</param>
        /// <param name="blendMode">Any blendined required</param>
        public virtual void Draw(SpriteBatch batch, Color color, SpriteBlendMode blendMode)
        {
            // Passes it on to the internal sprite class for drawing
            sprite.Draw(batch, color, blendMode, position);
        }

        #endregion

        #region Content Type Reader

        /// <summary>
        /// Reads a character base from the pipeline
        /// </summary>
        public class CharacterBaseReader : ContentTypeReader<CharacterBase>
        {
            /// <summary>
            /// Reads a CharacterBase object from the content pipeline.
            /// </summary>
            protected override CharacterBase Read(ContentReader input,
                CharacterBase existingInstance)
            {
                CharacterBase charBase = existingInstance;
                if (existingInstance == null)
                {
                    charBase = new CharacterBase();
                }

                charBase.Name = input.ReadString();
                charBase.Position = input.ReadVector2();
                charBase.Sprite = input.ReadObject<AnimatingSprite>();

                return charBase;
            }
        }

        #endregion
    }
}