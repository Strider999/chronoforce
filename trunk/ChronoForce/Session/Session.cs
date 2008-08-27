#region File Description
//-----------------------------------------------------------------------------
// Session.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// - Keeps track of all game states and
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using ChronoForceData.Character;
#endregion

namespace ChronoForce.Session
{
    class Session
    {
        #region Singleton

        /// <summary>
        /// The single Session instance that can be active at a time.
        /// </summary>
        private static Session singleton;

        #endregion

        #region Party


        /// <summary>
        /// The party that is playing the game right now.
        /// </summary>
        private PartyClass party;

        /// <summary>
        /// The party that is playing the game right now.
        /// </summary>
        public static PartyClass Party
        {
            get { return (singleton == null ? null : singleton.party); }
        }


        #endregion
    }
}
