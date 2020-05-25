<h2>Configuration & Setup</h2>

This solution contains four applications that works together to make the game work.

1. Login Server
2. Lobby Server
3. Game Server
4. Game Application

Remember to build'em all beforehand.

In the current version is MANDATORY that all servers are opened in the same order as I have mentioned above.

All the necessary configuration (IP tables, public/local IPs and database address) can be edited by changing the configuration files that each application creates when are opened.

<h3>Configuring the Solution</h3>

1. First of all, you have to generate the config files, for that, all you have to do is open the LoginServer, LobbyServer and GameServer executables at least once and edit the generated .txt files.

The default paths for the configuration files are:

- Login Server/Config
- Lobby Server/Config
- Game Server/Config

Further instructions on how to proceed and a few specifics about configurations are written on the default configuration .txt files.
In case you have made a mess on any file, delete it and let the application create new ones!

Database connections are handled by Entity Framework. In case you prefer a free database, such as MySQL, all you have to do is download It's driver and change the contexts on
Openbound Network Object/OpenboundDatabaseContext. But it would require a few programming understanding.

2. Configure your database custom user to have admin privilleges. It is mandatory because of the EntityFramework's (flawed?) design.

This application creates the database by itself as long as the entered configurations are correct. Here is a checklist of things that you must do to ensure the server will work:
   - Check if the address is valid.
   - Check if the database is connectable.
   - Check if the database instance is running. 
   - Check if the user is valid and has admin privilleges.

The easiest way to check these things is to connect using VS's Server Explorer, SQL Server Management Studio or any other database interface and then perform some quick operations
like creating and then dropping a table on master using your newly creted user.

Currently compatible databases:

- SQL Server LocalDB (Installed automatically with VS Community 2019 or superior).
- SQL Server Express
- SQL Server (Recommended)

<h2>Different things from the original game:</h2>

<h3>Assets</h3>

Reminder: The following clients were datamined in order to acquire all necessary assets. Because of that, newer assets/mobiles/features aren't going to be incorporated into the game.
- MagicPunk (Non-GB game made by the same company)
- Beta
- WC (Season 1) (North American/European/South American/Korean clients)
- Season 2 (North American/South American clients)

1. Audio
   - Movement - A few mobiles IN THE ORIGINAL GAME have no sound when moving or when are stuck. To address this (not) issue, I've decided to add placeholders from several other sources. Here is the complete list containing general information:

     - Mobiles that does not make sound when move:
       - Aduka [3]
       - J. Frog [3]
       - Kalsiddon [3]
       - Knight [1]
       - Turtle [1]

     - Mobiles that does not make sound when are stuck or out of movement left:
       - Aduka [3]
       - A. Sate [1]
       - Dragon [2]
       - Grub [2]
       - Ice [2]
       - JD [3]
       - J. Frog [3]
       - Kalsiddon [3]
       - Knight [1]
       - Turtle [1]

Legend:
[1] - Sound currently reproduced is a placeholder that comes from a different source (more details on Assets.3) 
[2] - Sound currently reproduced is a placeholder that comes from another sound with a different pitch to fool player's ears
[3] - All sounds played by this mobile are copied resources from other mobiles.

2. Assets that couldn't be extracted from the original game
   - Mobile
     - Dragon's Fireball (S1/S2)

   - Audio
     - Trico's unable to move sound (if it has any)

3. Current Project's Placeholder Assets:
   - Mobiles
     - Knight
       - Moving - Gallop sound extracted from HoMM3
       - UnableToMove - Horse neigh sound extracted from HoMM3

<h3>General</h3>

1. Audio
   - Music player randomizes songs from every GB version
   - Music supports transition effects. The music fades out/in some scenes, the "original" music sudden change was kept for a few exceptions.
   - As explained in Assets.1.1, some mobiles were suposed to make no sound. However, a placeholder is being played.

2. Mobiles
   - Dragon and Knight were slightly changed in order to make'em balanced.
   - There is no restrictions concerning mobiles. Dragon and Knight are now available to pick.
     - Dragon
       - SS was altered to make it more useful.
     - Mage
       - S1, S2 sprites are the same as in the original game. However, it's not exacly as it should be. I intend to address this issue later.

3. Game Mechanics
   - The server can broadcast the number of expected servers.
   - The 'unable to move' animation is now shared among players in the same room.
   - There is no 'Leave Match' button anymore. If the user wants to leave the match he must close the game.

4. Interface
   - There are blended interface elements between S1 and S2 clients. Mobile's picklist for instance.
   - The InGame/Wind Compass has a richer animation than the original client.
   - Upcoming weather system has also a improved animation.

5. Technical Information
   - The resolution can be chanced. However, big resolutions that surpass the stage's foreground/background sprite size can break the camera and the parallaxing. Keep in mind that there is no solution to that since it is limited by its sprites dimensions.
   - FPS is locked at 60 while the original game had lock at 30. The animations aren't rich enough to sustain bigger refresh rates.
   - The original game only supported the 800x600 resolution, hence, all the menus sprites were created for this resolution. In order to adapt the menus for bigger resolutions, I've scaled up all interface dynamically.
   - The game can be opened at window mode.
   - This game is built on monogame, meaning that it can be ported to mobile devices or other OS.

6. Networking
   - All clients are fed with the same information at the same time, meaning that it is expected to have no delay between clients.

<h3>Known bugs</h3>

1. Client
   - It is possible to freeze the match by passing the turn (there is a button on the interface for that).
   - The mobiles are unbalanced (INTENDED)
   - [Testing] A few mobile projectiles and mobile animations are unsynched.
   - [Testing] There is a slightly improbable chance of a projectile deal different damage on different clients.
   - Weather animations are not fps proof. A laggy player can have different wind settings.
   - Maximum resolution can not be greater than 1600 width OR 1600 height.

2. Server
   - There is no verification check on teams equality when a game is being started (INTENDED).
   - All servers are not smart enough to "re-attatch" the connections. In order to make it work again close it and open again in the same order described in Configurations & Setup.

<h2>Contributors</h2>

<h3>Developers</h3>
- [WickedPeanuts](https://github.com/WickedPeanuts/) (Carlos Henrique)
- [Icy Willow] (https://github.com/IcyWillow/) - Since 05/21/2020

<h2>Bug Reports/Testers
- [sdyalor](https://github.com/sdyalor)

<h2>Licence and copyright</h2>

All repositories/programs that copies this repository must include the following line:
- Copyright (C) 2020, Carlos H.M.S. <carlos_judo@hotmail.com>

Copyright (C) 2020, Carlos H.M.S. <carlos_judo@hotmail.com>

This file is part of OpenBound.

OpenBound is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

OpenBound is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with OpenBound. If not, see http://www.gnu.org/licenses/.
