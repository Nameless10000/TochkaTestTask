
using System;
using System.Collections.Generic;
using System.Linq;


class Program
{
    // Константы для символов ключей и дверей
    static readonly char[] keys_char = Enumerable.Range('a', 26).Select(i => (char)i).ToArray();
    static readonly char[] doors_char = keys_char.Select(char.ToUpper).ToArray();

    // Метод для чтения входных данных
    static List<List<char>> GetInput()
    {
        var data = new List<List<char>>();
        string line;
        while ((line = Console.ReadLine()) != null && line != "")
        {
            data.Add(line.ToCharArray().ToList());
        }
        return data;
    }


    // Узел в сжатом графе: либо стартовая точка (KeyChar = '\0'), либо ключ 'a'..'z'
    class Node
    {
        public int X, Y;
        public char KeyChar;
    }

    
    class Edge
    {
        public int To;         
        public int Dist;       
        public int ReqKeys;    
    }

    
    class State
    {
        public int[] Positions;  
        public int KeysMask;     
        public int Dist;         
    }

    static int Solve(List<List<char>> grid)
    {
        var dirs = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
        int rowsCount = grid.Count, colsCount = grid[0].Count;

        
        var nodes = new List<Node>();
        var keyToIndex = new Dictionary<char, int>();

        
        for (int i = 0; i < rowsCount; i++)
            for (int j = 0; j < colsCount; j++)
                if (grid[i][j] == '@')
                {
                    nodes.Add(new Node { X = i, Y = j, KeyChar = '\0' });
                    grid[i][j] = '.';  
                }

        for (int i = 0; i < rowsCount; i++)
            for (int j = 0; j < colsCount; j++)
            {
                char ch = grid[i][j];
                if (ch >= 'a' && ch <= 'z')
                {
                    if (!keyToIndex.ContainsKey(ch))
                        keyToIndex[ch] = keyToIndex.Count;
                    nodes.Add(new Node { X = i, Y = j, KeyChar = ch });
                }
            }

        int nodesCount = nodes.Count;
        int K = keyToIndex.Count;
        int allKeysMask = (1 << K) - 1;

        var graph = new List<Edge>[nodesCount];
        for (int nodeIdx = 0; nodeIdx < nodesCount; nodeIdx++)
        {
            graph[nodeIdx] = new List<Edge>();

            var dist = new int[rowsCount, colsCount];
            var reqs = new int[rowsCount, colsCount];
            for (int i = 0; i < rowsCount; i++)
                for (int j = 0; j < colsCount; j++)
                    dist[i, j] = -1;

            var q = new Queue<(int x, int y)>();
            var src = nodes[nodeIdx];
            dist[src.X, src.Y] = 0;
            q.Enqueue((src.X, src.Y));

            
            while (q.Count > 0)
            {
                var (x, y) = q.Dequeue();
                int currDist = dist[x, y], requiredMask = reqs[x, y];

                foreach (var (dx, dy) in dirs)
                {
                    int newX = x + dx, newY = y + dy;
                    if (newX < 0 || newX >= rowsCount || newY < 0 || newY >= colsCount
                        || dist[newX, newY] != -1) 
                        continue;
                    
                    char cell = grid[newX][newY];
                    if (cell == '#') 
                        continue;              

                    int newMask = requiredMask;
                    if (cell >= 'A' && cell <= 'Z')
                    {
                        char keyChar = char.ToLower(cell);
                        if (keyToIndex.TryGetValue(keyChar, out var keyIdx))
                            newMask |= 1 << keyIdx;
                    }

                    dist[newX, newY] = currDist + 1;
                    reqs[newX, newY] = newMask;
                    q.Enqueue((newX, newY));
                }
            }

            for (int anotherNodeIdx = 0; anotherNodeIdx < nodesCount; anotherNodeIdx++)
            {
                if (anotherNodeIdx == nodeIdx) 
                    continue;

                var anotherNode = nodes[anotherNodeIdx];

                if (anotherNode.KeyChar == '\0') 
                    continue;  

                int dx = anotherNode.X, dy = anotherNode.Y;
                if (dist[dx, dy] >= 0)
                {
                    graph[nodeIdx].Add(new Edge
                    {
                        To = anotherNodeIdx,
                        Dist = dist[dx, dy],
                        ReqKeys = reqs[dx, dy]
                    });
                }
            }
        }

        var stateQueue = new PriorityQueue<State, int>();
        var bestPerState = new Dictionary<string, int>();

        var startPositions = Enumerable.Range(0, 4).ToArray();
        var initState = new State
        {
            Positions = startPositions,
            KeysMask = 0,
            Dist = 0
        };

        var startHash = HashState(startPositions, 0);
        bestPerState[startHash] = 0;
        stateQueue.Enqueue(initState, 0);

        while (stateQueue.Count > 0)
        {
            var cur = stateQueue.Dequeue();
            string curHash = HashState(cur.Positions, cur.KeysMask);

            if (cur.Dist > bestPerState[curHash])
                continue;

            if (cur.KeysMask == allKeysMask)
                return cur.Dist;

            for (int r = 0; r < 4; r++)
            {
                int u = cur.Positions[r];
                foreach (var edge in graph[u])
                {
                    char kc = nodes[edge.To].KeyChar;
                    int kid = keyToIndex[kc];

                    if ((cur.KeysMask & (1 << kid)) != 0)
                        continue;

                    if ((edge.ReqKeys & cur.KeysMask) != edge.ReqKeys)
                        continue;

                    var newPos = (int[])cur.Positions.Clone();
                    newPos[r] = edge.To;
                    int newKeys = cur.KeysMask | (1 << kid);
                    int newDist = cur.Dist + edge.Dist;
                    string newHash = HashState(newPos, newKeys);

                    if (!bestPerState.TryGetValue(newHash, out int prevDist) || newDist < prevDist)
                    {
                        bestPerState[newHash] = newDist;
                        stateQueue.Enqueue(new State
                        {
                            Positions = newPos,
                            KeysMask = newKeys,
                            Dist = newDist
                        }, newDist);
                    }
                }
            }
        }

        return -1;
    }

    static string HashState(int[] pos, int keysMask)
        => string.Join(",", pos) + "|" + keysMask;


    static void Main()
    {
        var data = GetInput();
        int result = Solve(data);

        if (result == -1)
        {
            Console.WriteLine("No solution found");
        }
        else
        {
            Console.WriteLine(result);
        }
    }
}