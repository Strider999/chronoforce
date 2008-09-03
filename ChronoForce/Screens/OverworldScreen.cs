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
using ChronoForceData.Base;
using ChronoForce.Engine;
using ChronoForce.Parser;
using ChronoForceData.Character;
using ChronoForceData.Graphics;
#endregion

namespace ChronoForce.Screens
{
    /// <summary>
    /// Gaming screen for overhead map, world travel, and dungeons.
    /// </summary>
    class OverworldScreen : GameScreen
    {
        #region Constants
        /// <summary>
        /// How fast to move the character across the field in pixels
        /// </summary>
        const int cWalkingSpeed = 10;
        #endregion

        #region Fields
        //public string mapFile;
        ContentManager content;
        CharacterBase worldSprite;

        // Where the character is on the map
        Point characterPosition = Point.Zero;

        // True if we want the camera to follow the party
        bool followParty = true;

        // DEBUG:  Director test.
        // NOTE:  Should the directors be in the actual game session/engine instead of
        // created with every screen?
        WorldDirector director = new WorldDirector();

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
            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();

            content = new ContentManager(ScreenManager.Game.Services, "Content");

            // DEBUG:  Load a test sprite
            worldSprite = content.Load<CharacterBase>("TestSprite");
            worldSprite.Sprite.ScreenCenter = ChronosSetting.WindowSize / 2;

            // DEBUG:  Load a map
            //gameEngine.mapEngine.loadMap("Maps\\mountaintest.map");
            MapEngine.LoadMapEngine("Maps\\largemaptest.map", ScreenManager.GraphicsDevice, content, 
                worldSprite, director);
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            // Update the status messages
            MapEngine.UpdateDebugMsg(worldSprite, characterPosition);

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                // Update the engine
                MapEngine.Update(gameTime.ElapsedGameTime.Milliseconds);

                // Update the character
                worldSprite.Update(gameTime.ElapsedGameTime.Milliseconds);

                // Update the director
                director.Update(gameTime.ElapsedGameTime.Milliseconds);

                // If we're following the party, update the camera to match the party position
                if (followParty)
                {
                    MapEngine.MoveCamera(worldSprite.Position);
                }
            }
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input, int elapsed)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Pass the input to the engine for debugging controls
            MapEngine.processInput(input, elapsed / (float)1000);

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
                List<string> testList = DialogParser.ParseString(testString, Fonts.GeneralFont,
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
            else if (input.IsNewKeyPress(Keys.H)) // DEBUG testing controls
            {
                followParty = !followParty;
                MapEngine.AddDebugMsg("[H] Toggling follow party");
            }

            // Movement commands
            if (input.PressedDownKey)
            {
                if (!director.PartyMoving)
                {
                    // First, change the character to face that direction
                    worldSprite.Sprite.Motion = "Face";
                    worldSprite.Sprite.Direction = "Front";

                    // Next, Check to see if the character can move that direction
                    if (MapEngine.IsPassable(characterPosition, MapDirection.Down))
                    {
                        worldSprite.Sprite.Motion = "Walk";
                        characterPosition.Y++;
                        director.MoveParty(worldSprite, MapDirection.Down);
                    }
                }
            }
            if (input.PressedUpKey)
            {
                if (!director.PartyMoving)
                {
                    // Change the direction of the character
                    worldSprite.Sprite.Motion = "Face";
                    worldSprite.Sprite.Direction = "Back";

                    if (MapEngine.IsPassable(characterPosition, MapDirection.Up))
                    {
                        worldSprite.Sprite.Motion = "Walk";
                        characterPosition.Y--;
                        director.MoveParty(worldSprite, MapDirection.Up);
                    }
                }
            }
            if (input.PressedLeftKey)
            {
                if (!director.PartyMoving)
                {
                    // Change the direction of the character
                    worldSprite.Sprite.Motion = "Face";
                    worldSprite.Sprite.Direction = "Left";

                    if (MapEngine.IsPassable(characterPosition, MapDirection.Left))
                    {
                        worldSprite.Sprite.Motion = "Walk";
                        characterPosition.X--;
                        director.MoveParty(worldSprite, MapDirection.Left);
                    }
                }
            }
            if (input.PressedRightKey)
            {
                if (!director.PartyMoving)
                {
                    // Change the direction of the character
                    worldSprite.Sprite.Motion = "Face";
                    worldSprite.Sprite.Direction = "Right";

                    if (MapEngine.IsPassable(characterPosition, MapDirection.Right))
                    {
                        worldSprite.Sprite.Motion = "Walk";
                        characterPosition.X++;
                        director.MoveParty(worldSprite, MapDirection.Right);
                    }
                }
            }
        }

        #endregion

        #region Drawing

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
            MapEngine.Draw(gameTime, worldSprite);

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
        }


        #endregion

        #region Private Helper Functions


        #endregion
    }
}
