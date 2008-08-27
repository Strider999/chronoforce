#region File Description
//-----------------------------------------------------------------------------
// BattleEngine.cs
//
// Copyright (C) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using ChronoForce.Base;
using ChronoForceData.Base;
using ChronoForceData.Character;
#endregion

namespace ChronoForce.Engine
{
    /// <summary>
    /// Handles battles and renders the screen accordingly and handles inputs for
    /// determining what the character actions are
    /// </summary>
    class BattleEngine
    {
        #region Singleton

        /// <summary>
        /// The single BattleEngine instance that can be active at a time.
        /// </summary>
        private static BattleEngine singleton;

        /// <summary>
        /// Check to see if there is a battle going on, and throw an exception if not.
        /// </summary>
        private static void CheckSingleton()
        {
            if (singleton == null)
            {
                throw new InvalidOperationException(
                    "There is no active battle at this time.");
            }
        }

        #endregion

        #region Constants

        #endregion

        #region Fields
        // Players and enemies in battle
        PartyClass playerParty;
        PartyClass enemyParty;

        // True if the battle is over
        bool battleOver = false;

        // Determines the background the battle should have
        int mapType = 0;

        // Flag for debug control
        bool debugOn;
        // Debugger for the battle engine
        Debugger battleDebug;
        // String for the debugger
        static string statusMsg;

        #endregion

        #region Events

        public event EventHandler<EventArgs> Initialized;

        /// <summary>
        /// Method for raising the Initialized event.
        /// </summary>
        protected internal void OnInitialized()
        {
            if (Initialized != null)
                Initialized(this, EventArgs.Empty);
        }

        /// <summary>
        /// Signals when the player party has won the battle.
        /// </summary>
        public static event EventHandler<EventArgs> Victory;

        /// <summary>
        /// Method for raising the Victory event.
        /// </summary>
        protected internal void OnVictory()
        {
            if (Victory != null)
                Victory(this, EventArgs.Empty);
        }

        /// <summary>
        /// Signals when the player party has no living member.
        /// </summary>
        public static event EventHandler<EventArgs> Defeat;

