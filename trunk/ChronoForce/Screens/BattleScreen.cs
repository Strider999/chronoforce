#region File Description
//-----------------------------------------------------------------------------
// BattleScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// Created by David Hsu
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
using ChronoForceData.Base;
using ChronoForceData.Character;
#endregion

namespace ChronoForce.Screens
{
    /// <summary>
    /// Main class to handle battle sequences in the game.
    /// </summary>
    class BattleScreen : GameScreen
    {
        #region Fields
        ContentManager content;
        // Battle director for rendering the sprites and actions
        BattleDirector battleDirector;
        // Battle Menu for characters
        BattleMenuScreen menuScreen;

        // DEBUG:  Load textures for battle.  The textures should be on one sheet
        Texture2D battleDialog;
        Texture2D battleMenu;
        Texture2D battleTimer;
        Texture2D redBox;
        Texture2D blueBox;

        // Font used for battle
        SpriteFont battleFont;
        #endregion

        #region Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public BattleScreen()
            : base()
        {

        }

        /// <summary>
        /// Load graphics content for the screen (in this case, the battle engine and all necessary classes).
        /// </summary>
        public override void LoadContent()
        {
            // Create the menu screen
            menuScreen = new BattleMenuScreen(new Vector2(10, 515));
            menuScreen.ScreenManager = this.ScreenManager;

            // Load the textures
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            battleDialog = content.Load<Texture2D>("battledialog");
            battleMenu = content.Load<Texture2D>("battlemenuback");
            battleTimer = content.Load<Texture2D>("battletime");
            redBox = content.Load<Texture2D>("redbox");
            blueBox = content.Load<Texture2D>("bluebox");

            // Load the character sprites
            CharacterSprite playerSprite = new CharacterSprite(redBox);
            CharacterSprite enemySprite = new CharacterSprite(blueBox);

            // DEBUG:  Create a dummy party
            List<CombatCharacter> testList = new List<CombatCharacter>(ChronoConstants.cMaxPartyMembers);
            CharacterStats testStat = new CharacterStats(1, 10, 10, 10, 10, 10, 10, 10);
            testStat.Speed = 50;
            PlayerClass testPlayer = new PlayerClass("Dummy", testStat, playerSprite, 0);
            testStat.Speed = 25;
            PlayerClass testPlayer2 = new PlayerClass("Dummy2", testStat, playerSprite, 0);
            testPlayer.HP = 25;
            testPlayer2.HP = 25;
            testList.Add(testPlayer);
            testList.Add(testPlayer2);
            PartyClass testParty = new PartyClass(testList);

            EnemyClass testEnemy = new EnemyClass("Enemy1", testStat, enemySprite);
            EnemyClass testEnemy2 = new EnemyClass("Enemy2", testStat, enemySprite);
            List<CombatCharacter> enemyList = new List<CombatCharacter>(ChronoConstants.cMaxPartyMembers);
            enemyList.Add(testEnemy);
            enemyList.Add(testEnemy2);
            PartyClass enemyParty = new PartyClass(enemyList);

            testParty.InBattle = true;
            enemyParty.InBattle = true;

            // Link events from the battle engine
            testParty.Ready += ReadyHandler;
            //enemyPlayer.Ready += EnemyReadyHandler;
            menuScreen.AttackSelected += AttackHandler;
            BattleEngine.Defeat += DefeatHandler;
            BattleEngine.Victory += VictoryHandler;

            // Initialize the engine
            BattleEngine.StartNewBattle(testParty, enemyParty);

            // Create the battle director
            battleDirector = new BattleDirector(ScreenManager.Arrow);
        }

        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles displaying the battle menu when the player is ready
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public void ReadyHandler(object o, NumberEventArgs e)
        {
            // DEBUG:  Check e
            Console.WriteLine("PlayerID = {0}", e.ID);

            // Show the menu for this player
            menuScreen.SetMenuVisible(e.ID, true);
        }

        /// <summary>
        /// Handles the attack command from the menu
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public void AttackHandler(object o, NumberEventArgs e)
        {
            // DEBUG:  For now reset the timer and make the menu disappear
            Console.WriteLine("Attacking with ID {0}", e.ID);

            // TODO:  Reset the timer after the attack animation is finished.
            // Need to create an attack queue.
            // Do damage against the enemy
            //int dmg = BattleEngine.PerformAttack(e.ID, e.Target, false);
            //BattleEngine.ResetTime(e.ID, false);
            BattleEngine.PlayerReady[e.ID] = false;
            battleDirector.CreateAttack(BattleEngine.PlayerPartyList[e.ID], BattleEngine.EnemyPartyList[e.Target]);
        }

