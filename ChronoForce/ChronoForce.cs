#region File Description
//-----------------------------------------------------------------------------
// ChronoForce.cs
//
// Copyright (C) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChronoForce.Base;
using ChronoForce.Screens;
#endregion

namespace ChronoForce
{
    /// <summary>
    /// ChronoForce Class is the main class tha starts the program.  It loads
    /// the ScreenManager and this handles all the game logic.
    /// </summary>
    public class ChronoForce : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        // Handles transitions between screens
        ScreenManager screenManager;
        // Settings for the game
        public static ChronosSetting gameSettings;
        
        public ChronoForce()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Load the default settings
            gameSettings = new ChronosSetting();

            // NOTE:  This should be a dynamic resolution based on user settings
            // For now, leaving this on XBox friendly resolution.
            graphics.PreferredBackBufferWidth = gameSettings.WindowWidth;
            graphics.PreferredBackBufferHeight = gameSettings.WindowHeight;

            // Create the screen manager component.
            screenManager = new ScreenManager(this);

            Components.Add(screenManager);

            // Activate the first screens.
            // NOTE:  I need to create the screens before attempting to load any of them
            //screenManager.AddScreen(new BackgroundScreen());
            screenManager.AddScreen(new MainMenuScreen());
            //screenManager.AddScreen(new BattleScreen());
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            // Currently, there's nothing to initialize
            base.Initialize();
        }

        /// <summary>
        /// Load the graphical contents of the game, including the game engine
        /// </summary>
        protected override void LoadContent()
        {
            // Loading the game engine
            //gameEngine = new ChronosEngine(graphics.GraphicsDevice, Content);

            // Pass the game engine to the game screens
            //GameScreen.gameEngine = gameEngine;

            base.LoadContent();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // The real drawing happens inside the screen manager component.
            base.Draw(gameTime);
        }
    }

    #region Entry Point

    static class ChronoForceProgram
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (ChronoForce game = new ChronoForce())
            {
                game.Run();
            }
        }
    }
    #endregion
}
