# FiVES - Experimental Plugins.

This repository provides a set of specialised plugins that can be used for experimenting, but may be unstable.
FiVES is designed to be not only a fast development framework for running virtual world applications, but also as a sandbox to try and evaluate different technologies.

  _FiVES is part of the Future Internet project FIWARE of the European Union, where it is used as alternative implementation of the Synchronization Generic enabler. Please find more information about FIWARE at http://fiware.org_
  
## How to use these Plugins

These plugins provide new, partially untested, experimental features for the synchronization framework _FiVES_ : https://github.com/fives-team/fives . To add them to your FiVES application, perform the following steps:

1. Open the solution file in the IDE of your choice
2. Enable NuGet package restore on rebuild (more information here: http://www.nuget.org)
3. Select "Rebuild Solution"
4. You will find the compiled plugin libraries as .dll in the [$SOLUTION\_FOLDER]/Binaries/[$BUILD\_VERSION] folder
5. Copy all of these files into the _Plugins_ directory of your FiVES distribution
6. Make sure that the new Plugins are not blacklisted in your _FiVES.exe.cfg_, or whitelist them, if whitelisting is enabled

## Quick Intro to Experimental Plugins

Plugin | Function
-------|---------
ServerSync | Allows to connect several FiVES instances in a network via SINFONI to a cluster of synchronized server nodes
Scalability | Adds interest management using Domains of Responsibility, while the actual interest management algorithms can be extended via a simple interface
ConfigScalability | Provides extended configuration possibilities for the Scalability Plugin
