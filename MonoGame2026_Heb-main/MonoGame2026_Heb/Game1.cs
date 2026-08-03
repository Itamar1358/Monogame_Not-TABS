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

    Texture2D _logo;
    Texture2D _pongAtlas;
    
    public static Vector2 _screenCenter;
    
    private BattleManager battleManager = new();
    
    
    
    private FireProjectile fireProjectile;
    private HypnosisProjectile hypnosisProjectile;

    private SpriteFont _fontOswald;
    
    MousePositionText mousePositionText = new MousePositionText();

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

        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;

        _graphics.IsFullScreen = false;
        
        _screenCenter =  new Vector2(
            _graphics.PreferredBackBufferWidth * 0.5f,
            _graphics.PreferredBackBufferHeight * 0.5f);

    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        
        //AudioManager.AddSong("theme", "Audio/Music/theme");
        //AudioManager.AddSoundEffect("collect", "Audio/SFX/collect");
        //AudioManager.AddSoundEffect("bounce", "Audio/SFX/bounce");
        
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        SpriteManager.AddSprite("Pixel", "Images/pixel",1,1);
        SpriteManager.AddSprite("Knight","Images/Units/Knight", 1,1);
        SpriteManager.AddSprite("Ogre","Images/Units/Ogre", 1,1);
        SpriteManager.AddSprite("Wizard","Images/Units/Wizard", 1,1);
        SpriteManager.AddSprite("Hypnotist","Images/Units/Hypnotist", 1, 1);
        SpriteManager.AddSprite("Fireball", "Images/Fireball",  2, 2);
        SpriteManager.AddSprite("HypnosisBall", "Images/HypnosisBall", 2, 2);
 
        mousePositionText.font = Content.Load<SpriteFont>("Fonts/Oswald");
        
        Start();
    }

    void Start()
    {
        //AudioManager.PlaySong("theme");
        
        battleManager = SceneManager.Create<BattleManager>();
        
        Knight knight = SceneManager.Create<Knight>();
        Knight knight2 = SceneManager.Create<Knight>();
        Wizard wizard = SceneManager.Create<Wizard>();
        Wizard wizard2 = SceneManager.Create<Wizard>();
        Ogre ogre = SceneManager.Create<Ogre>();
        Ogre ogre2 = SceneManager.Create<Ogre>();
        Hypnotist hypnotist = SceneManager.Create<Hypnotist>();
        Hypnotist hypnotist2 = SceneManager.Create<Hypnotist>();
        
        fireProjectile = SceneManager.Create<FireProjectile>();
        
        
        
        
        
        
        knight.InitializeUnit(new Vector2(800, 800), Unit.Team.Blue);
        knight2.InitializeUnit(new Vector2(800, 600), Unit.Team.Blue);
        wizard.InitializeUnit(new Vector2(1100, 800), Unit.Team.Blue);
        hypnotist2.InitializeUnit(new Vector2(1100, 600), Unit.Team.Blue);
        
        ogre.InitializeUnit(new Vector2(300,200),Unit.Team.Red);
        ogre2.InitializeUnit(new Vector2(300,400),Unit.Team.Red);
        hypnotist.InitializeUnit(new Vector2(100, 200), Unit.Team.Red);
        wizard2.InitializeUnit(new Vector2(100, 400), Unit.Team.Red);
        
        battleManager.RegisterUnit(knight);
        battleManager.RegisterUnit(knight2);
        battleManager.RegisterUnit(wizard);
        battleManager.RegisterUnit(wizard2);
        battleManager.RegisterUnit(ogre);
        battleManager.RegisterUnit(ogre2);
        battleManager.RegisterUnit(hypnotist);
        battleManager.RegisterUnit(hypnotist2);
        
        
        battleManager.StartBattle();
        
        
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
        GraphicsDevice.Clear(Color.DarkRed);

        _spriteBatch.Begin();

        SceneManager.Instance.Draw(_spriteBatch);
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}