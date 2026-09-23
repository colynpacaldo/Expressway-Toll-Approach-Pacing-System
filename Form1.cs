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

        // --- NEW: 2D animated road visualization ---
        private PictureBox pbRoad;
        private System.Windows.Forms.Timer animTimer;
        private double animPhase = 0;

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
            this.Size = new Size(1050, 880); // original frame size — everything below is laid out to fit inside it
            this.Font = new Font("Segoe UI", 10);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bgDark;

            // --- HEADER ---
            Label lblHeader = new Label { Text = "EXPRESSWAY PACING SYSTEM", Location = new Point(20, 12), AutoSize = true, Font = new Font("Segoe UI Black", 16, FontStyle.Bold), ForeColor = accentCyan };
            Label lblSubHeader = new Label { Text = "Mamdani Fuzzy Logic Controller", Location = new Point(23, 40), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = textMuted };

            // --- LEFT COLUMN: INPUT PANELS (compact) ---
            // Distance Panel
            Panel pnlDist = CreateCard(20, 68, 500, 95);
            Label lblDistTitle = new Label { Text = "DISTANCE TO TOLL", Location = new Point(15, 10), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = textMuted };
            lblDistanceValue = new Label { Text = "350 m", Location = new Point(330, 5), Size = new Size(150, 32), TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = accentCyan };
            tbDistance = new TrackBar { Minimum = 0, Maximum = 1000, Value = 350, Location = new Point(15, 42), Width = 470, Height = 40, TickFrequency = 50, TickStyle = TickStyle.BottomRight };
            tbDistance.Scroll += (s, e) => { lblDistanceValue.Text = tbDistance.Value + " m"; CalculateFuzzyLogic(); };
            pnlDist.Controls.AddRange(new Control[] { lblDistTitle, lblDistanceValue, tbDistance });

            // Traffic Panel
            Panel pnlTraf = CreateCard(20, 173, 500, 95);
            Label lblTrafTitle = new Label { Text = "TRAFFIC DENSITY", Location = new Point(15, 10), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = textMuted };
            lblTrafficValue = new Label { Text = "75 vpm", Location = new Point(330, 5), Size = new Size(150, 32), TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = accentCyan };
            tbTraffic = new TrackBar { Minimum = 0, Maximum = 100, Value = 75, Location = new Point(15, 42), Width = 470, Height = 40, TickFrequency = 10, TickStyle = TickStyle.BottomRight };
            tbTraffic.Scroll += (s, e) => { lblTrafficValue.Text = tbTraffic.Value + " vpm"; CalculateFuzzyLogic(); };
            pnlTraf.Controls.AddRange(new Control[] { lblTrafTitle, lblTrafficValue, tbTraffic });

            // --- LEFT COLUMN: OUTPUT PANEL (compact) ---
            Panel pnlOut = CreateCard(20, 278, 500, 105);
            Label lblOutTitle = new Label { Text = "TARGET APPROACH SPEED", Location = new Point(15, 10), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = textMuted };
            lblResult = new Label { Text = "0.0 km/h", Location = new Point(15, 32), AutoSize = true, Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = Color.White };
            lblStatus = new Label { Text = "STATUS: SAFE", Location = new Point(20, 80), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.LimeGreen };
            pnlOut.Controls.AddRange(new Control[] { lblOutTitle, lblResult, lblStatus });

            // --- LEFT COLUMN: LOG PANEL (compact) ---
            Panel pnlLog = CreateCard(20, 393, 500, 172);
            Label lblLogTitle = new Label { Text = "SYSTEM TRACE", Location = new Point(15, 8), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = textMuted };
            rtbLog = new RichTextBox { Location = new Point(15, 28), Width = 470, Height = 135, ReadOnly = true, Font = new Font("Consolas", 8), BackColor = bgDark, ForeColor = textLight, BorderStyle = BorderStyle.None };
            pnlLog.Controls.AddRange(new Control[] { lblLogTitle, rtbLog });

            // --- RIGHT COLUMN: DYNAMIC GRAPHS (compact, 3 stacked) ---
            int graphX = 540;
            Label lblDistGraph = new Label { Text = "Input: Distance Membership", Location = new Point(graphX, 68), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = textMuted };
            pbDistance = new PictureBox { Size = new Size(460, 130), Location = new Point(graphX, 88), BackColor = graphBg };
            pbDistance.Paint += DrawDistanceGraph;

            Label lblTrafGraph = new Label { Text = "Input: Traffic Membership", Location = new Point(graphX, 228), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = textMuted };
            pbTraffic = new PictureBox { Size = new Size(460, 130), Location = new Point(graphX, 248), BackColor = graphBg };
            pbTraffic.Paint += DrawTrafficGraph;

            Label lblOutGraph = new Label { Text = "Output: Target Speed Membership", Location = new Point(graphX, 388), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = textMuted };
            pbOutput = new PictureBox { Size = new Size(460, 130), Location = new Point(graphX, 408), BackColor = graphBg };
            pbOutput.Paint += DrawOutputGraph;

            // --- NEW: BOTTOM FULL-WIDTH 2D ANIMATED ROAD VISUALIZATION ---
            // Sits below both columns (which now end around y=565), still inside the original 880-tall frame.
            Label lblRoadTitle = new Label { Text = "LIVE APPROACH VISUALIZATION (2D)", Location = new Point(20, 578), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = textMuted };
            pbRoad = new PictureBox { Size = new Size(980, 230), Location = new Point(20, 598), BackColor = Color.FromArgb(15, 15, 22) };
            pbRoad.Paint += DrawRoadScene;

            // Add Master Controls (nothing removed, only repositioned/compacted + appended)
            this.Controls.AddRange(new Control[] {
                lblHeader, lblSubHeader, pnlDist, pnlTraf, pnlOut, pnlLog,
                lblDistGraph, pbDistance, lblTrafGraph, pbTraffic, lblOutGraph, pbOutput,
                lblRoadTitle, pbRoad
            });

            // Animation loop: redraws the road scene continuously (~30fps)
            animTimer = new System.Windows.Forms.Timer { Interval = 33 };
            animTimer.Tick += (s, e) => { animPhase += 1; pbRoad.Invalidate(); };
            animTimer.Start();
            this.FormClosing += (s, e) => { animTimer.Stop(); };

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
            log.AppendLine($"Traffic ({traf}vpm) -> Light: {tLight:F2} | Mod: {tMod:F2} | Heavy: {tHeavy:F2}\n");

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
            if (r1 > 0) log.AppendLine($"R1 (Near + Light) -> Slow = {r1:F2}");
            if (r2 > 0) log.AppendLine($"R2 (Near + Mod) -> Slow = {r2:F2}");
            if (r3 > 0) log.AppendLine($"R3 (Near + Heavy) -> Slow = {r3:F2}");
            if (r4 > 0) log.AppendLine($"R4 (Med + Light) -> Coast = {r4:F2}");
            if (r5 > 0) log.AppendLine($"R5 (Med + Mod) -> Coast = {r5:F2}");
            if (r6 > 0) log.AppendLine($"R6 (Med + Heavy) -> Slow = {r6:F2}");
            if (r7 > 0) log.AppendLine($"R7 (Far + Light) -> Cruise = {r7:F2}");
            if (r8 > 0) log.AppendLine($"R8 (Far + Mod) -> Coast = {r8:F2}");
            if (r9 > 0) log.AppendLine($"R9 (Far + Heavy) -> Slow = {r9:F2}");

            double outSlow = Math.Max(r1, Math.Max(r2, Math.Max(r3, Math.Max(r6, r9))));
            double outCoast = Math.Max(r4, Math.Max(r5, r8));
            double outCruise = r7;

            log.AppendLine($"\n=== Step C: Aggregation (MAX) ===");
            log.AppendLine($"Slow Cutoff : {outSlow:F2}");
            log.AppendLine($"Coast Cutoff : {outCoast:F2}");
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
            log.AppendLine($"Speed (COG) : {finalSpeed:F2} km/h");

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
            if (pbRoad != null) pbRoad.Invalidate(); // NEW: refresh road scene on input change
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
            Font f = new Font("Segoe UI", 8, FontStyle.Bold);

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
            Font f = new Font("Segoe UI", 8, FontStyle.Bold);

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
            Font f = new Font("Segoe UI", 8, FontStyle.Bold);

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

                RectangleF rect = new RectangleF(lineX + 5, 20, 75, 18);
                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 255, 255)), rect);
                g.DrawString($"{currentCrispSpeed:F1} km/h", f, Brushes.Black, lineX + 7, 21);
            }
        }

        // =====================================================================
        // NEW: 2D animated road visualization — your car, ambient traffic near
        // the toll booth, scrolling road, and a live speed HUD. Sized to sit
        // inside the same 1050x880 frame as everything above it.
        // =====================================================================

        // Maps a "distance to toll" value (1000m = far, 0m = at booth) onto x-pixels.
        private float MapDistanceToX(double distanceMeters, float leftX, float rightX)
        {
            double t = 1.0 - Math.Max(0, Math.Min(1000, distanceMeters)) / 1000.0;
            return (float)(leftX + t * (rightX - leftX));
        }

        private GraphicsPath RoundedRectPath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = radius * 2;
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // Draws a simple stylized car (body + cabin + wheels + optional lights).
        private void DrawCarShape(Graphics g, float x, float y, float scale, Color bodyColor, bool headlightsOn, bool brakeLightOn)
        {
            float w = 46 * scale, h = 20 * scale;
            RectangleF body = new RectangleF(x - w / 2f, y - h / 2f, w, h);
            using (GraphicsPath path = RoundedRectPath(body, 6 * scale))
            {
                g.FillPath(new SolidBrush(bodyColor), path);
                g.DrawPath(new Pen(Color.FromArgb(60, 255, 255, 255), 1), path);
            }

            RectangleF cabin = new RectangleF(x - w * 0.12f, y - h * 1.0f, w * 0.5f, h * 0.75f);
            using (GraphicsPath cabinPath = RoundedRectPath(cabin, 3 * scale))
                g.FillPath(new SolidBrush(Color.FromArgb(190, 15, 15, 25)), cabinPath);

            float wheelR = 5 * scale;
            g.FillEllipse(Brushes.Black, x - w * 0.32f - wheelR, y + h * 0.35f, wheelR * 2, wheelR * 2);
            g.FillEllipse(Brushes.Black, x + w * 0.18f - wheelR, y + h * 0.35f, wheelR * 2, wheelR * 2);

            if (brakeLightOn)
                g.FillEllipse(new SolidBrush(Color.FromArgb(230, 255, 40, 40)), x - w * 0.52f, y - 3 * scale, 6 * scale, 6 * scale);
            if (headlightsOn)
                g.FillEllipse(new SolidBrush(Color.FromArgb(230, 255, 240, 180)), x + w * 0.42f, y - 3 * scale, 6 * scale, 6 * scale);
        }

        private void DrawRoadScene(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = pbRoad.Width, h = pbRoad.Height;

            // Background gradient (dusk sky feel, consistent with dark theme)
            using (var skyBrush = new LinearGradientBrush(new Rectangle(0, 0, w, h), Color.FromArgb(30, 30, 48), Color.FromArgb(10, 10, 18), 90f))
                g.FillRectangle(skyBrush, 0, 0, w, h);

            float roadTop = h * 0.42f;
            float roadBottom = h * 0.84f;
            float leftX = 50, rightX = w - 140;
            float laneY = (roadTop + roadBottom) / 2f;

            // Road surface
            g.FillRectangle(new SolidBrush(Color.FromArgb(45, 45, 55)), leftX - 30, roadTop, (rightX - leftX) + 260, roadBottom - roadTop);
            g.DrawLine(new Pen(Color.FromArgb(90, 90, 105), 2), leftX - 30, roadTop, rightX + 230, roadTop);
            g.DrawLine(new Pen(Color.FromArgb(90, 90, 105), 2), leftX - 30, roadBottom, rightX + 230, roadBottom);

            // Scrolling lane dashes — scroll speed reflects the fuzzy-computed suggested speed
            float dashSpeed = (float)(currentCrispSpeed * 0.10) + 0.4f;
            float scrollOffset = (float)((animPhase * dashSpeed) % 40);
            Pen dashPen = new Pen(Color.FromArgb(160, 210, 210, 220), 3);
            for (float dx = leftX - 40 - scrollOffset; dx < rightX + 220; dx += 40)
                g.DrawLine(dashPen, dx, laneY, dx + 20, laneY);

            // Distance markers every 200m
            Font markerFont = new Font("Segoe UI", 7);
            using (var markerBrush = new SolidBrush(textMuted))
            {
                for (int m = 0; m <= 1000; m += 200)
                {
                    float mx = MapDistanceToX(m, leftX, rightX);
                    g.DrawLine(new Pen(Color.FromArgb(70, 70, 85), 1), mx, roadTop - 5, mx, roadTop);
                    g.DrawString(m + "m", markerFont, markerBrush, mx - 12, roadTop - 18);
                }
            }

            // Toll plaza gantry, fixed at the "0m" position
            float tollX = MapDistanceToX(0, leftX, rightX);
            g.DrawLine(new Pen(Color.FromArgb(200, 200, 210), 6), tollX + 15, roadTop - 38, tollX + 15, roadBottom + 12);
            g.FillRectangle(new SolidBrush(accentCyan), tollX - 15, roadTop - 38, 60, 11);
            g.DrawString("TOLL", new Font("Segoe UI", 8, FontStyle.Bold), Brushes.White, tollX - 12, roadTop - 36);

            // Ambient traffic cluster near the booth — count driven by Traffic Density input
            int trafficCount = (int)Math.Round(tbTraffic.Value / 100.0 * 8);
            for (int i = 0; i < trafficCount; i++)
            {
                float bob = (float)Math.Sin((animPhase / 12.0) + i * 1.3) * 2.2f;
                float cx = tollX - 55 - i * 26;
                float cy = laneY + (i % 2 == 0 ? -12 : 12) + bob;
                DrawCarShape(g, cx, cy, 0.55f, Color.FromArgb(150, 150, 160), false, tbTraffic.Value > 70);
            }

            // Ego (your) vehicle — position driven by Distance-to-Toll input
            float egoX = MapDistanceToX(tbDistance.Value, leftX, rightX);
            float egoBob = (float)Math.Sin(animPhase / 6.0) * 1.4f;
            Color egoColor = lblStatus.ForeColor;
            bool braking = currentCrispSpeed < 25;

            // Speed trail streaks behind the car (length scales with suggested speed)
            float trailLen = (float)(currentCrispSpeed * 1.2);
            for (int t = 0; t < 3; t++)
            {
                float ty = laneY + egoBob - 6 + t * 6;
                g.DrawLine(new Pen(Color.FromArgb(100, egoColor), 2), egoX - 28, ty, egoX - 28 - trailLen, ty);
            }

            DrawCarShape(g, egoX, laneY + egoBob, 0.9f, egoColor, true, braking);

            // Speed HUD above the ego car
            using (var hudBrush = new SolidBrush(Color.FromArgb(190, 20, 20, 30)))
                g.FillRectangle(hudBrush, egoX - 32, roadTop - 60, 95, 24);
            g.DrawString($"{currentCrispSpeed:F0} km/h", new Font("Segoe UI", 9, FontStyle.Bold), new SolidBrush(egoColor), egoX - 27, roadTop - 57);

            // Legend
            Font legendFont = new Font("Segoe UI", 7.5f);
            g.FillEllipse(new SolidBrush(accentCyan), 12, h - 18, 8, 8);
            g.DrawString("Your Vehicle", legendFont, new SolidBrush(textLight), 24, h - 20);
            g.FillEllipse(new SolidBrush(Color.FromArgb(150, 150, 160)), 150, h - 18, 8, 8);
            g.DrawString("Traffic Ahead (density-driven)", legendFont, new SolidBrush(textLight), 162, h - 20);
        }
    }
}