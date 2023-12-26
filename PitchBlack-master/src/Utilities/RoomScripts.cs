﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace PitchBlack;

public class RoomScripts
{
    public static void Apply()
    {
        On.RainWorldGame.ctor += RainWorldGame_ctor;
        On.RoomSpecificScript.AddRoomSpecificScript += RoomSpecificScript_AddRoomSpecificScript;
        On.Player.Update += asdasdasd;
    }
    private static void asdasdasd(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        Debug.Log($"PB: {self.mainBodyChunk.pos}");
    }
    private static void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);
        SH_CABINETS1_IntroScript.alreadyRun = false;
    }

    private static void RoomSpecificScript_AddRoomSpecificScript(On.RoomSpecificScript.orig_AddRoomSpecificScript orig, Room room)
    {
        orig(room);
        if (room.game.session is StoryGameSession story
            && story.saveStateNumber == Plugin.BeaconName
            && story.saveState.denPosition == "SH_CABINETS1"
            && room.abstractRoom.name == "SH_CABINETS1")
        {
            room.AddObject(new SH_CABINETS1_IntroScript(room));
        }
        if (room.game.session is StoryGameSession session
            && session.saveStateNumber == Plugin.PhotoName
            && room.abstractRoom.name == "GW_photo01")
        {
            room.AddObject(new GW_PHOTO01_IntroScript(room));
        }
    }
}

public class SH_CABINETS1_IntroScript : UpdatableAndDeletable
{
    internal static bool alreadyRun; //sighs. but hey it fixes the problem
    int counter;
    const int COUNTER_MAX = 3 * 40; //when to force the script to stop and give players their controllers back
    bool alreadyTeleportedCoopPlayers;
    bool StayInAir => room.game.manager.FadeDelayInProgress || !room.fullyLoaded || !room.BeingViewed;
    Player RealizedPlayer => room.game.Players.Count > 0 ? room.game.Players[0].realizedCreature as Player : null;

    public SH_CABINETS1_IntroScript(Room room)
    {
        this.room = room;
        counter = 0;
        alreadyTeleportedCoopPlayers = false;
        Debug.Log($"Pitch Black: Created new {nameof(SH_CABINETS1_IntroScript)} in room {room.abstractRoom.name}");
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (null == RealizedPlayer)
        {
            return;
        }

        if (alreadyRun)
        {
            GiveAllPlayersControllersBack();
            Destroy();
            return;
        }

        counter++;

        if (StayInAir)
        {
            foreach (var abstrCrit in room.game.session.Players)
            {
                if (abstrCrit == null || abstrCrit.realizedCreature == null)
                    continue;

                var player = abstrCrit.realizedCreature as Player;

                player.controller ??= new Player.NullController(); //??= (null coalescing operator) is if null, then assign new thing

                if (RealizedPlayer != player && !alreadyTeleportedCoopPlayers)
                {
                    player.SuperHardSetPosition(new Vector2(923, 294)); //co-op players start in the water
                }
            }
            alreadyTeleportedCoopPlayers = true;
            RealizedPlayer.SuperHardSetPosition(new Vector2(923, 2373)); //player 1 starts at the top and falls down
            return;
        }

        alreadyRun = true;

        if (RealizedPlayer.Submersion > 0f || COUNTER_MAX == counter)
        {
            //has a counter check to forcibly give players back their controller, even if the script fucks up
            GiveAllPlayersControllersBack();
            return;
        }
    }

    public void GiveAllPlayersControllersBack()
    {
        foreach (var abstrCrit in room.game.session.Players)
        {
            if (abstrCrit?.realizedCreature is Player player)
                player.controller = null;
        }
    }
}

public class GW_PHOTO01_IntroScript : UpdatableAndDeletable
{
    private bool alreadyRun = false;
    private List<bool?> prevLavaImmune = new List<bool?>{null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null}; 
    // Just in case any custom scugs have this true as default. (Make 16 because myriad exists)
    public GW_PHOTO01_IntroScript(Room room)
    {
        this.room = room;
    }
    public override void Update(bool eu)
    {
        base.Update(eu);
        if (!alreadyRun) {
            for (int i = 0; i < room.game.Players.Count; i++) {
                if (prevLavaImmune[i] == null) {
                    prevLavaImmune[i] = room.game.Players[i].lavaImmune;
                }
                room.game.Players[i].lavaImmune = true;
                if (room.game.Players[i].realizedCreature is Player player) {
                    alreadyRun = true;
                    // I had trouble with 4-player groups pushing eachother into the acid. 
                        // This will spawn the first two players on the right, cannon spawning spot, and the other two on the platforms above the acid
                    float xPos = i <= 1? (i * 40f) : (-i * 80f);
                    player.SuperHardSetPosition(new Vector2(700f + xPos, 665f));
                    player.mainBodyChunk.vel = Vector2.zero;
                    player.standing = true;
                    room.game.Players[i].lavaImmune = (bool)prevLavaImmune[i];
                }
            }
        }
        else if (alreadyRun) {
            Destroy();
        }
    }
}