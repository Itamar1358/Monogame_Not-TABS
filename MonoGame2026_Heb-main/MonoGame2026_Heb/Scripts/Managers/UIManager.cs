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
    
    public BattleManager battleManager;
    private bool isBattlePhase = false;
    private Rectangle playButtonBounds;
    
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
        
        // Setup Play Button
        int playWidth = 200;
        int playHeight = 80;
        playButtonBounds = new Rectangle((screenWidth / 2) - (playWidth / 2), 20, playWidth, playHeight);
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
        Point mousePos = new Point(currentMouseState.X, currentMouseState.Y);

        if (isRightClick)
        {
            HandleRightClick();
        }
        if (isLeftClick)
        {
            HandleLeftClick(mousePos);
        }
        previousMouseState = currentMouseState;
    }
    
    // =======================================================================================================================================================
    //              INPUT HANDLING FUNCTIONS
    // =======================================================================================================================================================

    private void HandleRightClick()
    {
        if (isBattlePhase) return; // Disallow interaction in battle phase

        if (selectedButton != null) 
        { 
            CancelPlacement(); 
        }
        else if (selectedPlacedUnit != null)
        {
            // Refund the unit
            if (selectedPlacedUnit.UnitTeam == Unit.Team.Blue) currentManaBlue += selectedPlacedUnit.Cost;
            else currentManaRed += selectedPlacedUnit.Cost;

            SceneManager.Remove(selectedPlacedUnit.collider);
            SceneManager.Remove(selectedPlacedUnit);
            placedUnits.Remove(selectedPlacedUnit);
            selectedPlacedUnit = null;
        }
    }

    private void HandleLeftClick(Point mousePos)
    {
        // 0. Check Play Button
        if (!isBattlePhase && playButtonBounds.Contains(mousePos))
        {
            StartBattle();
            return;
        }

        if (isBattlePhase) return; // Disallow interaction in battle phase

        bool isUnitSelected = selectedButton != null || selectedPlacedUnit != null;
        bool clickedOnUI = false;

        // Try to interact with UI buttons if we aren't currently placing/moving a unit
        if (!isUnitSelected)
        {
            clickedOnUI = TryClickUIButtons(mousePos);
        }
        // Try to select or move an existing placed unit
        if (!clickedOnUI && selectedButton == null)
        {
            TrySelectOrMoveUnit(mousePos);
        }
        // Try to place a newly bought unit
        if (!clickedOnUI && selectedButton != null) 
        {
            TryPlaceNewUnit(mousePos);
        }
    }

    private bool TryClickUIButtons(Point mousePos)
    {
        foreach (var button in buttons)
        {
            if (button.Bounds.Contains(mousePos))
            {
                if (selectedButton == button)
                {
                    CancelPlacement();
                    return true;
                }
                if (selectedButton != null)
                {
                    CancelPlacement();
                }

                if (button.team == Team.Blue)
                {
                    if (currentManaBlue >= button.Cost)
                    {
                        currentManaBlue -= button.Cost;
                        selectedButton = button;
                        Console.WriteLine($"Blue Player Picked up {button.Name}");
                    }
                    else Console.WriteLine("Not enough mana!");
                }
                else if (button.team == Team.Red)
                {
                    if (currentManaRed >= button.Cost)
                    {
                        currentManaRed -= button.Cost;
                        selectedButton = button;
                        Console.WriteLine($"Red Player Picked up {button.Name}");
                    }
                    else Console.WriteLine("Not enough mana!");
                }
                return true;
            }
        }
        return false;
    }

    private void TrySelectOrMoveUnit(Point mousePos)
    {
        Unit clickedUnit = null;
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
            DeselectPlacedUnit(); 
            selectedPlacedUnit = clickedUnit; 
            selectedPlacedUnit.color *= 0.5f;
        }
        else if (selectedPlacedUnit != null)
        {
            // Move unit if clicking inside valid placement area
            if (selectedPlacedUnit.UnitTeam == Unit.Team.Blue && bluePlacementArea.Contains(mousePos) ||
                selectedPlacedUnit.UnitTeam == Unit.Team.Red && redPlacementArea.Contains(mousePos))
            {
                selectedPlacedUnit.tm.position = new Vector2(mousePos.X, mousePos.Y);
                DeselectPlacedUnit();
            }
        }
    }

    private void TryPlaceNewUnit(Point mousePos)
    {
        if (selectedButton.team == Team.Blue) 
        {
            if (bluePlacementArea.Contains(mousePos))
            {
                PlaceUnit(selectedButton.Name, new Vector2(mousePos.X, mousePos.Y), Team.Blue);
                selectedButton = null; 
            }
            else CancelPlacement();
        }
        else if (selectedButton.team == Team.Red) 
        {
            if (redPlacementArea.Contains(mousePos))
            {
                PlaceUnit(selectedButton.Name, new Vector2(mousePos.X, mousePos.Y), Team.Red);
                selectedButton = null; 
            }
            else CancelPlacement();
        }
    }

    private void DeselectPlacedUnit()
    {
        if (selectedPlacedUnit != null)
        {
            selectedPlacedUnit.color = (selectedPlacedUnit.UnitTeam == Unit.Team.Blue) ? Color.CadetBlue : Color.IndianRed;
            selectedPlacedUnit = null;
        }
    }
    
    private void StartBattle()
    {
        CancelPlacement();
        DeselectPlacedUnit();
        
        isBattlePhase = true;
        
        foreach (var unit in placedUnits)
        {
            battleManager.RegisterUnit(unit);
        }
        
        battleManager.StartBattle();
        Console.WriteLine("Battle Phase Started!");
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
        
        if (!isUnitSelected && !isBattlePhase)
        {
            if (font != null) // Draw current mana
            {
                spriteBatch.DrawString(font, $"Blue Player`s Mana: {currentManaBlue}", new Vector2(10, 10), Color.Cyan);
                
                string redManaText = $"Red Player`s Mana: {currentManaRed}";
                Vector2 redManaSize = font.MeasureString(redManaText);
                spriteBatch.DrawString(font, redManaText, new Vector2(Game1.ScreenWidth - redManaSize.X - 10, 10), Color.Cyan);
            }
            
            // Draw Play Button
            Color playColor = playButtonBounds.Contains(Mouse.GetState().X, Mouse.GetState().Y) ? Color.LimeGreen : Color.ForestGreen;
            Spritesheet playButtonSprite = SpriteManager.GetSprite("CustomButton");
            if (playButtonSprite != null) spriteBatch.Draw(playButtonSprite.texture, playButtonBounds, playColor);
            else spriteBatch.Draw(dummyTexture, playButtonBounds, playColor);
            
            if (font != null)
            {
                Vector2 textSize = font.MeasureString("PLAY");
                float scale = Math.Min((playButtonBounds.Width - 40) / textSize.X, (playButtonBounds.Height - 30) / textSize.Y);
                Vector2 textPos = new Vector2(
                    playButtonBounds.X + (playButtonBounds.Width - textSize.X * scale) / 2,
                    playButtonBounds.Y + (playButtonBounds.Height - textSize.Y * scale) / 2
                );
                spriteBatch.DrawString(font, "PLAY", textPos, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
            
            foreach (var button in buttons) // Draw UI Buttons
            {
                bool isHovered = button.Bounds.Contains(Mouse.GetState().X, Mouse.GetState().Y);
                Color buttonColor;

                if (button.team == Team.Blue)
                {
                    // Green if selected, LightGray if hovered & affordable, BaseColor if affordable, Dark Red if not affordable
                    if (selectedButton == button) buttonColor = Color.LimeGreen;
                    else if (currentManaBlue >= button.Cost) buttonColor = isHovered ? Color.LightGray : Color.White;
                    else buttonColor = Color.DarkRed;
                }
                else
                {
                    // Green if selected, LightGray if hovered & affordable, BaseColor if affordable, Dark Red if not affordable
                    if (selectedButton == button) buttonColor = Color.LimeGreen;
                    else if (currentManaRed >= button.Cost) buttonColor = isHovered ? Color.LightGray : Color.White;
                    else buttonColor = Color.DarkRed;
                }

                Spritesheet buttonSprite = SpriteManager.GetSprite("CustomButton");
                if (buttonSprite != null)
                {
                    spriteBatch.Draw(buttonSprite.texture, button.Bounds, buttonColor);
                }
                else
                {
                    spriteBatch.Draw(dummyTexture, button.Bounds, buttonColor);
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
        if (isUnitSelected) // Draw a text preview on the mouse cursor and a highlighted placement zone
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