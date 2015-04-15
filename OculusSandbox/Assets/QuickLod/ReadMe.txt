Quick steps:

1:
- Add the lod manager to any object that doesn't get disabled or destroyed
- Add the lod object to each object that should be used
- Add the lod source to each used camera

2:
- Setup the lod manager by defining the grid (boundary) and cell size
- Setup the lod sources by defining the max update distance
- [Optional] Setup the lod objects by changing the object references and distances

Read the Manual_EN.pdf for more informations.
Read the HowAutosetupWorks_EN.pdf for detailed information about the automated setup.
Test out the DemoScene if you want to try QuickLod without messing things up.

---------------------------------------

UPDATE INFORMATIONS:
In the list bellow you can see if you need to do anything after upgrading to a newer version.
If you import QuickLod the first time, no actions are needed.

From 1.5 to 1.6:
No additional actions needed.

From 1.4 to 1.5:
There are obsolete files, you can safely remove them.
See the Version History.txt for more informations

The grid data has been renamed.
If you access it in your code, please check for errors.

From 1.3 to 1.4:
No additional actions needed.

From 1.2 to 1.3:
The references in some lod objects might be lost, so check all used lod objects for missing references!
You can remove the "Attributes/OverwriteGlobalAttribute.cs" file as it is no longer used.

From 1.1 to 1.2:
Delete the old QuickLod folder before importing the newest version because some files have been renamed.

---------------------------------------

Detailed informations on:
http://tirelessart.ch/en/QuickCode/QuickLod/

The forum for help and sugestions:
http://forum.unity3d.com/threads/237196-QuickLod

The demo can be found on:
http://tirelessart.ch/en/quickcode/quicklod/demo/

The API documentation:
http://chillersanim.bplaced.net/

---------------------------------------

If you have sugestions or found a bug, make sure to leave me a reply on the forum. I'm happy to help.
If you like QuickLod then please rate and review it in the Asset Store. :)

Have fun!