        /// <summary>
        /// Method for raising the Defeat event.
        /// </summary>
        protected internal void OnDefeat()
        {
            if (Defeat != null)
                Defeat(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the playerParty
        /// </summary>
        private static PartyClass PlayerParty
        {
            get 
            {
                CheckSingleton();
                return singleton.playerParty; 
            }
            set 
            {
                CheckSingleton();
                singleton.playerParty = value; 
            }
        }

        /// <summary>
        /// Returns the enemyParty
        /// </summary>
        private static PartyClass EnemyParty
        {
            get
            {
                CheckSingleton();
                return singleton.enemyParty; 
            }
            set
            {
                CheckSingleton();
                singleton.enemyParty = value; 
            }
        }

        /// <summary>
        /// Flag to turn on or off debug
        /// </summary>
        public static bool DebugOn
        {
            get { return singleton.debugOn; }
            set { singleton.debugOn = value; }
        }

        /// <summary>
        /// True if the player is ready and the menu should display.  Note
        /// this returns the list of players.
        /// </summary>
        public static bool[] PlayerReady
        {
            get
            {
                CheckSingleton();
                return singleton.playerParty.IsReady; 
            }
        }

        /// <summary>
        /// True if the enemy is ready and can attack.  Note this
        /// returns a list of enemies.
        /// </summary>
        public static bool[] EnemyReady
        {
            get
            {
                CheckSingleton();
                return singleton.enemyParty.IsReady; 
            }
        }

        /// <summary>
        /// Returns the number of enemies alive on the screen.
        /// </summary>
        public static int EnemyAliveCount
        {
            get
            {
                CheckSingleton();
                return singleton.enemyParty.NumberAlive; 
            }
        }

        /// <summary>
        /// Returns the number of players alive on the screen.
        /// </summary>
        public static int PlayerAliveCount
        {
            get
            {
                CheckSingleton();
                return singleton.playerParty.NumberAlive; 
            }
        }

        /// <summary>
        /// Returns the total number of enemies in this battle.
        /// </summary>
        public static int EnemyCount
        {
            get
            {
                CheckSingleton();
                return singleton.enemyParty.Count; 
            }
        }

        /// <summary>
        /// Returns the total number of players in the party
        /// </summary>
        public static int PartyCount
        {
            get
            {
                CheckSingleton();
                return singleton.playerParty.Count; 
            }
        }

        /// <summary>
        /// Returns a list of the players in the party.
        /// </summary>
        public static List<CombatCharacter> PlayerPartyList
        {
            get
            {
                CheckSingleton();
                return singleton.playerParty.Party; 
            }
        }

        /// <summary>
        /// Returns a list of the enemies in battle
        /// </summary>
        public static List<CombatCharacter> EnemyPartyList
        {
            get
            {
                CheckSingleton();
                return singleton.enemyParty.Party; 
            }
        }

        /// <summary>
        /// Returns true if the battle is over, when the enemy loses or the
        /// player loses.
        /// </summary>
        public static bool BattleOver
        {
            get
            {
                CheckSingleton();
                return singleton.battleOver; 
            }
            private set { singleton.battleOver = value; }
        }

        /// <summary>
        /// Returns the battle debugger
        /// </summary>
        private static Debugger BattleDebug
        {
            get
            {
                CheckSingleton();
                return singleton.battleDebug; 
            }
        }

        #endregion

        #region Start New Battle

        public static void StartNewBattle(PartyClass playerParty, PartyClass enemyParty)
        {
            // If either are null, exit this class
            if (playerParty == null || enemyParty == null)
            {
                throw new InvalidOperationException("PlayerParty or EnemyParty is null!");
            }
            // check if we are already in battle
            if (singleton != null)
            {
                throw new InvalidOperationException("There can only be one battle at a time.");
            }

            singleton = new BattleEngine(playerParty, enemyParty);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor for the battle engine.
        /// </summary>
        private BattleEngine(PartyClass playerParty, PartyClass enemyParty)
        {       
            // If either are null, exit this class
            if (playerParty == null || enemyParty == null)
            {
                BattleDebug.debugPrint("PlayerParty or EnemyParty is null!  Not initializing...");
                return;
            }
            // If the party sizes aren't correct, also exit
            if (playerParty.Count <= 0 || enemyParty.Count <= 0)
            {
                BattleDebug.debugPrint("PlayerParty or EnemyParty do not have any combatants!");
                return;
            }

            this.playerParty = playerParty;
            this.enemyParty = enemyParty;

            // Set all times to 0
            // NOTE:  Time should be set based on how much speed a character has
            playerParty.Initialize(true);
            enemyParty.Initialize(true);

            // Set the character triggers to respond
            playerParty.Ready += ReadyHandler;
            enemyParty.Ready += EnemyReadyHandler;

            // Flag that the battle is going on and the timer should update
            for (int i = 0; i < playerParty.Count; i++)
            {
                playerParty.Party[i].BattleGoing = true;
            }

            for (int i = 0; i < enemyParty.Count; i++)
            {
                enemyParty.Party[i].BattleGoing = true;
            }

            // Initialize debug controls and variables
            debugOn = true;
            battleDebug = new Debugger("BattleDebug", debugOn);

            // Signal that the engine has initialized
            OnInitialized();
        }

        #endregion

        #region Event Functions

        /// <summary>
        /// Handles when a party member is ready
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ReadyHandler(object sender, NumberEventArgs e)
        {
            // TODO:  Generate the menu based on what position the character is
            // in the party
            // For now, make it in the first spot
            PlayerParty.IsReady[e.ID] = true;
        }

        /// <summary>
        /// Handles when an enemy is ready
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void EnemyReadyHandler(object sender, NumberEventArgs e)
        {
            // TODO:  Properly get the enemy to attack a random party member
            EnemyParty.IsReady[e.ID] = true;
        }

        #endregion

        #region Public Methods

        #region Player Menu Abilities

        /// <summary>
        /// Checks attack ability of player
        /// </summary>
        /// <param name="playerID">Player to check</param>
        /// <param name="isActive">Returns true if active</param>
        /// <returns>Returns true if the player has the ability to attack</returns>
        public static bool HasAttack(int playerID, out bool isActive)
        {
            isActive = true;

            // Cast the party member to the correct type
            PlayerClass tmpPlayer = (PlayerClass)PlayerParty.Party[playerID];

            if (tmpPlayer.HasAttack)
            {
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Checks skill ability of player
        /// </summary>
        /// <param name="playerID">Player to check</param>
        /// <param name="isActive">Returns true if active</param>
        /// <returns>Returns true if the player has the ability to use skills</returns>
        public static bool HasSkill(int playerID, out bool isActive)
        {
            isActive = true;

            // Cast the party member to the correct type
            PlayerClass tmpPlayer = (PlayerClass)PlayerParty.Party[playerID];

            if (tmpPlayer.HasSkill)
            {
                if (tmpPlayer.Locked)
                    isActive = false;
                return true;
            }
            else
                return false;
        }


        /// <summary>
        /// Checks time ability of player
        /// </summary>
        /// <param name="playerID">Player to check</param>
        /// <param name="isActive">Returns true if active</param>
        /// <returns>Returns true if the player has the ability to use time attacks</returns>
        public static bool HasTime(int playerID, out bool isActive)
        {
            isActive = true;

            // Cast the party member to the correct type
            PlayerClass tmpPlayer = (PlayerClass)PlayerParty.Party[playerID];

            if (tmpPlayer.HasTime)
            {
                if (tmpPlayer.Shattered)
                    isActive = false;
                return true;
            }
            else
                return false;
        }


        /// <summary>
        /// Checks anti ability of player
        /// </summary>
        /// <param name="playerID">Player to check</param>
        /// <param name="isActive">Returns true if active</param>
        /// <returns>Returns true if the player has the ability to anit-time abilities</returns>
        public static bool HasAnti(int playerID, out bool isActive)
        {
            isActive = true;

            // Cast the party member to the correct type
            PlayerClass tmpPlayer = (PlayerClass)PlayerParty.Party[playerID];

            if (tmpPlayer.HasAnti)
            {
                if (tmpPlayer.Shattered)
                    isActive = false;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Checks item ability of player
        /// </summary>
        /// <param name="playerID">Player to check</param>
        /// <param name="isActive">Returns true if active</param>
        /// <returns>Returns true if the player has the ability to use items</returns>
        public static bool HasItem(int playerID, out bool isActive)
        {
            isActive = true;

            // Cast the party member to the correct type
            PlayerClass tmpPlayer = (PlayerClass)PlayerParty.Party[playerID];

            if (tmpPlayer.HasItem)
            {
                return true;
            }
            else
                return false;
        }

        #endregion

        #region Battle Functions

        /// <summary>
        /// Does attack animation and calculates the amount of damage done by the characters
        /// </summary>
        /// <param name="playerID">Player being targeted/attacking</param>
        /// <param name="enemyID">Enemy bing targeted/attacking</param>
        /// <param name="enemyAttack">True if the enemy is attacking</param>
        /// <returns>The damage the player or enemy did</returns>
        public static int PerformAttack(int playerID, int enemyID, bool enemyAttack)
        {
            // If the enemy is attack, get the damage from them and perform
            // attack animation against the player.  Else, do the same for the
            // player againts the enemy
            if (enemyAttack)
            {
                return 0;
            }
            else
            {
                int damage = PlayerParty.Party[playerID].PerformAttack();
                Console.WriteLine("Player {0} does {1} damage to Enemey {2}!", playerID, damage, enemyID);

                // Do damage to the enemy
                EnemyParty.Party[enemyID].HP -= damage;

                return damage;
            }
        }

        #endregion

        #region Other

        /// <summary>
        /// Resets the timer back to zero.
        /// </summary>
        /// <param name="playerID">Character to reset time</param>
        /// <param name="enemy">True if resetting and enemy time</param>
        public static void ResetTime(int playerID, bool enemy)
        {
            // Set the time and reset flags
            if (enemy)
            {
                EnemyParty.Party[playerID].SetTime(0);
                EnemyReady[playerID] = false;
            }
            else
            {
                PlayerParty.Party[playerID].SetTime(0);
                PlayerReady[playerID] = false;
            }
        }

        /// <summary>
        /// Checks to see if a specific enemy is alive.
        /// </summary>
        /// <param name="id">Enemy to check</param>
        /// <returns>True if the enemy is alive</returns>
        public static bool IsEnemyAlive(int id)
        {
            return EnemyParty.IsAlive(id);
        }

        #endregion

        #endregion

        #region Drawing and Updating

        /// <summary>
        /// Updates the state of the engine.
        /// </summary>
        /// <param name="elapsed">How much time passed since the last update in milliseconds</param>
        public static void Update(int elapsed)
        {
            // Update debug status with enemy information
            statusMsg = "";
            for (int i = 0; i < EnemyParty.Count; i++)
            {
                statusMsg += "Enemy #" + i + ": ";
                statusMsg += "HP:" + EnemyParty.Party[i].HP + "/" + EnemyParty.Party[i].MaxHP + "  ";
                statusMsg += "MP:" + EnemyParty.Party[i].MP + "/" + EnemyParty.Party[i].MaxMP + "  ";
                statusMsg += "CF:" + EnemyParty.Party[i].CF + "/" + EnemyParty.Party[i].MaxCF + "\n";
            }
            BattleDebug.StatusMsg = statusMsg;

            // Checks the conditions of the battle.  If all the enemies are dead, signal
            // victory.  If the players are dead, signal defeat.
            if (EnemyAliveCount == 0)
            {
                BattleOver = true;
                singleton.OnVictory();
            }
            else if (PlayerAliveCount == 0)
            {
                BattleOver = true;
                singleton.OnDefeat();
            }

            // Update the character timers
            PlayerParty.Update(elapsed);
            EnemyParty.Update(elapsed);
        }

        /// <summary>
        /// Renders the battle field.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Draw(GameTime gameTime)
        {
            // Draw the debug messages
            BattleDebug.Draw(gameTime, ChronosEngine.ScreenManager.SpriteBatch);
        }

        #endregion

        #region Ending Battle

        /// <summary>
        /// Ensure that there is no battle happening right now.
        /// </summary>
        public static void ClearBattle()
        {
            // Clear the singleton
            if (singleton != null)
            {
                singleton = null;
            }
        }

        #endregion

        #region Debugging Methods

        /// <summary>
        /// Handles input and used for controlling specific parameters for debugging
        /// </summary>
        /// <param name="input">Input from the keyboard</param>
        /// <param name="elapsed">Time different from last update in seconds</param>
        public static void processInput(InputState input, float elapsed)
        {
            // DEBUG CONTROLS
            if (DebugOn)
            {
                // Toggle status message display
                if (input.IsNewKeyPress(Keys.NumPad5))
                {
                    BattleDebug.ShowStatusMsg = !BattleDebug.ShowStatusMsg;
                    BattleDebug.Add("[5] Toggling Status Display");
                }
            }
        }

        #endregion
    }
}