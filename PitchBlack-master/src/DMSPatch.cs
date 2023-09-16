﻿using DressMySlugcat;
using static PitchBlack.Plugin;

namespace PitchBlack;

internal class DMSPatch
{
    //DMS v1.6.6
    public static void AddSpritesToDMS()
    {
        SpriteDefinitions.AddSprite(new SpriteDefinitions.AvailableSprite
        {
            Name = "WHISKERS", //name at the top when clicking on the description box
            Description = "Whiskers", //description on dms menu
            GallerySprite = "LizardScaleA0",
            RequiredSprites = new() { "LizardScaleA0" },
            Slugcats = new() { BeaconName.value, PhotoName.value }
        });
    }
}
