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
using ChronoForceData.Graphics;
#endregion

namespace ChronoForceData.Graphics
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
    public class AnimatingSprite
    {
        #region Constants

        readonly string[] cKeys = new string[6]{"WorldFaceBack", "WorldFaceFront", "WorldFaceLeft", "WorldWalkBack", 
                "WorldWalkFront", "WorldWalkLeft"};
        readonly int[,] cPoints = new int[6, 2] { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 0, 1 }, { 0, 2 }, { 0, 3 } };
        readonly int[] cFrames = new int[6] { 1, 1, 1, 4, 4, 4 };

        #endregion

        #region Fields
        // Container to hold all the world sprite frames
        Dictionary<string, Animation> sprites;
        // Holder for the current animation
        Animation currentSprite;
        // Strings for determining how to animate the sprite
        ActionString action = new ActionString();
        // Bool for determining if the sprite will be mirrored.  Used mainly
        // when the character is moving right since there's only a Left sprite
        bool isMirrored = false;
        // Timer for when to advance the frame
        int spriteTimer = 0;
        // Current frame of animation
        int currentFrameValue = 0;
        // Dimensions of a frame
        Point frameDimension;
        // Number of frames per row
        int framesPerRow;
        // Padding
        int padding;

        // Camera values for rendering the sprite in respect to the camera
        Vector2 cameraPositionValue = Vector2.Zero;
        float rotationValue = 0;
        Matrix rotationMatrix;
        Vector2 scaleValue = Vector2.One;
        float zoomValue = 0;

        // Center of the screen for rendering sprites (reference point in respect to the camera)
        Vector2 screenCenter;
        // Rectangle of the source sprite.  Will be reused whenever the animation is updated and
        // calculated based on the dimensions and current frame
        Rectangle sourceRect;

        // XML fields for reading from the file
        // String name of the texture to load from the content pipeline
        string textureName;
        // Type of texture being loaded.  This is between players, monsters, and NPCs
        string textureType;
        // TODO:  Need vectors and ints for texture size, animation frames, and offsets?  Or should
        // this be partially hard-coded as a standard?

        // Texture of the sprite
        Texture2D texture;

        // NOTE:  Need some sort of debugging?
        #endregion

        #region Properties

        #region Camera Properties

        /// <summary>
        /// Where the camera is currently positioned
        /// </summary>
        [ContentSerializerIgnore]
        public Vector2 CameraPosition
        {
            set { cameraPositionValue = value; }
            get { return cameraPositionValue; }
        }

        /// <summary>
        /// How the camera is currently rotated on the screen
        /// </summary>
        [ContentSerializerIgnore]
        public float CameraRotation
        {
            set { 
                rotationValue = value;
                rotationMatrix = Matrix.CreateRotationZ(rotationValue);
            }
            get { return rotationValue; }
        }

        /// <summary>
        /// What zoom level the camera is currently using
        /// </summary>
        [ContentSerializerIgnore]
        public float CameraZoom
        {
            set { zoomValue = value; }
            get { return zoomValue; }
        }

        /// <summary>
        /// Scale of the Texture being drawn. 1 is default and unchanged.
        /// </summary>
        [ContentSerializerIgnore]
        public Vector2 Scale
        {
            set { scaleValue = value; }
            get { return scaleValue; }
        }

        #endregion

        #region CharacterSprite Properties

        /// <summary>
        /// Filename of the texture for this sprite
        /// </summary>
        public string TextureName
        {
            get { return textureName; }
            set { textureName = value; }
        }

        /// <summary>
        /// Type of texture being loaded, whether player, monster, or NPC
        /// </summary>
        public string TextureType
        {
            get { return textureType; }
            set { textureType = value; }
        }

        /// <summary>
        /// Dimensions of a frame of animation
        /// </summary>
        public Point FrameDimension
        {
            get { return frameDimension; }
            set { frameDimension = value; }
        }

        /// <summary>
        /// Number of animation frames per row of the texture
        /// </summary>
        public int FramesPerRow
        {
            get { return framesPerRow; }
            set { framesPerRow = value; }
        }

        /// <summary>
        /// Padding on each frame
        /// </summary>
        public int Padding
        {
            get { return padding; }
            set { padding = value; }
        }

        /// <summary>
        /// Returns the texture that represents the character
        /// </summary>
        /// <remarks>This is only a temporary function for debugging</remarks>
        [ContentSerializerIgnore]
        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        /// <summary>
        /// Sets/gets what to draw for the character when overworld.
        /// </summary>
        [ContentSerializerIgnore]
        public ActionString Action
        {
            get { return action; }
            set { 
                action = value;
                UpdateAnimation();
            }
        }

        /// <summary>
        /// Specifies what sprite type this is, either World or Battle
        /// </summary>
        [ContentSerializer(Optional = true)]
        public string Type
        {
            get { return action.type; }
            set
            {
                action.type = value;
                UpdateAnimation();
            }
        }

        /// <summary>
        /// Specifies whether the sprite will be in motion. "Face" means the character
        /// stands still while "Walk" animates the character walking in the direction.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public string Motion
        {
            get { return action.motion; }
            set
            {
                action.motion = value;
                UpdateAnimation();
            }
        }

        /// <summary>
        /// Direction sprite to render
        /// </summary>
        [ContentSerializer(Optional = true)]
        public string Direction
        {
            get { return action.direction; }
            set
            {
                action.direction = value;
                UpdateAnimation();
            }
        }

        /// <summary>
        /// Dictionary holding animation data for the sprite
        /// </summary>
        public Dictionary<string, Animation> Sprites
        {
            get { return sprites; }
            set { sprites = value; }
        }

        /// <summary>
        /// True if the sprite render will be mirrored (mainly for displaying
        /// right movement)
        /// </summary>
        [ContentSerializerIgnore]
        public bool IsMirrored
        {
            get { return isMirrored; }
            set { isMirrored = value; }
        }

        /// <summary>
        /// Position of the center of the screen.  Should only be set when first loading
        /// the screen and any change to the resolution.
        /// </summary>
        [ContentSerializerIgnore]
        public Vector2 ScreenCenter
        {
            get { return screenCenter; }
            set { screenCenter = value; }
        }

        /// <summary>
        /// Obtains the current frame of animation
        /// </summary>
        [ContentSerializerIgnore]
        public int CurrentFrame
        {
            set { currentFrameValue = value; }
            get { return currentFrameValue; }

        }

        #endregion

        #endregion

        #region Initialization

        /// <summary>
        /// Default contructor
        /// </summary>
        public AnimatingSprite()
        {
            // Initialize the dictionary
            sprites = new Dictionary<string, Animation>();
            sourceRect = new Rectangle();

            // Place default values into the action string
            action.type = "World";
            action.motion = "Face";
            action.direction = "Front";
            action.fullName = "WorldFaceFront";
        }

        /// <summary>
        /// Constructor with a provided texture for debugging
        /// </summary>
        public AnimatingSprite(Texture2D character)
            : this()
        {
            texture = character;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the full name of the action and obtains the current animation based on
        /// the new full name.
        /// </summary>
        public void UpdateAnimation()
        {
            action.fullName = action.type + action.motion + action.direction;

            // Make sure the dictionary actually contains the name.  The only case when this
            // will happen is when loading the XML file and the dictionary isn't set yet.
            if (sprites.ContainsKey(action.fullName))
            {
                currentSprite = sprites[action.fullName];

                // Update the source rectangle
                int actualFrame = currentSprite.StartingFrame;
                sourceRect.Width = frameDimension.X;
                sourceRect.Height = frameDimension.Y;
                sourceRect.X = (actualFrame % framesPerRow) * (frameDimension.X + padding);
                sourceRect.Y = (actualFrame / framesPerRow) * (frameDimension.Y + padding);
            }
        }

        /**
        /// <summary>
        /// Loads the default sprites from the XML
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public bool LoadCharacterSprite()
        {
            // NOTE:  Load the six different sprites.  Note not 8 because Right is the same as
            // Left, only mirrored, which will be done in CharacterSprite.  The number of sprites
            // should be a parameter though and not hard-coded as 6.
            for (int i = 0; i < 6; i++)
            {
                // NOTE:  the default sprite is 32x48 px, but this should be a parameters in XML, not
                // hard-coded.  For testing, this will work, but need to change.
                AddWorldSprite(cKeys[i],
                    new AnimatedSprite(characterTexture, 32, 48, 2, 4, 4,
                        new Point(cPoints[i, 0], cPoints[i, 1]), cFrames[i]));
            }

            return true;
        }

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
        */
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

                // If this is the last frame, check to see if this animation loops.
                if (currentSprite.IsLoop)
                    currentFrameValue = (currentFrameValue + 1) % currentSprite.NumFrames;
                else
                    currentFrameValue = Math.Min(currentFrameValue++, currentSprite.NumFrames - 1);

                // Get the current source rectangle
                int actualFrame = currentFrameValue + currentSprite.StartingFrame;
                sourceRect.Width = frameDimension.X;
                sourceRect.Height = frameDimension.Y;
                sourceRect.X = (actualFrame % framesPerRow) * (frameDimension.X + padding);
                sourceRect.Y = (actualFrame / framesPerRow) * (frameDimension.Y + padding);
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

            /**
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
             */
                // TODO:  Need a safety check when accessing the dictionary since the string
                // may be invalid.
                //drawingSprite = worldSprites[action.fullName];

                Vector2 scale = Vector2.One;

                // Since position is a struct, we can change the values here without effecting the
                // actual position.  We need to do this to convert the coordinates to
                // camera coordinates.
                // First, scale the positions based on the sprite scale
                position.X *= scaleValue.X;
                position.Y *= scaleValue.Y;

                // Now, we get the camera position relative to the sprite's position
                Vector2.Subtract(ref cameraPositionValue, ref position,
                    out position);

                // Get the sprite's final size (note that scaling is done after
                // determining the position)
                Vector2.Multiply(ref scaleValue, zoomValue, out scale);

                // Update the positions with world positions before drawing
                // Note that the origin is the position and position is the screen center.
                // This is required to enable scaling and rotation about the center of the 
                // screen by drawing tiles as an offset from the center coordinate
                //drawingSprite.Position = screenCenter;
                //drawingSprite.Origin = position;
                //drawingSprite.Rotation = rotationValue;
                //drawingSprite.ScaleValue = scale;

                // If the sprite is facing right, set the effect to reflect the left facing one
                if (action.direction == "Right")
                    effect = SpriteEffects.FlipHorizontally;

                batch.Begin(blendMode, SpriteSortMode.Immediate, SaveStateMode.None);

                batch.Draw(texture, screenCenter, sourceRect, color, rotationValue, position, 
                    scaleValue, effect, 0f);

                batch.End();

                //drawingSprite.Draw(batch, color, blendMode, effect);
            //}
        }

        #endregion

        #region Content Type Reader

        /// <summary>
        /// Reads a character sprite from the pipeline
        /// </summary>
        public class CharacterSpriteReader : ContentTypeReader<AnimatingSprite>
        {
            /// <summary>
            /// Reads a CharacterSprite object from the content pipeline.
            /// </summary>
            protected override AnimatingSprite Read(ContentReader input,
                AnimatingSprite existingInstance)
            {
                AnimatingSprite charSprite = existingInstance;
                if (existingInstance == null)
                {
                    charSprite = new AnimatingSprite();
                }

                charSprite.TextureName = input.ReadString();

                // Load the texture and sprite
                charSprite.Texture = input.ContentManager.Load<Texture2D>(
                        System.IO.Path.Combine(@"Textures",
                        charSprite.TextureName));
                //charSprite.LoadCharacterSprite();

                charSprite.TextureType = input.ReadString();
                charSprite.FrameDimension = input.ReadObject<Point>();
                charSprite.FramesPerRow = input.ReadInt32();
                charSprite.Padding = input.ReadInt32();
                charSprite.Type = input.ReadString();
                charSprite.Motion = input.ReadString();
                charSprite.Direction = input.ReadString();

                // Read the dictionary of animations
                charSprite.Sprites = input.ReadObject<Dictionary<string, Animation>>();

                // After the animations are loaded, get the current animation to render
                charSprite.UpdateAnimation();

                return charSprite;
            }
        }

        #endregion
    }
}
