using System;
using System.Collections;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace SDDSHelper
{
    /// <summary>
    /// SDDS golden berry - the push block version of the vanilla 1A "no-dash golden"
    /// (winged golden / memorialTextController).
    ///
    /// Feeds a cloned EntityData with the name rewritten to "goldenBerry" into the
    /// vanilla Strawberry constructor, thereby inheriting every vanilla golden mechanic
    /// as-is: following, carrying, IsFirstStrawberry settlement rules, collection only
    /// at GoldBerryCollectTrigger / chapter completion, death triggering
    /// GoldenBerryRestart, save persistence, and so on.
    ///
    /// On top of that, exactly one extra rule applies: while the berry is ungrabbed,
    /// the moment a push block dash interaction happens after the current room was
    /// entered (cumulative total > room-entry baseline), the berry immediately flies
    /// away - the in-room counterpart of the vanilla winged berry's "dash and it's gone".
    /// </summary>
    public class SDDSGoldenBerry : Strawberry
    {
        public const string EntityName = "SDDSHelper/SDDSGoldenBerry";

        private bool flyingAway;

        public SDDSGoldenBerry(EntityData data, Vector2 offset, EntityID gid)
            : base(CloneAsGolden(data), offset, gid)
        {
        }

        // The vanilla Strawberry constructor decides "golden" by entityData.Name ==
        // "goldenBerry" and "winged" by the winged attribute, so clone the EntityData
        // and rewrite only the Name. The Values dictionary is shared (read-only usage)
        // and editor attributes pass through untouched.
        private static EntityData CloneAsGolden(EntityData src)
        {
            return new EntityData
            {
                ID = src.ID,
                Name = "goldenBerry",
                Level = src.Level,
                Position = src.Position,
                Origin = src.Origin,
                Width = src.Width,
                Height = src.Height,
                Nodes = src.Nodes,
                Values = src.Values
            };
        }

        public override void Update()
        {
            base.Update();
            if (flyingAway)
            {
                return;
            }

            Level level = SceneAs<Level>();
            Session session = level.Session;

            // Already settled this session, or grabbed by the player (state frozen,
            // matching the vanilla no-dash golden's post-grab wing fold): do not fly.
            if (session.Strawberries.Contains(ID) || Follower.Leader != null)
            {
                return;
            }

            // A push block dash interaction happened after this room was entered ->
            // fly away immediately (this attempt is void).
            if (session.GetCounter(SDDSPushBlockTracker.TotalKey)
                > session.GetCounter(SDDSPushBlockTracker.PinKey))
            {
                flyingAway = true;
                Audio.Play("event:/game/general/strawberry_laugh", Position);
                Add(new Coroutine(FlyAwayRoutine()));
            }
        }

        private IEnumerator FlyAwayRoutine()
        {
            Audio.Play("event:/game/general/strawberry_flyaway", Position);
            float speed = 60f;
            while (Y > (float)(SceneAs<Level>().Bounds.Top - 24))
            {
                speed = Calc.Approach(speed, 240f, 400f * Engine.DeltaTime);
                Y -= speed * Engine.DeltaTime;
                yield return null;
            }
            RemoveSelf();
        }
    }
}
