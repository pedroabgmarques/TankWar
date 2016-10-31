using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RobotWar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TankWar.Network;

namespace TankWarServer
{
    class Program
    {
        static int port = 7777;

        static TcpListener tcpListener;
        static Thread listenThread;
        static Thread clientThread;
        static Thread processar;
        static List<Player> listaJogadores;
        static List<Game> listaJogos;

        static Random random;

        #region Entry Point
        static void Main(string[] args)
        {
            Initialize();
            StartServer();
        }
        #endregion

        #region Initialize
        static private void Initialize()
        {
            listaJogadores = new List<Player>();
            listaJogos = new List<Game>();
            random = new Random();
        }
        #endregion

        #region Server Controller
        private static void StartServer()
        {
            Console.WriteLine("Servidor iniciado.");
            Console.WriteLine("A escutar no porto "+ port +"...");

            tcpListener = new TcpListener(IPAddress.Any, port);
            listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();
        }

        private static void ListenForClients()
        {
            tcpListener.Start();

            while (true)
            {
                //Aguarda que um cliente se ligue ao servidor
                TcpClient client = new TcpClient();

                client = tcpListener.AcceptTcpClient();

                Console.WriteLine("\nCliente ligado: " + client.Client.RemoteEndPoint.ToString());

                //Receber o cliente
                NewPlayerReception(client);

                //Criar uma thread para comunicar com este cliente
                clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);

            }
        }

        private static void ClientDisconnected(string reason, TcpClient client)
        {
            Console.WriteLine("\n" + reason + " " + client.Client.RemoteEndPoint);

            Player player = listaJogadores.Find(x => x.Client == client);
            if (player != null)
            {
                //Estava num jogo?
                Game jogo = listaJogos.Find(x => x.player1 == player || x.player2 == player);
                if (jogo != null)
                {
                    //Avisar o adversário de que o outro jogador saiu
                    if (player == jogo.player1)
                    {
                        jogo.player2.PlayerStatus = PlayerStatus.Waiting;
                        SendMessage(new ControlMessage(MessageType.Control, ControlCommand.AdversaryQuit), jogo.player2);
                    }
                    else
                    {
                        jogo.player1.PlayerStatus = PlayerStatus.Waiting;
                        SendMessage(new ControlMessage(MessageType.Control, ControlCommand.AdversaryQuit), jogo.player1);
                    }
                    listaJogos.Remove(jogo);
                    Console.WriteLine("1 jogo terminado.");
                }
                listaJogadores.Remove(player);
            }
            EscreverStats();

            //Verificar se existem clientes em espera por ambos os adversários terem saido
            CheckForLonelyPlayers();
        }

        private static void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;

            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {

                if (tcpClient.Connected)
                {
                    bytesRead = 0;

                    try
                    {
                        //blocks until a client sends a message
                        bytesRead = clientStream.Read(message, 0, 4096);
                    }
                    catch
                    {
                        //a socket error has occured
                        ClientDisconnected("Cliente desligado: (socket error)", tcpClient);
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        //the client has disconnected from the server
                        ClientDisconnected("Cliente desligado: (client disconnected)", tcpClient);
                        break;
                    }

                    //message has successfully been received
                    ASCIIEncoding encoder = new ASCIIEncoding();

                    string mensagem = encoder.GetString(message, 0, bytesRead);
                    object objecto_mensagem = (object)mensagem;
                    object objecto_cliente = (object)tcpClient;
                    object[] parametros = { objecto_mensagem, objecto_cliente };
                    object param = (object)parametros;

                    processar = new Thread(new ParameterizedThreadStart(ProcessMessage));
                    try
                    {
                        processar.Start(param);
                    }
                    catch
                    {
                        Console.WriteLine("Erro na receção da mensagem!");
                        processar.Abort();
                        processar.Join();
                    }
                }
                else
                {
                    ClientDisconnected("Cliente desligado: (Connected = false)", tcpClient);
                }          
            }

