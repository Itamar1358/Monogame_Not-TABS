using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame2026_Heb.Content;

namespace MonoGame2026_Heb;

public class UnitManualManager : IUpdatable, IDrawable
{
    public SpriteFont font;
    private Rectangle backButtonBounds;
    private MouseState previousMouseState;
    private Texture2D dummyTexture;
    
    public bool IsPopup = false;
    public Action OnBack;

    private class UnitInfo
    {
        public string Name;
        public string SpriteName;
        public string Stats;
        public string Description;
    }

    private List<UnitInfo> units = new();

    public void Start()
    {
        if (OnBack == null)
            OnBack = () => Game1.Instance.LoadMainMenu();
            
        previousMouseState = Mouse.GetState();
        units.Clear();
        
        int buttonWidth = 600;
        int buttonHeight = 130;
        backButtonBounds = new Rectangle((Game1.ScreenWidth - buttonWidth) / 2, Game1.ScreenHeight - buttonHeight - 40, buttonWidth, buttonHeight);

        units.Add(new UnitInfo {
            Name = "Knight",
            SpriteName = "Knight",
            Stats = "HP: 50 - DMG: 10 - Cost: 50 - Range: 100",
            Description = "A reliable melee fighter that charges directly into battle."
        });

        units.Add(new UnitInfo {
            Name = "Ogre",
            SpriteName = "Ogre",
            Stats = "HP: 200 - DMG: 30 - Cost: 100 - Range: 100",
            Description = "A massive, slow tank that deals heavy damage."
        });

        units.Add(new UnitInfo {
            Name = "Wizard",
            SpriteName = "Wizard",
            Stats = "HP: 30 - DMG: 15 - Cost: 75 - Range: 300",
            Description = "Shoots fireballs from a safe distance. Fragile but deadly."
        });

        units.Add(new UnitInfo {
            Name = "Hypnotist",
            SpriteName = "Hypnotist",
            Stats = "HP: 25 - DMG: 0 - Cost: 125 - Range: 250",
            Description = "Fires a hypnosis orb that temporarily turns enemies into allies!"
        });
    }

    public void Update(GameTime gameTime)
    {
        MouseState currentMouseState = Mouse.GetState();

        if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
        {
            Point mousePos = new Point(currentMouseState.X, currentMouseState.Y);
            
            if (backButtonBounds.Contains(mousePos))
            {
                AudioManager.PlaySFX?.Invoke("ButtonSFX");
                OnBack?.Invoke();
            }
        }

        previousMouseState = currentMouseState;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (font == null) return;

        // Draw dim overlay and big background if it's a popup
        if (IsPopup)
        {
            if (dummyTexture == null)
            {
                dummyTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                dummyTexture.SetData(new[] { Color.White });
            }
            spriteBatch.Draw(dummyTexture, new Rectangle(0, 0, Game1.ScreenWidth, Game1.ScreenHeight), Color.Black * 0.7f);

            Spritesheet bgSprite = SpriteManager.GetSprite("CustomButton");
            if (bgSprite != null)
            {
                Rectangle bgBounds = new Rectangle(
                    0, 
                    -10, 
                    Game1.ScreenWidth, 
                    Game1.ScreenHeight + 20
                );
                spriteBatch.Draw(bgSprite.texture, bgBounds, Color.White);
            }
        }

        // Draw title
        string title = "UNIT MANUAL";
        float titleScale = IsPopup ? 1.2f : 1.5f;
        Vector2 titleSize = font.MeasureString(title) * titleScale;
        float titleY = IsPopup ? (Game1.ScreenHeight * 0.15f) : 30f;
        Vector2 titlePos = new Vector2((Game1.ScreenWidth - titleSize.X) / 2, titleY);
        spriteBatch.DrawString(font, title, titlePos, Color.Gold, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

        // Draw Back Button
        Spritesheet buttonSprite = SpriteManager.GetSprite("CustomButton");
        MouseState mouseState = Mouse.GetState();
        Color backColor = backButtonBounds.Contains(mouseState.X, mouseState.Y) ? Color.LightGray : Color.White;
        
        if (buttonSprite != null) spriteBatch.Draw(buttonSprite.texture, backButtonBounds, backColor);
        
        Vector2 backTextSize = font.MeasureString("Back");
        float scale = Math.Min((backButtonBounds.Width - 80) / backTextSize.X, (backButtonBounds.Height - 60) / backTextSize.Y);
        Vector2 backTextPos = new Vector2(
            backButtonBounds.X + (backButtonBounds.Width - backTextSize.X * scale) / 2,
            backButtonBounds.Y + (backButtonBounds.Height - backTextSize.Y * scale) / 2
        );
        spriteBatch.DrawString(font, "Back", backTextPos, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

        // Draw Units
        int startY = IsPopup ? (int)(Game1.ScreenHeight * 0.25f) : 150;
        int spacingY = IsPopup ? (int)(Game1.ScreenHeight * 0.14f) : 240;
        
        float nameScale = IsPopup ? 0.9f : 1.2f;
        float statsScale = IsPopup ? 0.65f : 0.9f;
        float descScale = IsPopup ? 0.6f : 0.8f;
        
        float maxTextWidth = 0;
        foreach(var u in units) {
            float w = font.MeasureString(u.Description).X * descScale;
            if (w > maxTextWidth) maxTextWidth = w;
        }
        
        int totalBlockWidth = (int)(130 + maxTextWidth);
        int startX = IsPopup ? (Game1.ScreenWidth - totalBlockWidth) / 2 : Game1.ScreenWidth / 4;

        for (int i = 0; i < units.Count; i++)
        {
            var unit = units[i];
            int yPos = startY + (i * spacingY);

            // Draw Sprite
            Spritesheet sprite = SpriteManager.GetSprite(unit.SpriteName);
            if (sprite != null)
            {
                // Draw only the first frame of the spritesheet
                int frameWidth = sprite.texture.Width / sprite.columns;
                Rectangle sourceRect = new Rectangle(0, 0, frameWidth, sprite.texture.Height);
                Rectangle destRect = new Rectangle(startX, yPos, 100, 100);
                spriteBatch.Draw(sprite.texture, destRect, sourceRect, Color.White);
            }

            // Draw Text
            float statsYOffset = IsPopup ? 55f : 70f;
            float descYOffset = IsPopup ? 90f : 120f;
            
            int textX = startX + 130;
            spriteBatch.DrawString(font, unit.Name, new Vector2(textX, yPos), Color.Cyan, 0f, Vector2.Zero, nameScale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, unit.Stats, new Vector2(textX, yPos + statsYOffset), Color.LightGreen, 0f, Vector2.Zero, statsScale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, unit.Description, new Vector2(textX, yPos + descYOffset), Color.White, 0f, Vector2.Zero, descScale, SpriteEffects.None, 0f);
        }
    }
}
