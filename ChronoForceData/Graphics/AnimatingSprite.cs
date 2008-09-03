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
    /// Sprite struct that holds three strings representing how to render the character/sprite.  For
    /// regular sprites, only type and direction is used.  For characters, all three are used.
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
    /// A collection of sprites and animations.  The necessary
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

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">AnimatingSprite to copy from</param>
        public AnimatingSprite(AnimatingSprite source)
        {
            sprites = new Dictionary<string, Animation>(source.Sprites);
            sourceRect = new Rectangle();

            textureName = source.TextureName;
            TextureType = source.TextureType;
            texture = source.Texture;
            frameDimension = source.FrameDimension;
            framesPerRow = source.FramesPerRow;
            padding = source.Padding;
            action.type = source.Type;
            action.motion = source.Motion;
            action.direction  = source.Direction;

            UpdateAnimation();
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
                sourceRect.Width = frameDimension.X;
                sourceRect.Height = frameDimension.Y;
                sourceRect.X = (currentSprite.ActualFrame % framesPerRow) * (frameDimension.X + padding);
                sourceRect.Y = (currentSprite.ActualFrame / framesPerRow) * (frameDimension.Y + padding);
            }
        }

        #endregion

        #region Updating and Drawing

        /// <summary>
        /// Update the sprite with the time passed since the last update for animating
        /// </summary>
        /// <param name="elapsed"></param>
        public void Update(int elapsed)
        {
            // If the sprite isn't looping or animating, no need to update anything
            if (currentSprite.IsFinished)
                return;

            spriteTimer += elapsed;

            // If the timer passed the animation delay * 5 to make the animation run at 6fps,
            // update the frame.
            if (spriteTimer >= ChronoConstants.cAnimationDelay * 5)
            {
                spriteTimer -= ChronoConstants.cAnimationDelay * 5;

                // Update the frame on the animation
                currentSprite.CurrentFrame++;

                // Get the current source rectangle
                sourceRect.Width = frameDimension.X;
                sourceRect.Height = frameDimension.Y;
                sourceRect.X = (currentSprite.ActualFrame % framesPerRow) * (frameDimension.X + padding);
                sourceRect.Y = (currentSprite.ActualFrame / framesPerRow) * (frameDimension.Y + padding);
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
            // First, check to see if the sprite is within the view of the camera.  If not, don't bother
            // rendering the sprite to speed up rendering
            if ((position.X + sourceRect.Width < CameraPosition.X - ScreenCenter.X) ||
                (position.X - sourceRect.Width > CameraPosition.X + ScreenCenter.X) ||
                (position.Y + sourceRect.Height < CameraPosition.Y - ScreenCenter.Y) ||
                (position.Y - sourceRect.Height > CameraPosition.Y + ScreenCenter.Y))
            {
                return;
            }

            SpriteEffects effect = SpriteEffects.None;

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

            // If the sprite is facing right, set the effect to reflect the left facing one
            if (action.direction == "Right")
                effect = SpriteEffects.FlipHorizontally;

            batch.Begin(blendMode, SpriteSortMode.Immediate, SaveStateMode.None);

            batch.Draw(texture, screenCenter, sourceRect, color, rotationValue, position, 
                scaleValue, effect, 0f);

            batch.End();
        }

        #endregion

        #region Content Type Reader

        /// <summary>
        /// Reads a character sprite from the pipeline
        /// </summary>
        public class AnimatingSpriteReader : ContentTypeReader<AnimatingSprite>
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
