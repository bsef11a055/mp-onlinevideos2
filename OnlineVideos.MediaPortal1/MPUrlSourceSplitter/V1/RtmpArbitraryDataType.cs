﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnlineVideos.MediaPortal1.MPUrlSourceSplitter.V1
{
    /// <summary>
    /// Specifies type of arbitrary data for RTMP protocol.
    /// </summary>
    public enum RtmpArbitraryDataType
    {
        /// <summary>
        /// Boolean type.
        /// </summary>
        Boolean,

        /// <summary>
        /// Number type.
        /// </summary>
        Number,

        /// <summary>
        /// String type.
        /// </summary>
        String,

        /// <summary>
        /// Object type.
        /// </summary>
        Object,

        /// <summary>
        /// Null type.
        /// </summary>
        Null
    }
}
