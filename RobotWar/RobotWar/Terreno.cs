using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RobotWar
{
    public class Terreno
    {
        public Vector2 posicao;
        public Texture2D textura;
        public Vector2 posicao_grelha;
        public int explosoes;
        static public float escala=0.5f;
        public int largura
        {
            get { return textura.Width; }
        }
        public int altura
        {
            get { return textura.Height; }
        }

        public void Initialiazing(Texture2D textura, Vector2 posicao, Vector2 posicao_grelha)
        {
            this.textura = textura;
            this.posicao = posicao;
            this.posicao_grelha = posicao_grelha;
            this.explosoes = 0;
        }

        public void Draw(SpriteBatch spritebatch)
        {
            spritebatch.Draw(textura, posicao, null, Color.White, 0f, Vector2.Zero, escala, SpriteEffects.None, 0f);
        }

        public void Draw(SpriteBatch spritebatch, Vector2 escala, Texture2D textura)
        {
            spritebatch.Draw(textura, posicao, null, Color.White*0.3f, 0f, Vector2.Zero, escala, SpriteEffects.None, 0f);
        }
    }
}
