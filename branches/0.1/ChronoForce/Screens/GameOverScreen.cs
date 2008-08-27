#region File Description
//-----------------------------------------------------------------------------
// DeathScreen.cs
//
// Copyright (C) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ChronoForce.Base;
#endregion

namespace ChronoForce.Screens
{
    /// <summary>
    /// The game over screen appears when the party is wiped out or you lose the game
    /// in some way.  All it shows is a single image and goes back to the main menu.
    /// This screen is basically the same as the background screen, only
    /// </summary>
    class GameOverScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        Texture2D backgroundTexture;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameOverScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the Game class, the content
        /// would remain loaded forever.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            backgroundTexture = content.Load<Texture2D>("gameoverscreen");
        }


        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Handle Input

        /// <summary>
        /// Grabs any recognized user input and exits the screen.
        /// </summary>
        /// <param name="elapsed">Time passed since the last update in milliseconds</param>
        public override void HandleInput(InputState input, int elapsed)
        {
            // Checks for any button presses
            if (input.CancelKey || input.ConfirmKey)
                ExitScreen();
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Updates the background screen.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }


        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);
            byte fade = TransitionAlpha;

            spriteBatch.Begin(SpriteBlendMode.None);

            spriteBatch.Draw(backgroundTexture, fullscreen,
                             new Color(fade, fade, fade));

            spriteBatch.End();
        }


        #endregion
    }
}
