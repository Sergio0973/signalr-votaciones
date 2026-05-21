using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace VotingConsole;

public record PollOptionDto(Guid Id, string Text, int VoteCount);
public record PollDto(Guid Id, string Question, List<PollOptionDto> Options, bool IsActive, int TotalVotes);

class Program
{
    private static HubConnection? _connection;
    private static string _voterId = "";
    private static string _nickname = "";
    private static PollDto? _currentPoll;
    private static Guid? _myVoteOptionId;
    private static bool _hasVoted = false;
    private static bool _isAdmin = false;
    private static bool _isTyping = false;

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==================================================");
        Console.WriteLine("        SISTEMA DE VOTACIÓN EN VIVO (SIGNALR)     ");
        Console.WriteLine("==================================================");
        Console.ResetColor();

        Console.WriteLine("Selecciona tu rol:");
        Console.WriteLine(" [1] Votante (Participar en la clase)");
        Console.WriteLine(" [2] Administrador (Presentador - Controlar Votaciones)");
        Console.Write("\nOpción (1 o 2): ");
        
        string roleChoice = Console.ReadLine()?.Trim() ?? "1";
        if (roleChoice == "2")
        {
            _isAdmin = true;
        }

        if (!_isAdmin)
        {
            Console.Write("\nIngresa tu Nombre/Apodo: ");
            _nickname = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(_nickname))
            {
                _nickname = "Estudiante_" + new Random().Next(100, 999);
            }
            _voterId = _nickname;
        }

        Console.Write("Ingresa la URL del Servidor (Enter para http://localhost:5280): ");
        string serverUrl = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrEmpty(serverUrl))
        {
            serverUrl = "http://localhost:5280";
        }

        string hubUrl = serverUrl.TrimEnd('/') + "/pollHub";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.Reconnecting += (error) =>
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[Conexión perdida. Intentando reconectar...]");
            Console.ResetColor();
            return Task.CompletedTask;
        };

        _connection.Reconnected += (connectionId) =>
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[¡Conexión restablecida!]");
            Console.ResetColor();
            return Task.CompletedTask;
        };

        _connection.On<PollDto>("PollUpdated", (poll) =>
        {
            _currentPoll = poll;
            if (!_isTyping)
            {
                if (_isAdmin)
                    RenderAdminScreen();
                else
                    RenderScreen();
            }
        });

        _connection.On("PollClosed", () =>
        {
            _currentPoll = null;
            _hasVoted = false;
            _myVoteOptionId = null;
            
            if (!_isTyping)
            {
                if (_isAdmin)
                {
                    RenderAdminScreen();
                }
                else
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("==================================================");
                    Console.WriteLine("La votación activa ha sido cerrada por el admin.");
                    Console.WriteLine("Esperando una nueva votación...");
                    Console.WriteLine("==================================================");
                    Console.ResetColor();
                }
            }
        });

        _connection.On<string>("VoteFailed", (message) =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[ERROR]: {message}");
            Console.ResetColor();
        });

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Conectando a {hubUrl}...");
        Console.ResetColor();

        try
        {
            await _connection.StartAsync();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("¡Conectado exitosamente!");
            Console.ResetColor();
            await Task.Delay(1000);

            if (_isAdmin)
            {
                RenderAdminScreen();
                await RunAdminLoopAsync();
            }
            else
            {
                if (_currentPoll == null)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("==================================================");
                    Console.WriteLine($"Hola {_nickname}, estás listo para votar.");
                    Console.WriteLine("Esperando a que el presentador inicie la votación...");
                    Console.WriteLine("==================================================");
                    Console.ResetColor();
                }
                else
                {
                    RenderScreen();
                }
                await RunVoterLoopAsync();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error de conexión: {ex.Message}");
            Console.WriteLine("Asegúrate de que la API esté corriendo y la URL sea correcta.");
            Console.ResetColor();
            Console.WriteLine("\nPresiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }

    private static async Task RunAdminLoopAsync()
    {
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.KeyChar == '1')
            {
                _isTyping = true;
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("==================================================");
                Console.WriteLine("          CREAR NUEVA VOTACIÓN EN VIVO            ");
                Console.WriteLine("==================================================");
                Console.ResetColor();

                Console.Write("Ingresa la Pregunta: ");
                string question = Console.ReadLine()?.Trim() ?? "";

                Console.Write("Ingresa las Opciones (separadas por comas): ");
                string optionsInput = Console.ReadLine()?.Trim() ?? "";

                if (string.IsNullOrEmpty(question) || string.IsNullOrEmpty(optionsInput))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nError: Pregunta u opciones vacías. Operación cancelada.");
                    Console.ResetColor();
                    Console.WriteLine("Presiona cualquier tecla para regresar...");
                    Console.ReadKey();
                    _isTyping = false;
                    RenderAdminScreen();
                    continue;
                }

                var options = optionsInput.Split(',')
                    .Select(o => o.Trim())
                    .Where(o => !string.IsNullOrEmpty(o))
                    .ToList();

                if (options.Count < 2)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nError: Debes escribir al menos 2 opciones.");
                    Console.ResetColor();
                    Console.WriteLine("Presiona cualquier tecla para regresar...");
                    Console.ReadKey();
                    _isTyping = false;
                    RenderAdminScreen();
                    continue;
                }

                try
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\nEnviando y publicando votación...");
                    Console.ResetColor();
                    
                    await _connection!.InvokeAsync("CreatePoll", question, options);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error al crear votación: {ex.Message}");
                    Console.ResetColor();
                    Console.ReadKey();
                }

                _isTyping = false;
                RenderAdminScreen();
            }
            else if (key.KeyChar == '2')
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\nCerrando votación activa...");
                    Console.ResetColor();
                    
                    await _connection!.InvokeAsync("ClosePoll");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error al cerrar votación: {ex.Message}");
                    Console.ResetColor();
                    Console.ReadKey();
                }
                RenderAdminScreen();
            }
            else if (key.KeyChar == '3')
            {
                Console.Clear();
                Console.WriteLine("Saliendo del administrador. ¡Adiós!");
                Environment.Exit(0);
            }
            else
            {
                RenderAdminScreen();
            }
        }
    }

    private static async Task RunVoterLoopAsync()
    {
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (_currentPoll != null && _currentPoll.IsActive)
            {
                if (char.IsDigit(key.KeyChar))
                {
                    int index = (int)char.GetNumericValue(key.KeyChar) - 1;
                    if (index >= 0 && index < _currentPoll.Options.Count)
                    {
                        var selectedOption = _currentPoll.Options[index];
                        _myVoteOptionId = selectedOption.Id;
                        _hasVoted = true;

                        try
                        {
                            await _connection!.InvokeAsync("CastVote", selectedOption.Id, _voterId);
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"\nError al enviar voto: {ex.Message}");
                            Console.ResetColor();
                        }
                    }
                }
                else if (key.KeyChar == 'c' || key.KeyChar == 'C')
                {
                    _hasVoted = false;
                    RenderScreen();
                }
            }
        }
    }

    private static void RenderAdminScreen()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("==================================================");
        Console.WriteLine("         PANEL DE ADMINISTRACIÓN (PRESENTER)      ");
        Console.WriteLine("==================================================");
        Console.ResetColor();

        if (_currentPoll != null && _currentPoll.IsActive)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"PREGUNTA ACTIVA: {_currentPoll.Question}");
            Console.WriteLine($"Votos recibidos en la sala: {_currentPoll.TotalVotes}");
            Console.WriteLine("--------------------------------------------------\n");

            int maxVotes = _currentPoll.Options.Any() ? _currentPoll.Options.Max(o => o.VoteCount) : 0;

            for (int i = 0; i < _currentPoll.Options.Count; i++)
            {
                var opt = _currentPoll.Options[i];
                double percentage = _currentPoll.TotalVotes > 0 ? (double)opt.VoteCount / _currentPoll.TotalVotes * 100 : 0;
                
                int barWidth = 20;
                int filledWidth = _currentPoll.TotalVotes > 0 ? (int)Math.Round(percentage / 100 * barWidth) : 0;
                string bar = new string('█', filledWidth) + new string('░', barWidth - filledWidth);

                Console.WriteLine($"   {i + 1}. {opt.Text.PadRight(15)} [{bar}] {opt.VoteCount} votos ({percentage:F0}%)");
            }
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("No hay ninguna votación activa en este momento.");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n==================================================");
        Console.ResetColor();
        Console.WriteLine(" [1] Iniciar nueva votación");
        Console.WriteLine(" [2] Cerrar votación activa");
        Console.WriteLine(" [3] Salir");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("==================================================");
        Console.ResetColor();
        Console.Write("Selecciona una opción: ");
    }

    private static void RenderScreen()
    {
        if (_currentPoll == null) return;

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("==================================================");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"PREGUNTA: {_currentPoll.Question}");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("==================================================");
        Console.ResetColor();

        Console.WriteLine($"Usuario: {_nickname} | Votos totales en la sala: {_currentPoll.TotalVotes}");
        Console.WriteLine("--------------------------------------------------\n");

        if (!_hasVoted)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Presiona el NÚMERO en tu teclado para votar:\n");
            Console.ResetColor();

            for (int i = 0; i < _currentPoll.Options.Count; i++)
            {
                Console.WriteLine($" [{i + 1}] {_currentPoll.Options[i].Text}");
            }
            Console.WriteLine("\n--------------------------------------------------");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("¡Has votado! A continuación verás los resultados en tiempo real.\n");
            Console.ResetColor();

            int maxVotes = _currentPoll.Options.Any() ? _currentPoll.Options.Max(o => o.VoteCount) : 0;

            for (int i = 0; i < _currentPoll.Options.Count; i++)
            {
                var opt = _currentPoll.Options[i];
                double percentage = _currentPoll.TotalVotes > 0 ? (double)opt.VoteCount / _currentPoll.TotalVotes * 100 : 0;
                
                int barWidth = 20;
                int filledWidth = _currentPoll.TotalVotes > 0 ? (int)Math.Round(percentage / 100 * barWidth) : 0;
                string bar = new string('█', filledWidth) + new string('░', barWidth - filledWidth);

                bool isMyVote = opt.Id == _myVoteOptionId;

                if (isMyVote)
                    Console.ForegroundColor = ConsoleColor.Cyan;

                string label = $"{(isMyVote ? "-> " : "   ")}{i + 1}. {opt.Text.PadRight(15)}";
                Console.WriteLine($"{label} [{bar}] {opt.VoteCount} votos ({percentage:F0}%)");
                Console.ResetColor();
            }

            Console.WriteLine("\n--------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Presiona [C] si deseas cambiar tu voto.");
            Console.ResetColor();
        }
    }
}
