using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;

namespace FuncHandle
{
    public class Model
    {
        private string connectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=PikachuDB;Integrated Security=True";

        public int[,] matrix;
        private Random rand = new Random();

        // cache ảnh
        public Dictionary<int, Image> PokemonImagesCache = new Dictionary<int, Image>();

        public Model()
        {
            matrix = new int[Config.Rows, Config.Cols];
            ResetMatrix();
        }

        // image
        public void LoadAllImagesToCache(string startupPath)
        {
            PokemonImagesCache.Clear();
            for (int i = 1; i <= 25; i++)
            {
                PokemonImagesCache[i] = GetSafeImage(i, startupPath);
            }
        }

        private Image GetSafeImage(int pokemonId, string startupPath)
        {
            try
            {
                string fileName;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT ImagePath FROM PokemonData WHERE Id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", pokemonId);

                    var result = cmd.ExecuteScalar();
                    fileName = result != null ? result.ToString() : $"pieces{pokemonId}.png";
                }

                string path = Path.Combine(startupPath, fileName);

                if (!File.Exists(path)) return null;

                byte[] bytes = File.ReadAllBytes(path);
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    return Image.FromStream(ms);
                }
            }
            catch
            {
                return null;
            }
        }

        // tạo ma trận pikachu
        public void ResetMatrix()
        {
            for (int i = 0; i < Config.Rows; i++)
                for (int j = 0; j < Config.Cols; j++)
                    matrix[i, j] = -1;
        }

        public void GenerateMap(int numTypes)
        {
            ResetMatrix();

            int playRows = Config.Rows - 2;
            int playCols = Config.Cols - 2;
            int total = playRows * playCols;

            List<int> data = new List<int>(total);

            for (int i = 0; i < total / 2; i++)
            {
                int id = (i % numTypes) + 1;
                data.Add(id);
                data.Add(id);
            }
            //xóc lên
            Shuffle(data);

            int k = 0;
            for (int r = 1; r <= playRows; r++)
                for (int c = 1; c <= playCols; c++)
                    matrix[r, c] = data[k++];
        }

        //đảo vị trí
        private void Shuffle(List<int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public void RefMatrix()
        {
            List<int> temp = new List<int>();

            for (int r = 1; r <= Config.Rows - 2; r++)
            {
                for (int c = 1; c <= Config.Cols - 2; c++)
                {
                    if (matrix[r, c] != -1)
                        temp.Add(matrix[r, c]);
                }
            }

            Shuffle(temp);

            int index = 0;
            for (int r = 1; r <= Config.Rows - 2; r++)
            {
                for (int c = 1; c <= Config.Cols - 2; c++)
                {
                    if (matrix[r, c] != -1)
                        matrix[r, c] = temp[index++];
                }
            }
        }

        // độ khó 
        public void shiftUp()
        {
            for (int c = 1; c <= Config.Cols - 2; c++)
            {
                int insert = 1;
                for (int r = 1; r <= Config.Rows - 2; r++)
                {
                    if (matrix[r, c] != -1)
                    {
                        if (r != insert)
                        {
                            matrix[insert, c] = matrix[r, c];
                            matrix[r, c] = -1;
                        }
                        insert++;
                    }
                }
            }
        }

        public void shiftDown()
        {
            for (int c = 1; c <= Config.Cols - 2; c++)
            {
                int insert = Config.Rows - 2;
                for (int r = Config.Rows - 2; r >= 1; r--)
                {
                    if (matrix[r, c] != -1)
                    {
                        if (r != insert)
                        {
                            matrix[insert, c] = matrix[r, c];
                            matrix[r, c] = -1;
                        }
                        insert--;
                    }
                }
            }
        }

        public void shiftLeft()
        {
            for (int r = 1; r <= Config.Rows - 2; r++)
            {
                int insert = 1;
                for (int c = 1; c <= Config.Cols - 2; c++)
                {
                    if (matrix[r, c] != -1)
                    {
                        if (c != insert)
                        {
                            matrix[r, insert] = matrix[r, c];
                            matrix[r, c] = -1;
                        }
                        insert++;
                    }
                }
            }
        }

        public void shiftRight()
        {
            for (int r = 1; r <= Config.Rows - 2; r++)
            {
                int insert = Config.Cols - 2;
                for (int c = Config.Cols - 2; c >= 1; c--)
                {
                    if (matrix[r, c] != -1)
                    {
                        if (c != insert)
                        {
                            matrix[r, insert] = matrix[r, c];
                            matrix[r, c] = -1;
                        }
                        insert--;
                    }
                }
            }
        }

        // save - load
        public string matrixToString()
        {
            List<string> list = new List<string>();

            for (int r = 1; r <= Config.Rows - 2; r++)
                for (int c = 1; c <= Config.Cols - 2; c++)
                    list.Add(matrix[r, c].ToString());

            return string.Join(",", list);
        }

        public void stringToMatrix(string data)
        {
            string[] arr = data.Split(',');
            int index = 0;

            for (int r = 1; r <= Config.Rows - 2; r++)
            {
                for (int c = 1; c <= Config.Cols - 2; c++)
                {
                    if (index < arr.Length)
                        matrix[r, c] = int.Parse(arr[index++]);
                }
            }
        }

        //kiểm tra còn đường đi hợp lệ ko

        public bool HasValid()
        {
            Dictionary<int, List<Point>> map = new Dictionary<int, List<Point>>();

            // gom theo loại
            for (int r = 1; r <= Config.Rows - 2; r++)
            {
                for (int c = 1; c <= Config.Cols - 2; c++)
                {
                    int val = matrix[r, c];
                    if (val == -1) continue;

                    if (!map.ContainsKey(val))
                        map[val] = new List<Point>();

                    map[val].Add(new Point(r, c));
                }
            }

            // check cùng loại
            foreach (var pair in map)
            {
                var list = pair.Value;

                for (int i = 0; i < list.Count; i++)
                {
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        var path = FindPath.FindingPath(matrix, list[i], list[j]);
                        if (path.Count >= 2 && path.Count <= 4)
                            return true;
                    }
                }
            }

            return false;
        }
    }
}