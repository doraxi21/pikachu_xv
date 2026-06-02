using System.Collections.Generic;
using System.Drawing;

namespace FuncHandle
{
    public static class FindPath
    {
        public static List<Point> FindingPath(int[,] matrix, Point p1, Point p2)
        {
            int[] dx = { -1, 0, 1, 0 };
            int[] dy = { 0, 1, 0, -1 };

            Dictionary<Point, Point> trace = new Dictionary<Point, Point>();
            Queue<Point> q = new Queue<Point>();
            q.Enqueue(p1);

            while (q.Count > 0)
            {
                Point cur = q.Dequeue();
                if (cur == p2) break;

                for (int i = 0; i < 4; i++)
                {
                    int nx = cur.X + dx[i];
                    int ny = cur.Y + dy[i];

                    while (nx >= 0 && nx < Config.Rows && ny >= 0 && ny < Config.Cols && 
                          (matrix[nx, ny] == -1 || (nx == p2.X && ny == p2.Y)))
                    {
                        Point next = new Point(nx, ny);
                        if (!trace.ContainsKey(next))
                        {
                            trace[next] = cur;
                            q.Enqueue(next);
                        }
                        nx += dx[i];
                        ny += dy[i];
                    }
                }
            }
            return GetPathTrace(trace, p2);
        }

        private static List<Point> GetPathTrace(Dictionary<Point, Point> trace, Point target)
        {
            List<Point> path = new List<Point>();
            if (!trace.ContainsKey(target)) return path;

            Point cur = target;
            while (true)
            {
                path.Add(cur);
                if (trace.ContainsKey(cur)) cur = trace[cur];
                else break;
            }
            path.Reverse();
            return path;
        }
    }
}