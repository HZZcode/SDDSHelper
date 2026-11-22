using Celeste;
using Monocle;

namespace SDDSHelper
{
    /// <summary>
    /// Push block dash-interaction counter.
    ///
    /// CanyonHelper's PushBlock executes
    ///   level.Session.IncrementCounter("pushBlocksHit");
    /// on every successful dash hit, and CanyonModule rolls that counter back to its
    /// value at spawn time when the player dies (the "every life starts clean" semantics).
    ///
    /// The vanilla no-dash golden instead relies on Session.Dashes, which accumulates
    /// across rooms and only rolls back within a room (on death). To get the same
    /// judgement, this entity diffs CanyonHelper's counter every frame and folds only
    /// positive deltas into our own session counter TotalKey (decrements caused by the
    /// death rollback are ignored, so history survives deaths and room changes).
    /// </summary>
    [Tracked]
    public class SDDSPushBlockTracker : Entity
    {
        /// <summary>Session counter key incremented by CanyonHelper.PushBlock.</summary>
        public const string CanyonCounterKey = "pushBlocksHit";

        /// <summary>Our cumulative key: total push block dash interactions this session, monotonic.</summary>
        public const string TotalKey = "SDDSHelper/pushBlocksHitTotal";

        /// <summary>Our baseline key: snapshot of the total taken on the last screen transition (current room's entry baseline).</summary>
        public const string PinKey = "SDDSHelper/pushBlocksHitPin";

        private int lastSeen;

        public override void Added(Scene scene)
        {
            base.Added(scene);
            // Diff against CanyonHelper's counter as of creation, so that respawn after
            // death (when CanyonHelper has already rolled its counter back) or resuming
            // a save never re-accumulates historical values.
            lastSeen = (Scene as Level)?.Session.GetCounter(CanyonCounterKey) ?? 0;
        }

        public override void Update()
        {
            base.Update();

            Level level = Scene as Level;
            if (level == null)
            {
                return;
            }

            Session session = level.Session;
            int current = session.GetCounter(CanyonCounterKey);
            if (current > lastSeen)
            {
                // Take the positive delta only; drops caused by CanyonHelper's death
                // rollback never reduce the cumulative total.
                session.SetCounter(TotalKey, session.GetCounter(TotalKey) + (current - lastSeen));
            }
            lastSeen = current;
        }
    }
}
