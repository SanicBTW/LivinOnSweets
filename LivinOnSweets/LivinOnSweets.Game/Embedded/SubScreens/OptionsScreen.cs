using System;
using System.Numerics;
using JetBrains.Annotations;
using LivinOnSweets.API.Components;
using LivinOnSweets.API.Configuration;
using LivinOnSweets.API.Enums;
using LivinOnSweets.API.Graphics.Sprites;
using LivinOnSweets.API.Graphics.Sprites.Embed;
using LivinOnSweets.API.Graphics.UserInterface;
using LivinOnSweets.API.Input;
using LivinOnSweets.API.Screens;
using LivinOnSweets.API.Skinning;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Framework.Screens;
using Vector2 = osuTK.Vector2;

namespace LivinOnSweets.Game.Embedded.SubScreens
{
    public partial class OptionsScreen : SweetSubScreen, IKeyBindingHandler<ManiaAction>
    {
        private static FontUsage defaultFont => new(family: "DNFBitBit", size: 52);

        [Resolved]
        private GameStateManager stateManager { get; set; }

        private bool backing;

        [BackgroundDependencyLoader]
        private void load(IResourcePackSource pack)
        {
            Texture texture = pack.GetTexture("OptionsModal/Popup", WrapMode.None, WrapMode.None, false, false, TextureFilteringMode.Nearest);
            texture.ScaleAdjust = 1;

            InternalChildren =
            [
                new Sprite()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = texture,
                },

                new FillFlowContainer()
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    X = 230,
                    Y = -22,
                    Spacing = new Vector2(0, 52),
                    Children =
                    [
                        // pending to implement, but its pretty easy since its just cycling between a set array of locales and refreshing the confirm button (afaik?)
                        new OptionsSelectorContainer<string>("LANGUAGE", FrameworkSetting.Locale) { ValueSanitize = sanitizeLanguage },
                        new OptionsSliderContainer<double>("MASTER", FrameworkSetting.VolumeUniversal), // this is a quick addition to this version, since osu framework handles 3 different volume types, a master volume slider would be useful
                        new OptionsSliderContainer<double>("BGM", FrameworkSetting.VolumeMusic),
                        new OptionsSliderContainer<double>("SFX", FrameworkSetting.VolumeEffect),
                        new OptionsSelectorContainer<double>("SYNC", null, SweetSetting.AudioOffset) { ValueSanitize = sanitizeSync, Action = syncAction },
                    ]
                },

                new PixelButton(PixelButtonType.Confirm)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Y = -70,
                    Action = () => OnPressed(new KeyBindingPressEvent<ManiaAction>(new InputState(), ManiaAction.BACK))
                }
            ];

            Y = texture.DisplayHeight;
        }

        private string sanitizeLanguage(string locale) => locale.ToUpperInvariant();

        private string sanitizeSync(double sync) => $"{(double.IsPositive(sync) ? "+" : "")}{sync:0.0}";

        private void syncAction(Bindable<double> bindable, bool isRight)
        {
            BindableDouble bindableDouble = bindable as BindableDouble;
            if (bindableDouble == null)
                return;

            double amount = bindableDouble.Precision * (isRight ? 1 : -1);
            bindableDouble.Add(amount);
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            if (e.Action != ManiaAction.BACK || backing)
                return false;

            this.MoveToY(DrawHeight, 400D, Easing.InOutCubic);
            Exit();
            backing = true;
            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e) { }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            this.MoveToY(0, 400D, Easing.InOutCubic);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            stateManager.GameplayMachine.UpdateState(true);
            return false;
        }

        private partial class OptionsSliderContainer<T>(
            string label,
            FrameworkSetting targetSetting,
            SweetSetting? sweetTarget = null) : Container
            where T : struct, INumber<T>, IMinMaxValue<T>
        {
            [BackgroundDependencyLoader]
            private void load(FrameworkConfigManager configManager, SweetConfigManager sweetConfig)
            {
                Anchor = Origin = Anchor.Centre;

                Width = 838;
                AutoSizeAxes = Axes.Y;

                Children =
                [
                    // im such a fucking mastermind gng - nah you are not brototype
                    new Container()
                    {
                        Width = Width / 3.8F,
                        AutoSizeAxes = Axes.Y,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Child = new SpriteText()
                        {
                            Text = label,
                            Font = defaultFont,
                            Colour = Colour4.Black,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    },
                    new PixelSliderBar<T>()
                    {
                        Current = sweetTarget.HasValue ? sweetConfig.GetBindable<T>(sweetTarget.Value) : configManager.GetBindable<T>(targetSetting),
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                    }
                ];
            }
        }

        private partial class OptionsSelectorContainer<T>(
            string label,
            FrameworkSetting? targetSetting = null,
            SweetSetting? sweetTarget = null) : Container
        {
            private Bindable<T> bindable;

            private SpriteText valueRepr;

            [CanBeNull] public Func<T, string> ValueSanitize;
            [CanBeNull] public Action<Bindable<T>, bool> Action;

            [BackgroundDependencyLoader]
            private void load(FrameworkConfigManager configManager, SweetConfigManager sweetConfig)
            {
                Anchor = Origin = Anchor.Centre;

                Width = 838;
                AutoSizeAxes = Axes.Y;

                if (targetSetting.HasValue)
                    bindable = configManager.GetBindable<T>(targetSetting.Value);
                else if (sweetTarget.HasValue)
                    bindable = sweetConfig.GetBindable<T>(sweetTarget.Value);
                else
                    throw new NotSupportedException(); // we MUST receive one of it

                Children =
                [
                    new Container()
                    {
                        Width = Width / 3.8F,
                        AutoSizeAxes = Axes.Y,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Child = new SpriteText()
                        {
                            Text = label,
                            Font = defaultFont,
                            Colour = Colour4.Black,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    },
                    new Container()
                    {
                        Width = Width / 1.86F,
                        AutoSizeAxes = Axes.Y,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Children =
                        [
                            new FramedButton("OptionsModal/ArrowSelect") { Anchor = Anchor.CentreLeft, Origin = Anchor.CentreRight, Rotation = -180, Action = () => Action?.Invoke(bindable, false) },
                            valueRepr = new SpriteText()
                            {
                                Text = "?",
                                Font = defaultFont,
                                Colour = Colour4.Black,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                            new FramedButton("OptionsModal/ArrowSelect") { Anchor = Anchor.CentreRight, Origin = Anchor.CentreRight, Action = () => Action?.Invoke(bindable, true) },
                        ]
                    },
                ];

                bindable.BindValueChanged(updateValue, true);
            }

            private void updateValue(ValueChangedEvent<T> ev)
            {
                valueRepr.Text = ValueSanitize?.Invoke(ev.NewValue) ?? ev.NewValue?.ToString();
            }
        }
    }
}
