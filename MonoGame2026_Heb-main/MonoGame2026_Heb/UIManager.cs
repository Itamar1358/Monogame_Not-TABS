using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
        // Blue Player Buttons
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(10, 10, 150, 40), 
            Name = "Knight", 
            Cost = 25,
            team = Team.Blue
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(170, 10, 150, 40), 
            Name = "Ogre", 
            Cost = 75,
            team = Team.Blue
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(330, 10, 150, 40), 
            Name = "Wizard", 
            Cost = 100,
            team = Team.Blue
        });
        /*
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(490, 10, 150, 40), 
            Name = "Hypnotist", 
            Cost = 125,
            team = Team.Blue
        });
        */
        
        // Red Player Buttons
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(650, 10, 150, 40), 
            Name = "Knight", 
            Cost = 25,
            team = Team.Red
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(810, 10, 150, 40), 
            Name = "Ogre", 
            Cost = 75,
            team = Team.Red
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(970, 10, 150, 40), 
            Name = "Wizard", 
            Cost = 100,
            team = Team.Red
        });
        /*
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(1130, 10, 150, 40), 
            Name = "Hypnotist", 
            Cost = 125,
            team = Team.Red
        });
        */
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
        // else if (name == "Hypnotist") newUnit = SceneManager.Create<Hypnotist>();
        
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
            spriteBatch.DrawString(font, $"Blue Player`s Mana: {currentManaBlue}", new Vector2(10, 60), Color.Cyan);
            spriteBatch.DrawString(font, $"Red Player`s Mana: {currentManaRed}", new Vector2(10, 70), Color.Cyan);
        }
        
        foreach (var button in buttons) // Draw UI Buttons
        {
            if (button.team == Team.Blue)
            {
                // Green if selected, Gray if affordable, Dark Red if not affordable
                Color buttonColor = (selectedButton == button)
                    ? Color.LimeGreen
                    : (currentManaBlue >= button.Cost ? button.BaseColor : Color.DarkRed);

                spriteBatch.Draw(dummyTexture, button.Bounds, buttonColor);
            }
            if (button.team == Team.Red)
            {
                // Green if selected, Gray if affordable, Dark Red if not affordable
                Color buttonColor = (selectedButton == button)
                    ? Color.LimeGreen
                    : (currentManaRed >= button.Cost ? button.BaseColor : Color.DarkRed);

                spriteBatch.Draw(dummyTexture, button.Bounds, buttonColor);
            }

            if (font != null)
            {
                string text = $"{button.Name} ({button.Cost})";
                Vector2 textSize = font.MeasureString(text);
                
                Vector2 textPos = new Vector2(
                    button.Bounds.X + (button.Bounds.Width - textSize.X) / 2,
                    button.Bounds.Y + (button.Bounds.Height - textSize.Y) / 2
                );
                
                spriteBatch.DrawString(font, text, textPos, Color.Black);
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
