﻿using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PitchBlack;

public static class NightTerrorTrackers
// This parenthesis is red why is it red oh god please stop being red - Moon
{
    /// <summary>
    /// Tracker for NightTerror to help track the player and pursue it.
    /// </summary>
    public class NightTerrorTracker
    {
        public WeakReference<AbstractCreature> _Ac;
        public WeakReference<World> _World;
        public int slowTick;  // Used for ticking something after waiting a while (to reduce lag/wait for AI)
        public int fastTick;  // Used for ticking every once in a while (to reduce lag/wait for AI)
        public int doHax;  // Used to teleport 


        /// <summary>
        /// Initializes the class, hooking onto update so it updates on its own as long as the associated abstractcreature keeps existing.
        /// </summary>
        /// <param name="ac">NightTerror's AbstractCreature</param>
        /// <param name="world">a world</param>
        public NightTerrorTracker(AbstractCreature ac, World world)
        {
            this._Ac = new WeakReference<AbstractCreature>(ac);
            this._World = new WeakReference<World>(world);  // What do I even use this for? I can't remember lol.

            // TODO: Move the update hook out of the CWT (so it doesn't hook for every single new CWT created) and instead hook into abstractCreature.Update() outside.
            //On.Centipede.Update += Centipede_Update;
            On.RainWorldGame.Update += RainWorldGame_Update;
            //On.CentipedeAI.Update += CentipedeAIUPDATER;

            //Debug.Log(">>> NIGHTTERROR TRACKER INIT! BEWARE OF THE DARK");
        }


        #region Update Shenanigans
#if false
        /// <summary>
        /// Update attempt on CentipedeAI. Decided against using this since it isn't reliable. Potentially ticks more than once even.
        /// </summary>
        private void CentipedeAIUPDATER(On.CentipedeAI.orig_Update orig, CentipedeAI self)
        {
            orig(self);
            if (ac is not null) Update();
        }
#endif

        /// <summary>
        /// The sweetspot for updating the tracker... it's how the expedition pursue does it. I could just hook into AbstractCreature.Update() outside of this CWT and upon checking the template type as NightTerror access the CWT and use this Update()... but I'm lazy so uhh I'll leave it as a todo or something. It won't affect that much so it's low priority.
        /// </summary>
        private void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            orig(self);
            if (_Ac is not null) Update();
        }

#if false
        /// <summary>
        /// Theoretically only works when the centipede is realized, so until then this code wouldn't run. But in reality, as long as ONE centipede is alive, it would tick... or maybe it would tick for EVERY SINGLE REALIZED CENTIPEDE meaning the tracker could be ticking more than once... oh dear.
        /// </summary>
        private void Centipede_Update(On.Centipede.orig_Update orig, Centipede self, bool eu)
        {
            orig(self, eu);
            if (ac is not null) Update();
        }
#endif
#endregion

        /// <summary>
        /// Ticks the tickers.
        /// </summary>
        private void Tick()
        {
            // Ticking like this reduces lag!
            if (slowTick > 0)
            {
                slowTick--;
            }
            else
            {
                slowTick = 600;  // Updates once per 15 seconds
            }

            if (fastTick > 0)
            {
                fastTick--;
            }
            else
            {
                fastTick = 20;  // Updates twice per second
            }
        }

        /// <summary>
        /// Update the tracker. Enjoy!
        /// </summary>
        public void Update()
        {
            if (!_Ac.TryGetTarget(out var ac) || ac.abstractAI is null || !_World.TryGetTarget(out World world) || world.game?.session?.Players is null)
            {
                return;
            }

            static bool RoomIsAStartingCabinetsRoom(string roomName)
            {
                if (roomName == "SH_CABINETMERCHANT")
                    return true;

                for (int i = 1; i <= 5; i++)
                {
                    //spinch: nt gets to track SH_CABINETS6, as a treat
                    if (roomName == $"SH_CABINETS{i}")
                        return true;
                }

                return false;
            }

            Tick();  // Tick the tickers
            AbstractCreature firstPlayer = null;  // For tracking the first non-dead player
            AbstractCreature sameRoomPlayer = null;  // For tracking a player in same room
            // if (_World.TryGetTarget(out World world))
            foreach (AbstractCreature c in world.game.Players)
            {
                if (
                    c.realizedCreature is Player player && 
                    player.room?.abstractRoom is not null && // Null check before checking for shelters/gates
                    !player.room.abstractRoom.shelter &&  // Forbid tracking players in shelters
                    !player.room.abstractRoom.gate && // Forbid tracking players in gates
                    !player.dead
                    //spinch: AND i made it so the night terror doesn't track in the spawn rooms
                    && !RoomIsAStartingCabinetsRoom(player.room.abstractRoom.name)
                    )
                {
                    firstPlayer ??= c;  // Only store the first usable player

                    // Same room player detected?! More likely than you think
                    if (ac.abstractAI?.RealAI is not null && ac.pos.room == c.pos.room)
                    {
                        firstPlayer = c;  // To sync with the same room player so it doesn't try to set the wrong coordinates
                        sameRoomPlayer = c;
                        break;
                    }
                }
            }

            #region Location update (FOR LOGGING PURPOSES)
#if false
            try
            {
                if (ac.Room != null && ac.Room?.name != currentRoom)
                {
                    // Debug.Log(">>> Nightterror moves! From " + currentRoom + " to " + ac.Room.name);
                    currentRoom = ac.Room.name;
                }
            }
            catch (NullReferenceException nerr)
            {
                Debug.LogWarning(">>> Nightterror null error while attempting to update position!");
                Debug.LogException(nerr);
            }
            catch (Exception err)
            {
                Debug.LogWarning(">>> Nightterror generic error while attempting to update position!");
                Debug.LogException(err);
            }
#endif
            #endregion


            #region Non-same room update
            try
            {
                if (ac.abstractAI is null || firstPlayer is null) return;  // End update if trackable player not found
                if (ac.pos.room != firstPlayer.pos.room)
                {
                    //Debug.Log(">>> Nightterror shifts Destination! From " + previousRoom.ResolveRoomName() + " to " + firstPlayer.pos.ResolveRoomName());
                    // previousRoom = firstPlayer.pos;
                    // Change destination
                    if (ac.abstractAI.RealAI is not null && true)  // I found this to not work so shrimply death to code.   //But it might?
                    {
                        ac.abstractAI.RealAI.SetDestination(firstPlayer.pos);
                    }
                    else
                    {
                        ac.abstractAI.SetDestination(firstPlayer.pos);
                    }
                }

                // Detect if ai is stuck
                if (ac.abstractAI.path == null || ac.abstractAI.path.Count == 0)
                {
                    doHax++;
                }
                else if (doHax > 0)  // If the Nightterror manages to find a path before a teleport, decrement it slowly so it can be a short while before teleport if it gets stuck again
                {
                    doHax--;
                }

                // TELEPORT UWU (stranded doesn't work as far as my testing goes, but just in case? Otherwise, wait 10 seconds equivalent of not being able to find a path.
                if ((doHax > 400 || ac.abstractAI.strandedInRoom != -1 || (ac.abstractAI.RealAI != null && ac.abstractAI.RealAI.stranded)) && ac.pos.room != firstPlayer.pos.room && fastTick == 0)
                {
                    if (firstPlayer?.realizedCreature?.room != null && ac.realizedCreature is null)
                    {
                        //spinch: apparently 2nd check is for if centipede isnt realized, allow it to tp
                        //Debug.Log(">>> Migrate Nightterror!");

                        // Find a room that isn't a shelter or a gate and place the Nightterror there. No shelters because that makes no sense (no den for Nightterror to use the excuse "it shrimply used a den"). No gates because it tends to get stuck on the wrong side.
                        if (firstPlayer != null && firstPlayer.realizedCreature != null && firstPlayer.realizedCreature.room != null) {
                            RWCustom.IntVector2 ar = firstPlayer.realizedCreature.room.exitAndDenIndex[UnityEngine.Random.Range(0, firstPlayer.realizedCreature.room.exitAndDenIndex.Length)];
                            //if (!(firstPlayer.realizedCreature.room.WhichRoomDoesThisExitLeadTo(ar).gate
                            //    || firstPlayer.realizedCreature.room.WhichRoomDoesThisExitLeadTo(ar).shelter))

                            var roomBeingChecked = firstPlayer.realizedCreature.room.WhichRoomDoesThisExitLeadTo(ar);
                            if (roomBeingChecked != null && !(roomBeingChecked.gate || roomBeingChecked.shelter))
                            {
                                //ac.Move(firstPlayer.realizedCreature.room.WhichRoomDoesThisExitLeadTo(ar).RandomNodeInRoom());
                                //doHax = 0;
                                var randomNodeInRoom = roomBeingChecked.RandomNodeInRoom();
                                if (randomNodeInRoom != null)
                                {
                                    ac.Move(randomNodeInRoom);
                                    doHax = 0;
                                }
                            }
                        }
                    }

#if false
                    // It moves fine pretty well without this alternative destination set (and I haven't actually verified if this works...). But if it gets stuck in random corners constantly, you might give this code a try.
                    else
                    {
                        Debug.Log("Attempt at alternative destination setter");
                        ac.abstractAI.SetDestinationNoPathing(firstPlayer.pos, true);
                        doHax -= 80;
                    }
#endif
                }

#if false
                // No need to migrate! We got migrate at home... the migrate at home: *is a goddamn teleport btn*
                // Migration
                if (ac.timeSpentHere > 3600 && ac.pos.room != firstPlayer.pos.room && slowTick == 0)    
                {
                    if (firstPlayer.Room != null)
                    {
                        ac.abstractAI.MigrateTo(firstPlayer.Room.RandomNodeInRoom());
                    }
#endif
                }
                //}
                //catch (NullReferenceException nerr)
                //{
                //    Debug.LogError(">>> Nightterror null error while attempting to update destination!");
                //    Debug.LogException(nerr);
                //}
            catch (Exception err)
            {
                //Debug.LogError(">>> Nightterror generic error while attempting to update destination!");
                Debug.LogException(err);
            }
            #endregion


            #region Same Room update
            try
            {
                if (sameRoomPlayer is not null && ac.abstractAI.RealAI != null)
                {
                    // Focus on one player
                    if (fastTick == 0 && true)  // Why was this false?
                    {
                        foreach (Tracker.CreatureRepresentation tracked in ac.abstractAI.RealAI.tracker.creatures)
                        {
                            //Debug.Log("This tracked creature has: " + (tracked is not null ? "Tracker " : "") + (tracked?.representedCreature is not null ? "RepresentedCreature" : ""));
                            if (tracked?.representedCreature is null)
                            {
                                //Debug.Log(">>> Nightterror has an unrealized creature!");
                                continue;
                            }
                            if (tracked.representedCreature.creatureTemplate.type != CreatureTemplate.Type.Slugcat)
                            {
                                //Debug.Log(">>> Nightterror forgets creature: " + tracked.representedCreature.creatureTemplate.name);
                                ac.abstractAI.RealAI.tracker.ForgetCreature(tracked.representedCreature);
                                //Debug.Log("    Nightterror forgor!");
                            }
                            else
                            {
                                //Debug.Log(">>> Nightterror is angy at player!");
                                ac.abstractAI.RealAI.tracker.SeeCreature(sameRoomPlayer);
                                (ac.realizedCreature as Centipede).bodyDirection = true;
                                ac.abstractAI.freezeDestination = false;
                                ac.abstractAI.RealAI.SetDestination(sameRoomPlayer.pos);
                                //Debug.Log("    Nightterror angr!");
                            }
                        }
                    }

#if true
                    // An assisting code that helps NTT track the player when the .SeeCreature doesn't do enough... which it kind of doesn't, but if you like, you could disable this segment of code and see what happens.
                    if (fastTick == 0)
                    {
                        ac.abstractAI.SetDestination(sameRoomPlayer.pos); 

                    }
#endif

                    // Make centipede see creature and *finger crossed* follow the player.
                    //Debug.Log(">>> Nightterror See YOU!");
                    //ac.abstractAI.RealAI.tracker.CreatureNoticed(sameRoomPlayer);
                    ac.abstractAI.RealAI.tracker.SeeCreature(sameRoomPlayer);
                    //Debug.Log(">>> Nightterror WANT TO KILL YOU!");
                }
            }
            //catch (NullReferenceException nerr)
            //{
            //    Debug.LogError(">>> Nightterror null error while attempting to track player in same room!");
            //    Debug.LogException(nerr);
            //}
            catch (Exception err)
            {
                //Debug.LogError(">>> Nightterror generic error while attempting to track player in same room!");
                Debug.LogException(err);
            }
            #endregion

        }
    }
    

    /// <summary>
    /// A place to store the NightTerror CWTs uwu
    /// </summary>
    private static readonly ConditionalWeakTable<AbstractCreature, NightTerrorTracker> CWT = new();
    
    /// <summary>
    /// CWT to store a nightterror tracker for each nightterror. Does questionable hooking but since you're not gonna have a dozen of these in a giant room it should be fiiiine? Right? Right?! Also it is only used as a setter so don't expect to get any values from this CWT.
    /// </summary>
    /// <param name="ac">AbstractCreature (the NightTerror abstractcreature)</param>
    /// <param name="world">A world I guess</param>
    /// <returns></returns>
    public static NightTerrorTracker NTT(this AbstractCreature ac, World world) => CWT.GetValue(ac, _ => new(ac, world));
}