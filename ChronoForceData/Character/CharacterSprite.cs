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
using ChronoForceData.Base;
#endregion

namespace ChronoForceData.Character
{
    #region Struct

    /// <summary>
    /// Sprite struct that holds three strings representing how to render the character
    /// </summary>
    public struct ActionString
    {
        /// <summary>
        /// Specifies what sprite type this is, either World or Battle
        /// </summary>
        public string type;
        
        /// <summary>
        /// Specifies whether the sprite will be in motion. "Face" means the character
        /// stands still while "Walk" animates the character walking in the direction.
        /// </summary>
        public string motion;

        /// <summary>
        /// Direction sprite to render
        /// </summary>
        public string direction;

        /// <summary>
        /// Combined three strings of the struct
        /// </summary>
        public string fullName;
    }

    #endregion

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
        ActionString action = new ActionString();
        // Bool for determining where the character is to determine which
        // sprite to render
        bool inBattle = false;
        // Bool for determining if the sprite will be mirrored.  Used mainly
        // when the character is moving right since there's only a Left sprite
        bool isMirrored = false;
        // Timer for when to advance the frame
        int spriteTimer = 0;
        // Local copy for drawing the sprite in Draw
        AnimatedSprite drawingSprite;

        // Camera values for rendering the sprite in respect to the camera
        Vector2 originValue = Vector2.Zero;
        float rotationValue = 0;
        Vector2 scaleValue = Vector2.One;

        // NOTE:  These should be sprite sheets and animated sprite sheets, but for now, leave it as
        // a single texture for testing purposes.
        Texture2D characterTexture;

        // NOTE:  Need some sort of debugging?
        #endregion

        #region Properties

        #region Camera Properties

        public Vector2 Origin
        {
            set { originValue = value; }
            get { return originValue; }
        }

        public float Rotation
        {
            set { rotationValue = value; }
            get { return rotationValue; }
        }

        public Vector2 Scale
        {
            set { scaleValue = value; }
            get { return scaleValue; }
        }

        #endregion

        #region CharacterSprite Properties

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
        public ActionString Action
        {
            get { return action; }
            set 
            { 
                // Special case:  If the direction is "right", make it left and
                // mirror the sprite
                action = value;
                if (action.direction == "Right" )
                {
                    action.direction = "Left";
                    isMirrored = true;
                }
                else
                {
                    isMirrored = false;
                }
            }
        }

        /// <summary>
        /// Specifies what sprite type this is, either World or Battle
        /// </summary>
        public string Type
        {
            get { return action.type; }
            set
            {
                action.type = value;
                action.fullName = action.type + action.motion + action.direction;
            }
        }

        /// <summary>
        /// Specifies whether the sprite will be in motion. "Face" means the character
        /// stands still while "Walk" animates the character walking in the direction.
        /// </summary>
        public string Motion
        {
            get { return action.motion; }
            set
            {
                action.motion = value;
                action.fullName = action.type + action.motion + action.direction;
            }
        }

        /// <summary>
        /// Direction sprite to render
        /// </summary>
        public string Direction
        {
            get { return action.direction; }
            set
            {
                action.direction = value;

                // Special case:  If the direction is "right", make it left and
                // mirror the sprite
                if (action.direction == "Right")
                {
                    action.direction = "Left";
                    isMirrored = true;
                }
                else
                {
                    isMirrored = false;
                }

                action.fullName = action.type + action.motion + action.direction;
            }
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

            // Place default values into the action string
            action.type = "World";
            action.motion = "Face";
            action.direction = "Front";
        }

        /// <summary>
        /// Constructor with a provided texture for debugging
        /// </summary>
        public CharacterSprite(Texture2D character)
            : this()
        {
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
                Console.WriteLine("Filename doesn't end in .act");
                return false;
            }

            // Checks to make sure the file is there
            if (!File.Exists(filename))
            {
                string msg = "File " + filename + " doesn't exist";
                Console.WriteLine(msg);
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
                    worldSprites[action.fullName].IncrementAnimationFrame();
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
                drawingSprite = worldSprites[action.fullName];
                
                // Update the positions before drawing
                drawingSprite.Position = position;
                drawingSprite.Origin = originValue;
                drawingSprite.Rotation = rotationValue;
                drawingSprite.ScaleValue = scaleValue;

                // If the sprite is mirrored, set the effect to reflect it
                if (isMirrored)
                    effect = SpriteEffects.FlipHorizontally;

                drawingSprite.Draw(batch, color, blendMode, effect);
            }
        }

        #endregion
    }
}
