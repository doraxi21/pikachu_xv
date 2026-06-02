using FuncHandle;
using System.Data.SqlTypes;
using System.Threading.Tasks;

namespace UI
{
    public partial class Form1 : Form
    {
        private Model gameModel = new Model();  //tạo khung game theo func Model.cs
        private PictureBox firstClickPic = null;   //nhớ ô chọn đầu tiên
        private Point firstClickPos;              // và tọa độ

        private List<Point> currentPath = new List<Point>();
        private System.Windows.Forms.Timer lineTimer = new System.Windows.Forms.Timer();

        private int currentStep = 0; //step back

        private int score = 0;
        private int timeLeft = 0;
        private int demRemain = 0; //đếm số cặp còn lại để bt hết hay chauw
        private System.Windows.Forms.Timer countdownTimer = new System.Windows.Forms.Timer();

        private int remake = 10;  //cho phep refresh 10 lan
        private int tileSize = 45;
        private int margin = 0;
        private int offsetX = 90;
        private int offsetY = 50;
        //phase 6
        private BossModel bM = new BossModel();
        private int blevel = 1;

        private bool isBossPhase = false;  //phan biet man thuong man boss
        private PictureBox secondBossPic = null; // nho o de tu bat cap
        private Point secondBossPos;
        private QuestionItem currentBossQuestion = null;
        private List<Image> winImageList = new List<Image>();
        private List<Image> loseImageList = new List<Image>();
        private Random rd = new Random();
        private SQLModel sqlmodel = new SQLModel();


        private string currentPlayer = "";
        private string currentID = "";

        //gallery
        private List<Image> galleryList = new List<Image>();
        private int currentGalleryIndex = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mayPhatNhac.PlayLooping();
            gameModel.LoadAllImagesToCache(Application.StartupPath + "\\Data\\Image");
            lineTimer.Interval = 100;
            lineTimer.Tick += LineTimer_Tick;
            //setting dong ho
            countdownTimer.Interval = 1000; // 1s nhay 1 lan
            countdownTimer.Tick += CoutdownTimer_Tick;

            //load pic phase6
            winImageList.Add(Properties.Resources.true1);
            winImageList.Add(Properties.Resources.true2);
            winImageList.Add(Properties.Resources.Goodjob);
            loseImageList.Add(Properties.Resources.non);
            loseImageList.Add(Properties.Resources.wrong3);
            loseImageList.Add((Properties.Resources.wrong4));
            loseImageList.Add(Properties.Resources.wrong5);

            //lưu ảnh vô túi to
            galleryList.AddRange(winImageList);
            galleryList.AddRange(loseImageList);
            //Before login
            panelMenu.Visible = false;
            panelBoard.Visible = false;
            registerPanel.Visible = false;
            panellogin.Visible = true;
            panellogin.BringToFront();
            backBut.BringToFront();
        }

