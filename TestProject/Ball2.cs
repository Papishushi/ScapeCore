using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ScapeCore.Core.Batching;
using ScapeCore.Core.Engine;
using ScapeCore.Core.Engine.Components;

[ResourceLoad("ball")]
public class Ball2 : MonoBehaviour
{
    public SpriteRenderer sRenderer;

    protected override void Start()
    {
        sRenderer = gameObject.AddBehaviour<SpriteRenderer>();
        sRenderer.texture = (Texture2D)ResourceManager.Content["ball"];
        var size = new Point(sRenderer.texture.Width, sRenderer.texture.Height);
        var center = Point.Zero;
        sRenderer.rtransform = new RectTransform(new(center, size), Vector2.Zero, Vector2.One);
        transform.position = new Vector2(game.Graphics.PreferredBackBufferWidth / 2,
                                         game.Graphics.PreferredBackBufferHeight / 2);
    }

    protected override void Update()
    {
        float deltaTime;
        double seconds, milliseconds;
        seconds = Time.ElapsedGameTime.TotalSeconds;
        milliseconds = Time.ElapsedGameTime.TotalMilliseconds;
        deltaTime = (float)seconds + ((float)milliseconds / 1000);
        transform.position.X += 100f * deltaTime;
    }

}

