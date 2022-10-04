﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    [Cached]
    public class SmokeContainer : Container, IRequireHighFrequencyMousePosition, IKeyBindingHandler<OsuAction>
    {
        public event Action<Vector2, double>? SmokeMoved;
        public event Action<double>? SmokeEnded;

        public Vector2 LastMousePosition;

        private bool isSmoking;

        public override bool ReceivePositionalInputAt(Vector2 _) => true;

        public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
        {
            if (e.Action == OsuAction.Smoke)
            {
                Logger.Log("holy moly");

                isSmoking = true;
                AddInternal(new SkinnableDrawable(new OsuSkinComponent(OsuSkinComponents.Smoke), _ => new DefaultSmoke()));

                return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
        {
            if (e.Action == OsuAction.Smoke)
            {
                isSmoking = false;
                SmokeEnded?.Invoke(Time.Current);

                foreach (SkinnableDrawable skinnable in Children)
                    skinnable.LifetimeEnd = skinnable.Drawable.LifetimeEnd;
            }
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (isSmoking)
                SmokeMoved?.Invoke(e.MousePosition, Time.Current);

            LastMousePosition = e.MousePosition;

            return base.OnMouseMove(e);
        }
    }
}
