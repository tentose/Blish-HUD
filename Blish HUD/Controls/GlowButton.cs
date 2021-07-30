﻿using System;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls {
    public class GlowButton : Control, ICheckable {

        private const int BUTTON_WIDTH = 32;
        private const int BUTTON_HEIGHT = 32;

        public event EventHandler<CheckChangedEvent> CheckedChanged;

        private void OnChecked(CheckChangedEvent e) {
            CheckedChanged?.Invoke(this, e);
        }

        private bool _checked = false;
        public bool Checked {
            get => _checked;
            set {
                if (SetProperty(ref _checked, value)) {
                    if (_toggleGlow && _activeIcon == null) {
                        _spriteBatchParameters.Effect = _checked
                                                            ? GetGlowEffect()
                                                            : null;
                    }

                    OnChecked(new CheckChangedEvent(_checked));
                } 
            }
        }

        protected AsyncTexture2D _icon;
        public AsyncTexture2D Icon {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        protected AsyncTexture2D _activeIcon;
        /// <summary>
        /// If provided, this icon will be shown when the button is active.  If not provided, a basic glow shader will be used instead.
        /// </summary>
        public AsyncTexture2D ActiveIcon {
            get => _activeIcon;
            set => SetProperty(ref _activeIcon, value);
        }

        protected Color _glowColor = Color.White;
        /// <summary>
        /// Used to change the tint of the glow generated by the glow shader.  Is not used if <see cref="ActiveIcon"/> is defined.
        /// </summary>
        public Color GlowColor {
            get => _glowColor;
            set {
                if (SetProperty(ref _glowColor, value)) {
                    // Need to update the DrawEffect if it's currently active
                    _glowEffect?.Parameters["GlowColor"].SetValue(_glowColor.ToVector4());
                }
            }
        }

        protected bool _toggleGlow = false;
        public bool ToggleGlow {
            get => _toggleGlow;
            set => SetProperty(ref _toggleGlow, value);
        }

        private static Effect _glowEffect;
        private Effect GetGlowEffect() {
            _glowEffect ??= GameService.Content.ContentManager.Load<Effect>(@"effects\glow");
            _glowEffect.Parameters["TextureWidth"].SetValue((float)this.Width);
            _glowEffect.Parameters["GlowColor"].SetValue(_glowColor.ToVector4());
            _glowEffect.Parameters["Opacity"].SetValue(this.Opacity);

            return _glowEffect;
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        public GlowButton() {
            _spriteBatchParameters = new SpriteBatchParameters(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            this.Size = new Point(BUTTON_WIDTH, BUTTON_HEIGHT);
        }

        protected override void OnMouseEntered(MouseEventArgs e) {
            if (!_toggleGlow && _activeIcon == null) {
                _spriteBatchParameters.Effect = GetGlowEffect();
            }

            base.OnMouseEntered(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            if (!_toggleGlow) {
                _spriteBatchParameters.Effect = null;
            }

            base.OnMouseLeft(e);
        }

        protected override void OnClick(MouseEventArgs e) {
            if (_toggleGlow) {
                // TODO: A different sound should be played for toggle
                Content.PlaySoundEffectByName(@"button-click");
                this.Checked = !_checked;
            } else {
                Content.PlaySoundEffectByName(@"button-click");
            }

            base.OnClick(e);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            if (_icon != null) {
                var drawIcon = _icon;

                if (_activeIcon != null) {
                    if (!_toggleGlow && MouseOver) drawIcon = _activeIcon;
                    if (_toggleGlow && _checked) drawIcon = _activeIcon;
                }

                spriteBatch.DrawOnCtrl(this, drawIcon, bounds);
            }
        }

    }
}
