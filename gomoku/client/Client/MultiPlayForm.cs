using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Globalization;

namespace Client
{
    public partial class MultiPlayForm : Form
    {
        private Thread thread; // 쓰레드
        private TcpClient tcpClient;// TCP
        private NetworkStream stream;

        private const int rectSize = 33; 
        private enum Side{ none = 0, BLACK, WHITE };
        private Side[,] board;
        private Side currPlayer;
        private bool nowTurn;

        private bool playing;
        private bool entered;
        private bool threading;
        private int outofrange = 0;
        private DateTime n1;
        private bool connect = false;
        private bool didend = false;
        private void refresh()
        {
            this.boardPicture.Refresh();
            for (int i = 0; i < 11; i++)
                for (int j = 0; j < 11; j++)
                    board[i, j] = Side.none;
        }


        public MultiPlayForm()
        {
            InitializeComponent();
            playing = false;
            entered = false;
            threading = false;
            board = new Side[11, 11];
            nowTurn = false;
            n1 = DateTime.Now;
            this.readyButton.Enabled = false;
        }

        private void enterButton_Click(object sender, EventArgs e)
        {
            if (!connect)
            {
                tcpClient = new TcpClient();
                tcpClient.Connect("147.46.240.42", 20385);
                stream = tcpClient.GetStream();
                thread = new Thread(new ThreadStart(read));
                thread.Start();
                threading = true;
                connect = true;
                askButton.Enabled = false;
            }

            this.readyButton.Enabled = true;
            
            /* 방 접속 진행하기 */
            string message = "[Enter]";
            byte[] buf = Encoding.ASCII.GetBytes(message + this.roomTextBox.Text);
            stream.Write(buf, 0, buf.Length);
        }

        private void timer()
        {
            if (playing)
            {
                DateTime n1 = DateTime.Now;
                int diffint = 0;
                while (nowTurn && playing)
                {
                    TimeSpan diff = DateTime.Now - n1;
                    diffint = diff.Seconds;
                    status.Text = (59 - diffint).ToString();
                    if (diffint >= 59) break;
                }

                if (diffint >= 59)
                {
                    int play = 0;
                    if (currPlayer == Side.BLACK) play = 1;
                    string oor = "[Put]" + roomTextBox.Text + "," + "100" + "," + "100" + "," + play;
                    byte[] oorbuf = Encoding.ASCII.GetBytes(oor);
                    stream.Write(oorbuf, 0, oorbuf.Length);
                }
                else if(playing)
                {
                    status.Text = "상대방이 둘 차례입니다.";
                }
            }
        }

