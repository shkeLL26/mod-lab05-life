using cli_life;
using ScottPlot;
using ScottPlot.Palettes;
using System.Runtime;

namespace LifeTests
{
    public class UnitTest1
    {
        [Fact]
        public void Advance_alive_with_one_neighbor()
        {
            var cell = new Cell()
            {
                IsAlive = true,
            };
            cell.neighbors.Add(new Cell() { IsAlive = true });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.False(cell.IsAlive);
        }

        [Fact]
        public void Advance_alive_with_two_neighbor()
        {
            var cell = new Cell()
            {
                IsAlive = true,
            };
            cell.neighbors.Add(new Cell() { IsAlive = true });
            cell.neighbors.Add(new Cell() { IsAlive = true });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.True(cell.IsAlive);
        }

        [Fact]
        public void Advance_alive_with_three_neighbor()
        {
            var cell = new Cell()
            {
                IsAlive = true,
            };
            cell.neighbors.Add(new Cell() { IsAlive = true });
            cell.neighbors.Add(new Cell() { IsAlive = true });
            cell.neighbors.Add(new Cell() { IsAlive = true });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.True(cell.IsAlive);
        }

        [Fact]
        public void Advance_alive_with_four_neighbor()
        {
            var cell = new Cell()
            {
                IsAlive = true,
            };
            cell.neighbors.Add(new Cell() { IsAlive = true });
            cell.neighbors.Add(new Cell() { IsAlive = true });
            cell.neighbors.Add(new Cell() { IsAlive = true });
            cell.neighbors.Add(new Cell() { IsAlive = true });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.False(cell.IsAlive);
        }

        [Fact]
        public void Advance_dead_with_one_neighbor()
        {
            var cell = new Cell()
            {
                IsAlive = false,
            };
            cell.neighbors.Add(new Cell() { IsAlive = true });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.False(cell.IsAlive);
        }

        [Fact]
        public void Advance_dead_with_two_neighbor()
        {
            var cell = new Cell()
            {
                IsAlive = false,
            };
            cell.neighbors.Add(new Cell() { IsAlive = true });
            cell.neighbors.Add(new Cell() { IsAlive = true });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.False(cell.IsAlive);
        }

        [Fact]
        public void Advance_dead_with_three_neighbor()
        {
            var cell = new Cell()
            {
                IsAlive = false,
            };
            cell.neighbors.Add(new Cell() { IsAlive = true });
            cell.neighbors.Add(new Cell() { IsAlive = true });
            cell.neighbors.Add(new Cell() { IsAlive = true });
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.True(cell.IsAlive);
        }

        [Fact]
        public void Board_save_and_load()
        {
            var _settings = Settings.Load();
            var board = new Board(_settings.Width, _settings.Height, _settings.CellSize);
            board.Save("ConditionSaveLoadTest.txt");
            var loadedBoard = Board.Load("ConditionSaveLoadTest.txt");
            for (int row = 0; row < loadedBoard.Rows; row++)
            {
                for (int column = 0; column < loadedBoard.Columns; column++)
                {
                    Assert.Equal(board.Cells[column, row].IsAlive, loadedBoard.Cells[column, row].IsAlive);
                }
            }
        }

        [Fact]
        public void Board_count_alive()
        {
            var board = new Board(2, 2, 1);
            board.Cells[0, 0].IsAlive = true;
            board.Cells[0, 1].IsAlive = true;
            board.Cells[1, 0].IsAlive = false;
            board.Cells[1, 1].IsAlive = true;
            Assert.Equal(3, board.CountAlive());
        }

        [Fact]
        public void Board_count_conditions()
        {
            var board = Board.Load("ConditionTest.txt");
            Assert.Equal(6, board.CountCombinations());
        }

        [Fact]
        public void Load_pattern_boat()
        {
            bool flag = false;
            Dictionary<string, List<HashSet<(int x, int y)>>> _shapes = [];
            _shapes["Boat"] = Program.LoadPattern("Boat.txt");
            var combo = new HashSet<(int x, int y)>()
            {
                (0, 1), (1, 0), (1, 2), (2, 1), (2, 2)
            };
            combo = Program.Normalize(combo);
            foreach (var shape in _shapes)
            {
                if (shape.Value.Any(pattern => pattern.SetEquals(combo)))
                {
                    flag = true;
                    break;
                }
            }
            Assert.True(flag);
        }