        public void ActionDoneHandler(object o, NumberEventArgs e)
        {

        }

        /// <summary>
        /// On default, displays the game over screen.  In special cases, like during
        /// scripted events where you die, this can be over written to reroute the screen.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public void DefeatHandler(object o, EventArgs e)
        {
            Console.WriteLine("DEFEAT!!");
        }

        /// <summary>
        /// Displays victory dialog box and then advance to another screen if needed.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public void VictoryHandler(object o, EventArgs e)
        {
            Console.WriteLine("VICTORY!!");

            // Remove the selection arrow if it's there
            menuScreen.SelectingTarget = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the battle engine and other statuses.  Also determines which menus to display based
        /// on who's ready.
        /// </summary>
        /// <param name="gameTime">In-game time</param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // If the battle is still going, keep updating all the classes
            if (!BattleEngine.BattleOver)
            {
                // Update the battle director with the last information from the menu
                battleDirector.SelectingTarget = menuScreen.SelectingTarget;
                battleDirector.CurrentTarget = menuScreen.TargetID;

                // Update the battle director
                battleDirector.Update(gameTime.ElapsedGameTime.Milliseconds);

                // If the director is moving sprites, don't update the battle engine
                if (!battleDirector.HasAction)
                {
                    // Update battle information
                    BattleEngine.Update(gameTime.ElapsedGameTime.Milliseconds);
                }

                // Update menu information
                menuScreen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            }

            // Update information in the base class
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Handles input presses and passes it to the appropriate function
        /// </summary>
        /// <param name="input">Input to recognize keyboard/gamepad commands</param>
        /// <param name="elapsed">Time passed since last update in milliseconds</param>
        public override void HandleInput(InputState input, int elapsed)
        {
            // If the battle is over don't handle inputs for the menu.  The inputs
            // should be routed to the new screen (displaying victory or death).
            if (!BattleEngine.BattleOver)
            {
                // BattleMenuScreen class will handle navigating the menu
                menuScreen.HandleInput(input, elapsed);
            }

            // Process input in the battle engine for debugging purposes
            BattleEngine.processInput(input, elapsed / (float)1000);
        }

        /// <summary>
        /// Draws the game screen.
        /// </summary>
        /// <param name="gameTime">In-game time</param>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // Draw the battle field
            spriteBatch.Begin();

            // Draw the battle menus and displays
            spriteBatch.Draw(battleDialog,
                new Vector2(0, ChronosSetting.WindowHeight - battleDialog.Height), Color.White);

            // DEBUG:  Drawing dummy values
            int y = 515;
            for (int i = 0; i < BattleEngine.PartyCount; i++)
            {
                spriteBatch.DrawString(Fonts.BattleFont, BattleEngine.PlayerPartyList[i].Name, 
                    new Vector2(365, y), Color.White, 0,
                    Vector2.Zero, 1, SpriteEffects.None, 0);
                spriteBatch.DrawString(Fonts.BattleFont, "999", new Vector2(525, y), Color.White, 0,
                    Vector2.Zero, 1, SpriteEffects.None, 0);
                spriteBatch.DrawString(Fonts.BattleFont, "999", new Vector2(557, y + 4), Color.White, 0,
                    Vector2.Zero, 0.75f, SpriteEffects.None, 0);

                // DEBUG:  Test out the timer bar based on player timer
                spriteBatch.Draw(battleTimer, new Vector2(680, y),
                    new Rectangle(0, 0, (int)(BattleEngine.PlayerPartyList[i].TimerPercent * 100), 20), Color.White);

                // Advance the line
                y += 30;
            }

            spriteBatch.End();

            // Draw the menus
            menuScreen.Draw(gameTime);

            // Draw the sprites on the screen
            battleDirector.Draw(spriteBatch, SpriteBlendMode.None);

            // Draw the battle and any debugs
            BattleEngine.Draw(gameTime);

            // Draw battle text if any
            //for (int i = 0; i < ChronoConstants.cMaxPartyMembers; i++)
            //    battleTextList[i].Draw();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
        }

        #endregion

        #region Private Methods

        #endregion
    }
}
