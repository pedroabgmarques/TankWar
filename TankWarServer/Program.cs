using Common;
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

            Console.WriteLine("Mensagem do cliente: "+ mensagem);
        }

        private static void SendMessage(Message mensagem, Player player)
        {
            TcpClient tcpClient = player.Client;
            try
            {
                mensagem.msgNumber = player.MsgCounter;

                NetworkStream clientStream = tcpClient.GetStream();
                ASCIIEncoding encoder = new ASCIIEncoding();
                byte[] buffer = mensagem.byteMessage();
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
                Array equipas = Enum.GetValues(typeof(Team));
                newPlayerTeam = (Team)equipas.GetValue(random.Next(equipas.Length));
            }
            return newPlayerTeam;
        }

        private static void MatchMaking(Player player)
        {
            Player waitingPlayer = listaJogadores.Find(x => x.PlayerStatus == PlayerStatus.Waiting);
            if (waitingPlayer != null)
            {
                //Temos um jogador à espera de adversário!
                waitingPlayer.PlayerStatus = PlayerStatus.Playing;
                player.PlayerStatus = PlayerStatus.Playing;
                listaJogos.Add(new Game(waitingPlayer, player));
                Console.WriteLine("Novo jogador iniciou um novo jogo!");

                IniciarJogo(waitingPlayer, player);
            }
            else
            {
                player.PlayerStatus = PlayerStatus.Waiting;
                Console.WriteLine("Novo jogador foi colocado em espera.");
                SendMessage(new ControlMessage(MessageType.Control, ControlCommand.Lobby), player);
                SendMessage(new GameMoveMessage(MessageType.Move, 10, 5), player);
                SendMessage(new ControlMessage(MessageType.Control, ControlCommand.Lobby), player);
                SendMessage(new GameMoveMessage(MessageType.Move, 10, 5), player);
                SendMessage(new ControlMessage(MessageType.Control, ControlCommand.Lobby), player);
                SendMessage(new GameMoveMessage(MessageType.Move, 10, 5), player);
                SendMessage(new ControlMessage(MessageType.Control, ControlCommand.Lobby), player);
                SendMessage(new GameMoveMessage(MessageType.Move, 10, 5), player);
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

                    listaJogos.Add(new Game(player1, player2));

                    IniciarJogo(player1, player2);

                    Console.WriteLine("\nNovo jogo começado entre jogadores solitários.");
                    EscreverStats();

                    contadorSolitarios -= 2;
                }
            }
        }

        private static void IniciarJogo(Player player1, Player player2)
        {
            if (random.Next(1) == 0)
            {
                player1.PlayerStatus = PlayerStatus.Playing;
                SendMessage(new ControlMessage(MessageType.Control, ControlCommand.StartGameYourTurn), player1);
                SendMessage(new ControlMessage(MessageType.Control, ControlCommand.StartGameAdversaryTurn), player2);
            }
            else
            {
                player1.PlayerStatus = PlayerStatus.Playing;
                SendMessage(new ControlMessage(MessageType.Control, ControlCommand.StartGameAdversaryTurn), player1);
                SendMessage(new ControlMessage(MessageType.Control, ControlCommand.StartGameYourTurn), player2);
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
