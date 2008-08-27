#region File Description
//-----------------------------------------------------------------------------
// BattleItemScreen.cs
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
#endregion

namespace ChronoForce.Screens
{
    /// <summary>
    /// Provides a window pop-up in battle with the list of selectable items/skills to use.
    /// </summary>
    class BattleItemScreen : GameScreen
    {
        #region Fields

        #endregion

        #region Events

        public EventHandler<EventArgs> Selected;

        #endregion

        #region Initialization

        public BattleItemScreen()
        {

        }

        #endregion
    }
}