        private void read()
        {
            while(true)
            {
                byte[] buf = new byte[1024];
                int bufBytes = stream.Read(buf, 0, buf.Length);
                string message = Encoding.ASCII.GetString(buf, 0, bufBytes);
                /* 있는 방 묻기 */
                if (message.Contains("[Ask]"))
                {
                    string position = message.Split(']')[1];
                    position += "접속 가능";
                    status.Text = position;
                }
                /* 접속 성공  */
                if (message.Contains("[Enter]"))
                {
                    this.status.Text = "[" + this.roomTextBox.Text + "]번 방에 접속했습니다.";
                    this.roomTextBox.Enabled = false;
                    this.enterButton.Enabled = false;
                    entered = true;
                }
                /* 상대방이 나감 */
                if (message.Contains("[Exit]"))
                {
                    this.status.Text = "상대방이 나갔습니다.";
                    playing = false;
                    refresh();
                }
                /* 방 이미 가득 */
                if (message.Contains("[Full]"))
                {
                    this.status.Text = "이미 가득 찬 방입니다.";
                    closeNetwork();
                }
                /* 게임 시작 처리 */
                if (message.Contains("[Play]"))
                {
                    refresh();
                    string horse = message.Split(']')[1];
                    if (horse.Contains("Black"))
                    {
                        this.status.Text = "당신의 차례입니다.";
                        nowTurn = true;
                        currPlayer = Side.BLACK;
                        playing = true;
                        timer();
                    }
                    else
                    {
                        this.status.Text = "상대방의 차례입니다.";
                        nowTurn = false;
                        currPlayer = Side.WHITE;
                    }
                    playing = true;
                }
                /* 상태 변화 */
                if (message.Contains("[Put]"))
                {
                    string position = message.Split(']')[1];
                    int x = Convert.ToInt32(position.Split(',')[0]);
                    int y = Convert.ToInt32(position.Split(',')[1]);
                    int z = Convert.ToInt32(position.Split(',')[2]);
                    Side enemyPlayer;
                    if(currPlayer == Side.BLACK) enemyPlayer = Side.WHITE;                
                    else enemyPlayer = Side.BLACK;

                    if(x == -1 && y == -1)
                    {
                        if (!playing && !didend) status.Text = "상대방이 나갔습니다.";
                        else if( (z==0 && currPlayer == Side.BLACK) || (z == 1 && currPlayer == Side.WHITE))
                        {
                            nowTurn = false;
                            status.Text = "이김";
                        }
                        else
                        {
                            nowTurn = false;
                            status.Text = "짐";
                        }
                        enterButton.Enabled = false;
                        playing = false;
                        closeNetwork();
                    }
                    else if( x== -2 && y == -2)
                    {
                        nowTurn = false;
                        status.Text = "비김";
                        enterButton.Enabled = false;
                        playing = false;
                        closeNetwork();
                    }
                    if (board[x, y] != Side.none) continue;
                    board[x, y] = enemyPlayer;
                    Graphics g = this.boardPicture.CreateGraphics();
                    if (enemyPlayer == Side.BLACK)
                    {
                        SolidBrush brush = new SolidBrush(Color.Black);
                        g.FillEllipse(brush, x * rectSize, y * rectSize, rectSize, rectSize);
                    }
                    else
                    {
                        SolidBrush brush = new SolidBrush(Color.White);
                        g.FillEllipse(brush, x * rectSize, y * rectSize, rectSize, rectSize);
                    }
                    if (!playing && status.Text != "짐") status.Text = "상대방이 나갔습니다.";
                    if (judge(enemyPlayer))
                    {
                        playing = false;
                        didend = true;
                    }
                    else
                    {
                        status.Text = "당신이 둘 차례입니다.";
                    }
                    nowTurn = true;
                    if(playing &&  !didend) timer();
                }
            }
        }

        private void boardPicture_MouseDown(object sender, MouseEventArgs e)
        {
            if (!playing)
            {
                MessageBox.Show("게임을 실행 이전");
                return;
            }
            if (!nowTurn) return;
            Graphics g = this.boardPicture.CreateGraphics();
            int x = e.X / rectSize;
            int y = e.Y / rectSize;
            int play = 0;
            if (currPlayer == Side.WHITE) play = 1;

            /* 범위 벗어났는지 체크 */
            if (x < 0 || y < 0 || x >= 11 || y >= 11)
            {
                MessageBox.Show("범위를 벗어났습니다. 2회시 자동 패배");
                outofrange++;
                if(outofrange >= 2)
                {
                    nowTurn = false;
                    if (play == 0) play = 1;
                    else play = 0;
                    string oor = "[Put]" + roomTextBox.Text + "," + "100" + "," + "100" + "," + play;
                    byte[] oorbuf = Encoding.ASCII.GetBytes(oor);
                    stream.Write(oorbuf, 0, oorbuf.Length);
                }
                return;
            }
            if (board[x, y] != Side.none) return;
            board[x, y] = currPlayer;

            /* 돌 칠하기 */
            if (currPlayer == Side.BLACK)
            {
                SolidBrush brush = new SolidBrush(Color.Black);
                g.FillEllipse(brush, x * rectSize, y * rectSize, rectSize, rectSize);
            }
            else
            {
                SolidBrush brush = new SolidBrush(Color.White);
                g.FillEllipse(brush, x * rectSize, y * rectSize, rectSize, rectSize);
            }
            /* 놓은 돌의 위치 전송 */
            string message = "[Put]" + roomTextBox.Text + "," + x + "," + y+","+play;
            byte[] buf = Encoding.ASCII.GetBytes(message);
            stream.Write(buf, 0, buf.Length);

            int count = 0;
            for (int i = 0; i < 11; i++){
                for(int j=0; j<11; j++)
                {
                    if (board[i,j] != Side.none) count++;
                }
            }
            if (count >= 49) didend = true;

            /* 상대방의 차레 */
            status.Text = "상대방이 둘 차례입니다.";
            nowTurn = false;
        }

