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
                Game1.Instance.LoadMainMenu();
            }
        }

        previousMouseState = currentMouseState;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (font == null) return;

        // Draw title
        string title = "UNIT MANUAL";
        Vector2 titleSize = font.MeasureString(title);
        Vector2 titlePos = new Vector2((Game1.ScreenWidth - titleSize.X * 1.5f) / 2, 30);
        spriteBatch.DrawString(font, title, titlePos, Color.Gold, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);

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
        int startY = 150;
        int spacingY = 240;
        int startX = Game1.ScreenWidth / 4;

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
            int textX = startX + 130;
            spriteBatch.DrawString(font, unit.Name, new Vector2(textX, yPos), Color.Cyan, 0f, Vector2.Zero, 1.2f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, unit.Stats, new Vector2(textX, yPos + 70), Color.LightGreen, 0f, Vector2.Zero, 0.9f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, unit.Description, new Vector2(textX, yPos + 120), Color.White, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
        }
    }
}
