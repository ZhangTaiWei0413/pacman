using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Collections.Generic;

namespace PacmanGame
{
    public partial class MainWindow : Window
    {
        private int pacmanRow = 13;
        private int pacmanCol = 15;

        // 新增玩家移動的定時器
        private DispatcherTimer playerMoveTimer;
        private Key lastKeyPressed = Key.None; // 記錄最後按下的方向鍵

        private int blinkyRow = 7;
        private int blinkyCol = 14;

        private int pinkyRow = 6;
        private int pinkyCol = 17;

        private int inkyRow = 6;
        private int inkyCol = 13;

        private int clydeRow = 7;
        private int clydeCol = 16;

        private DispatcherTimer blinkyTimer;
        private DispatcherTimer pinkyTimer;
        private DispatcherTimer inkyTimer;
        private DispatcherTimer clydeTimer;

        private int dotsCollected = 0; // 已经收集的豆子数量

        private bool[,] walls = new bool[15, 30];  // 15x30 的地图
        private bool[,] dots = new bool[15, 30];  // 追蹤豆子的狀態，true 表示該位置有豆子


        public MainWindow()
        {
            InitializeComponent();
            // 設置窗口最大化
            this.WindowState = WindowState.Maximized;
            InitializeWallsAndDots(); // 初始化墙壁数组

            // 初始化玩家的移動定時器，限制玩家移動速度
            playerMoveTimer = new DispatcherTimer();
            playerMoveTimer.Interval = TimeSpan.FromSeconds(0.5/1.08); // 與鬼魂的速度相同
            playerMoveTimer.Tick += PlayerMoveTimer_Tick;
            playerMoveTimer.Start();

            // 初始化敌人移动定时器
            blinkyTimer = new DispatcherTimer();
            blinkyTimer.Interval = TimeSpan.FromSeconds(0.5); // 每0.5秒移动一次
            blinkyTimer.Tick += BlinkyTimer_Tick;
            blinkyTimer.Start();

            pinkyTimer = new DispatcherTimer();
            pinkyTimer.Interval = TimeSpan.FromSeconds(0.5);
            pinkyTimer.Tick += PinkyTimer_Tick;
            pinkyTimer.Start();

            inkyTimer = new DispatcherTimer();
            inkyTimer.Interval = TimeSpan.FromSeconds(0.5);
            inkyTimer.Tick += InkyTimer_Tick;
            inkyTimer.Start();

            clydeTimer = new DispatcherTimer();
            clydeTimer.Interval = TimeSpan.FromSeconds(0.5);
            clydeTimer.Tick += ClydeTimer_Tick;
            clydeTimer.Start();

            //blinkyTimer.Stop();
            //gameGrid.Children.Remove(blinky);
            //pinkyTimer.Stop();
            //gameGrid.Children.Remove(pinky);
            //inkyTimer.Stop();
           // gameGrid.Children.Remove(inky);
            //clydeTimer.Stop();
           // gameGrid.Children.Remove(clyde);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // 記錄按鍵但不立即移動
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
            {
                lastKeyPressed = e.Key;

                Dispatcher.Invoke(() =>
                {
                    lastKeyText.Text = $"LastKey:\n {lastKeyPressed.ToString()}";
                });
            }
        }

        // 每次定時器觸發時移動 Pac-Man
        private void PlayerMoveTimer_Tick(object sender, EventArgs e)
        {
            switch (lastKeyPressed)
            {
                case Key.Up:
                    if (pacmanRow > 0 && !IsWall(pacmanRow - 1, pacmanCol)) pacmanRow--; // Pac-Man 向上移動
                    break;
                case Key.Down:
                    if (pacmanRow < 14 && !IsWall(pacmanRow + 1, pacmanCol)) pacmanRow++; // Pac-Man 向下移動
                    break;
                case Key.Left:
                    if (pacmanCol > 0 && !IsWall(pacmanRow, pacmanCol - 1)) pacmanCol--; // Pac-Man 向左移動
                    break;
                case Key.Right:
                    if (pacmanCol < 29 && !IsWall(pacmanRow, pacmanCol + 1)) pacmanCol++; // Pac-Man 向右移動
                    break;
            }
            MovePacman();  // 更新
            Dispatcher.Invoke(() =>
            {
                playerPositionText.Text = $"Player:\n({pacmanRow}, {pacmanCol})";
            });
            CheckForDotCollision();  // 檢查是否吃到豆子
            CheckForGameOver();  // 檢查遊戲是否結束
        }

        // 定義 MovePacman，更新 Pac-Man 的位置
        private void MovePacman()
        {
            Grid.SetRow(pacman, pacmanRow); // 將 Pac-Man 移動到新的行
            Grid.SetColumn(pacman, pacmanCol); // 將 Pac-Man 移動到新的列
        }

