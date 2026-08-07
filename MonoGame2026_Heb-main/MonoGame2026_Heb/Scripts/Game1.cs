using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media;
using MonoGame2026_Heb.Content;

namespace MonoGame2026_Heb;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public static Vector2 _screenCenter;
    public static int ScreenWidth;
    public static int ScreenHeight;
    public string CurrentBackground = "MainMenuBackground";
    public Song mainMenuTheme;
    
    public static float Gamma = 0.5f;
    
    public static Game1 Instance;
    
    private BattleManager battleManager;
    
    private Knight knight;
    private Ogre ogre;
    private Wizard wizard;
    private Hypnotist hypnotist;
    
    private FireProjectile fireProjectile;
    private HypnosisProjectile hypnosisProjectile;

    private SpriteFont _font;

    #region ResourcesManager
    
    private ResourcesManager<Texture2D> textureManager;
    private ResourcesManager<Song> songManager;
    private ResourcesManager<SoundEffect> soundEffectManager;

    #endregion
    
    
    private SpriteManager spriteManager = null;
    public Game1()
    {
        Instance = this;
        _graphics = new GraphicsDeviceManager(this);

        textureManager = new(Content);
        songManager = new(Content);
        soundEffectManager = new(Content);
        
        spriteManager = new SpriteManager();
        
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        ScreenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        ScreenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        
        _graphics.PreferredBackBufferWidth = ScreenWidth;
        _graphics.PreferredBackBufferHeight = ScreenHeight;

        _graphics.IsFullScreen = false;
        Window.IsBorderless = true;
        
        _screenCenter =  new Vector2(
            ScreenWidth * 0.5f,
            ScreenHeight * 0.5f);

    }

    protected override void Initialize()
    {
        _graphics.ApplyChanges();
        LoadSettings();
        base.Initialize();
    }
    
    public static void SaveSettings()
    {
        try 
        {
            string data = $"{AudioManager.MusicVolume},{AudioManager.SFXVolume},{Gamma}";
            System.IO.File.WriteAllText("settings.txt", data);
        } catch {}
    }

    public static void LoadSettings()
    {
        try 
        {
            if (System.IO.File.Exists("settings.txt")) 
            {
                string[] parts = System.IO.File.ReadAllText("settings.txt").Split(',');
                if (parts.Length == 3) 
                {
                    AudioManager.SetMusicVolume(float.Parse(parts[0]));
                    AudioManager.SetSFXVolume(float.Parse(parts[1]));
                    Gamma = float.Parse(parts[2]);
                }
            }
        } catch {}
    }

    protected override void LoadContent()
    {
        AudioManager.AddSong("GameplayMusic", "Audio/Music/GameplayMusic");
        
        AudioManager.AddSoundEffect("ButtonSFX", "Audio/SFX/ButtonSFX");
        AudioManager.AddSoundEffect("ClubHitSFX", "Audio/SFX/ClubHitSFX");
        AudioManager.AddSoundEffect("ConfusionSpellSFX", "Audio/SFX/ConfusionSpellSFX");
        AudioManager.AddSoundEffect("FireballSFX", "Audio/SFX/FireballSFX");
        AudioManager.AddSoundEffect("HitSFX", "Audio/SFX/HitSFX");
        AudioManager.AddSoundEffect("HypnotistDeath", "Audio/SFX/HypnotistDeath");
        AudioManager.AddSoundEffect("KnightDeath", "Audio/SFX/KnightDeath");
        AudioManager.AddSoundEffect("MagicianDeath", "Audio/SFX/MagicianDeath");
        AudioManager.AddSoundEffect("OgreDeath", "Audio/SFX/OgreDeath");
        AudioManager.AddSoundEffect("SpawnUnitSFX", "Audio/SFX/SpawnUnitSFX");
        AudioManager.AddSoundEffect("SwordHitSFX", "Audio/SFX/SwordHitSFX");
        AudioManager.AddSoundEffect("VictorySFX", "Audio/SFX/VictorySFX");
        AudioManager.AddSoundEffect("HypnotistPlacedSFX", "Audio/SFX/HypnotistPlacedSFX");
        AudioManager.AddSoundEffect("MagicianPlacedSFX", "Audio/SFX/MagicianPlacedSFX");
        AudioManager.AddSoundEffect("OgrePlacedSFX", "Audio/SFX/OgrePlacedSFX");
        AudioManager.AddSoundEffect("KnightPlacedSFX", "Audio/SFX/KnightPlacedSFX");
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        SpriteManager.AddSprite("Background", "Images/BackGrounds/BattleField", 1, 1);
        SpriteManager.AddSprite("MainMenuBackground", "Images/BackGrounds/MainMenuBackground", 1, 1);
        SpriteManager.AddSprite("CustomButton", "Images/button", 1, 1);
        SpriteManager.AddSprite("Pixel", "Images/pixel",1,1);
        
        SpriteManager.AddSprite("Knight","Images/Units/Knight", 1,1);
        SpriteManager.AddSprite("Knight_Hurt","Images/Units/Knight_Hurt", 1,1);
        SpriteManager.AddSprite("Knight_VeryHurt","Images/Units/Knight_VeryHurt", 1,1);
        
        SpriteManager.AddSprite("Ogre","Images/Units/Ogre", 1,1);
        SpriteManager.AddSprite("Ogre_Hurt","Images/Units/Ogre_Hurt", 1,1);
        SpriteManager.AddSprite("Ogre_VeryHurt","Images/Units/Ogre_VeryHurt", 1,1);
        
        SpriteManager.AddSprite("Wizard","Images/Units/Wizard", 1,1);
        SpriteManager.AddSprite("Wizard_Hurt","Images/Units/Wizard_Hurt", 1,1);
        SpriteManager.AddSprite("Wizard_VeryHurt","Images/Units/Wizard_VeryHurt", 1,1);
        
        SpriteManager.AddSprite("Hypnotist","Images/Units/Hypnotist", 1, 1);
        SpriteManager.AddSprite("Hypnotist_Hurt","Images/Units/Hypnotist_Hurt", 1,1);
        SpriteManager.AddSprite("Hypnotist_VeryHurt","Images/Units/Hypnotist_VeryHurt", 1,1);
        
        SpriteManager.AddSprite("Fireball", "Images/Fireball",  2, 2);
        SpriteManager.AddSprite("HypnosisBall", "Images/HypnosisBall", 2, 2);
        SpriteManager.AddSprite("Sword", "Images/Sword",  1, 1);
        SpriteManager.AddSprite("Club", "Images/Club",  1, 1);
        
        _font = Content.Load<SpriteFont>("Fonts/GameFont");
        
        AudioManager.AddSong("MainMenuSoundTrack", "Audio/Music/MainMenuSoundTrack");
        
        LoadMainMenu();
    }

    public void LoadMainMenu()
    {
        CurrentBackground = "MainMenuBackground";
        
        AudioManager.PlaySong("MainMenuSoundTrack");

        SceneManager.Clear();
        
        MainMenuManager menuManager = SceneManager.Create<MainMenuManager>();
        menuManager.font = _font;
        
        SceneManager.Instance.Start();
    }

    public void LoadGame()
    {
        CurrentBackground = "Background";
        AudioManager.PlaySong("GameplayMusic");
        SceneManager.Clear();
        
        battleManager = SceneManager.Create<BattleManager>();
        
        UIManager uiManager = SceneManager.Create<UIManager>();
        uiManager.font = _font;
        uiManager.battleManager = battleManager;
        
        SceneManager.Instance.Start();
    }

    public void LoadUnitManual()
    {
        CurrentBackground = "MainMenuBackground";
        SceneManager.Clear();
        
        UnitManualManager manualManager = SceneManager.Create<UnitManualManager>();
        manualManager.font = _font;
        
        SceneManager.Instance.Start();
    }

    public void LoadSettingsMenu()
    {
        CurrentBackground = "MainMenuBackground";
        SceneManager.Clear();
        
        SettingsManager settingsManager = SceneManager.Create<SettingsManager>();
        settingsManager.font = _font;
        
        SceneManager.Instance.Start();
    }

    bool ShouldExitApplication()
    {
        return GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
               Keyboard.GetState().IsKeyDown(Keys.Escape);
    }

    protected override void Update(GameTime gameTime)
    {
        if (ShouldExitApplication()) Exit();
        
        SceneManager.Instance.Update(gameTime);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        Spritesheet bgSprite = SpriteManager.GetSprite(CurrentBackground);
        if (bgSprite != null)
        {
            _spriteBatch.Draw(bgSprite.texture, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.White);
        }

        SceneManager.Instance.Draw(_spriteBatch);
        
        _spriteBatch.End();

        if (Gamma != 0.5f)
        {
            _spriteBatch.Begin();
            Spritesheet pixelSprite = SpriteManager.GetSprite("Pixel");
            if (pixelSprite != null)
            {
                if (Gamma < 0.5f)
                {
                    // Map Gamma 0.0 -> 0.5 to alpha 0.3 -> 0.0 (max 30% black)
                    float alpha = (0.5f - Gamma) * 2f * 0.3f;
                    _spriteBatch.Draw(pixelSprite.texture, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.Black * alpha);
                }
                else
                {
                    // Map Gamma 0.5 -> 1.0 to alpha 0.0 -> 0.6 (max 60% white)
                    float alpha = (Gamma - 0.5f) * 2f * 0.6f;
                    _spriteBatch.Draw(pixelSprite.texture, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.White * alpha);
                }
            }
            _spriteBatch.End();
        }

        base.Draw(gameTime);
    }
}