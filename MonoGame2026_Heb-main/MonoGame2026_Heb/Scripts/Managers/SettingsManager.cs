using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame2026_Heb.Content;

namespace MonoGame2026_Heb;

public class SettingsManager : IUpdatable, IDrawable
{
    public SpriteFont font;
    
    private Rectangle backButtonBounds;
    private Rectangle gammaPanelBounds;
    private Texture2D dummyTexture;
    
    private bool isDraggingMusic;
    private bool isDraggingSFX;
    private bool isDraggingGamma;
    
    private Rectangle musicPanelBounds;
    private Rectangle sfxPanelBounds;
    
    private MouseState previousMouseState;

    public int SortingOrder => 30000;

    public void Start()
    {
        previousMouseState = Mouse.GetState();
        
        int screenWidth = Game1.ScreenWidth;
        int screenHeight = Game1.ScreenHeight;
        int centerX = screenWidth / 2;
        int centerY = screenHeight / 2;
        
        // Track dimensions
        int buttonWidth = 600;
        int buttonHeight = 130;
        
        // Music Panel Setup
        musicPanelBounds = new Rectangle(centerX - buttonWidth / 2, centerY - 220, buttonWidth, buttonHeight);
        
        // SFX Panel Setup
        sfxPanelBounds = new Rectangle(centerX - buttonWidth / 2, centerY - 30, buttonWidth, buttonHeight);
        
        // Gamma Panel Setup
        gammaPanelBounds = new Rectangle(centerX - buttonWidth / 2, centerY + 160, buttonWidth, buttonHeight);
        
        // Back Button
        backButtonBounds = new Rectangle((screenWidth - buttonWidth) / 2, screenHeight - buttonHeight - 40, buttonWidth, buttonHeight);
    }

