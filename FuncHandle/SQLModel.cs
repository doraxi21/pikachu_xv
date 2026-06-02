using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuncHandle
{
    public class SQLModel
    {

        private string connect = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=PikachuDB;Integrated Security=True";
        public bool CheckLogin(string id, string usermame)
        {
            using (SqlConnection con = new SqlConnection(connect)) 
            { 
                con.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE ID = @id AND Username=@user";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@user",usermame);
                    int count=(int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }
        public void savehighscore(string id, string playerName, int score, int level, int timeLeft)
        {
            using (SqlConnection con = new SqlConnection(connect))
            {
                try
                {
                    con.Open();
                   
                    string checkQuery = "SELECT Score FROM HighScores WHERE ID = @id";
                    int currentBestScore = -1;

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@id", id);
                        object result = checkCmd.ExecuteScalar();
                        if (result != null)
                        {
                            currentBestScore = Convert.ToInt32(result);
                        }
                    }

                    if (currentBestScore == -1)
                    {
                        
                        string insertQuery = "INSERT INTO HighScores (ID, PlayerName, Score, Level, PlayDate, TimeLeft) VALUES (@id, @name, @score, @level, @date, @timeLeft)";
                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, con))
                        {
                            insertCmd.Parameters.AddWithValue("@id", id);
                            insertCmd.Parameters.AddWithValue("@name", playerName);
                            insertCmd.Parameters.AddWithValue("@score", score);
                            insertCmd.Parameters.AddWithValue("@level", level);
                            insertCmd.Parameters.AddWithValue("@date", DateTime.Now);
                            insertCmd.Parameters.AddWithValue("@timeLeft", timeLeft);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                    else if (score > currentBestScore)
                    {
                        
                        string updateQuery = "UPDATE HighScores SET Score = @score, Level = @level, PlayDate = @date, TimeLeft = @timeLeft, PlayerName = @name WHERE ID = @id";
                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, con))
                        {
                            updateCmd.Parameters.AddWithValue("@id", id);
                            updateCmd.Parameters.AddWithValue("@name", playerName); 
                            updateCmd.Parameters.AddWithValue("@score", score);
                            updateCmd.Parameters.AddWithValue("@level", level);
                            updateCmd.Parameters.AddWithValue("@date", DateTime.Now);
                            updateCmd.Parameters.AddWithValue("@timeLeft", timeLeft);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi Data: " + ex.Message);
                }
            }
        }
        public void SaveGameData(string id, int score, int timeLeft, int currentStep, string matrixData)
        {
            using (SqlConnection con = new SqlConnection(connect))
            {
                con.Open();

                
                string checkQuery = "SELECT COUNT(*) FROM SaveGame WHERE ID = @id";
                using (SqlCommand cmd = new SqlCommand(checkQuery, con))
                {
                    cmd.Parameters.AddWithValue("@id", id); 
                    int exists = (int)cmd.ExecuteScalar();

                    string query = "";
                    if (exists > 0)
                    {
                        query = "UPDATE SaveGame SET Score=@score, TimeLeft=@time, CurrentStep=@step, MatrixData=@matrix WHERE ID=@id";
                    }
                    else
                    {                 
                        query = "INSERT INTO SaveGame (ID, Score, TimeLeft, CurrentStep, MatrixData) VALUES (@id, @score, @time, @step, @matrix)";
                    }

                    using (SqlCommand cmd1 = new SqlCommand(query, con))
                    {
                        //Gắn tham số
                        cmd1.Parameters.AddWithValue("@id", id);
                        cmd1.Parameters.AddWithValue("@score", score);
                        cmd1.Parameters.AddWithValue("@time", timeLeft);
                        cmd1.Parameters.AddWithValue("@step", currentStep);
                        cmd1.Parameters.AddWithValue("@matrix", matrixData);
                        cmd1.ExecuteNonQuery();
                    }
                }
            }
        }
        public void gethighscore(out int topscore, out int toplevel)
        {
            toplevel = 0;
            topscore = 0;   
            using (SqlConnection con = new SqlConnection(connect))
            {
                con.Open();
                string query = "SELECT TOP 1 Score, Level FROM HighScores ORDER BY Score DESC";
                using (SqlCommand cmd = new SqlCommand (query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader()) 
                        {
                        if (reader.Read())
                        {
                            topscore = Convert.ToInt32(reader["Score"]);
                            toplevel = Convert.ToInt32(reader["Level"]);

                        }
                        }
                }
            }
        }

        public bool CheckUserExist(string id)
        {
            using (SqlConnection con = new SqlConnection(connect))
            {
                con.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE ID=@id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue ("@id", id);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }
        public void RegisterUser(string id, string username)
        {
            using (SqlConnection con = new SqlConnection(connect))
            {
                try
                {
                    con.Open();
                    String query = "INSERT INTO Users (ID,Username,MaxLevel) VALUES (@id,@user,1)";
                    using (SqlCommand cmd= new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi "+ ex.Message);
                }
            }
        }
        public int GetMaxLevel(string id)
        {
            using (SqlConnection con = new SqlConnection(connect))
            {
                con.Open();
                string query = "SELECT MaxLevel FROM Users WHERE ID=@id";
                using (SqlCommand cmd= new SqlCommand (query, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    object result= cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToInt32(result) : 1;
                }
            }
        }
        public void UpdateMaxLevel(string id,int newLevel)
        {
            using (SqlConnection con = new SqlConnection(connect))
            {
                con.Open();
                //chỉ update khi màn hiện tại > màn đã lưu
                string query = "UPDATE Users SET MaxLevel =@newLevel WHERE ID = @id AND MaxLevel<@newLevel";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@newLevel", newLevel);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool HasSaveGame(string id)
        {
            using (SqlConnection con = new SqlConnection(connect))
            {
                con.Open();
                string query = "SELECT COUNT(*) FROM SaveGame WHERE ID=@id ";
                using (SqlCommand cmd= new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    int count=(int)cmd.ExecuteScalar();
                    return count > 0;   
                }
            }
        }
        public bool LoadGameData(string id, out int score, out int timeLeft, out int currentStep, out string matrixData)
        {
            score = 0; timeLeft = 0; currentStep = 0; matrixData = "";
            using (SqlConnection con = new SqlConnection(connect))
            {
                con.Open();
                string query = "SELECT Score, TimeLeft, CurrentStep, MatrixData FROM SaveGame WHERE ID = @id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            score = Convert.ToInt32(reader["Score"]);
                            timeLeft = Convert.ToInt32(reader["TimeLeft"]);
                            currentStep = Convert.ToInt32(reader["CurrentStep"]);
                            matrixData = reader["MatrixData"].ToString();
                            return true; // Tìm thấy file save
                        }
                    }
                }
            }
            return false; // Không tìm thấy
        }
        //reset màn chơi
        public void ResetMaxLevel(string id)
        {
            using (SqlConnection con = new SqlConnection(connect))
            {
                con.Open();
                string query = "UPDATE Users SET MaxLevel = 1 WHERE ID = @id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void DeleteSaveGame(string id)
        {
            using (SqlConnection con = new SqlConnection(connect))
            {
                con.Open();
                string query = "DELETE FROM SaveGame WHERE ID = @id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //lấy data 5 best
        public System.Data.DataTable GetTop5Leaderboard()
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            using (SqlConnection con = new SqlConnection(connect))
            {
                con.Open();
                // Ưu tiên: Level ->Score ->  TimeLeft
                string query = "SELECT TOP 5 PlayerName as 'Tên', Score as 'Điểm', Level as 'Màn', TimeLeft as 'Time' " +
                               "FROM HighScores " +
                               "ORDER BY Level DESC, Score DESC,  TimeLeft DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }   
            }
            return dt;
        }
    }
}
