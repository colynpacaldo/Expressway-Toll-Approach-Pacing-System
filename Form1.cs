using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace FuzzyLogic
{
    public partial class Form1 : Form
    {
        private PictureBox pbDistance, pbTraffic, pbOutput;
        private double currentCrispSpeed = 0;

        private Label lblDistanceValue, lblTrafficValue, lblResult, lblStatus;
        private TrackBar tbDistance, tbTraffic;
        private RichTextBox rtbLog;

        // Modern UI Colors
        private Color bgDark = Color.FromArgb(24, 24, 36);
        private Color panelDark = Color.FromArgb(34, 34, 50);
        private Color textLight = Color.FromArgb(230, 230, 230);
        private Color textMuted = Color.FromArgb(150, 150, 170);
        private Color accentCyan = Color.FromArgb(0, 212, 255);
        private Color graphBg = Color.FromArgb(20, 20, 28);

        public Form1()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "CCLEX Toll Pacing Dashboard";
            this.Size = new Size(1050, 880);
            this.Font = new Font("Segoe UI", 10);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bgDark;

            // --- HEADER ---
            Label lblHeader = new Label { Text = "EXPRESSWAY PACING SYSTEM", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI Black", 18, FontStyle.Bold), ForeColor = accentCyan };
            Label lblSubHeader = new Label { Text = "Mamdani Fuzzy Logic Controller", Location = new Point(23, 50), AutoSize = true, Font = new Font("Segoe UI", 10), ForeColor = textMuted };

            // --- LEFT COLUMN: INPUT PANELS ---

            // Distance Panel
            Panel pnlDist = CreateCard(20, 90, 500, 130);
            Label lblDistTitle = new Label { Text = "DISTANCE TO TOLL", Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = textMuted };
            lblDistanceValue = new Label { Text = "350 m", Location = new Point(360, 10), Size = new Size(120, 40), TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = accentCyan };
            tbDistance = new TrackBar { Minimum = 0, Maximum = 1000, Value = 350, Location = new Point(15, 60), Width = 470, TickFrequency = 50, TickStyle = TickStyle.BottomRight };
            tbDistance.Scroll += (s, e) => { lblDistanceValue.Text = tbDistance.Value + " m"; CalculateFuzzyLogic(); };
            pnlDist.Controls.AddRange(new Control[] { lblDistTitle, lblDistanceValue, tbDistance });

            // Traffic Panel
            Panel pnlTraf = CreateCard(20, 235, 500, 130);
            Label lblTrafTitle = new Label { Text = "TRAFFIC DENSITY", Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = textMuted };
            lblTrafficValue = new Label { Text = "75 vpm", Location = new Point(360, 10), Size = new Size(120, 40), TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = accentCyan };
            tbTraffic = new TrackBar { Minimum = 0, Maximum = 100, Value = 75, Location = new Point(15, 60), Width = 470, TickFrequency = 10, TickStyle = TickStyle.BottomRight };
            tbTraffic.Scroll += (s, e) => { lblTrafficValue.Text = tbTraffic.Value + " vpm"; CalculateFuzzyLogic(); };
            pnlTraf.Controls.AddRange(new Control[] { lblTrafTitle, lblTrafficValue, tbTraffic });

            // --- LEFT COLUMN: OUTPUT PANEL ---
            Panel pnlOut = CreateCard(20, 380, 500, 140);
            Label lblOutTitle = new Label { Text = "TARGET APPROACH SPEED", Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = textMuted };
            lblResult = new Label { Text = "0.0 km/h", Location = new Point(15, 45), AutoSize = true, Font = new Font("Segoe UI", 36, FontStyle.Bold), ForeColor = Color.White };
            lblStatus = new Label { Text = "STATUS: SAFE", Location = new Point(20, 105), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.LimeGreen };
            pnlOut.Controls.AddRange(new Control[] { lblOutTitle, lblResult, lblStatus });

            // --- LEFT COLUMN: LOG PANEL ---
            Panel pnlLog = CreateCard(20, 535, 500, 285);
            Label lblLogTitle = new Label { Text = "SYSTEM TRACE", Location = new Point(15, 10), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = textMuted };
            rtbLog = new RichTextBox { Location = new Point(15, 35), Width = 470, Height = 235, ReadOnly = true, Font = new Font("Consolas", 9), BackColor = bgDark, ForeColor = textLight, BorderStyle = BorderStyle.None };
            pnlLog.Controls.AddRange(new Control[] { lblLogTitle, rtbLog });

            // --- RIGHT COLUMN: DYNAMIC GRAPHS ---
            int graphX = 540;

            Label lblDistGraph = new Label { Text = "Input: Distance Membership", Location = new Point(graphX, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = textMuted };
            pbDistance = new PictureBox { Size = new Size(460, 240), Location = new Point(graphX, 45), BackColor = graphBg };
            pbDistance.Paint += DrawDistanceGraph;

            Label lblTrafGraph = new Label { Text = "Input: Traffic Membership", Location = new Point(graphX, 305), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = textMuted };
            pbTraffic = new PictureBox { Size = new Size(460, 240), Location = new Point(graphX, 330), BackColor = graphBg };
            pbTraffic.Paint += DrawTrafficGraph;

            Label lblOutGraph = new Label { Text = "Output: Target Speed Membership", Location = new Point(graphX, 590), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = textMuted };
            pbOutput = new PictureBox { Size = new Size(460, 240), Location = new Point(graphX, 615), BackColor = graphBg };
            pbOutput.Paint += DrawOutputGraph;

            // Add Master Controls
            this.Controls.AddRange(new Control[] { lblHeader, lblSubHeader, pnlDist, pnlTraf, pnlOut, pnlLog, lblDistGraph, pbDistance, lblTrafGraph, pbTraffic, lblOutGraph, pbOutput });

            CalculateFuzzyLogic();
        }

        // Helper to create dark mode panels
        private Panel CreateCard(int x, int y, int w, int h)
        {
            return new Panel { Location = new Point(x, y), Size = new Size(w, h), BackColor = panelDark, BorderStyle = BorderStyle.None };
        }

        private double Trap(double x, double a, double b, double c, double d)
        {
            if (x < a || x > d) return 0.0;
            if (x >= b && x <= c) return 1.0;
            double leftSlope = (b > a) ? (x - a) / (b - a) : 1.0;
            double rightSlope = (d > c) ? (d - x) / (d - c) : 1.0;
            if (x < b) return leftSlope;
            if (x > c) return rightSlope;
            return 0.0;
        }

        private void CalculateFuzzyLogic()
        {
            double dist = tbDistance.Value;
            double traf = tbTraffic.Value;
            StringBuilder log = new StringBuilder();

            double dNear = Trap(dist, 0, 0, 200, 500);
            double dMed = Trap(dist, 200, 500, 500, 800);
            double dFar = Trap(dist, 500, 800, 1000, 1000);

            double tLight = Trap(traf, 0, 0, 30, 60);
            double tMod = Trap(traf, 30, 50, 50, 80);
            double tHeavy = Trap(traf, 50, 80, 100, 100);

            log.AppendLine($"=== Step A: Fuzzification ===");
            log.AppendLine($"Distance ({dist}m) -> Near: {dNear:F2} | Med: {dMed:F2} | Far: {dFar:F2}");
            log.AppendLine($"Traffic  ({traf}vpm) -> Light: {tLight:F2} | Mod: {tMod:F2} | Heavy: {tHeavy:F2}\n");

            double r1 = Math.Min(dNear, tLight);
            double r2 = Math.Min(dNear, tMod);
            double r3 = Math.Min(dNear, tHeavy);
            double r4 = Math.Min(dMed, tLight);
            double r5 = Math.Min(dMed, tMod);
            double r6 = Math.Min(dMed, tHeavy);
            double r7 = Math.Min(dFar, tLight);
            double r8 = Math.Min(dFar, tMod);
            double r9 = Math.Min(dFar, tHeavy);

            log.AppendLine($"=== Step B: Rule Implication (Active) ===");
            if (r1 > 0) log.AppendLine($"R1 (Near + Light) -> Slow   = {r1:F2}");
            if (r2 > 0) log.AppendLine($"R2 (Near + Mod)   -> Slow   = {r2:F2}");
            if (r3 > 0) log.AppendLine($"R3 (Near + Heavy) -> Slow   = {r3:F2}");
            if (r4 > 0) log.AppendLine($"R4 (Med + Light)  -> Coast  = {r4:F2}");
            if (r5 > 0) log.AppendLine($"R5 (Med + Mod)    -> Coast  = {r5:F2}");
            if (r6 > 0) log.AppendLine($"R6 (Med + Heavy)  -> Slow   = {r6:F2}");
            if (r7 > 0) log.AppendLine($"R7 (Far + Light)  -> Cruise = {r7:F2}");
            if (r8 > 0) log.AppendLine($"R8 (Far + Mod)    -> Coast  = {r8:F2}");
            if (r9 > 0) log.AppendLine($"R9 (Far + Heavy)  -> Slow   = {r9:F2}");

            double outSlow = Math.Max(r1, Math.Max(r2, Math.Max(r3, Math.Max(r6, r9))));
            double outCoast = Math.Max(r4, Math.Max(r5, r8));
            double outCruise = r7;

            log.AppendLine($"\n=== Step C: Aggregation (MAX) ===");
            log.AppendLine($"Slow Cutoff   : {outSlow:F2}");
            log.AppendLine($"Coast Cutoff  : {outCoast:F2}");
            log.AppendLine($"Cruise Cutoff : {outCruise:F2}\n");

            double numerator = 0;
            double denominator = 0;

            for (double v = 0; v <= 80; v += 1.0)
            {
                double mSlow = Trap(v, 0, 0, 0, 40);
                double mCoast = Trap(v, 20, 40, 40, 60);
                double mCruise = Trap(v, 40, 80, 80, 80);

                double cTotal = Math.Max(Math.Min(mSlow, outSlow), Math.Max(Math.Min(mCoast, outCoast), Math.Min(mCruise, outCruise)));
                numerator += v * cTotal;
                denominator += cTotal;
            }

            double finalSpeed = (denominator == 0) ? 0 : (numerator / denominator);
            currentCrispSpeed = finalSpeed;

            log.AppendLine($"=== Step D: Defuzzification (Centroid) ===");
            log.AppendLine($"Area (Denom) : {denominator:F2}");
            log.AppendLine($"Speed (COG)  : {finalSpeed:F2} km/h");

            // Update UI based on logic
            lblResult.Text = $"{finalSpeed:F1} km/h";

            if (finalSpeed < 25)
            {
                lblStatus.Text = "▶ STATUS: BRAKING / SLOW APPROACH";
                lblStatus.ForeColor = Color.Tomato;
                lblResult.ForeColor = Color.Tomato;
            }
            else if (finalSpeed < 55)
            {
                lblStatus.Text = "▶ STATUS: COASTING";
                lblStatus.ForeColor = Color.Gold;
                lblResult.ForeColor = Color.Gold;
            }
            else
            {
                lblStatus.Text = "▶ STATUS: CRUISING / CLEAR";
                lblStatus.ForeColor = Color.LimeGreen;
                lblResult.ForeColor = Color.LimeGreen;
            }

            rtbLog.Text = log.ToString();
            rtbLog.SelectionStart = 0;
            rtbLog.ScrollToCaret();

            pbDistance.Invalidate();
            pbTraffic.Invalidate();
            pbOutput.Invalidate();
        }

        // Helper for modern graph scaling
        private PointF ScalePt(double val, double maxVal, double truth, int w, int h)
        {
            float padding = 20;
            float graphW = w - (padding * 2);
            float graphH = h - (padding * 2);
            float px = padding + (float)(val / maxVal * graphW);
            float py = (h - padding) - (float)(truth * graphH);
            return new PointF(px, py);
        }

        private void DrawBaseGrid(Graphics g, int w, int h)
        {
            g.Clear(graphBg);
            Pen gridPen = new Pen(Color.FromArgb(40, 40, 50), 1) { DashStyle = DashStyle.Dash };
            for (int i = 1; i < 5; i++)
            {
                float y = 20 + (i * ((h - 40) / 5.0f));
                g.DrawLine(gridPen, 20, y, w - 20, y);
            }
            g.DrawLine(new Pen(Color.FromArgb(80, 80, 100), 2), 20, h - 20, w - 20, h - 20); // Base X-Axis
        }

        private void DrawDistanceGraph(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = pbDistance.Width; int h = pbDistance.Height;
            DrawBaseGrid(g, w, h);

            Pen pNear = new Pen(Color.FromArgb(255, 99, 132), 2);
            Pen pMed = new Pen(Color.FromArgb(54, 162, 235), 2);
            Pen pFar = new Pen(Color.FromArgb(75, 192, 192), 2);
            Font f = new Font("Segoe UI", 9, FontStyle.Bold);

            g.DrawLines(pNear, new[] { ScalePt(0, 1000, 1, w, h), ScalePt(200, 1000, 1, w, h), ScalePt(500, 1000, 0, w, h) });
            g.DrawString("Near", f, new SolidBrush(pNear.Color), ScalePt(10, 1000, 1.05, w, h));

            g.DrawLines(pMed, new[] { ScalePt(200, 1000, 0, w, h), ScalePt(500, 1000, 1, w, h), ScalePt(800, 1000, 0, w, h) });
            g.DrawString("Medium", f, new SolidBrush(pMed.Color), ScalePt(460, 1000, 1.05, w, h));

            g.DrawLines(pFar, new[] { ScalePt(500, 1000, 0, w, h), ScalePt(800, 1000, 1, w, h), ScalePt(1000, 1000, 1, w, h) });
            g.DrawString("Far", f, new SolidBrush(pFar.Color), ScalePt(910, 1000, 1.05, w, h));

            // Dynamic Tracker
            float lineX = ScalePt(tbDistance.Value, 1000, 0, w, h).X;
            g.DrawLine(new Pen(Color.White, 2) { DashStyle = DashStyle.Dot }, lineX, 20, lineX, h - 20);
            g.DrawString($"{tbDistance.Value}m", f, Brushes.White, lineX + 5, 20);
        }

        private void DrawTrafficGraph(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = pbTraffic.Width; int h = pbTraffic.Height;
            DrawBaseGrid(g, w, h);

            Pen pLight = new Pen(Color.FromArgb(75, 192, 192), 2);
            Pen pMod = new Pen(Color.FromArgb(255, 206, 86), 2);
            Pen pHeavy = new Pen(Color.FromArgb(255, 99, 132), 2);
            Font f = new Font("Segoe UI", 9, FontStyle.Bold);

            g.DrawLines(pLight, new[] { ScalePt(0, 100, 1, w, h), ScalePt(30, 100, 1, w, h), ScalePt(60, 100, 0, w, h) });
            g.DrawString("Light", f, new SolidBrush(pLight.Color), ScalePt(5, 100, 1.05, w, h));

            g.DrawLines(pMod, new[] { ScalePt(30, 100, 0, w, h), ScalePt(50, 100, 1, w, h), ScalePt(80, 100, 0, w, h) });
            g.DrawString("Mod", f, new SolidBrush(pMod.Color), ScalePt(46, 100, 1.05, w, h));

            g.DrawLines(pHeavy, new[] { ScalePt(50, 100, 0, w, h), ScalePt(80, 100, 1, w, h), ScalePt(100, 100, 1, w, h) });
            g.DrawString("Heavy", f, new SolidBrush(pHeavy.Color), ScalePt(88, 100, 1.05, w, h));

            float lineX = ScalePt(tbTraffic.Value, 100, 0, w, h).X;
            g.DrawLine(new Pen(Color.White, 2) { DashStyle = DashStyle.Dot }, lineX, 20, lineX, h - 20);
            g.DrawString($"{tbTraffic.Value}vpm", f, Brushes.White, lineX + 5, 20);
        }

        private void DrawOutputGraph(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = pbOutput.Width; int h = pbOutput.Height;
            DrawBaseGrid(g, w, h);

            Pen pSlow = new Pen(Color.FromArgb(255, 99, 132), 2);
            Pen pCoast = new Pen(Color.FromArgb(255, 206, 86), 2);
            Pen pCruise = new Pen(Color.FromArgb(75, 192, 192), 2);
            Font f = new Font("Segoe UI", 9, FontStyle.Bold);

            g.DrawLines(pSlow, new[] { ScalePt(0, 80, 1, w, h), ScalePt(20, 80, 1, w, h), ScalePt(40, 80, 0, w, h) });
            g.DrawString("Slow", f, new SolidBrush(pSlow.Color), ScalePt(5, 80, 1.05, w, h));

            g.DrawLines(pCoast, new[] { ScalePt(20, 80, 0, w, h), ScalePt(40, 80, 1, w, h), ScalePt(60, 80, 0, w, h) });
            g.DrawString("Coast", f, new SolidBrush(pCoast.Color), ScalePt(36, 80, 1.05, w, h));

            g.DrawLines(pCruise, new[] { ScalePt(40, 80, 0, w, h), ScalePt(60, 80, 1, w, h), ScalePt(80, 80, 1, w, h) });
            g.DrawString("Cruise", f, new SolidBrush(pCruise.Color), ScalePt(72, 80, 1.05, w, h));

            if (currentCrispSpeed > 0)
            {
                float lineX = ScalePt(currentCrispSpeed, 80, 0, w, h).X;
                g.DrawLine(new Pen(Color.White, 3), lineX, 20, lineX, h - 20);

                // Draw a colored background box for the output text to make it pop
                RectangleF rect = new RectangleF(lineX + 5, 20, 75, 20);
                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 255, 255)), rect);
                g.DrawString($"{currentCrispSpeed:F1} km/h", f, Brushes.Black, lineX + 7, 22);
            }
        }
    }
}