    public void Update(GameTime gameTime)
    {
        MouseState mouse = Mouse.GetState();
        Point mousePos = new Point(mouse.X, mouse.Y);
        
        // Handle Back Button
        if (mouse.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
        {
            if (backButtonBounds.Contains(mousePos) && !isDraggingMusic && !isDraggingSFX && !isDraggingGamma)
            {
                AudioManager.PlaySFX?.Invoke("ButtonSFX");
                Game1.Instance.LoadMainMenu();
                return;
            }
        }
        
        // Music Slider Dragging
        int padX = 100;
        int innerStartX = musicPanelBounds.X + padX;
        int innerWidth = musicPanelBounds.Width - (padX * 2);
        
        if (mouse.LeftButton == ButtonState.Pressed)
        {
            Rectangle expandedMusic = new Rectangle(musicPanelBounds.X - 20, musicPanelBounds.Y - 20, musicPanelBounds.Width + 40, musicPanelBounds.Height + 40);
            
            if (!isDraggingSFX && !isDraggingGamma && ((expandedMusic.Contains(mousePos) && previousMouseState.LeftButton == ButtonState.Released) || isDraggingMusic))
            {
                isDraggingMusic = true;
                float percent = (float)(mouse.X - innerStartX) / innerWidth;
                AudioManager.SetMusicVolume(MathHelper.Clamp(percent, 0f, 1f));
            }
        }
        else
        {
            if (isDraggingMusic)
            {
                Game1.SaveSettings();
                isDraggingMusic = false;
            }
        }
        
        // SFX Slider Dragging
        int sfxInnerStartX = sfxPanelBounds.X + padX;
        if (mouse.LeftButton == ButtonState.Pressed)
        {
            Rectangle expandedSFX = new Rectangle(sfxPanelBounds.X - 20, sfxPanelBounds.Y - 20, sfxPanelBounds.Width + 40, sfxPanelBounds.Height + 40);
            
            if (!isDraggingMusic && !isDraggingGamma && ((expandedSFX.Contains(mousePos) && previousMouseState.LeftButton == ButtonState.Released) || isDraggingSFX))
            {
                isDraggingSFX = true;
                float percent = (float)(mouse.X - sfxInnerStartX) / innerWidth;
                AudioManager.SetSFXVolume(MathHelper.Clamp(percent, 0f, 1f));
            }
        }
        else
        {
            if (isDraggingSFX)
            {
                // Play test sound when they release the SFX slider
                AudioManager.PlaySFX?.Invoke("ButtonSFX");
                Game1.SaveSettings();
                isDraggingSFX = false;
            }
        }
        
        // Gamma Slider Dragging
        int gammaInnerStartX = gammaPanelBounds.X + padX;
        if (mouse.LeftButton == ButtonState.Pressed)
        {
            Rectangle expandedGamma = new Rectangle(gammaPanelBounds.X - 20, gammaPanelBounds.Y - 20, gammaPanelBounds.Width + 40, gammaPanelBounds.Height + 40);
            
            if (!isDraggingMusic && !isDraggingSFX && ((expandedGamma.Contains(mousePos) && previousMouseState.LeftButton == ButtonState.Released) || isDraggingGamma))
            {
                isDraggingGamma = true;
                float percent = (float)(mouse.X - gammaInnerStartX) / innerWidth;
                Game1.Gamma = MathHelper.Clamp(percent, 0f, 1f);
            }
        }
        else
        {
            if (isDraggingGamma)
            {
                Game1.SaveSettings();
                isDraggingGamma = false;
            }
        }
        
        previousMouseState = mouse;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (dummyTexture == null)
        {
            dummyTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            dummyTexture.SetData(new[] { Color.White });
        }
        
        Spritesheet buttonSprite = SpriteManager.GetSprite("CustomButton");
        
        // Draw panels
        if (buttonSprite != null)
        {
            spriteBatch.Draw(buttonSprite.texture, musicPanelBounds, Color.White);
            spriteBatch.Draw(buttonSprite.texture, sfxPanelBounds, Color.White);
            spriteBatch.Draw(buttonSprite.texture, gammaPanelBounds, Color.White);
            
            int padX = 100;
            int padY = 35;
            int innerWidth = musicPanelBounds.Width - (padX * 2);
            int innerHeight = musicPanelBounds.Height - (padY * 2);
            
            // Draw fills inside the panels
            int innerMusicWidth = (int)(innerWidth * AudioManager.MusicVolume);
            Rectangle musicFill = new Rectangle(musicPanelBounds.X + padX, musicPanelBounds.Y + padY, innerMusicWidth, innerHeight);
            spriteBatch.Draw(dummyTexture, musicFill, Color.Gold * 0.8f);
            
            int innerSfxWidth = (int)(innerWidth * AudioManager.SFXVolume);
            Rectangle sfxFill = new Rectangle(sfxPanelBounds.X + padX, sfxPanelBounds.Y + padY, innerSfxWidth, innerHeight);
            spriteBatch.Draw(dummyTexture, sfxFill, Color.Gold * 0.8f);
            
            int innerGammaWidth = (int)(innerWidth * Game1.Gamma);
            Rectangle gammaFill = new Rectangle(gammaPanelBounds.X + padX, gammaPanelBounds.Y + padY, innerGammaWidth, innerHeight);
            spriteBatch.Draw(dummyTexture, gammaFill, Color.Gold * 0.8f);
        }
        
        // Draw title
        if (font != null)
        {
            string title = "Settings";
            Vector2 titleSize = font.MeasureString(title);
            spriteBatch.DrawString(font, title, new Vector2((Game1.ScreenWidth - titleSize.X * 1.5f) / 2, 50), Color.Gold, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0f);
            
            // Music Label
            string musicText = $"Music Volume: {(int)(AudioManager.MusicVolume * 100)}%";
            Vector2 mSize = font.MeasureString(musicText);
            spriteBatch.DrawString(font, musicText, new Vector2(musicPanelBounds.Center.X - mSize.X / 2, musicPanelBounds.Y - 60), Color.White);
            
            // SFX Label
            string sfxText = $"SFX Volume: {(int)(AudioManager.SFXVolume * 100)}%";
            Vector2 sSize = font.MeasureString(sfxText);
            spriteBatch.DrawString(font, sfxText, new Vector2(sfxPanelBounds.Center.X - sSize.X / 2, sfxPanelBounds.Y - 60), Color.White);
            
            // Gamma Label
            string gammaText = $"Gamma: {(int)(Game1.Gamma * 100)}%";
            Vector2 gSize = font.MeasureString(gammaText);
            spriteBatch.DrawString(font, gammaText, new Vector2(gammaPanelBounds.Center.X - gSize.X / 2, gammaPanelBounds.Y - 60), Color.White);
        }
        
        // Draw Back Button
        Color backColor = backButtonBounds.Contains(Mouse.GetState().X, Mouse.GetState().Y) ? Color.LightGray : Color.White;
        
        if (buttonSprite != null) spriteBatch.Draw(buttonSprite.texture, backButtonBounds, backColor);
        else spriteBatch.Draw(dummyTexture, backButtonBounds, backColor);
        
        if (font != null)
        {
            Vector2 textSize = font.MeasureString("Back");
            float scale = Math.Min((backButtonBounds.Width - 80) / textSize.X, (backButtonBounds.Height - 60) / textSize.Y);
            Vector2 textPos = new Vector2(
                backButtonBounds.X + (backButtonBounds.Width - textSize.X * scale) / 2,
                backButtonBounds.Y + (backButtonBounds.Height - textSize.Y * scale) / 2
            );
            spriteBatch.DrawString(font, "Back", textPos, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
