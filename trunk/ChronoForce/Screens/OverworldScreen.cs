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

        // DEBUG:  Director test.
        // NOTE:  Should the directors be in the actual game session/engine instead of
        // created with every screen?
        WorldDirector director = new WorldDirector();
        ActionString actionString;

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

            // DEBUG:  Load a character with default sprites that are hard coded.
            // NOTE:  These sprites should be loaded inside the CharacterSprite class
            // from a file.  This is for testing only.
            content = new ContentManager(ScreenManager.Game.Services, "Content");

            Texture2D textureSprite = content.Load<Texture2D>("lockesprite");
            CharacterSprite sprite = new CharacterSprite();
            
            // Default values for the loop
            string[] keys = new string[6]{"WorldFaceBack", "WorldFaceFront", "WorldFaceLeft", "WorldWalkBack", 
                "WorldWalkFront", "WorldWalkLeft"};
            int[,] points = new int[6, 2] { { 0, 0 }, { 1, 0 }, { 2, 0 }, { 0, 1 }, { 0, 2 }, { 0, 3 } };
            int[] frames = new int[6] { 1, 1, 1, 4, 4, 4 };
            // Load the six different sprites.  Note not 8 because Right is the same as
            // Left, only mirrored, which will be done in CharacterSprite
            for (int i = 0; i < 6; i++)
            {
                sprite.AddWorldSprite(keys[i], 
                    new AnimatedSprite(textureSprite, 32, 48, 2, 4, 4, 
                        new Point(points[i,0], points[i,1]), frames[i]));
            }

            worldSprite = new CharacterBase(sprite);
            worldSprite.Sprite.Type = "World";
            worldSprite.Sprite.Motion = "Face";
            worldSprite.Sprite.Direction = "Front";

            // DEBUG:  Load a map
            //gameEngine.mapEngine.loadMap("Maps\\mountaintest.map");
            MapEngine.LoadMapEngine("Maps\\largemaptest.map", ScreenManager.GraphicsDevice, content, worldSprite);
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
                // Update the character
                worldSprite.Update(gameTime.ElapsedGameTime.Milliseconds);

                // Update the director
                director.Update(gameTime.ElapsedGameTime.Milliseconds);
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

            // If there's no input, then the party group should remain still
            // TODO:  Stop animation
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
