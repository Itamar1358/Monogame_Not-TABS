using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame2026_Heb.Content;

namespace MonoGame2026_Heb;

public class UIManager : IUpdatable, IDrawable
{
    // ============ Variables & References ==================================================================================================================
    public enum Team { Blue, Red }
    
    private int currentManaBlue = 500;
    private int currentManaRed = 500;
    private MouseState previousMouseState;
    public SpriteFont font;
    
    private List<UIButton> buttons = new List<UIButton>();
    private UIButton selectedButton = null;
    
    private Texture2D dummyTexture;
    
    // ========================================================================================================================================================
    
    private class UIButton 
    {
        public Rectangle Bounds;
        public string Name;
        public int Cost;
        public Color BaseColor = Color.LightGray;
        public Team team;
    }

    public UIManager() // Create the buttons for all the units
    {
        int screenWidth = Game1.ScreenWidth;
        int screenHeight = Game1.ScreenHeight;
        int buttonWidth = 280;
        int buttonHeight = 110;
        int buttonY = screenHeight - buttonHeight - 10;
        
        // Blue Player Buttons
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(10, buttonY, buttonWidth, buttonHeight), 
            Name = "Knight", 
            Cost = 25,
            team = Team.Blue
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(10 + (buttonWidth + 10) * 1, buttonY, buttonWidth, buttonHeight), 
            Name = "Ogre", 
            Cost = 75,
            team = Team.Blue
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(10 + (buttonWidth + 10) * 2, buttonY, buttonWidth, buttonHeight), 
            Name = "Wizard", 
            Cost = 100,
            team = Team.Blue
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(10 + (buttonWidth + 10) * 3, buttonY, buttonWidth, buttonHeight), 
            Name = "Hypnotist", 
            Cost = 125,
            team = Team.Blue
        });
        
        // Red Player Buttons
        int rightEdge = screenWidth - 10;
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(rightEdge - buttonWidth * 4 - 30, buttonY, buttonWidth, buttonHeight), 
            Name = "Knight", 
            Cost = 25,
            team = Team.Red
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(rightEdge - buttonWidth * 3 - 20, buttonY, buttonWidth, buttonHeight), 
            Name = "Ogre", 
            Cost = 75,
            team = Team.Red
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(rightEdge - buttonWidth * 2 - 10, buttonY, buttonWidth, buttonHeight), 
            Name = "Wizard", 
            Cost = 100,
            team = Team.Red
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(rightEdge - buttonWidth, buttonY, buttonWidth, buttonHeight), 
            Name = "Hypnotist", 
            Cost = 125,
            team = Team.Red
        });
    }
    
    public void Start() 
    { 
        previousMouseState = Mouse.GetState();
    }
    
    public void Update(GameTime gameTime)
    {
        MouseState currentMouseState = Mouse.GetState();
        bool isLeftClick = currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
        bool isRightClick = currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released;

        if (isRightClick && selectedButton != null) // Right-clicking cancels the current placement
        {
            CancelPlacement();
        }

        if (isLeftClick)
        {
            Point mousePos = new Point(currentMouseState.X, currentMouseState.Y);
            bool clickedOnUI = false;

            // Unit not selected - Check for button pressed -----------------------------------------------------------------------
            
            foreach (var button in buttons) // Check if user clicked on any UI button
            {
                if (button.Bounds.Contains(mousePos))
                {
                    clickedOnUI = true;
                    
                    if (selectedButton == button) // If clicked the selected button again, it cancels the placement
                    {
                        CancelPlacement();
                        break;
                    }
                    if (selectedButton != null) // If clicked a different button, refund its cost first
                    {
                        CancelPlacement();
                    }

                    if (button.team == Team.Blue) // Clicked on Blue Unit Button
                    {
                        if (currentManaBlue >= button.Cost)
                        {
                            currentManaBlue -= button.Cost;
                            selectedButton = button;
                            Console.WriteLine($"Blue Player Picked up {button.Name}");
                        }
                        else { Console.WriteLine("Not enough mana!"); }
                        break;
                    }
                    if (button.team == Team.Red) // Clicked on Red Unit Button
                    {
                        if (currentManaRed >= button.Cost)
                        {
                            currentManaRed -= button.Cost;
                            selectedButton = button;
                            Console.WriteLine($"Red Player Picked up {button.Name}");
                        }
                        else { Console.WriteLine("Not enough mana!"); }
                        break;
                    }
                }
            }
            // If Unit selected - Place Unit on map -----------------------------------------------------------------------
            
            if (!clickedOnUI && selectedButton != null && selectedButton.team == Team.Blue) 
            {
                // Place unit if clicking on the Blue player's side of the map (left side)
                if (currentMouseState.X < Game1._screenCenter.X)
                {
                    PlaceUnit(selectedButton.Name, new Vector2(currentMouseState.X, currentMouseState.Y), Team.Blue);
                    selectedButton = null; // Successfully placed, reset selection
                }
                else { CancelPlacement(); }
            }
            
            if (!clickedOnUI && selectedButton != null && selectedButton.team == Team.Red) 
            {
                // Place unit if clicking on the Red player's side of the map (Right side)
                if (currentMouseState.X > Game1._screenCenter.X)
                {
                    PlaceUnit(selectedButton.Name, new Vector2(currentMouseState.X, currentMouseState.Y), Team.Red);
                    selectedButton = null; // Successfully placed, reset selection
                }
                else { CancelPlacement(); }
            }
            // --------------------------------------------------------------------------------------------------------------
        }
        previousMouseState = currentMouseState;
    }
    
    private void CancelPlacement() // Cancel placement and return mana
    {
        if (selectedButton != null)
        {
            if (selectedButton.team == Team.Blue) { currentManaBlue += selectedButton.Cost; }
            else  { currentManaRed += selectedButton.Cost; }
            
            selectedButton = null;
        }
    }
    
    private void PlaceUnit(string name, Vector2 position, Team team) // Instantiate & place unit on map
    {
        Unit newUnit = null;

        if (name == "Knight") newUnit = SceneManager.Create<Knight>();
        else if (name == "Ogre") newUnit = SceneManager.Create<Ogre>();
        else if (name == "Wizard") newUnit = SceneManager.Create<Wizard>();
        else if (name == "Hypnotist") newUnit = SceneManager.Create<Hypnotist>();
        
        if (newUnit != null)
        {
            if (team == Team.Blue)
            {
                newUnit.InitializeUnit(position, Unit.Team.Blue);
                Console.WriteLine($"Blue Player Placed {name} at {position}");
            }
            else
            {
                newUnit.InitializeUnit(position, Unit.Team.Red);
                Console.WriteLine($"Red Player Placed {name} at {position}");
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (dummyTexture == null)
        {
            dummyTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            dummyTexture.SetData(new[] { Color.White });
        }
        
        if (font != null) // Draw current mana
        {
            spriteBatch.DrawString(font, $"Blue Player`s Mana: {currentManaBlue}", new Vector2(10, 10), Color.Cyan);
            
            string redManaText = $"Red Player`s Mana: {currentManaRed}";
            Vector2 redManaSize = font.MeasureString(redManaText);
            spriteBatch.DrawString(font, redManaText, new Vector2(Game1.ScreenWidth - redManaSize.X - 10, 10), Color.Cyan);
        }
        
        foreach (var button in buttons) // Draw UI Buttons
        {
            if (button.team == Team.Blue)
            {
                // Green if selected, Gray if affordable, Dark Red if not affordable
                Color buttonColor = (selectedButton == button)
                    ? Color.LimeGreen
                    : (currentManaBlue >= button.Cost ? button.BaseColor : Color.DarkRed);
                
                Spritesheet buttonSprite = SpriteManager.GetSprite("CustomButton");
                if (buttonSprite != null)
                {
                    spriteBatch.Draw(buttonSprite.texture, button.Bounds, buttonColor);
                }
                else
                {
                    spriteBatch.Draw(dummyTexture, button.Bounds, buttonColor);
                }
            }
            if (button.team == Team.Red)
            {
                // Green if selected, Gray if affordable, Dark Red if not affordable
                Color buttonColor = (selectedButton == button)
                    ? Color.LimeGreen
                    : (currentManaRed >= button.Cost ? button.BaseColor : Color.DarkRed);

                Spritesheet buttonSprite = SpriteManager.GetSprite("CustomButton");
                if (buttonSprite != null)
                {
                    spriteBatch.Draw(buttonSprite.texture, button.Bounds, buttonColor);
                }
                else
                {
                    spriteBatch.Draw(dummyTexture, button.Bounds, buttonColor);
                }
            }

            if (font != null)
            {
                string text = $"{button.Name}\n({button.Cost})";
                Vector2 textSize = font.MeasureString(text);
                
                // Calculate scale to ensure text fits inside the button bounds (accounting for thick button borders)
                float scaleX = (button.Bounds.Width - 80) / textSize.X;
                float scaleY = (button.Bounds.Height - 55) / textSize.Y;
                float scale = Math.Min(1.0f, Math.Min(scaleX, scaleY));
                
                Vector2 scaledTextSize = textSize * scale;
                
                Vector2 textPos = new Vector2(
                    button.Bounds.X + (button.Bounds.Width - scaledTextSize.X) / 2,
                    button.Bounds.Y + (button.Bounds.Height - scaledTextSize.Y) / 2
                );
                
                spriteBatch.DrawString(
                    font, 
                    text, 
                    textPos, 
                    Color.White, 
                    0f, 
                    Vector2.Zero, 
                    scale, 
                    SpriteEffects.None, 
                    0f
                );
            }
        }
        
        // Draw a text preview on the mouse cursor while placing
        if (selectedButton != null && font != null)
        {
            MouseState mouse = Mouse.GetState();
            spriteBatch.DrawString(font, $"Placing {selectedButton.Name}...", new Vector2(mouse.X + 15, mouse.Y + 15), Color.Yellow);
        }
    }
}
