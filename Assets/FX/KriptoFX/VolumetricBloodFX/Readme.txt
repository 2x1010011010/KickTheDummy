Version 1.0.11

- My email is "kripto289@gmail.com"
- Discord channel https://discord.gg/GUUZ9D96Uq (you can get all new changes/fixes/features in the discord channel. The asset store version will only receive major updates)

You can contact me for any questions.
My English is not very good, and if I made mistakes, you can write me :)



------------------------------------     FIRST STEPS    -------------------------------------------------------------------------------

1) For HDRP and URP you need to import patches from the folder "\Assets\KriptoFX\VolumetricBloodFX\URP and HDRP Patches"

2) How to TURN ON DECALS:
    -standard rendering (only for forward rendering): MainCamera -> Add the script "BFX_RenderDepth"
    -URP rendering: Add "Decal" render feature. https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@10.1/manual/urp-renderer-feature-how-to-add.html
    -HDRP decal rendering works out of the box


3) For all platforms required format "RGBA half" or compressed "RGB9e5 23 bit Shared Exponent Float" for textures with the name "blood_norm" and "blood_pos"
RGB9e5 this is a lossy format so the blood drops will be sharper.

4) For mobile devices you must to do "texture overide for android -> RGBA half/RGB9e5", because by default unity used RGBM encoding for android and this format does not work correctly.

------------------------------------------------------------------------------------------------------------------------------------------

Effects using:

1) Insantiate effect:
The blood should be rotated to the right angle.
Since the animation is baked and gravity must flow down. If you turn the effect up (for example) then the gravity animation will flow left/right.

var direction = .... //you must assign the direction, for example Physics.Raycast(ray, out hit); direction = hit.normal;
float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + 180;
var instance = Instantiate(bloodPrefab, hit.point, Quaternion.Euler(0, angle + 90, 0));

2) Set the start height position for decal. It's your terrain/floor transform position Y

instance.GetComponent<BFX_BloodSettings>().GroundHeight = ...


Decal at the shot point (for example, on the character) may have problems with moved objects.
Unfortunately, there is no way to add a blood decal without visible artefacts to any object with different shaders and different UV.
Therefore, the only method is to attach the decal as a child to a bone or object. But this will produce visual artifacts (when the decal moves separately from the mesh).
I use follow code for find nearest bone in the hit point:


Transform GetNearestBone(Transform characterTransform, Vector3 hitPos)
{
    var closestPos = 10f;
    Transform closestBone = null;
    var childs = characterTransform.GetComponentsInChildren<Transform>();

    foreach (var child in childs)
    {
        var dist = Vector3.Distance(child.position, hitPos);
        if (dist < closestPos)
        {
            closestPos = dist;
            closestBone = child;
        }
    }

    var distRoot = Vector3.Distance(characterTransform.position, hitPos);
    if (distRoot < closestPos)
    {
        closestPos = distRoot;
        closestBone = characterTransform;
    }
    return closestBone;
}

Code example:

if (Input.GetMouseButtonDown(0)) //create blood splashes at mouse position (by left mouse button click)
{
    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;
    if (Physics.Raycast(ray, out hit))
    {
        var nearestBone = GetNearestObject(hit.transform.root, hit.point);
        if(nearestBone == null) return;

        var attachedBloodInstance = Instantiate(attachedBloodDecal, nearestBone);
        var bloodT = attachedBloodInstance.transform;
        bloodT.position = hit.point;
        bloodT.localRotation = Quaternion.identity;
        bloodT.LookAt(hit.point + hit.normal, direction);
        bloodT.Rotate(90, 0, 0);
        Destroy(attachBloodInstance, 20);
    }
}

------------------------------------------------------------------------------------------------------------------------------------------

Effect optimisations:

1) You can change texture format for textures with the name "blood_norm" and "blood_pos".
Click on current platform (not a "default" tab!) -> override for "current platform name" -> select format "RGB9e5 23 bit Shared Exponent Float"
This format available with unity 2019.x+. Visual quality will suffer a little bit.

2) You can decrease height of decals. The decals is drawn in the form of a cube. Smaller the height of the cube -> the less pixels are drawn.
Open blood prefab -> select decal -> change transform "y"

3) You can remove shadows and depth writing. With directional light and depth rendering you can decrease number of triangles x4 times!
Open BFX_Blood shader -> find ths lines "//start remove line " and "//end remove light"
Remove the code from the "start" to "end" lines. (150 -> 231 line number)

4) Use lower resolution for mobile devices :)
Do you really need to render the game in 2-4k on screen with size 5-7 inches?? Even not many pc games can work in 4k. I find this unnecessary.
For example use half resolution = 4x faster!
Screen.SetResolution((int)(Screen.width * 0.5f), (int)(Screen.height * 0.5f), FullScreenMode.ExclusiveFullScreen, 60);

------------------------------------------------------------------------------------------------------------------------------------------

You can draw decals for only some layers.
For that just add the script "BFX_BloodDecalLayers" on the main camera and select a layers for decal drawing.
You can change the "decal rendering mode" to draw or ignore the selected layers.
But this method has overhead since the selected layers need to be drawn in the depth buffer. You can optimize it (with a small artefacts) using smaller texture size. Just change the "LayerDepthResoulution" property.

