﻿/*
 * Copyright 2013 Mikko Teräs and Niilo Säämänen.
 *
 * This file is part of Auremo.
 *
 * Auremo is free software: you can redistribute it and/or modify it under the
 * terms of the GNU General Public License as published by the Free Software
 * Foundation, version 2.
 *
 * Auremo is distributed in the hope that it will be useful, but WITHOUT ANY
 * WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR
 * A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with Auremo. If not, see http://www.gnu.org/licenses/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Auremo
{
    public class LinkMetadata : Playable
    {
        public LinkMetadata()
        {
        }

        public string Path
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public string Artist
        {
            get
            {
                return "";
            }
        }

        public string Album
        {
            get
            {
                return "";
            }
        }

        public bool IsLocal
        {
            get
            {
                return Path.StartsWith("local:");
            }
        }

        public bool IsSpotify
        {
            get
            {
                return Path.StartsWith("spotify:");
            }
        }

        public bool IsSoundCloud
        {
            get
            {
                return false;
            }
        }

        public string DisplayName
        {
            get
            {
                return Title;
            }
        }
    }
}
