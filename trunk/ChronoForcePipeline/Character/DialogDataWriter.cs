#region File Description
//-----------------------------------------------------------------------------
// DialogDataWriter.cs
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
using ChronoForceData.Character;
using ChronoForceData.Graphics;
#endregion

namespace ChronoForcePipeline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// Writes the DialogData data
    /// </summary>
    [ContentTypeWriter]
    public class DialogDataWriter : ChronoForceWriter<DialogData>
    {
        protected override void Write(ContentWriter output, DialogData value)
        {
            // Write the name of the speaker
            output.Write(value.Speaker);

            // Write the dialog
            output.Write(value.Dialog);
        }
    }
}