        private async void CoutdownTimer_Tick(object sender, EventArgs e)
        {
            timeLeft--;
            lbTime.Text = ": " + timeLeft;
            if (isLanMode)
            {
                string packet = $"SYNC_STATS|{currentPlayer}|{score}|{timeLeft}|{demRemain}";

                if (isHost && lanServer != null)
                    lanServer.BroadcastMessage(packet);
                else if (!isHost && lanClient != null)
                    lanClient.SendMessage(packet);
            }
            if (timeLeft <= 0)
            {
                countdownTimer.Stop();

                if (isBossPhase && panelQuizi.Visible)
                {

                    panelQuizi.Visible = false;
                    lose_meme.Image = Properties.Resources.non; // hiện ảnh thua
                    lose_meme.Visible = true;
                    lose_meme.BringToFront();
                    // khôi phục 2 Pokemon 
                    if (firstClickPic != null) firstClickPic.BorderStyle = BorderStyle.FixedSingle;
                    if (secondBossPic != null)
                    {
                        secondBossPic.BorderStyle = BorderStyle.FixedSingle;
                        secondBossPic.BackColor = Color.Transparent;
                    }
                    firstClickPic = null;
                    secondBossPic = null;
                    panelBoard.Enabled = true;

                    System.Windows.Forms.Timer hideLoseTimer = new System.Windows.Forms.Timer();
                    hideLoseTimer.Interval = 2000;
                    hideLoseTimer.Tick += (s, args) =>
                    {
                        lose_meme.Visible = false;
                        hideLoseTimer.Stop();
                        hideLoseTimer.Dispose();
                    };
                    hideLoseTimer.Start();
                }
                else
                {
                    panelBoard.Enabled = false;
                    Lose.Visible = true;
                }
            }
        }
        private void LineTimer_Tick(object sender, EventArgs e)
        {
            currentPath.Clear();      //xóa data path
            panelBoard.Invalidate();  // xóa đường nối
            lineTimer.Stop();         // dừng đồng hồ
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        //tat bat nut loa
        System.Media.SoundPlayer mayPhatNhac = new System.Media.SoundPlayer(Properties.Resources.ladade);
        System.Media.SoundPlayer Phase6OP = new System.Media.SoundPlayer(Properties.Resources.nhacnen3);
        bool isMuted = false;
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            isMuted = !isMuted;
            Phase6OP.Stop(); // Tắt hết
            mayPhatNhac.Stop();

            if (isMuted == true)
            {
                loa.Image = Properties.Resources.turnoff;
            }
            else
            {
                loa.Image = Properties.Resources.turnon;
                // Bật lại đúng bài nhạc đang chơi dở
                if (phase6op) Phase6OP.PlayLooping();
                else mayPhatNhac.PlayLooping();
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void panelBoard_Paint(object sender, PaintEventArgs e)
        {
            if (currentPath == null || currentPath.Count < 2) return;

            using (Pen pen = new Pen(Color.Red, 4))
            {
                pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;



                Point[] screenPoints = new Point[currentPath.Count];

                for (int i = 0; i < currentPath.Count; i++)
                {
                    int r = currentPath[i].X;
                    int c = currentPath[i].Y;

                    int centerX = offsetX + (c - 1) * (tileSize + margin) + (tileSize / 2);
                    int centerY = offsetY + (r - 1) * (tileSize + margin) + (tileSize / 2);

                    screenPoints[i] = new Point(centerX, centerY);
                }

                e.Graphics.DrawLines(pen, screenPoints);
            }
        }

        private void Pic_Click(object sender, EventArgs e)
        {
            PictureBox clickedPic = sender as PictureBox;
            Point clickedPos = (Point)clickedPic.Tag;

            if (gameModel.matrix[clickedPos.X, clickedPos.Y] == -1) return;
            //xu ly man boss
            if (isBossPhase)
            {
                firstClickPic = clickedPic;
                firstClickPos = clickedPos;
                firstClickPic.BorderStyle = BorderStyle.Fixed3D;

                int targetId = gameModel.matrix[firstClickPos.X, firstClickPos.Y];
                foreach (Control ctrl in panelBoard.Controls) //quet tim nua kia
                {
                    if (ctrl is PictureBox pic && pic != firstClickPic && pic.Tag != null && pic.Visible)
                    {
                        Point picPos = (Point)pic.Tag;
                        if (gameModel.matrix[picPos.X, picPos.Y] == targetId)
                        {
                            secondBossPic = pic;
                            secondBossPos = picPos;
                            secondBossPic.BorderStyle = BorderStyle.Fixed3D;
                            secondBossPic.BackColor = Color.Green;
                            break;
                        }
                    }
                }

                // quest  on
                panelQuizi.Visible = true;
                panelQuizi.BringToFront();
                if (blevel <= 4)
                {
                    currentBossQuestion = bM.EasyQuestions[blevel - 1];
                }
                else
                {
                    currentBossQuestion = bM.HardQuestions[blevel - 5];
                }

                lbQuest.Text = currentBossQuestion.QuestionText;
                lbAnswer.Text = "";
                timeLeft = 15;
                lbTime.Text = ": " + timeLeft;
                lbTime.Visible = true;
                countdownTimer.Start();
                panelBoard.Enabled = false;
                return;  //ngắt ko chạy code dưới

            }
            //xu ly chung
            if (firstClickPic == null)
            {
                firstClickPic = clickedPic;
                firstClickPos = clickedPos;
                clickedPic.BorderStyle = BorderStyle.Fixed3D;
                return;
            }

            if (firstClickPic != null && clickedPic != firstClickPic)
            {
                int id1 = gameModel.matrix[firstClickPos.X, firstClickPos.Y];
                int id2 = gameModel.matrix[clickedPos.X, clickedPos.Y];

                if (id1 == id2)
                {
                    List<Point> path = FindPath.FindingPath(gameModel.matrix, firstClickPos, clickedPos);

                    if (path.Count >= 2 && path.Count <= 4)
                    {
                        //update ma trận
                        gameModel.matrix[firstClickPos.X, firstClickPos.Y] = -1;
                        gameModel.matrix[clickedPos.X, clickedPos.Y] = -1;

                        //ẩn 2 ảnh đi
                        firstClickPic.Visible = false;
                        clickedPic.Visible = false;

                        score += 100; // ăn 1 cặp + 100 điểm
                        lbScore.Text = "Score: " + score;
                        demRemain--; // giảm số lượng cặp còn lại đi 1

                        if (currentStep == 2)
                        {
                            gameModel.shiftLeft();
                        }
                        else if (currentStep == 3)
                        {
                            gameModel.shiftUp();

                        }
                        else if (currentStep == 4)
                        {
                            gameModel.shiftLeft();
                            gameModel.shiftUp();
                        }
                        else if (currentStep == 5)
                        {
                            gameModel.shiftRight();
                            gameModel.shiftDown();
                        }

                        UpdateBoard();

                        // kiem tra dk win
                        if (demRemain == 0)
                        {
                            countdownTimer.Stop();
                            int bonusScore = timeLeft * 10;
                            score += bonusScore;
                            lbScore.Text = "Score: " + score;

                            sqlmodel.savehighscore(currentID, currentPlayer, score, currentStep, timeLeft);
                            sqlmodel.UpdateMaxLevel(currentID, currentStep + 1);

                            UpdateHighScoreDisplay();
                            panelBoard.Enabled = false;

                            // Bật ảnh Win
                            Win.Visible = true;
                            Win.BringToFront();

                            // ---- LOGIC TỰ ĐỘNG CHUYỂN MÀN SAU 2 GIÂY ----
                            if (!isLanMode && currentStep < 6) // Không áp dụng cho LAN và màn Boss 6
                            {
                                System.Windows.Forms.Timer autoNextTimer = new System.Windows.Forms.Timer();
                                autoNextTimer.Interval = 2000; // Đợi 2 giây
                                autoNextTimer.Tick += (s, args) =>
                                {
                                    autoNextTimer.Stop();
                                    autoNextTimer.Dispose();
                                    Win.Visible = false;

                                    // Tự động nhích lên 1 màn và tạo game mới
                                    currentStep++;

                                    if (txtPhase != null)
                                    {
                                        txtPhase.Text = "MÀN: " + currentStep;
                                        txtPhase.Visible = true;
                                    }
                                    if (currentStep == 6)
                                    {
                                        mayPhatNhac.Stop();

                                        if (!isMuted)
                                        {
                                            Phase6OP.PlayLooping();
                                        }
                                        else
                                        {
                                            loa.Image = Properties.Resources.turnoff;
                                            Phase6OP.PlayLooping();
                                        }
                                        phase6op = true;
                                        StartBossLevel(); // Khởi động màn Boss
                                    }
                                    else
                                    {
                                        StartGameLevel(300, true); // Khởi động màn thường
                                    }
                                };
                                autoNextTimer.Start();
                            }

                        }
                        //hiển thị đường ăn
                        currentPath = path;
                        panelBoard.Invalidate();
                        lineTimer.Start();
                        while (demRemain > 0 && !gameModel.HasValid())
                        {
                            if (remake > 0)
                            {
                                remake--; // Trừ đúng vào biến remake dùng chung
                                RefTime.Text = ": " + remake; // Cập nhật số 10 -> 9 -> 8 lên giao diện

                                MessageBox.Show($"Hết đường đi! Hệ thống tự động đảo mảng (Còn {remake} lượt cứu trợ).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                gameModel.RefMatrix();
                                UpdateBoard();
                            }
                            else
                            {
                                // Hết sạch cả 10 lượt cứu trợ -> Chết đứng
                                countdownTimer.Stop();
                                panelBoard.Enabled = false;
                                Lose.Visible = true;
                                MessageBox.Show("Hết đường đi và cũng hết lượt đảo! Game Over.", "Thua cuộc", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break; // Bí quá thì thoát vòng lặp
                            }
                        }
                    }
                }

                firstClickPic.BorderStyle = BorderStyle.FixedSingle;
                firstClickPic = null;
            }
        }
        private void panelChose_Paint(object sender, PaintEventArgs e)
        {
        }

        private void panelMenu_Paint(object sender, PaintEventArgs e)
        {

        }

        private void startGame_Click(object sender, EventArgs e)
        {

            panelMenu.Visible = false;
            panelBXH.Visible = false;

            panelLANJoin.Visible = true;
            panelLANJoin.BringToFront();


            backBut.Visible = true;
            backBut.BringToFront();
            currentStep = 999;
        }

        private void OFFLine_Click(object sender, EventArgs e)
        {
            panelMenu.Visible = false;
            panelLANJoin.Visible = false;
            panelLoad.Visible = true;
            panelBXH.Visible = false;
            panelLoad.BringToFront();
            currentStep = 120;
            panelOnl.Visible = false;
            if (ThongBaoOnl != null) ThongBaoOnl.Visible = false;
            if (txtPhase != null) txtPhase.Visible = false;
        }
        private void phase1_Click(object sender, EventArgs e)
        {
            StartGameLevel(420);
            txtPhase.Visible = true;
            txtPhase.Text = "MÀN: 1";
            currentStep = 1;
        }
        private void phase2_Click_1(object sender, EventArgs e)
        {
            int maxlevel = sqlmodel.GetMaxLevel(currentID);
            if (maxlevel < 2) return;
            txtPhase.Visible = true;
            txtPhase.Text = "MÀN: 2";
            StartGameLevel(420);
            currentStep = 2;
        }

        private void phase3_Click(object sender, EventArgs e)
        {
            int maxlevel = sqlmodel.GetMaxLevel(currentID);
            if (maxlevel < 3) return;
            txtPhase.Visible = true;
            txtPhase.Text = "MÀN: 3";
            StartGameLevel(400);
            currentStep = 3;
        }
        private void phase4_Click(object sender, EventArgs e)
        {
            int maxlevel = sqlmodel.GetMaxLevel(currentID);
            if (maxlevel < 4) return;
            txtPhase.Visible = true;
            txtPhase.Text = "MÀN: 4";
            StartGameLevel(380);
            currentStep = 4;
        }
        private void phase5_Click(object sender, EventArgs e)
        {
            int maxlevel = sqlmodel.GetMaxLevel(currentID);
            if (maxlevel < 5) return;
            txtPhase.Visible = true;
            txtPhase.Text = "MÀN: 5";
            StartGameLevel(360);
            currentStep = 5;
        }
        bool phase6op = false;
        private void phase6_Click(object sender, EventArgs e)
        {
            int maxlevel = sqlmodel.GetMaxLevel(currentID);
            txtPhase.Visible = true;
            txtPhase.Text = "MÀN: 6";
            if (maxlevel < 6) return;
            mayPhatNhac.Stop();
            if (!isMuted)
            {
                Phase6OP.PlayLooping();
            }
            else
            {
                loa.Image = Properties.Resources.turnon;
                Phase6OP.PlayLooping();
            }

                phase6op = true;
            currentStep = 6;
            panelChose.Visible = false;
            StartBossLevel();
        }
        //backbutton
        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

            label1.Visible = true;
            pass6.Visible = false;
            if (phase6op == true)
            {
                Phase6OP.Stop();
                phase6op = false;

                if (!isMuted) 
                { 
                    mayPhatNhac.PlayLooping();
                }
                else
                {
                    loa.Image = Properties.Resources.turnoff;
                }
            }
            vid1.Ctlcontrols.stop();
            vid1.Visible = false;
            if (currentStep >= 1 && currentStep <= 6)
            {
                if (Win.Visible == true || Lose.Visible == true)
                {
                    txtPhase.Visible = false;
                    Win.Visible = false;
                    Lose.Visible = false;
                    panelBoard.Visible = false;
                    pass6.Visible = false;
                    continuebut_Click(sender, e);
                    return;
                }
                //reset all boss
                if (currentStep == 6)
                {
                    txtPhase.Visible = false;
                    isBossPhase = false;
                    panelQuizi.Visible = false;
                    lose_meme.Visible = false;
                    vid1.Ctlcontrols.stop();
                    vid1.Visible = false;
                    vid2.Ctlcontrols.stop();
                    vid2.Visible = false;

                    // xóa sạch Pokemon trên bàn để không bị bóng ma
                    for (int i = panelBoard.Controls.Count - 1; i >= 0; i--)
                    {
                        if (panelBoard.Controls[i] is PictureBox && panelBoard.Controls[i].Tag != null)
                        {
                            panelBoard.Controls[i].Dispose();
                        }
                    }
                }
                // đang chơi dở -> bật bảng Pause
                countdownTimer.Stop();
                panelBoard.Enabled = false;
                panelPause.Visible = true;
                panelPause.BringToFront();
                if (ThongBaoOnl != null) ThongBaoOnl.Visible = false;
            }
            else if (currentStep == 0)
            {
                currentStep = -1; // Lùi về Menu
                panelChose.Visible = false;
                panelMenu.Visible = true;
                backBut.Visible = false;
                panelBXH.Visible = true;
                LoadLeaderboard();
            }
            else if (currentStep == 99)
            {
                currentStep = -1;
                panelGallery.Visible = false;
                panelMenu.Visible = true;
                backBut.Visible = false;
                panelBXH.Visible = true;
                LoadLeaderboard();
            }
            else if (currentStep == 98)
            {
                currentStep = 0; // Đưa mã về sảnh chờ
                pass6.Visible = false;
                panelBoard.Visible = false;

                panelChose.Visible = true; // Bật lại bảng chọn màn
                backBut.Visible = true;

                // Nhạc trở lại bình thường
                Phase6OP.Stop();
                phase6op = false;
                if (!isMuted) mayPhatNhac.PlayLooping();

                RefreshLevelImages(); // Làm mới ổ khóa các map
            }
            else if (currentStep == 999) // Đang ở bảng LAN/OFFLINE -> Bấm lùi về MENU
            {
                currentStep = -1;
                panelLANJoin.Visible = false;
                panelMenu.Visible = true;
                panelBXH.Visible = true;
                backBut.Visible = false;
            }
            else if (currentStep == 120)
            {
                panelLoad.Visible = false;
                backBut.Visible =true;
                panelLANJoin.Visible = true;
                currentStep = 999;
                panelBXH.Visible = false;
            }
            else if (currentStep == 100) // Đang ở bảng CREATE/JOIN -> Bấm lùi về LAN/OFFLINE
            {
                currentStep = 999; // Mã bảng LAN/OFFLINE
                panelJoinCreate.Visible = false;
                panelLANJoin.Visible = true;
            }
            else if (currentStep == 101) // Đang ở ô nhập IP -> Bấm lùi về CREATE/JOIN
            {
                currentStep = 100; // Mã bảng CREATE/JOIN
                IPimport.Visible = false;
                panelJoinCreate.Visible = true;
            }
            else if (currentStep == 102 || currentStep == 103)
            {
                currentStep = 100; // Lùi về bảng CREATE/JOIN

                // Dọn dẹp mạng
                if (lanServer != null) lanServer.Stop();
                if (lanClient != null) lanClient.Disconnect();
                isLanMode = false;
                isHost = false;

                // Tắt bàn cờ, dọn sạch Map, trả về giao diện cũ
                panelBoard.Visible = false;
                for (int i = panelBoard.Controls.Count - 1; i >= 0; i--)
                {
                    if (panelBoard.Controls[i] is PictureBox && panelBoard.Controls[i].Tag != null)
                        panelBoard.Controls[i].Dispose();
                }

                panelJoinCreate.Visible = true;
            }
        }

        private void pictureBox1_Click_2(object sender, EventArgs e)
        {

        }


        //hàm tạo màn
        private void StartGameLevel(int timeLimt, bool taoMapMoi = true)
        {

            panelBXH.Visible = false;
            isBossPhase = false;
            panelChose.Visible = false;
            panelMenu.Visible = false;
            label1.Visible = false;
            panelBoard.Visible = true;
            if (!isLanMode) panelOnl.Visible = false;
            backBut.Visible = true;
            Win.Visible = false;
            Lose.Visible = false;
            remake = 10;
            panelBoard.Enabled = true;
            clock.Visible = true;
            lbScore.Visible = true;
            lbTime.Visible = true;
            refresh.Visible = true;
            if (ThongBaoOnl != null) ThongBaoOnl.Visible = false;

            UpdateHighScoreDisplay();
            //khoi tao gia tri
            countdownTimer.Stop(); // Tắt đồng hồ cũ 
            score = 0;
            timeLeft = timeLimt;
            demRemain = ((Config.Rows - 2) * (Config.Cols - 2)) / 2; //100 o -> 50 cap
            lbTime.Text = ": " + timeLeft;
            lbScore.Text = "Score : " + score;

            RefTime.Text = ": " + remake;
            RefTime.Visible = true;
            countdownTimer.Start();
            //lam moi board
            this.Refresh();
            for (int i = panelBoard.Controls.Count - 1; i >= 0; i--)
            {
                Control ctrl = panelBoard.Controls[i];

                // Nếu nó là PictureBox VÀ có mang theo túi Tag (tức là Pokemon) thì mới đem đi hủy
                if (ctrl is PictureBox && ctrl.Tag != null)
                {
                    ctrl.Dispose();
                }
            }
            firstClickPic = null;
            currentPath.Clear();

            //sinh map va add pic
            if (taoMapMoi)
            {
                gameModel.GenerateMap(20);
            }



            for (int r = 1; r <= Config.Rows - 2; r++)
            {
                for (int c = 1; c <= Config.Cols - 2; c++)
                {
                    int pokeID = gameModel.matrix[r, c];
                    PictureBox pic = new PictureBox();
                    pic.Width = tileSize;
                    pic.Height = tileSize;
                    int x = offsetX + (c - 1) * (tileSize + margin);
                    int y = offsetY + (r - 1) * (tileSize + margin);
                    pic.Location = new Point(x, y);
                    if (gameModel.PokemonImagesCache.ContainsKey(pokeID)
                        && gameModel.PokemonImagesCache[pokeID] != null)
                    {
                        pic.Image = gameModel.PokemonImagesCache[pokeID];
                        pic.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                    else
                    {
                        pic.BackColor = Color.Black;
                    }
                    pic.BorderStyle = BorderStyle.FixedSingle;
                    pic.Tag = new Point(r, c);
                    pic.Click += Pic_Click;
                    panelBoard.Controls.Add(pic);
                }

            }
        }


        private void UpdateBoard()
        {
            foreach (Control ct in panelBoard.Controls)
            {
                if (ct is PictureBox pic && pic.Tag != null)
                {
                    Point pos = (Point)pic.Tag;
                    int id = gameModel.matrix[pos.X, pos.Y];

                    if (id != -1)
                    {
                        pic.Visible = true;
                        if (gameModel.PokemonImagesCache.ContainsKey(id) && gameModel.PokemonImagesCache[id] != null)
                        {
                            pic.Image = gameModel.PokemonImagesCache[id];
                        }
                    }
                    else
                    {
                        pic.Visible = false;
                    }
                }
            }
        }
        private void refresh_Click(object sender, EventArgs e)
        {
            if (remake > 0)
            {
                remake--;
                RefTime.Text = ": " + remake;

                //gọi hàm đảo ma trận
                gameModel.RefMatrix();

                // vẽ lại
                UpdateBoard();


                if (firstClickPic != null)
                {
                    firstClickPic.BorderStyle = BorderStyle.FixedSingle;
                    firstClickPic = null;
                }
            }
            else
            {
                MessageBox.Show("Hết lượt");
            }
        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }

        private async void submit_Click(object sender, EventArgs e)
        {
            string playerAns = lbAnswer.Text.Trim().ToLower();
            string trueAns = currentBossQuestion.Answer.ToLower();
            countdownTimer.Stop();
            lbTime.Visible = false;

            if (playerAns.Equals(trueAns) && timeLeft > 0)
            {
                int random = rd.Next(winImageList.Count);
                panelQuizi.Visible = false;
                pass6.Image = winImageList[random];
                pass6.SizeMode = PictureBoxSizeMode.Zoom;
                if (blevel >= 1 && blevel <= 5)
                {
                    pass6.Visible = true;
                    pass6.BringToFront();
                    await Task.Delay(2000);

                    pass6.Visible = false;
                }
                if (blevel == 6)
                {
                    //video1
                    vid1.Visible = true;
                    vid1.BringToFront();
                    vid1.URL = Application.StartupPath + "\\vid1.mp4";
                    vid1.Ctlcontrols.play();

                    await Task.Delay(14000);

                    vid1.Ctlcontrols.stop();
                    vid1.URL = "";
                    vid1.Visible = false;
                }
                if (blevel == 7)
                {
                    //video2
                    vid2.Visible = true;
                    vid2.BringToFront();
                    vid2.URL = Application.StartupPath + "\\end.mp4";
                    vid2.Ctlcontrols.play();

                    await Task.Delay(6000);

                    vid2.Ctlcontrols.stop();
                    vid2.URL = "";
                    vid2.Visible = false;
                }

                //ẩn 2 ảnh Pokemon đi 
                if (firstClickPic != null) firstClickPic.Visible = false;
                if (secondBossPic != null) secondBossPic.Visible = false;
                gameModel.matrix[firstClickPos.X, firstClickPos.Y] = -1;
                gameModel.matrix[secondBossPos.X, secondBossPos.Y] = -1;

                //reset biến và tăng Level
                firstClickPic = null;
                secondBossPic = null;
                blevel++; // Level up
                panelBoard.Enabled = true;

                //kiểm tra hoàn thành lv6
                if (blevel > 8)
                {
                    panelQuizi.Visible = false;
                    panelBoard.Enabled = false;

                    currentStep = 98;
                    pass6.Image = Properties.Resources.Goodjob;
                    pass6.SizeMode = PictureBoxSizeMode.Zoom;
                    pass6.Visible = true;
                    score += 2000;
                    pass6.BringToFront();

                    sqlmodel.savehighscore(currentID, currentPlayer, score, 6, timeLeft);
                    UpdateHighScoreDisplay();
                }
            }
            else
            {
                int randomlose = rd.Next(loseImageList.Count);
                panelQuizi.Visible = false;
                lose_meme.Image = loseImageList[randomlose];
                lose_meme.SizeMode = PictureBoxSizeMode.StretchImage;


                if (blevel == 8)
                {
                    lose_meme.Image = Properties.Resources.non;
                    lose_meme.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                else
                {
                    lose_meme.Image = loseImageList[randomlose];
                    lose_meme.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                panelQuizi.Visible = false;
                panelBoard.Enabled = true;

                lose_meme.Visible = true;
                lose_meme.BringToFront();

                await Task.Delay(2000);

                lose_meme.Visible = false;

                //return trạng thái cũ cho 2 con Pokemon 
                if (firstClickPic != null) firstClickPic.BorderStyle = BorderStyle.FixedSingle;
                if (secondBossPic != null)
                {
                    secondBossPic.BorderStyle = BorderStyle.FixedSingle;
                    secondBossPic.BackColor = Color.Transparent;
                }

                // dọn bộ nhớ chọn
                firstClickPic = null;
                secondBossPic = null;
            }
        }

        private void StartBossLevel()
        {
            panelBXH.Visible = false;
            isBossPhase = true;
            blevel = 1;
            bM.LoadQuestionBank();
            panelChose.Visible = false;
            panelMenu.Visible = false;
            label1.Visible = false;
            panelBoard.Visible = true;
            backBut.Visible = true;
            Win.Visible = false;
            Lose.Visible = false;
            panelBoard.Enabled = true;
            clock.Visible = true;
            refresh.Visible = false;    //ko cho lm moi
            lbScore.Visible = false;
            lbTime.Visible = true;
            RefTime.Visible = false;
            UpdateHighScoreDisplay();
            countdownTimer.Stop();
            lbTime.Visible = false;
            //don ban
            this.Refresh();
            for (int i = panelBoard.Controls.Count - 1; i >= 0; i--)
            {
                if (panelBoard.Controls[i] is PictureBox pic && pic.Tag != null)
                {
                    pic.Dispose();
                }
            }
            firstClickPic = null;
            secondBossPic = null;
            //sinh ma tran 4x4
            bM.GenerateBossMap(gameModel.matrix, Config.Rows, Config.Cols);



            for (int r = 1; r <= Config.Rows - 2; r++)
            {
                for (int c = 1; c <= Config.Cols - 2; c++)
                {
                    int pokeId = gameModel.matrix[r, c];
                    if (pokeId == -1) continue;

                    PictureBox pic = new PictureBox();
                    pic.Width = tileSize;
                    pic.Height = tileSize;
                    pic.Location = new Point(offsetX + (c - 1) * (tileSize + margin), offsetY + (r - 1) * (tileSize + margin));

                    if (gameModel.PokemonImagesCache.ContainsKey(pokeId))
                    {
                        pic.Image = gameModel.PokemonImagesCache[pokeId];
                        pic.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                    pic.BorderStyle = BorderStyle.FixedSingle;
                    pic.Tag = new Point(r, c);
                    pic.Click += Pic_Click;
                    panelBoard.Controls.Add(pic);
                }
            }
        }


        private void login_Click(object sender, EventArgs e)
        {
            string id = textID.Text.Trim(); // Ô nhập ID
            string user = textUser.Text.Trim(); // Ô nhập User

            if (sqlmodel.CheckLogin(id, user))
            {
                //đăng nhập đúng
                //lưu tạm info
                currentPlayer = user;
                currentID = id;

                panellogin.Visible = false;
                panelSignOut.Visible = false;
                panelMenu.Visible = true;
                currentStep = -1;
                // hiển thị kỷ lục
                UpdateHighScoreDisplay();
                panelBXH.Visible = true;
                LoadLeaderboard();
            }
            else
            {
                MessageBox.Show("Wrong info", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void UpdateHighScoreDisplay()
        {
            int topScore, topLevel;
            sqlmodel.gethighscore(out topScore, out topLevel);

            lbhighscore.Text = $"Best:{topScore}";
            lbhighscore.Visible = true;
            lbhighscore.BringToFront();
        }

        private void register_Click(object sender, EventArgs e)
        {
            panellogin.Visible = false;
            registerPanel.Visible = true;
            panelMenu.Visible = false;
            panelChose.Visible = false;
            panelQuizi.Visible = false;
            panelBoard.Visible = false;
        }

        private void ReturnLogin_Click(object sender, EventArgs e)
        {
            panellogin.Visible = true;
            registerPanel.Visible = false;
            panelMenu.Visible = false;
            panelChose.Visible = false;
            panelQuizi.Visible = false;
            panelBoard.Visible = false;
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            string newID = IdRegText.Text.Trim();
            string newUser = UserRegText.Text.Trim();
            if (string.IsNullOrEmpty(newID) || string.IsNullOrEmpty(newUser))
            {
                MessageBox.Show("Điền đủ info", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                if (sqlmodel.CheckUserExist(newID))
                {
                    MessageBox.Show("ID này đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                sqlmodel.RegisterUser(newID, newUser);
                MessageBox.Show("Tạo tài khoản thành công", "Return", MessageBoxButtons.OK, MessageBoxIcon.Information);
                IdRegText.Text = "";
                UserRegText.Text = "";
                registerPanel.Visible = false;
                panellogin.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi hệ thống");
            }

        }

        private void quit_Click(object sender, EventArgs e)
        {
            panelSignOut.Visible = true;
            panelMenu.Visible = false;
            panelBXH.Visible = false;
        }
        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void SignOut_Click(object sender, EventArgs e)
        {
            panelMenu.Visible = false;
            panellogin.Visible = true;
            panelSignOut.Visible = false;
            panelBXH.Visible = false;
        }

        private void gallery_Click(object sender, EventArgs e)
        {
            if (galleryList.Count == 0) return;
            currentStep = 99;
            panelBXH.Visible = false;
            panelMenu.Visible = false;
            panelGallery.Visible = true;
            panelGallery.BringToFront();
            backBut.Visible = true;

            currentGalleryIndex = 0;
            picGallery.Image = galleryList[currentGalleryIndex];
            picGallery.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void rightGallery_Click(object sender, EventArgs e)
        {
            if (galleryList.Count == 0) return;

            currentGalleryIndex++;
            if (currentGalleryIndex >= galleryList.Count)
            {
                currentGalleryIndex = 0; //return pic 1
            }
            picGallery.Image = galleryList[currentGalleryIndex];
        }

        private void leftGalley_Click(object sender, EventArgs e)
        {
            if (galleryList.Count == 0) return;
            currentGalleryIndex--;
            if (currentGalleryIndex < 0)
            {
                currentGalleryIndex = galleryList.Count - 1;
            }
            picGallery.Image = galleryList[currentGalleryIndex];
        }

        private void continuebutton_Click(object sender, EventArgs e)
        {
            panelPause.Visible = false;
            panelBoard.Visible = true;
            countdownTimer.Start();
        }

        private void homebutton_Click(object sender, EventArgs e)
        {
            panelPause.Visible = false;
            currentStep = 0;
            panelBoard.Visible = false;
            panelChose.Visible = true;
            isBossPhase = false;
            panelQuizi.Visible = false;
            panelBXH.Visible = false;
            firstClickPic = null;
            panelOnl.Visible = false;
            if (ThongBaoOnl != null) ThongBaoOnl.Visible = false;
            if (txtPhase != null) txtPhase.Visible = false;
            currentPath.Clear();
            for (int i = panelBoard.Controls.Count - 1; i >= 0; i--)
            {
                if (panelBoard.Controls[i] is PictureBox && panelBoard.Controls[i].Tag != null)
                {
                    panelBoard.Controls[i].Dispose();
                }
            }
            RefreshLevelImages();
        }

        private void savebutton_Click(object sender, EventArgs e)
        {
            if (isLanMode)
            {
                MessageBox.Show("Không thể lưu game khi đang chơi thi đấu Online!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Ngắt luôn, không cho chạy code SQL bên dưới
            }
            try
            {

                gameModel.RefMatrix();
                UpdateBoard();
                string matrixData = gameModel.matrixToString();
                sqlmodel.SaveGameData(currentID, score, timeLeft, currentStep, matrixData);
                MessageBox.Show("Đã lưu tiến trình game thành công!", "Save Game", MessageBoxButtons.OK, MessageBoxIcon.Information);
                homebutton_Click(sender, e);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi Lưu Game");
            }

        }

        private void newbutton_Click(object sender, EventArgs e)
        {
            //warning
            DialogResult dialog = MessageBox.Show("Are you sure delete ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dialog == DialogResult.Yes)
            {
                //gọi SQL reset cấp độ về 1 và xóa file save
                sqlmodel.ResetMaxLevel(currentID);
                sqlmodel.DeleteSaveGame(currentID);

                //đưa vào bảng chọn màn
                panelLoad.Visible = false;
                currentStep = 0;
                panelChose.Visible = true;
                backBut.Visible = true;

                //khóa toàn bộ các màn từ 2 đến 6
                phase2.BackgroundImageLayout = ImageLayout.Zoom;
                phase3.BackgroundImageLayout = ImageLayout.Zoom;
                phase4.BackgroundImageLayout = ImageLayout.Zoom;
                phase5.BackgroundImageLayout = ImageLayout.Zoom;
                phase6.BackgroundImageLayout = ImageLayout.Zoom;

                phase2.BackgroundImage = Properties.Resources.phase_2_lock;
                phase3.BackgroundImage = Properties.Resources.phase_3_lock;
                phase4.BackgroundImage = Properties.Resources.phase_4_lock;
                phase5.BackgroundImage = Properties.Resources.phase_5_lock;
                phase6.BackgroundImage = Properties.Resources.phase_6_lock;

                phase2.Image = null;
                phase3.Image = null;
                phase4.Image = null;
                phase5.Image = null;
                phase6.Image = null;
            }

        }

        private void loadbutton_Click(object sender, EventArgs e)
        {
            if (!sqlmodel.HasSaveGame(currentID))
            {
                MessageBox.Show("Bạn chưa có file lưu nào!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int saveScore, saveTime, saveStep;
            string matrixData;
            if (sqlmodel.LoadGameData(currentID, out saveScore, out saveTime, out saveStep, out matrixData))
            {
                panelBXH.Visible = false;
                panelLoad.Visible = false;
                //khôi phục chỉ số lưu
                currentStep = saveStep;
                score = saveScore;
                timeLeft = saveTime;
                gameModel.stringToMatrix(matrixData);
                demRemain = 0; //tính lại số cặp
                for (int r = 1; r <= Config.Rows - 2; r++)
                {
                    for (int c = 1; c <= Config.Cols - 2; c++)
                    {
                        if (gameModel.matrix[r, c] != -1) demRemain++;
                    }
                }
                demRemain = demRemain / 2;

                panelMenu.Visible = false;
                panelChose.Visible = false;
                panelBoard.Visible = true;
                backBut.Visible = true;
                clock.Visible = true;
                lbScore.Visible = true;
                lbTime.Visible = true;
                refresh.Visible = true;
                panelBoard.Enabled = true;
                lbScore.Text = "Score : " + score;
                lbTime.Text = ": " + timeLeft;
                UpdateHighScoreDisplay();
                // Vẽ lại pokemon lên bàn cờ dựa vào matrix
                DrawBoardFromMatrix();

                countdownTimer.Start();
            }
        }
        private void DrawBoardFromMatrix()
        {
            //Xóa rác cũ
            for (int i = panelBoard.Controls.Count - 1; i >= 0; i--)
            {
                if (panelBoard.Controls[i] is PictureBox && panelBoard.Controls[i].Tag != null)
                {
                    panelBoard.Controls[i].Dispose();
                }
            }




            for (int r = 1; r <= Config.Rows - 2; r++)
            {
                for (int c = 1; c <= Config.Cols - 2; c++)
                {
                    int pokeID = gameModel.matrix[r, c];
                    if (pokeID == -1) continue; // Ô trống thì không vẽ

                    PictureBox pic = new PictureBox();
                    pic.Width = tileSize;
                    pic.Height = tileSize;
                    pic.Location = new Point(offsetX + (c - 1) * (tileSize + margin), offsetY + (r - 1) * (tileSize + margin));

                    if (gameModel.PokemonImagesCache.ContainsKey(pokeID) && gameModel.PokemonImagesCache[pokeID] != null)
                    {
                        pic.Image = gameModel.PokemonImagesCache[pokeID];
                        pic.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                    pic.BorderStyle = BorderStyle.FixedSingle;
                    pic.Tag = new Point(r, c);
                    pic.Click += Pic_Click;
                    panelBoard.Controls.Add(pic);
                }
            }
        }
        private void RefreshLevelImages()
        {
            int maxLevel = sqlmodel.GetMaxLevel(currentID);

            // Gán layout
            phase2.BackgroundImageLayout = ImageLayout.Zoom;
            phase3.BackgroundImageLayout = ImageLayout.Zoom;
            phase4.BackgroundImageLayout = ImageLayout.Zoom;
            phase5.BackgroundImageLayout = ImageLayout.Zoom;
            phase6.BackgroundImageLayout = ImageLayout.Zoom;

            // Xóa ảnh đè
            phase2.Image = null; phase3.Image = null; phase4.Image = null; phase5.Image = null; phase6.Image = null;

            // Check để mở khóa
            phase2.BackgroundImage = (maxLevel >= 2) ? Properties.Resources.phase_2 : Properties.Resources.phase_2_lock;
            phase3.BackgroundImage = (maxLevel >= 3) ? Properties.Resources.phase_3 : Properties.Resources.phase_3_lock;
            phase4.BackgroundImage = (maxLevel >= 4) ? Properties.Resources.phase_4 : Properties.Resources.phase_4_lock;
            phase5.BackgroundImage = (maxLevel >= 5) ? Properties.Resources.phase_5 : Properties.Resources.phase_5_lock;
            phase6.BackgroundImage = (maxLevel >= 6) ? Properties.Resources.phase_6 : Properties.Resources.phase_6_lock;
        }
        private void continuebut_Click(object sender, EventArgs e)
        {
            panelLoad.Visible = false;
            panelMenu.Visible = false;
            currentStep = 0;
            panelChose.Visible = true;
            backBut.Visible = true;

            // Gọi hàm mở khóa ảnh
            RefreshLevelImages();
        }
        private void LoadLeaderboard()
        {
            try
            {
                System.Data.DataTable dt = sqlmodel.GetTop5Leaderboard();
                dataBXH.DataSource = dt;
                dataBXH.ClearSelection();
                //ép màu cho các dòng dữ liệu 
                dataBXH.DefaultCellStyle.BackColor = Color.Black;
                dataBXH.DefaultCellStyle.ForeColor = Color.White;
                dataBXH.DefaultCellStyle.SelectionBackColor = Color.Black;
                dataBXH.DefaultCellStyle.SelectionForeColor = Color.White;
                dataBXH.RowsDefaultCellStyle.BackColor = Color.Black;
                dataBXH.RowsDefaultCellStyle.ForeColor = Color.White;
                dataBXH.AlternatingRowsDefaultCellStyle.BackColor = Color.Black;
                dataBXH.AlternatingRowsDefaultCellStyle.ForeColor = Color.White;

                //ép màu thanh tiêu Đề 
                dataBXH.EnableHeadersVisualStyles = false;
                dataBXH.ColumnHeadersDefaultCellStyle.BackColor = Color.Black;
                dataBXH.ColumnHeadersDefaultCellStyle.ForeColor = Color.Yellow;
                dataBXH.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                //xóa rác hiển thị xung quanh
                dataBXH.RowHeadersVisible = false;
                dataBXH.AllowUserToAddRows = false;
                dataBXH.BackgroundColor = Color.Black;
                dataBXH.BorderStyle = BorderStyle.None;
                dataBXH.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

                if (dataBXH.Columns.Count > 0)
                {
                    if (dataBXH.Columns.Contains("Tên")) dataBXH.Columns["Tên"].Width = 80;
                    if (dataBXH.Columns.Contains("Điểm")) dataBXH.Columns["Điểm"].Width = 60;
                    if (dataBXH.Columns.Contains("Màn")) dataBXH.Columns["Màn"].Width = 40;
                    if (dataBXH.Columns.Contains("Cấp")) dataBXH.Columns["Cấp"].Width = 40;
                    if (dataBXH.Columns.Contains("Time")) dataBXH.Columns["Time"].Width = 50;

                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi Load Bảng xếp hạng: " + ex.Message);
            }
        }


        // ==========================================
        // CÁC BIẾN DÀNH CHO CHẾ ĐỘ ONLINE (LAN)
        // ==========================================
        private LanGameServer lanServer;
        private LanGameClient lanClient;
        private bool isHost = false;    // Xác định xem mình là Chủ phòng hay Khách
        private bool isLanMode = false; // Xác định game đang chạy offline hay online
        // ==========================================
        private void LanConnect_Click(object sender, EventArgs e)
        {
            panelLANJoin.Visible = false;

            panelJoinCreate.Visible = true;
            panelJoinCreate.BringToFront();

            backBut.Visible = true;
            backBut.BringToFront();
            currentStep = 100;
        }
        private void JoinRoom_Click(object sender, EventArgs e)
        {
            panelJoinCreate.Visible = false;

            // Hiện bảng nhập IP
            IPimport.Visible = true;
            IPimport.BringToFront();

            currentStep = 101; // Đánh dấu mã 101 là đang ở bảng nhập IP
        }
        private void CreateRoom_Click(object sender, EventArgs e)
        {
            isLanMode = true;
            isHost = true;

            // Bật Mạng
            lanServer = new LanGameServer(5000);
            lanServer.Start();
            string hostIP = lanServer.GetServerIP();

            lanClient = new LanGameClient(currentPlayer);
            lanClient.OnMessageReceived += HandleIncomingNetworkMessage;
            lanClient.Connect("127.0.0.1", 5000);

            panelJoinCreate.Visible = false;

            // +++++ CHUI VÀO LOBBY (PHÒNG CHỜ) +++++
            StartGameLevel(300, true);  // Host tự tạo sẵn Map
            panelBoard.Enabled = false; // Khóa bảng, cấm bấm
            countdownTimer.Stop();      // Tắt đồng hồ
            panelOnl.Visible = true;    // Hiện khung Stats của P2

            // Cập nhật chữ NOTE
            if (ThongBaoOnl != null)
            {
                MessageBox.Show("hello");
                ThongBaoOnl.Text = $"NOTE: Đợi đối thủ... (IP: {hostIP})";

                ThongBaoOnl.Visible = true;
            }

            currentStep = 102; // Mã 102: Host đang đợi
        }
        private void joinsubmit_Click(object sender, EventArgs e)
        {
            string ip = textIP.Text.Trim();
            if (string.IsNullOrEmpty(ip))
            {
                MessageBox.Show("Vui lòng nhập IP!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            isLanMode = true;
            isHost = false;

            lanClient = new LanGameClient(currentPlayer);
            lanClient.OnMessageReceived += HandleIncomingNetworkMessage;

            if (lanClient.Connect(ip, 5000))
            {
                IPimport.Visible = false;
                panelJoinCreate.Visible = false;

                // +++++ CLIENT CŨNG CHUI VÀO PHÒNG CHỜ +++++
                panelBoard.Visible = true;
                panelOnl.Visible = true;
                backBut.Visible = true;

                if (ThongBaoOnl != null)
                {
                    Console.WriteLine("goodbye");
                    ThongBaoOnl.Text = "NOTE: Đã nối! Chờ đồng bộ Map...";
                    ThongBaoOnl.Visible = true;
                }
                lanClient.SendMessage("PLAYER_JOINED");

                currentStep = 103; // Mã 103: Client đang đợi
            }
            else
            {
                MessageBox.Show("Không tìm thấy phòng! Kiểm tra lại IP.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                isLanMode = false;
            }
        }


        // HÀM 1: DÀNH CHO HOST (Chủ phòng)
        private void HandleIncomingNetworkMessage(string msg)
        {
            this.Invoke((MethodInvoker)delegate
            {
                string[] parts = msg.Split('|');
                string command = parts[0];

                switch (command)
                {
                    case "PLAYER_JOINED":
                        if (isHost)
                        {
                            // Mở khóa bàn cờ, bật đồng hồ
                            panelBoard.Enabled = true;
                            countdownTimer.Start();
                            currentStep = rd.Next(2, 6); // Random độ khó

                            if (ThongBaoOnl != null)
                                ThongBaoOnl.Text = $"NOTE: Bắt đầu! (Luật màn {currentStep})";

                            // Bắn Map sang cho Client

                            string mapData = gameModel.matrixToString();
                            Task.Run(async () =>
                            {
                                await Task.Delay(500);
                                if (lanServer != null)
                                {
                                    lanServer.BroadcastMessage($"SYNC_BOARD|{mapData}");
                                }
                            });
                        }
                        break;

                    case "SYNC_BOARD":
                        if (!isHost)
                        {
                            string matrixData = parts[1];
                            gameModel.stringToMatrix(matrixData);
                            StartGameLevel_LAN_Client(); // Dựng map
                        }
                        break;

                    case "SYNC_STATS":
                        if (parts.Length >= 5)
                        {
                            string pName = parts[1];
                            int pScore = int.Parse(parts[2]);
                            int pTime = int.Parse(parts[3]);
                            int pPairs = int.Parse(parts[4]);

                            if (pName != currentPlayer)
                            {
                                player2.Text = $"PLAYER 2: {pName} ({pPairs} cặp)";
                                scoreplayer2.Text = $"SCORE: {pScore}";
                                timeplayer2.Text = $"TIME: {pTime}s";
                            }
                        }
                        break;
                }
            });
        }

        // =========================================================
        // HÀM 2: DÀNH CHO CLIENT (Khách nhận Map từ Host)
        // =========================================================
        private void StartGameLevel_LAN_Client()
        {
            panelOnl.Visible = true;
            StartGameLevel(300, false); // False để xài Map của Host

            // Sửa lỗi 2: Client cũng phải bật đồng hồ và mở bảng
            countdownTimer.Start();
            panelBoard.Enabled = true;

            currentStep = rd.Next(2, 6);

            // Sửa lỗi 3: Bỏ điều kiện if Visible, set thẳng
            ThongBaoOnl.Text = $"NOTE: Bắt đầu! (Luật màn {currentStep})";
            ThongBaoOnl.Visible = true;
        }

    }
}
