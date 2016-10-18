using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace RobotWar
{

    public enum direccao
    {
        Cima,
        Baixo,
        Esquerda,
        Direita,
        CimaDireita,
        CimaEsquerda,
        BaixoDireita,
        BaixoEsquerda
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D textura_tanque2, textura_terreno_seleccionado,
            mira, textura_terreno_mover, textura_cratera_1, textura_cratera_2, textura_cratera_3, 
            textura_cratera_4, textura_cratera_5, seta_rato_mover, seta_rato_esperar, explosao, seta_rato_nepias;
        static Texture2D textura_terreno, textura_tanque1;
        MouseState rato;
        MouseState rato_anterior;
        KeyboardState teclado_anterior;
        Vector2 posicao_rato;
        InfoBox infoBox;
        static Random gerador_numeros = new Random();
        public static Texture2D apontador, seta_rato;
        public static Tank tanque_seleccionado;
        Animacao explode, fogacho;
        List<Animacao> lista_animacoes;
        static DamageBox damageBox;
        static Turnbox turnBox;

        //listas e matriz do terreno
        static List<Tank> lista_tanques;
        List<Terreno> terreno_pode_mover;
        static List<PowerUp> lista_powerups;
        static Terreno[,] grelha;
        Terreno terreno_seleccionado;

        //n� de tanques de cada equipa
        int n_tanques = 20;

        //contador de jogadas
        static int jogadas;
        public static int turnos;

        //equipa activa
        static Team equipa_activa;

        //Network:
        TcpClient client;
        NetworkStream clientStream;
        Thread waitForServerMessages;
        Thread processMessage;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1366;
            graphics.PreferredBackBufferHeight = 768;
            graphics.PreferMultiSampling = true;
            graphics.IsFullScreen = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            jogadas = 0;
            turnos = 1;

            lista_tanques = new List<Tank>();
            terreno_pode_mover = new List<Terreno>();
            lista_powerups = new List<PowerUp>();

            lista_animacoes = new List<Animacao>();

            infoBox = new InfoBox();

            damageBox = new DamageBox();

            turnBox = new Turnbox();

            infoBox.Initializing(lista_tanques);

            //decidir a equipa que come�a primeiro
            int n_aleatorio = gerador_numeros.Next(2);
            if (n_aleatorio == 0)
            {
                equipa_activa = Team.Alliance;
            }
            else { equipa_activa = Team.Coalition; }

            //Network
            ConnectToServer("localhost", 7777);

            base.Initialize();
        }

        #region Network
        private void ConnectToServer(string servidor, int porto)
        {
            client = new TcpClient();
            client.Connect(servidor, porto);
            clientStream = client.GetStream();

            //thread que fica a aguardar comunica��es iniciadas pelo servidor
            waitForServerMessages = new Thread(new ParameterizedThreadStart(ReceiveServerMessage));
            waitForServerMessages.IsBackground = true;
            waitForServerMessages.Start(clientStream);
        }

        private void ReceiveServerMessage(object clientStream)
        {

            while (true)
            {
                NetworkStream stream = (NetworkStream)clientStream;
                byte[] message = new byte[4096];
                int bytesRead;

                while (true)
                {
                    bytesRead = 0;

                    try
                    {
                        //blocks until a client sends a message
                        bytesRead = stream.Read(message, 0, 4096);
                    }
                    catch
                    {
                        //a socket error has occured
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        //the client has disconnected from the server
                        break;
                    }

                    //message has successfully been received
                    ASCIIEncoding encoder = new ASCIIEncoding();

                    string mensagem = encoder.GetString(message, 0, bytesRead);

                    //cria nova thread para processar o conteudo da mensagem
                    processMessage = new Thread(new ParameterizedThreadStart(ProcessServerMessage));
                    processMessage.IsBackground = true;
                    processMessage.Start(mensagem);

                }
            }
        }

        private void ProcessServerMessage(object mens)
        {
            string message = (string)mens;
            Console.WriteLine("Mensagem do servidor: "+ message);
        }

        private void SendMessageToServer(string message)
        {
            try
            {
                ASCIIEncoding encoder = new ASCIIEncoding();
                byte[] buffer = encoder.GetBytes(message);

                clientStream.Write(buffer, 0, buffer.Length);
                clientStream.Flush();
            }
            catch
            {
                Console.WriteLine("Erro no envio de mensagem para o servidor!");
            }
        }

        #endregion

        #region Gerar tanques1
        private void gerarTanques1()
        {
            Vector2 posicao_tanque1;
            for (int i = 0; i < n_tanques; i=i+2)
            {
                Tank tank = new Tank();

                
                #region DEGUB - tanques1 a aparecerem em locais aleatorios
                
                /*
                int x_aleatorio, y_aleatorio;
                x_aleatorio = gerador_numeros.Next(10);
                y_aleatorio = gerador_numeros.Next(20);
                posicao_tanque1 = new Vector2((grelha[x_aleatorio, y_aleatorio].posicao.X) + (textura_terreno.Width * Terreno.escala / 2) - textura_tanque1.Width * Tank.escala / 2, grelha[x_aleatorio, y_aleatorio].posicao.Y);
                tank.posicao_grelha = new Vector2((float)x_aleatorio, (float)y_aleatorio);
                */
                
                #endregion
                
                
                posicao_tanque1 = new Vector2((grelha[0, i].posicao.X)+(textura_terreno.Width*Terreno.escala/2)-textura_tanque1.Width*Tank.escala/2, grelha[0, i].posicao.Y);
                tank.posicao_grelha = new Vector2(0f, i);
                
                
                tank.Initialiazing(textura_tanque1, posicao_tanque1, SpriteEffects.FlipVertically, Team.Alliance,gerador_numeros,i);
                lista_tanques.Add(tank);
            }
        }
        #endregion

        #region Gerar tanques2
        private void gerarTanques2()
        {
            Vector2 posicao_tanque2;
            for (int i = 1; i < n_tanques; i=i+2)
            {
                Tank tank = new Tank();

                #region DEGUB - tanques2 a aparecerem em locais aleatorios
                
                /*
                int x_aleatorio, y_aleatorio;
                x_aleatorio = gerador_numeros.Next(10);
                y_aleatorio = gerador_numeros.Next(20);
                posicao_tanque2 = new Vector2((grelha[x_aleatorio, y_aleatorio].posicao.X) + (textura_terreno.Width * Terreno.escala / 2) - textura_tanque1.Width * Tank.escala / 2, grelha[x_aleatorio, y_aleatorio].posicao.Y);
                tank.posicao_grelha = new Vector2((float)x_aleatorio, (float)y_aleatorio);
                */
                 
                #endregion

                
                posicao_tanque2 = new Vector2((grelha[0, i].posicao.X) + (textura_terreno.Width * Terreno.escala / 2) - textura_tanque1.Width * Tank.escala / 2, grelha[10, i].posicao.Y);
                tank.posicao_grelha = new Vector2(10f, i);
                

                tank.Initialiazing(textura_tanque2, posicao_tanque2,SpriteEffects.None, Team.Coalition,gerador_numeros,i);
                lista_tanques.Add(tank);
            }
        }
        #endregion

        #region Gerar lista inicial de Powerups
        private void gerarPowerUps()
        {
            for (int i = 0; i < 5; i++)
            {
                PowerUp powerup = gerarPowerupIndividual(Content);
                lista_powerups.Add(powerup);
            }
        }
        #endregion

        #region Gerar posicao da grelha de um powerup individual
        static private Vector2 gerarPosicaoGrelhaPowerup()
        {
            Vector2 posicao_grelha = new Vector2();
            bool posicao_ocupada;
            do
            {
                posicao_ocupada = false;
                if (turnos <= 20)
                {
                    posicao_grelha.X = gerador_numeros.Next(5) + 3;
                }
                else
                {
                    posicao_grelha.X = gerador_numeros.Next(11);
                }
                posicao_grelha.Y = gerador_numeros.Next(20);
                //verificar se existem robos nesta posicao
                foreach (Tank tanque in lista_tanques)
                {
                    if (tanque.posicao_grelha == posicao_grelha)
                    {
                        posicao_ocupada = true;
                        break;
                    }
                }
                foreach (PowerUp power in lista_powerups)
                {
                    if (power.posicao_grelha == posicao_grelha)
                    {
                        posicao_ocupada = true;
                        break;
                    }
                }
            }
            while (posicao_ocupada);
            return posicao_grelha;
        }
        #endregion

        #region Gerar grelha
        private void gerarGrelha()
        {
            grelha = new Terreno[11,n_tanques];
            Vector2 posicao_terreno, posicao_grelha;
            int offset_x = 22;
            int offset_y = 22;
            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < n_tanques; j++)
                {
                    Terreno terreno = new Terreno();
                    posicao_terreno = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X+offset_x, GraphicsDevice.Viewport.TitleSafeArea.Y+offset_y);
                    posicao_grelha = new Vector2((float)i,(float)j);
                    terreno.Initialiazing(textura_terreno, posicao_terreno, posicao_grelha);
                    offset_x += 66;
                    grelha[i,j] = terreno;
                }
                offset_y += 66;
                offset_x = 22;
            }
        }
        #endregion

        #region Desenhar grelha
        public void desenharGrelha()
        {
            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    Terreno terreno = grelha[i, j];
                    if (terreno.explosoes == 0)
                    {
                        terreno.textura = textura_terreno;
                    }
                    if (terreno.explosoes == 1)
                    {
                        terreno.textura = textura_cratera_4;
                    }
                    if (terreno.explosoes == 2)
                    {
                        terreno.textura = textura_cratera_5;
                    }
                    if (terreno.explosoes == 3)
                    {
                        terreno.textura = textura_cratera_3;
                    }
                    if (terreno.explosoes == 4)
                    {
                        terreno.textura = textura_cratera_2;
                    }
                    if (terreno.explosoes >= 5)
                    {
                        terreno.textura = textura_cratera_1;
                    }
                    grelha[i,j].Draw(spriteBatch);
                }
            }
        }
        #endregion

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
       
            textura_tanque1 = Content.Load<Texture2D>("texturas/sprites/tanques/tank");
            textura_tanque2 = Content.Load<Texture2D>("texturas/sprites/tanques/tank2");
            textura_terreno = Content.Load<Texture2D>("texturas/sprites/terrenos/textura_relva");
            textura_cratera_1 = Content.Load<Texture2D>("texturas/sprites/terrenos/textura_relva_cratera");
            textura_cratera_2 = Content.Load<Texture2D>("texturas/sprites/terrenos/textura_relva_cratera2");
            textura_cratera_3 = Content.Load<Texture2D>("texturas/sprites/terrenos/textura_relva_cratera3");
            textura_cratera_4 = Content.Load<Texture2D>("texturas/sprites/terrenos/textura_relva_cratera4");
            textura_cratera_5 = Content.Load<Texture2D>("texturas/sprites/terrenos/textura_relva_cratera5");
            seta_rato = Content.Load<Texture2D>("texturas/componentes/seta_rato");
            seta_rato_nepias = Content.Load<Texture2D>("texturas/componentes/cursor_nepias");
            seta_rato_mover = Content.Load<Texture2D>("texturas/componentes/seta_rato_mover");
            seta_rato_esperar = Content.Load<Texture2D>("texturas/componentes/seta_rato_esperar");
            mira = Content.Load<Texture2D>("texturas/componentes/crosshairs");
            explosao = Content.Load<Texture2D>("animacoes/explosion");

            //textura sobresposta dos terrenos para os quais se pode mover
            textura_terreno_mover = new Texture2D(GraphicsDevice, 1,1);
            textura_terreno_mover.SetData(new[] {Color.LightGreen*2f});

            //textura sobresposta do terreno do tanque seleccionado
            textura_terreno_seleccionado = new Texture2D(GraphicsDevice, 1, 1);
            textura_terreno_seleccionado.SetData(new[] {Color.DarkGreen*2f});

            //Load dos conteudos especificos da InfoBox
            infoBox.LoadContent(Content, GraphicsDevice,tanque_seleccionado);

            //Load dos conte�dos da damageBox
            damageBox.LoadContent(Content, GraphicsDevice);

            //Load dos conte�dos da turnbox
            turnBox.LoadContent(Content, GraphicsDevice);

            gerarGrelha();
            gerarTanques1();
            gerarTanques2();
            gerarPowerUps();

            //acinzentar adversarios
            acinzentarAdversarios();

            //Load dos conte�dos dos PowerUps
            foreach (PowerUp powerup in lista_powerups)
            {
                powerup.LoadContent(Content);
            }
         
            int n_tanques_equipa = numeroTanquesActivosEquipa(lista_tanques, equipa_activa);
            int n_tanques_prontos = numeroTanquesProntosEquipa(lista_tanques, equipa_activa);
            turnBox.Initializing(equipa_activa, n_tanques_equipa, n_tanques_prontos,false);
          

            //Load dos conteudos especificos dos tanques
            foreach (Tank tanque in lista_tanques)
            {
                tanque.LoadContent(Content);
            }
            
        }

        protected override void UnloadContent()
        {
            spriteBatch.Dispose();
            
            //Unload dos conteudos especificos da InfoBox
            infoBox.UnloadContent();

            //Unload dos conteudos especificos da DamageBox
            damageBox.UnloadContent();

            //Unload dos conteudos especificos da TurnBox
            turnBox.UnloadContent();

            //Unload dos conteudos especificos dos tanques
            foreach (Tank tanque in lista_tanques)
            {
                tanque.UnloadContent();
            }

        }

        private void CloseNetworkStuffOnExit()
        {
            client.Close();
            clientStream.Dispose();
            waitForServerMessages.Abort();
            waitForServerMessages.Join();
            processMessage.Abort();
            processMessage.Join();
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                CloseNetworkStuffOnExit();
                this.Exit();
            }
                

            KeyboardState teclado = Keyboard.GetState();

            turnBox.Update();

            if (teclado.IsKeyDown(Keys.Space) != teclado_anterior.IsKeyDown(Keys.Space) && teclado.IsKeyDown(Keys.Space))
            {
                executarActualizacaoJogadas(Content);
            }

            teclado_anterior = teclado;
            
            //guardar o estado do rato para topar cliques �nicos
            rato_anterior = rato;

            rato = Mouse.GetState();
            posicao_rato.X = rato.X;
            posicao_rato.Y = rato.Y;

            //verificar e agir quando o rato est� por cima de um tanque
            accoesRatoTanque();

            if (tanque_seleccionado != null)
            {
                //rodar o tanque se for caso disso
                tanque_seleccionado.Update(gameTime, lista_tanques, equipa_activa,Content);
            }

            if (verificarRatoTerrenoMover(posicao_rato) != null) apontador = seta_rato_mover;
            //topar cliques no rato
            if (rato.LeftButton == ButtonState.Pressed && rato.LeftButton != rato_anterior.LeftButton)
            {
                verificarAndar(posicao_rato);
            }

            //update explosoes
            actualizarExplosoes(gameTime);

            //update damageBox
            damageBox.Update(Content);

            actualizarGameOver();

            base.Update(gameTime);
        }

        private void actualizarGameOver()
        {
            if (numeroTanquesActivosEquipa(lista_tanques, Team.Alliance) == 0)
            {
                turnBox.Initializing(Team.Coalition, numeroTanquesActivosEquipa(lista_tanques, Team.Coalition), numeroTanquesProntosEquipa(lista_tanques, Team.Coalition), true);
            }
            if (numeroTanquesActivosEquipa(lista_tanques, Team.Coalition) == 0)
            {
                turnBox.Initializing(Team.Alliance, numeroTanquesActivosEquipa(lista_tanques, Team.Alliance), numeroTanquesProntosEquipa(lista_tanques, Team.Alliance), true);
            }
        }

        static private void acinzentarAdversarios()
        {
            foreach (Tank tanque in lista_tanques)
            {
                if (tanque.equipa != equipa_activa && tanque.activo)
                {
                    tanque.cor = Color.Yellow;
                }
                else
                {
                    tanque.cor = Color.White;
                }
            }
        }

        #region Actualizar powerups
        static private void actualizarPowerups(ContentManager Content)
        {

            for (int i = lista_powerups.Count - 1; i >= 0; i--)
            {
                lista_powerups[i].duracao--;
                if (lista_powerups[i].duracao == 0)
                {
                    lista_powerups.RemoveAt(i);
                }
            }

            while (lista_powerups.Count < 3)
            {
                lista_powerups.Add(gerarPowerupIndividual(Content));
            }


        }
        #endregion

        #region Actualizar explosoes
        private void actualizarExplosoes(GameTime gameTime)
        {
                    
            for (int i = lista_animacoes.Count - 1; i >= 0; i--)
            {
                lista_animacoes[i].Update(gameTime);
                if (lista_animacoes[i].activo == false)
                {
                    lista_animacoes.RemoveAt(i);
                }
            }
     
        }
        #endregion

        #region Verificar se est� alguem a andar
        private bool verificarAndamento()
        {
            bool em_movimento = false;
            foreach (Tank tanque in lista_tanques)
            {
                if (tanque.em_movimento == true)
                    em_movimento = true;
            }
            return em_movimento;
        }
        #endregion

        #region Verificar se � para mover um tanque
        private void verificarAndar(Vector2 posicao_rato)
        {
            if (verificarRatoTerrenoMover(posicao_rato)!=null)
            {
                Terreno terreno = verificarRatoTerrenoMover(posicao_rato);
                //foi clicada uma posi��o de andamento v�lida
                apontador = seta_rato_esperar;
                if (tanque_seleccionado.equipa == Team.Coalition) tanque_seleccionado.angulo_destino = tanque_seleccionado.anguloDoisVectores(
                    new Vector2(terreno.posicao.X + terreno.largura / 4, terreno.posicao.Y + terreno.altura / 4), 0f);
                if (tanque_seleccionado.equipa == Team.Alliance) tanque_seleccionado.angulo_destino = tanque_seleccionado.anguloDoisVectores(
                    new Vector2(terreno.posicao.X + terreno.largura / 4, terreno.posicao.Y + terreno.altura / 4), MathHelper.Pi);
                //mandar o tanque andar
                tanque_seleccionado.alterarPosicaoDestino(new Vector2(terreno.posicao.X + terreno.largura / 8, terreno.posicao.Y), terreno_pode_mover, tanque_seleccionado);
                tanque_seleccionado.posicao_grelha = terreno.posicao_grelha;
                terreno_seleccionado = null;
                

            }           
        }
        #endregion

        #region Gerar um powerup individual
        static private PowerUp gerarPowerupIndividual(ContentManager Content)
        {
            PowerUp powerup = new PowerUp();
            tipo_PowerUp tipo = tipo_PowerUp.double_shot;
            int n_powerup = gerador_numeros.Next(5);
            if (n_powerup == 0) tipo = tipo_PowerUp.double_shot;
            if (n_powerup == 1) tipo = tipo_PowerUp.double_turn;
            if (n_powerup == 2) tipo = tipo_PowerUp.mega_power;
            if (n_powerup == 3) tipo = tipo_PowerUp.sniper;
            if (n_powerup == 4) tipo = tipo_PowerUp.vida;
            Vector2 posicao_grelha = gerarPosicaoGrelhaPowerup();
            Vector2 posicao = new Vector2((grelha[(int)posicao_grelha.X, (int)posicao_grelha.Y].posicao.X) + (textura_terreno.Width * Terreno.escala / 2) - textura_tanque1.Width * Tank.escala + 3, (grelha[(int)posicao_grelha.X, (int)posicao_grelha.Y].posicao.Y));
            powerup.Initializing(tipo, posicao, posicao_grelha, Content);
            return powerup;
        }
        #endregion

        #region Actualizar Jogadas
        static public void actualizarJogadas(ContentManager Content)
        {
            jogadas++;
            if (numeroTanquesActivosEquipa(lista_tanques, equipa_activa) >= 3)
            {
                if (jogadas == 3)
                {
                    executarActualizacaoJogadas(Content);
                }
            }
            else
            {
                //O jogador activo tem menos que 3 tanques
                if(jogadas == numeroTanquesActivosEquipa(lista_tanques,equipa_activa))
                {
                    executarActualizacaoJogadas(Content);
                }
            }
        }
        #endregion

        static public void executarActualizacaoJogadas(ContentManager Content)
        {
            turnos++;
            jogadas = 0;
            if (equipa_activa == Team.Alliance)
            {
                equipa_activa = Team.Coalition;
            }
            else
            {
                equipa_activa = Team.Alliance;
            }

            //Actualizar os tanques que estiverem � espera para disparar
            foreach (Tank tanque in lista_tanques)
            {
                if (tanque.turnos_espera_disparar > 0)
                {
                    tanque.turnos_espera_disparar--;
                }
                if (!tanque.pode_mover)
                {
                    tanque.pode_mover = true;
                }
            }

            int n_tanques = numeroTanquesActivosEquipa(lista_tanques, equipa_activa);
            int n_prontos = numeroTanquesProntosEquipa(lista_tanques, equipa_activa);
            turnBox.Initializing(equipa_activa, n_tanques, n_prontos,false);

            

            //acinzentar adversarios
            acinzentarAdversarios();

            actualizarPowerups(Content);
        }

        #region Calcular numero de tanques activos de uma equipa
        static public int numeroTanquesActivosEquipa(List<Tank> lista_tanques, Team equipa_activa)
        {
            int n_tanques=0;
            foreach (Tank tanque in lista_tanques)
            {
                if (tanque.equipa == equipa_activa && tanque.activo)
                {
                    n_tanques++;
                }
            }
            return n_tanques;
        }
        #endregion

        #region Calcular numero de tanques prontos a disparar de uma equipa
        static public int numeroTanquesProntosEquipa(List<Tank> lista_tanques, Team equipa_activa)
        {
            int n_tanques = 0;
            foreach (Tank tanque in lista_tanques)
            {
                if (tanque.equipa == equipa_activa && tanque.activo && tanque.turnos_espera_disparar==0)
                {
                    n_tanques++;
                }
            }
            return n_tanques;
        }
        #endregion

        #region Verificar se o rato est� em cima de um tanque
        private Tank verificarRatoTanque()
        {
            if (!verificarAndamento())
            {
                apontador = seta_rato;
            }
            else
            {
                apontador = seta_rato_esperar;
            }
            Tank tanque_sob_rato = null;
            Rectangle rectangulo_rato = new Rectangle((int)posicao_rato.X, (int)posicao_rato.Y, 1, 1);
            foreach (Tank tanque in lista_tanques)
            {
                Rectangle rectangulo_tanque = new Rectangle((int)tanque.posicao.X, (int)tanque.posicao.Y, tanque.textura.Width / 2, tanque.textura.Height / 2);
                if (rectangulo_rato.Intersects(rectangulo_tanque) && tanque.activo)
                {
                    tanque_sob_rato = tanque;
                }
            }
            return tanque_sob_rato;
        }
        #endregion

        #region Verificar se o rato esta por cima de um terreno para o qual se possa mover
        private Terreno verificarRatoTerrenoMover(Vector2 posicao_rato)
        {
            Terreno terreno_sob = null;
            if (tanque_seleccionado != null)
            {
                foreach (Terreno terreno in terreno_pode_mover)
                {
                    Rectangle rato = new Rectangle((int)posicao_rato.X, (int)posicao_rato.Y, 1, 1);
                    Rectangle rect_terreno = new Rectangle((int)terreno.posicao.X, (int)terreno.posicao.Y, terreno.largura / 2, terreno.altura / 2);
                    if (rato.Intersects(rect_terreno))
                    {
                        terreno_sob = terreno;
                    }
                }
            }
            return terreno_sob;
        }
        #endregion

        #region Ac��es quando o rato est� por cima de um tanque
        private void accoesRatoTanque()
        {
            //quando o rato est� por cima de um tanque
            if (verificarRatoTanque() != null)
            {
                infoBox.activo = true;
                infoBox.tanque_activo = verificarRatoTanque();
                infoBox.posicao_rato = posicao_rato;
                //se n�o est� ningu�m a andar
                if (!verificarAndamento())
                {

                    if (!verificarRatoTanque().pode_mover || verificarRatoTanque().equipa!=equipa_activa)
                    {
                        apontador = seta_rato_nepias;
                    }

                    //verificar se h� um seleccionado e se o tanque apontado � da equipa advers�ria
                    if (tanque_seleccionado != null && tanque_seleccionado.equipa != verificarRatoTanque().equipa && tanque_seleccionado.turnos_espera_disparar == 0)
                    {
                        if (tanque_seleccionado.equipa == Team.Coalition) tanque_seleccionado.angulo_destino = tanque_seleccionado.anguloDoisVectores(posicao_rato, 0f);
                        if (tanque_seleccionado.equipa == Team.Alliance) tanque_seleccionado.angulo_destino = tanque_seleccionado.anguloDoisVectores(posicao_rato, MathHelper.Pi);
                        apontador = mira;
                    }
                }
            }
            else
            {
                infoBox.activo = false;
                
                /* Repor a posi��o do tanque, fica algo foleiro
                if (tanque_seleccionado != null)
                {
                    tanque_seleccionado.angulo_destino = MathHelper.Pi;
                }
                */
            }

            //clique do rato em cima de um tanque
            if (rato.LeftButton == ButtonState.Pressed && rato.LeftButton != rato_anterior.LeftButton)
            {
                //verificar se n�o esta nenhum tanque em andamento
                if(!verificarAndamento())
                seleccionarTanque(posicao_rato);
            }
        }
        #endregion

        #region Seleccionar Tanque
        private void seleccionarTanque(Tank tanque)
        {
            terreno_seleccionado = new Terreno();
            //temos um tanque no sitio que foi clicado, alterar o fundo desta posi��o
            if (tanque.seleccionado)
            {
                gerarTerrenoPossivel(tanque, "desactivar");
                terreno_seleccionado = null;
                tanque.seleccionado = false;
                tanque_seleccionado = null;
            }
            else
            {
                if (tanque.pode_mover)
                {
                    terreno_seleccionado.posicao = grelha[(int)tanque.posicao_grelha.X, (int)tanque.posicao_grelha.Y].posicao;
                    terreno_seleccionado.posicao_grelha = grelha[(int)tanque.posicao_grelha.X, (int)tanque.posicao_grelha.Y].posicao_grelha;
                    terreno_seleccionado.textura = textura_terreno;
                    if (tanque_seleccionado == null)
                    {
                        tanque.seleccionado = true;
                        tanque_seleccionado = tanque;
                        gerarTerrenoPossivel(tanque, "activar");
                    }
                    else
                    {
                        tanque.seleccionado = true;
                        gerarTerrenoPossivel(tanque_seleccionado, "desactivar");
                        terreno_seleccionado = null;
                        gerarTerrenoPossivel(tanque, "activar");
                        tanque_seleccionado.seleccionado = false;
                        terreno_seleccionado = new Terreno();
                        terreno_seleccionado.posicao = grelha[(int)tanque.posicao_grelha.X, (int)tanque.posicao_grelha.Y].posicao;
                        terreno_seleccionado.posicao_grelha = grelha[(int)tanque.posicao_grelha.X, (int)tanque.posicao_grelha.Y].posicao_grelha;
                        terreno_seleccionado.textura = textura_terreno;
                        tanque_seleccionado = tanque;
                    }
                }

            }
        }
        #endregion

        #region ATACAR!!!
        private void atacarTanque(Tank tanque)
        {
            bool morreu = false;

            //calcular dano que vamos retirar ao tanque atacado
            float distancia = Vector2.Distance(new Vector2(tanque_seleccionado.posicao.X+(tanque_seleccionado.textura.Width/4),
                    tanque_seleccionado.posicao.Y + (tanque_seleccionado.textura.Height / 4)),
                new Vector2(tanque.posicao.X + (tanque.textura.Width / 4), 
                    tanque.posicao.Y + (tanque.textura.Height / 4)));

            int firepower = tanque_seleccionado.power;

            //normalizar a distancia para dar valores entre 0 e 100
            distancia = 100 * (distancia - 66) / (1418 - 66);

            //lidar com o powe-up SNIPER
            if (tanque_seleccionado.sniper)
            {
                distancia = 0;
                tanque_seleccionado.sniper = false;
            }
            
            //power aleatorio
            int power = gerador_numeros.Next(firepower - 50);
           
            //MEGA-POWER
            if (tanque_seleccionado.mega_power)
            {
                power = 80 + power;
            }
            else
            {
                power = 50 + power;
            }
            
            //subtrair ao firepower a distancia
            int dano = power - (int)distancia*2;
            
            //retirar a vida ao tanque atacado
            if (dano > 0) { tanque.vida -= dano; } else { dano=0; };
            if (tanque.vida <= 0)
            {
                tanque.activo = false;
                if (tanque.equipa == Team.Alliance)
                {
                    tanque.textura = tanque.alliance_destruido;
                }
                else
                {
                    tanque.textura = tanque.coalition_destuido;
                }
                morreu = true;
            }

            //calcular os desvios da explosao de acordo com a sorte que teve no tiro

            int max_desvio;
            if (firepower - dano <= 0)
            {
                max_desvio = 1;
            }
            else
            {
                max_desvio = firepower - dano;
            }
            int desvio_x = gerador_numeros.Next(max_desvio);
            //normalizar desvio
            desvio_x = 50*(desvio_x - 1) / (100 - 1);
            int sinal_x = gerador_numeros.Next(2);
            if (sinal_x == 0)
            {
                sinal_x = -1;
            }
            else { sinal_x = 1; }

            int dano_maximo;
            if (firepower - dano <= 0)
            {
                dano_maximo = 199;
            }
            else
            {
                dano_maximo = firepower - dano;
            }

            int desvio_y = gerador_numeros.Next(dano_maximo);
            //normalizar desvio
            desvio_y = 50 * (desvio_y - 1) / (100 - 1);
            int sinal_y = gerador_numeros.Next(2);
            if (sinal_y == 0)
            {
                sinal_y = -1;
            }
            else { sinal_y = 1; }

            //calcular o tamanho e tempo da explosao de acordo com o dano retirado
            int tempo_explosao;
            float escala_explosao;
            if (!morreu)
            {
                tempo_explosao = 100 * (dano - 25) / (100 - 25);
                escala_explosao = 3 * (dano - 0.3f) / (100 - 0.3f);
            }
            else
            {
                tempo_explosao = 80;
                escala_explosao = 3f;
            }


            float posicao_x = (tanque.posicao.X + (tanque.textura.Width / 4))+(sinal_x * desvio_x);
            float posicao_y = (tanque.posicao.Y + (tanque.textura.Height / 4))+(sinal_y * desvio_y);
            Vector2 posicao = new Vector2(posicao_x, posicao_y);
            explode = new Animacao();
            explode.Initialize(explosao, posicao , 134, 134, 12, tempo_explosao, Color.White, escala_explosao, false,0f);
            lista_animacoes.Add(explode);

            //fogacho no tanque que dispara
            //N�O CONSIGO POR O FOGACHO NA PONTINHA DO TANQUE POR CAUSA DAS ROTA��ES! ver isto melhor

            /*
            float posicao_x_fogacho = (tanque_seleccionado.posicao.X + (tanque_seleccionado.textura.Width / 4));
            float posicao_y_fogacho = (tanque_seleccionado.posicao.Y);
            Vector2 posicao_fogacho = new Vector2(posicao_x_fogacho, posicao_y_fogacho);
            fogacho = new Animacao();
            fogacho.Initialize(explosao, posicao_fogacho, 134, 134, 12, 25, Color.White, 0.2f, false,0f);
            lista_animacoes.Add(fogacho);
            */
            
            //encontrar o terreno que cai na posicao da explosao e alterar-lhe o contador de explosoes
            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    Terreno terreno = grelha[i, j];
                    Rectangle rect_terreno = new Rectangle((int)terreno.posicao.X, (int)terreno.posicao.Y, terreno.largura / 2, terreno.altura / 2);
                    Rectangle rect_explosao = new Rectangle((int)posicao_x,(int)posicao_y,1,1);
                    if (rect_explosao.Intersects(rect_terreno))
                    {
                        if (!morreu)
                        {
                            terreno.explosoes++;
                        }
                        else
                        {
                            terreno.explosoes = 6;
                            terreno.textura = textura_cratera_1; 
                        } 
                        break;
                    }
                }
            }

            damageBox.activo = true;
            //mostrar a damageBox
            float damageBox_x, damageBox_y;
            damageBox_x = tanque.posicao.X-21;
            damageBox_y = (tanque.posicao.Y + (tanque.textura.Height/2))-DamageBox.altura;
            Vector2 posicao_damageBox = new Vector2(damageBox_x, damageBox_y);
            damageBox.Initializing(posicao_damageBox, tanque_seleccionado.power, dano,morreu);
            
            //verificar o double shot
            if (tanque_seleccionado.double_shot)
            {
                tanque_seleccionado.double_shot = false;
            }
            else
            {
                //n�o pode disparar durante 5 turnos!    
                tanque_seleccionado.turnos_espera_disparar = 5;
            }
            
            tanque_seleccionado.pode_mover = false;
            tanque_seleccionado.cor = Color.Red;
                    
            //se tem mega-power, repor o power
            if (tanque_seleccionado.mega_power)
            {
                tanque_seleccionado.power = tanque_seleccionado.power_anterior;
                tanque_seleccionado.mega_power = false;
            }

            tanque_seleccionado.seleccionado = false;
            tanque_seleccionado = null;
            terreno_pode_mover.Clear();
            terreno_seleccionado = null;
        }
        #endregion

        #region Seleccionar tanque
        private void seleccionarTanque(Vector2 posicao_rato)
        {
            //ver se nesta posicao existe algum robot
            foreach (Tank tanque in lista_tanques)
            {
                Rectangle rato = new Rectangle((int)posicao_rato.X, (int)posicao_rato.Y, 1, 1);
                Rectangle robot = new Rectangle((int)tanque.posicao.X, (int)tanque.posicao.Y, tanque.textura.Width / 2, tanque.textura.Height / 2);
                if (rato.Intersects(robot))
                {
                    if (tanque.activo)
                    {
                        //o clique caiu numa posicao em que existe um tanque
                        if (tanque_seleccionado == null || (tanque_seleccionado != null && tanque_seleccionado.equipa == tanque.equipa))
                        {
                            if(tanque.equipa == equipa_activa && tanque.pode_mover)
                            seleccionarTanque(tanque);
                        }
                        else
                        {
                            if(tanque.equipa!=equipa_activa && tanque_seleccionado.turnos_espera_disparar==0)
                            atacarTanque(tanque);
                        }
                    }
                    
                }
            }
        }
        #endregion

        #region Gerar terreno para onde � possivel mover
        private void gerarTerrenoPossivel(Tank tanque, string accao)
        {
            Vector2 posicao_original = tanque.posicao_grelha;
            List<Vector2> terrenos_alterar = new List<Vector2>();
            List<Vector2> posicoes_tanques = new List<Vector2>();

            //preencher a lista com as posi�oes de todos os tanques
            foreach (Tank tanque_individual in lista_tanques)
            {
                posicoes_tanques.Add(new Vector2((int)tanque_individual.posicao_grelha.X, (int)tanque_individual.posicao_grelha.Y));
            }

            //procurar terrenos possiveis e inseri-los na lista

            Vector2 terreno;

            for (int i = 1; i < 3; i++)
            {
                //procurar para baixo
                if ((posicao_original.X + i) < 11 && (posicao_original.X + i) > 0)
                {
                    if (!obstaculo(new Vector2((float)posicao_original.X + i, (float)posicao_original.Y),i,direccao.Baixo))
                    {
                        terreno = new Vector2((float)posicao_original.X + i, (float)posicao_original.Y);
                        terrenos_alterar.Add(terreno);
                    }

                    if (i == 1)
                    {
                        //baixo e direita
                        if ((posicao_original.X + i) <= 10 && (posicao_original.Y + i) < 20)
                        {
                            if (!obstaculo(new Vector2((float)posicao_original.X + i, (float)posicao_original.Y + i),i,direccao.BaixoDireita))
                            {
                                terreno = new Vector2((float)posicao_original.X + i, (float)posicao_original.Y + i);
                                terrenos_alterar.Add(terreno);
                            }
                        }

                        //baixo e esquerda
                        if ((posicao_original.X + i) <= 10 && (posicao_original.Y - i) >= 0)
                        {
                            if (!obstaculo(new Vector2((float)posicao_original.X + 1, (float)posicao_original.Y - i),i,direccao.BaixoEsquerda))
                            {
                                terreno = new Vector2((float)posicao_original.X + 1, (float)posicao_original.Y - i);
                                terrenos_alterar.Add(terreno);
                            }
                        }

                    }

                }
                //procurar para a esquerda
                if ((posicao_original.Y - i) >= 0)
                {
                    if (!obstaculo(new Vector2((float)posicao_original.X, (float)posicao_original.Y-i),i,direccao.Esquerda))
                    {
                        terreno = new Vector2((float)posicao_original.X, (float)posicao_original.Y-i);
                        terrenos_alterar.Add(terreno);
                    }
                }
                //procurar para a direita
                if ((posicao_original.Y + i) < 20)
                {
                    if (!obstaculo(new Vector2((float)posicao_original.X, (float)posicao_original.Y + i),i,direccao.Direita))
                    {
                        terreno = new Vector2((float)posicao_original.X, (float)posicao_original.Y + i);
                        terrenos_alterar.Add(terreno);
                    }
                }
                //procurar para cima
                if ((posicao_original.X - i) >= 0 && (posicao_original.X - i) <= 10)
                {
                    if (!obstaculo(new Vector2((float)posicao_original.X - i, (float)posicao_original.Y),i,direccao.Cima))
                    {
                        terreno = new Vector2((float)posicao_original.X - i, (float)posicao_original.Y);
                        terrenos_alterar.Add(terreno);
                    }

                    if (i == 1)
                    {
                        //cima e direita
                        if ((posicao_original.X - i) >= 0 && (posicao_original.Y + i) < 20)
                        {
                            if (!obstaculo(new Vector2((float)posicao_original.X - i, (float)posicao_original.Y + i),i,direccao.CimaDireita))
                            {
                                terreno = new Vector2((float)posicao_original.X - i, (float)posicao_original.Y + i);
                                terrenos_alterar.Add(terreno);
                            }
                        }

                        //cima e esquerda
                        if ((posicao_original.X - i) >= 0 && (posicao_original.Y - i) >= 0)
                        {
                            if (!obstaculo(new Vector2((float)posicao_original.X - 1, (float)posicao_original.Y - i),i,direccao.CimaEsquerda))
                            {
                                terreno = new Vector2((float)posicao_original.X - 1, (float)posicao_original.Y - i);
                                terrenos_alterar.Add(terreno);
                            }
                        }

                    }

                }

            }
               
            if (accao=="activar")
            {
                //alterar os terrenos encontrados
                foreach (Vector2 terreno_individual in terrenos_alterar)
                {
                    Terreno terreno_a_mover = new Terreno();
                    terreno_a_mover.posicao = grelha[(int)terreno_individual.X, (int)terreno_individual.Y].posicao;
                    terreno_a_mover.posicao_grelha = grelha[(int)terreno_individual.X, (int)terreno_individual.Y].posicao_grelha;
                    terreno_a_mover.textura = textura_terreno;
                    terreno_pode_mover.Add(terreno_a_mover);
                }

            }
            if (accao == "desactivar")
            {
                //alterar os terrenos encontrados
                terreno_pode_mover.Clear();
            }
            terrenos_alterar.Clear();
        }
        #endregion

        #region Detec��o de obst�culos ao movimento SIMPLE
        private bool obstaculo(Vector2 coordenadas)
        {
            bool obstaculo = false;
            foreach (Tank tanque_individual in lista_tanques)
            {
                if (tanque_individual.posicao_grelha.X == coordenadas.X && tanque_individual.posicao_grelha.Y == coordenadas.Y)
                {
                    obstaculo = true;
                }
            }
            return obstaculo;
        }
        #endregion

        #region Detec��o de obst�culos ao movimento HARDCORE
        private bool obstaculo(Vector2 coordenadas, int i, direccao dir)
        {
            bool obstaculo = false;
            foreach (Tank tanque_individual in lista_tanques)
            {
                if (i == 1)
                {
                    if (tanque_individual.posicao_grelha.X == coordenadas.X && tanque_individual.posicao_grelha.Y == coordenadas.Y)
                    {
                        obstaculo = true;
                    }
                    //testado, a bombar
                    if (dir == direccao.BaixoDireita)
                    {
                        if ((tanque_individual.posicao_grelha.X == (coordenadas.X - 1) && tanque_individual.posicao_grelha.Y == coordenadas.Y)
                            || (tanque_individual.posicao_grelha.X == (coordenadas.X) && tanque_individual.posicao_grelha.Y == (coordenadas.Y - 1)))
                        {
                            foreach (Tank tanque_individual2 in lista_tanques)
                            {
                                if ((tanque_individual.posicao_grelha.X == (coordenadas.X) && tanque_individual.posicao_grelha.Y == (coordenadas.Y - 1))
                                    && (tanque_individual2.posicao_grelha.X == (coordenadas.X - 1) && tanque_individual2.posicao_grelha.Y == coordenadas.Y)
                                    || (tanque_individual2.posicao_grelha.X == (coordenadas.X) && tanque_individual2.posicao_grelha.Y == (coordenadas.Y - 1))
                                    && (tanque_individual.posicao_grelha.X == (coordenadas.X - 1) && tanque_individual.posicao_grelha.Y == coordenadas.Y))
                                {
                                    obstaculo = true;
                                    break;
                                }
                            }
                        }
                    }
                    //testado, a bombar
                    if (dir == direccao.CimaDireita)
                    {
                        if ((tanque_individual.posicao_grelha.X == (coordenadas.X + 1) && tanque_individual.posicao_grelha.Y == coordenadas.Y)
                            || (tanque_individual.posicao_grelha.X == (coordenadas.X) && tanque_individual.posicao_grelha.Y == (coordenadas.Y - 1)))
                        {
                            foreach (Tank tanque_individual2 in lista_tanques)
                            {
                                if ((tanque_individual.posicao_grelha.X == (coordenadas.X) && tanque_individual.posicao_grelha.Y == (coordenadas.Y - 1))
                                    && (tanque_individual2.posicao_grelha.X == (coordenadas.X + 1) && tanque_individual2.posicao_grelha.Y == coordenadas.Y)
                                    || (tanque_individual2.posicao_grelha.X == (coordenadas.X) && tanque_individual2.posicao_grelha.Y == (coordenadas.Y - 1))
                                    && (tanque_individual.posicao_grelha.X == (coordenadas.X + 1) && tanque_individual.posicao_grelha.Y == coordenadas.Y))
                                {
                                    obstaculo = true;
                                    break;
                                }
                            }
                        }
                    }
                    //testado, a bombar
                    if (dir == direccao.BaixoEsquerda)
                    {
                        if ((tanque_individual.posicao_grelha.X == (coordenadas.X - 1) && tanque_individual.posicao_grelha.Y == coordenadas.Y)
                            || (tanque_individual.posicao_grelha.X == (coordenadas.X) && tanque_individual.posicao_grelha.Y == (coordenadas.Y + 1)))
                        {
                            foreach (Tank tanque_individual2 in lista_tanques)
                            {
                                if ((tanque_individual.posicao_grelha.X == (coordenadas.X) && tanque_individual.posicao_grelha.Y == (coordenadas.Y + 1))
                                    && (tanque_individual2.posicao_grelha.X == (coordenadas.X - 1) && tanque_individual2.posicao_grelha.Y == coordenadas.Y)
                                    || (tanque_individual2.posicao_grelha.X == (coordenadas.X) && tanque_individual2.posicao_grelha.Y == (coordenadas.Y + 1))
                                    && (tanque_individual.posicao_grelha.X == (coordenadas.X - 1) && tanque_individual.posicao_grelha.Y == coordenadas.Y))
                                {
                                    obstaculo = true;
                                    break;
                                }
                            }
                        }
                    }
                    //testado, a bombar
                    if (dir == direccao.CimaEsquerda)
                    {
                        if ((tanque_individual.posicao_grelha.X == (coordenadas.X + 1) && tanque_individual.posicao_grelha.Y == coordenadas.Y)
                            || (tanque_individual.posicao_grelha.X == (coordenadas.X) && tanque_individual.posicao_grelha.Y == (coordenadas.Y + 1)))
                        {
                            foreach (Tank tanque_individual2 in lista_tanques)
                            {
                                if ((tanque_individual.posicao_grelha.X == (coordenadas.X) && tanque_individual.posicao_grelha.Y == (coordenadas.Y + 1))
                                    && (tanque_individual2.posicao_grelha.X == (coordenadas.X + 1) && tanque_individual2.posicao_grelha.Y == coordenadas.Y)
                                    || (tanque_individual2.posicao_grelha.X == (coordenadas.X) && tanque_individual2.posicao_grelha.Y == (coordenadas.Y + 1))
                                    && (tanque_individual.posicao_grelha.X == (coordenadas.X + 1) && tanque_individual.posicao_grelha.Y == coordenadas.Y))
                                {
                                    obstaculo = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (i == 2)
                {
                    if (dir == direccao.Baixo)
                    {
                        if ((tanque_individual.posicao_grelha.X == (coordenadas.X - 1) && tanque_individual.posicao_grelha.Y == coordenadas.Y)
                            || (tanque_individual.posicao_grelha.X == (coordenadas.X) && tanque_individual.posicao_grelha.Y == coordenadas.Y))
                        {
                            obstaculo = true;
                        }
                    }
                    if (dir == direccao.Cima)
                    {
                        if (tanque_individual.posicao_grelha.X == (coordenadas.X + 1) && tanque_individual.posicao_grelha.Y == coordenadas.Y
                            || tanque_individual.posicao_grelha.X == (coordenadas.X) && tanque_individual.posicao_grelha.Y == coordenadas.Y)
                        {
                            obstaculo = true;
                        }
                    }
                    if (dir == direccao.Esquerda)
                    {
                        if (tanque_individual.posicao_grelha.X == (coordenadas.X) && tanque_individual.posicao_grelha.Y == (coordenadas.Y+1)
                            || tanque_individual.posicao_grelha.X == (coordenadas.X) && tanque_individual.posicao_grelha.Y == (coordenadas.Y))
                        {
                            obstaculo = true;
                        }
                    }
                    if (dir == direccao.Direita)
                    {
                        if (tanque_individual.posicao_grelha.X == (coordenadas.X) && tanque_individual.posicao_grelha.Y == (coordenadas.Y - 1)
                            || tanque_individual.posicao_grelha.X == (coordenadas.X) && tanque_individual.posicao_grelha.Y == (coordenadas.Y))
                        {
                            obstaculo = true;
                        }
                    }
                }
            }
            return obstaculo;
        }
        #endregion

        #region Apanhar um PowerUp
        static public bool apanharPowerUp(Tank tanque, ContentManager Content)
        {
            bool apanhou = false;
            bool passar_turno = true;
            for (int i = lista_powerups.Count - 1; i >= 0; i--)
            {
                if (lista_powerups[i].posicao_grelha == tanque.posicao_grelha)
                {
                    //tanque apanhou um powerup!
                    apanhou = true;
                    switch (lista_powerups[i].tipo) 
                    {
                        case tipo_PowerUp.double_turn:
                            tanque.double_turn = true;
                            passar_turno = false;
                            break;
                        case tipo_PowerUp.mega_power:
                            tanque.mega_power = true;
                            tanque.power = 200;
                            break;
                        case tipo_PowerUp.double_shot:
                            tanque.double_shot = true;
                            break;
                        case tipo_PowerUp.sniper:
                            tanque.sniper = true;
                            break;
                        case tipo_PowerUp.vida:
                            tanque.vida = 200;
                            break;
                        default:
                            break;
                    }
                    tipo_PowerUp tipo = lista_powerups[i].tipo;
                    lista_powerups.RemoveAt(i);
                    lista_powerups.Add(gerarPowerupIndividual(Content));
                    damageBox.activo = true;
                    damageBox.Initializing(tanque.posicao, tipo, passar_turno);
                }
            }
            return apanhou;
        }
        #endregion

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            //come�ar a desenhar
            spriteBatch.Begin();

            //desenhar grelha
            desenharGrelha();

            //desenhar quadrados para onde pode mover
            foreach (Terreno terreno in terreno_pode_mover)
            {
                terreno.Draw(spriteBatch,new Vector2(66f,66f),textura_terreno_mover);
            }

            //desenhar terreno seleccionado
            if (terreno_seleccionado != null)
            {
                terreno_seleccionado.Draw(spriteBatch, new Vector2(66f, 66f),textura_terreno_seleccionado);
            }

            //desenhar os powerups
            foreach (PowerUp powerup in lista_powerups)
            {
                powerup.Draw(spriteBatch);
            }
            
            //desenhar tanques
            foreach (Tank tanque in lista_tanques)
            {
                tanque.Draw(spriteBatch,posicao_rato);
            }

            //desenhar a InfoBox
            infoBox.Draw(spriteBatch,GraphicsDevice,tanque_seleccionado);

            //desenhar a damageBox
            if (damageBox.activo)
            {
                damageBox.Draw(spriteBatch);
            }

            //desenhar a tunrBox
            turnBox.Draw(spriteBatch, GraphicsDevice);

            //desenhar seta do rato
            Vector2 origem_rato;
            if (apontador == mira)
            {
                origem_rato = new Vector2(mira.Width / 2, mira.Height / 2);
            }
            else
            {
                origem_rato = Vector2.Zero;
            }
            spriteBatch.Draw(apontador, posicao_rato, null, Color.White, 0f, origem_rato, 1f, SpriteEffects.None, 0f);

            //desenhar explos�es
            foreach (Animacao animacao in lista_animacoes)
            {
                animacao.Draw(spriteBatch);
            }

            //parar de desenhar
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
