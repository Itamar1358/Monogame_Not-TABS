using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        
        //AudioManager.AddSong("theme", "Audio/Music/theme");
        //AudioManager.AddSoundEffect("collect", "Audio/SFX/collect");
        //AudioManager.AddSoundEffect("bounce", "Audio/SFX/bounce");
        
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        SpriteManager.AddSprite("Background", "Images/BackGrounds/BattleField", 1, 1);
        SpriteManager.AddSprite("CustomButton", "Images/button", 1, 1);
        SpriteManager.AddSprite("Pixel", "Images/pixel",1,1);
        SpriteManager.AddSprite("Knight","Images/Units/Knight", 1,1);
        SpriteManager.AddSprite("Ogre","Images/Units/Ogre", 1,1);
        SpriteManager.AddSprite("Wizard","Images/Units/Wizard", 1,1);
        SpriteManager.AddSprite("Hypnotist","Images/Units/Hypnotist", 1, 1);
        SpriteManager.AddSprite("Fireball", "Images/Fireball",  2, 2);
        SpriteManager.AddSprite("HypnosisBall", "Images/HypnosisBall", 2, 2);
        
        _font = Content.Load<SpriteFont>("Fonts/GameFont");
        
        
        Start();
    }

    void Start()
    {
        //AudioManager.PlaySong("theme");
        
        battleManager = SceneManager.Create<BattleManager>();
        
        UIManager uiManager = SceneManager.Create<UIManager>();
        uiManager.font = _font;
        uiManager.battleManager = battleManager;
        
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

        Spritesheet bgSprite = SpriteManager.GetSprite("Background");
        if (bgSprite != null)
        {
            _spriteBatch.Draw(bgSprite.texture, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.White);
        }

        SceneManager.Instance.Draw(_spriteBatch);
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}