        [Fact]
        public void Load_pattern_block()
        {
            bool flag = false;
            Dictionary<string, List<HashSet<(int x, int y)>>> _shapes = [];
            _shapes["Block"] = Program.LoadPattern("Block.txt");
            var combo = new HashSet<(int x, int y)>()
            {
                (0, 0), (0, 1), (1, 0), (1, 1)
            };
            combo = Program.Normalize(combo);
            foreach (var shape in _shapes)
            {
                if (shape.Value.Any(pattern => pattern.SetEquals(combo)))
                {
                    flag = true;
                    break;
                }
            }
            Assert.True(flag);
        }

        [Fact]
        public void Load_pattern_LongHive()
        {
            bool flag = false;
            Dictionary<string, List<HashSet<(int x, int y)>>> _shapes = [];
            _shapes["LongHive"] = Program.LoadPattern("LongHive.txt");
            var combo = new HashSet<(int x, int y)>()
            {
                (0, 1), (0, 2), (1, 0), (1, 3), (2, 1), (2, 2)
            };
            combo = Program.Normalize(combo);
            foreach (var shape in _shapes)
            {
                if (shape.Value.Any(pattern => pattern.SetEquals(combo)))
                {
                    flag = true;
                    break;
                }
            }
            Assert.True(flag);
        }

        [Fact]
        public void Advance_pattern_LongHive()
        {
            var fileName = Path.Combine("StablePatterns", "LongHive.txt");
            var pattern = Board.Load(fileName);
            var advancedPattern = pattern;
            advancedPattern.Advance();
            for (int row = 0; row < advancedPattern.Rows; row++)
            {
                for (int column = 0; column < advancedPattern.Columns; column++)
                {
                    Assert.Equal(pattern.Cells[column, row].IsAlive, advancedPattern.Cells[column, row].IsAlive);
                }
            }
        }
        [Fact]
        public void Advance_pattern_Boat()
        {
            var fileName = Path.Combine("StablePatterns", "Boat.txt");
            var pattern = Board.Load(fileName);
            var advancedPattern = pattern;
            advancedPattern.Advance();
            for (int row = 0; row < advancedPattern.Rows; row++)
            {
                for (int column = 0; column < advancedPattern.Columns; column++)
                {
                    Assert.Equal(pattern.Cells[column, row].IsAlive, advancedPattern.Cells[column, row].IsAlive);
                }
            }
        }

        [Fact]
        public void Advance_pattern_line()
        {
            var cells = new Cell[5, 5]
            {
                { new Cell() { IsAlive = false }, new Cell() { IsAlive = false }, new Cell() { IsAlive = false }, new Cell() { IsAlive = false }, new Cell() { IsAlive = false } },
                { new Cell() { IsAlive = false }, new Cell() { IsAlive = false }, new Cell() { IsAlive = true }, new Cell() { IsAlive = false }, new Cell() { IsAlive = false } },
                { new Cell() { IsAlive = false }, new Cell() { IsAlive = false }, new Cell() { IsAlive = true }, new Cell() { IsAlive = false }, new Cell() { IsAlive = false } },
                { new Cell() { IsAlive = false }, new Cell() { IsAlive = false }, new Cell() { IsAlive = true }, new Cell() { IsAlive = false }, new Cell() { IsAlive = false } },
                { new Cell() { IsAlive = false }, new Cell() { IsAlive = false }, new Cell() { IsAlive = false }, new Cell() { IsAlive = false }, new Cell() { IsAlive = false } },
            };
            var pattern = new Board(1, cells);
            pattern.Advance();
            Assert.False(pattern.Cells[1, 2].IsAlive);
            Assert.True(pattern.Cells[2, 1].IsAlive);
            Assert.True(pattern.Cells[2, 2].IsAlive);
            Assert.True(pattern.Cells[2, 3].IsAlive);
            Assert.False(pattern.Cells[3, 2].IsAlive);
        }
    }
}