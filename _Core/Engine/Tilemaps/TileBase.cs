using Microsoft.Xna.Framework.Graphics;

namespace ScapeCore.Core.Engine.Tilemaps
{
    public abstract class TileBase : MonoBehaviour
    {
        public TileRenderer tRenderer;
        public Texture2D Texture { get => tRenderer.texture; }

        protected override void Start()
        {
            tRenderer = new TileRenderer();
            gameObject.AddBehaviour(tRenderer);
        }

        protected override void Update()
        {

        }
    }
}