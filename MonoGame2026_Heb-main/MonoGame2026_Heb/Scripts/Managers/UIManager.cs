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
    
    private int currentManaBlue = 1500;
    private int currentManaRed = 1500;
    private MouseState previousMouseState;
    public SpriteFont font;
    
    private List<UIButton> buttons = new List<UIButton>();
    private UIButton selectedButton = null;
    
    private List<Unit> placedUnits = new List<Unit>();
    private Unit selectedPlacedUnit = null;
    
    private Texture2D dummyTexture;
    
    private Rectangle bluePlacementArea;
    private Rectangle redPlacementArea;
    
    private BattleManager _battleManager;
    public BattleManager battleManager 
    { 
        get => _battleManager; 
        set 
        {
            _battleManager = value;
            _battleManager.OnVictory += HandleVictory;
        }
    }
    
    private bool isBattlePhase = false;
    private Rectangle playButtonBounds;
    private Rectangle manualButtonBounds;
    private bool isManualOpen = false;
    
    private bool isVictoryPhase = false;
    private string victoryMessage = "";
    private Rectangle restartButtonBounds;
    private Rectangle menuButtonBounds;
    
    private bool showPlacementError = false;
    private float placementErrorTimer = 0f;
    
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
        int buttonWidth = ((screenWidth / 2) - 60) / 5;
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
            Cost = Knight.BaseCost,
            team = Team.Blue
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(10 + (buttonWidth + 10) * 1, buttonY, buttonWidth, buttonHeight), 
            Name = "Ogre", 
            Cost = Ogre.BaseCost,
            team = Team.Blue
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(10 + (buttonWidth + 10) * 2, buttonY, buttonWidth, buttonHeight), 
            Name = "Wizard", 
            Cost = Wizard.BaseCost,
            team = Team.Blue
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(10 + (buttonWidth + 10) * 3, buttonY, buttonWidth, buttonHeight), 
            Name = "Hypnotist", 
            Cost = Hypnotist.BaseCost,
            team = Team.Blue
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(10 + (buttonWidth + 10) * 4, buttonY, buttonWidth, buttonHeight), 
            Name = "Healer", 
            Cost = Healer.BaseCost,
            team = Team.Blue
        });
        
        // Red Player Buttons
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(screenWidth - buttonWidth - 10 - (buttonWidth + 10) * 4, buttonY, buttonWidth, buttonHeight), 
            Name = "Knight", 
            Cost = Knight.BaseCost,
            team = Team.Red
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(screenWidth - buttonWidth - 10 - (buttonWidth + 10) * 3, buttonY, buttonWidth, buttonHeight), 
            Name = "Ogre", 
            Cost = Ogre.BaseCost,
            team = Team.Red
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(screenWidth - buttonWidth - 10 - (buttonWidth + 10) * 2, buttonY, buttonWidth, buttonHeight), 
            Name = "Wizard", 
            Cost = Wizard.BaseCost,
            team = Team.Red
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(screenWidth - buttonWidth - 10 - (buttonWidth + 10) * 1, buttonY, buttonWidth, buttonHeight), 
            Name = "Hypnotist", 
            Cost = Hypnotist.BaseCost,
            team = Team.Red
        });
        buttons.Add(new UIButton { 
            Bounds = new Rectangle(screenWidth - buttonWidth - 10, buttonY, buttonWidth, buttonHeight), 
            Name = "Healer", 
            Cost = Healer.BaseCost,
            team = Team.Red
        });
        
        // Setup Play Button and Manual Button
        int playWidth = 260;
        int playHeight = 100;
        playButtonBounds = new Rectangle((screenWidth / 2) - playWidth - 10, 20, playWidth, playHeight);
        manualButtonBounds = new Rectangle((screenWidth / 2) + 10, 20, playWidth, playHeight);
        
        // Setup Victory Screen Buttons
        int victoryButtonWidth = 800;
        int victoryButtonHeight = 150;
        int centerX = screenWidth / 2;
        int centerY = screenHeight / 2;
        
        restartButtonBounds = new Rectangle(centerX - victoryButtonWidth / 2, centerY, victoryButtonWidth, victoryButtonHeight);
        menuButtonBounds = new Rectangle(centerX - victoryButtonWidth / 2, centerY + victoryButtonHeight + 30, victoryButtonWidth, victoryButtonHeight);
    }
    
    public void Start() { previousMouseState = Mouse.GetState(); }
    
    public void Update(GameTime gameTime) // Handles mouse input for selecting units, placing them on the field, and spending mana
    {
        if (isManualOpen)
        {
            previousMouseState = Mouse.GetState();
            return;
        }
        if (showPlacementError)
        {
            placementErrorTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (placementErrorTimer <= 0) { showPlacementError = false; }
        }

        MouseState currentMouseState = Mouse.GetState();
        bool isLeftClick = currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
        bool isRightClick = currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released;
        Point mousePos = new Point(currentMouseState.X, currentMouseState.Y);

        if (isRightClick) { HandleRightClick(); }
        if (isLeftClick) { HandleLeftClick(mousePos); }
        previousMouseState = currentMouseState;
    }

    private void HandleRightClick() // Cancels unit placement or refunds and deletes a placed unit
    {
        if (isVictoryPhase) return;
        if (isBattlePhase) return;
        if (selectedButton != null) { CancelPlacement(); }
        else if (selectedPlacedUnit != null)
        {
            if (selectedPlacedUnit.UnitTeam == Unit.Team.Blue) currentManaBlue += selectedPlacedUnit.Cost;
            else currentManaRed += selectedPlacedUnit.Cost;

            selectedPlacedUnit.Cleanup();
            SceneManager.Remove(selectedPlacedUnit.collider);
            SceneManager.Remove(selectedPlacedUnit);
            placedUnits.Remove(selectedPlacedUnit);
            selectedPlacedUnit = null;
        }
    }

    private void HandleLeftClick(Point mousePos) // Routes left clicks to victory screen actions, manual/play buttons, or unit placement logic
    {
        if (isVictoryPhase)
        {
            if (restartButtonBounds.Contains(mousePos))
            {
                AudioManager.PlaySFX?.Invoke("ButtonSFX");
                Game1.Instance.LoadGame();
            }
            else if (menuButtonBounds.Contains(mousePos))
            {
                AudioManager.PlaySFX?.Invoke("ButtonSFX");
                Game1.Instance.LoadMainMenu();
            }
            return;
        }
        if (!isBattlePhase && manualButtonBounds.Contains(mousePos))
        {
            AudioManager.PlaySFX?.Invoke("ButtonSFX");
            isManualOpen = true;
            UnitManualManager manual = SceneManager.Create<UnitManualManager>();
            manual.font = this.font;
            manual.IsPopup = true;
            manual.OnBack = () => { SceneManager.Remove(manual); isManualOpen = false; };
            return;
        }
        if (!isBattlePhase && playButtonBounds.Contains(mousePos))
        {
            AudioManager.PlaySFX?.Invoke("ButtonSFX");
            bool hasBlue = false;
            bool hasRed = false;
            foreach (var unit in placedUnits)
            {
                if (unit.UnitTeam == Unit.Team.Blue) hasBlue = true;
                if (unit.UnitTeam == Unit.Team.Red) hasRed = true;
            }
            if (!hasBlue || !hasRed)
            {
                showPlacementError = true;
                placementErrorTimer = 3.0f;
                return;
            }
            StartBattle();
            return;
        }
        if (isBattlePhase) return;

        bool isUnitSelected = selectedButton != null || selectedPlacedUnit != null;
        bool clickedOnUI = false;

        if (!isUnitSelected) { clickedOnUI = TryClickUIButtons(mousePos); }
        if (!clickedOnUI && selectedButton == null) { TrySelectOrMoveUnit(mousePos); }
        if (!clickedOnUI && selectedButton != null) { TryPlaceNewUnit(mousePos); }
    }

    private bool TryClickUIButtons(Point mousePos) // Evaluates if a unit purchase button was clicked and deducts mana if affordable
    {
        foreach (var button in buttons)
        {
            if (button.Bounds.Contains(mousePos))
            {
                if (selectedButton == button)
                {
                    AudioManager.PlaySFX?.Invoke("ButtonSFX");
                    CancelPlacement();
                    return true;
                }
                if (selectedButton != null) { CancelPlacement(); }
                if (button.team == Team.Blue)
                {
                    if (currentManaBlue >= button.Cost)
                    {
                        currentManaBlue -= button.Cost;
                        selectedButton = button;
                        AudioManager.PlaySFX?.Invoke("SpawnUnitSFX");
                        Console.WriteLine($"Blue Player Picked up {button.Name}");
                    }
                    else 
                    {
                        AudioManager.PlaySFX?.Invoke("ButtonSFX");
                        Console.WriteLine("Not enough mana!");
                    }
                }
                else if (button.team == Team.Red)
                {
                    if (currentManaRed >= button.Cost)
                    {
                        currentManaRed -= button.Cost;
                        selectedButton = button;
                        AudioManager.PlaySFX?.Invoke("SpawnUnitSFX");
                        Console.WriteLine($"Red Player Picked up {button.Name}");
                    }
                    else 
                    {
                        AudioManager.PlaySFX?.Invoke("ButtonSFX");
                        Console.WriteLine("Not enough mana!");
                    }
                }
                return true;
            }
        }
        return false;
    }

    private void TrySelectOrMoveUnit(Point mousePos) // Selects a previously placed unit or repositions the currently selected unit on the board
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

    private void TryPlaceNewUnit(Point mousePos) // Places a newly purchased unit within valid team bounds
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

    private void DeselectPlacedUnit() // Resets the color and selection state of the currently selected placed unit
    {
        if (selectedPlacedUnit != null)
        {
            selectedPlacedUnit.color = (selectedPlacedUnit.UnitTeam == Unit.Team.Blue) ? Color.CadetBlue : Color.IndianRed;
            selectedPlacedUnit = null;
        }
    }
    
    private void StartBattle() // Locks in unit placements, registers all units with the BattleManager, and starts the combat phase
    {
        CancelPlacement();
        DeselectPlacedUnit();
        
        isBattlePhase = true;
        
        foreach (var unit in placedUnits) { battleManager.RegisterUnit(unit); }
        
        battleManager.StartBattle();
        Console.WriteLine("Battle Phase Started!");
    }
    
    private void HandleVictory(Unit.Team winningTeam) // Listens to the BattleManager's victory event and triggers the victory UI screen
    {
        isVictoryPhase = true;
        victoryMessage = $"{winningTeam} Team Wins!";
        AudioManager.PlaySFX?.Invoke("VictorySFX");
    }

    private void CancelPlacement() //Cancels the current unit selection from the spawn menu and refunds the mana
    {
        if (selectedButton != null)
        {
            if (selectedButton.team == Team.Blue) { currentManaBlue += selectedButton.Cost; }
            else  { currentManaRed += selectedButton.Cost; }
            selectedButton = null;
        }
    }
    
    private void PlaceUnit(string name, Vector2 position, Team team) //Instantiates the selected unit class, places it at the target position, and triggers spawn SFX
    {
        Unit newUnit = null;

        if (name == "Knight") newUnit = SceneManager.Create<Knight>();
        else if (name == "Ogre") newUnit = SceneManager.Create<Ogre>();
        else if (name == "Wizard") newUnit = SceneManager.Create<Wizard>();
        else if (name == "Hypnotist") newUnit = SceneManager.Create<Hypnotist>();
        else if (name == "Healer") newUnit = SceneManager.Create<Healer>();
        
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
            string soundName = name == "Wizard" ? "MagicianPlacedSFX" : name + "PlacedSFX";
            AudioManager.PlaySFX?.Invoke(soundName);
        }
    }

    public void Draw(SpriteBatch spriteBatch) // Renders the mana texts, unit buttons, play/manual buttons, placement preview zones, and victory screens
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
            Color playColor = (!isManualOpen && playButtonBounds.Contains(Mouse.GetState().X, Mouse.GetState().Y)) ? Color.LimeGreen : Color.ForestGreen;
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
            
            // Draw Manual Button
            Color manualColor = (!isManualOpen && manualButtonBounds.Contains(Mouse.GetState().X, Mouse.GetState().Y)) ? Color.LimeGreen : Color.ForestGreen;
            if (playButtonSprite != null) spriteBatch.Draw(playButtonSprite.texture, manualButtonBounds, manualColor);
            else spriteBatch.Draw(dummyTexture, manualButtonBounds, manualColor);
            
            if (font != null)
            {
                Vector2 textSize = font.MeasureString("MANUAL");
                float scale = Math.Min((manualButtonBounds.Width - 40) / textSize.X, (manualButtonBounds.Height - 30) / textSize.Y);
                Vector2 textPos = new Vector2(
                    manualButtonBounds.X + (manualButtonBounds.Width - textSize.X * scale) / 2,
                    manualButtonBounds.Y + (manualButtonBounds.Height - textSize.Y * scale) / 2
                );
                spriteBatch.DrawString(font, "MANUAL", textPos, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
            
            if (showPlacementError && font != null)
            {
                string errorMsg = "Both teams must have at least one unit to start!";
                Vector2 size = font.MeasureString(errorMsg);
                Vector2 pos = new Vector2((Game1.ScreenWidth - size.X) / 2, playButtonBounds.Bottom + 10);
                spriteBatch.DrawString(font, errorMsg, pos, Color.Red);
            }
            
            foreach (var button in buttons) // Draw UI Buttons
            {
                bool isHovered = !isManualOpen && button.Bounds.Contains(Mouse.GetState().X, Mouse.GetState().Y);
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
                if (buttonSprite != null) { spriteBatch.Draw(buttonSprite.texture, button.Bounds, buttonColor); }
                else { spriteBatch.Draw(dummyTexture, button.Bounds, buttonColor); }

                if (font != null)
                {
                    string text = $"{button.Name}\n({button.Cost})";
                    Vector2 textSize = font.MeasureString(text);
                    
                    // Calculate scale to ensure text fits inside the button bounds
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

        if (isVictoryPhase)
        {
            spriteBatch.Draw(dummyTexture, new Rectangle(0, 0, Game1.ScreenWidth, Game1.ScreenHeight), Color.Black * 0.5f);
            if (font != null)
            {
                Vector2 titleSize = font.MeasureString(victoryMessage);
                Vector2 titlePos = new Vector2((Game1.ScreenWidth - titleSize.X * 2f) / 2, Game1.ScreenHeight * 0.3f);
                spriteBatch.DrawString(font, victoryMessage, titlePos, Color.Gold, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
            }
            
            // Draw Restart Button
            Spritesheet buttonSprite = SpriteManager.GetSprite("CustomButton");
            MouseState mouseState = Mouse.GetState();
            
            Color restartColor = restartButtonBounds.Contains(mouseState.X, mouseState.Y) ? Color.LightGray : Color.White;
            if (buttonSprite != null) spriteBatch.Draw(buttonSprite.texture, restartButtonBounds, restartColor);
            else spriteBatch.Draw(dummyTexture, restartButtonBounds, restartColor);
            
            if (font != null)
            {
                Vector2 textSize = font.MeasureString("Restart Game");
                float scale = Math.Min((restartButtonBounds.Width - 300) / textSize.X, (restartButtonBounds.Height - 60) / textSize.Y);
                Vector2 textPos = new Vector2(
                    restartButtonBounds.X + (restartButtonBounds.Width - textSize.X * scale) / 2,
                    restartButtonBounds.Y + (restartButtonBounds.Height - textSize.Y * scale) / 2
                );
                spriteBatch.DrawString(font, "Restart Game", textPos, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
            
            // Draw Menu Button
            Color menuColor = menuButtonBounds.Contains(mouseState.X, mouseState.Y) ? Color.LightGray : Color.White;
            if (buttonSprite != null) spriteBatch.Draw(buttonSprite.texture, menuButtonBounds, menuColor);
            else spriteBatch.Draw(dummyTexture, menuButtonBounds, menuColor);
            if (font != null)
            {
                Vector2 textSize = font.MeasureString("Main Menu");
                float scale = Math.Min((menuButtonBounds.Width - 300) / textSize.X, (menuButtonBounds.Height - 60) / textSize.Y);
                Vector2 textPos = new Vector2(
                    menuButtonBounds.X + (menuButtonBounds.Width - textSize.X * scale) / 2,
                    menuButtonBounds.Y + (menuButtonBounds.Height - textSize.Y * scale) / 2
                );
                spriteBatch.DrawString(font, "Main Menu", textPos, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
        }
    }
}