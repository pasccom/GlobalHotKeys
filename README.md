REPOSITORY DESCRIPTION
----------------------

This repository contains GlobalHotKeys, a small C# application allowing to
define custom global shortcuts to trigger some actions.

FEATURES
--------

Here is a list of the current features of the program:
- Uses plain text config files
- Allows to load other config files from a shortcut
- Allows to start and focus a specific application with shortcuts
- Allows to lock, shutdown and restart the computer with shortcuts

INSTALLATION
------------

GlobalHotKeys is developed in C# and was tested against 4.0 and 4.5 versions of
the .NET framework. It uses Apache log4net, which is distibuted along the
program under compiled form in the `lib/` folder.
	
To log to Windows event log, a log folder named Pascom should be created.
This can be done using the following PowerShell command with administrator
priviledges.
```posh
	New-EventLog -LogName Pascom -Source GlobalHotKeys
```

I personnally advise to add GlobalHotkeys as a planned task when session opens.
This can easily be done using the Task Manager.

CONFIGURATION
-------------

Currently GlobalHotKeys uses two configuration files
- The first defines the processes GlobalHotKeys can startup. This file should
not be editable by an ordinary user (for security related problems). It should
belong to either the Admin user or the System user.
- The second lists the user-defined shortcuts. It can be edited by an ordinary
user As of now these files should follow a plain text format. Examples of such
files are distributed along with the code. 
See files `globalhotkeys.processes.conf` and `globalhotkeys.conf`.

To write the `globalhotkeys.conf` file you will need to look at the
documentation of `GlobalHotKeys.Shortcuts.Handler`,
`GlobalHotKeys.Windows.Manager` and `GlobalHotKeys.Power.Manager` classes for
the available features. A list of the callable functions and their arguments is
given in the descriptions of these classes.

PLANNED DEVELOPMENTS
--------------------

Here are some ideas I plan to implement later:
- Put the functionalities of the Windows and the Power namespace into separate
plugins, so that the used does not need to load every code he does not need.
This will be useful when GlobalHotKeys has a lot of functionnalities.
- Implement new formats for the configuration. For instance I would like to
have XML configuration available.
- Develop a graphic tool to write GlocbalHotKeys config files.

If you have any other feature you will be interested in, please let me know.
I will be pleased to develop it if I think it is a must have.

If you want to implement extension, also tell me please. Admittedly you
can do what you desire with the code (under the constraints stated in the
section LICENSING INFORMATION below), but this will avoid double work.

LICENSING INFORMATION
---------------------

GlobalHotKeys is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

GlobalHotKeys is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with GlobalHotKeys. If not, see http://www.gnu.org/licenses/