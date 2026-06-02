using System;
using System.Collections.Generic;
using System.Data.SqlClient; 
using System.Drawing;
using System.IO;


namespace FuncHandle

{
    public class QuestionItem
    {
        public string QuestionText { get; set; }
        public string Answer { get; set; }

        public QuestionItem(string q, string a)
        {
            QuestionText = q;
            Answer = a;
        }
    }
    public class BossModel
    {

        public List<QuestionItem> EasyQuestions = new List<QuestionItem>();
        public List<QuestionItem> HardQuestions = new List<QuestionItem>();
        public void LoadQuestionBank()
        {
            EasyQuestions.Clear();
            HardQuestions.Clear();

            // 4 câu dễ 
            EasyQuestions.Add(new QuestionItem("1 + 1 = ?", "2"));
            EasyQuestions.Add(new QuestionItem("(5 * 4) / 2 + 15 - 12 = ?", "13"));
            EasyQuestions.Add(new QuestionItem("100 + 100 * 0 = ?", "100"));
            EasyQuestions.Add(new QuestionItem("Một năm: ?tháng", "12"));

            Random rng = new Random();
            int n = EasyQuestions.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                QuestionItem value = EasyQuestions[k];
                EasyQuestions[k] = EasyQuestions[n];
                EasyQuestions[n] = value;
            }

            // 4 câu khó 
            HardQuestions.Add(new QuestionItem("Môn đang học", "c#"));
            HardQuestions.Add(new QuestionItem("X = 1206 * 3", "3618"));
            HardQuestions.Add(new QuestionItem("3618", "67"));
            HardQuestions.Add(new QuestionItem("2*10", "20"));
        }
        public void GenerateBossMap(int[,] map, int row, int col)
        {
            for(int r = 0; r < row; r++)
            {
                for(int c = 0; c < col; c++)
                {
                    map[r, c] = -1;
                }
            }

            //ma tran co dinh 4x4 doi xung
            int[,] matrix = new int[,]
            {
               {1,2,3,4 },
               {5,6,7,8},
               {5,6,7,8},
               {1,2,3,4 },
            };

            
            int startRow = (row - 4) / 2;
            int startCol=(col - 4) / 2; 

            for(int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 4; j++)
                {
                    map[startRow + i, startCol + j] = matrix[i,j];
                }
            }
        }

       
    }
}
