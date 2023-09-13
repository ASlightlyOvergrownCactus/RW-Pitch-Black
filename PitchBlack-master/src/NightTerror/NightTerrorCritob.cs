﻿using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using System.Collections.Generic;
using UnityEngine;
using static PathCost.Legality;

namespace PitchBlack;
sealed class NightTerrorCritob : Critob
{
    internal NightTerrorCritob() : base(CreatureTemplateType.NightTerror)
    {
        Icon = new SimpleIcon("Night_Terror", new Color(0.1f, 0.1f, 0.1f));
        RegisterUnlock(KillScore.Configurable(25), SandboxUnlockID.NightTerror);
        SandboxPerformanceCost = new(3f, 1.5f);
        LoadedPerformanceCost = 200f;
        ShelterDanger = ShelterDanger.Hostile;
        NightTerrorHooks.Apply();
    }

    public override void ConnectionIsAllowed(AImap map, MovementConnection connection, ref bool? allow)
    {
        if (connection.type == MovementConnection.MovementType.ShortCut)
        {
            if (connection.startCoord.TileDefined && map.room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.Normal)
                allow = true;
            if (connection.destinationCoord.TileDefined && map.room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.Normal)
                allow = true;
        }
        else if (connection.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze)
        {
            if (map.room.GetTile(connection.startCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && map.room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.Normal)
                allow = true;
            if (map.room.GetTile(connection.destinationCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && map.room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.Normal)
                allow = true;
        }
    }

    //public override void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allow) => allow = ((map.getAItile(tilePos).terrainProximity > 1f && map.getAItile(tilePos).terrainProximity < 3f) || (map.getAItile(tilePos).narrowSpace) || map.getAItile(tilePos).walkable) && map.getAItile(tilePos).terrainProximity != 0;

    public override int ExpeditionScore() => 25;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.blue;

    public override string DevtoolsMapName(AbstractCreature acrit) => "ntr";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[] { RoomAttractivenessPanel.Category.LikesInside };

    public override IEnumerable<string> WorldFileAliases() => new[] { "nightterror" };

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.RedCentipede, Type, "NightTerror")
        {
#if true
                // Turned this off to use red centipede's allows and disallows for optimum prebaked pathing stuff idk. Turn on if you want to mess with these values
                // These should help with it's pathfinding, but feel free to change them if you have better ideas. Climb and Ceiling are punished because it often pulls itself off them with it's body dangling and that's not very scary
            TileResistances = new()
            {
                OffScreen = new(1f, Allowed),
                Floor = new(1f, Allowed),
                Corridor = new(1f, Allowed),
                Climb = new(0.3f, Allowed),
                Wall = new(1f, Allowed),
                Ceiling = new(1f, Allowed),
                Air = new(1f, Unallowed),
                Solid = new(1f, Unallowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, Allowed),
                ReachOverGap = new(0.9f, Allowed),
                ReachUp = new(0.4f, Allowed),
                DoubleReachUp = new(0.3f, Allowed),
                ReachDown = new(0.7f, Allowed),
                SemiDiagonalReach = new(1f, Allowed),
                DropToFloor = new(0.6f, Allowed),
                DropToClimb = new(0.4f, Allowed),
                DropToWater = new(0.7f, Unwanted),
                LizardTurn = new(1f, Allowed),  //No idea what this one means?
                OpenDiagonal = new(1f, Allowed),
                Slope = new(1f, Allowed),
                CeilingSlope = new(0.4f, Allowed),
                ShortCut = new(1f, Allowed),
                NPCTransportation = new(1f, Allowed),
                BigCreatureShortCutSqueeze = new(1f, Allowed),
                OutsideRoom = new(1f, Allowed),
                RegionTransportation = new(1f, IllegalConnection),
                BetweenRooms = new(1f, Allowed),
                OffScreenMovement = new(1f, Allowed)
            },
#endif
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 1f),
            DamageResistances = new() { Base = 200f, Explosion = .03f },
            StunResistances = new() { Base = 200f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.RedCentipede),
        }.IntoTemplate();
        return t;
    }

    public override void EstablishRelationships()
    {
        var daddy = new Relationships(Type);
        daddy.Attacks(CreatureTemplate.Type.Slugcat, 99f);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new CentipedeAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Centipede(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new Centipede.CentipedeState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.RedCentipede;
}