#region File Description
//-----------------------------------------------------------------------------
// ActionSlotWriter.cs
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

namespace ChronoForcePipline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// Writes the ActionScript data
    /// </summary>
    [ContentTypeWriter]
    public class ActionScriptWriter : ChronoForceWriter<ActionScript>
    {
        protected override void Write(ContentWriter output, ActionScript value)
        {
            // TODO:  Make the pipline actually work
            // Write the name of the script
            output.Write(value.Name);

            // Write the list of strings
            output.WriteObject<List<string>>(value.Commands);
        }
    }
}
