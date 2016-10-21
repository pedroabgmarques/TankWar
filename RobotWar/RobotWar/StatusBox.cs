using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace RobotWar
{
    public class StatusBox
    {

        Texture2D textura, textura_alliance, textura_coalition, textura_proximo;
        SpriteFont font, arial_12, tahoma_20, tahoma_20_bold;
        Team equipa;
        public bool activo;
        static public float largura = 295, altura = 155;
        float transparencia;
        float velocidade;
        int n_tanques, n_prontos;
        bool stay_visible;
        string message;
        bool showTeam;


        public void Initializing(Team equipa, int n_tanques, int n_prontos, string message, bool showTeam, bool stay_visible)
        {
            this.activo = true;
            this.transparencia = 1f;
            this.equipa = equipa;
            this.stay_visible = stay_visible;
            if (stay_visible)
            {
                velocidade = 0f;
            }
            else
            {
                velocidade = 0.001f;
            }
            this.n_tanques = n_tanques;
            this.n_prontos = n_prontos;
            this.showTeam = showTeam;
            this.message = message;
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

            if (this.equipa == Team.Alliance)
            {
                textura_proximo = textura_alliance;
            }
            else
            {
                textura_proximo = textura_coalition;
            }

            if (transparencia != 0f)
            {
                transparencia -= velocidade;
                if (!stay_visible)
                {
                    velocidade += 0.0001f;
                }

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

                spritebatch.Draw(textura, new Vector2(x, y), null,
                        Color.White * transparencia, 0f, Vector2.Zero, new Vector2(largura, altura), SpriteEffects.None, 0f);

                if (showTeam)
                {
                    spritebatch.Draw(textura_proximo, new Vector2(x + 10, y + 10), null,
                    Color.White * transparencia, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    spritebatch.DrawString(font, equipa.ToString(), new Vector2(x + 100, y + 13), Color.Black * transparencia);
                    spritebatch.DrawString(tahoma_20_bold, "Turno " + Game1.turnos, new Vector2(x + 100, y + 40), Color.Black * transparencia);
                }


                spritebatch.DrawString(tahoma_20, message, new Vector2(x + (showTeam ? 100 : 60), y + (showTeam ? 60 : (y / 4 - 3))), Color.Black * transparencia);
                

            }
        }


    }
}
