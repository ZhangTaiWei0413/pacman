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
        private int pacmanRow = 13;  // 玩家初始行
        private int pacmanCol = 15;  // 玩家初始列

        private DispatcherTimer playerMoveTimer;  // 玩家移動定時器
        private Key lastKeyPressed = Key.None;  // 最後按下的方向鍵

        private int blinkyRow = 7;  // Blinky 初始行
        private int blinkyCol = 14;  // Blinky 初始列

        private int pinkyRow = 6;  // Pinky 初始行
        private int pinkyCol = 17;  // Pinky 初始列

        private int inkyRow = 6;  // Inky 初始行
        private int inkyCol = 13;  // Inky 初始列

        private int clydeRow = 7;  // Clyde 初始行
        private int clydeCol = 16;  // Clyde 初始列

        private DispatcherTimer blinkyTimer;  // Blinky 移動定時器
        private DispatcherTimer pinkyTimer;  // Pinky 移動定時器
        private DispatcherTimer inkyTimer;  // Inky 移動定時器
        private DispatcherTimer clydeTimer;  // Clyde 移動定時器

        private int dotsCollected = 0;  // 收集的豆子數量

        private bool[,] walls = new bool[15, 30];  // 墙壁二维数组
        private bool[,] dots = new bool[15, 30];  // 豆子二维数组

        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;  // 最大化窗口
            InitializeWallsAndDots();  // 初始化牆壁和豆子

            playerMoveTimer = new DispatcherTimer();  // 初始化玩家移動定時器
            playerMoveTimer.Interval = TimeSpan.FromSeconds(0.5 / 1.2);  // 設置移動速度
            playerMoveTimer.Tick += PlayerMoveTimer_Tick;  // 定時器觸發玩家移動
            playerMoveTimer.Start();

            blinkyTimer = new DispatcherTimer();  // 初始化 Blinky 移動定時器
            blinkyTimer.Interval = TimeSpan.FromSeconds(0.5);  // 設置移動速度
            blinkyTimer.Tick += BlinkyTimer_Tick;  // 定時器觸發 Blinky 移動
            blinkyTimer.Start();

            pinkyTimer = new DispatcherTimer();  // 初始化 Pinky 移動定時器
            pinkyTimer.Interval = TimeSpan.FromSeconds(0.5);  // 設置移動速度
            pinkyTimer.Tick += PinkyTimer_Tick;  // 定時器觸發 Pinky 移動
            pinkyTimer.Start();

            inkyTimer = new DispatcherTimer();  // 初始化 Inky 移動定時器
            inkyTimer.Interval = TimeSpan.FromSeconds(0.5);  // 設置移動速度
            inkyTimer.Tick += InkyTimer_Tick;  // 定時器觸發 Inky 移動
            inkyTimer.Start();

            clydeTimer = new DispatcherTimer();  // 初始化 Clyde 移動定時器
            clydeTimer.Interval = TimeSpan.FromSeconds(0.5);  // 設置移動速度
            clydeTimer.Tick += ClydeTimer_Tick;  // 定時器觸發 Clyde 移動
            clydeTimer.Start();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
            {
                lastKeyPressed = e.Key;  // 記錄最後按下的鍵

                Dispatcher.Invoke(() =>
                {
                    lastKeyText.Text = $"LastKey:\n {lastKeyPressed.ToString()}";  // 顯示按鍵狀態
                });
            }
        }

        private void PlayerMoveTimer_Tick(object sender, EventArgs e)
        {
            switch (lastKeyPressed)
            {
                case Key.Up:
                    if (pacmanRow > 0 && !IsWall(pacmanRow - 1, pacmanCol)) pacmanRow--;  // 向上移動
                    break;
                case Key.Down:
                    if (pacmanRow < 14 && !IsWall(pacmanRow + 1, pacmanCol)) pacmanRow++;  // 向下移動
                    break;
                case Key.Left:
                    if (pacmanCol > 0 && !IsWall(pacmanRow, pacmanCol - 1)) pacmanCol--;  // 向左移動
                    break;
                case Key.Right:
                    if (pacmanCol < 29 && !IsWall(pacmanRow, pacmanCol + 1)) pacmanCol++;  // 向右移動
                    break;
            }
            MovePacman();  // 更新位置
            Dispatcher.Invoke(() =>
            {
                playerPositionText.Text = $"Player:\n({pacmanRow}, {pacmanCol})";  // 顯示位置
            });
            CheckForDotCollision();  // 檢查是否吃豆子
            CheckForGameOver();  // 檢查遊戲是否結束
        }

        private void MovePacman()
        {
            Grid.SetRow(pacman, pacmanRow);  // 更新 Pac-Man 行位置
            Grid.SetColumn(pacman, pacmanCol);  // 更新 Pac-Man 列位置
        }

        private void CheckForDotCollision()
        {
            if (dots[pacmanRow, pacmanCol])
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

            if (dotsCollected == GetTotalDotCount())  // 如果豆子收集完
            {
                playerMoveTimer.Stop();  // 停止所有定時器
                blinkyTimer.Stop();
                pinkyTimer.Stop();
                inkyTimer.Stop();
                clydeTimer.Stop();

                this.KeyDown -= Window_KeyDown;  // 禁用鍵盤輸入

                MessageBoxResult result = MessageBox.Show("You Win!", "Congratulations", MessageBoxButton.OK);  // 顯示勝利訊息

                if (result == MessageBoxResult.OK)
                {
                    Application.Current.Shutdown();  // 關閉應用
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
                    total++;  // 計算總豆子數
                }
            }
            return total;
        }

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
                        walls[row, col] = true;  // 標記牆壁位置
                    }
                    else if (textBlock.Text == "•")
                    {
                        dots[row, col] = true;  // 標記豆子位置
                    }
                }
            }
        }

        private bool IsWall(int row, int col)
        {
            return row >= 0 && row < 15 && col >= 0 && col < 30 && walls[row, col];  // 檢查是否為牆
        }

        private void BlinkyTimer_Tick(object sender, EventArgs e)
        {
            int targetRow = pacmanRow;  // Blinky 的目標是 Pac-Man 的位置
            int targetCol = pacmanCol;

            (int nextRow, int nextCol) = MoveGhostUsingBFS(blinkyRow, blinkyCol, targetRow, targetCol);  // 使用 BFS 移動

            blinkyRow = nextRow;
            blinkyCol = nextCol;

            Grid.SetRow(blinky, blinkyRow);  // 更新 Blinky 的位置
            Grid.SetColumn(blinky, blinkyCol);

            Dispatcher.Invoke(() =>
            {
                blinkyTargetText.Text = $"Blinky:\n({targetRow}, {targetCol})";  // 更新目標位置
            });

            CheckForGameOver();  // 檢查遊戲是否結束
        }
        private void PinkyTimer_Tick(object sender, EventArgs e)
        {
            int targetRow = pacmanRow;
            int targetCol = pacmanCol;

            int maxDistance = 4;  // Pinky 追踪 Pac-Man 前方 4 格

            switch (lastKeyPressed)
            {
                case Key.Up:
                    for (int distance = maxDistance; distance >= 0; distance--)
                    {
                        if (targetRow - distance >= 0 && !IsWall(targetRow - distance, targetCol))
                        {
                            targetRow -= distance;
                            break;
                        }
                    }
                    break;
                case Key.Down:
                    for (int distance = maxDistance; distance >= 0; distance--)
                    {
                        if (targetRow + distance < 15 && !IsWall(targetRow + distance, targetCol))
                        {
                            targetRow += distance;
                            break;
                        }
                    }
                    break;
                case Key.Left:
                    for (int distance = maxDistance; distance >= 0; distance--)
                    {
                        if (targetCol - distance >= 0 && !IsWall(targetRow, targetCol - distance))
                        {
                            targetCol -= distance;
                            break;
                        }
                    }
                    break;
                case Key.Right:
                    for (int distance = maxDistance; distance >= 0; distance--)
                    {
                        if (targetCol + distance < 30 && !IsWall(targetRow, targetCol + distance))
                        {
                            targetCol += distance;
                            break;
                        }
                    }
                    break;
            }

            (int nextRow, int nextCol) = MoveGhostUsingBFS(pinkyRow, pinkyCol, targetRow, targetCol);

            pinkyRow = Math.Max(0, Math.Min(nextRow, 14));  // 确保行范围正确
            pinkyCol = Math.Max(0, Math.Min(nextCol, 29));  // 确保列范围正确

            Grid.SetRow(pinky, pinkyRow);
            Grid.SetColumn(pinky, pinkyCol);

            Dispatcher.Invoke(() =>
            {
                pinkyTargetText.Text = $"Pinky:\n ({targetRow}, {targetCol})";
            });

            CheckForGameOver();
        }

        private void InkyTimer_Tick(object sender, EventArgs e)
        {
            int vectorRow = pacmanRow - blinkyRow;
            int vectorCol = pacmanCol - blinkyCol;

            int targetRow = pacmanRow + vectorRow;  // Inky 的目標是基於 Blinky 和 Pac-Man 的位置
            int targetCol = pacmanCol + vectorCol;

            targetRow = Math.Max(0, Math.Min(targetRow, 14));  // 限制行範圍
            targetCol = Math.Max(0, Math.Min(targetCol, 29));  // 限制列範圍

            (int nextRow, int nextCol) = MoveGhostUsingBFS(inkyRow, inkyCol, targetRow, targetCol);  // 使用 BFS 移動

            inkyRow = Math.Max(0, Math.Min(nextRow, 14));  // 更新 Inky 的行
            inkyCol = Math.Max(0, Math.Min(nextCol, 29));  // 更新 Inky 的列

            Grid.SetRow(inky, inkyRow);
            Grid.SetColumn(inky, inkyCol);

            Dispatcher.Invoke(() =>
            {
                inkyTargetText.Text = $"Inky:\n ({targetRow}, {targetCol})";  // 更新目標位置
            });

            CheckForGameOver();  // 檢查遊戲是否結束
        }

        private void ClydeTimer_Tick(object sender, EventArgs e)
        {
            int distanceToPacman = Math.Abs(pacmanRow - clydeRow) + Math.Abs(pacmanCol - clydeCol);  // Clyde 與 Pac-Man 的距離

            int targetRow = clydeRow;
            int targetCol = clydeCol;

            if (distanceToPacman <= 8)  // 若距離 <= 8，Clyde 逃跑
            {
                List<(int row, int col)> possibleMoves = new List<(int row, int col)>();  // 記錄可能移動位置

                if (clydeRow > 0 && !IsWall(clydeRow - 1, clydeCol)) possibleMoves.Add((clydeRow - 1, clydeCol));  // 向上移動
                if (clydeRow < 14 && !IsWall(clydeRow + 1, clydeCol)) possibleMoves.Add((clydeRow + 1, clydeCol));  // 向下移動
                if (clydeCol > 0 && !IsWall(clydeRow, clydeCol - 1)) possibleMoves.Add((clydeRow, clydeCol - 1));  // 向左移動
                if (clydeCol < 29 && !IsWall(clydeRow, clydeCol + 1)) possibleMoves.Add((clydeRow, clydeCol + 1));  // 向右移動

                if (possibleMoves.Count > 0)
                {
                    var bestMove = possibleMoves[0];
                    int maxDistance = Math.Abs(pacmanRow - bestMove.row) + Math.Abs(pacmanCol - bestMove.col);  // 計算最遠的移動

                    foreach (var move in possibleMoves)
                    {
                        int newDistance = Math.Abs(pacmanRow - move.row) + Math.Abs(pacmanCol - move.col);
                        if (newDistance > maxDistance)
                        {
                            bestMove = move;
                            maxDistance = newDistance;
                        }
                    }

                    targetRow = bestMove.row;
                    targetCol = bestMove.col;
                }
            }
            else  // Clyde 距離 > 8 時，開始追蹤 Pac-Man
            {
                targetRow = pacmanRow;
                targetCol = pacmanCol;
            }

            (int nextRow, int nextCol) = MoveGhostUsingBFS(clydeRow, clydeCol, targetRow, targetCol);  // 使用 BFS 移動

            clydeRow = Math.Max(0, Math.Min(nextRow, 14));  // 更新 Clyde 的行
            clydeCol = Math.Max(0, Math.Min(nextCol, 29));  // 更新 Clyde 的列

            Grid.SetRow(clyde, clydeRow);  // 更新 Clyde 位置
            Grid.SetColumn(clyde, clydeCol);

            Dispatcher.Invoke(() =>
            {
                clydeTargetText.Text = $"Clyde:\n ({targetRow}, {targetCol})\n {(distanceToPacman <= 8 ? "(Fleeing)" : "(Chasing)")}";  // 更新目標位置
            });

            CheckForGameOver();  // 檢查遊戲是否結束
        }
        private (int, int) MoveGhostUsingBFS(int ghostRow, int ghostCol, int targetRow, int targetCol)
        {
            Queue<(int row, int col)> queue = new Queue<(int row, int col)>();
            bool[,] visited = new bool[15, 30];
            (int row, int col)[,] parent = new (int, int)[15, 30];

            queue.Enqueue((ghostRow, ghostCol));
            visited[ghostRow, ghostCol] = true;
            parent[ghostRow, ghostCol] = (-1, -1);

            while (queue.Count > 0)
            {
                var (currentRow, currentCol) = queue.Dequeue();

                // 當找到目標位置時，回溯找到下一步移動位置
                if (currentRow == targetRow && currentCol == targetCol)
                {
                    return GetNextMove(ghostRow, ghostCol, targetRow, targetCol, parent);
                }

                // 擴展鄰居節點，加入合法的移動
                EnqueueIfValid(queue, visited, parent, currentRow, currentCol, currentRow + 1, currentCol);  // 向下
                EnqueueIfValid(queue, visited, parent, currentRow, currentCol, currentRow - 1, currentCol);  // 向上
                EnqueueIfValid(queue, visited, parent, currentRow, currentCol, currentRow, currentCol + 1);  // 向右
                EnqueueIfValid(queue, visited, parent, currentRow, currentCol, currentRow, currentCol - 1);  // 向左
            }

            // 如果沒有找到有效路徑，隨機選擇下一步或保持原地不動
            List<(int row, int col)> possibleMoves = new List<(int row, int col)>();

            if (!IsWall(ghostRow - 1, ghostCol)) possibleMoves.Add((ghostRow - 1, ghostCol)); // 向上
            if (!IsWall(ghostRow + 1, ghostCol)) possibleMoves.Add((ghostRow + 1, ghostCol)); // 向下
            if (!IsWall(ghostRow, ghostCol - 1)) possibleMoves.Add((ghostRow, ghostCol - 1)); // 向左
            if (!IsWall(ghostRow, ghostCol + 1)) possibleMoves.Add((ghostRow, ghostCol + 1)); // 向右

            if (possibleMoves.Count > 0)
            {
                Random rand = new Random();
                var nextMove = possibleMoves[rand.Next(possibleMoves.Count)];
                return (nextMove.row, nextMove.col);
            }

            // 保持原地不動
            return (ghostRow, ghostCol);
        }

        private bool IsSurroundedByWalls(int row, int col)
        {
            return IsWall(row - 1, col) && IsWall(row + 1, col) && IsWall(row, col - 1) && IsWall(row, col + 1);  // 檢查是否被牆包圍
        }
        private (int nextRow, int nextCol) GetNextMove(int startRow, int startCol, int targetRow, int targetCol, (int, int)[,] parent)
        {
            int row = targetRow;
            int col = targetCol;

            // 如果目標點在父節點表中沒有回溯路徑，則返回當前鬼魂的位置
            if (parent[row, col] == (-1, -1))  // 無法找到父節點
            {
                return (startRow, startCol);  // 保持原地不動
            }

            // 回溯從目標到當前位置的路徑，找到下一步要移動的節點
            while (parent[row, col] != (startRow, startCol))
            {
                (row, col) = parent[row, col];
            }

            return (row, col);
        }

        private void EnqueueIfValid(Queue<(int row, int col)> queue, bool[,] visited, (int, int)[,] parent, int currentRow, int currentCol, int newRow, int newCol)
        {
            if (newRow >= 0 && newRow < 15 && newCol >= 0 && newCol < 30 && !visited[newRow, newCol] && !IsWall(newRow, newCol))  // 檢查是否為合法移動
            {
                queue.Enqueue((newRow, newCol));  // 加入新節點
                visited[newRow, newCol] = true;  // 標記訪問
                parent[newRow, newCol] = (currentRow, currentCol);  // 記錄父節點
            }
        }

        private void CheckForGameOver()
        {
            if ((blinkyRow == pacmanRow && blinkyCol == pacmanCol) ||
                (pinkyRow == pacmanRow && pinkyCol == pacmanCol) ||
                (inkyRow == pacmanRow && inkyCol == pacmanCol) ||
                (clydeRow == pacmanRow && clydeCol == pacmanCol))  // 檢查是否碰到鬼魂
            {
                GameOver();  // 結束遊戲
            }
        }

        private void GameOver()
        {
            playerMoveTimer.Stop();  // 停止所有定時器
            blinkyTimer.Stop();
            pinkyTimer.Stop();
            inkyTimer.Stop();
            clydeTimer.Stop();

            this.KeyDown -= Window_KeyDown;  // 禁用玩家輸入

            MessageBox.Show("Game Over! You were caught by a ghost.");  // 顯示遊戲結束訊息

            if (Application.Current != null)
            {
                Application.Current.Shutdown();  // 關閉遊戲
            }
        }

        private void ResetGame()
        {
            Grid.SetRow(pacman, pacmanRow);  // 重置 Pac-Man 位置
            Grid.SetColumn(pacman, pacmanCol);

            blinkyRow = 14;  // 重置 Blinky 位置
            blinkyCol = 14;
            Grid.SetRow(blinky, blinkyRow);
            Grid.SetColumn(blinky, blinkyCol);

            pinkyRow = 14;  // 重置 Pinky 位置
            pinkyCol = 0;
            Grid.SetRow(pinky, pinkyRow);
            Grid.SetColumn(pinky, pinkyCol);

            inkyRow = 0;  // 重置 Inky 位置
            inkyCol = 14;
            Grid.SetRow(inky, inkyRow);
            Grid.SetColumn(inky, inkyCol);

            clydeRow = 7;  // 重置 Clyde 位置
            clydeCol = 7;
            Grid.SetRow(clyde, clydeRow);
            Grid.SetColumn(clyde, clydeCol);

            blinkyTimer.Start();  // 重啟鬼魂定時器
            pinkyTimer.Start();
            inkyTimer.Start();
            clydeTimer.Start();
        }
    }
}
