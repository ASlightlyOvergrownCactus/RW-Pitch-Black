using MonoMod.RuntimeDetour;
using System.Reflection;
using RWCustom;
//using static PitchBlack.Plugin;
using Colour = UnityEngine.Color;
using UnityEngine;

namespace PitchBlack
{
    public static class OverseerGraphics1
    {
        public static void Apply()
        {
            On.OverseerGraphics.ColorOfSegment += OverseerGraphics_ColorOfSegment;
            new Hook(typeof(OverseerGraphics).GetProperty("MainColor", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(), typeof(Plugin).GetMethod("OverseerGraphics_MainColor_get", BindingFlags.Static | BindingFlags.Public));
        }
        //Fixes funny coloring
        public static Colour OverseerGraphics_ColorOfSegment(On.OverseerGraphics.orig_ColorOfSegment orig, OverseerGraphics self, float f, float timeStacker)
        {
            Colour val = orig(self, f, timeStacker);
            if ((self.overseer.abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator == 87)
            //if (self.overseer.room?.world.game.session is StoryGameSession story && MiscUtils.SlugIsInMod(story.game.StoryCharacter) && self.overseer.PlayerGuide)
            {
                return Color.Lerp(
                    Color.Lerp(Custom.RGB2RGBA((self.MainColor + new Color(0f, 0f, 1f) + self.earthColor * 8f) / 10f, 0.5f),
                    Color.Lerp(self.MainColor, Color.Lerp(self.NeutralColor, self.earthColor, Mathf.Pow(f, 2f)), self.overseer.SandboxOverseer ? 0.15f : 0.5f),
                    self.ExtensionOfSegment(f, timeStacker)), Custom.RGB2RGBA(self.MainColor, 0f),
                    Mathf.Lerp(self.overseer.lastDying, self.overseer.dying, timeStacker));
            }
            return val;
        }
        public delegate Colour orig_OverseerMainColor(OverseerGraphics self);
        public static Colour OverseerGraphics_MainColor_get(orig_OverseerMainColor orig, OverseerGraphics self)
        {
            Colour val = orig(self);
            if ((self.overseer.abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator == 87)
            //if (self.overseer.room?.world.game.session is StoryGameSession story && MiscUtils.SlugIsInMod(story.game.StoryCharacter) && self.overseer.PlayerGuide)
            {
                return Custom.hexToColor("bf194e");
            }
            return val;
        }
    }
}