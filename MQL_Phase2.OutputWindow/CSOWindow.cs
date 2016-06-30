//------------------------------------------------------------------------------
// <copyright file="CSOWindow.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace MQL_Phase2.OutputWindow
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("5b1685db-b2fa-448f-b32c-83855cf7a162")]
    public class CSOWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CSOWindow"/> class.
        /// </summary>
        public CSOWindow() : base(null)
        {
            this.Caption = "CSOWindow";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            var newControl = new CSOWindowControl();
            this.Content = newControl;
        }
    }
}