        // 檢查 Pac-Man 是否吃到豆子
        private void CheckForDotCollision()
        {
            if (dots[pacmanRow, pacmanCol]) // 如果當前格子有豆子
            {
                dots[pacmanRow, pacmanCol] = false;  // 吃掉豆子
                foreach (var child in gameGrid.Children)
                {
                    if (child is TextBlock textBlock && Grid.GetRow(textBlock) == pacmanRow && Grid.GetColumn(textBlock) == pacmanCol && textBlock.Text == "•")
                    {
                        textBlock.Visibility = Visibility.Hidden;  // 隱藏豆子
                        dotsCollected++;
                        break;
                    }
                }
            }

            // 如果所有豆子都被吃掉
            if (dotsCollected == GetTotalDotCount())
            {
                // 停止所有定時器，暫停所有物件
                playerMoveTimer.Stop(); // 停止玩家移動的定時器
                blinkyTimer.Stop();
                pinkyTimer.Stop();
                inkyTimer.Stop();
                clydeTimer.Stop();

                // 禁用玩家輸入
                this.KeyDown -= Window_KeyDown;

                // 顯示勝利訊息
                MessageBoxResult result = MessageBox.Show("You Win!", "Congratulations", MessageBoxButton.OK);

                if (result == MessageBoxResult.OK)
                {
                    Application.Current.Shutdown();  // 關閉應用程序
                }
            }
        }

        private int GetTotalDotCount()
        {
            int total = 0;
            foreach (var child in gameGrid.Children)
            {
                if (child is TextBlock textBlock && textBlock.Text == "•")
                {
                    total++;
                }
            }
            return total;
        }


        // 初始化墙壁和豆子数组
        private void InitializeWallsAndDots()
        {
            foreach (var child in gameGrid.Children)
            {
                if (child is TextBlock textBlock)
                {
                    int row = Grid.GetRow(textBlock);
                    int col = Grid.GetColumn(textBlock);

                    if (textBlock.Text == "█")
                    {
                        walls[row, col] = true;  // 這個位置是牆壁
                    }
                    else if (textBlock.Text == "•")
                    {
                        dots[row, col] = true;   // 這個位置是豆子
                    }
                }
            }
        }

        // 使用二维数组检测是否为墙
        private bool IsWall(int row, int col)
        {
            // 检查是否在数组范围内，并且是否为墙
            return row >= 0 && row < 15 && col >= 0 && col < 30 && walls[row, col];
        }

        // Blinky 直接追踪 Pac-Man 的位置
        private void BlinkyTimer_Tick(object sender, EventArgs e)
        {
            // Blinky 直接追踪 Pac-Man 的位置
            int targetRow = pacmanRow;
            int targetCol = pacmanCol;

            // 使用 BFS 尋找最短路徑
            MoveGhostUsingBFS(ref blinkyRow, ref blinkyCol, targetRow, targetCol);

            Grid.SetRow(blinky, blinkyRow);
            Grid.SetColumn(blinky, blinkyCol);

            Dispatcher.Invoke(() =>
            {
                blinkyTargetText.Text = $"Blinky:\n({targetRow}, {targetCol})";
            }); ;

            CheckForGameOver();
        }

        // Pinky 追踪 Pac-Man 前方4个格子
        private void PinkyTimer_Tick(object sender, EventArgs e)
        {
            // 计算 Pac-Man 前方 4 格的位置
            int targetRow = pacmanRow;
            int targetCol = pacmanCol;

            if (pacmanCol < 26) targetCol += 4;  // Pac-Man 向右移動
            else if (pacmanCol > 0) targetCol -= 4;  // Pac-Man 向左移動

            // 使用 BFS 尋找通往 Pac-Man 預測位置的最短路徑
            MoveGhostUsingBFS(ref pinkyRow, ref pinkyCol, targetRow, targetCol);

            // 確保 Pinky 的行和列值在有效範圍內
            pinkyRow = Math.Max(0, Math.Min(pinkyRow, 14));  // 行的範圍是 0 到 14
            pinkyCol = Math.Max(0, Math.Min(pinkyCol, 29));  // 列的範圍是 0 到 29
            Grid.SetRow(pinky, pinkyRow);
            Grid.SetColumn(pinky, pinkyCol);

            Dispatcher.Invoke(() =>
            {
                pinkyTargetText.Text = $"Pinky:\n ({targetRow}, {targetCol})";
            });

            CheckForGameOver();
        }

