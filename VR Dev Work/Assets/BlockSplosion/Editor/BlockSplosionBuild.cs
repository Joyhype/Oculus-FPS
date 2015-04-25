/************************************************************************************

Filename    :   BlockSplosionBuild.cs
Content     :   Build scripts for Blocksplosion
Created     :   May 8, 2014
Authors     :   Alex Howland

Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.

Use of this software is subject to the terms of the Oculus LLC license
agreement provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

************************************************************************************/
using UnityEngine;
using UnityEditor;

//-------------------------------------------------------------------------------------
// ***** OculusBuildApp
//
// OculusBuildApp allows us to build other Oculus apps from the command line. 
//
partial class OculusBuildApp
{
    static void BuildBlockSplosionAPK()
    {
		string[] scenes = { "Assets/BlockSplosion/Scenes/Startup_Sample.unity", "Assets/BlockSplosion/Scenes/Main.unity" };
        BuildPipeline.BuildPlayer(scenes, "BlockSplosion.apk", BuildTarget.Android, BuildOptions.None);
    }
}