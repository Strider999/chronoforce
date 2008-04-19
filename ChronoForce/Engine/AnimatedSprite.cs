#region File Description
//-----------------------------------------------------------------------------
// AnimatedSprite.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.\
// Modified by David Hsu
// - Added a boolean to make the animation run only once or continuously
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace ChronoForce.Engine
{
    public class AnimatedSprite
    {
        #region Fields
        private SpriteSheet sheet;
        private Vector2 positionValue;
        private Vector2 originValue;
        private Vector2 scaleValue;
        private float rotationValue;
        private int currentFrameValue;
        private int numFrames;

        // If true, the animation loops.  By default, this is true.
        bool isLooping = true;
        #endregion

        #region Public Properties
        public Vector2 Position
        {
            set
            {
                positionValue = value;
            }
            get
            {
                return positionValue;
            }
        }

        public Vector2 Origin
        {
            set
            {
                originValue = value;
            }
            get
            {
                return originValue;
            }
        }

        public float Rotation
        {
            set
            {
                rotationValue = value;
            }
            get
            {
                return rotationValue;
            }
        }

        public Vector2 ScaleValue
        {
            set
            {
                scaleValue = value;
            }
            get
            {
                return scaleValue;
            }
        }
        public int CurrentFrame
        {
            set
            {
                if (value > (numFrames - 1))
                {
                    string message =
                        string.Format("{0} is an invalid value for CurrentFrame.  " +
                        "Valid values are from 0 to numFrames - 1 ({1})",
                        value, numFrames - 1);
                    throw new ArgumentOutOfRangeException("value", message);
                }
                currentFrameValue = value;
            }
            get
            {
                return currentFrameValue;
            }

        }

        /// <summary>
        /// Boolean to determine whether the animation loops or stops after one go
        /// </summary>
        public bool IsLooping
        {
            get { return isLooping; }
            set { isLooping = value; }
        }
        #endregion

        #region Contructors
        public AnimatedSprite(SpriteSheet spriteSheet, int frameWidth,
            int frameHeight, int padding, int rows, int columns,
            Point startFrame, int frames)
        {
            if (spriteSheet == null)
            {
                throw new ArgumentNullException("spriteSheet");
            }
            int spriteAreaHeight = (frameHeight + padding) * rows - padding;
            int spriteAreaWidth = (frameWidth + padding) * columns - padding;

            //first, make sure the sheet is possible
            if ((spriteAreaWidth > spriteSheet.Texture.Width) ||
                (spriteAreaHeight > spriteSheet.Texture.Height))
            {
                throw new ArgumentException(
                    "The layout specified is too large for the SpriteSheet."
                    );
            }

            sheet = spriteSheet;
            numFrames = frames;

            int startFrameIndex = startFrame.Y * columns + startFrame.X;


            //now auto-generate the animation data,
            //left to right, top to bottom.
            int frameIndex = 0;
            for (int i = startFrameIndex; i < (numFrames + startFrameIndex); i++)
            {
                int x = (i % columns);
                int y = (i / columns);
                int left = (x * (frameWidth + padding));
                int top = (y * (frameHeight + padding));

                top = top % spriteAreaHeight;

                sheet.AddSourceSprite(frameIndex,
                    new Rectangle(left, top, frameWidth, frameHeight));
                frameIndex++;
            }
        }

        /// <summary>
        /// Additional constructor that takes in an extra bool at the end to change
        /// whether or not this animated sprite loops.
        /// </summary>
        /// <param name="spriteSheet"></param>
        /// <param name="frameWidth"></param>
        /// <param name="frameHeight"></param>
        /// <param name="padding"></param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <param name="startFrame"></param>
        /// <param name="frames"></param>
        /// <param name="loops"></param>
        public AnimatedSprite(SpriteSheet spriteSheet, int frameWidth, 
            int frameHeight, int padding, int rows, int columns,
            Point startFrame, int frames, bool loops)
            : this(spriteSheet, frameWidth, frameHeight, padding, rows, 
                   columns, startFrame, frames)
        {
            isLooping = loops;
        }
        #endregion

        #region Methods
        public void IncrementAnimationFrame()
        {
            // If this is the last frame, check to see if this animation loops.
            // If not, then 
            if (isLooping)
                currentFrameValue = (currentFrameValue + 1) % numFrames;
            else
                currentFrameValue = Math.Min(currentFrameValue++, numFrames - 1);
        }

        public void Draw(SpriteBatch batch, Color color, SpriteBlendMode blendMode, SpriteEffects effects)
        {
            batch.Begin(blendMode, SpriteSortMode.Immediate, 
                SaveStateMode.None);
            batch.Draw(sheet.Texture, positionValue, sheet[currentFrameValue],
                color, rotationValue, originValue, scaleValue,
                effects, 0f);
            batch.End();
        }
        #endregion
    }
}
