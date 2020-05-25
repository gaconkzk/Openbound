/* 
 * Copyright (C) 2020, Carlos H.M.S. <carlos_judo@hotmail.com>
 * This file is part of OpenBound.
 * OpenBound is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or(at your option) any later version.
 * 
 * OpenBound is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with OpenBound. If not, see http://www.gnu.org/licenses/.
 */

using OpenBound.GameComponents.Level.Scene.Menu;
using OpenBound.Launcher;
using OpenBound.ServerCommunication;
using System;
using System.Windows.Forms;

namespace OpenBound
{
#if WINDOWS || LINUX || DEBUGSCENE
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            GameLauncher gameLauncher = new GameLauncher();

#if !DEBUGSCENE
            if (gameLauncher.ShowDialog() != DialogResult.OK)
                return;
#endif

#if DEBUGSCENE
            DebugScene.InitializeObjects();
#endif


            using (var game = new Game1())
            {
                game.Run();
            }

            ServerInformationBroker.Instance.StopServices();
        }
    }
#endif
}