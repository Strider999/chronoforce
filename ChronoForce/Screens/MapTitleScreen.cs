#region File Description
//-----------------------------------------------------------------------------
// MapTitleScreen.cs
//
// Copyright (C) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ChronoForce.Base;
#endregion

namespace ChronoForce.Screens
{
    /// <summary>
    /// Displays a title text and sub title for new areas of a map
    /// </summary>
    public class MapTitleScreen : GameScreen
    {
        #region Constants

        // Offset from the center of the screen
        const int cOffset = 100;
        // Delay before moving to the next animation step
        const int cFrameDelay = 33;
        // Step size of the transitions
        const float cStepSize = 0.02f;
        // How long the text will stay on the screen before transitioning away,
        // in frames
        const int cDisplayCounter = 40;

        // How much to fade the backtexture
        const float cBackAlpha = 0.25f;

        #endregion

        #region Fields

        // Title of the map, or series of maps
        string title;
        // Sub title for the specific map (name, floor level, etc)
        string subtitle;
        // Offset position of the title animation, from 0-100%, 
        // where 0% is no offset (directly centered)
        float position = 1f;
        // Positions of the title and subtitle
        Vector2 titlePos = new Vector2(0, 55);
        Vector2 subtitlePos = new Vector2(0, 115);

        // Back texture to render behind the text to make it easier to see
        Texture2D backTexture;
        // Position to draw the backtexture (width and height will be modified in Draw)
        Rectangle backPosition = new Rectangle(0, 50, 1, 100);

        // Alpha for fading
        float alpha = 0f;

        // Delay timer for controlling when to move the text
        int delayTimer = 0;
        // Frame counter for how long the text will stay on the screen
        int frameCounter = 0;
        // Counter for transitions
        float transitionCounter = 0.3f;

        #endregion

        #region Initialization

        /// <summary>
        /// Makes the map title bar with title and subtitle
        /// </summary>
        /// <param name="titleArg">Name of the map or series of maps</param>
        /// <param name="messageArg">Name of the specific map (name, floor level, etc.)</param>
        public MapTitleScreen(string titleArg, string subtitleArg)
        {
            title = titleArg;
            subtitle = subtitleArg;
        }

        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent DialogBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            backTexture = content.Load<Texture2D>("blank");
        }


        #endregion

        #region Private Helper Functions

        /// <summary>
        /// Generates a step size based on a parabolic formula for smoother transitions
        /// </summary>
        /// <param name="x">Position on the parabola</param>
        /// <returns>Step size</returns>
        private float GetStepSize(float x)
        {
            return (x * x);
        }

        #endregion

        #region Update

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            delayTimer += gameTime.ElapsedGameTime.Milliseconds;

            if (delayTimer > cFrameDelay)
            {
                delayTimer -= cFrameDelay;

                // In the first stage, the alpha will be increasing and the frameCounter
                // hasn't moved, so we transition in
                if (alpha < 1 && frameCounter == 0)
                {
                    transitionCounter += cStepSize;

                    alpha = MathHelper.Clamp(alpha + GetStepSize(transitionCounter), 0, 1);
                    position = MathHelper.Clamp(position - GetStepSize(transitionCounter), 0, 1);
                }
                else if (alpha == 1 && frameCounter < cDisplayCounter)
                {
                    // In the second stage, everything is showing, so we let it stay
                    // up there for the default amount of time, cDisplayCounter
                    frameCounter++;
                }
                else
                {
                    transitionCounter -= cStepSize;

                    // In the third stage, we transition the text and graphics off
                    alpha = MathHelper.Clamp(alpha - GetStepSize(transitionCounter), 0, 1);
                    position = MathHelper.Clamp(position - GetStepSize(transitionCounter), -1, 1);

                    // Once the alpha is 0, we exit the screen
                    if (alpha == 0)
                        ExitScreen();
                }
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws the title at the top
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont titleFont = Fonts.TitleFont;
            SpriteFont subtitleFont = Fonts.GeneralFont;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            // Generate the background rectangle
            backPosition.Width = viewport.Width;

            // Get the sizes of each string
            float titleSize = titleFont.MeasureString(title).X;
            float subtitleSize = subtitleFont.MeasureString(subtitle).X;

            // Center the text horizontally while applying the position offset
            titlePos.X = ((viewport.Width - titleSize) / 2) - (position * cOffset);
            subtitlePos.X = ((viewport.Width - subtitleSize) / 2) + (position * cOffset);

            // Round all the calculations to prevent an aliasing issue that causes
            // fuzzy fonts on the screen
            titlePos.X = (float)Math.Round(titlePos.X);
            subtitlePos.X = (float)Math.Round(subtitlePos.X);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            // Render the faded back rectangle
            spriteBatch.Draw(backTexture, backPosition, new Color(200, 200, 200, (byte)(alpha * cBackAlpha * 255)) );

            // Render the strings
            spriteBatch.DrawString(titleFont, title, titlePos, new Color(255, 255, 255, (byte)(alpha * 255)));
            spriteBatch.DrawString(subtitleFont, subtitle, subtitlePos, new Color(255, 255, 255, (byte)(alpha * 255)));

            spriteBatch.End();
        }


        #endregion
    }
}