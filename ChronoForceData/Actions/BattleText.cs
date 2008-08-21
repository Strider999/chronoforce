#region File Description
//-----------------------------------------------------------------------------
// BattleText.cs
//
// Copyright (c) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace ChronoForceData.Actions
{
    /// <summary>
    /// Specifically handles rendering numbers/text on the battle field when a someone takes
    /// damage or is healed.
    /// </summary>
    public class BattleText
    {
        #region Constants
        // Default animation speed for the text
        const int cDefaultSpeed = 30;
        // Default time for the text to remain on the screen
        const int cDefaultTime = 500;
        // Animation parameters for rendering the text.  Offset determines how far up the
        // text will jump from the center while the scale determines how fast the text 
        // will grow to the proper size.  Note that the offset is parabolic to give the text
        // a more natural animation so it slows down near the peak.
        const int cAnimationFrames = 6;
        readonly float[] cDefaultOffset = new float[] { -8, -14, -16, -14, -8, 0 };
        #endregion

        #region Fields

        // Text to render
        string text;
        // Center position of the text
        Vector2 position;
        // Font color to use when rendering the text
        Color textColor = Color.White;
        // Animation speed
        int animationSpeed = cDefaultSpeed;
        // Animation position to determine how to render the text
        int animationPosition = 0;
        // Used to store the individual text positions for animating
        int[] textPositions;
        // Timer for keeping track of how long the text should be rendered
        int textTime = cDefaultTime;
        // Delay for animating the text
        int delayTimer = 0;
        // True if the text is now disappearing
        bool textDisappearing = false;
        // True if the text finished rendering
        bool textDone = false;
        // True if the battle text is animating on the screen.  This determines
        // whether or not to start rendering.
        bool isRendering = false;

        #endregion

        #region Events

        /// <summary>
        /// Signal for when the battle text has finished rendering the text
        /// </summary>
        public EventHandler<EventArgs> Finished;

        // Internal function for signaling the event
        protected internal void OnDone()
        {
            if (Finished != null)
                Finished(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Text to render on the screen
        /// </summary>
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        /// <summary>
        /// How long the text will remain on the screen before fading away in milliseconds
        /// </summary>
        public int TextTime
        {
            get { return textTime; }
            set { textTime = value; }
        }

        /// <summary>
        /// Determines the color the battle font will render in
        /// </summary>
        public Color TextColor
        {
            get { return textColor; }
            set { textColor = value; }
        }

        /// <summary>
        /// Center position where the text will be rendered on the screen
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// How fast the text will animate in milliseconds
        /// </summary>
        public int AnimationSpeed
        {
            get { return animationSpeed; }
            set { animationSpeed = value; }
        }

        /// <summary>
        /// Returns true if the battle text is currently rendering
        /// </summary>
        public bool IsRendering
        {
            get { return isRendering; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor with defaults
        /// </summary>
        public BattleText()
        {
            // Nothing for now
        }

        /// <summary>
        /// Constructor to load the text and position
        /// </summary>
        /// <param name="text">Actual text to display on the screen</param>
        /// <param name="position">Center position to render the text</param>
        public BattleText(string text, Vector2 position)
        {
            this.text = text;
            this.position = position;

            // Initialize the text positions
            textPositions = new int[text.Length];
        }

        /// <summary>
        /// Constructor to load the necessary variables and text with font color
        /// </summary>
        /// <param name="text">Actual text to display on the screen</param>
        /// <param name="position">Center position to render the text</param>
        /// <param name="color">Color of the text</param>
        public BattleText(string text, Vector2 position, Color color)
            : this(text, position)
        {
            textColor = color;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the animation of the battle text on the screen.
        /// </summary>
        public void Start()
        {
            // Reset everything, which will flag the update and draw to start
            delayTimer = 0;
            animationPosition = 0;
            textDone = false;
            textDisappearing = false;
            isRendering = true;
        }

        #endregion

        #region Drawing and Updating

        /// <summary>
        /// Updates the class with the amount of time passed since the last update.
        /// </summary>
        /// <param name="elapsed">Time since last update in milliseconds</param>
        public void Update(int elapsed)
        {
            if (isRendering)
            {
                delayTimer += elapsed;

                if (delayTimer >= animationSpeed)
                {
                    // If the text isn't done or the text is disappearing, advance the animation
                    // position and reset the delayTimer
                    if (!textDone || textDisappearing)
                    {
                        delayTimer -= animationSpeed;

                        // Update the animation position and make sure it's within bounds
                        animationPosition = (int)MathHelper.Clamp(++animationPosition, 0, cAnimationFrames - 1);

                        // If the animation position is far enough (i.e. the whole text is rendered)
                        // mark the text as done
                        if (animationPosition >= cAnimationFrames - 1)
                            textDone = true;
                    }
                }

                if (textDone && !textDisappearing)
                {
                    // When the text has been on the screen long enough, begin fading the text out
                    if (delayTimer >= textTime)
                    {
                        textDisappearing = true;
                        animationPosition = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Draws the text on the battle field.
        /// </summary>
        /// <param name="spriteBatch">Batch used for rendering the font</param>
        /// <param name="textFont">Font used for the text</param>
        public void Draw(SpriteBatch spriteBatch, SpriteFont textFont)
        {
            if (isRendering)
            {
                spriteBatch.Begin();

                // Make the origin the direct center of the text
                Vector2 origin = textFont.MeasureString(text) / 2;

                // Add the offset to the text
                Vector2 pos;
                if (textDisappearing)
                {
                    pos = new Vector2(position.X, position.Y + cDefaultOffset[cAnimationFrames - animationPosition - 1]);
                }
                else
                {
                    pos = new Vector2(position.X, position.Y + cDefaultOffset[animationPosition]);
                }

                // If the text isn't done, animate it coming out
                if (!textDone)
                {
                    // Draw the text for animating
                    spriteBatch.DrawString(textFont, text, pos, textColor, 0, origin,
                        1, SpriteEffects.None, 0);
                }
                else
                {
                    // Slowly fade the text out of the screen based on the animation speed
                    if (textDisappearing)
                    {
                        // Decrease the alpha every animation frame
                        byte alpha = (byte)(255 * (1 - (animationPosition / (float)(cAnimationFrames - 1))));
                        spriteBatch.DrawString(textFont, text, pos,
                            new Color(textColor.R, textColor.G, textColor.B, alpha),
                            0, origin, 1, SpriteEffects.None, 0);

                        // If the alpha is 0, the text is completely gone and should signal that
                        // it finished.
                        if (alpha == 0)
                        {
                            OnDone();
                            isRendering = false;
                        }
                    }
                    else
                    {
                        // Continuing drawing the text
                        spriteBatch.DrawString(textFont, text, pos, textColor, 0, origin,
                                               1, SpriteEffects.None, 0);
                    }

                }
                spriteBatch.End();
            }
        }

        #endregion
    }
}
