#region File Description
//-----------------------------------------------------------------------------
// OverworldScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// Modified by David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ChronoForce.Base;
using ChronoForce.Engine;
using ChronoForce.Parser;
using ChronoForce.Character;
#endregion

namespace ChronoForce.Screens
{
    /// <summary>
    /// Gaming screen for overhead map, world travel, and dungeons.
    /// </summary>
    class OverworldScreen : GameScreen
    {
        #region Fields
        //public string mapFile;
        ContentManager content;
        MapEngine mapEngine;
        CharacterBase worldSprite;
        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public OverworldScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            // Store a local pointer to the map engine
            mapEngine = ScreenManager.GameEngine.Map;
            
            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();

            // DEBUG:  Load a character with default sprites that are hard coded.
            // NOTE:  These sprites should be loaded inside the CharacterSprite class
            // from a file.  This is for testing only.
            content = new ContentManager(ScreenManager.Game.Services, "Content");

            SpriteSheet sheet = new SpriteSheet(content.Load<Texture2D>("lockesprite"));
            CharacterSprite sprite = new CharacterSprite();

            // Default values for the loop
            string[] keys = new string[6]{"Back", "Front", "Left", "WalkBack", "WalkFront", "WalkLeft"};
            int[,] points = new int[6, 2] { { 0, 0 }, { 34, 0 }, { 68, 0 }, { 0, 50 }, { 0, 100 }, { 0, 150 } };
            int[] frames = new int[6] { 1, 1, 1, 4, 4, 4 };
            // Load the six different sprites.  Note not 8 because Right is the same as
            // Left, only mirrored, which will be done in CharacterSprite
            for (int i = 0; i < 6; i++)
            {
                sprite.AddWorldSprite(keys[i], 
                    new AnimatedSprite(sheet, 32, 48, 2, 4, 4, 
                        new Point(points[i,0], points[i,1]), frames[i]));
            }

            worldSprite = new CharacterBase();

            // DEBUG:  Load a map
            //gameEngine.mapEngine.loadMap("Maps\\mountaintest.map");
            mapEngine.LoadMap("Maps\\largemaptest.map");
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                // TODO: Handle inputs and change accordingly
                
            }
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input, int elapsed)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Pass the input to the engine for debugging controls
            mapEngine.processInput(input, elapsed / (float)1000);

            if (input.PauseKey)
            {
                // TODO: If they pressed pause key, display the world menu and player stats
                //ScreenManager.AddScreen(new PauseMenuScreen());
            }
            else if (input.IsNewKeyPress(Keys.F)) // DEBUG testing controls
            {
                string testString = "This is a test and only a test.  Hopefully, the splitter works correctly" +
                    " and performs or exceeds my expectations.  Go go parser!  Work!  And more text " +
                    "is being added to help add fluff and maybe break the code so I can fix it.";
                List<string> testList = DialogParser.ParseString(testString, ScreenManager.GeneralFont,
                    150, 4);
                ScreenManager.AddScreen(new DialogBoxScreen("Tester", testList,
                    DialogBoxScreen.DialogSpeed.Fast, 1000));
            }
            else if (input.IsNewKeyPress(Keys.G)) // DEBUG testing controls
            {
                List<string> testMessage = new List<string>();
                testMessage.Add("This is the first part\nof this message.");
                testMessage.Add("This is another part\nof this message.");
                testMessage.Add("Last part of this cool\nmessage!!");
                ScreenManager.AddScreen(new DialogBoxScreen("Tester", testMessage,
                    DialogBoxScreen.DialogSpeed.Medium, new Vector2(100,100), new Vector2(500, 400)));
            }
            else if (input.PressedDownKey)
            {
                // Character is moving down
                worldSprite.SpriteType = "Down";

            }
            else if (input.PressedUpKey)
            {

            }
            else if (input.PressedLeftKey)
            {

            }
            else if (input.PressedRightKey)
            {

            }
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.Black, 0, 0);

            // Draw the game engine and character sprite
            // NOTE:  We pass the sprite in so the sprite can be draw in between
            // map layers for additional effect.
            mapEngine.Draw(gameTime, worldSprite);

            // Any drawing for this scene?
            //spriteBatch.Begin();
            //spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
        }


        #endregion
    }
}
