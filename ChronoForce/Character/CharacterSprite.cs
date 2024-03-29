#region File Description
//-----------------------------------------------------------------------------
// CharacterSprite.cs
//
// Copyright (c) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ChronoForce.Engine;
using ChronoForce.Base;
#endregion

namespace ChronoForce.Character
{
    /// <summary>
    /// A collection of sprites and animations that represent the character.  The necessary
    /// information is loaded from the Content Manager
    /// </summary>
    public class CharacterSprite
    {
        #region Fields
        // Container to hold all the world sprites
        Dictionary<string, AnimatedSprite> worldSprites;
        // Container to hold all the battle sprites
        Dictionary<string, AnimatedSprite> battleSprites;
        // Strings for determining how to animate the character
        string worldType = "Front";
        string battleType = "Front";
        // Bool for determining where the character is to determine which
        // sprite to redner
        bool inBattle = false;
        // Bool for determining if the sprite will be mirrored.  Used mainly
        // when the character is moving right since there's only a Left sprite
        bool isMirrored = false;
        // Timer for when to advance the frame
        int spriteTimer = 0;

        // NOTE:  These should be sprite sheets and animated sprite sheets, but for now, leave it as
        // a single texture for testing purposes.
        Texture2D characterTexture;

        // Debugger for the CharacterSprite
        Debugger SpriteDebug = new Debugger("CharacterSprite", true);
        #endregion

        #region Properties

        /// <summary>
        /// DEBUG:  Returns the texture that represents the character
        /// </summary>
        /// <remarks>This is only a temporary function for debugging</remarks>
        public Texture2D CharacterTexture
        {
            get { return characterTexture; }
        }

        /// <summary>
        /// Sets/gets what to draw for the character when overworld.
        /// </summary>
        public string WorldType
        {
            get { return worldType; }
            set 
            { 
                // Special case:  If the direction is "right", make it left and
                // mirror the sprite
                if (value == "Right")
                {
                    worldType = "Left";
                    isMirrored = true;
                }
                else
                {
                    worldType = value;
                    isMirrored = false;
                }
            
            }
        }

        /// <summary>
        /// Sets/gets what to draw for the character when in battle.
        /// </summary>
        public string BattleType
        {
            get { return battleType; }
            set { battleType = value; }
        }

        /// <summary>
        /// True if the character is in battle and should render battle sprites.
        /// </summary>
        public bool InBattle
        {
            get { return inBattle; }
            set { inBattle = value; }
        }

        /// <summary>
        /// True if the sprite render will be mirrored (mainly for displaying
        /// right movement)
        /// </summary>
        public bool IsMirrored
        {
            get { return isMirrored; }
            set { isMirrored = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Default contructor
        /// </summary>
        public CharacterSprite()
        {
            // Initialize the dictionaries
            worldSprites = new Dictionary<string, AnimatedSprite>();
            battleSprites = new Dictionary<string, AnimatedSprite>();
        }

        /// <summary>
        /// Constructor with a provided texture for debugging
        /// </summary>
        public CharacterSprite(Texture2D character)
        {
            // Initialize the dictionaries
            worldSprites = new Dictionary<string, AnimatedSprite>();
            battleSprites = new Dictionary<string, AnimatedSprite>();

            characterTexture = character;
        }

        #endregion

        #region Public Methods

        // TODO:  Read in a specification file that has all the information for each sprite
        public bool LoadCharacterSprite(string filename)
        {
            // First, check to see if the filename has the right format
            if (!filename.Contains(".act"))
            {
                SpriteDebug.debugPrint("Filename doesn't end in .act");
                return false;
            }

            // Checks to make sure the file is there
            if (!File.Exists(filename))
            {
                string msg = "File " + filename + " doesn't exist";
                SpriteDebug.debugPrint(msg);
                return false;
            }

            // TODO:  Read a file and add them into the dictionaries
            return true;
        }

        /// <summary>
        /// Adds an animated sprite to the character for display in the overworld screen.
        /// </summary>
        /// <param name="key">Key to reference this animation</param>
        /// <param name="sprite">Animated sprite</param>
        public void AddWorldSprite(string key, AnimatedSprite sprite)
        {
            worldSprites.Add(key, sprite);
        }

        /// <summary>
        /// Adds an animated sprite to the character for display in the battle screen.
        /// </summary>
        /// <param name="key">Key to reference this animation</param>
        /// <param name="sprite">Animated sprite</param>
        public void AddBattleSprite(string key, AnimatedSprite sprite)
        {
            battleSprites.Add(key, sprite);
        }

        #endregion

        #region Updating and Drawing

        /// <summary>
        /// Update the sprite with the time passed since the last update for animating
        /// </summary>
        /// <param name="elapsed"></param>
        public void Update(int elapsed)
        {
            spriteTimer += elapsed;

            // If the timer passed the animation delay * 5 to make the animation run at 6fps,
            // update the frame.
            if (spriteTimer >= ChronoConstants.cAnimationDelay * 5)
            {
                spriteTimer -= ChronoConstants.cAnimationDelay * 5;

                // Increment the frame
                if (!inBattle)
                    worldSprites[worldType].IncrementAnimationFrame();
            }
        }

        /// <summary>
        /// Draw the sprite onto the screen
        /// </summary>
        /// <param name="batch">Sprite Batch used for drawing</param>
        /// <param name="color">Any color tinting, White if none</param>
        /// <param name="blendMode">Type of blending to use when drawing</param>
        /// <param name="position">Where to draw the sprite</param>
        public void Draw(SpriteBatch batch, Color color, SpriteBlendMode blendMode, Vector2 position)
        {
            SpriteEffects effect = SpriteEffects.None;

            // Checks to see what to draw depending on where the character is
            if (inBattle)
            {
                batch.Begin(blendMode, SpriteSortMode.BackToFront, SaveStateMode.None);

                // NOTE: For right now, just draw the texture for battles
                batch.Draw(characterTexture, position, new Rectangle(0, 0, characterTexture.Width, characterTexture.Height),
                    color, 0, Vector2.Zero, 1, SpriteEffects.None, 0f);
                batch.End();
            }
            else // Overworld sprites
            {
                // TODO:  Need a safety check when accessing the dictionary since the string
                // may be invalid.
                AnimatedSprite sprite = worldSprites[worldType];
                sprite.Position = position;

                // If the sprite is mirrored, set the effect to reflect it
                if (isMirrored)
                    effect = SpriteEffects.FlipHorizontally;

                sprite.Draw(batch, color, blendMode, effect);
            }
        }

        #endregion
    }
}
