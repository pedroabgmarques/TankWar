using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace RobotWar
{
    public class Turnbox
    {

        Texture2D textura, textura_alliance, textura_coalition, textura_proximo;
        SpriteFont font, arial_12, tahoma_20, tahoma_20_bold;
        equipa equipa;
        public bool activo;
        static public float largura = 295, altura = 155;
        float transparencia;
        float velocidade;
        int n_tanques,n_prontos;
        bool game_over;

        public void Initializing(equipa equipa, int n_tanques, int n_prontos, bool game_over)
        {
            this.activo = true;
            this.transparencia = 1f;
            this.equipa = equipa;
            this.game_over = game_over;
            if (game_over)
            {
                velocidade = 0f;
            }
            else
            {
                velocidade = 0.001f;
            }
            this.n_tanques = n_tanques;
            this.n_prontos = n_prontos;
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphic)
        {
            font = content.Load<SpriteFont>("fontes/font_tahoma_25_bold");
            arial_12 = content.Load<SpriteFont>("fontes/arial_12");
            tahoma_20 = content.Load<SpriteFont>("fontes/font_tahoma_20");
            tahoma_20_bold = content.Load<SpriteFont>("fontes/font_tahoma_20_bold");
            textura = new Texture2D(graphic, 1, 1);
            textura.SetData(new[] { Color.White });
            textura_alliance = content.Load<Texture2D>("Texturas/sprites/tanques/tank");
            textura_coalition = content.Load<Texture2D>("Texturas/sprites/tanques/tank2");

        }

        public void UnloadContent()
        {
        }

        public void Update()
        {

            if (this.equipa == equipa.Alliance)
            {
                textura_proximo = textura_alliance;
            }
            else
            {
                textura_proximo = textura_coalition;
            }

            if (transparencia!=0f)
            {
                transparencia -= velocidade;
                velocidade += 0.0001f;
            }
            else
            {
                this.activo = false;
            }
        }

        public void Draw(SpriteBatch spritebatch, GraphicsDevice graphics)
        {
            if (activo)
            {
                float x = graphics.Viewport.Width / 2 - largura / 2;
                float y = graphics.Viewport.Height / 2 - altura / 2;
                spritebatch.Draw(textura, new Vector2(x,y), null,
                        Color.White * transparencia, 0f, Vector2.Zero, new Vector2(largura, altura), SpriteEffects.None, 0f);

                spritebatch.Draw(textura_proximo, new Vector2(x+10, y+10), null,
                     Color.White * transparencia, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                spritebatch.DrawString(font, equipa.ToString(), new Vector2(x + 100, y + 13), Color.Black*transparencia);
                spritebatch.DrawString(tahoma_20_bold, "Turno "+Game1.turnos, new Vector2(x + 100, y + 40), Color.Black * transparencia);
                if (game_over)
                {
                    spritebatch.DrawString(tahoma_20, "Tanques: " + n_tanques, new Vector2(x + 100, y + 65), Color.Black * transparencia);
                    spritebatch.DrawString(font, "WINNER!", new Vector2(x + 100, y + 120), Color.Black * transparencia);
                }
                else
                {
                    spritebatch.DrawString(tahoma_20, "Tanques: " + n_tanques, new Vector2(x + 100, y + 96), Color.Black * transparencia);
                    spritebatch.DrawString(tahoma_20, "Prontos a disparar: " + n_prontos, new Vector2(x + 100, y + 120), Color.Black * transparencia);
                }
            }
        }


    }
}
