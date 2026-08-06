using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame2026_Heb.Content;

namespace MonoGame2026_Heb;

public class MainMenuManager : IUpdatable, IDrawable
{
    public SpriteFont font;
    private List<MenuButton> buttons = new();
    private MouseState previousMouseState;

    private class MenuButton
    {
        public string Text;
        public Rectangle Bounds;
        public Action OnClick;
    }

    public void Start()
    {
        int screenWidth = Game1.ScreenWidth;
        int screenHeight = Game1.ScreenHeight;
        int buttonWidth = 600;
        int buttonHeight = 130;
        int spacing = 50;

        int startY = screenHeight / 2 - (buttonHeight * 2 + spacing * 1) + 100;

        buttons.Add(new MenuButton {
            Text = "Start Game",
            Bounds = new Rectangle((screenWidth - buttonWidth) / 2, startY, buttonWidth, buttonHeight),
            OnClick = () => Game1.Instance.LoadGame()
        });

        buttons.Add(new MenuButton {
            Text = "Unit Manual",
            Bounds = new Rectangle((screenWidth - buttonWidth) / 2, startY + (buttonHeight + spacing) * 1, buttonWidth, buttonHeight),
            OnClick = () => Game1.Instance.LoadUnitManual()
        });

        buttons.Add(new MenuButton {
            Text = "Settings",
            Bounds = new Rectangle((screenWidth - buttonWidth) / 2, startY + (buttonHeight + spacing) * 2, buttonWidth, buttonHeight),
            OnClick = () => Console.WriteLine("Settings coming soon!")
        });

        buttons.Add(new MenuButton {
            Text = "Exit",
            Bounds = new Rectangle((screenWidth - buttonWidth) / 2, startY + (buttonHeight + spacing) * 3, buttonWidth, buttonHeight),
            OnClick = () => Game1.Instance.Exit()
        });
    }

    public void Update(GameTime gameTime)
    {
        MouseState currentMouseState = Mouse.GetState();

        if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
        {
            Point mousePos = new Point(currentMouseState.X, currentMouseState.Y);
            
            foreach (var button in buttons)
            {
                if (button.Bounds.Contains(mousePos))
                {
                    AudioManager.PlaySFX?.Invoke("ButtonSFX");
                    button.OnClick?.Invoke();
                    break;
                }
            }
        }

        previousMouseState = currentMouseState;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (font == null) return;
        
        string title = "FANTASY BRAWLERS";
        Vector2 titleSize = font.MeasureString(title);
        Vector2 titlePos = new Vector2((Game1.ScreenWidth - titleSize.X * 2f) / 2, Game1.ScreenHeight * 0.15f);
        spriteBatch.DrawString(font, title, titlePos, Color.Gold, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

        Spritesheet buttonSprite = SpriteManager.GetSprite("CustomButton");
        MouseState mouseState = Mouse.GetState();

        foreach (var button in buttons)
        {
            Color color = button.Bounds.Contains(mouseState.X, mouseState.Y) ? Color.LightGray : Color.White;
            
            if (buttonSprite != null)
            {
                spriteBatch.Draw(buttonSprite.texture, button.Bounds, color);
            }

            Vector2 textSize = font.MeasureString(button.Text);
            // The button texture has thick borders, so we subtract 60 from height and 80 from width to constrain the text size
            float scale = Math.Min((button.Bounds.Width - 80) / textSize.X, (button.Bounds.Height - 60) / textSize.Y);
            Vector2 textPos = new Vector2(
                button.Bounds.X + (button.Bounds.Width - textSize.X * scale) / 2,
                button.Bounds.Y + (button.Bounds.Height - textSize.Y * scale) / 2
            );

            spriteBatch.DrawString(font, button.Text, textPos, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