            tcpClient.Close();
        }

        private static void ProcessMessage(object param)
        {
            object[] parametros = (object[])param;
            string mensagem = (string)parametros[0];
            TcpClient cliente = (TcpClient)parametros[1];

            Console.WriteLine("\nMensagem de " + cliente .Client.RemoteEndPoint+ ": " + mensagem);

            //Serializar a mensagem para um objeto JSON para lermos o tipo de mensagem
            JObject jsonObj = JsonConvert.DeserializeObject<JObject>(mensagem);
            int intMsgType = (int)jsonObj["msgType"];
            MessageType msgType = (MessageType)intMsgType;

            //Switch ao tipo de mensagem
            switch (msgType)
            {
                case MessageType.Control:
                    //O cliente não envia mensagens de controlo, nada a fazer aqui.
                    break;
                case MessageType.Move:
                    //Serializar a mensagem para o tipo correto
                    GameMoveMessage moveMessage = JsonConvert.DeserializeObject<GameMoveMessage>(jsonObj.ToString());
                    //Encontrar o jogador que moveu
                    Player jogadorMove = listaJogadores.Find(p => p.Client == cliente);
                    Tuple<Player, Game> adversarioEJogoMove = EncontrarAdversarioEJogo(jogadorMove);
                    if (adversarioEJogoMove.Item1 != null)
                    {
                        //Reencaminhar a mensagem de movimento para o adversário
                        SendMessage(moveMessage, adversarioEJogoMove.Item1);
                    }
                    break;
                case MessageType.Attack:
                    //Serializar a mensagem para o tipo correto
                    GameAttackMessage attackMessage = JsonConvert.DeserializeObject<GameAttackMessage>(jsonObj.ToString());
                    //Encontrar o jogador que atacou
                    Player jogadorAttack = listaJogadores.Find(p => p.Client == cliente);
                    Tuple<Player, Game> adversarioEJogoAttack = EncontrarAdversarioEJogo(jogadorAttack);
                    if (adversarioEJogoAttack.Item1 != null)
                    {
                        //Reencaminhar a mensagem de movimento para o adversário
                        SendMessage(attackMessage, adversarioEJogoAttack.Item1);
                    }
                    break;
                case MessageType.EndTurn:
                    //Mensagem não tem informação, é apenas um sinal, não é preciso desserializá-la
                    //Encontrar o jogador que terminou o turno
                    Player jogadorEndTurn = listaJogadores.Find(p => p.Client == cliente);
                    Tuple<Player, Game> adversarioEJogoEndTurn = EncontrarAdversarioEJogo(jogadorEndTurn);
                    if (adversarioEJogoEndTurn.Item1 != null && adversarioEJogoEndTurn.Item2 != null)
                    {
                        //Atualizar o jogador que tem a vez de jogar agora
                        adversarioEJogoEndTurn.Item2.currentPlayer = adversarioEJogoEndTurn.Item1;
                        //Enviar mensagens para os jogadores com a trocar de turnos
                        SendMessage(new ControlMessage(MessageType.Control, ControlCommand.AdversaryTurn), jogadorEndTurn);
                        SendMessage(new ControlMessage(MessageType.Control, ControlCommand.YourTurn), adversarioEJogoEndTurn.Item1);
                        Console.WriteLine("\nMudança de turno.");
                    }
                    
                    break;
                case MessageType.PowerUpList:
                    //Serializar a mensagem para o tipo correto
                    PowerUpListMessage powerUpListMessage = JsonConvert.DeserializeObject<PowerUpListMessage>(jsonObj.ToString());
                    Player jogadorPowerUpList = listaJogadores.Find(p => p.Client == cliente);
                    Tuple<Player, Game> adversarioEJogopowerUpList = EncontrarAdversarioEJogo(jogadorPowerUpList);
                    //Reencaminhar a lista de powerUps para o adversario
                    SendMessage(powerUpListMessage, adversarioEJogopowerUpList.Item1);
                    break;
                case MessageType.PowerUp:
                    //Serializar a mensagem para o tipo correto
                    PowerUpMessage powerUpMessage = JsonConvert.DeserializeObject<PowerUpMessage>(jsonObj.ToString());
                    //Encontrar o jogador que moveu
                    Player jogadorPowerUp = listaJogadores.Find(p => p.Client == cliente);
                    Tuple<Player, Game> adversarioEJogoPowerUp = EncontrarAdversarioEJogo(jogadorPowerUp);
                    if (adversarioEJogoPowerUp.Item1 != null)
                    {
                        //Reencaminhar a mensagem de movimento para o adversário
                        SendMessage(powerUpMessage, adversarioEJogoPowerUp.Item1);
                    }
                    break;
                case MessageType.PlayerWon:
                    //Mensagem não tem informação, é apenas um sinal, não é preciso desserializá-la
                    //Encontrar o jogador que ganhou o jogo
                    Player jogadorPlayerWon = listaJogadores.Find(p => p.Client == cliente);
                    Tuple<Player, Game> adversarioEJogoPlayerWon = EncontrarAdversarioEJogo(jogadorPlayerWon);
                    if (adversarioEJogoPlayerWon.Item1 != null)
                    {
                        //Enviar mensagem para os jogadores a informar quem ganhou e quem perdeu
                        SendMessage(new ControlMessage(MessageType.Control, ControlCommand.GameEndYouWin), jogadorPlayerWon);
                        SendMessage(new ControlMessage(MessageType.Control, ControlCommand.GameEndAdversaryWins), adversarioEJogoPlayerWon.Item1);
                        Console.WriteLine("\nJogo terminado!");
                    }
                    break;
                case MessageType.TankList:
                    //O primeiro jogador a jogar envia a lista de powers e vida dos tanques para serem passados ao segundo jogador
                    //Serializar a mensagem para o tipo correto
                    TankListMessage tankListMessage = JsonConvert.DeserializeObject<TankListMessage>(jsonObj.ToString());
                    //Encontrar o jogador que moveu
                    Player jogadorTankList = listaJogadores.Find(p => p.Client == cliente);
                    Tuple<Player, Game> adversarioEJogoTankList = EncontrarAdversarioEJogo(jogadorTankList);
                    if (adversarioEJogoTankList.Item1 != null)
                    {
                        //Reencaminhar a mensagem de movimento para o adversário
                        SendMessage(tankListMessage, adversarioEJogoTankList.Item1);
                    }
                    break;
                default:
                    break;
            }
        }

        private static Tuple<Player, Game> EncontrarAdversarioEJogo(Player jogador)
        {
            Tuple<Player, Game> adversarioEJogo = null;
            //Encontrar o adversário
            adversarioEJogo = new Tuple<Player, Game>(
                    listaJogadores.Find(j => j.CurrentGame == jogador.CurrentGame && j.Equipa != jogador.Equipa), jogador.CurrentGame);
            
            return adversarioEJogo;
        }

        private static void SendMessage(Message mensagem, Player player)
        {
            TcpClient tcpClient = player.Client;
            try
            {
                mensagem.msgNumber = player.MsgCounter;

                NetworkStream clientStream = tcpClient.GetStream();
                byte[] buffer = mensagem.ByteMessage();
                clientStream.Write(buffer, 0, buffer.Length);
            }
            catch
            {
                Console.WriteLine("Erro no envio de mensagem para cliente " + tcpClient.Client.RemoteEndPoint.ToString());
            }
        }

        #endregion

        #region MatchMaking / Lobby
        private static void NewPlayerReception(TcpClient client)
        {
            //Criar uma instância de Player
            Player player = new Player(client, AssignNewPlayerTeam());
            Console.WriteLine("Equipa assignada ao novo jogador: " + player.Equipa);

            MatchMaking(player);

            listaJogadores.Add(player);
            EscreverStats();
        }

        private static Team AssignNewPlayerTeam()
        {
            Team newPlayerTeam = Team.Alliance; //Este valor é alterado nas próximas linhas
            Player waitingPlayer = listaJogadores.Find(x => x.PlayerStatus == PlayerStatus.Waiting);
            if (waitingPlayer != null)
            {
                //Existe um jogador em espera, vamos ser da equipa adversária
                Team adversaria = waitingPlayer.Equipa;
                if (adversaria == Team.Alliance)
                {
                    newPlayerTeam = Team.Coalition;
                }
                else
                {
                    newPlayerTeam = Team.Alliance;
                }
            }
            else
            {
                //É o primeiro jogador ou todos os outros estão aos pares, equipa aleatória
                //newPlayerTeam = GetRandomTeam();
                newPlayerTeam = Team.Alliance;
            }
            return newPlayerTeam;
        }

        private static Team GetRandomTeam()
        {
            if (random.NextDouble() > 0.5)
            {
                return Team.Alliance;
            }
            else
            {
                return Team.Coalition;
            }
        }

        private static void MatchMaking(Player player)
        {
            Player waitingPlayer = listaJogadores.Find(x => x.PlayerStatus == PlayerStatus.Waiting);

            if (waitingPlayer != null)
            {
                //Temos um jogador à espera de adversário!
                waitingPlayer.PlayerStatus = PlayerStatus.Playing;
                player.PlayerStatus = PlayerStatus.Playing;
                Game jogo = new Game(waitingPlayer, player);
                listaJogos.Add(jogo);
                waitingPlayer.CurrentGame = jogo;
                player.CurrentGame = jogo;

                Console.WriteLine("Novo jogador iniciou um novo jogo!");

                if (player.Equipa == Team.Alliance)
                {
                    SendMessage(new ControlMessage(MessageType.Control, ControlCommand.TeamAlliance), player);
                    SendMessage(new ControlMessage(MessageType.Control, ControlCommand.TeamCoalition), waitingPlayer);
                }
                else
                {
                    SendMessage(new ControlMessage(MessageType.Control, ControlCommand.TeamCoalition), player);
                    SendMessage(new ControlMessage(MessageType.Control, ControlCommand.TeamAlliance), waitingPlayer);
                }
                
                IniciarJogo(waitingPlayer, player, jogo);
            }
            else
            {
                player.PlayerStatus = PlayerStatus.Waiting;
                Console.WriteLine("Novo jogador foi colocado em espera.");
                SendMessage(new ControlMessage(MessageType.Control, ControlCommand.Lobby), player);
            }
        }

        private static void CheckForLonelyPlayers()
        {
            int contadorSolitarios = listaJogadores.Count(x => x.PlayerStatus == PlayerStatus.Waiting);
            if (contadorSolitarios >= 2)
            {
                while (contadorSolitarios > 1)
                {
                    Player player1 = listaJogadores.Find(x => x.PlayerStatus == PlayerStatus.Waiting);
                    Player player2 = listaJogadores.Find(x => x.PlayerStatus == PlayerStatus.Waiting && x != player1);

                    player1.PlayerStatus = PlayerStatus.Playing;
                    player2.PlayerStatus = PlayerStatus.Playing;

                    //Verificar equipas
                    if (player1.Equipa == player2.Equipa)
                    {
                        if (player1.Equipa == Team.Alliance)
                        {
                            player2.Equipa = Team.Coalition;
                        }
                        else
                        {
                            player2.Equipa = Team.Alliance;
                        }
                    }

                    Game jogo = new Game(player1, player2);
                    listaJogos.Add(jogo);
                    player1.CurrentGame = jogo;
                    player2.CurrentGame = jogo;

                    if (player1.Equipa == Team.Alliance)
                    {
                        SendMessage(new ControlMessage(MessageType.Control, ControlCommand.TeamAlliance), player1);
                        SendMessage(new ControlMessage(MessageType.Control, ControlCommand.TeamCoalition), player2);
                    }
                    else
                    {
                        SendMessage(new ControlMessage(MessageType.Control, ControlCommand.TeamAlliance), player2);
                        SendMessage(new ControlMessage(MessageType.Control, ControlCommand.TeamCoalition), player1);
                    }

                    IniciarJogo(player1, player2, jogo);

                    Console.WriteLine("\nNovo jogo começado entre jogadores solitários.");
                    EscreverStats();

                    contadorSolitarios -= 2;
                }
            }
        }

        private static void IniciarJogo(Player player1, Player player2, Game jogo)
        {
            player1.PlayerStatus = PlayerStatus.Playing;
            player2.PlayerStatus = PlayerStatus.Playing;
            if (random.NextDouble() > 0.5)
            {
                SendMessage(new ControlMessage(MessageType.Control, ControlCommand.StartGameYourTurn), player1);
                SendMessage(new ControlMessage(MessageType.Control, ControlCommand.StartGameAdversaryTurn), player2);
                jogo.currentPlayer = player1;
            }
            else
            {
                SendMessage(new ControlMessage(MessageType.Control, ControlCommand.StartGameAdversaryTurn), player1);
                SendMessage(new ControlMessage(MessageType.Control, ControlCommand.StartGameYourTurn), player2);
                jogo.currentPlayer = player2;
            }
        }
        #endregion

        #region Utils
        private static void EscreverStats()
        {
            Console.WriteLine("Numero de jogos: " + listaJogos.Count);
            Console.WriteLine("Numero de jogadores: " + listaJogadores.Count);
        }
        #endregion

    }
}
