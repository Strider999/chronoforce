#region File Description
//-----------------------------------------------------------------------------
// ActionObjectWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ChronoForceData.Actions;
#endregion

namespace ChronoForcePipeline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// Writes the ActionObject data
    /// </summary>
    [ContentTypeWriter]
    public class ActionObjectWriter : ChronoForceWriter<ActionObject>
    {
        protected override void Write(ContentWriter output, ActionObject value)
        {
            // Write the object type
            output.Write(value.ObjectType);
        }
    }
}
