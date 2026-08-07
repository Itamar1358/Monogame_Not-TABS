using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame2026_Heb.Content;

namespace MonoGame2026_Heb;

public class InstructionsManager : IUpdatable, IDrawable
{
    // ============ Variables & References ==================================================================================================================
    
    public SpriteFont font;
    private MouseState previousMouseState;
    private Rectangle backButtonBounds;

    // =======================================================================================================================================================

    public void Start() // Initializes the layout and bounds for the instructions screen back button
    {
        previousMouseState = Mouse.GetState();
        int buttonWidth = 600;
        int buttonHeight = 130;
        backButtonBounds = new Rectangle((Game1.ScreenWidth - buttonWidth) / 2, Game1.ScreenHeight - buttonHeight - 40, buttonWidth, buttonHeight);
    }

    public void Update(GameTime gameTime) // Listens for clicks on the back button to return to the main menu
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

    public void Draw(SpriteBatch spriteBatch) // Renders the controls text, gameplay explanation, and back button
    {
        if (font == null) return;
        string title = "HOW TO PLAY";
        Vector2 titleSize = font.MeasureString(title);
        Vector2 titlePos = new Vector2((Game1.ScreenWidth - titleSize.X * 2f) / 2, Game1.ScreenHeight * 0.1f);
        spriteBatch.DrawString(font, title, titlePos, Color.Gold, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

        string instructions = "Controls:\n" +
                              "- Left Click: Select units and place them on the battlefield.\n" +
                              "- Escape: Exit the game.\n\n" +
                              "Gameplay:\n" +
                              "1. Select a unit from the bottom menu.\n" +
                              "2. Click anywhere on your side of the screen to spend mana and place it.\n" +
                              "3. Click the 'Play' button in the top left to start the battle.\n" +
                              "4. Watch your units fight automatically until one side wins!";

        Vector2 instructionsSize = font.MeasureString(instructions);
        Vector2 instructionsPos = new Vector2((Game1.ScreenWidth - instructionsSize.X) / 2, Game1.ScreenHeight * 0.3f);
        spriteBatch.DrawString(font, instructions, instructionsPos, Color.White);

        Spritesheet buttonSprite = SpriteManager.GetSprite("CustomButton");
        MouseState mouseState = Mouse.GetState();

        Color color = backButtonBounds.Contains(mouseState.X, mouseState.Y) ? Color.LightGray : Color.White;
        
        if (buttonSprite != null) { spriteBatch.Draw(buttonSprite.texture, backButtonBounds, color); }

        Vector2 textSize = font.MeasureString("Back");
        float scale = Math.Min((backButtonBounds.Width - 300) / textSize.X, (backButtonBounds.Height - 60) / textSize.Y);
        Vector2 textPos = new Vector2(
            backButtonBounds.X + (backButtonBounds.Width - textSize.X * scale) / 2,
            backButtonBounds.Y + (backButtonBounds.Height - textSize.Y * scale) / 2
        );
        spriteBatch.DrawString(font, "Back", textPos, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}
