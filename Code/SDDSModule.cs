extern alias mm;
using System;
using Celeste;
using mm::Celeste.Mod;
using Microsoft.Xna.Framework;

namespace SDDSHelper
{
    /// <summary>
    /// SDDSHelper main module.
    ///
    /// Implements a special golden berry isomorphic to the vanilla 1A "no-dash golden"
    /// (winged golden, entity name memorialTextController), but replaces the "no dash"
    /// condition with: this session has never had a dash interact with a CanyonHelper
    /// push block.
    ///
    /// The gate structure fully mirrors the vanilla one (see decompiled Level.cs /
    /// Session.cs / PlayerDeadBody.cs):
    ///   Vanilla: the final room checks Session.Dashes == 0 at load time (Level.Reload
    ///            rolls Dashes back to the room-entry value on death).
    ///   Ours:    the final room checks "room-entry baseline" Pin == 0 (death Reload
    ///            never rewrites the baseline, so mistakes inside the berry's room can
    ///            be retried via death; on screen transitions OnTransitionTo pins the
    ///            baseline to the current total, so cross-room history can never be
    ///            wiped - matching vanilla cross-room dash history).
    /// </summary>
    public class SDDSModule : EverestModule
    {
        public static SDDSModule Instance { get; private set; }

        public SDDSModule()
        {
            Instance = this;
        }

        public override void Load()
        {
            Everest.Events.Level.OnLoadEntity += OnLoadEntity;
            Everest.Events.Level.OnLoadLevel += OnLoadLevel;
            Everest.Events.Level.OnTransitionTo += OnTransitionTo;

            // Register with the same treatment as the vanilla "goldenBerry":
            // not tracked on the HUD (tracked: false) and yields the follower chain
            // head (blocksNormalCollection: true, goldens only settle at the end).
            StrawberryRegistry.Register(
                typeof(SDDSGoldenBerry),
                SDDSGoldenBerry.EntityName,
                tracked: false,
                blocksNormalCollection: true);
        }

        public override void Unload()
        {
            Everest.Events.Level.OnLoadEntity -= OnLoadEntity;
            Everest.Events.Level.OnLoadLevel -= OnLoadLevel;
            Everest.Events.Level.OnTransitionTo -= OnTransitionTo;
        }

        // Every room entry (transition / respawn / teleport) keeps the delta tracker present.
        private void OnLoadLevel(Level level, Player.IntroTypes intro, bool isFromLoader)
        {
            if (level.Tracker.GetEntity<SDDSPushBlockTracker>() == null)
            {
                level.Add(new SDDSPushBlockTracker());
            }
        }

        // When leaving the current room for the next one, pin the running total as the
        // "room-entry baseline". After TransitionTo the player is frozen in the wipe,
        // so no push block interaction can happen between this moment and the new
        // room's entity load - the baseline equals the total at the moment of entry.
        private void OnTransitionTo(Level level, LevelData next, Vector2 direction)
        {
            Session session = level.Session;
            session.SetCounter(SDDSPushBlockTracker.PinKey,
                session.GetCounter(SDDSPushBlockTracker.TotalKey));
        }

        // SDDS golden berry load gate: started from the beginning of the chapter AND
        // the room-entry baseline is 0 (no push block interaction along the whole run).
        // On gate failure the entity is simply not added - mirroring the vanilla
        //   if (Session.Dashes == 0 && Session.StartedFromBeginning) base.Add(new Strawberry(...));
        private bool OnLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        {
            if (entityData.Name != SDDSGoldenBerry.EntityName)
            {
                return false;
            }

            Session session = level.Session;
            if (session.StartedFromBeginning
                && session.GetCounter(SDDSPushBlockTracker.PinKey) == 0)
            {
                level.Add(new SDDSGoldenBerry(entityData, offset, new EntityID(levelData.Name, entityData.ID)));
            }

            // Return true regardless of the gate: the entity is handled (on failure it
            // just does not exist, same as vanilla).
            return true;
        }
    }
}
