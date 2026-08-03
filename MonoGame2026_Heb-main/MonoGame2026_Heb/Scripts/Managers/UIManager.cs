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
    
    private List<Unit> placedUnits = new List<Unit>();
    private Unit selectedPlacedUnit = null;
    
    private Texture2D dummyTexture;
    
    private Rectangle bluePlacementArea;
    private Rectangle redPlacementArea;
    
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
        
        // Placement zones based on the background image (leaving room for walls and UI)
        int wallThickness = 100;
        int topWallHeight = 170;
        int bottomUIHeight = 100;
        
        bluePlacementArea = new Rectangle(wallThickness, topWallHeight, (screenWidth / 2) - wallThickness, screenHeight - topWallHeight - bottomUIHeight);
        redPlacementArea = new Rectangle(screenWidth / 2, topWallHeight, (screenWidth / 2) - wallThickness, screenHeight - topWallHeight - bottomUIHeight);
        
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

        if (isRightClick)
        {
            if (selectedButton != null) { CancelPlacement(); }
            else if (selectedPlacedUnit != null)
            {
                if (selectedPlacedUnit.UnitTeam == Unit.Team.Blue) currentManaBlue += selectedPlacedUnit.Cost;
                else currentManaRed += selectedPlacedUnit.Cost;

                SceneManager.Remove(selectedPlacedUnit.collider);
                SceneManager.Remove(selectedPlacedUnit);
                placedUnits.Remove(selectedPlacedUnit);
                selectedPlacedUnit = null;
            }
        }

        if (isLeftClick)
        {
            Point mousePos = new Point(currentMouseState.X, currentMouseState.Y);
            bool clickedOnUI = false;

            bool isUnitSelected = selectedButton != null || selectedPlacedUnit != null;

            // Unit not selected - Check for button pressed -----------------------------------------------------------------------
            
            if (!isUnitSelected)
            {
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
            }
            if (!clickedOnUI && selectedButton == null) // If we didn't click a UI button and we aren't trying to buy a new unit
            {
                Unit clickedUnit = null;
                
                // Check if our mouse is hovering over an existing unit (within 30 pixels)
                foreach (var unit in placedUnits)
                {
                    if (Vector2.Distance(unit.tm.position, new Vector2(mousePos.X, mousePos.Y)) < 70f)
                    {
                        clickedUnit = unit;
                        break;
                    }
                }
                if (clickedUnit != null)
                {
                    // Deselect previous unit if we click a different one
                    if (selectedPlacedUnit != null)
                    {
                        selectedPlacedUnit.color = (selectedPlacedUnit.UnitTeam == Unit.Team.Blue) ? Color.CadetBlue : Color.IndianRed;
                    }
                    // Select the new unit and make it half transparent
                    selectedPlacedUnit = clickedUnit; 
                    selectedPlacedUnit.color *= 0.5f; 
                }
                else if (selectedPlacedUnit != null)
                {
                    // We clicked empty space while a unit is selected -> Move it!
                    if (selectedPlacedUnit.UnitTeam == Unit.Team.Blue && bluePlacementArea.Contains(mousePos) ||
                        selectedPlacedUnit.UnitTeam == Unit.Team.Red && redPlacementArea.Contains(mousePos))
                    {
                        selectedPlacedUnit.tm.position = new Vector2(mousePos.X, mousePos.Y);
                        selectedPlacedUnit.color = (selectedPlacedUnit.UnitTeam == Unit.Team.Blue) ? Color.CadetBlue : Color.IndianRed;
                        selectedPlacedUnit = null;
                    }
                }
            }
            else if (clickedOnUI && selectedPlacedUnit != null)
            {
                selectedPlacedUnit.color = (selectedPlacedUnit.UnitTeam == Unit.Team.Blue) ? Color.CadetBlue : Color.IndianRed;
                selectedPlacedUnit = null;
            }
            
            // If Unit selected - Place Unit on map -----------------------------------------------------------------------
            
            if (!clickedOnUI && selectedButton != null && selectedButton.team == Team.Blue) 
            {
                // Place unit if clicking inside the Blue player's placement area
                if (bluePlacementArea.Contains(mousePos))
                {
                    PlaceUnit(selectedButton.Name, new Vector2(mousePos.X, mousePos.Y), Team.Blue);
                    selectedButton = null; // Successfully placed, reset selection
                }
                else { CancelPlacement(); }
            }
            
            if (!clickedOnUI && selectedButton != null && selectedButton.team == Team.Red) 
            {
                // Place unit if clicking inside the Red player's placement area
                if (redPlacementArea.Contains(mousePos))
                {
                    PlaceUnit(selectedButton.Name, new Vector2(mousePos.X, mousePos.Y), Team.Red);
                    selectedButton = null; // Successfully placed, reset selection
                }
                else { CancelPlacement(); }
            }
            // --------------------------------------------------------------------------------------------------------------
        }
        previousMouseState = currentMouseState;
    }
    
    // =======================================================================================================================================================
    //              HELPER FUNCTIONS
    // =======================================================================================================================================================
    
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
            placedUnits.Add(newUnit); 
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (dummyTexture == null)
        {
            dummyTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            dummyTexture.SetData(new[] { Color.White });
        }
        
        bool isUnitSelected = selectedButton != null || selectedPlacedUnit != null;
        
        if (!isUnitSelected)
        {
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
        }
        
        // Draw a text preview on the mouse cursor and a highlighted placement zone
        if (isUnitSelected)
        {
            // Determine which team's unit is selected
            Team selectedTeam = Team.Blue;
            if (selectedButton != null) selectedTeam = selectedButton.team;
            else if (selectedPlacedUnit != null) selectedTeam = (selectedPlacedUnit.UnitTeam == Unit.Team.Blue) ? Team.Blue : Team.Red;

            // Draw highlighted placement zone
            Rectangle activeArea = (selectedTeam == Team.Blue) ? bluePlacementArea : redPlacementArea;
            Color zoneColor = (selectedTeam == Team.Blue) ? (Color.Blue * 0.2f) : (Color.Red * 0.2f);
            spriteBatch.Draw(dummyTexture, activeArea, zoneColor);

            if (selectedButton != null && font != null)
            {
                MouseState mouse = Mouse.GetState();
                spriteBatch.DrawString(font, $"Placing {selectedButton.Name}...", new Vector2(mouse.X + 15, mouse.Y + 15), Color.Yellow);
            }
        }
    }
}
