using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using static Pom.Pom;

namespace PitchBlack;

public class CreatureSpawnerHooks
{
    private static PlacedObject.Type ReliableCreatureSpawner = new PlacedObject.Type("ReliableCreatureSpawner", false);
    public static void Apply() {
        IL.Room.Loaded += Room_Loaded;
    }
    private static void Room_Loaded(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdsfld<PlacedObject.Type>(nameof(PlacedObject.Type.FlareBomb)))) {
            Plugin.logger.LogDebug("Pitch Black IL Room Placedobjects failed");
            return;
        }
        if (!cursor.TryGotoPrev(MoveType.After, i => i.MatchLdarg(0))) {
            Plugin.logger.LogDebug("Pitch Black IL Room Placedobjects failed");
            return;
        }
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloc, 41);
        cursor.EmitDelegate((Room self, int i) => {
            if (self.roomSettings.placedObjects[i].active && self.roomSettings.placedObjects[i].type == ReliableCreatureSpawner) {
                
                if (self.game.session is SandboxGameSession session && !session.PlayMode) {
                    return;
                }

                string[] objectSettings = self.roomSettings.placedObjects[i].data.ToString().Split('~');

                CreatureTemplate.Type creatureTemplateType = (CreatureTemplate.Type)ExtEnum<CreatureTemplate.Type>.Parse(typeof(CreatureTemplate.Type), objectSettings[2], true);
                bool dead = Convert.ToBoolean(objectSettings[3]);
                string ID = objectSettings[4];
                EntityID entityID = self.game.GetNewID();

                if (ID != "" && ID.Contains(".")) {
                    string[] IDSplit = ID.Split('.');
                    try {
                        entityID = new EntityID(Convert.ToInt32(IDSplit[0]), Convert.ToInt32(IDSplit[1]));
                    } catch (FormatException err) {
                        Debug.Log("ID was incorrectly formated");
                        Debug.Log(err);
                    } catch (OverflowException err) {
                        Debug.Log("ID was too big");
                        Debug.Log(err);
                    } catch (Exception err) {
                        Debug.Log(err);
                    }
                }

                AbstractCreature abstractCreature = new AbstractCreature(self.world, StaticWorld.GetCreatureTemplate(creatureTemplateType), null, self.GetWorldCoordinate(self.roomSettings.placedObjects[i].pos), entityID);
                if (dead) {
                    abstractCreature.Die();
                }
                self.abstractRoom.AddEntity(abstractCreature);
            }
        });
    }
}

public class ReliableCreatureSpawner
{
    enum CreatureTemplateTypeEnum
    {
        BigEel,
        BigNeedleWorm,
        BigSpider,
        BlackLizard,
        BlueLizard,
        BrotherLongLegs,
        Centipede,
        Centiwing,
        CicadaA,
        CicadaB,
        CyanLizard,
        DaddyLongLegs,
        Deer,
        DropBug,
        EggBug,
        Fly,
        GarbageWorm,
        GreenLizard,
        Hazer,
        JetFish,
        KingVulture,
        LanternMouse,
        Leech,
        MirosBird,
        Overseer,
        PinkLizard,
        RedCentipede,
        RedLizard,
        Salamander,
        Scavenger,
        SeaLeech,
        Slugcat,
        SmallCentipede,
        SmallNeedleWorm,
        Snail,
        Spider,
        SpitterSpider,
        TempleGuard,
        Vulture,
        VultureGrub,
        WhiteLizard,
        YellowLizard
    }
    internal class Spawner : UpdatableAndDeletable
    {
        public Spawner(PlacedObject pObj, Room room) {
        }
    }
    internal static void RegisterSpawner() {
        List<ManagedField> fields = new List<ManagedField> {
			new EnumField<CreatureTemplateTypeEnum>("CreatureType", CreatureTemplateTypeEnum.GreenLizard, null, ManagedFieldWithPanel.ControlType.arrows, "Creature Type"),
            new BooleanField("Dead", true, ManagedFieldWithPanel.ControlType.arrows, "Spawn Dead"),
            new StringField("EntityID", "", "Creature ID")
		};
        RegisterFullyManagedObjectType(fields.ToArray(), typeof(Spawner), "ReliableCreatureSpawner", "Pitch-Black");
    }
}