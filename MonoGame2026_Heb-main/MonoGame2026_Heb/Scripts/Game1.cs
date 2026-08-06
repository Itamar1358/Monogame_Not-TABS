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
        base.Initialize();
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
        
        mainMenuTheme = Content.Load<Song>("Audio/Music/MainMenuSoundTrack");
        
        LoadMainMenu();
    }

    public void LoadMainMenu()
    {
        CurrentBackground = "MainMenuBackground";
        
        if (MediaPlayer.State != MediaState.Playing)
        {
            MediaPlayer.Play(mainMenuTheme);
            MediaPlayer.IsRepeating = true;
        }

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

        base.Draw(gameTime);
    }
}