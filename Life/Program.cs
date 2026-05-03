using ScottPlot;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace cli_life
{
    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            IsAliveNext = IsAlive ? liveNeighbors == 2 || liveNeighbors == 3 : liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    Cells[x, y] = new Cell();
                }
            }

            ConnectNeighbors();
            Randomize(liveDensity);
        }

        public Board(int cellSize, Cell[,] cells)
        {
            CellSize = cellSize;
            Cells = cells;
            ConnectNeighbors();
        }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
            {
                cell.IsAlive = rand.NextDouble() < liveDensity;
            }
        }
        public void Advance()
        {
            foreach (var cell in Cells)
            {
                cell.DetermineNextLiveState();
            }

            foreach (var cell in Cells)
            {
                cell.Advance();
            }
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }
        public void Save(string filename = "Condition.txt")
        {
            using var writer = new StreamWriter(filename);
            writer.WriteLine($"{Rows}&{Columns}&{CellSize}");
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    if (Cells[col, row].IsAlive)
                    {
                        writer.Write('*');
                    }
                    else
                    {
                        writer.Write(' ');
                    }
                }
                writer.WriteLine();
            }
        }
        public static Board Load(string filename = "Condition.txt")
        {
            string[] lines = File.ReadAllLines(filename);
            string[] sizes = lines[0].Split('&');
            int rows = int.Parse(sizes[0]);
            int columns = int.Parse(sizes[1]);
            int cellSize = int.Parse(sizes[2]);
            Cell[,] readenCell = new Cell[columns, rows];
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    readenCell[col, row] = new Cell()
                    {
                        IsAlive = lines[row + 1][col] == '*'
                    };
                }
            }
            return new Board(cellSize, readenCell);
        }
        public int CountAlive()
        {
            int count = 0;
            for (int i = 0; i < Columns; i++)
            {
                for (int j = 0; j < Rows; j++)
                {
                    if (Cells[i, j].IsAlive)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
        public int CountCombinations()
        {
            var visited = new bool[Columns, Rows];
            int combinations = 0;
            int[] di = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] dj = { -1, -1, -1, 0, 0, 1, 1, 1 };

            for (int i = 0; i < Columns; i++)
            {
                for (int j = 0; j < Rows; j++)
                {
                    if (Cells[i, j].IsAlive && !visited[i, j])
                    {
                        combinations++;
                        var queue = new Queue<(int x, int y)>();
                        queue.Enqueue((i, j));
                        visited[i, j] = true;
                        while (queue.Count > 0)
                        {
                            var (cx, cy) = queue.Dequeue();
                            for (int d = 0; d < 8; d++)
                            {
                                int nx = cx + di[d];
                                int ny = cy + dj[d];
                                if (nx >= 0 && nx < Columns && ny >= 0 && ny < Rows &&
                                    Cells[nx, ny].IsAlive && !visited[nx, ny])
                                {
                                    visited[nx, ny] = true;
                                    queue.Enqueue((nx, ny));
                                }
                            }
                        }
                    }
                }
            }
            return combinations;
        }
        public List<HashSet<(int, int)>> GetAllCombinations()
        {
            var visited = new bool[Columns, Rows];
            var combinations = new List<HashSet<(int, int)>> ();
            int[] di = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] dj = { -1, -1, -1, 0, 0, 1, 1, 1 };

            for (int i = 0; i < Columns; i++)
            {
                for (int j = 0; j < Rows; j++)
                {
                    if (Cells[i, j].IsAlive && !visited[i, j])
                    {
                        var combo = new HashSet<(int, int)>();
                        var queue = new Queue<(int x, int y)>();
                        queue.Enqueue((i, j));
                        visited[i, j] = true;
                        combo.Add((i, j));
                        while (queue.Count > 0)
                        {
                            var (cx, cy) = queue.Dequeue();
                            for (int d = 0; d < 8; d++)
                            {
                                int nx = cx + di[d];
                                int ny = cy + dj[d];
                                if (nx >= 0 && nx < Columns && ny >= 0 && ny < Rows &&
                                    Cells[nx, ny].IsAlive && !visited[nx, ny])
                                {
                                    visited[nx, ny] = true;
                                    queue.Enqueue((nx, ny));
                                    combo.Add((nx, ny));
                                }
                            }
                        }
                        combinations.Add(combo);
                    }
                }
            }
            return combinations;
        }

    }
    public class Program
    {
        static Board _board;
        static Settings _settings;
        static bool _pauseFlag;
        static Dictionary<string, List<HashSet<(int x, int y)>>> _shapes = [];
        static private void Reset()
        {
            _board = new Board(_settings.Width, _settings.Height, _settings.CellSize, _settings.LiveDensity);
        }
        static void Render()
        {
            for (int row = 0; row < _board.Rows; row++)
            {
                for (int col = 0; col < _board.Columns; col++)
                {
                    var cell = _board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        Console.Write('*');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.Write('\n');
            }
        }
        static public void InitShapes()
        {
            _shapes["Block"] = LoadPattern("Block.txt");

            _shapes["Boat"] = LoadPattern("Boat.txt");

            _shapes["Hive"] = LoadPattern("Hive.txt");

            _shapes["LongHive"] = LoadPattern("LongHive.txt");

            _shapes["Pond"] = LoadPattern("Pond.txt");
        }
        static public  List<HashSet<(int, int)>> LoadPattern(string fileName)
        {
            fileName = Path.Combine("StablePatterns", fileName);
            var pattern = Board.Load(fileName);
            var set = new HashSet<(int, int)> { };
            for (int i = 0; i < pattern.Columns; i++)
            {
                for (int j = 0; j < pattern.Rows; j++)
                {
                    if (pattern.Cells[i, j].IsAlive)
                    {
                        set.Add((i, j));
                    }
                }
            }
            var result = new List<HashSet<(int, int)>>();
            var current = set;
            for (int i = 0; i < 4; i++)
            {
                result.Add(current);
                current = Rotate(current);
            }
            var unique = new List<HashSet<(int, int)>>();
            foreach (var variant in result)
            {
                if (!unique.Any(u => u.SetEquals(variant)))
                {
                    unique.Add(variant);
                }
            }
            return unique;
        }
        static private HashSet<(int x, int y)> Rotate(HashSet<(int x, int y)> set)
        {
            var rotated = new HashSet<(int, int)>();
            foreach (var (x, y) in set)
            {
                rotated.Add((y, -x));
            }
            return Normalize(rotated);
        }
        static public HashSet<(int x, int y)> Normalize(HashSet<(int x, int y)> combo)
        {
            int minX = combo.Min(c => c.x);
            int minY = combo.Min(c => c.y);
            var norm = new HashSet<(int, int)>();
            foreach (var (x, y) in combo)
            {
                norm.Add((x - minX, y - minY));
            }
            return norm;
        }
        static public string ClassifyCombinations(HashSet<(int x, int y)> combination)
        {
            var norm = Normalize(combination);
            foreach (var shape in _shapes)
            {
                if (shape.Value.Any(pattern => pattern.SetEquals(norm)))
                {
                    return shape.Key;
                }
            }
            return "Unknown";
        }
        static void StabilityTests()
        {
            double[] densities = { 0.1, 0.2, 0.3, 0.4, 0.5 };
            var results = new List<(double density, double avgGenerations)>();
            using var report = new StreamWriter("experiments_report.txt");
            report.WriteLine("Density\tAvgGenerations");

            foreach (double d in densities)
            {
                Console.WriteLine($"Testing density {d}...");
                var gens = new List<int>();
                for (int i = 0; i < 20; i++)
                {
                    int stableGen = Simulation(d);
                    if (stableGen > 0)
                    {
                        gens.Add(stableGen);
                    }
                }
                double avg = gens.Count > 0 ? gens.Average() : 0;
                results.Add((d, avg));
                report.WriteLine($"{d}\t{avg}");
                Console.WriteLine($"  Average: {avg} generations");
            }

            var plt = new Plot();
            plt.Add.Scatter(results.Select(r => r.density).ToArray(), results.Select(r => r.avgGenerations).ToArray());
            plt.Title("Stable phase");
            plt.XLabel("Density");
            plt.YLabel("Average generations");
            plt.SavePng("plot.png", 800, 600);
        }
        static int Simulation(double density, int stableWindow = 5)
        {
            var simBoard = new Board(_settings.Width, _settings.Height, _settings.CellSize, density);
            var history = new Queue<int>();
            int generation = 0;
            int lastStableGen = -1;

            while (true)
            {
                int alive = simBoard.CountAlive();
                history.Enqueue(alive);
                if (history.Count > stableWindow)
                {
                    history.Dequeue();
                }
                if (history.Count == stableWindow && history.Distinct().Count() == 1)
                {
                    lastStableGen = generation - stableWindow + 1;
                    break;
                }
                simBoard.Advance();
                generation++;
                if (generation > 10000)
                {
                    lastStableGen = -1;
                    break;
                }
            }
            return lastStableGen;
        }

        static void Menu()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.S:
                        _board.Save("Condition.txt");
                        Console.Clear();
                        Console.WriteLine("Condition saved\nPress any button...");
                        Console.ReadKey();
                        break;
                    case ConsoleKey.L:
                        _board = Board.Load("Condition.txt");
                        Console.Clear();
                        Console.WriteLine("Condition loaded\nPress any button...");
                        Console.ReadKey();
                        break;
                    case ConsoleKey.Spacebar:
                        _pauseFlag = !_pauseFlag;
                        break;
                    case ConsoleKey.Tab:
                        Console.Clear();
                        int aliveNumber = _board.CountAlive();
                        int combinationNumber = _board.CountCombinations();
                        Console.WriteLine($"Cells alive - {aliveNumber}\nCombinations - {combinationNumber}");
                        var allCombinations = _board.GetAllCombinations();
                        Console.WriteLine("Classification:");
                        foreach (var combination in allCombinations)
                        {
                            string type = ClassifyCombinations(combination);
                            Console.WriteLine($"  {type} (size {combination.Count})");
                        }
                        Console.WriteLine("Press any button...");
                        Console.ReadKey();
                        break;
                    case ConsoleKey.T:
                        Console.Clear();
                        StabilityTests();
                        Console.WriteLine("Plot saved to mod-lab05-life\\Life\\bin\\Debug\\plot.png\n Data saved to mod-lab05-life\\Life\\bin\\Debug\\experiments_report.txt\nPress any button...");
                        Console.ReadKey();
                        break;
                }
            }
        }
        static void Main(string[] args)
        {
            _settings = Settings.Load();
            InitShapes();
            Reset();
            while (true)
            {
                Menu();
                Console.Clear();
                Render();
                if (_pauseFlag)
                {
                    _board.Advance();
                }
                //Thread.Sleep(settings.Delay);
            }
        }
    }
    public class Settings
    {
        public int Columns { get; set; } = 50;
        public int Rows { get; set; } = 20;
        public int CellSize { get; set; } = 1;
        public int Width => Columns * CellSize;
        public int Height => Rows * CellSize;
        public double LiveDensity { get; set; } = 0.5;
        public int Delay { get; set; } = 500;
        public int MaxGenerations { get; set; } = 500;

        public static Settings Load(string filename = "settings.json")
        {
            if (File.Exists(filename))
            {
                string json = File.ReadAllText(filename);
                Settings settings = JsonSerializer.Deserialize<Settings>(json);
            }
            return new Settings();
        }

        public void Save(string filename = "settings.json")
        {
            if (File.Exists(filename))
            {
                string json = JsonSerializer.Serialize<Settings>(this);
                File.WriteAllText(filename, json);
            }
        }
    }
}