        // Inky 基于 Blinky 和 Pac-Man 的位置计算目标点
        // Inky 的追踪逻辑：基于 Blinky 和 Pac-Man 的相对位置
        private void InkyTimer_Tick(object sender, EventArgs e)
        {
            // 计算 Blinky 和 Pac-Man 的相对位置
            int vectorRow = pacmanRow - blinkyRow;
            int vectorCol = pacmanCol - blinkyCol;

            int targetRow = pacmanRow + vectorRow;
            int targetCol = pacmanCol + vectorCol;

            // 确保目标位置在合法范围内
            targetRow = Math.Max(0, Math.Min(targetRow, 14));  // 保证在网格范围内
            targetCol = Math.Max(0, Math.Min(targetCol, 29));

            // 使用 BFS 讓 Inky 找到最短路徑
            MoveGhostUsingBFS(ref inkyRow, ref inkyCol, targetRow, targetCol);

            // 確保 Inky 的行和列值在有效範圍內
            inkyRow = Math.Max(0, Math.Min(inkyRow, 14));  // 行的範圍是 0 到 14
            inkyCol = Math.Max(0, Math.Min(inkyCol, 29));  // 列的範圍是 0 到 29

            // 更新 Inky 位置
            Grid.SetRow(inky, inkyRow);
            Grid.SetColumn(inky, inkyCol);

            Dispatcher.Invoke(() =>
            {
                inkyTargetText.Text = $"Inky:\n ({targetRow}, {targetCol})";
            });

            CheckForGameOver();
        }

        // Clyde 距离 Pac-Man 大于 8 时追踪，否则逃跑
        private void ClydeTimer_Tick(object sender, EventArgs e)
        {
            int distanceToPacman = Math.Abs(pacmanRow - clydeRow) + Math.Abs(pacmanCol - clydeCol);

            int targetRow, targetCol;

            if (distanceToPacman > 8)
            {
                targetRow = pacmanRow;
                targetCol = pacmanCol;
            }
            else
            {
                // 當 Clyde 距離 Pac-Man 小於 8 時，逃跑
                targetRow = (clydeRow < pacmanRow) ? clydeRow - 1 : clydeRow + 1;
                targetCol = (clydeCol < pacmanCol) ? clydeCol - 1 : clydeCol + 1;
            }

            // 使用 BFS 尋找 Clyde 的路徑
            MoveGhostUsingBFS(ref clydeRow, ref clydeCol, targetRow, targetCol);

            Grid.SetRow(clyde, clydeRow);
            Grid.SetColumn(clyde, clydeCol);

            Dispatcher.Invoke(() =>
            {
                clydeTargetText.Text = $"Clyde:\n ({targetRow}, {targetCol})\n (Chasing)";
            });

            CheckForGameOver();
        }

        // 使用 BFS 讓鬼魂找到最短路徑
        // 使用 BFS 讓鬼魂找到最短路徑，但只能水平或垂直移動一格
        // 使用 BFS 讓鬼魂找到最短路徑，但只能水平或垂直移動一格
        private void MoveGhostUsingBFS(ref int ghostRow, ref int ghostCol, int targetRow, int targetCol)
        {
            // BFS 搜索框架，使用队列進行广度优先搜索
            Queue<(int row, int col)> queue = new Queue<(int row, int col)>();
            bool[,] visited = new bool[15, 30];  // 記錄已訪問的節點
            (int row, int col)[,] parent = new (int, int)[15, 30];  // 記錄父節點

            queue.Enqueue((ghostRow, ghostCol));
            visited[ghostRow, ghostCol] = true;
            parent[ghostRow, ghostCol] = (-1, -1); // 根節點沒有父節點

            // BFS 搜索過程
            while (queue.Count > 0)
            {
                var (currentRow, currentCol) = queue.Dequeue();

                // 當找到目標
                if (currentRow == targetRow && currentCol == targetCol)
                {
                    // 回溯找到下一步的位置，確保只能移動一格
                    (int nextRow, int nextCol) = GetNextMove(ghostRow, ghostCol, targetRow, targetCol, parent);

                    // 移動鬼魂
                    ghostRow = nextRow;
                    ghostCol = nextCol;
                    return;
                }

                // 擴展鄰居節點
                EnqueueIfValid(queue, visited, parent, currentRow, currentCol, currentRow + 1, currentCol);  // 向下
                EnqueueIfValid(queue, visited, parent, currentRow, currentCol, currentRow - 1, currentCol);  // 向上
                EnqueueIfValid(queue, visited, parent, currentRow, currentCol, currentRow, currentCol + 1);  // 向右
                EnqueueIfValid(queue, visited, parent, currentRow, currentCol, currentRow, currentCol - 1);  // 向左
            }

            // 如果沒有找到有效路徑，隨機移動
            List<(int row, int col)> possibleMoves = new List<(int row, int col)>();

            if (!IsWall(ghostRow - 1, ghostCol)) possibleMoves.Add((ghostRow - 1, ghostCol)); // 向上
            if (!IsWall(ghostRow + 1, ghostCol)) possibleMoves.Add((ghostRow + 1, ghostCol)); // 向下
            if (!IsWall(ghostRow, ghostCol - 1)) possibleMoves.Add((ghostRow, ghostCol - 1)); // 向左
            if (!IsWall(ghostRow, ghostCol + 1)) possibleMoves.Add((ghostRow, ghostCol + 1)); // 向右

            if (possibleMoves.Count > 0)
            {
                Random rand = new Random();
                var nextMove = possibleMoves[rand.Next(possibleMoves.Count)];
                ghostRow = nextMove.row;
                ghostCol = nextMove.col;
            }
        }

