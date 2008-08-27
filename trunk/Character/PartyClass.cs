#region File Description
//-----------------------------------------------------------------------------
// PartyClass.cs
//
// Copyright (C) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using ChronoForce.Base;
#endregion

namespace ChronoForce.Character
{
    /// <summary>
    /// Stores information about all the characters in battle
    /// </summary>
    public class PartyClass
    {
        #region Fields
        List<CharacterBase> party = new List<CharacterBase>(ChronoConstants.cMaxPartyMembers);

        // True if the character in party is ready
        bool[] playerReady = new bool[ChronoConstants.cMaxPartyMembers];
        #endregion

        #region Events

        /// <summary>
        /// Signals when a party member is ready
        /// </summary>
        public EventHandler<NumberEventArgs> Ready;

        /// <summary>
        /// Method for raising the Ready event.
        /// </summary>
        protected internal void OnReady(int playerID)
        {
            // Set the ready flag for the character
            playerReady[playerID] = true;

            if (Ready != null)
                Ready(this, new NumberEventArgs(playerID));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Obtains the list contain party members
        /// </summary>
        public List<CharacterBase> Party
        {
            get { return party; }
            set { party = value; }
        }

        /// <summary>
        /// Obtains the array of bools to determine which player is ready
        /// </summary>
        public bool[] IsReady
        {
            get { return playerReady; }
            set { playerReady = value; }
        }

        /// <summary>
        /// Returns the number of characters in the party that are alive.
        /// </summary>
        public int NumberAlive
        {
            get
            {
                int counter = 0;
                for (int i = 0; i < party.Count; i++)
                {
                    if (party[i].HP > 0)
                        counter++;
                }

                return counter;
            }
        }

        /// <summary>
        /// Returns the number of characters in the party.
        /// </summary>
        public int Count
        {
            get { return party.Count; }
        }

        /// <summary>
        /// Gets/sets whether or not the party is in battle
        /// </summary>
        public bool InBattle
        {
            get { return party[0].InBattle; }
            set
            {
                for (int i = 0; i < party.Count; i++)
                    party[i].InBattle = value;
            }
        }
        #endregion

        #region Initialization

        /// <summary>
        /// Default contructor, doesn't do anything.
        /// </summary>
        public PartyClass()
        {

        }

        /// <summary>
        /// Constructor to add one player to the party.
        /// </summary>
        /// <param name="player">Player to have with created class</param>
        public PartyClass(CharacterBase player)
        {
            party.Add(player);
                
            // Gives the first player the ID and map the event
            party[0].ID = 0;
            party[0].Ready += ReadyHandler;
        }

        /// <summary>
        /// Constructor to add three separate players to the party
        /// </summary>
        /// <param name="first">First player to have with created class</param>
        /// <param name="second">Second player to have with created class</param>
        /// <param name="third">Third player to have with created class</param>
        public PartyClass(CharacterBase first, CharacterBase second, CharacterBase third)
        {
            party.Add(first);
            party.Add(second);
            party.Add(third);

            // Gives IDs to all the players and map them to the event handler
            for (int i = 0; i < party.Count; i++)
            {
                party[i].ID = i;
                party[i].Ready += ReadyHandler;
            }
        }

        /// <summary>
        /// Constructor with list of party members
        /// </summary>
        /// <param name="newParty">Party members to have in the created class</param>
        public PartyClass(List<CharacterBase> newParty)
        {
            party = newParty;

            // Map the new party members to the event handler
            for (int i = 0; i < party.Count; i++)
            {
                party[i].ID = i;
                party[i].Ready += ReadyHandler;
            }
        }

        /// <summary>
        /// Initializes the beginning battle parameters
        /// </summary>
        /// <param name="reset">True if everyone should start at 0 time</param>
        public void Initialize(bool reset)
        {
            for (int i = 0; i < party.Count; i++)
            {
                if (reset)
                    party[i].SetTime(0);
                else // Initialize time based on character speed
                    party[i].InitializeTime();

                // Also reset the flag for ready
                playerReady[i] = false;

                // Flag the party in battle
                party[i].InBattle = true;
            }
        }

        #endregion

        #region Event Handlers

        void ReadyHandler(object o, NumberEventArgs e)
        {
            OnReady(e.ID);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates information in the party
        /// </summary>
        /// <param name="elapsed">Time since last update</param>
        public void Update(int elapsed)
        {
            for (int i = 0; i < party.Count; i++)
                party[i].Update(elapsed);
        }
        /// <summary>
        /// Checks whether or not a specified character in party is alive
        /// </summary>
        /// <param name="id">Character in party to check</param>
        /// <returns>True if the character ID is still alive</returns>
        public bool IsAlive(int id)
        {
            // Bound check
            if (id < 0 || id >= party.Count)
                return false;

            return (party[id].HP > 0);
        }

        #endregion
    }
}
