// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist
{
    public class QueueList : DrawableRoomPlaylist
    {
        public readonly Bindable<QueueMode> QueueMode = new Bindable<QueueMode>();

        public QueueList(bool allowEdit, bool allowSelection, bool reverse = false)
            : base(allowEdit, allowSelection, reverse)
        {
        }

        protected override FillFlowContainer<RearrangeableListItem<PlaylistItem>> CreateListFillFlowContainer() => new QueueFillFlowContainer
        {
            QueueMode = { BindTarget = QueueMode },
            Spacing = new Vector2(0, 2)
        };

        private class QueueFillFlowContainer : FillFlowContainer<RearrangeableListItem<PlaylistItem>>
        {
            public readonly IBindable<QueueMode> QueueMode = new Bindable<QueueMode>();

            protected override void LoadComplete()
            {
                base.LoadComplete();
                QueueMode.BindValueChanged(_ => InvalidateLayout());
            }

            public override IEnumerable<Drawable> FlowingChildren
            {
                get
                {
                    switch (QueueMode.Value)
                    {
                        default:
                            return AliveInternalChildren.Where(d => d.IsPresent)
                                                        .OfType<RearrangeableListItem<PlaylistItem>>()
                                                        .OrderBy(item => item.Model.ID);

                        case Game.Online.Multiplayer.QueueMode.AllPlayersRoundRobin:
                            // TODO: THIS IS SO INEFFICIENT, can it be done any better?

                            // Group all items by their owners.
                            var groups = AliveInternalChildren.Where(d => d.IsPresent)
                                                              .OfType<RearrangeableListItem<PlaylistItem>>()
                                                              .GroupBy(item => item.Model.OwnerID)
                                                              .Select(g => g.ToArray())
                                                              .ToArray();

                            if (groups.Length == 0)
                                return Enumerable.Empty<Drawable>();

                            // Find the initial picking order for the groups. The group with the smallest 'weight' picks first.
                            int[] groupWeights = new int[groups.Length];

                            for (int i = 0; i < groups.Length; i++)
                            {
                                groupWeights[i] = groups[i].Count(item => item.Model.Expired);
                                groups[i] = groups[i].Where(item => !item.Model.Expired).ToArray();
                            }

                            var result = new List<Drawable>();

                            // Simulate the playlist by picking in order from the smallest-weighted room each time until no longer able to.
                            while (true)
                            {
                                var candidateGroup = groups
                                                     // Map each group to an index.
                                                     .Select((items, index) => new { index, items })
                                                     // Order groups by their weights.
                                                     .OrderBy(group => groupWeights[group.index])
                                                     // Select the first group with remaining items (null is set from previous iterations).
                                                     .FirstOrDefault(group => group.items.Any(i => i != null));

                                // Iteration ends when all groups have been exhausted of items.
                                if (candidateGroup == null)
                                    break;

                                // Find the index of the first non-null (i.e. unused) item in the group.
                                int candidateItemIndex = 0;
                                RearrangeableListItem<PlaylistItem> candidateItem = null;

                                for (int i = 0; i < candidateGroup.items.Length; i++)
                                {
                                    if (candidateGroup.items[i] != null)
                                    {
                                        candidateItemIndex = i;
                                        candidateItem = candidateGroup.items[i];
                                    }
                                }

                                // The item is guaranteed to not be expired, since we've previously removed all expired items.
                                Debug.Assert(candidateItem?.Model.Expired == false);

                                // Add the item to the result set.
                                result.Add(candidateItem);

                                // Update the group for the next iteration.
                                candidateGroup.items[candidateItemIndex] = null;
                                groupWeights[candidateGroup.index]++;
                            }

                            return result;
                    }
                }
            }
        }
    }
}