        private bool IsSurroundedByWalls(int row, int col)
        {
            return IsWall(row - 1, col) && IsWall(row + 1, col) && IsWall(row, col - 1) && IsWall(row, col + 1);
        }


        // 設定鬼魂移動一格的回溯邏輯
        private (int nextRow, int nextCol) GetNextMove(int startRow, int startCol, int targetRow, int targetCol, (int, int)[,] parent)
        {
            int row = targetRow;
            int col = targetCol;

            // 回溯從目標到當前位置的路徑，找到下一步要移動的節點
            while (row >= 0 && col >= 0 && parent[row, col] != (startRow, startCol))
            {
                (row, col) = parent[row, col];
            }

            return (row, col);
        }

        // 確保將相鄰的有效節點加入到 BFS 隊列
        private void EnqueueIfValid(Queue<(int row, int col)> queue, bool[,] visited, (int, int)[,] parent, int currentRow, int currentCol, int newRow, int newCol)
        {
            // 確保新行列值在地圖範圍內，且沒有被訪問過且不是牆壁
            if (newRow >= 0 && newRow < 15 && newCol >= 0 && newCol < 30 && !visited[newRow, newCol] && !IsWall(newRow, newCol))
            {
                queue.Enqueue((newRow, newCol));  // 將新的合法節點加入隊列
                visited[newRow, newCol] = true;   // 標記已訪問
                parent[newRow, newCol] = (currentRow, currentCol);  // 記錄父節點
            }
        }

        // 檢查遊戲是否結束
        private void CheckForGameOver()
        {
            if ((blinkyRow == pacmanRow && blinkyCol == pacmanCol) ||
                (pinkyRow == pacmanRow && pinkyCol == pacmanCol) ||
                (inkyRow == pacmanRow && inkyCol == pacmanCol) ||
                (clydeRow == pacmanRow && clydeCol == pacmanCol))
            {
                GameOver();
            }
        }

        private void GameOver()
        {
            // 停止所有定時器，暫停所有物件
            playerMoveTimer.Stop(); // 停止玩家移動的定時器
            blinkyTimer.Stop();
            pinkyTimer.Stop();
            inkyTimer.Stop();
            clydeTimer.Stop();

            // 禁用玩家輸入
            this.KeyDown -= Window_KeyDown;

            // 顯示遊戲結束訊息
            MessageBox.Show("Game Over! You were caught by a ghost.");

            // 關閉遊戲
            if (Application.Current != null)
            {
                Application.Current.Shutdown();
            }
        }


        private void ResetGame()
        {
            // 重置 Pac-Man 的位置
            Grid.SetRow(pacman, pacmanRow);
            Grid.SetColumn(pacman, pacmanCol);

            // 重置 Blinky 的位置
            blinkyRow = 14;
            blinkyCol = 14;
            Grid.SetRow(blinky, blinkyRow);
            Grid.SetColumn(blinky, blinkyCol);

            // 重置 Pinky 的位置
            pinkyRow = 14;
            pinkyCol = 0;
            Grid.SetRow(pinky, pinkyRow);
            Grid.SetColumn(pinky, pinkyCol);

            // 重置 Inky 的位置
            inkyRow = 0;
            inkyCol = 14;
            Grid.SetRow(inky, inkyRow);
            Grid.SetColumn(inky, inkyCol);

            // 重置 Clyde 的位置
            clydeRow = 7;
            clydeCol = 7;
            Grid.SetRow(clyde, clydeRow);
            Grid.SetColumn(clyde, clydeCol);

            // 重新启动鬼魂的定时器
            blinkyTimer.Start();
            pinkyTimer.Start();
            inkyTimer.Start();
            clydeTimer.Start();
        }
    }
}