        private void boardPicture_Paint(object sender, PaintEventArgs e)
        {
            Graphics gp = e.Graphics;
            Color lineColor = Color.Black;
            Pen p = new Pen(lineColor, 2);
            int half = rectSize / 2;
            int end = rectSize * 11 - rectSize / 2;
            gp.DrawLine(p, half, half, end, half); // 상
            gp.DrawLine(p, half, end, end, end); // 하
            gp.DrawLine(p, half, half, half, end); // 좌
            gp.DrawLine(p, end, half, end, end); // 우
            p = new Pen(lineColor, 1);
            // 대각선 방향으로 이동하면서 십자가 모양의 선 그리기
            for (int i = rectSize + half; i < end; i += rectSize)
            {
                gp.DrawLine(p, half, i, end, i);
                gp.DrawLine(p, i, half, i, end);
            }
        }

        private void MultiPlayForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            closeNetwork();
        }

        void closeNetwork()
        {
            if (threading && thread.IsAlive) thread.Abort();
            if (entered) tcpClient.Close();
        }

        private void boardPicture_Click(object sender, EventArgs e)
        {

        }

        private void askButton_Click(object sender, EventArgs e)
        {
            if (!connect)
            {
                tcpClient = new TcpClient();
                tcpClient.Connect("147.46.240.42", 20385);
                stream = tcpClient.GetStream();

                thread = new Thread(new ThreadStart(read));
                thread.Start();
                threading = true;
                connect = true;
            }
            string message = "[Ask]";
            byte[] buf = Encoding.ASCII.GetBytes(message + this.roomTextBox.Text);
            stream.Write(buf, 0, buf.Length);
            askButton.Enabled = false;
        }

        private void MultiPlayForm_Load(object sender, EventArgs e)
        {

        }

        private void readyButton_Click(object sender, EventArgs e)
        {

            string message = "[Ready]";
            byte[] buf = Encoding.ASCII.GetBytes(message + this.roomTextBox.Text);
            stream.Write(buf, 0, buf.Length);
            readyButton.Enabled = false;
        }

        /* 승패 확인 */
        private bool judge(Side Player)
        {
            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 11; j++)
                    if (board[i, j] == Player && board[i + 1, j] == Player && board[i + 2, j] == Player && board[i + 3, j] == Player && board[i + 4, j] == Player)
                        return true;
            for (int i = 4; i < 11; i++)
                for (int j = 0; j < 7; j++)
                    if (board[i, j] == Player && board[i - 1, j + 1] == Player && board[i - 2, j + 2] == Player && board[i - 3, j + 3] == Player && board[i - 4, j + 4] == Player)
                        return true;
            for (int i = 0; i < 11; i++)
                for (int j = 4; j < 11; j++)
                    if (board[i, j] == Player && board[i, j - 1] == Player && board[i, j - 2] == Player && board[i, j - 3] == Player && board[i, j - 4] == Player)
                        return true;
            for (int i = 0; i < 7; i++)
                for (int j = 0; j < 7; j++)
                    if (board[i, j] == Player && board[i + 1, j + 1] == Player && board[i + 2, j + 2] == Player && board[i + 3, j + 3] == Player && board[i + 4, j + 4] == Player)
                        return true;
            return false;
        }
    }
}
